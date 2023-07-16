Shader "VFX/VFX_Dice" {
	Properties {
		_mainTex ("mainTex", 2D) = "white" {}
		_emission ("emission", 2D) = "white" {}
		[HDR] _EmissionColor ("EmissionColor", Vector) = (2,0,0,0)
		_FillColor ("FillColor", Vector) = (0,0,0,0)
		_FillAmount ("FillAmount", Range(0, 1)) = 0
		_EmissionAmount ("EmissionAmount", Range(0, 1)) = 0
		_OutlineWidth ("OutlineWidth", Range(0, 1)) = 1
		[HDR] _OutlineColor ("OutlineColor", Vector) = (1.414214,0,0,1)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
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
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}