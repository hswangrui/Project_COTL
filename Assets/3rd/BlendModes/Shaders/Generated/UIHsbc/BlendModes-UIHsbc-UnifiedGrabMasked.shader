
Shader "Hidden/BlendModes/UIHsbc/UnifiedGrabMasked"
{
   Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

       _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

       _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _Hue("Hue", Range(0.0, 1.0)) = 0
        _Saturation("Saturation", Range(-1.0, 1.0)) = 0
        _Brightness("Brightness", Range(-1.0, 1.0)) = 0
        _Contrast("Contrast", Range(-1.0, 1.0)) = 0

        _BLENDMODES_StencilId ("Stencil ID", Float) = 0
        _BLENDMODES_BlendStencilComp ("Blend Stencil Comparison", Float) = 0
        _BLENDMODES_NormalStencilComp ("Normal Stencil Comparison", Float) = 1
    }

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
        ZTest [unity_GUIZTestMode]
       Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

       GrabPass { "_BLENDMODES_UnifiedGrabTexture" }

        Pass
       {
            Name "Default"

            Stencil { Ref [_BLENDMODES_StencilId] Comp [_BLENDMODES_BlendStencilComp] }

        CGPROGRAM
           #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            

            #include "UnityCG.cginc"
           #include "UnityUI.cginc"
            #include "../../BlendModesCG.cginc"

           #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #pragma multi_compile BLENDMODES_MODE_DARKEN BLENDMODES_MODE_MULTIPLY BLENDMODES_MODE_COLORBURN BLENDMODES_MODE_LINEARBURN BLENDMODES_MODE_DARKERCOLOR BLENDMODES_MODE_LIGHTEN BLENDMODES_MODE_SCREEN BLENDMODES_MODE_COLORDODGE BLENDMODES_MODE_LINEARDODGE BLENDMODES_MODE_LIGHTERCOLOR BLENDMODES_MODE_OVERLAY BLENDMODES_MODE_SOFTLIGHT BLENDMODES_MODE_HARDLIGHT BLENDMODES_MODE_VIVIDLIGHT BLENDMODES_MODE_LINEARLIGHT BLENDMODES_MODE_PINLIGHT BLENDMODES_MODE_HARDMIX BLENDMODES_MODE_DIFFERENCE BLENDMODES_MODE_EXCLUSION BLENDMODES_MODE_SUBTRACT BLENDMODES_MODE_DIVIDE BLENDMODES_MODE_HUE BLENDMODES_MODE_SATURATION BLENDMODES_MODE_COLOR BLENDMODES_MODE_LUMINOSITY

            struct appdata_t
            {
                float4 vertex   : POSITION;
               float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
           };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
               BLENDMODES_GRAB_POSITION(2)
                UNITY_VERTEX_OUTPUT_STEREO
           };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
           float4 _MainTex_ST;
            fixed _Hue, _Saturation, _Brightness, _Contrast;
            BLENDMODES_UNIFIED_GRAB_TEXTURE_SAMPLER

           v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
               OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                BLENDMODES_COMPUTE_GRAB_POSITION(OUT, OUT.vertex)

                OUT.color = v.color * _Color;
                return OUT;
            }

			
            fixed4 frag(v2f IN) : SV_Target
           {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                BLENDMODES_BLEND_PIXEL_UNIFIED_GRAB(color, IN)
                
                color.rgb = ApplyHsbc(color.rgb, fixed4(_Hue, _Saturation, _Brightness, _Contrast));

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
               clip (color.a - 0.001);
                #endif

				
	            return color;
            }
        ENDCG
        }
       Pass
        {
            Name "DefaultNormalBlend"

           Stencil { Ref [_BLENDMODES_StencilId] Comp [_BLENDMODES_NormalStencilComp] }

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "../../BlendModesCG.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
           #pragma multi_compile __ UNITY_UI_ALPHACLIP
            

           struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
           };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
               
                UNITY_VERTEX_OUTPUT_STEREO
            };

           sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            fixed _Hue, _Saturation, _Brightness, _Contrast;
           

            v2f vert(appdata_t v)
            {
               v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
               UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
               OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                

               OUT.color = v.color * _Color;
                return OUT;
           }

			
            fixed4 frag(v2f IN) : SV_Target
           {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                
               color.rgb = ApplyHsbc(color.rgb, fixed4(_Hue, _Saturation, _Brightness, _Contrast));

                #ifdef UNITY_UI_CLIP_RECT
               color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
               #endif

                #ifdef UNITY_UI_ALPHACLIP
               clip (color.a - 0.001);
                #endif

				
	            return color;
            }
        ENDCG
        }
   }

    Fallback "UI/Default"
}
