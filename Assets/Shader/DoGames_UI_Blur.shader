Shader "DoGames/UI/Blur" {
	Properties {
		_Size ("Blur", Range(0, 30)) = 1
		[HideInInspector] _MainTex ("Masking Texture", 2D) = "white" {}
		_AdditiveColor ("Additive Tint color", Vector) = (0,0,0,0)
		_MultiplyColor ("Multiply Tint color", Vector) = (1,1,1,1)
		[Header(Stencil Buffer)] _StencilRTMaskInf ("Stencil Render Texture Influence", Range(0, 1)) = 1
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
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
}