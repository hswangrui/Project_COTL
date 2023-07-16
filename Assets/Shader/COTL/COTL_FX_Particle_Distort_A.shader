Shader "COTL/FX_Particle_Distort_A" {
	Properties {
		[NoScaleOffset] [Normal] _DistortMap ("Distortion guide", 2D) = "bump" {}
		_DistortionAmount ("Distortion amount", Float) = 0
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
}