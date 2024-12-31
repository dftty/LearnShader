Shader "Unlit/First Lighting Shader_6_test"
{
	// 在开头添加属性
	Properties{
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]_NormalMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1	
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
			// #pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex MyVertexProgram      		// 顶点着色器
			#pragma fragment MyFragmentProgram			// 片段着色器

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 vertex : SV_POSITION;

				float3 normalWorld : TEXCOORD1;
				float3 tangentWorld : TEXCOORD2;
				float3 binormalWorld : TEXCOORD3;
			};

			float4 _Tint;
			float _BumpScale;
			float _Metallic;
			float _Smoothness;
			sampler2D _MainTex;
			sampler2D _NormalMap;

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

			v2f MyVertexProgram(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;

				o.normalWorld = mul((float3x3)unity_WorldToObject, v.normal);
				o.tangentWorld = mul((float3x3)unity_WorldToObject, v.tangent.xyz);
				o.binormalWorld = cross(o.normalWorld, o.tangentWorld) * v.tangent.w;
				return o;
			}

			float4 MyFragmentProgram(v2f i) : SV_Target
			{
				// float4 colorAtPixel = tex2D(_NormalMap, i.texcoord);
				// // return colorAtPixel.xyz;
				// // Normal value converted from Color value
				// fixed3 normalAtPixed = normalFromColor(colorAtPixel);
				// // Compose TBN matrix
				// float3x3 TBNWorld = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
				// //float3x3 TBNWorld = float3x3(i.tangentWorld.xyz, float3(1, 1, 1), float3(1, 1, 1));
				// float3 worldNormalAtPixel = normalize(mul(normalAtPixed, TBNWorld));
				// return float4(worldNormalAtPixel, 1);
				
				float3 row1 = float3(1, 1, 0);
				float3 row2 = float3(0, 1, 0);
				float3 row3 = float3(0, 0, 1);
				float3x3 m = float3x3(row1, row2, row3);

				return float4(m[0], 1);
			}

			// 表示结束
			ENDCG
		}

	}
}
