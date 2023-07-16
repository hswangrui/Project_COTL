// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine.Tilemaps;

namespace BlendModes
{
    [ExtendedComponent(typeof(TilemapRenderer))]
    public class TilemapRendererExtension : RendererExtension<TilemapRenderer>
    {
        private static ShaderProperty[] cachedDefaultProperties;

        public override string[] GetSupportedShaderFamilies ()
        {
            return new[] {
                "SpritesDefault",
                "SpritesHsbc",

                #if BLENDMODES_LWRP
                "LwrpSpritesDefault",
                #endif
            };
        }

        public override ShaderProperty[] GetDefaultShaderProperties ()
        {
            return cachedDefaultProperties ?? (cachedDefaultProperties = new[] {
                    new ShaderProperty("_Hue", ShaderPropertyType.Float, 0),
                    new ShaderProperty("_Saturation", ShaderPropertyType.Float, 0),
                    new ShaderProperty("_Brightness", ShaderPropertyType.Float, 0),
                    new ShaderProperty("_Contrast", ShaderPropertyType.Float, 0)
                });
        }

        protected override string GetDefaultShaderName ()
        {
            return "Sprites/Default";
        }
    }
}
