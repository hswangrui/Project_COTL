Shader "LogoDesaturate_ScreenSpace" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_Size ("Size", Vector) = (1,1,0,0)
		_Speed1 ("Speed", Float) = 5
		_Size1 ("Size", Float) = 1
		[Toggle] _DoodleUV_UseUnscaledTime ("DoodleUV_UseUnscaledTime", Float) = 0
		_NoiseSpeed ("NoiseSpeed", Float) = 0
		[Toggle] _UseUnscaledTime ("UseUnscaledTime", Float) = 1
		_NoiseScale ("NoiseScale", Range(0, 20)) = 1.300236
		[HDR] _Color0 ("Color 0", Color) = (1,0,0,0)
		_SmoothStepMin ("SmoothStepMin", Float) = 0
		[Toggle] _UseGrayscale ("UseGrayscale", Float) = 1
		_SmoothStepMax ("SmoothStepMax", Float) = 0
		_UnderColor ("UnderColor", Color) = (1,1,1,0)
		_PowerExp ("PowerExp", Float) = 23
		_MoveUV ("MoveUV", Float) = 0.5
		[Toggle] _UseRainbow ("UseRainbow", Float) = 1
		_RainbowLerp ("RainbowLerp", Range(0, 1)) = 0
	}

	SubShader {
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		Cull Off
		Stencil {
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{

		CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _ _CANVAS_GROUP_COMPATIBLE
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

				struct VertexInput {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput {
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};



		sampler2D _MainTex;
		fixed4 _Color;
		float _Speed1;
		fixed4 _Size;
		float _Size1;
		float _NoiseSpeed;
		float _NoiseScale;
		float _SmoothStepMin;
		float _SmoothStepMax;
		float _PowerExp;
		float _MoveUV;
		float4 _Color0;
		float4 _UnderColor;
		float _RainbowLerp;



		half _UseUnscaledTime;
		half _UseGrayscale;
		half _DoodleUV_UseUnscaledTime;
		half _UseRainbow;

		VertexOutput vert (VertexInput IN) {
				VertexOutput OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;

				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
				#endif

				OUT.color = IN.color * float4(_Color.rgb * _Color.a, _Color.a); // Combine a PMA version of _Color with vertexColor.
				return OUT;
			}

		// 着色函数
		fixed4 frag (VertexOutput IN) : SV_Target
		{
			float2 uv = IN.texcoord.xy;
			fixed4 c = tex2D(_MainTex, uv);

			#if defined(_STRAIGHT_ALPHA_INPUT)
			c.rgb *= c.a;

			
			#endif

			// 灰度化
			if (_UseGrayscale > 0)
			{
				fixed gray = dot(c.rgb, float3(0.299, 0.587, 0.114));
				c.rgb = lerp(c.rgb, gray, _PowerExp);
			}

			// 彩虹效果
			if (_UseRainbow > 0)
			{
				float4 rainbowColor = lerp(_Color0, _UnderColor, _RainbowLerp);
				c.rgb = lerp(c.rgb, rainbowColor.rgb, _PowerExp);
			}

			// 尺寸调整
			uv -= 0.5;
			uv *= _Size.xy;
			uv += 0.5;

			// 移动UV
			uv.x += sin(uv.y *(_Speed1 + _NoiseSpeed) * (_UseUnscaledTime > 0 ? _Time.y : _Time.y * _DoodleUV_UseUnscaledTime));
				// 噪声
		float noise = 0;
		if (_NoiseScale > 0)
		{
			float2 noiseUV = uv * _NoiseScale;
			noise += sin(noiseUV.x + noiseUV.y) * 0.5 + 0.5;
		}

		// 平滑步进调整
		float smoothStepValue = smoothstep(_SmoothStepMin, _SmoothStepMax, noise);
		uv += (smoothStepValue - 0.5) * _MoveUV;

		// 应用调整后的UV
	//	c += tex2D(_MainTex, uv);

		// 应用颜色和透明度
		c.rgb *= _Color.rgb;
		c.a *= _Color.a;

		#ifdef UNITY_UI_ALPHACLIP
				clip (c.a - 0.001);
		#endif

		return c;

	}
	ENDCG
	}
}
}
//Fallback "UI/Default"