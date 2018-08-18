Shader "Custom/10VertAnimNormal" {

	Properties{
		// [HideInInspector]  不在面板显示
		//[NoScaleOffset] 不显示tiling和offset
		[Normal]   
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Frequency ("Frequency", float) = 1
		_Amplitude ("Amplitude", float) = 1
		_Speed ("Speed", float) = 1
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
			uniform float _Frequency;
			uniform float _Amplitude;
			uniform float _Speed;

			struct vertIn{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};

			struct vertOut{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			float4 vertexFlagAnim(float4 vertPos, float2 uv){
				vertPos.y = vertPos.y + uv.x * -1; 
				vertPos.z = vertPos.z + sin((uv.x - (_Time.y * _Speed)) * _Frequency) * (_Amplitude * uv.x);
				return vertPos;
			}

			float4 vertexFlagAnimNormal(float4 vertPos,float4 vertNormal, float2 uv){
				// vertPos.y = vertPos.y + uv.x * -1; 
				vertPos = vertPos + sin((vertNormal - (_Time.y * _Speed)) * _Frequency) * (_Amplitude * vertNormal);
				return vertPos;
			}
		
			vertOut vert(vertIn v){
				vertOut o;
				v.vertex = vertexFlagAnimNormal(v.vertex, v.normal, v.texcoord);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord.xy = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
				return o;
			}

			float4 frag(vertOut i) : SV_Target{
				float4 col = tex2D(_MainTex, i.texcoord);
				// 如果 第一个参数小于0， 则返回0， 如果大于1， 则返回1， 否则直接返回第一个参数
				//col.a = drawLine(i.texcoord, _Start, _Width);
				//float2 center = float2(_Center_X, _Center_Y);
				//col.a = drawCircleFade(i.texcoord, center, _Radius, _Feather);
				return col;
			}

			

			ENDCG
		}
	}
}
