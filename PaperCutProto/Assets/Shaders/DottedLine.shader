Shader "Unlit/DottedLine"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Background ("Background", Color) = (0,0,0,0)
        _Repeat ("Dot Repeat", Float) = 10
        _Threshold ("Dot Size", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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

            float4 _Color;
            float4 _Background;
            float _Repeat;
            float _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Repeat UVs along X
                float pattern = frac(i.uv.x * _Repeat);

                // Dot on / off
                if (pattern < _Threshold)
                {
                    return _Color;
                }
                else
                {
                    return _Background;
                }
            }
            ENDCG
        }
    }
}
