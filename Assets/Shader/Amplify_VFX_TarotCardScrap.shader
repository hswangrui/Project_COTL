Shader "Amplify/VFX/TarotCardScrap" {
	Properties {
		_MainTex ("MainTex", 2D) = "white" {}
		[HDR] _MainColor ("MainColor", Vector) = (0,0,0,0)
		[Header(___FLIPBOOK___)] _ScrapsFlipbook ("ScrapsFlipbook", 2D) = "white" {}
		_StartFrame ("StartFrame", Float) = 0
		[IntRange] _FlipbookRows ("FlipbookRows", Range(1, 10)) = 1
		[IntRange] _FlipbookColumns ("FlipbookColumns", Range(1, 10)) = 1
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
	//CustomEditor "ASEMaterialInspector"
}