Shader "Hidden/Amplify Color/Mask" {
	Properties {
		_MainTex ("Base (RGB)", any) = "" {}
		_RgbTex ("LUT (RGB)", 2D) = "" {}
		_LerpRgbTex ("LerpRGB (RGB)", 2D) = "" {}
		_MaskTex ("Mask (RGB)", any) = "" {}
		_RgbBlendCacheTex ("RgbBlendCache (RGB)", 2D) = "" {}
		_StereoScale ("StereoScale", Vector) = (1,1,0,0)
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