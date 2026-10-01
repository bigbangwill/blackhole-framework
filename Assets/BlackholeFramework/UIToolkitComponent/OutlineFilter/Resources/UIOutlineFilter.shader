Shader "Hidden/BigBang/UI Toolkit/Outline Filter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 2
        _OutlineResolution ("Outline Resolution", Range(1, 6)) = 4
        _SourceOpacity ("Source Opacity", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Blend One Zero
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _UIE_OUTPUT_LINEAR

            #include "UnityCG.cginc"
            #include "UnityUIEFilter.cginc"

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                uint rectIndex : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineResolution;
            float _OutlineWidth;
            float _SourceOpacity;

            Varyings Vert(FilterVertexInput input)
            {
                Varyings output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.rectIndex = GetFilterRectIndex(input);
                return output;
            }

            fixed4 SampleSource(float2 uv, float4 uvRect)
            {
                float2 rectMin = uvRect.xy;
                float2 rectMax = uvRect.xy + uvRect.zw;
                float inside = step(rectMin.x, uv.x) * step(rectMin.y, uv.y) * step(uv.x, rectMax.x) * step(uv.y, rectMax.y);
                return tex2D(_MainTex, clamp(uv, rectMin, rectMax)) * inside;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float4 uvRect = GetFilterUVRect(input.rectIndex);
                fixed4 source = SampleSource(input.uv, uvRect);
                float maxAlpha = source.a;
                int ringCount = clamp((int)round(_OutlineResolution * 4.0), 4, 24);
                int directionCount = clamp((int)round(8.0 + _OutlineResolution * 8.0), 16, 56);

                [loop]
                for (int ring = 1; ring <= ringCount; ring++)
                {
                    float radius = _OutlineWidth * ring / ringCount;

                    [loop]
                    for (int direction = 0; direction < directionCount; direction++)
                    {
                        float angle = direction * 6.2831853072 / directionCount;
                        float sine;
                        float cosine;
                        sincos(angle, sine, cosine);
                        float2 offset = float2(cosine, sine) * radius * _MainTex_TexelSize.xy;
                        maxAlpha = max(maxAlpha, SampleSource(input.uv + offset, uvRect).a);
                    }
                }

                float outlineAlpha = saturate(maxAlpha - source.a) * _OutlineColor.a;
                float sourceOpacity = saturate(_SourceOpacity);
                fixed4 result;
                result.rgb = source.rgb * sourceOpacity + _OutlineColor.rgb * outlineAlpha;
                result.a = source.a * sourceOpacity + outlineAlpha;

                #if _UIE_OUTPUT_LINEAR
                result.rgb = GammaToLinearSpace(result.rgb);
                #endif

                return result;
            }
            ENDCG
        }
    }
}
