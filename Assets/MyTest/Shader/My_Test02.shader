Shader "Custom/My_Test02" {
	Properties{
		_Color ("_Color", color) = (1, 1, 1, 1)
		_MainTex ("_MainTex", 2D) = "white" {}
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
					o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					return o;
				}

				float4 frag(vertOutput o) : COLOR {
					return tex2D(_MainTex, o.texcoord) * _Color;
					//return sin(o.texcoord.y) * _Color;
					//return cos(o.texcoord.x) * _Color;
					//return sqrt(o.texcoord.x) * _Color;
				}


			ENDCG
		}
	}
}
