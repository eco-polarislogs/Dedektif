// Fareyle arama parıltısı: üzerine gelinen delil / saklama yeri / kişi için ek malzeme olarak çizilir.
// Toplamalı karışım ve HDR renk sayesinde Bloom ile gerçekten ışık saçar. İki geçiş:
//  • Rim: nesnenin kenarlarına doğru güçlenen ışıma + hafif iç dolgu (gölge içindeki eşya da okunur)
//  • Outline: ekranda sabit kalınlıkta dış kontur (küçük ya da düz eşyalar da belli olur)
// Varyant kullanmaz; Resources altında olduğu için derlemeye her zaman girer.
Shader "Dedektif/HoverGlow"
{
    Properties
    {
        [HDR] _GlowColor ("Glow Color", Color) = (2.6, 1.8, 0.8, 1)
        _OutlinePixels ("Outline Width (px @1080p)", Float) = 2.4
        _Fill ("Inner Fill", Float) = 0.16
    }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _GlowColor;
            float _OutlinePixels;
            float _Fill;
        CBUFFER_END

        struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };

        float Pulse() { return 0.8 + 0.2 * sin(_Time.y * 4.6); }
        ENDHLSL

        Pass
        {
            Name "Rim"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 viewWS : TEXCOORD1; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 pos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(pos);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewWS = GetWorldSpaceViewDir(pos);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float ndv = saturate(dot(normalize(i.normalWS), normalize(i.viewWS)));
                float rim = pow(1.0 - ndv, 2.4);
                float k = (_Fill + rim * 0.95) * Pulse();
                return half4(_GlowColor.rgb * k, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float4 cs = TransformObjectToHClip(v.positionOS.xyz);
                float3 nWS = TransformObjectToWorldNormal(v.normalOS);
                float2 nCS = mul((float3x3)UNITY_MATRIX_VP, nWS).xy;
                float len = max(length(nCS), 1e-4);
                float px = _OutlinePixels * (_ScreenParams.y / 1080.0);
                cs.xy += (nCS / len) * px * 2.0 / _ScreenParams.xy * cs.w;
                o.positionCS = cs;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return half4(_GlowColor.rgb * 0.9 * Pulse(), 0);
            }
            ENDHLSL
        }
    }
}
