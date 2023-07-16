Shader "COTL/FX_ParticleMaster_A_UI" {
	Properties {
		[Header(Main Texture)] _MainTex ("Main Texure", 2D) = "white" {}
		[NoScaleOffset] _GradientMap ("Gradient map", 2D) = "white" {}
		_GradSpread ("Gradient Spread", Range(0, 5)) = 1
		_GradOffset ("Gradient Offset", Range(-1, 1)) = 0
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		[Separator] [Header(Noise)] _NoiseA ("Noise Texure A", 2D) = "white" {}
		_NoiseAInf ("Influence", Range(0, 1)) = 1
		[KeywordEnum(None,Multiply,Subtract)] _OPNOISEA ("Noise A - Op", Float) = 1
		[Toggle(POLAR_A)] _POLAR_A ("Polar UV", Float) = 0
		[Space(20)] _NoiseB ("Noise Texure B", 2D) = "white" {}
		_NoiseBInf ("Influence", Range(0, 1)) = 1
		[KeywordEnum(None,Multiply,Subtract)] _OPNOISEB ("Noise B - Op", Float) = 0
		[Toggle(POLAR_B)] _POLAR_B ("Polar UV", Float) = 0
		[Space(20)] [Separator] [Header(Displacement)] [Toggle(DISPLACE)] _Displace ("Displacement", Float) = 0
		[PowerSlider(3.0)] _DisplaceAmount ("Displace Amount", Range(0.001, 0.5)) = 0
		_DisplaceMap ("Displace Map (XY)", 2D) = "grey" {}
		[Toggle(POLAR_DISP)] _POLAR_DISP ("Polar UV", Float) = 0
		[Separator] [Header(Polar Coordinates)] _PolarTransfrom ("Center (XY) Radial (Z) Length (W)", Vector) = (0.5,0.5,1,1)
		[Separator] [Header(Color Banding)] [Toggle(BANDING)] _Banding ("Color banding", Float) = 0
		_Bands ("Number of bands", Float) = 3
		[Separator] [Header(Alpha Clipping)] _Cutoff ("Cutoff", Range(0.001, 1)) = 0.1
		_CutoffSmooth ("Cutoff softness", Range(0.001, 1)) = 0.1
		[Separator] [Header(Vertex Data)] [Toggle(UNSCALED_TIME)] _UnscaledTime ("Unscaled Time", Float) = 0
		[KeywordEnum(None,Custom,VColor)] _DATA ("(X/R) Cutoff (Y/G) Smoothness (Z/B) Offset", Float) = 0
		[Separator] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		[Header(Stencil)] _StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Separator] [HideInInspector] _dummy ("DummyValue", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}