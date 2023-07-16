Shader "Skeleton_ASE_v1_SoftAlphaTest" {
	Properties {
		_Color ("Tint", Vector) = (1,1,1,1)
		_Cutoff ("Base Alpha cutoff", Range(0, 0.99)) = 0.9
		[Header(Fill)] [Space(10)] _FillColor ("_FillColor", Vector) = (0,0,0,0)
		_FillAlpha ("_FillAlpha", Range(0, 1)) = 0
		_SparkleTexture ("SparkleTexture", 2D) = "white" {}
		[Header(Emission)] [Space(10)] _EmissionMap ("EmissionMap", 2D) = "white" {}
		[HDR] _GlowColour ("Glow Colour", Vector) = (1,0,0,0)
		[Toggle(_USEEMISSION_ON)] _UseEmission ("UseEmission", Float) = 0
		_MaxEmission ("_MaxEmission", Float) = 1.1
		_ShinyScale ("ShinyScale", Float) = 10
		_MainTex ("MainTex", 2D) = "white" {}
		_ShineSpeed ("ShineSpeed", Float) = 8
		_Texture0 ("Texture 0", 2D) = "white" {}
		[Toggle(_SPARKLE_ON)] _Sparkle ("Sparkle", Float) = 0
		[Toggle(_SHINY_ON)] _Shiny ("Shiny", Float) = 0
		[HDR] _SparkleColor ("SparkleColor", Vector) = (1,1,1,0)
		_SparkleIntensity ("SparkleIntensity", Float) = 1
		_SparkleOverlay ("SparkleOverlay", 2D) = "white" {}
		_SparkleScale ("SparkleScale", Float) = 0.3
		_SparkleOverlayScale ("SparkleOverlayScale", Float) = 0
		_ShinyRotation ("ShinyRotation", Float) = -0.54
		_LeaderEncounterColorBoost ("LeaderEncounterColorBoost", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[Header(World Space Clipping)] [Toggle(_CLIPBELOWGROUND)] _ClipBelowGround ("Clip Below Ground", Float) = 0
		[Separator] [Header(Point Light Shading)] [Toggle(_IGNORESPRITEFACING)] _IgnoreSpriteFacing ("Ignore Sprite Facing?", Float) = 0
		[Toggle(_FADEINTOWOODS_ON)] _FadeIntoWoods ("FadeIntoWoods", Float) = 0
		[Toggle(_USEFADEINWOODSCOLOR)] _UseFadeInWoodsColor ("UseFadeInWoodsColor", Float) = 1
		_FadeInWoodsColorOverride ("FadeInWoodsColorOverride", Vector) = (1,1,1,1)
		[Separator] [Header(Shadows)] [Toggle(_RECEIVESHADOW)] _ReceiveShadow ("Receive Shadow", Float) = 1
		[Separator] [HideInInspector] _dummy ("DummyValue", Float) = 0
	
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

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
			sampler2D _MainTex;
			sampler2D _SparkleTexture;
			sampler2D _EmissionMap;
			sampler2D _SparkleOverlay;

			float4 _Color;
			float _Cutoff;
			float4 _FillColor;
			float _FillAlpha;
			float4 _GlowColour;
			float _UseEmission;
			float _MaxEmission;
			float _ShinyScale;
			float _ShineSpeed;
			float _Sparkle;
			float _Shiny;
			float4 _SparkleColor;
			float _SparkleIntensity;
			float _SparkleScale;
			float _SparkleOverlayScale;
			float _ShinyRotation;
			float4 _LeaderEncounterColorBoost;
			float _ClipBelowGround;
			float _IgnoreSpriteFacing;
			float _FadeIntoWoods;
			float _UseFadeInWoodsColor;		
			float4 _FadeInWoodsColorOverride;
			float _ReceiveShadow;
			float dummy;

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

			float4 frag (VertexOutput i) : SV_Target {
				float4 texColor = tex2D(_MainTex, i.uv);

				#if defined(_STRAIGHT_ALPHA_INPUT)
				tex.rgb *= texColor.a;
				#endif

				float4 sparkleTexColor = tex2D(_SparkleTexture, i.uv);
				float4 emissionColor = tex2D(_EmissionMap, i.uv);
				float4 sparkleOverlayColor = tex2D(_SparkleOverlay, i.uv);

				float4 finalColor = texColor * i.vertexColor;

				finalColor *= _Color;

				if (_Sparkle > 0) {
					finalColor += sparkleTexColor * _SparkleColor * _SparkleIntensity;
				}

				if (_UseEmission > 0) {
					finalColor += emissionColor * _MaxEmission;
				}

				if (_Shiny > 0) {
					float2 rotatedUV = i.uv;
					rotatedUV -= 0.5;
					float c = cos(_ShinyRotation);
					float s = sin(_ShinyRotation);
					rotatedUV = float2(rotatedUV.x * c - rotatedUV.y * s, rotatedUV.x * s + rotatedUV.y * c);
					rotatedUV += 0.5;
					finalColor += tex2D(_MainTex, rotatedUV) * _ShinyScale;
				}

				if (_FillAlpha > 0) {
					finalColor = lerp(finalColor, _FillColor, _FillAlpha);
				}

				if (_SparkleOverlayScale > 0) {
					finalColor += sparkleOverlayColor * _SparkleOverlayScale;
				}

				return finalColor;
			}
			ENDCG
		}
	}
}
