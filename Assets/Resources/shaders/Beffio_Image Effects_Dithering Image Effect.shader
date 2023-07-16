Shader "Beffio/Image Effects/Dithering Image Effect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_PaletteColorCount ("Mixed Color Count", Float) = 4
		_PaletteHeight ("Palette Height", Float) = 128
		_PaletteTex ("Palette", 2D) = "black" {}
		_PatternSize ("Palette Size", Float) = 8
		_PatternTex ("Palette Texture", 2D) = "black" {}
		_PatternScale ("Pattern Scale", Float) = 1
		[Toggle(_USESECONDPALETTE)] _UseSecondPalette ("Use Second Palette", Float) = 0
		_PaletteTex2 ("Palette2", 2D) = "black" {}
		_PaletteLerp ("PalatteLerp", Float) = 1
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
	Fallback "Unlit/Texture"
}