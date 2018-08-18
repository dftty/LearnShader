Shader "Unlit/First Lighting Shader_5_1"
{
	// 在开头添加属性
	Properties{
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Albedo", 2D) = "white" {}
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
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
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex MyVertexProgram      		// 顶点着色器
			#pragma fragment MyFragmentProgram			// 片段着色器

			//#include "UnityCG.cginc"					// 包含一些通用方法
			//#include "UnityStandardBRDF.cginc"			// 这个文件中已经包含UnityCG.cginc
			#include "Shader_5_1.cginc"

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
			#pragma multi_compile_fwdadd

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			//#define POINT
			#include "Shader_5.cginc"
			

			ENDCG
		}

	}
}
