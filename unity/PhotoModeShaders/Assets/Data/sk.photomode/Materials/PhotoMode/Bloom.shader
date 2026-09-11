Shader "PhotoMode/Bloom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BloomTex ("Bloom Texture", 2D) = "black" {}
        _BloomThreshold ("Bloom Threshold", Float) = 0.75
        _BloomIntensity ("Bloom Intensity", Float) = 1
        _BlurDirection ("Blur Direction", Vector) = (1, 0, 0, 0)
        _BlurOffset ("Blur Offset", Float) = 1
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
        sampler2D _BloomTex;
        float _BloomThreshold;
        float _BloomIntensity;
        float2 _BlurDirection;
        float _BlurOffset;

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
            Name "Threshold"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed luminance = dot(col.rgb, fixed3(0.2126, 0.7152, 0.0722));
                fixed contribution = max(luminance - _BloomThreshold, 0.0);
                fixed scale = contribution / max(luminance, 0.0001);
                col.rgb *= scale;
                return fixed4(col.rgb, 1);
            }
            ENDCG
        }

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
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed3 bloom = tex2D(_BloomTex, i.uv).rgb;
                col.rgb = saturate(col.rgb + bloom * _BloomIntensity);
                return col;
            }
            ENDCG
        }
    }
}
