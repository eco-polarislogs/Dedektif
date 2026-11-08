Shader "Dedektif/ForensicEvidence"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        // Adli Tıp Katmanları
        _BloodMap ("Blood Pattern", 2D) = "black" {}
        _BloodColor ("Blood Luminol Color", Color) = (0.2, 0.8, 1.0, 1.0)
        _BloodReveal ("Blood Reveal Progress", Range(0, 1)) = 0.0

        _PrintMap ("Fingerprint Pattern", 2D) = "black" {}
        _PrintColor ("Fingerprint Dust Color", Color) = (0.9, 0.85, 0.7, 1.0)
        _PrintReveal ("Print Reveal Progress", Range(0, 1)) = 0.0

        // UV Işık Etkisi
        _UVIntensity ("UV Lamp Intensity", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BloodMap);
            SAMPLER(sampler_BloodMap);
            TEXTURE2D(_PrintMap);
            SAMPLER(sampler_PrintMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Smoothness;
                float _Metallic;
                float4 _BloodColor;
                float _BloodReveal;
                float4 _PrintColor;
                float _PrintReveal;
                float _UVIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half4 bloodTex = SAMPLE_TEXTURE2D(_BloodMap, sampler_BloodMap, input.uv * 2.0);
                half4 printTex = SAMPLE_TEXTURE2D(_PrintMap, sampler_PrintMap, input.uv * 3.0);

                // Normal aydınlatma
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalize(input.normalWS), mainLight.direction));
                half3 lighting = mainLight.color * (NdotL * 0.75 + 0.25);

                half3 finalColor = baseTex.rgb * lighting;

                // Tozlama fırçası ile ortaya çıkan parmak izi
                half printMask = smoothstep(0.2, 0.8, printTex.r) * _PrintReveal;
                finalColor = lerp(finalColor, _PrintColor.rgb, printMask * 0.85);

                // UV Işığı altında parlayan Luminol Kan Reaksiyonu
                half bloodMask = smoothstep(0.1, 0.7, bloodTex.r) * _BloodReveal;
                half3 uvGlow = _BloodColor.rgb * bloodMask * (1.5 + sin(_Time.y * 3.0) * 0.3) * _UVIntensity;

                finalColor += uvGlow;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}

