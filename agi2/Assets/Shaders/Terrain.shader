Shader "Custom/Terrain"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _GrassTex ("Grass Texture (RGB)", 2D) = "white" {}
        _SandTex ("Sand Texture (RGB)", 2D) = "white" {}
        _NoiseTex ("Edge Noise Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MaskTex;
        sampler2D _GrassTex;
        sampler2D _SandTex;
        sampler2D _NoiseTex;

        struct Input
        {
            float2 uv_MaskTex;
            float2 uv_GrassTex;
            float2 uv_SandTex;
            float2 uv_NoiseTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 mask = tex2D (_MaskTex, IN.uv_MaskTex);
            fixed4 grass = tex2D (_GrassTex, IN.uv_GrassTex);
            fixed4 sand = tex2D (_SandTex, IN.uv_SandTex);
            fixed4 noise = tex2D (_NoiseTex, IN.uv_NoiseTex) * 0.3;
            half noisedMask = saturate(mask.r - noise);
            // Shade edges of sand
            fixed3 shadedSand = lerp(sand.rgb, fixed3(0.2, 0, 0), mask.r * mask.r);
            // Shade edges of grass
            half grassShadeStep = step(noisedMask, 0.7);
            fixed3 shadedGrass = lerp(grass.rgb, grass.rgb * 0.3, 1 - mask.r + grassShadeStep * 0.2);
            // Blend sand to grass based on mask texture
            half grassSandStep = step(noisedMask, 0.5);
            o.Albedo = lerp(shadedGrass, shadedSand, grassSandStep);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
