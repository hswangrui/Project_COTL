Shader "Hidden/BlendModes/DappleLighting/Grab" {
	Properties {
		_TexA ("Texture A (R)", 2D) = "black" {}
		[Toggle(INVERT_ON)] _InvertOn ("Invert On", Float) = 0
		_ShadowColor ("Shadow Color", Vector) = (0.5,0.5,0.7,1)
		_LightThreshold ("Light Threshold", Range(0, 1)) = 0.5
		_LightClamp ("Light Clamp", Range(0, 1)) = 0.5
		[Header(Dynamics)] _BloomAmnt ("Bloom - Amount", Range(0, 5)) = 1
		_DistortAmnt ("Distort - Amount", Range(0, 1)) = 0.5
		_WaveSpeed ("Wave - Speed", Range(0, 2)) = 1
		_WaveScale ("Wave - Scale", Range(0, 2)) = 1
		[Header(Blend State)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DestBlend", Float) = 0
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