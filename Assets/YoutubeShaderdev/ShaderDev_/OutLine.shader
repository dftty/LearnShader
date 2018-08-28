// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShaderDev/0012OutLine"{
	Properties{
	//  变量名   lable      data type R  G  B  A
		_Color("Main Color", color) = (1, 1, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		_OutLine("OutLine", float) = 0.1
		_OutLineColor("OutLineColor", color) = (1, 1, 1, 1)
	}

	SubShader{

		// Tag 既可以放在SubShader 中，也可以放在Pass中，在Pass中表示仅在该pass中有效
		Tags{
			// 减号之间不能有空格
			// 这里必须是Transparent
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass{
			// 该Pass 的名称
			Name "First"

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			ZWrite off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			uniform float _OutLine;
			uniform half4 _OutLineColor;

			struct VertexInput{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct VertexOutput{
				float4 pos : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			float4 OutLine(float4 vertex, float outLine){
				float4x4 scaleMat = float4x4(float4(1 + outLine, 0, 0, 0), 
											 float4(0, 1 + outLine, 0, 0), 
											 float4(0, 0, 1 + outLine, 0),
											 float4(0, 0, 0, 1));
				return mul(scaleMat, vertex);
			}

			VertexOutput vert(VertexInput v){
				VertexOutput o;
				o.pos = UnityObjectToClipPos(OutLine(v.vertex, _OutLine));
				o.texcoord = v.texcoord;
				return o;
			}

			half4 frag(VertexOutput i) : COLOR{
				if(i.texcoord.x < 0.1){
					_OutLineColor.a = smoothstep(0, 0.1, i.texcoord.x);
				}
				if(i.texcoord.y < 0.1){
					_OutLineColor.a = smoothstep(0, 0.1, i.texcoord.y);
				}
				return _OutLineColor;
			}


			ENDCG
		}

		Pass{
			// 该Pass 的名称
			Name "Second"

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
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				o.pos = UnityObjectToClipPos( v.vertex);
				o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				return o;
			}

			half4 frag(VertexOutput i) : COLOR{
				return tex2D(_MainTex, i.texcoord) * _Color;
			}


			ENDCG
		}

	}
}