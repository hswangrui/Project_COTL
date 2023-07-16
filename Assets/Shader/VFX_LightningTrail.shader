Shader "VFX/LightningTrail" {
	Properties {
		[Toggle(_FLIPDIRECTION_ON)] _FlipDirection ("FlipDirection", Float) = 0
		_Texture0 ("Texture 0", 2D) = "white" {}
		[HDR] _LightningInnerColor ("LightningInnerColor", Vector) = (0,0,0,0)
		[HDR] _LightningOuterColor ("LightningOuterColor", Vector) = (0,0,0,0)
		_RevealPower ("RevealPower", Float) = 0
		[Toggle(_PARTICLEREVEALMODE_ON)] _ParticleRevealMode ("ParticleRevealMode", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	//CustomEditor "ASEMaterialInspector"
}