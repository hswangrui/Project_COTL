// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace BlendModes
{
    [ExtendedComponent(typeof(TrailRenderer))]
    public class TrailRendererExtension : RendererExtension<TrailRenderer>
    {
        private static ShaderProperty[] cachedDefaultProperties;

        public override string[] GetSupportedShaderFamilies ()
        {
            return new[] {
                "ParticlesAdditive",

                #if BLENDMODES_LWRP
                "LwrpParticlesAdditive",
                #endif
            };
        }

        public override ShaderProperty[] GetDefaultShaderProperties ()
        {
            return cachedDefaultProperties ?? (cachedDefaultProperties = new[] {
                    new ShaderProperty("_MainTex", ShaderPropertyType.Texture, Texture2D.whiteTexture),
                    new ShaderProperty("_TintColor", ShaderPropertyType.Color, Color.white),
                    new ShaderProperty("_InvFade", ShaderPropertyType.Float, 1f)
                });
        }

        protected override string GetDefaultShaderName ()
        {
            return "Particles/Standard Unlit";
        }
    }
}
