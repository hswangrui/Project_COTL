// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VFX/BloodParticleShader" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		_CloudDensity_0 ("CloudDensity_0", Float) = 0
		_CloudDensity_1 ("CloudDensity_1", Float) = 1
		_Texture2 ("Texture 2", 2D) = "white" {}
		_NoiseScale ("NoiseScale", Float) = 0
		_NoiseOffset ("NoiseOffset", Vector) = (0,0,0,0)
		_Texture4 ("Texture 4", 2D) = "white" {}
		[Toggle] _UseMask ("UseMask", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off         
		Lighting Off     
		ZWrite Off       
		Blend One OneMinusSrcAlpha   

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON    
			#pragma shader_feature ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
			
			struct appdata_t                          
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f                                
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;


			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;

			float _CloudDensity_0;
			float _CloudDensity_1;
			sampler2D _Texture2;
			float _NoiseScale;
			float4 _NoiseOffset;
			sampler2D _Texture4;
			float _UseMask;



			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA        
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif 

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed2 noiseUV = IN.texcoord * _NoiseScale + _Time * _NoiseOffset.xy;

				fixed cloud = tex2D(_Texture2, noiseUV).r;

				
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;

				c.rgb *= c.a;

				fixed mask= tex2D(_Texture4,IN.texcoord).r;

				c.a *= mask * cloud * _CloudDensity_0;
				return c;
			}
		ENDCG
		}
	}
}