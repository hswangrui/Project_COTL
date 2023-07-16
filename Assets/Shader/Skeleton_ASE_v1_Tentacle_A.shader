Shader "Skeleton_ASE_v1_Tentacle_A" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_MainTex ("MainTex", 2D) = "white" {}
		_NoiseATex ("Noise A Tex", 2D) = "black" {}
		[HDR] _GlowCol ("GlowColor", Vector) = (1,0,0,1)
		[Separator] [Header(Point Light Shading)] [Toggle(_IGNORESPRITEFACING)] _IgnoreSpriteFacing ("Ignore Sprite Facing?", Float) = 0
		[Separator] [Header(World Space Clipping)] [Toggle(_CLIPBELOWGROUND)] _ClipBelowGround ("Clip Below Ground", Float) = 0
		[Separator] [Header(Alpha Clipping)] _Cutoff ("Cutoff", Range(0.001, 1)) = 0.1
		_CutoffSmooth ("Cutoff softness", Range(0.001, 1)) = 0.1
		[Separator] [Header(Blending)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 10
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