Shader "VFX/ImpactInvert" {
	Properties {
		_StepMidpoint ("StepMidpoint", Float) = 0
		_MaxBrightness ("MaxBrightness", Float) = 1
		_Offset ("Offset", Float) = 0
		_Scale ("Scale", Float) = 4
		[HDR] _InvertTint ("InvertTint", Vector) = (1,1,1,1)
		[HDR] _InvertColor ("InvertColor", Vector) = (0,0,0,0)
		[HDR] _InvertHighlight ("InvertHighlight", Vector) = (0,0,0,0)
		_Falloff ("Falloff", Float) = 0.28
		[Header(PARTICLES Controlled By Texcoord Z)] _Spikey ("Spikey", Float) = 0.28
		[Header(_______)] [Header(DISTORT)] [Toggle(_DISTORTENABLED_ON)] _DistortEnabled ("DistortEnabled", Float) = 0
		[Header(PARTICLES Controlled By Texcoord W)] _DistortAmount ("DistortAmount", Float) = 0
		_Speed1 ("Speed", Float) = 5
		_Size1 ("Size", Float) = 1
		[Toggle] _DoodleUV_UseUnscaledTime ("DoodleUV_UseUnscaledTime", Float) = 0
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
	//CustomEditor "ASEMaterialInspector"
}