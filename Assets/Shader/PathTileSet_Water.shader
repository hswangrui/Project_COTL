Shader "PathTileSet_Water" {
	Properties {
		_MainTex ("_MainTex", 2D) = "white" {}
		_Cutoff ("_Cutoff", Range(0, 1)) = 0.5
		_GradColHigh ("_GradColHigh", Vector) = (0.2069242,0.4811392,0.7075472,0)
		_GradColLow ("_GradColLow", Vector) = (0.03203988,0.2223729,0.3773585,0)
		_NoiseA ("_NoiseA", 2D) = "white" {}
		_RippleNoiseScaleOffset ("_RippleNoiseScaleOffset", Vector) = (1,0.64,0.2,0.1)
		_RippleCol ("_RippleCol", Vector) = (1,1,1,0)
		_RippleThreshold ("_RippleThreshold", Range(0, 2)) = 0.9968979
		_RippleSmooth ("_RippleSmooth", Range(0.01, 1)) = 0.01
		_RippleSpeed ("_RippleSpeed", Range(0, 10)) = 0.9235293
		_EdgeNoise ("_EdgeNoise", 2D) = "white" {}
		_EdgeNoiseScaleOffset ("_EdgeNoiseScaleOffset", Vector) = (0.5,1,0.1,0.2)
		_EdgeCol ("_EdgeCol", Vector) = (1,1,1,0)
		_EdgeThreshold ("_EdgeThreshold", Range(0, 2)) = 0.6330423
		_EdgeSmooth ("_EdgeSmooth", Range(0.01, 1)) = 0.01
		_EdgeNoiseInfluence ("_EdgeNoiseInfluence", Range(0, 2)) = 2
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		_RimColor ("Rim Color", Vector) = (0,0,0,0)
		_RimMin ("Rim Min", Range(0, 2)) = 0.5
		_RimMax ("Rim Max", Range(0, 2)) = 1
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
	Fallback "False"
	//CustomEditor "ASEMaterialInspector"
}