// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderDev/0013Lighting_Diffuse"{
	Properties{
	//  变量名   lable      data type R  G  B  A
		_Color("Main Color", color) = (1, 1, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		_NormalMap("NromalMap", 2D) = "white" {}
		[KeywordEnum(Off, On)] _UseNormal("Use Normal Map?", float) = 0
		_Diffuse ("_Diffuse", Range(0, 1)) = 1
		[KeywordEnum(Off, Vert, Frag)] _Lighting ("Lighting Mode", float) = 0
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

			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _USENORMAL_OFF _USENORMAL_ON
			#pragma shader_feature _LIGHTING_OFF _LIGHTING_VERT _LIGHTING_FRAG
			#include "CVGLighting.cginc"

			uniform half4 _Color;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			uniform sampler2D _NormalMap;
			uniform float4 _NormalMap_ST;

			uniform float _Diffuse;
			uniform float4 _LightColor0;


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
				#if _USENORMAL_ON
					float4 tangentWorld : TEXCOORD2;
					float3 binormalWorld : TEXCOORD3;
					float4 normalTexcoord: TEXCOORD4;
				#endif
				#if _LIGHTING_VERT
					float4 surfaceColor : COLOR0;
				#endif
			};
			
			float3 DiffuseLambert(float3 normalVal, float3 lightDir,float3 lightColor, float diffuseFactor, float attenuation){
				return lightColor * diffuseFactor * attenuation * max(0, dot(normalVal, lightDir));
			}

			VertexOutput vert(VertexInput v){
				VertexOutput o;
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				o.pos = UnityObjectToClipPos( v.vertex);
				o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				
				// 法线世界坐标
				o.normalWorld = normalize(mul(v.normal, unity_WorldToObject));
				#if _USENORMAL_ON
					o.normalTexcoord.xy = (v.texcoord.xy * _NormalMap_ST.xy + _NormalMap_ST.zw);
					// 切线世界坐标
					o.tangentWorld = normalize(mul(v.tangent, unity_ObjectToWorld));
					o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w);
				#endif
				#if _LIGHTING_VERT
					float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
					float3 lightColor = _LightColor0.xyz;
					float attenuation = 1;

					o.surfaceColor = float4(DiffuseLambert(o.normalWorld, lightDir, lightColor, _Diffuse, attenuation, 1), 1);
				#endif
				return o;
			}



			half4 frag(VertexOutput i) : COLOR{
				#if _USENORMAL_ON
					float3 worldNormalAtPixel = WorldNormalFromNormalMap(_NormalMap, i.normalTexCoord.xy, i.tangentWorld.xyz, i.binormalWorld.xyz, i.normalWorld.xyz);

					return float4(worldNormalAtPixel, 1);
					//return tex2D(_MainTex, i.texcoord) * _Color;
				#else

					return float4(i.normalWorld.xyz, 1);
				#endif
			}


			ENDCG
		}
	}
}