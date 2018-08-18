Shader "Unlit/Texture With Detail"
{
	Properties{
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Texture", 2D) = "white" {}
		_DetailTex ("Detail Texture", 2D) = "grey" {}
	}

	SubShader{
		Pass{
			// 表示开始
			CGPROGRAM

			#pragma vertex MyVertexProgram      		// 顶点着色器
			#pragma fragment MyFragmentProgram			// 片段着色器

			#include "UnityCG.cginc"					// 包含一些通用方法

			// 使用属性需要名称和属性定义中相同
			float4 _Tint;
			sampler2D _MainTex, _DetailTex;
			float4 _MainTex_ST, _DetailTex_ST;

			// 定义一个结构体
			struct Interpolators{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uvDetail : TEXCOORD1;
			};

			struct VertexData{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			Interpolators MyVertexProgram(VertexData v){
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.uvDetail = TRANSFORM_TEX(v.uv, _DetailTex);
				return i;
			}

			float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
				float4 color =  tex2D(_MainTex, i.uv) * _Tint;
				// 颜色相乘 Color c1 = (r1, g1, b1, a1) Color c2 = (r2, g2, b2, a2) 
				// c1 * c2 = (r1 * r2, g1 * g2, b1 * b2, a1 * a2)
				// https://www.zhihu.com/question/24026277  颜色相加和颜色相乘的区别
				// unity_ColorSpaceDouble 定义了在liner color space 和 gamma color space 下的不同倍率  
				color *= tex2D(_DetailTex, i.uvDetail * 10) * unity_ColorSpaceDouble;
				return color;
			}

			// 表示结束
			ENDCG
		}
	}
}
