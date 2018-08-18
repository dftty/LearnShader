// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderDev/01BareBone"{
	Properties{
	//  变量名   lable      data type R  G  B  A
		_Color("Main Color", color) = (1, 1, 1, 1)
		
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

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			uniform half4 _Color;

			struct VertexInput{
				float4 vertex : POSITION;

			};

			struct VertexOutput{
				float4 pos : SV_POSITION;
			};

			VertexOutput vert(VertexInput v){
				VertexOutput o;
				o.pos = UnityObjectToClipPos( v.vertex);
				return o;
			}

			half4 frag(VertexOutput i) : COLOR{
				return _Color;
			}


			ENDCG
		}
	}
}