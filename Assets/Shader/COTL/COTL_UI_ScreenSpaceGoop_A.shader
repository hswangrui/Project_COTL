Shader "COTL/UI_ScreenSpaceGoop_A" {
	Properties {
		[Header(Core)] _NoiseThreshold ("Noise Threshold", Range(0, 1)) = 0
		_NoiseSmoothness ("Noise Smoothness", Range(0.001, 1)) = 0
		[NoScaleOffset] _GradientMap ("Gradient map", 2D) = "white" {}
		[Separator] [Header(Noise)] [Toggle(SCREEN_UV)] _ScreenUV ("Screenspace UVs", Float) = 0
		[Toggle(SCREEN_ASPECT)] _ScreenAspect ("Screen Aspect Correction", Float) = 0
		[Space(20)] _NoiseA ("Noise Texure A", 2D) = "white" {}
		_NoiseA_UScale ("Uniform Scale", Float) = 1
		_NoiseAInf ("Influence", Range(0, 1)) = 1
		[KeywordEnum(None,Multiply,Subtract)] _OPNOISEA ("Noise A - Op", Float) = 1
		[Space(20)] _NoiseB ("Noise Texure B", 2D) = "white" {}
		_NoiseB_UScale ("Uniform Scale", Float) = 1
		_NoiseBInf ("Influence", Range(0, 1)) = 1
		[KeywordEnum(None,Multiply,Subtract)] _OPNOISEB ("Noise B - Op", Float) = 0
		[Separator] [Header(Displacement)] _DisplaceTex ("Displace Tex", 2D) = "grey" {}
		[PowerSlider(3.0)] _DisplaceAmount ("Displace Amount", Range(0.001, 0.5)) = 0
		_Disp_UScale ("Uniform Scale", Float) = 1
		[Separator] [Header(Time)] [PowerSlider(2.0)] _TimeScale ("Time Scale", Range(0, 0.5)) = 0.1
		[Separator] [Header(Vignette)] _Falloff ("Falloff", Range(0, 1)) = 0.5
		_FalloffSmooth ("Falloff Smoothness", Range(1, 10)) = 1
		[Separator] [Header(Mask)] [Toggle(MASK_DEBUG)] _RectMask ("Debug mask", Float) = 0
		[Space(20)] _RectAParams ("Rectangle A - Scale (XY) Offset (ZW)", Vector) = (0.5,0.5,0.25,0.25)
		_RectARotate ("Rectangle A - Rotate", Range(0, 1)) = 0
		[Space(20)] _RectBParams ("Rectangle B - Scale (XY) Offset (ZW)", Vector) = (0.5,0.5,0.25,0.25)
		_RectBRotate ("Rectangle B - Rotate", Range(0, 1)) = 0
		[Space(20)] _RectMaskCutoff ("Rectangle mask cutoff", Range(0, 1)) = 0
		_RectSmoothness ("Rectangle mask smoothness", Range(0, 1)) = 0
		[Header(Blending)] [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Blend Operation", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 10
		[Header(Stencil)] _StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
}