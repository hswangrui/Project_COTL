Shader "Shadero Customs/Resource" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		ZoomUV_Zoom_1 ("ZoomUV_Zoom_1", Range(0.2, 4)) = 1
		ZoomUV_PosX_1 ("ZoomUV_PosX_1", Range(-3, 3)) = 0.5
		ZoomUV_PosY_1 ("ZoomUV_PosY_1", Range(-3, 3)) = 0.5
		LiquidUV_WaveX_1 ("LiquidUV_WaveX_1", Range(0, 2)) = 2
		LiquidUV_WaveY_1 ("LiquidUV_WaveY_1", Range(0, 2)) = 2
		LiquidUV_DistanceX_1 ("LiquidUV_DistanceX_1", Range(0, 1)) = 0.3
		LiquidUV_DistanceY_1 ("LiquidUV_DistanceY_1", Range(0, 1)) = 0.3
		LiquidUV_Speed_1 ("LiquidUV_Speed_1", Range(-2, 2)) = 1
		_ShinyFX_Pos_1 ("_ShinyFX_Pos_1", Range(-1, 1)) = 0
		_ShinyFX_Size_1 ("_ShinyFX_Size_1", Range(-1, 1)) = -0.1
		_ShinyFX_Smooth_1 ("_ShinyFX_Smooth_1", Range(0, 1)) = 0.25
		_ShinyFX_Intensity_1 ("_ShinyFX_Intensity_1", Range(0, 4)) = 1
		_ShinyFX_Speed_1 ("_ShinyFX_Speed_1", Range(0, 8)) = 1
		_PlasmaLightFX_Fade_1 ("_PlasmaLightFX_Fade_1", Range(0, 1)) = 0.5
		_PlasmaLightFX_Speed_1 ("_PlasmaLightFX_Speed_1", Range(0, 1)) = 0.5
		_PlasmaLightFX_BW_1 ("_PlasmaLightFX_BW_1", Range(0, 1)) = 1
		_ThresholdSmooth_Value_1 ("_ThresholdSmooth_Value_1", Range(-1, 2)) = 1
		_ThresholdSmooth_Smooth_1 ("_ThresholdSmooth_Smooth_1", Range(0, 1)) = 0.7
		_TurnBlackToAlpha_Fade_1 ("_TurnBlackToAlpha_Fade_1", Range(0, 1)) = 1
		_MaskAlpha_Fade_1 ("_MaskAlpha_Fade_1", Range(0, 1)) = 0
		_Add_Fade_2 ("_Add_Fade_2", Range(0, 4)) = 1
		_SpriteFade ("SpriteFade", Range(0, 1)) = 1
		[Toggle(_USEUVOFFSET_)] _UseUVOffset ("Use_UV_OffsetTexture", Float) = 0
		_UVTexture ("UV_OffsetTexture 4", 2D) = "white" {}
		[Separator] [Toggle(_USEDITHERFADE_)] _UseDitherFade ("_Use_Dither_Fade", Float) = 0
		[Header(CameraFade)] _CameraFadeDistance ("Camera Fade Distance", Range(0, 10)) = 5
		[Header(DepthFade)] [Space(10)] _DepthThreshold ("Depth Vignette - Threshold", Range(0, 1)) = 0.3
		_DepthSmoothness ("Depth Vignette - Smoothness", Range(0.01, 1)) = 0.2
		[Space(10)] [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
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