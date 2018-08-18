Shader "Custom/My_Test01" {
	Properties{
		_Color ("_Color", color) = (1, 1, 1, 1)
	}

	SubShader{
		Pass{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			float4 _Color;

			struct vertInput{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float4 normal : TEXCOORD0;
			};


			v2f vert(vertInput v){
				v2f f;
				f.pos = UnityObjectToClipPos(v.vertex);
				f.normal = normalize(mul(v.normal, unity_WorldToObject)) ;
				return f;
			}

			float4 frag(v2f f) : COLOR{
				return f.normal * 0.5 + 0.5;
			}

			ENDCG
		}
	}
}
