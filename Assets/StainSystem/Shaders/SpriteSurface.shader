Shader "Splatter/SpriteSurface"
{
    Properties
    {
		_StencilRef ("Stencil Layer", Float) = 1
		[PerRendererData] _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
			Stencil
			{
				Ref [_StencilRef]
				ReadMask 128
				WriteMask 128
				Comp Always
				Pass Replace
			}

        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment CustomSpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            
			fixed _AlphaCutoff;

            fixed4 CustomSpriteFrag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                c.rgb *= c.a;
                
                if (c.a < _AlphaCutoff) {
                    discard;
                }
                
                return c;
            }

        ENDCG
        }
    }
}
