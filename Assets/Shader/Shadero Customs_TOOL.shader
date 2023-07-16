Shader "Shadero Customs/TOOL" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Outline_Size_1 ("_Outline_Size_1", Range(1, 16)) = 2
		_Outline_Color_1 ("_Outline_Color_1", Vector) = (0,0,0,1)
		_Outline_HDR_1 ("_Outline_HDR_1", Range(0, 2)) = 1
		_FillColor_Color_1 ("_FillColor_Color_1", Vector) = (1,1,1,0)
		_ShinyFX_Pos_1 ("_ShinyFX_Pos_1", Range(-1, 1)) = 0
		_ShinyFX_Size_1 ("_ShinyFX_Size_1", Range(-1, 1)) = -0.1
		_ShinyFX_Smooth_1 ("_ShinyFX_Smooth_1", Range(0, 1)) = 0.25
		_ShinyFX_Intensity_1 ("_ShinyFX_Intensity_1", Range(0, 4)) = 1
		_ShinyFX_Speed_1 ("_ShinyFX_Speed_1", Range(0, 8)) = 1
		_OperationBlend_Fade_1 ("_OperationBlend_Fade_1", Range(0, 1)) = 1
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