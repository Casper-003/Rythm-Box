Shader "UI/FastBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0,0.02)) = 0.005
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 简单的盒式模糊 (Box Blur)，采样周围 9 个点
                fixed4 col = tex2D(_MainTex, i.uv);
                col += tex2D(_MainTex, i.uv + float2(_BlurSize, 0));
                col += tex2D(_MainTex, i.uv - float2(_BlurSize, 0));
                col += tex2D(_MainTex, i.uv + float2(0, _BlurSize));
                col += tex2D(_MainTex, i.uv - float2(0, _BlurSize));
                col += tex2D(_MainTex, i.uv + float2(_BlurSize, _BlurSize));
                col += tex2D(_MainTex, i.uv - float2(_BlurSize, _BlurSize));
                col += tex2D(_MainTex, i.uv + float2(_BlurSize, -_BlurSize));
                col += tex2D(_MainTex, i.uv - float2(_BlurSize, -_BlurSize));
                
                col /= 9.0;
                col.rgb *= 0.6; // 稍微压暗一点
                return col;
            }
            ENDCG
        }
    }
}