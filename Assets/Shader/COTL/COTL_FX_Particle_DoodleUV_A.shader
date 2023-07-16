Shader "COTL/FX_Particle_DoodleUV_A" {
	Properties {
		[Header(Core)] [HDR] _Color ("Color", Vector) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Particle", 2D) = "black" {}
		[Separator] [Header(Alpha)] _Cutoff ("CutOff", Range(0, 1)) = 0.5
		_CutoffSmooth ("CutOff - Smooth", Range(0.01, 1)) = 0.5
		[Separator] [Header(Soft Particle)] [Toggle(_USESOFTPARTICLE)] _isSoftParticle ("Soft Particle", Float) = 0
		_InvFade ("Soft Particles Factor", Range(0.01, 5)) = 1
		[Separator] [Header(DoodleUV)] [Toggle(UNSCALED_TIME)] _isUnscaledTime ("Is Unscaled Time?", Float) = 0
		_DoodleSpeed ("Speed", Float) = 5
		_DoodleSize ("Size", Float) = 1
		[Separator] [Header(Particle Data)] [Toggle(VCOL_DATA)] _isColData ("Vertex Color is Data", Float) = 0
		[Separator] [Header(Blending)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 10
		[Toggle(_ISADDITIVE)] _isAdditive ("Is Additive?", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
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