
Shader "Hidden/BlendModes/UnlitTransparent/UnifiedGrabMasked" {
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Color ("Tint Color", Color) = (1,1,1,1)
    _BLENDMODES_StencilId ("Stencil ID", Float) = 0
        _BLENDMODES_BlendStencilComp ("Blend Stencil Comparison", Float) = 0
        _BLENDMODES_NormalStencilComp ("Normal Stencil Comparison", Float) = 1
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
   LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

   GrabPass { "_BLENDMODES_UnifiedGrabTexture" }

    Pass {

       Stencil { Ref [_BLENDMODES_StencilId] Comp [_BLENDMODES_BlendStencilComp] }

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
           
            #pragma multi_compile BLENDMODES_MODE_DARKEN BLENDMODES_MODE_MULTIPLY BLENDMODES_MODE_COLORBURN BLENDMODES_MODE_LINEARBURN BLENDMODES_MODE_DARKERCOLOR BLENDMODES_MODE_LIGHTEN BLENDMODES_MODE_SCREEN BLENDMODES_MODE_COLORDODGE BLENDMODES_MODE_LINEARDODGE BLENDMODES_MODE_LIGHTERCOLOR BLENDMODES_MODE_OVERLAY BLENDMODES_MODE_SOFTLIGHT BLENDMODES_MODE_HARDLIGHT BLENDMODES_MODE_VIVIDLIGHT BLENDMODES_MODE_LINEARLIGHT BLENDMODES_MODE_PINLIGHT BLENDMODES_MODE_HARDMIX BLENDMODES_MODE_DIFFERENCE BLENDMODES_MODE_EXCLUSION BLENDMODES_MODE_SUBTRACT BLENDMODES_MODE_DIVIDE BLENDMODES_MODE_HUE BLENDMODES_MODE_SATURATION BLENDMODES_MODE_COLOR BLENDMODES_MODE_LUMINOSITY

            #include "UnityCG.cginc"
           #include "../../BlendModesCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
           };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
               BLENDMODES_GRAB_POSITION(2)
                UNITY_VERTEX_OUTPUT_STEREO
           };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            BLENDMODES_UNIFIED_GRAB_TEXTURE_SAMPLER

           v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
               o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                BLENDMODES_COMPUTE_GRAB_POSITION(o, o.vertex)
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                BLENDMODES_BLEND_PIXEL_UNIFIED_GRAB(col, i)
                UNITY_APPLY_FOG(i.fogCoord, col);
                
               return col;
            }
        ENDCG
    }
   Pass {

        Stencil { Ref [_BLENDMODES_StencilId] Comp [_BLENDMODES_NormalStencilComp] }

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
           
            

            #include "UnityCG.cginc"
           #include "../../BlendModesCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
           };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
               
                UNITY_VERTEX_OUTPUT_STEREO
            };

           sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            

            v2f vert (appdata_t v)
            {
               v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
               UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                UNITY_TRANSFER_FOG(o,o.vertex);
               return o;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
       ENDCG
    }
}

    Fallback "Unlit/Transparent"
}
