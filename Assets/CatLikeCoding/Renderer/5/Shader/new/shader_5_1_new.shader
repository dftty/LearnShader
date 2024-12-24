Shader "Unlit/First Lighting Shader New UnityBRDF_5"
{
    Properties {
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _MainTex("Albedo", 2D) = "white" {}
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
    }

    SubShader {
        Pass{
			Tags{
				// 光照模式标签 
				"LightMode" = "ForwardBase"
			}

			// 表示开始
			CGPROGRAM

			#pragma target 3.0

			// 为了在 vertex函数中直接计算光照，需要进行以下定义，仅可用于点光光源
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex vert      		// 顶点着色器
			#pragma fragment frag			// 片段着色器

			//#include "UnityCG.cginc"					// 包含一些通用方法
			//#include "UnityStandardBRDF.cginc"			// 这个文件中已经包含UnityCG.cginc
			#include "shader_5_1_new.cginc"

			// 表示结束
			ENDCG
		}
        
        Pass {
            Tags {
                "LightMode"="ForwardAdd"
            }

            // Blend默认是 One Zero，即不进行混合，这里需要改成混合才能达到多个光照效果
            Blend One One
            // ZWrite默认是 On，即开启深度写入，这里需要关闭深度写入，否则后绘制的像素会被前绘制的像素覆盖
            // ZWrite 仅对非透明物体有效，对透明物体无效
            ZWrite Off

            CGPROGRAM
            
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fwdadd

            #include "shader_5_1_new.cginc"

            ENDCG
        }
    }
}