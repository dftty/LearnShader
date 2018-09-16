// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderDev/0014Lighting_Specular"{
	Properties{
	//  变量名   lable      data type R  G  B  A
		_Color("Main Color", color) = (1, 1, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		_NormalMap("NromalMap", 2D) = "white" {}
		[KeywordEnum(Off, On)] _UseNormal("Use Normal Map?", float) = 0
		_Diffuse ("_Diffuse", Range(0, 1)) = 1
		[KeywordEnum(Off, Vert, Frag)] _Lighting ("Lighting Mode", float) = 0
		_SpecularMap ("Specular Map", 2D) = "black" {}
		_SpecularFactor("SpecularFactor", Range(0, 1)) = 1
		_SpecularPower("SpecularPower", float) = 100
	}

	SubShader{

		// Tag 既可以放在SubShader 中，也可以放在Pass中，在Pass中表示仅在该pass中有效
		Tags{
			// 减号之间不能有空格
			"Queue" = "Geometry-500"
			"IgnoreProjector" = "False"
		}
		Pass{
			Tags{
				"LightMode" = "ForwardBase"
			}
			// 该Pass 的名称
			Name "First"

			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _USENORMAL_OFF _USENORMAL_ON
			#pragma shader_feature _LIGHTING_OFF _LIGHTING_VERT _LIGHTING_FRAG
			#include "CVGLighting.cginc"
			#include "UnityCG.cginc"

			uniform half4 _Color;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			uniform sampler2D _NormalMap;
			uniform float4 _NormalMap_ST;

			uniform float _Diffuse;
			uniform float4 _LightColor0;

			uniform sampler2D _SpecularMap;
			uniform float _SpecularFactor;
			uniform float _SpecularPower;


			struct VertexInput{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				#if _USENORMAL_ON
					float4 tangent : TANGENT;
				#endif
			};

			struct VertexOutput{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				float4 normalWorld : TEXCOORD1;
				float3 worldSpaceViewDir : TEXCOORD2;
				#if _USENORMAL_ON
					float4 tangentWorld : TEXCOORD3;
					float3 binormalWorld : TEXCOORD4;
					float4 normalTexcoord: TEXCOORD5;
				#endif
				#if _LIGHTING_VERT
					float4 surfaceColor : COLOR0;
				#endif
			};
		

			VertexOutput vert(VertexInput v){
				VertexOutput o;
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				
				// 计算物体到摄像机的向量
				float3 worldSpaceViewDir = normalize(WorldSpaceViewDir(v.vertex));
				o.worldSpaceViewDir = worldSpaceViewDir;
				// 法线世界坐标
				o.normalWorld = float4(normalize(mul(normalize(v.normal.xyz), (float3x3)unity_WorldToObject)), v.normal.w);
				#if _USENORMAL_ON
					o.normalTexcoord.xy = (v.texcoord.xy * _NormalMap_ST.xy + _NormalMap_ST.zw);
					// 切线世界坐标
					o.tangentWorld = float4(normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz)), v.tangent.w);
					o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w);
				#endif
				#if _LIGHTING_VERT
					float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
					float3 lightColor = _LightColor0.xyz;
					float attenuation = 1;
					float4 specularColor = tex2Dlod(_SpecularMap, float4(v.texcoord.xy, 0, 0));
					float3 specularCol = SpecularBlinnPhong(o.normalWorld, lightDir, worldSpaceViewDir, specularColor.rgb, _SpecularFactor, 1, _SpecularPower);

					o.surfaceColor = float4(DiffuseLambert(o.normalWorld, lightDir, lightColor, _Diffuse, 1) + specularCol, 1);
				#endif
				return o;
			}



			half4 frag(VertexOutput i) : COLOR{
				#if _USENORMAL_ON
					float3 worldNormalAtPixel = WorldNormalFromNormalMap(_NormalMap, i.normalTexcoord.xy, i.tangentWorld.xyz, i.binormalWorld.xyz, i.normalWorld.xyz);
					//return tex2D(_MainTex, i.texcoord) * _Color;
				#else

					float3 worldNormalAtPixel = i.normalWorld.xyz;
				#endif

				#if _LIGHTING_FRAG
					float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
					float3 lightColor = _LightColor0.xyz;
					float attenuation = 1;

					float4 diffuseCol = float4(DiffuseLambert(worldNormalAtPixel, lightDir, lightColor, _Diffuse, 1), 1);
					float4 specularColor = tex2D(_SpecularMap, float4(i.texcoord.xy, 0, 0));


					float3 specularCol = SpecularBlinnPhong(worldNormalAtPixel, lightDir, i.worldSpaceViewDir, specularColor.rgb, _SpecularFactor, 1, _SpecularPower);

					return float4(diffuseCol + specularCol, 1);
				#elif _LIGHTING_VERT
					return i.surfaceColor;
				#else
					return float4(worldNormalAtPixel, 1);
				#endif
			}


			ENDCG
		}
	}
}