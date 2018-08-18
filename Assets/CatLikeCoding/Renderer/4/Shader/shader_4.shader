Shader "Unlit/First Lighting Shader"
{
	// 在开头添加属性
	Properties{
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Albedo", 2D) = "white" {}
		_SpecularTint ("Specular", Color) = (0.5, 0.5, 0.5)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
	}

	// 可以添加多个subshader用于不同平台处理，或者用于不同细节或不同水平处理
	SubShader{

		Tags{
			// 光照模式标签 
			"LightMode" = "ForwardBase"
		}

		// 表示一个物体进行渲染，可以有多个pass 进行多次渲染
		Pass{
			// 表示开始
			CGPROGRAM

			#pragma vertex MyVertexProgram      		// 顶点着色器
			#pragma fragment MyFragmentProgram			// 片段着色器

			//#include "UnityCG.cginc"					// 包含一些通用方法
			#include "UnityStandardBRDF.cginc"			// 这个文件中已经包含UnityCG.cginc

			// 使用属性需要名称和属性定义中相同
			float4 _Tint;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Smoothness; 							// 光滑度
			float4 _SpecularTint;						// 反射光着色
			
			struct VertexData{
				float4 position : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			// 定义一个结构体
			struct Interpolators{
				float4 position : SV_POSITION;  // 冒号后面代表语义
				float2 uv : TEXCOORD0;			// uv坐标
				float3 normal : TEXCOORD1;		// 法线
				float3 worldPos : TEXCOORD2;	// 物体世界坐标
			};


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

			float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
				i.normal = normalize(i.normal);
				// 获取光照在世界坐标的位置
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				// 摄像机坐标
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				// 获取光照颜色
				float3 lightColor = _LightColor0.rgb;
				// 返照率
				float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
				albedo *= 1 - _SpecularTint.rgb;
				// 漫反射
				float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal); // DotClamped 方法返回点积，并且保证不会为负
				
				//float3 reflectionDir = reflect(-lightDir, i.normal);
				// Blinn-Phong 模型
				float3 halfVector = normalize(lightDir + viewDir);
				// 镜面反射
				float3 specular = _SpecularTint.rgb * lightColor * pow(DotClamped(halfVector, i.normal), _Smoothness * 100);

				return float4(diffuse + specular, 1);  // pos(x, y) return x^y
			}

			// 表示结束
			ENDCG
		}

	}
}
