Shader "Skeleton_ASE_v1_SoftAlphaTest_Sparkle" {
	Properties {
		_Color ("Tint", Vector) = (1,1,1,1)
		_Cutoff ("Base Alpha cutoff", Range(0, 0.99)) = 0.9
		[Header(Fill)] [Space(10)] _FillColor ("_FillColor", Vector) = (0,0,0,0)
		_SparkleTexture ("Sparkle Texture", 2D) = "white" {}
		_FillAlpha ("_FillAlpha", Range(0, 1)) = 0
		_SparkleNoiseTexture ("Sparkle Noise Texture", 2D) = "white" {}
		[Header(Emission)] [Space(10)] _EmissionMap ("EmissionMap", 2D) = "white" {}
		_Spark_Em_Mult ("Spark_Em_Mult", Float) = 1
		[HDR] _GlowColour ("Glow Colour", Vector) = (1,0,0,0)
		[HDR] _Spark_Em_Color ("Spark_Em_Color", Vector) = (1,1,1,0)
		[Toggle(_USEEMISSION_ON)] _UseEmission ("UseEmission", Float) = 0
		_Spark_Noise_Speed ("Spark_Noise_Speed", Vector) = (-0.2,-0.2,0,0)
		_MaxEmission ("_MaxEmission", Float) = 1.1
		_MainTex ("MainTex", 2D) = "white" {}
		_SparkleScale ("SparkleScale", Float) = 1
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[Header(World Space Clipping)] [Toggle(_CLIPBELOWGROUND)] _ClipBelowGround ("Clip Below Ground", Float) = 0
		[Separator] [Header(Point Light Shading)] [Toggle(_IGNORESPRITEFACING)] _IgnoreSpriteFacing ("Ignore Sprite Facing?", Float) = 0
		[Separator] [Header(Shadows)] [Toggle(_RECEIVESHADOW)] _ReceiveShadow ("Receive Shadow", Float) = 1
		[Separator] [HideInInspector] _dummy ("DummyValue", Float) = 0
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