Shader "Unlit/First Lighting Shader New UnityBRDF"
{
    Properties {
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _MainTex("Albedo", 2D) = "white" {}
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
    }

    SubShader {
        Pass {
            Tags {
                "LightMode"="ForwardBase"
            }

            CGPROGRAM

            #include "UnityPBSLighting.cginc"
            
            float4 _Tint;
            float _Metallic;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Smoothness;
            
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            struct VertexData {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            Interpolators vert(VertexData v) {
                Interpolators i;
                i.vertex = UnityObjectToClipPos(v.vertex);
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                i.normal = UnityObjectToWorldNormal(v.normal);
                i.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return i;
            }

            float4 frag(Interpolators i) : SV_TARGET {
                i.normal = normalize(i.normal);
                // 场景中主光的方向，即使场景中有多个光源，这里也只考虑第一个光源
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                float3 lightColor = _LightColor0.rgb;
                float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

                float3 specularTint;
                float oneMinusReflectivity;
                albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);

                UnityLight light;
                light.color = lightColor;
                light.dir = lightDir;
                light.ndotl = DotClamped(i.normal, lightDir);

                UnityIndirect indirectLight;
                indirectLight.diffuse = 0;
                indirectLight.specular = 0;

                return UNITY_BRDF_PBS(albedo, specularTint,
                    oneMinusReflectivity, _Smoothness,
                    i.normal, viewDir,
                    light, indirectLight);
            }

            ENDCG
        }
    }
}