Shader "VFX/GradientMappedReveal" {
	Properties {
		_MainTexture ("MainTexture", 2D) = "white" {}
		_Reveal ("Reveal", Range(0, 1)) = 0.4740491
		_Start ("Start", Float) = 0.1
		_Falloff ("Falloff", Float) = 0.02
		[HDR] _MainColor ("MainColor", Vector) = (0,0.1918864,1,0.4)
		_RevealTexture ("RevealTexture", 2D) = "white" {}
		_TipIntensity ("TipIntensity", Float) = 0
		_TipLength ("TipLength", Float) = 0.2
		[Toggle(_USEMAINTEXTURECOLOUR_ON)] _UseMainTextureColour ("UseMainTextureColour", Float) = 0
		[Toggle(_REVEALUSES_TEXCOORD0_Z_ON)] _RevealUses_Texcoord0_Z ("RevealUses_Texcoord0_Z", Float) = 0
		_Alpha ("Alpha", Float) = 0
		_GradientMap ("GradientMap", 2D) = "white" {}
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