Shader "COTL/FX_FauxFluid_A_Lit" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Particle", 2D) = "black" {}
		[Header(Normal)] [NoScaleOffset] _BumpMap ("Normal", 2D) = "bump" {}
		_NormalScale ("Normal - Scale", Float) = 1
		_NormalAOffset ("Normal A Scale (XY) Normal Scroll (ZW)", Vector) = (1,1,0,0)
		_NormalBOffset ("Normal B Scale (XY) Normal Scroll (ZW)", Vector) = (1,1,0,0)
		[Separator] [Header(Faux Fluid)] _EdgeDistance ("Edge Distance", Range(0.01, 0.5)) = 0.1
		_GlowDistance ("Glow Distance", Range(0.01, 0.5)) = 0.1
		[HDR] _Glow ("Color", Vector) = (1,0,0,1)
		[Separator] _Cutoff ("CutOff", Range(0, 1)) = 0.5
		_Clip ("CutOff", Range(0, 1)) = 0.5
		_ClipSmooth ("CutOff", Range(0, 1)) = 0.5
		[Separator] [Header(Lighting)] _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.5
		_RampSmoothness ("Ramp Smoothness", Range(0.01, 1)) = 0.01
		_SColor ("Shadow Color", Vector) = (0.195,0.195,0.195,1)
		[Separator] [Header(Specular)] [HDR] _SpecColor ("Spec Color", Vector) = (0.195,0.195,0.195,1)
		_Specular ("Spec Size", Range(0, 1)) = 0.5
		_SpecClamp ("Spec Smoothness", Range(0.01, 1)) = 0.1
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}