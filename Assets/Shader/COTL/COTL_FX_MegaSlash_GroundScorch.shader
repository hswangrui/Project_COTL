Shader "COTL/FX_MegaSlash_GroundScorch" {
	Properties {
		[Header(Main Texture)] _MainTex ("Main Texure", 2D) = "white" {}
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		[Separator] [Header(Noise)] _NoiseInvScale ("Uniform Scale", Vector) = (1,1,0,0)
		[Space(20)] _NoiseA ("Noise Texure A", 2D) = "white" {}
		_NoiseAInf ("Influence", Range(0, 1)) = 1
		[Space(20)] _NoiseB ("Noise Texure B", 2D) = "white" {}
		_NoiseBInf ("Influence", Range(0, 1)) = 1
		[Space(20)] [Separator] [Header(Displacement)] [Toggle(DISPLACE)] _Displace ("Displacement", Float) = 0
		[PowerSlider(3.0)] _DisplaceAmount ("Displace Amount", Range(0.001, 0.5)) = 0
		_DisplaceMap ("Displace Map (XY)", 2D) = "grey" {}
		[Toggle(POLAR_DISP)] _POLAR_DISP ("Polar UV", Float) = 0
		[Separator] [Header(Color Banding)] [Toggle(BANDING)] _Banding ("Color banding", Float) = 0
		_Bands ("Number of bands", Float) = 3
		[Separator] [Header(Soft Particle)] [Toggle(_USESOFTPARTICLE)] _isSoftParticle ("Soft Particle", Float) = 0
		_InvFade ("Soft Particles Factor", Range(0.01, 5)) = 1
		[Separator] [Header(Alpha Clipping)] _Cutoff ("Cutoff", Range(0.001, 1)) = 0.1
		_CutoffSmooth ("Cutoff softness", Range(0.001, 1)) = 0.1
		[Separator] [Header(Spread Clipping)] _SpreadThreshold ("Spread", Range(0, 1)) = 0.5
		_SpreadSmooth ("Spread softness", Range(0.001, 1)) = 0.1
		[Separator] [Header(Scorch)] [HDR] _BurnCol ("Burn Color", Vector) = (1,1,1,1)
		_BurnPos ("Burn Pos", Range(0, 1)) = 0.1
		_BurnWidth ("Burn Width", Range(0, 1)) = 0.1
		_ScorchPos ("Scorch Pos", Range(0, 1)) = 0.1
		_ScorchTail ("Scorch Tail", Range(0, 1)) = 0.1
		[Header(Blending)] [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Blend Operation", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
		[Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 0
		[Header(Stencil)] _StencilRef ("Stencil Ref", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Pass", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Fail", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("ZFail", Float) = 0
		[Separator] [Header(Worldspace Clip)] [Toggle(WORLDZCLIP)] _WorldZClip ("WorldSpace Z Clip", Float) = 0
		[HideInInspector] _dummy ("DummyValue", Float) = 0
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