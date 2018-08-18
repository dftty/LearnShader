// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderDev/11NormalMap"{
	Properties{
	//  变量名   lable      data type R  G  B  A
		_Color("Main Color", color) = (1, 1, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		_NormalMap("NromalMap", 2D) = "white" {}
	}

	SubShader{

		// Tag 既可以放在SubShader 中，也可以放在Pass中，在Pass中表示仅在该pass中有效
		Tags{
			// 减号之间不能有空格
			"Queue" = "Geometry-500"
			"IgnoreProjector" = "False"
		}
		Pass{
			// 该Pass 的名称
			Name "First"

			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			uniform half4 _Color;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			uniform sampler2D _NormalMap;
			uniform float4 _NormalMap_ST;


			struct VertexInput{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
			};

			struct VertexOutput{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;

				float4 normalWorld : TEXCOORD1;
				float4 tangentWorld : TEXCOORD2;
				float3 binormalWorld : TEXCOORD3;
				float4 normalTexcoord: TEXCOORD4;
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

			VertexOutput vert(VertexInput v){
				VertexOutput o;
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				o.pos = UnityObjectToClipPos( v.vertex);
				o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				
				o.normalTexcoord.xy = (v.texcoord.xy * _NormalMap_ST.xy + _NormalMap_ST.zw);

				o.normalWorld = normalize(mul(v.normal, unity_WorldToObject));
				o.tangentWorld = normalize(mul(v.tangent, unity_ObjectToWorld));
				o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w);
				return o;
			}

			half4 frag(VertexOutput i) : COLOR{
				// Color at Pixel which we read from Tangent space normal map
				float4 colorAtPixel = tex2D(_NormalMap, i.normalTexcoord);
				
				// Normal value converted from Color value
				fixed3 normalAtPixed = normalFromColor(colorAtPixel);

				// Compose TBN matrix
				float3x3 TBNWorld = float3x3(i.tangentWorld.xyz, i.binormalWorld.xyz, i.normalWorld.xyz);
				//float3x3 TBNWorld = float3x3(i.tangentWorld.xyz, float3(1, 1, 1), float3(1, 1, 1));
				float2 worldNormalAtPixel = normalize(mul(normalAtPixed, TBNWorld));

				return tex2D(_MainTex, i.texcoord) * _Color;
			}


			ENDCG
		}
	}
}