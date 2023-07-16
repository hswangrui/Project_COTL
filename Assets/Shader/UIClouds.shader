Shader "UIClouds" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_ParticleTexture ("ParticleTexture", 2D) = "white" {}
		_Speed1 ("Speed", Float) = 5
		_Size1 ("Size", Float) = 1
		[Toggle] _DoodleUV_UseUnscaledTime ("DoodleUV_UseUnscaledTime", Float) = 0
		_OverlayTexture ("OverlayTexture", 2D) = "white" {}
		_AlphaCutOffMin ("AlphaCutOffMin", Float) = 0
		_Noise ("Noise", 2D) = "white" {}
		_DistanceFadeEnd ("DistanceFadeEnd", Float) = 1
		[Header(Distance Fade)] _DistanceFadeStart ("DistanceFadeStart", Float) = 0
		_NoiseScale ("NoiseScale", Range(0, 1)) = 0
		_SpeedMultiplier ("SpeedMultiplier", Float) = 0
		_NoiseGenScale ("NoiseGenScale", Float) = 0
		[Toggle] _ClipBelowGround ("ClipBelowGround", Float) = 1
		_ClipThreshold ("ClipThreshold", Float) = 0
		_TextureScale ("TextureScale", Range(0, 1)) = 0
		[HDR] _MainColor ("MainColor", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
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