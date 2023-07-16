Shader "COTL/FX_Trail_A" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_MaskTex ("Mask", 2D) = "black" {}
		[NoScaleOffset] _NoiseTex ("Noise", 2D) = "black" {}
		_NoiseInfluence ("Noise Influence", Range(0, 2)) = 1
		_VelocityOffset ("Scale.xy Offset.zw", Vector) = (1,1,0,0)
		[Toggle(MULTIPLY_ON)] _isMultiplicative ("Multiplicative Noise", Float) = 0
		[Header(Scroll)] _NoiseScroll ("Noise - Scroll", Vector) = (0,0,0,0)
		[HDR] _InnerColor ("Color", Vector) = (0,0,0,0)
		_GlowRange ("GlowRange - Smooth", Range(0, 1)) = 0.5
		[Header(Soft Particle)] [Toggle(_USESOFTPARTICLE)] _isSoftParticle ("Soft Particle", Float) = 0
		_InvFade ("Soft Particles Factor", Range(0.01, 5)) = 1
		[Header(Blending)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
		[Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 1
		[Separator] [Header(Stencil)] _StencilRef ("Stencil Ref", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Pass", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Float) = 0
		[Header(Shadows)] [Toggle(_CASTSHADOW)] _CastShadow ("Cast Shadow", Float) = 1
		_offsetX ("Offset X", Float) = 0
		_offsetY ("Offset Y", Float) = 0
		[Separator] [HideInInspector] _dummy ("DummyValue", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
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