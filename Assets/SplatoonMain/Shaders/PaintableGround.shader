Shader "MSY/DynamicSceneObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

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

            ZWrite On
            Cull off

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SamplerOffset;
                
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

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
                float4 col = float4(1, 1, 1, 1);
                // //uv如何映射到装在一维数组里的二维图像
                // int x = 1;
                // x *= i.uv.x * objPixelWidth;
                // int y = 1;
                // y *= i.uv.y * objPixelHeight;
                // int index = x + y * objPixelWidth;
                // float4 bufferColor = pixelArray[index].MainColor;
                // bufferColor = pow(bufferColor, 0.45);
                // // float totalfactor = step(1 - rect, saturate(rect * 0.5 + rect));
                // float4 finalcolor;
                // if (bufferColor.a > 0.2f)
                // {
                //     finalcolor = bufferColor * bufferColor.a + (1 - bufferColor.a) * col;
                // }
                // else
                // {
                //     finalcolor = bufferColor * (3 * bufferColor.a) + (1 - (3 * bufferColor.a)) * col;
                // }

                return col;
            }
            ENDHLSL
        }
    }
}