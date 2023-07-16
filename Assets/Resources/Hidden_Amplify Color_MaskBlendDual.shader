Shader "Hidden/Amplify Color/MaskBlendDual" {
	Properties {
		_MainTex ("Base (RGB)", any) = "" {}
		_RgbTex ("LUT (RGB)", 2D) = "" {}
		_LerpRgbTex ("LerpRGB (RGB)", 2D) = "" {}
		_MaskTex ("Mask (RGB)", any) = "" {}
		_HighlightTex ("Hightlight - LUT (RGB)", 2D) = "" {}
		_LerpHighlightTex ("LerpHightlight - LUT (RGB)", 2D) = "" {}
		_RgbBlendCacheTex ("RgbBlendCache (RGB)", 2D) = "" {}
		_HighlightBlendCacheTex ("HighlightBlendCache (RGB)", 2D) = "" {}
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