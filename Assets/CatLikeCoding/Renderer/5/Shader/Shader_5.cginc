#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED
//#include "UnityCG.cginc"					// 包含一些通用方法
//#include "UnityStandardBRDF.cginc"			// 这个文件中已经包含UnityCG.cginc
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"

// 使用属性需要名称和属性定义中相同
float4 _Tint;
sampler2D _MainTex;
float4 _MainTex_ST;
float _Smoothness; 							// 光滑度
float _Metallic;

struct VertexData{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD1;
	#endif
};

// 定义一个结构体
struct Interpolators{
	float4 position : SV_POSITION;  // 冒号后面代表语义
	float2 uv : TEXCOORD0;			// uv坐标
	float3 normal : TEXCOORD1;		// 法线
	float3 worldPos : TEXCOORD2;	// 物体世界坐标
};

void ComputeVertexLightColor(inout Interpolators i) {
	#if defined(VERTEXLIGHT_ON)

	#endif
}

UnityIndirect CreateIndirectLight(Interpolators i){
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse += i.vertexLightColor;
	#endif

	return indirectLight;
}

Interpolators MyVertexProgram(VertexData v){
	Interpolators i;
	i.position = UnityObjectToClipPos(v.position);
	i.uv = TRANSFORM_TEX(v.uv, _MainTex);
	// 将对象法线从对象空间转换到世界空间中
	i.normal = UnityObjectToWorldNormal(v.normal);
	i.worldPos = mul(unity_ObjectToWorld, v.position);
	// 计算完成后需要归一化
	i.normal = normalize(i.normal);
	return i;
}

UnityLight CreateLight(Interpolators i){
	UnityLight light;
	#if defined(POINT) || defined(SPOT) || defined(POINT_COOKIE)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif
	UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
	i.normal = normalize(i.normal);
	// 摄像机坐标
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

	float3 specularTint;
	float oneMinusReflectivity;
	albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
	
	// UnityIndirect indirectLight;
	// indirectLight.diffuse = 0;
	// indirectLight.specular = 0;
	
	//float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal); // DotClamped 方法返回点积，并且保证不会为负
	//float3 reflectionDir = reflect(-lightDir, i.normal);
	//float3 halfVector = normalize(lightDir + viewDir);
	//float3 specular = _SpecularTint.rgb * lightColor * pow(DotClamped(halfVector, i.normal), _Smoothness * 100);

	return UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i)
	);  // pos(x, y) return x^y
}

#endif