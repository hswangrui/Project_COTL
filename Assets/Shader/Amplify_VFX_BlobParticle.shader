Shader "Amplify/VFX/BlobParticle" {
	Properties {
		[Header(Shape Options)] [Toggle(_RIBBON_ON)] _Ribbon ("Ribbon", Float) = 0
		[Toggle(_TEXTURE_ON)] _Texture ("Texture", Float) = 0
		_BlobTexture ("BlobTexture", 2D) = "white" {}
		_TrailDistortStart ("TrailDistortStart", Float) = 0
		_TrailDistortEnd ("TrailDistortEnd", Float) = 0
		[HDR] _OutlineColor ("OutlineColor", Vector) = (0,0,0,0)
		_AlphaTest ("AlphaTest", Float) = 0.4
		_NoiseScale ("NoiseScale", Float) = 0
		_DistortFactor ("DistortFactor", Float) = 0
		_RibbonFade ("RibbonFade", Float) = 0
		[HDR] _MainColor ("MainColor", Vector) = (0,0,0,0)
		[HDR] _fadeColor ("fadeColor", Vector) = (0,0,0,0)
		[Toggle] _StencilledByGround ("StencilledByGround", Float) = 0
		_Alpha ("Alpha", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	//CustomEditor "ASEMaterialInspector"
}