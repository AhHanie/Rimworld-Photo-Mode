Shader "PhotoMode/SelectiveBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurTex ("Blur Texture", 2D) = "black" {}
        _BlurDirection ("Blur Direction", Vector) = (1, 0, 0, 0)
        _BlurOffset ("Blur Offset", Float) = 1
        _FocusPosition ("Focus Position", Float) = 0.5
        _FocusWidth ("Focus Width", Float) = 0.25
        _TiltShiftStrength ("Tilt-Shift Strength", Float) = 0
        _Falloff ("Falloff", Float) = 0.25
        _EdgeBlurStrength ("Edge Blur Strength", Float) = 0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
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
        sampler2D _BlurTex;
        float2 _BlurDirection;
        float _BlurOffset;
        float _FocusPosition;
        float _FocusWidth;
        float _TiltShiftStrength;
        float _Falloff;
        float _EdgeBlurStrength;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }
        ENDCG

        Pass
        {
            Name "Blur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _BlurDirection * _BlurOffset;

                fixed3 sum = tex2D(_MainTex, i.uv).rgb * 0.227027;
                sum += tex2D(_MainTex, i.uv + texel * 1.0).rgb * 0.1945946;
                sum += tex2D(_MainTex, i.uv - texel * 1.0).rgb * 0.1945946;
                sum += tex2D(_MainTex, i.uv + texel * 2.0).rgb * 0.1216216;
                sum += tex2D(_MainTex, i.uv - texel * 2.0).rgb * 0.1216216;

                return fixed4(sum, 1);
            }
            ENDCG
        }

        Pass
        {
            Name "Composite"
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 sharpCol = tex2D(_MainTex, i.uv);
                fixed3 blurredCol = tex2D(_BlurTex, i.uv).rgb;

                float tiltBlend = 0;
                if (_TiltShiftStrength > 0.0001)
                {
                    float dist = abs(i.uv.y - _FocusPosition) - _FocusWidth * 0.5;
                    tiltBlend = saturate(dist / max(_Falloff, 0.0001)) * _TiltShiftStrength;
                }

                float edgeBlend = 0;
                if (_EdgeBlurStrength > 0.0001)
                {
                    float2 centered = (i.uv - 0.5) * 2;
                    centered.x *= _MainTex_TexelSize.z / _MainTex_TexelSize.w;
                    float edgeDist = length(centered);
                    edgeBlend = saturate(smoothstep(0.5, 1.2, edgeDist)) * _EdgeBlurStrength;
                }

                float blend = saturate(max(tiltBlend, edgeBlend));
                fixed3 result = lerp(sharpCol.rgb, blurredCol, blend);
                return fixed4(result, sharpCol.a);
            }
            ENDCG
        }
    }
}
