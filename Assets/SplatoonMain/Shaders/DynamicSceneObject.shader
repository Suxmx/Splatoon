Shader "MSY/DynamicSceneObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [KeywordEnum(OFF,ON)]SHADOWRECEP("Shadow阴影接受开启/关闭",Float) = 0

        [Space(20)]
        [Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _srcBlend("Src Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _dstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue" = "Geometry+200"
        }
        LOD 100
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            } //LowResolution  UniversalForward
            Blend[_srcBlend][_dstBlend],[_srcAlphaBlend][_dstAlphaBlend]
            ZTest[_zTest]
            ZWrite On
            Cull off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma shader_feature SHADOWRECEP

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SamplerOffset;
                int inputWidth;
                int inputHeight;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct PixelInfo
            {
                float4 MainColor;
                int ColorType;
            };

            StructuredBuffer<PixelInfo> pixelArray;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 col = float4(1,0,0,1);
                //uv如何映射到装在一维数组里的二维图像
                int x = 1;
                x *= i.uv.x * inputWidth;
                int y = 1;
                y *= i.uv.y * inputHeight;
                int index = x + y * inputWidth;
                float4 bufferColor = pixelArray[index].MainColor;
                bufferColor = pow(bufferColor, 0.45);
                float4 finalcolor = col * (1 - bufferColor.a) + bufferColor * bufferColor.a;
                // float4 finalcolor = bufferColor;
                finalcolor.a = 1;
                return finalcolor;
            }
            ENDHLSL
        }
    }
}