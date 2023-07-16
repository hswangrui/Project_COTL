Shader "COTL/StencilLighting_Volumetric" {
	Properties {
		[NoScaleOffset] _Ramp ("Base (RGB) Trans (A)", 2D) = "white" {}
		[HideInInspector] [HDR] _Color ("Color", Vector) = (0,0,0,0)
		[HideInInspector] _StencilInfluence ("_StencilInfluence", Range(0, 2)) = 1
		_Noise ("Base (RGB) Trans (A)", 2D) = "black" {}
		_NoiseScale ("Noise Scale", Range(0, 3)) = 1
		[Toggle(ADDITIVE_ON)] _AdditiveOn ("Additive On?", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Transparent" "Queue"="Transparent-5" "IgnoreProjector"="True" }

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		CGPROGRAM

#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
}