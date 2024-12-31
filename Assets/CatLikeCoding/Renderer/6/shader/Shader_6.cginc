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
sampler2D _NormalMap;
float _BumpScale;
sampler2D _DetailTex;
float4 _DetailTex_ST;
sampler2D _DetailMap;

struct VertexData{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float4 uv : TEXCOORD0;
};

// 定义一个结构体
struct Interpolators{
	float4 position : SV_POSITION;  // 冒号后面代表语义
	float4 uv : TEXCOORD0;			// uv坐标
	float3 normal : TEXCOORD1;		// 法线
	float3 worldPos : TEXCOORD2;	// 物体世界坐标
	float3 tangent : TEXCOORD3;		// 切线
	float3 binormal : TEXCOORD4;	// 副切线

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD3;
	#endif
};

float3 normalFromColor(float4 colorVal){
	#if defined(UNITY_NO_DXT5mn)
		return colorVal * 2 - 1;
	#else 
		// R => x => A
		// G => y
		// B =? z => ignored
		float3 normalVal;
		normalVal = float3(colorVal.a * 2.0 - 1,
							colorVal.g * 2.0 - 1.0,
							0.0 );
		normalVal.z = sqrt(1.0 - dot(normalVal, normalVal));
		return normalVal;
	#endif
}

// 单独计算光照函数     需要输入和输出，因此用 inout
void ComputeVertexLightColor(inout Interpolators i){
	#if defined(VERTEXLIGHT_ON)
		i.vertexLightColor = unity_LightColor[0].rgb;
	#endif
}

Interpolators MyVertexProgram(VertexData v){
	Interpolators i;
	i.position = UnityObjectToClipPos(v.position);
	i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);
	i.worldPos = mul(unity_ObjectToWorld, v.position);

	// 将对象法线从对象空间转换到世界空间中
	i.normal = UnityObjectToWorldNormal(v.normal);
	// 计算完成后需要归一化
	i.normal = normalize(i.normal);
	i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
	i.binormal = cross(i.normal, i.tangent) * v.tangent.w;
	// i.normal = mul((float3x3)unity_WorldToObject, v.normal);
	// i.tangent = mul((float3x3)unity_WorldToObject, v.tangent.xyz);

	ComputeVertexLightColor(i);
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

UnityIndirect CreateIndirectLight(Interpolators i){
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse = i.vertexLightColor;
	#endif 
	return indirectLight;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
	// // 注意tex2D仅可以在像素着色器中使用
	// // https://learn.microsoft.com/zh-cn/windows/win32/direct3dhlsl/dx-graphics-hlsl-tex2d
	// float3 normalAtPixed = normalFromColor(tex2D(_NormalMap, i.uv));
	float3 mainNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);
	float3 detailNormal = UnpackScaleNormal(tex2D(_DetailMap, i.uv.zw), _BumpScale);
	float3 blendNormal = BlendNormals(mainNormal, detailNormal);
	// blendNormal = blendNormal.xzy;

	float3x3 tangentToWorld = float3x3(i.tangent, i.binormal, i.normal);
	// 注意矩阵乘法的顺序
	i.normal = normalize(mul(blendNormal, tangentToWorld));
	// 上述代码等价于
	// i.normal = normalize(float3(
	// 	blendNormal.x * i.tangent +
	// 	blendNormal.y * i.binormal + 
	// 	blendNormal.z * i.normal
	// ));

	// 摄像机坐标
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Tint.rgb;
	albedo *= tex2D(_DetailTex, i.uv.zw) * unity_ColorSpaceDouble;

	float3 specularTint;
	float oneMinusReflectivity;
	albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
	
	//UnityIndirect indirectLight;
	//indirectLight.diffuse = 0;
	//indirectLight.specular = 0;
	
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