Shader "Custom/DirtStain"
{
    Properties
    {
        _DirtTex ("Dirt Texture", 2D) = "white" {}
        _CleanTex ("Clean Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MinAlpha ("Min Alpha", Range(0, 1)) = 0.3
        _MaxAlpha ("Max Alpha", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            sampler2D _DirtTex;
            sampler2D _CleanTex;
            sampler2D _MaskTex;
            float _MinAlpha;
            float _MaxAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 dirt = tex2D(_DirtTex, i.uv);
                fixed4 clean = tex2D(_CleanTex, i.uv);
                float mask = tex2D(_MaskTex, i.uv).r;   // 1 = полностью грязно, 0 = чисто

                // Если грязь ещё есть (маска > 0), видимость грязи не может быть ниже _MinAlpha
                float dirtVisibility = mask > 0.001 ? lerp(_MinAlpha, _MaxAlpha, mask) : 0.0;

                // Смешиваем чистую и грязную текстуры с вычисленной видимостью
                return lerp(clean, dirt, dirtVisibility);
            }
            ENDCG
        }
    }
}