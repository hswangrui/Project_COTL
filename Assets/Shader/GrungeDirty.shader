Shader "GrungeDirty" {
	Properties {
		_Texture0 ("Texture 0", 2D) = "white" {}
		_EmissionMap ("EmissionMap", 2D) = "white" {}
		_Scale ("Scale", Range(0, 2)) = 0
		_Cutoff ("Cutoff", Range(0, 1)) = 0.7
		_MainTex ("MainTex", 2D) = "white" {}
		_Rotation ("Rotation", Range(0, 2)) = 0
		_Tint ("Tint", Vector) = (0,0,0,0)
		_Smoothness ("_Smoothness", Range(0, 1)) = 0
		_SpecColor ("_SpecColor", Vector) = (0,0,0,0)
		[HDR] _EmissionColor ("EmissionColor", Vector) = (0,0,0,0)
		[Toggle] _IgnoreSpriteFacing ("Ignore Sprite Facing", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		_RimColor ("Rim Color", Vector) = (0,0,0,0)
		_RimMin ("Rim Min", Range(0, 2)) = 0.5
		_RimMax ("Rim Max", Range(0, 2)) = 1
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