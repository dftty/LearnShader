Shader "Custom/06Cirlce" {

	Properties{
		// [HideInInspector]  不在面板显示
		//[NoScaleOffset] 不显示tiling和offset
		[Normal]   
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Center_X ("CenterX", float) = 0.5
		_Center_Y ("CenterY", float) = 0.5
		_Center ("Center", vector) = (1, 1, 1, 1)
		_Radius ("Radius", float) = 0.5
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
			uniform float _Center_X;
			uniform float _Center_Y;
			uniform float _Radius;

			struct vertIn{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct vertOut{
				float4 pos : SV_POSITION;
				float4 texcoord : TExCoord0;
			};

			float drawCircle(float2 uv, float2 center, float radius){
				radius = pow(radius, 2);
				float dis = pow(uv.x - center.x, 2) + pow(uv.y - center.y, 2);
				if(dis < radius) return 1;
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
				float2 center = float2(_Center_X, _Center_Y);
				col.a = drawCircle(i.texcoord, center, _Radius);
				return col;
			}

			

			ENDCG
		}
	}
}
