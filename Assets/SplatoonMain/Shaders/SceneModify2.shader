Shader "Unlit/SceneModify2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SceneNormalTex ("_SceneNormalTex", 2D) = "white" {}
        _NormalTex ("_NormalTex", 2D) = "white" {}
        _MetalSmoothTex ("_MetalSmoothTex", 2D) = "white" {}
        _BumpScale("_BumpScale", Float) = 1


        _Roughness("_Roughness", Range( 0 , 1)) = 0.5
        _Metalness("_Metalness", Range( 0 , 1)) = 0.5
        _SpecularPower("_SpecularPower", Float) = 1

        _MaskTex ("Mask", 2D) = "white" {}

        _Cube("Cube", Cube) = "_Skybox"{}
        _reflectInt("_reflectInt", Range(0.001, 10)) = 1
        _reflectRot("_reflectRot", Range(-3.14, 3.14)) = 1

        _BlackEdgePow("_BlackEdgePow", Range(1, 5)) = 1
        _BlackEdgeAdd("_BlackEdgeAdd", Range(0.01, 3)) = 1

        //_WhiteEdgeInt("_WhiteEdgeInt",Range(1, 10) ) = 1
        //_WhiteEdgePow("_WhiteEdgePow", Range(0, 8)) = 1
        //_WhiteEdgeDis("_WhiteEdgeDis", Range(0.45, 1)) = 0.45
        _RippleColor("_RippleColor",Color) = (1, 1, 1, 1)
        _RippleIntensity("_RippleIntensity", Range(0, 10)) = 1.5

        _RedColor("_RedColor",Color) = (1, 0, 0, 1)
        _BlueColor("_BlueColor",Color) = (0, 0, 1, 1)
    }
    SubShader
    {
        LOD 100

        Pass
        {
            Tags{"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 TtoW0 : TEXCOORD1;
                float4 TtoW1 : TEXCOORD2;
                float4 TtoW2 : TEXCOORD3;
                float3 WorldNormal : TEXCOORD4;
                float3 WorldPos : TEXCOORD5;
                float2 uv1 : TEXCOORD6;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            sampler2D _SceneNormalTex;
            float4 _SceneNormalTex_ST;
            sampler2D _MetalSmoothTex;
            float4 _MetalSmoothTex_ST;
            sampler2D _NormalTex;
            float4 _NormalTex_ST;
            float _BumpScale;
            float _Roughness;
            float _Metalness;
            float _SpecularPower;
            samplerCUBE _Cube;

            float _BlackEdgePow;
            float _BlackEdgeAdd;

            //float _WhiteEdgeInt;
            float _WhiteEdgeDis;
            float _WhiteEdgePow;

            float _reflectInt;
            float _reflectRot;

            float _RippleIntensity;
            float4 _RippleColor;

            float4 _RedColor;
            float4 _BlueColor;

            inline float GGXTermbase (float NdotH, float roughness)
            {
                float a2 = roughness * roughness;
                float d = (NdotH * a2 - NdotH) * NdotH + 1.0f;
                return UNITY_INV_PI * a2 / (d * d + 1e-7f);

            }

            inline float SmithJointGGXVisibilityTermbase(float NdotL, float NdotV, float roughness) 
            {
                float a = roughness;
                float lambdaV = NdotL * (NdotV * (1 - a) + a);
                float lambdaL = NdotV * (NdotL * (1 - a) + a);
                return 0.5f / (lambdaV + lambdaL + 1e-4f); 
            }

            inline float3 FresnelTermbase(float3 F0, float cosA) 
            {
                float t = Pow5(1 - cosA);
                return F0 + (1 - F0) * t;
            }

            inline float OneMinusReflectivityFromMetallicbase(float metallic) 
			{
                float oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
                return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
            }

            float3 RotateAround(float degree, float3 target)
            {
                float rad = degree * UNITY_PI / 180;
                float3x3 m_rotate = float3x3(cos(rad), -sin(rad), 0,
                    sin(rad), cos(rad), 0, 0, 0, 1);
                float3 dir_rotate = mul(m_rotate, target);
                target = float3(dir_rotate.x, dir_rotate.y, dir_rotate.z);
                return target;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = v.uv;
                o.uv.zw = TRANSFORM_TEX(v.uv, _NormalTex);
                o.uv1 = v.uv1;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.WorldPos = worldPos;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
                
                o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                o.WorldNormal = worldNormal;
                TRANSFER_SHADOW(o)
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                
                float3 lm = 1;
                #if LIGHTMAP_ON
                    fixed2 LightMapuv = i.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				    lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap,LightMapuv));
                    //return fixed4(lm, 1);
                #endif
                //Mask
                float4 mask_col = tex2D(_MaskTex, i.uv1);
                float ripple = mask_col.a;
                float rect = max(mask_col.r, mask_col.g);
                float totalfactor = step(1 - rect, saturate(rect * 0.5 + rect));
                //return totalfactor;
                #if SHADER_API_MOBILE
                    mask_col.a = 1-mask_col.a;
                    totalfactor = 1 - totalfactor;
                #endif
                //return lerp(0, 1, totalfactor); 
                float factor = step(1 - mask_col.g, saturate(mask_col.g * 0.5 + mask_col.g));
                //return factor;
                float3 final = lerp(col, _BlueColor, factor).rgb;
                float3 finalb = final;
                factor = step(1 - mask_col.r, saturate(mask_col.r * 0.5 + mask_col.r));
                final = lerp(final, _RedColor, factor).rgb;
                //黑边
                float edge = 0;
                if(mask_col.r > 0.36){
                    edge = 1 - mask_col.r;
                }
                if(mask_col.g > 0.36){
                    edge = 1 - mask_col.g;
                }
                //return edge;
                edge = smoothstep(0, 0.55, edge);
                edge = pow(edge, _BlackEdgePow);
                //return edge;
                float blackEdge = (1 - edge);
                //return blackEdge;
                float c = (lm.x + lm.y + lm.z) / 3;
                lm = lerp(lm, float3(c, c, c) ,totalfactor);
                float b = min(1 - edge, rect);
                float maskRef = step( 0.7, b);

                float maskSpecular = 1 - edge;
                
                ripple = lerp(0, ripple, totalfactor);
                //Light
                float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float3 normal = UnpackNormal(tex2D(_NormalTex, i.uv1 * _NormalTex_ST.xy + _NormalTex_ST.zw));
                normal.xy *= _BumpScale;
                normal.xy += ripple * 1;
                normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                normal = normalize(float3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
                
                float3 sceneNormal = UnpackNormal(tex2D(_SceneNormalTex, i.uv * _SceneNormalTex_ST.xy + _SceneNormalTex_ST.zw));
                sceneNormal.xy *= _BumpScale;
                sceneNormal.xy += ripple * 1;
                sceneNormal.z = sqrt(1.0 - saturate(dot(sceneNormal.xy, sceneNormal.xy)));
                sceneNormal = normalize(float3(dot(i.TtoW0.xyz, sceneNormal), dot(i.TtoW1.xyz, sceneNormal), dot(i.TtoW2.xyz, sceneNormal)));
                normal = lerp(sceneNormal, normal, totalfactor);
                //return fixed4(normal, 1);
                float3 viewReflectDirection = reflect(-viewDir, normal) ;
                
                float3 floatDirection = normalize(viewDir + lightDir);
                float3 reflectDir = -normalize(reflect(lightDir, normalize(normal)));
                //return fixed4(viewReflectDirection, 1);
                float nl = max(saturate(dot(normal, lightDir)), 0.0000001);//防止除0
                float nv = max(saturate(dot(normal, viewDir)), 0.0000001);
                float vh = max(saturate(dot(viewDir, floatDirection)), 0.0000001);
                float lh = max(saturate(dot(lightDir, floatDirection)), 0.0000001);
                float nh = max(saturate(dot(normal, floatDirection)), 0.0000001);
                float3 albedo = final.rgb;
                float3 ambient = albedo * lerp(dot(normal, lightDir) * 0.5 + 0.5, 1, totalfactor) * _LightColor0;
                //return fixed4(ambient * lm, 1);
                float4 metalSmooth = tex2D(_MetalSmoothTex, i.uv);
                _Roughness = _Roughness * lerp(metalSmooth.r, 1, totalfactor);
                _Metalness = _Metalness * lerp(metalSmooth.g, 1, totalfactor);
                float roughness = (1 - _Roughness) * (1 - _Roughness);
                roughness = max(roughness, 0.002);


                float D = GGXTermbase(nh, roughness);
                float G = SmithJointGGXVisibilityTermbase(nl, nv, roughness);
                float3 F0 = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo.rgb, _Metalness);
                float3 F = FresnelTermbase(F0, lh);
                //漫反射系数kd
                float3 kd = OneMinusReflectivityFromMetallicbase(_Metalness);
				kd *= albedo.rgb;

				float3 specular = (D * G * F) * _SpecularPower * _LightColor0.rgb * nl * 3;
                specular = lerp(float3(0, 0, 0), specular, maskSpecular) * (1 - ripple);

                
				float2 reflectCubeRot = float2(cos(_reflectRot), sin(_reflectRot));
                viewReflectDirection.xz = mul(float2x2(
                    reflectCubeRot,
                    float2(-reflectCubeRot.y, reflectCubeRot.x)
                ), viewReflectDirection.xz).xy;
                float4 r = texCUBE(_Cube, viewReflectDirection);
                float4 ref = r * _reflectInt;
                
                float3 iblSpecular = ref.rgb;
				iblSpecular *= iblSpecular * _reflectInt;
                iblSpecular /= (1.0 + roughness * roughness);
                iblSpecular = lerp(float3(0, 0, 0), iblSpecular, maskRef) * (1 - ripple);
                //specular = lerp(float3(0, 0, 0), specular, totalfactor);
                
                //白边
                //float edgeWhite = 0;
                //if(mask_col.r > _WhiteEdgeDis){
                //    edgeWhite = 1 - mask_col.r;
                //}
                //if(mask_col.g > _WhiteEdgeDis){
                //    edgeWhite = 1 - mask_col.g;
                //}
                //edgeWhite = smoothstep(0, 1 - _WhiteEdgeDis, edgeWhite);
                //edgeWhite = pow(edgeWhite, _WhiteEdgePow);
                blackEdge = lerp(1, blackEdge + _BlackEdgeAdd, edge);
                //return blackEdge;
                //edgeWhite = lerp(0, edgeWhite, totalfactor) * r.x ;
                //return blackEdge;
                //return edgeWhite;
                //return float4(specular, 1);
                return float4(ambient * blackEdge * lm  + specular.rgb * lm + iblSpecular.rgb * lm - ripple * 0.1, 1);
            }
            ENDCG
        }

        
    }

    //Fallback "Standard"
}
