// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Shader_2"
{

	// 在开头添加属性
	Properties{
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Texture", 2D) = "white" {}
	}

	// 可以添加多个subshader用于不同平台处理，或者用于不同细节或不同水平处理
	SubShader{

		// 表示一个物体进行渲染，可以有多个pass 进行多次渲染
		Pass{
			// 表示开始
			CGPROGRAM

			#pragma vertex MyVertexProgram      		// 顶点着色器
			#pragma fragment MyFragmentProgram			// 片段着色器

			#include "UnityCG.cginc"					// 包含一些通用方法

			// 使用属性需要名称和属性定义中相同
			float4 _Tint;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			// 定义一个结构体
			struct Interpolators{
				float4 position : SV_POSITION;  // 冒号后面代表语义
				float2 uv : TEXCOORD0;
			};

			struct VertexData{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			Interpolators MyVertexProgram(VertexData v){
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return i;
			}

			float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
				return tex2D(_MainTex, i.uv) * _Tint;
			}

			// 表示结束
			ENDCG
		}

	}
}
