
Shader "Hidden/BlendModes/ParticlesHsbc/Overlay" {
    Properties {
        _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex ("Particle Texture", 2D) = "white" {}
        _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
        _Hue("Hue", Range(0.0, 1.0)) = 0
        _Saturation("Saturation", Range(-1.0, 1.0)) = 0
        _Brightness("Brightness", Range(-1.0, 1.0)) = 0
        _Contrast("Contrast", Range(-1.0, 1.0)) = 0
        _BLENDMODES_OverlayTexture("Overlay Texture", 2D) = "white" {}
        _BLENDMODES_OverlayColor("Overlay Color", Color) = (1,1,1,1)
   }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Off Lighting Off ZWrite Off

        SubShader {

            

            Pass {

                CGPROGRAM
                #pragma vertex vert
               #pragma fragment frag
                #pragma target 2.0
               #pragma multi_compile_particles
                #pragma multi_compile_fog
                
                #pragma multi_compile BLENDMODES_MODE_DARKEN BLENDMODES_MODE_MULTIPLY BLENDMODES_MODE_COLORBURN BLENDMODES_MODE_LINEARBURN BLENDMODES_MODE_DARKERCOLOR BLENDMODES_MODE_LIGHTEN BLENDMODES_MODE_SCREEN BLENDMODES_MODE_COLORDODGE BLENDMODES_MODE_LINEARDODGE BLENDMODES_MODE_LIGHTERCOLOR BLENDMODES_MODE_OVERLAY BLENDMODES_MODE_SOFTLIGHT BLENDMODES_MODE_HARDLIGHT BLENDMODES_MODE_VIVIDLIGHT BLENDMODES_MODE_LINEARLIGHT BLENDMODES_MODE_PINLIGHT BLENDMODES_MODE_HARDMIX BLENDMODES_MODE_DIFFERENCE BLENDMODES_MODE_EXCLUSION BLENDMODES_MODE_SUBTRACT BLENDMODES_MODE_DIVIDE BLENDMODES_MODE_HUE BLENDMODES_MODE_SATURATION BLENDMODES_MODE_COLOR BLENDMODES_MODE_LUMINOSITY

               #include "UnityCG.cginc"
                #include "../../BlendModesCG.cginc"

               sampler2D _MainTex;
                fixed4 _TintColor;
               BLENDMODES_OVERLAY_VARIABLES

                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    
                   UNITY_VERTEX_INPUT_INSTANCE_ID
                };

               struct v2f {
                    float4 vertex : SV_POSITION;
                   fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    #ifdef SOFTPARTICLES_ON
                    float4 projPos : TEXCOORD2;
                    #endif
                   BLENDMODES_OVERLAY_TEX_COORD(3)
                    UNITY_VERTEX_OUTPUT_STEREO
               };

                float4 _MainTex_ST;
                fixed _Hue, _Saturation, _Brightness, _Contrast;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    #ifdef SOFTPARTICLES_ON
                   o.projPos = ComputeScreenPos (o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                   #endif
                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                    BLENDMODES_TRANSFORM_OVERLAY_TEX(o.texcoord, o)
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
               float _InvFade;

                
                fixed4 frag (v2f i) : SV_Target
                {
                    #ifdef SOFTPARTICLES_ON
                    float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                    float partZ = i.projPos.z;
                   float fade = saturate (_InvFade * (sceneZ-partZ));
                   i.color.a *= fade;
                    #endif

                   fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
                    BLENDMODES_BLEND_PIXEL_OVERLAY(col, i)
                   col.rgb = ApplyHsbc(col.rgb, fixed4(_Hue, _Saturation, _Brightness, _Contrast));
                    UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode

                   
                    return col;
                }
               ENDCG
            }
        }
    }
    Fallback "Particles/Standard Unlit"
}
