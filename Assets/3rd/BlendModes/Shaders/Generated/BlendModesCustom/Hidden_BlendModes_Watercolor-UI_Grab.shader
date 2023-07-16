Shader "Hidden/BlendModes/Watercolor-UI/Grab" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_BlotchMultiply ("BlotchMultiply", Range(0, 8)) = 4.003983
		_BlotchSubtract ("BlotchSubtract", Range(0, 8)) = 2
		_Texture1 ("Texture 1", 2D) = "white" {}
		_MovementSpeed ("MovementSpeed", Float) = 1
		_MovementDirection ("MovementDirection", Vector) = (0,1,0,0)
		_CloudDensity ("CloudDensity", Float) = 1
		_Texture0 ("Texture 0", 2D) = "white" {}
		_TilingUV ("TilingUV", Vector) = (1,1,0,0)
		_Rotation ("Rotation", Range(-360, 360)) = 0
		_PosX ("PosX", Range(-2, 2)) = 0
		_PosY ("PosY", Range(-2, 2)) = 0
		_PowerExp ("PowerExp", Float) = 0
		[Toggle] _MaskMainTex ("MaskMainTex", Float) = 0
		[HDR] _TintCOlor ("TintCOlor", Vector) = (1,1,1,1)
		_PositionOffset ("PositionOffset", Vector) = (0,0,0,0)
		[Toggle] _MultiplyTextureAlpha ("MultiplyTextureAlpha", Float) = 1
		_Mask ("Mask", 2D) = "white" {}
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