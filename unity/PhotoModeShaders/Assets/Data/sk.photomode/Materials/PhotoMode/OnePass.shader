Shader "PhotoMode/OnePass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Exposure ("Exposure", Float) = 0
        _Brightness ("Brightness", Float) = 0
        _Contrast ("Contrast", Float) = 0
        _Gamma ("Gamma", Float) = 1
        _Saturation ("Saturation", Float) = 0
        _Temperature ("Temperature", Float) = 0
        _Tint ("Tint", Float) = 0
        _VignetteIntensity ("Vignette Intensity", Float) = 0
        _VignetteRadius ("Vignette Radius", Float) = 1
        _VignetteSoftness ("Vignette Softness", Float) = 0.5
        _SharpenStrength ("Sharpen Strength", Float) = 0
        _LutTex ("LUT", 2D) = "white" {}
        _LutIntensity ("LUT Intensity", Float) = 0
        _GrainIntensity ("Grain Intensity", Float) = 0
        _GrainSize ("Grain Size", Float) = 1.5
        _ChromaticAberrationIntensity ("Chromatic Aberration Intensity", Float) = 0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Exposure;
            float _Brightness;
            float _Contrast;
            float _Gamma;
            float _Saturation;
            float _Temperature;
            float _Tint;
            float _VignetteIntensity;
            float _VignetteRadius;
            float _VignetteSoftness;
            float _SharpenStrength;
            sampler2D _LutTex;
            float4 _LutTex_TexelSize;
            float _LutIntensity;
            float _GrainIntensity;
            float _GrainSize;
            float _ChromaticAberrationIntensity;

            float GrainHash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
            }

            fixed3 ApplyLut(fixed3 color)
            {
                float lutSize = _LutTex_TexelSize.w;
                float3 scaleOffset = float3(_LutTex_TexelSize.x, _LutTex_TexelSize.y, lutSize - 1.0);

                float3 uvw = saturate(color);
                uvw.z *= scaleOffset.z;
                float shift = floor(uvw.z);
                uvw.xy = uvw.xy * scaleOffset.z * scaleOffset.xy + scaleOffset.xy * 0.5;
                uvw.x += shift * scaleOffset.y;

                fixed3 lutColor = lerp(
                    tex2D(_LutTex, uvw.xy).rgb,
                    tex2D(_LutTex, uvw.xy + float2(scaleOffset.y, 0)).rgb,
                    uvw.z - shift);

                return lerp(color, lutColor, saturate(_LutIntensity));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                if (_ChromaticAberrationIntensity > 0.0001)
                {
                    float2 caOffset = (i.uv - 0.5) * _ChromaticAberrationIntensity * 0.02;
                    col.r = tex2D(_MainTex, i.uv - caOffset).r;
                    col.g = tex2D(_MainTex, i.uv).g;
                    col.b = tex2D(_MainTex, i.uv + caOffset).b;
                    col.a = tex2D(_MainTex, i.uv).a;
                }
                else
                {
                    col = tex2D(_MainTex, i.uv);
                }

                col.rgb *= exp2(_Exposure);
                col.rgb += _Brightness;

                col.rgb = (col.rgb - 0.5) * (1 + _Contrast) + 0.5;

                col.rgb = pow(max(col.rgb, 0.0001), 1.0 / max(_Gamma, 0.0001));

                fixed luminance = dot(col.rgb, fixed3(0.2126, 0.7152, 0.0722));
                col.rgb = lerp(fixed3(luminance, luminance, luminance), col.rgb, 1 + _Saturation);

                col.rgb += fixed3(_Temperature, _Tint, -_Temperature) * 0.15;

                if (_LutIntensity > 0.0001)
                {
                    col.rgb = ApplyLut(saturate(col.rgb));
                }

                float2 centered = (i.uv - 0.5) * 2;
                centered.x *= _MainTex_TexelSize.z / _MainTex_TexelSize.w;
                float vignetteDist = length(centered);
                float vignette = 1 - smoothstep(_VignetteRadius, _VignetteRadius + _VignetteSoftness + 0.0001, vignetteDist);
                col.rgb *= lerp(1, vignette, saturate(_VignetteIntensity));

                if (_SharpenStrength > 0.0001)
                {
                    float2 texel = _MainTex_TexelSize.xy;
                    fixed3 north = tex2D(_MainTex, i.uv + float2(0, texel.y)).rgb;
                    fixed3 south = tex2D(_MainTex, i.uv - float2(0, texel.y)).rgb;
                    fixed3 east = tex2D(_MainTex, i.uv + float2(texel.x, 0)).rgb;
                    fixed3 west = tex2D(_MainTex, i.uv - float2(texel.x, 0)).rgb;
                    fixed3 blur = (north + south + east + west) * 0.25;
                    fixed3 detail = tex2D(_MainTex, i.uv).rgb - blur;
                    col.rgb += detail * _SharpenStrength * 2;
                }

                if (_GrainIntensity > 0.0001)
                {
                    float2 grainCell = floor(i.uv * _MainTex_TexelSize.zw / max(_GrainSize, 0.0001));
                    float grainNoise = GrainHash(grainCell) * 2.0 - 1.0;
                    col.rgb += grainNoise * _GrainIntensity * 0.12;
                }

                return col;
            }
            ENDCG
        }
    }
}
