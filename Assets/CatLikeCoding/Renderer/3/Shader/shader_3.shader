Shader "Unlit/Splat Map"
{
	Properties{
		_MainTex("Splat Map", 2D) = "white" {}
		[NoScaleOffset]Texture1("Texture1", 2D) = "white" {}
		[NoScaleOffset]Texture2("Texture2", 2D) = "white" {}
		[NoScaleOffset]Texture3("Texture3", 2D) = "white" {}
		[NoScaleOffset]Texture4("Texture4", 2D) = "white" {}
	}

	SubShader{
		Pass{
			// 表示开始
			CGPROGRAM

			#pragma vertex MyVertexProgram      		// 顶点着色器
			#pragma fragment MyFragmentProgram			// 片段着色器

			#include "UnityCG.cginc"					// 包含一些通用方法

			// 使用属性需要名称和属性定义中相同
			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D Texture1, Texture2, Texture3, Texture4;

			// 定义一个结构体
			struct Interpolators{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uvSplat : TEXCOORD1;
			};

			struct VertexData{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			Interpolators MyVertexProgram(VertexData v){
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.uvSplat = v.uv;
				return i;
			}

			float4 MyFragmentProgram(Interpolators i) : SV_TARGET{
				float4 splat = tex2D(_MainTex, i.uvSplat);
				return tex2D(Texture1, i.uv) * splat.r + tex2D(Texture2, i.uv) * (splat.g)
						+ tex2D(Texture3, i.uv) * splat.b + tex2D(Texture4, i.uv) * (1 - splat.r - splat.g - splat.b);
			}

			// 表示结束
			ENDCG
		}
	}
}
