Shader "Unlit/First Lighting Shader_8"
{
	// 在开头添加属性
	Properties{
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]_NormalMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1	
		[Gamma]_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_DetailTex("Detail", 2D) = "gray" {}
		[NoScaleOffset]_DetailNormal("Detail Normal", 2D) = "bump" {}
	}

	// 可以添加多个subshader用于不同平台处理，或者用于不同细节或不同水平处理
	SubShader{

		// 表示一个物体进行渲染，可以有多个pass 进行多次渲染
		Pass{
			Tags{
				// 光照模式标签 
				"LightMode" = "ForwardBase"
			}

			// 表示开始
			CGPROGRAM

			#pragma target 3.0

			// 为了在 vertex函数中直接计算光照，需要进行以下定义，仅可用于点光光源
			// #pragma multi_compile _ VERTEXLIGHT_ON
			// 接收阴影
			#pragma multi_compile _ SHADOWS_SCREEN

			#pragma vertex MyVertexProgram      		// 顶点着色器
			#pragma fragment MyFragmentProgram			// 片段着色器

			#define FORWARD_BASE_PASS

			//#include "UnityCG.cginc"					// 包含一些通用方法
			//#include "UnityStandardBRDF.cginc"			// 这个文件中已经包含UnityCG.cginc
			#include "Shader_8.cginc"

			// 表示结束
			ENDCG
		}

		Pass{
			Tags{
				"LightMode" = "ForwardAdd"
			}

			Blend one one
			ZWrite Off

			CGPROGRAM

			
			#pragma target 3.0

			// 这行代码表示，这个pass即为directional光编译，也为point光编译， 在Shader_5.cginc的代码中，进行判断即可
			//#pragma multi_compile DIRECTIONAL POINT SPOT
			// 额外编译阴影
			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			//#define POINT
			#include "Shader_8.cginc"
			

			ENDCG
		}

		// 该pass用于阴影投射
		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#include "Shadow_8.cginc"

			ENDCG
		}
	}
}
