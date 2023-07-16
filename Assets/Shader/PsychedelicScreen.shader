Shader "PsychedelicScreen" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_Texture0 ("Texture 0", 2D) = "white" {}
		_IntensityOffset ("IntensityOffset", Float) = 1
		_NoiseScale1 ("NoiseScale1", Float) = 0
		_Speed ("Speed", Float) = 1
		_NoiseScale2 ("NoiseScale2", Float) = 0
		_Float0 ("Float 0", Range(0, 1)) = 0.5
		_Texture1 ("Texture 1", 2D) = "white" {}
		_NoiseScale ("NoiseScale", Float) = 0.01
		_RainbowPower ("RainbowPower", Float) = 0
		_DistortPower ("DistortPower", Float) = 0
		_RainbowIntensity ("RainbowIntensity", Float) = 0
		_Texture2 ("Texture 2", 2D) = "white" {}
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
	//CustomEditor "ASEMaterialInspector"
}