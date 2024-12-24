// 类似c++的include，需要防止重复包含
#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"

float4 _Tint;
float _Metallic;
sampler2D _MainTex;
float4 _MainTex_ST;
float _Smoothness;

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

UnityLight CreateLight(Interpolators i) {
    UnityLight light;
    // #if defined(POINT)
        light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
    // #else
    //     light.dir = _WorldSpaceLightPos0.xyz;
    // #endif
    UNITY_LIGHT_ATTENUATION(atten, 0, i.worldPos);
    light.color = _LightColor0.rgb * atten;
    light.ndotl = DotClamped(i.normal, light.dir);
    return light;
}

float4 frag(Interpolators i) : SV_TARGET {
    i.normal = normalize(i.normal);
    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

    float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

    float3 specularTint;
    float oneMinusReflectivity;
    albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);

    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;

    return UNITY_BRDF_PBS(albedo, specularTint,
        oneMinusReflectivity, _Smoothness,
        i.normal, viewDir,
        CreateLight(i), indirectLight);
}

#endif