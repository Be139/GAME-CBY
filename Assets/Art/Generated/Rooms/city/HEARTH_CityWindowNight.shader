// Night windows for the background city. The old night textures were random coloured noise laid over a tiny UV patch, so every tower looked like confetti.
// This shader ignores the UVs for lighting: each window cell comes from the glass's position on its facade, lit or dark per floor and per window,
// mostly warm white with some cool white and a rare sign colour. Unlit (the city is pure background) and fogged like the rest of the scene.
Shader "HEARTH/CityWindowNight"
{
    Properties
    {
        _BaseMap ("Base Map (frames)", 2D) = "black" {}
        _BaseTint ("Base Tint", Color) = (0.05, 0.055, 0.075, 1)
        _MaskFromBase ("Glass Mask From Base Map", Range(0, 1)) = 0
        _GlassDark ("Dark Glass", Color) = (0.035, 0.045, 0.07, 1)
        _GlassSky ("Glass Sky Reflection", Color) = (0.16, 0.2, 0.34, 1)
        _CellSize ("Window Cell (width, height) m", Vector) = (1.9, 3.6, 0, 0)
        _LitRatio ("Lit Ratio", Range(0, 1)) = 0.45
        _Warm ("Warm White", Color) = (1, 0.82, 0.58, 1)
        _Amber ("Amber", Color) = (1, 0.6, 0.3, 1)
        _Cool ("Cool White", Color) = (0.78, 0.86, 1, 1)
        _Screen ("Screen Blue", Color) = (0.42, 0.62, 1, 1)
        _Accent ("Sign Accent", Color) = (0.25, 0.95, 1, 1)
        _Accent2 ("Sign Accent 2", Color) = (1, 0.35, 0.75, 1)
        _Mix ("Share warm, amber, cool, screen (rest accents)", Vector) = (0.55, 0.15, 0.18, 0.08)
        _Intensity ("Emission Intensity", Float) = 1.6
        _Seed ("Seed", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseTint;
            half _MaskFromBase;
            half4 _GlassDark;
            half4 _GlassSky;
            float4 _CellSize;
            half _LitRatio;
            half4 _Warm;
            half4 _Amber;
            half4 _Cool;
            half4 _Screen;
            half4 _Accent;
            half4 _Accent2;
            float4 _Mix;
            half _Intensity;
            float _Seed;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1; float3 normalWS : TEXCOORD2; float fogFactor : TEXCOORD3; float3 objectWS : TEXCOORD4; };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                o.fogFactor = ComputeFogFactor(pos.positionCS.z);
                o.objectWS = TransformObjectToWorld(float3(0, 0, 0));
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 t = normalize(float3(n.z, 0, -n.x) + float3(1e-5, 0, 0));
                float along = dot(i.positionWS.xz, t.xz);
                float2 g = float2(along / _CellSize.x, i.positionWS.y / _CellSize.y);
                float2 cell = floor(g);
                float2 f = frac(g);

                float building = Hash21(i.objectWS.xz * 0.0137 + _Seed);
                float floorRand = Hash21(float2(cell.y + _Seed * 7.0, building * 91.7));
                float windowRand = Hash21(cell + building * 31.7 + _Seed);
                // offices light up by floor: some floors almost dark, some mostly lit
                float lit = step(windowRand, saturate(_LitRatio * lerp(0.25, 1.6, floorRand)));

                float pick = Hash21(cell * 1.73 + 13.1 + building);
                float a = _Mix.x, b = a + _Mix.y, c = b + _Mix.z, d = c + _Mix.w;
                half3 col = pick < a ? _Warm.rgb : (pick < b ? _Amber.rgb : (pick < c ? _Cool.rgb : (pick < d ? _Screen.rgb : (frac(pick * 7.0) < 0.5 ? _Accent.rgb : _Accent2.rgb))));
                float bright = lerp(0.5, 1.0, Hash21(cell + 7.7 + building));
                // a soft frame inside each cell and a brighter ceiling band, so cells read as rooms rather than squares
                float frame = smoothstep(0.06, 0.12, f.x) * (1 - smoothstep(0.88, 0.94, f.x)) * smoothstep(0.1, 0.16, f.y) * (1 - smoothstep(0.86, 0.92, f.y));
                float ceiling = lerp(0.75, 1.0, smoothstep(0.2, 0.85, f.y));
                float vertical = saturate(1 - abs(n.y) * 2);

                half4 baseC = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float lum = dot(baseC.rgb, float3(0.3, 0.59, 0.11));
                float mask = lerp(1, saturate(1.5 - lum * 1.8), _MaskFromBase);

                float3 v = normalize(GetCameraPositionWS() - i.positionWS);
                float graze = pow(1 - saturate(abs(dot(n, v))), 3);
                half3 glass = lerp(_GlassDark.rgb, _GlassSky.rgb, graze);
                half3 frames = baseC.rgb * _BaseTint.rgb;
                half3 rgb = lerp(frames, glass, mask) + col * (bright * lit * frame * ceiling * vertical * mask * _Intensity);
                rgb = MixFog(rgb, i.fogFactor);
                return half4(rgb, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings vert(Attributes input) { Varyings o; UNITY_SETUP_INSTANCE_ID(input); o.positionCS = TransformObjectToHClip(input.positionOS.xyz); return o; }
            half frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack Off
}
