Shader "VolumetricLighting" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[HDR] _ColorOfLight ("ColorOfLight", Vector) = (1,0,0,1)
		_TransitionDistance ("TransitionDistance", Float) = 0.08
		_FallOff ("Fall Off", Float) = 0
		_Bias ("Bias", Float) = 0
		_Scale ("Scale", Float) = 2
		_Power ("Power", Float) = 5
		_Texture0 ("Texture 0", 2D) = "white" {}
		_NoiseIntensity ("NoiseIntensity", Float) = 0
		_AlphaOverride ("AlphaOverride", Range(0, 1)) = 0
		[Toggle] _UseNoise ("UseNoise?", Float) = 1
		_DistanceEnd1 ("DistanceEnd", Float) = 1
		_DistanceStart1 ("DistanceStart", Float) = 0
		[Toggle] _UseDistanceTint2 ("UseDistanceTint?", Float) = 1
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