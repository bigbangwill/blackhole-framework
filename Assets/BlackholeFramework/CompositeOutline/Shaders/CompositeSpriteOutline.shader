Shader "Blackhole/CompositeOutline/Outline"
{
    Properties
    {
        _MaskTex ("Composite Mask", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (0.05, 0.05, 0.05, 1)
        _OutlineWidth ("Outline Width", Range(1, 6)) = 3
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            float4 _MaskTex_TexelSize;
            half4 _OutlineColor;
            float _OutlineWidth;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;

                #if UNITY_UV_STARTS_AT_TOP
                output.uv.y = 1.0 - output.uv.y;
                #endif

                return output;
            }

            half SampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv).a;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half center = SampleMask(input.uv);
                half surrounding = 0;

                [unroll]
                for (int radius = 1; radius <= 6; radius++)
                {
                    if (radius > _OutlineWidth)
                    {
                        break;
                    }

                    float2 pixel = _MaskTex_TexelSize.xy * radius;
                    surrounding = max(surrounding, SampleMask(input.uv + float2(pixel.x, 0)));
                    surrounding = max(surrounding, SampleMask(input.uv - float2(pixel.x, 0)));
                    surrounding = max(surrounding, SampleMask(input.uv + float2(0, pixel.y)));
                    surrounding = max(surrounding, SampleMask(input.uv - float2(0, pixel.y)));
                    surrounding = max(surrounding, SampleMask(input.uv + pixel));
                    surrounding = max(surrounding, SampleMask(input.uv - pixel));
                    surrounding = max(surrounding, SampleMask(input.uv + float2(pixel.x, -pixel.y)));
                    surrounding = max(surrounding, SampleMask(input.uv + float2(-pixel.x, pixel.y)));
                }

                half outlineAlpha = saturate(surrounding - center) * _OutlineColor.a;
                return half4(_OutlineColor.rgb, outlineAlpha);
            }
            ENDHLSL
        }
    }
}
