Shader "Skeleton_ASE_v1" {
	Properties {
		_PlayerInFrontOfObjects_Color ("PlayerInFrontOfObjects_Color", Vector) = (1,0,0,0.5333334)
		_FillColor ("FillColor", Vector) = (0,0,0,0)
		_FillAlpha ("_FillAlpha", Range(0, 1)) = 0
		_ClipThreshold ("ClipThreshold", Range(0, 1)) = 0
		_MainTex ("MainTex", 2D) = "white" {}
		_Tint ("Tint", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "Queue"="Transparent-3" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass {
			Name "Normal"

		CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

		fixed4 _Tint;
		sampler2D _MainTex;
		float _FillAlpha;
		fixed _ClipThreshold;
		fixed4 _FillColor;
		fixed4 _PlayerInFrontOfObjects_Color;

		struct VertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.vertexColor = v.vertexColor;
				return o;
			}

		float4 frag (VertexOutput i) : SV_Target 
		{
				float4 texColor = tex2D(_MainTex, i.uv);

				fixed4 fillColor = _FillColor * _FillAlpha;
				texColor.rgb=lerp(texColor.rgb, fillColor.rgb, fillColor.a);

				
				#if defined(_STRAIGHT_ALPHA_INPUT)
				texColor.rgb *= texColor.a;
				#endif

				texColor.rgb *= _Tint.rgb;
			//	texColor.a *= _Tint.a;

				if (texColor.a < _ClipThreshold) {
					discard;
				}

				return texColor;//* _PlayerInFrontOfObjects_Color;
			}
			
		
		ENDCG
	}}
	//CustomEditor "ASEMaterialInspector"
}