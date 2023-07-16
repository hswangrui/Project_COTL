Shader "UberShader_RadialOcclusion" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[Header(Fill)] [Space(10)] _FillColor ("_FillColor", Vector) = (0,0,0,0)
		_FillAlpha ("_FillAlpha", Range(0, 1)) = 0
		[Header(Alpha Clip)] [Space(10)] _Cutoff ("ClipAlphaThreshold", Range(0, 1)) = 0.7
		[Header(Emission)] [Space(10)] [Toggle(_USEEMISSION_ON)] _UseEmission ("UseEmission", Float) = 0
		[HDR] _EmissionColor ("EmissionColor", Vector) = (1,0,0,0)
		_EmissionMap ("EmissionMap", 2D) = "white" {}
		_MaxEmission ("MaxEmission", Float) = 1.5
		[Header(Vertex Animation)] [Space(10)] [Toggle(_ANIMATEVERTEX_ON)] _AnimateVertex ("AnimateVertex", Float) = 0
		_WindTexture ("WindTexture", 2D) = "white" {}
		_VertexMask ("VertexMask", 2D) = "white" {}
		[Toggle] _Vertex_Offset_Y ("Vertex_Offset_Y", Float) = 1
		[Toggle] _Vertex_Offset_X ("Vertex_Offset_X", Float) = 1
		[Toggle] _Vertex_Flip_Mask ("Vertex_Flip_Mask", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[Header(Point Light Shading)] [Toggle(_IGNORESPRITEFACING)] _IgnoreSpriteFacing ("Ignore Sprite Facing?", Float) = 0
		[Separator] [Header(Fade Into Woods)] [Toggle(_FADEINTOWOODS_ON)] _FadeIntoWoods ("FadeIntoWoods", Float) = 0
		[Toggle(_FADEINTOFOG_ON)] _FadeIntoFog ("FadeIntoFog", Float) = 0
		[Separator] [Header(World Space Clipping)] [Toggle(_CLIPBELOWGROUND)] _ClipBelowGround ("Clip Below Ground", Float) = 0
		[Separator] [Header(CameraFade)] _CameraFadeDistance ("Camera Fade Distance", Range(0, 10)) = 5
		[Header(DepthFade)] [Space(10)] _DepthThreshold ("Depth Vignette - Threshold", Range(0, 1)) = 0.3
		_DepthSmoothness ("Depth Vignette - Smoothness", Range(0.01, 1)) = 0.2
		_RadialDistanceOffset ("Radial Distance Offset", Float) = 0.5
		[Space(10)] [HideInInspector] _dummy ("DummyValue", Float) = 0
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