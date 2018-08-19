Shader "Custom/My_Test04" {
	Properties{
		_Color ("_Color", color) = (1, 1, 1, 1)
		_MainTex ("_MainTex", 2D) = "white" {}
		_StartPos ("_StartPos", Range(0, 1)) = 0.5
		_Width ("Width", float) = 0.1
	}

	SubShader{
		Tags{
			// TODO Background Geometry AlphaTest Transparent
			"Queue"="Background-100"
		}
		Pass{
			// 如果有透明物体，则需要定义Blend
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				uniform float4 _Color;
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float _StartPos;
				uniform float _Width;

				// 这些数据由系统读取，然后传入
				struct vertInput{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
					float4 tengent : TANGENT;
					float4 normal : NORMAL;
					float4 color : COLOR;
				};

				struct vertOutput{
					float4 pos : SV_POSITION;
					float4 texcoord : TEXCOORD0;
				};

				vertOutput vert(vertInput v){
					vertOutput o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
					//o.texcoord.xy = v.texcoord.xy;
					return o;
				}

				float4 frag(vertOutput o) : COLOR {
					float4 col = tex2D(_MainTex, o.texcoord) * _Color;
					col.a = smoothstep(0.3, 0.8, o.texcoord.x) * _SinTime.w;
					return col;
				}

			ENDCG
		}
	}
}
