Shader "ASESampleShaders/MaskedUI" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_ColorWaveA ("ColorWaveA", Vector) = (0.4485294,0.313166,0.1517085,1)
		_WaveAmplitudeA ("WaveAmplitudeA", Float) = 0
		_YDisplacementA ("YDisplacementA", Float) = 0
		_WaveWidthA ("WaveWidthA", Float) = 2.5
		_WaveAmplitudeB ("WaveAmplitudeB", Float) = 0
		_YDisplacementB ("YDisplacementB", Float) = 5
		_WaveWidthB ("WaveWidthB", Float) = 2.5
		_ColorWaveB ("ColorWaveB", Vector) = (1,0.376056,0.05882353,1)
		_TextureSample2 ("Texture Sample 2", 2D) = "white" {}
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