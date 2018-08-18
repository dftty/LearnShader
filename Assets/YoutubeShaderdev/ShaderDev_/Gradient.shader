// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderDev/Gradient"{
	Properties{
	//  变量名   lable      data type R  G  B  A
		_Color("Main Color", color) = (1, 1, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
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


			struct VertexInput{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct VertexOutput{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			VertexOutput vert(VertexInput v){
				VertexOutput o;
				o.pos = UnityObjectToClipPos( v.vertex);
				o.texcoord.xy = v.texcoord.xy;
				return o;
			}

			half4 frag(VertexOutput i) : COLOR{
				float4 col = tex2D(_MainTex, i.texcoord) * _Color;
				col.a = i.texcoord.x;
				return col;
			}


			ENDCG
		}
	}
}