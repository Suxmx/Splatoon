// 此着色器使用代码中预定义的颜色来填充网格形状。
Shader "URP/CellShader"
{
    // Unity 着色器的 Properties 代码块。在此示例中，这个代码块为空，
    // 因为在片元着色器代码中预定义了输出颜色。
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }

    // 包含 Shader 代码的 SubShader 代码块。
    SubShader
    {
        // SubShader Tags 定义何时以及在何种条件下执行某个 SubShader 代码块
        // 或某个通道。
        Tags
        {
            "Queue" = "Transparent" "IgnoreProjector" = "true" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            half4 _Color;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag() : SV_Target
            {
                half4 customColor = _Color;
                return customColor;
            }
            ENDHLSL
        }
    }
}