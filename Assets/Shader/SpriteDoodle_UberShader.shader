Shader "SpriteDoodle_UberShader" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		_Speed ("Speed", Float) = 5
		_Size ("Size", Float) = 1
		_GrayscaleLerpFade ("GrayscaleLerpFade", Range(0, 1)) = 0
		[Toggle] _IsGrayscale ("IsGrayscale?", Float) = 0
		[HDR] _FillColor ("FillColor", Vector) = (0.9921569,0.1137255,0.01176471,1)
		_ShinyScale ("ShinyScale", Float) = 10
		_ShineSpeed ("ShineSpeed", Float) = 8
		_FillColorLerpFade ("FillColorLerpFade", Range(0, 1)) = 0
		_MaxShine ("MaxShine", Float) = 0.8
		_Texture0 ("Texture 0", 2D) = "white" {}
		[Toggle] _AddShine ("AddShine?", Float) = 1
		_EmissionMap ("EmissionMap", 2D) = "white" {}
		[HDR] _GlowColour ("Glow Colour", Vector) = (1,0,0,0)
		[Toggle] _HasEmissionMap ("HasEmissionMap?", Float) = 0
		_Rotation ("Rotation", Range(-360, 360)) = 0
		_PosX ("PosX", Range(-2, 2)) = 0
		_PosY ("PosY", Range(-2, 2)) = 0
		_RotationSpeed ("RotationSpeed", Float) = 0
		[Toggle] _DOODLE_UV ("DOODLE_UV", Float) = 0
		_Texture1 ("Texture 1", 2D) = "white" {}
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