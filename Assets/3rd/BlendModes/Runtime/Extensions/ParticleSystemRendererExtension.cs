// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace BlendModes
{
    [ExtendedComponent(typeof(ParticleSystemRenderer))]
    public class ParticleSystemRendererExtension : RendererExtension<ParticleSystemRenderer>
    {
        private static ShaderProperty[] cachedDefaultProperties;

        public override string[] GetSupportedShaderFamilies ()
        {
            return new[] {
                "ParticlesAdditive",
                "ParticlesHsbc",

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
                    new ShaderProperty("_InvFade", ShaderPropertyType.Float, 1f),
                    new ShaderProperty("_Hue", ShaderPropertyType.Float, 0),
                    new ShaderProperty("_Saturation", ShaderPropertyType.Float, 0),
                    new ShaderProperty("_Brightness", ShaderPropertyType.Float, 0),
                    new ShaderProperty("_Contrast", ShaderPropertyType.Float, 0)
                });
        }

        protected override string GetDefaultShaderName ()
        {
            return "Particles/Standard Unlit";
        }
    }
}
