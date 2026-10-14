Shader "TimeCourier/VoxelLit"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150
        CGPROGRAM
        #pragma surface surf Lambert addshadow fullforwardshadows
        #pragma target 3.0
        struct Input { float3 worldPos; };
        fixed4 _Color;
        void surf (Input IN, inout SurfaceOutput o)
        { o.Albedo = _Color.rgb; o.Alpha = _Color.a; }
        ENDCG
    }
    Fallback "Diffuse"
}
