Shader "Custom/03Sqrt" {

	Properties{
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader{

		Pass{

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _MainTex;
			uniform half4 _Color;
			uniform float4 _MainTex_ST;

			struct vertIn{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct vertOut{
				float4 pos : SV_POSITION;
				float4 texcoord : TExCoord0;
			};

			vertOut vert(vertIn v){
				vertOut o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord.xy = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
				return o;
			}
			float4 frag(vertOut i) : SV_Target{
				float4 col = tex2D(_MainTex, i.texcoord);
				// 如果 第一个参数小于0， 则返回0， 如果大于1， 则返回1， 否则直接返回第一个参数
				col.a = clamp(sin(i.texcoord.x * 20), 0, 1);
				return col;
			}

			ENDCG
		}
	}
}
