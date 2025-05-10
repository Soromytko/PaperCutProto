Shader "Custom/CrumpledPaper"
{
    Properties
    {
        _MainTex ("Текстура бумаги", 2D) = "white" {}
        _BumpMap ("Карта неровностей", 2D) = "bump" {}
        _HeightMap ("Карта высот", 2D) = "white" {}
        _Extrusion ("Сила выдавливания", Range(0, 2)) = 0.5
        _Crumple ("Сила сминания", Range(0, 1)) = 0
        _NoiseScale ("Масштаб шума", Float) = 5
        _Radius ("Радиус комка", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex, _BumpMap, _HeightMap;
        float _Extrusion, _Crumple, _NoiseScale, _Radius;

        // 3D шум для случайного смятия
        float noise(float3 p) {
            return frac(sin(dot(p, float3(12.9898, 78.233, 45.543))) * 43758.5453);
        }

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        void vert(inout appdata_full v)
        {
            // Исходная позиция вершины
            float3 pos = v.vertex.xyz;
            
            // 1. Выдавливание по карте высот
            float height = tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0)).r;
            pos += v.normal * height * _Extrusion;

            // 2. Эффект сминания (если _Crumple > 0)
            if (_Crumple > 0) {
                // Случайная точка на сфере (ядро комка)
                float3 seed = float3(
                    sin(pos.x * _NoiseScale) * 123.456,
                    cos(pos.y * _NoiseScale) * 789.012,
                    sin(pos.z * _NoiseScale) * 345.678
                );
                float3 crumpleCenter = normalize(seed) * _Radius;

                // Смещаем вершину к случайному центру
                float3 toCenter = crumpleCenter - pos;
                pos += toCenter * _Crumple * (1 - height); // Чем выше вершина, тем меньше сминаем
            }

            v.vertex.xyz = pos;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 color = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = color.rgb;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Metallic = 0;
            o.Smoothness = 0.2;
        }
        ENDCG
    }
    FallBack "Diffuse"
}