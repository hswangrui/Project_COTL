Shader "Shadero Customs/PlayerShadowOverlay" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		PositionUV_X_1 ("PositionUV_X_1", Range(-2, 2)) = 0
		PositionUV_Y_1 ("PositionUV_Y_1", Range(-2, 2)) = 0
		_SourceRenderTextureTex_17 ("_SourceRenderTextureTex_17(RGB)", 2D) = "white" {}
		_BlurHQ_Intensity_1 ("_BlurHQ_Intensity_1", Range(1, 16)) = 1
		_FillColor_Color_1 ("_FillColor_Color_1", Vector) = (1,1,1,1)
		_RenderTex_1 ("RenderTex_1(RGB)", 2D) = "white" {}
		_MaskAlpha_Fade_1 ("_MaskAlpha_Fade_1", Range(0, 1)) = 0
		_AlphaIntensity_Fade_1 ("_AlphaIntensity_Fade_1", Range(0, 6)) = 1
		_SpriteFade ("SpriteFade", Range(0, 1)) = 1
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Sprites/Default"
}