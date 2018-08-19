Shader "Custom/My_Test06" {
	Properties{
		_Color ("_Color", color) = (1, 1, 1, 1)
		_MainTex ("_MainTex", 2D) = "white" {}
		_NormalMap ("_NormalMap", 2D) = "white" {}
		_Diffuse ("_Diffuse", Range(0, 1)) = 1
		[KeywordEnum(Off, On)] _UseNormal("_UseNormal", float) = 1
		[KeywordEnum(Off, On)] _UseLight("_UseLight", float) = 1
	}

	SubShader{
		Tags{
			// TODO Background Geometry AlphaTest Transparent
			"Queue"="Background-100"
		}
		Pass{
			Tags{
				"LightMode"="ForwardBase"
			}
			// 如果有透明物体，则需要定义Blend
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma shader_feature _USENORMAL_OFF _USENORMAL_ON
				#pragma shader_feature _USELIGHT_OFF _USELIGHT_ON

				uniform float4 _Color;
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform sampler2D _NormalMap;
				uniform float4 _NormalMap_ST;
				uniform float _Diffuse;

				// 这些数据由系统读取，然后传入
				struct vertInput{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
					float4 tangent : TANGENT;
					float4 normal : NORMAL;
				};

				struct vertOutput{
					float4 pos : SV_POSITION;
					float3 texcoord : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float3 tangent : TEXCOORD2;
					float3 binormal : TEXCOORD3;
				};

				float3 Diffuse(float3 worldNormal, float3 lightColor, float3 lightDir, float diffuseFactor, float attenuation){
					return max(0, dot(worldNormal, lightDir)) * lightColor * diffuseFactor * attenuation;
				}

				float3 GetColor(float4 col){
					#if defined(UNITY_NO_DXT5mn)
						return col * 2 - 1;
					#else 
						float3 normal = float3(col.a * 2 - 1, col.g * 2 - 1, 0);
						normal.z = sqrt(1 - dot(normal, normal));
						return normal;
					#endif
				}

				vertOutput vert(vertInput v){
					vertOutput o;
					// 初始化
					UNITY_INITIALIZE_OUTPUT(vertOutput, o);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
					o.normal = UnityObjectToWorldNormal(v.normal);
					o.tangent = UnityObjectToWorldDir(v.tangent);
					o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
					//o.texcoord.xy = v.texcoord.xy;
					return o;
				}

				float4 frag(vertOutput o) : COLOR {
					float4 col = tex2D(_NormalMap, o.texcoord);

					float3 normalAtPix = GetColor(col);

					float3x3 tbn_World = float3x3(o.tangent, o.binormal, o.normal);

					float3 normal;
					#if defined(_USENORMAL_ON)
						normal = mul(normalAtPix, tbn_World);
					#else
						normal = o.normal;
					#endif

					#if defined(_USELIGHT_ON)
						float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
						float3 lightColor = _LightColor0.xyz;
						return float4(Diffuse(normal, lightColor, lightDir, _Diffuse, 1), 1);
					#else
						return float4(normal, 1);
					#endif	
				}

			ENDCG
		}
	}
}
