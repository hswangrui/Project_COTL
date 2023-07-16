

Shader "Hidden/BlendModes/DiffuseTransparent/UnifiedGrab" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    
}

SubShader {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    LOD 200

GrabPass { "_BLENDMODES_UnifiedGrabTexture" }

CGPROGRAM
#pragma surface surf Lambert vertex:vert alpha:fade
#pragma multi_compile BLENDMODES_MODE_DARKEN BLENDMODES_MODE_MULTIPLY BLENDMODES_MODE_COLORBURN BLENDMODES_MODE_LINEARBURN BLENDMODES_MODE_DARKERCOLOR BLENDMODES_MODE_LIGHTEN BLENDMODES_MODE_SCREEN BLENDMODES_MODE_COLORDODGE BLENDMODES_MODE_LINEARDODGE BLENDMODES_MODE_LIGHTERCOLOR BLENDMODES_MODE_OVERLAY BLENDMODES_MODE_SOFTLIGHT BLENDMODES_MODE_HARDLIGHT BLENDMODES_MODE_VIVIDLIGHT BLENDMODES_MODE_LINEARLIGHT BLENDMODES_MODE_PINLIGHT BLENDMODES_MODE_HARDMIX BLENDMODES_MODE_DIFFERENCE BLENDMODES_MODE_EXCLUSION BLENDMODES_MODE_SUBTRACT BLENDMODES_MODE_DIVIDE BLENDMODES_MODE_HUE BLENDMODES_MODE_SATURATION BLENDMODES_MODE_COLOR BLENDMODES_MODE_LUMINOSITY

#include "../../BlendModesCG.cginc"

sampler2D _MainTex;
fixed4 _Color;
BLENDMODES_UNIFIED_GRAB_TEXTURE_SAMPLER

struct Input {
    float2 uv_MainTex;
    BLENDMODES_GRAB_POSITION(0)
};

void vert (inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input, o);
    float4 vertex = UnityObjectToClipPos(v.vertex);
    BLENDMODES_COMPUTE_GRAB_POSITION(o, vertex)
}

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    BLENDMODES_BLEND_PIXEL_UNIFIED_GRAB(c, IN)
    o.Albedo = lerp(grabColor.rgb, c.rgb, c.a);
    o.Alpha = c.a;
}
ENDCG
}

Fallback "Legacy Shaders/Diffuse"
}
