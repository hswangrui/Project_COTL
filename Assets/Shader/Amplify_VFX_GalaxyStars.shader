Shader "Amplify_VFX_GalaxyStars" {
	Properties {
		_Speed1 ("Speed", Float) = 5
		[HDR] _MainColor ("MainColor", Vector) = (0,0,0,0)
		_Size1 ("Size", Float) = 1
		_StarTexture ("StarTexture", 2D) = "white" {}
		[Toggle] _DoodleUV_UseUnscaledTime ("DoodleUV_UseUnscaledTime", Float) = 0
		_Mask ("Mask", 2D) = "white" {}
		_StarScaleVariation ("StarScaleVariation", Float) = 0
		_DriftX ("DriftX", Float) = 0
		_DriftY ("DriftY", Float) = 0
		_GalaxyScale ("GalaxyScale", Float) = 0
		_ScreenScale ("ScreenScale", Float) = 5
		[Header(___FLIPBOOK___)] [Toggle(_FLIPBOOKENABLED_ON)] _FlipbookEnabled ("FlipbookEnabled", Float) = 0
		[IntRange] _FlipbookColumns ("FlipbookColumns", Range(1, 10)) = 1
		[IntRange] _FlipbookRows ("FlipbookRows", Range(1, 10)) = 1
		[Toggle] _StencilledByGround ("StencilledByGround", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
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