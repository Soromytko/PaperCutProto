Shader "Custom/Polygon"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _HeightMap ("HeightMap", 2D) = "white" {}
        _Center ("Center", Vector) = (0, 0, 0, 0) 
        _Progress ("Progress", Range(0, 1)) = 0
        _HorizontalNoiseForce ("HorizontalNoiseForce", Range(0, 10)) = 2.0
        _VerticalNoiseForce ("VerticalNoiseForce", Range(0, 10)) = 1.0
        _Radius ("Radius", Float) = 1
    }
    SubShader
    {
        Cull Off
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
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
            sampler2D _HeightMap;
            float4 _MainTex_ST;
            fixed4 _Color;
            
            float4 _Center;
            float _Progress, _Radius;
            float _HorizontalNoiseForce;
            float _VerticalNoiseForce;

            float rand(float minVal, float maxVal, float seed)
            {
                float r = frac(sin(seed * 12.9898) * 43758.5453);
                return lerp(minVal, maxVal, r); 
            }

            v2f vert (appdata v)
            {   
                if (_Progress > 0)
                {
                    float3 vertex = v.vertex.xyz - _Center;
                    float len = length(vertex);

                    float per = len / (_Radius);
                    per = clamp(per, 0, 1);

                    vertex.x = vertex.x * (1.0 - _Progress) + vertex.x * pow(1 - per, 1) * _Progress;
                    vertex.y = vertex.y * (1.0 - _Progress) + vertex.y * pow(1 - per, 1) * _Progress;
                    float height = tex2Dlod(_HeightMap, float4(v.uv.xy, 0, 0)).r;
                    vertex.z = height * _VerticalNoiseForce * _Progress;

                    vertex.x += rand(-1, 1, sin(height)) * _HorizontalNoiseForce * (1.0 - height) * _Progress;
                    vertex.y += rand(-1, 1, cos(height)) * _HorizontalNoiseForce * (1.0 - height) * _Progress;

                    v.vertex.xyz = vertex + _Center;
                }

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Color"
}