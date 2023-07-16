// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine.UI;

namespace BlendModes
{
    [ExtendedComponent(typeof(Text))]
    public class UITextExtension : MaskableGraphicExtension<Text>
    {
        public override string[] GetSupportedShaderFamilies ()
        {
            return new[] {
                "UIDefaultFont",

                #if BLENDMODES_LWRP
                "LwrpUIDefaultFont",
                #endif
            };
        }
    }
}
