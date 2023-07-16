Shader "Custom/RadialFillBillboard" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Progress ("Progress Bar Value", Range(0, 1)) = 1
		[NoScaleOffset] _GradientTex ("Gradient", 2D) = "white" {}
		_FillColor ("Fill Color", Vector) = (1,1,1,1)
		_BackColor ("Back Color", Vector) = (0,0,0,1)
		_PlayerDistanceFade ("Player Distance Fade", Float) = 1
		_FalloffMultiplier ("Falloff Multiplier", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}