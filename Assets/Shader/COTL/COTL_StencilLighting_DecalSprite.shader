Shader "COTL/StencilLighting_DecalSprite" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		[HideInInspector] _Influence ("Influence Default", Float) = 1
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_ProjectionScale ("Projection Scale", Range(0, 2)) = 1.27
		[Toggle(DOODLEUV_ON)] _DoodleUVOn ("DoodleUV On", Float) = 0
		[HideInInspector] [HDR] _ColorInstanceOverride ("OverrideColor", Vector) = (1,1,1,0)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "Queue"="Transparent"  "RenderType"="Transparent-5"}
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
		CGPROGRAM
			 #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
			#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _CameraDepthTexture;
		fixed4 _Color;
		fixed4 _ColorInstanceOverride;
		fixed _ProjectionScale;

		struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			struct v2f {
				float2 uv : TEXCOORD0;
				  UNITY_FOG_COORDS(1)
				  float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				 UNITY_VERTEX_OUTPUT_STEREO
			};
		
			v2f vert (appdata v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.screenPos = ComputeScreenPos(o.vertex);
			
				UNITY_TRANSFER_FOG(o,o.vertex);

				
				return o;
			}



		half4 frag (v2f i) : SV_Target 
		{
				float4 divW = i.screenPos / i.screenPos.w;

				float4 ndcPos = divW * 2 - 1;
				//将屏幕像素对应在摄像机远平面的点转换到剪裁空间，也是相机(0,0,0)指向该点的向量
				float far = _ProjectionParams.z;
				float3 farClipVec = float3(ndcPos.xy, 1) * far;
				//通过逆投影矩阵将向量转换到观察空间
				float3 viewVec = mul(unity_CameraInvProjection, farClipVec.xyzz).xyz;
				//将向量乘以线性深度值，得到在深度缓冲中储存的值在观察空间的位置
				float2 screenUV = divW.xy;
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
				float3 viewPos = viewVec * Linear01Depth(depth);
				//观察空间变换到世界空间
				float4 worldPos = mul(UNITY_MATRIX_I_V, float4(viewPos, 1.0));

				float4 objectPos = mul(unity_WorldToObject, worldPos);
				clip(float3(0.5, 0.5, 0.5) - abs(objectPos));
				float2 uv = objectPos.xz + 0.5;
				uv/=_ProjectionScale;
				fixed4 finalColor = tex2D(_MainTex, uv)*_Color*_ColorInstanceOverride;


				return finalColor;

		}
		ENDCG
	}
}
}