Shader "Custom/HoleMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        ColorMask 0
        ZWrite Off
        Cull Off

        Stencil
        {
            Ref 1
            Comp always
            Pass replace
        }

        Pass {}
    }
}
