Shader "Skeleton_ASE_v1_Stencil" {
	Properties {
		_Color ("Tint", Vector) = (1,1,1,1)
		_Cutoff ("Base Alpha cutoff", Range(0, 0.99)) = 0.9
		[Header(Fill)] [Space(10)] _FillColor ("_FillColor", Vector) = (0,0,0,0)
		_FillAlpha ("_FillAlpha", Range(0, 1)) = 0
		[Header(Emission)] [Space(10)] _EmissionMap ("EmissionMap", 2D) = "white" {}
		[HDR] _GlowColour ("Glow Colour", Vector) = (1,0,0,0)
		_MaxEmission ("_MaxEmission", Float) = 1.1
		_MainTex ("MainTex", 2D) = "white" {}
		[False] [True] [Toggle] _UseEmission ("UseEmission", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[Header(World Space Clipping)] [Toggle(_CLIPBELOWGROUND)] _ClipBelowGround ("Clip Below Ground", Float) = 0
		[Separator] [Header(Point Light Shading)] [Toggle(_IGNORESPRITEFACING)] _IgnoreSpriteFacing ("Ignore Sprite Facing?", Float) = 0
		[Separator] [Header(Shadows)] [Toggle(_RECEIVESHADOW)] _ReceiveShadow ("Receive Shadow", Float) = 1
		[Toggle(_RECIEVELIGHTING)] _ReceiveLighting ("Receive Lighting", Float) = 1
		[Separator] [Header(Fade In Woods)] [Toggle(_FADEINTOFOG_ON)] _FadeIntoFog ("FadeIntoFog", Float) = 0
		[Toggle(_FADEINTOWOODS_ON)] _FadeIntoWoods ("FadeIntoWoods", Float) = 0
		[Toggle(_USEFADEINWOODSCOLOR)] _UseFadeInWoodsColor ("UseFadeInWoodsColor", Float) = 1
		_VerticalFog_ZOffset ("_VerticalFog_ZOffset", Float) = 0
		[HDR] _FadeInWoodsColorOverride ("FadeInWoodsColorOverride", Vector) = (1,1,1,1)
		[Separator] [Header(Stencil)] _StencilRef ("Stencil Ref", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 6
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
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
	//CustomEditor "ASEMaterialInspector"
}