Shader "Custom/04Line" {

	Properties{
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Start ("Start", Range(0, 1)) = 0.4
		_Width ("Width", Range(0, 1)) = 0.6
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
			uniform float _Start;
			uniform float _Width;

			struct vertIn{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct vertOut{
				float4 pos : SV_POSITION;
				float4 texcoord : TExCoord0;
			};

			float drawLine(float2 uv, float start, float end){
				if(uv.x > start && uv.x < end){
					return 1;
				}
				return 0;
			}

			float drawCircle(float2 uv){
				float2 center = (0.5, 0.5);
				float radius = 0.5;

				float dis = (uv.x - center.x) * (uv.x - center.x) + (uv.y - center.y) * (uv.y - center.y);
				if(dis < radius * radius) return 1;
				return 0;
			}

			vertOut vert(vertIn v){
				vertOut o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord.xy = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
				return o;
			}

			float4 frag(vertOut i) : SV_Target{
				float4 col = tex2D(_MainTex, i.texcoord);
				// 如果 第一个参数小于0， 则返回0， 如果大于1， 则返回1， 否则直接返回第一个参数
				//col.a = drawLine(i.texcoord, _Start, _Width);
				col.a = drawCircle(i.texcoord);
				return col;
			}

			

			ENDCG
		}
	}
}
