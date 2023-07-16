Shader "Shadero Customs/Water" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		LiquidUV_WaveX_1 ("LiquidUV_WaveX_1", Range(0, 2)) = 2
		LiquidUV_WaveY_1 ("LiquidUV_WaveY_1", Range(0, 2)) = 2
		LiquidUV_DistanceX_1 ("LiquidUV_DistanceX_1", Range(0, 1)) = 0.3
		LiquidUV_DistanceY_1 ("LiquidUV_DistanceY_1", Range(0, 1)) = 0.3
		LiquidUV_Speed_1 ("LiquidUV_Speed_1", Range(-2, 2)) = 1
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