Shader "Unlit/First Lighting Shader New"
{
    Properties {
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _MainTex("Albedo", 2D) = "white" {}
        _SpecularTint ("Specular", Color) = (0.5, 0.5, 0.5, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
    }

    SubShader {
        Pass {
            Tags {
                "LightMode"="ForwardBase"
            }

            CGPROGRAM

            #include "UnityStandardBRDF.cginc"
            #include "UnityStandardUtils.cginc"

            float4 _Tint;
            float4 _SpecularTint;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Smoothness;
            
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
                // 为了让漫反射以及镜面反射光强度不超过1
                // albedo *= 1 - max(_SpecularTint.r, max(_SpecularTint.g, _SpecularTint.b));
                float oneMinusReflectivity;
                albedo = EnergyConservationBetweenDiffuseAndSpecular(albedo, _SpecularTint.rgb, oneMinusReflectivity);

                float3 diffuse = albedo * lightColor * DotClamped(i.normal, lightDir);

                // Blinn-Phong 使用halfVector替换reflectionDir，提高了计算效率
                // float3 reflectionDir = reflect(-lightDir, i.normal);
                float3 halfVector = normalize(lightDir + viewDir);
                float3 specular = lightColor * _SpecularTint.rgb * pow(DotClamped(halfVector, i.normal), _Smoothness * 100);
                return float4(diffuse + specular, 1);;
            }

            ENDCG
        }
    }
}