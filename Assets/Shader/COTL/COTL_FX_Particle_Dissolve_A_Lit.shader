Shader "COTL/FX_Particle_Dissolve_A_Lit" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Particle", 2D) = "black" {}
		[NoScaleOffset] _BumpMap ("Normal", 2D) = "bump" {}
		_NormalScale ("Normal - Scale", Float) = 1
		_Cutoff ("CutOff", Range(0, 1)) = 0.5
		[Toggle(_ISSOFT)] _isSoft ("Is Soft Particle?", Float) = 1
		_InvFade ("Soft Particles Factor", Range(0.01, 5)) = 1
		[Header(Lighting)] _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.5
		_RampSmoothness ("Ramp Smoothness", Range(0.01, 1)) = 0.01
		_SColor ("Shadow Color", Vector) = (0.195,0.195,0.195,1)
		[Space(20)] [Header(Specular)] _SpecColor ("Spec Color", Vector) = (0.195,0.195,0.195,1)
		_Specular ("Spec Size", Range(0, 1)) = 0.5
		_SpecClamp ("Spec Smoothness", Range(0.01, 1)) = 0.1
		[Space(20)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 10
		[Toggle(_ISADDITIVE)] _isAdditive ("Is Additive?", Float) = 0
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