Shader "Hidden/DepthOfField"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FocusDistance ("Focus Distance", Float) = 5
        _FocusRange ("Focus Range", Float) = 2
        _BlurAmount ("Blur Amount", Float) = 1.5
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
            float _FocusDistance;
            float _FocusRange;
            float _BlurAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Функция размытия
            fixed4 blur(sampler2D tex, float2 uv, float2 texelSize, float radius)
            {
                fixed4 col = 0;
                float total = 0;
                
                for (float x = -radius; x <= radius; x++)
                {
                    for (float y = -radius; y <= radius; y++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        float weight = 1.0 / (1.0 + sqrt(x*x + y*y));
                        col += tex2D(tex, uv + offset) * weight;
                        total += weight;
                    }
                }
                
                return col / total;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Симуляция глубины через UV.y (чем выше, тем дальше)
                float depth = i.uv.y;
                float blurFactor = saturate(abs(depth - _FocusDistance / 10) / _FocusRange);
                blurFactor = pow(blurFactor, 1.5) * _BlurAmount;
                
                fixed4 sharp = tex2D(_MainTex, i.uv);
                fixed4 blurred = blur(_MainTex, i.uv, _MainTex_TexelSize, 2.0);
                
                return lerp(sharp, blurred, blurFactor);
            }
            ENDCG
        }
    }
}