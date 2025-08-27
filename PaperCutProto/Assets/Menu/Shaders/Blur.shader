Shader "UI/BlurWithAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 20)) = 1
        _Alpha ("Alpha", Range(0, 1)) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        CGINCLUDE
        #include "UnityCG.cginc"
        
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            fixed4 color : COLOR;
        };
        
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
            fixed4 color : COLOR;
        };
        
        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _MainTex_TexelSize;
        float _BlurSize;
        float _Alpha;
        
        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.color = v.color;
            return o;
        }
        
        fixed4 frag (v2f i) : SV_Target
        {
            float2 offset = _MainTex_TexelSize.xy * _BlurSize;
            
            // Гауссово размытие с учетом альфа-канала
            fixed4 col = tex2D(_MainTex, i.uv) * 0.16;
            col += tex2D(_MainTex, i.uv + float2(offset.x, 0)) * 0.15;
            col += tex2D(_MainTex, i.uv - float2(offset.x, 0)) * 0.15;
            col += tex2D(_MainTex, i.uv + float2(0, offset.y)) * 0.15;
            col += tex2D(_MainTex, i.uv - float2(0, offset.y)) * 0.15;
            col += tex2D(_MainTex, i.uv + offset) * 0.09;
            col += tex2D(_MainTex, i.uv - offset) * 0.09;
            col += tex2D(_MainTex, i.uv + float2(-offset.x, offset.y)) * 0.09;
            col += tex2D(_MainTex, i.uv + float2(offset.x, -offset.y)) * 0.09;
            
            // Применяем альфа-канал и цвет вершины
            col.a *= _Alpha * i.color.a;
            col.rgb *= i.color.rgb;
            
            return col;
        }
        ENDCG
        
        Pass
        {
            Name "Blur"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
    
    FallBack "UI/Default"
}