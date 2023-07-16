Shader "Custom/RadialFill" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[HDR] _Color ("Tint", Vector) = (1,1,1,1)
		_Angle ("Angle", Range(0, 360)) = 0
		_Arc1 ("Arc Point 1", Range(0, 360)) = 15
		_Arc2 ("Arc Point 2", Range(0, 360)) = 15
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		_Speed1 ("Speed", Float) = 5
		_Size1 ("Size", Float) = 1
		[Toggle] _DoodleUV_UseUnscaledTime ("DoodleUV_UseUnscaledTime", Float) = 0
		[HDR] _GlowColor ("GlowColor", Vector) = (0,0,0,0)
		_FalloffMultiplier ("FalloffMultiplier", Float) = 1
		[Toggle] _FadeDistance ("FadeDistance", Float) = 1
		_PlayerDistanceFade ("PlayerDistanceFade", Float) = 0
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