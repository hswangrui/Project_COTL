// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

#if BLENDMODES_TMPRO

using TMPro;
using UnityEngine;

namespace BlendModes
{
    [ExtendedComponent(typeof(TextMeshProUGUI))]
    public class TMProUGUIExtension : MaskableGraphicExtension<TextMeshProUGUI>
    {
        private Material CurrentMaterial
        {
            get { return IsExtendedComponentValid ? GetExtendedComponent<TextMeshProUGUI>().fontMaterial : null; }
            set { if (IsExtendedComponentValid && value) GetExtendedComponent<TextMeshProUGUI>().fontMaterial = value; }
        }

        public override string[] GetSupportedShaderFamilies ()
        {
            return new[] {
                "TMProMobileDistanceField",
                "TMProDistanceField",

                #if BLENDMODES_LWRP
                "LwrpTMProMobileDistanceField",
                #endif
            };
        }

        public override string GetDefaultShaderFamily ()
        {
            if (!IsExtendedComponentValid) base.GetDefaultShaderFamily();
            if (DefaultMaterial.shader.name.Contains("Mobile")) return "TMProMobileDistanceField";
            else return "TMProDistanceField";
        }

        protected override Material GetDefaultMaterial ()
        {
            return IsExtendedComponentValid ? GetExtendedComponent<TextMeshProUGUI>().font.material : null;
        }

        public override void OnEffectMaterialCreated (Material material)
        {
            // Allow editing TMPro material via inspector.
            material.hideFlags = HideFlags.DontSave;
            // Copy atlas texture and other props from the previous material.
            TransferTMProProperties(DefaultMaterial, material);
        }

        public override void OnEffectEnabled () { }

        public override void OnEffectDisabled ()
        {
            if (!IsExtendedComponentValid) return;
            // Copy TMPro props from the current material before fallback to default material.
            // FIXME: Causes internal TMPro nullref when inside a UIMask object and blend mode effect component is disabled (project assets should be saved to reproduce)
            // or moving the object out of the mask object (project assets should NOT be saved to reproduce).
            // TransferTMProProperties(CurrentMaterial, DefaultMaterial);
            CurrentMaterial = DefaultMaterial;
        }

        public override void SetRenderMaterial (Material renderMaterial)
        {
            CurrentMaterial = renderMaterial ?? DefaultMaterial;
        }

        private static void TransferTMProProperties (Material fromMaterial, Material toMaterial)
        {
            // Not using Material.CopyPropertiesFromMaterial(), cause we shouldn't copy stencil props and some other stuff.

            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_MainTex, ShaderPropertyType.Texture, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_FaceTex, ShaderPropertyType.Texture, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_FaceColor, ShaderPropertyType.Color, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_FaceDilate, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_Shininess, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_UnderlayColor, ShaderPropertyType.Color, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_UnderlayOffsetX, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_UnderlayOffsetY, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_UnderlayDilate, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_UnderlaySoftness, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_WeightNormal, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_WeightBold, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_OutlineTex, ShaderPropertyType.Texture, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_OutlineWidth, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_OutlineSoftness, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_OutlineColor, ShaderPropertyType.Color, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_Padding, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_GradientScale, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_ScaleX, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_ScaleY, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_PerspectiveFilter, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_TextureWidth, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_TextureHeight, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_BevelAmount, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_GlowColor, ShaderPropertyType.Color, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_GlowOffset, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_GlowPower, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_GlowOuter, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_LightAngle, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_ShaderFlags, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_ScaleRatio_A, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_ScaleRatio_B, ShaderPropertyType.Float, fromMaterial, toMaterial);
            ShaderUtilities.TransferMaterialProperty(TMPro.ShaderUtilities.ID_ScaleRatio_C, ShaderPropertyType.Float, fromMaterial, toMaterial);

            if (fromMaterial.IsKeywordEnabled(TMPro.ShaderUtilities.Keyword_Bevel)) toMaterial.EnableKeyword(TMPro.ShaderUtilities.Keyword_Bevel);
            if (fromMaterial.IsKeywordEnabled(TMPro.ShaderUtilities.Keyword_Glow)) toMaterial.EnableKeyword(TMPro.ShaderUtilities.Keyword_Glow);
            if (fromMaterial.IsKeywordEnabled(TMPro.ShaderUtilities.Keyword_Underlay)) toMaterial.EnableKeyword(TMPro.ShaderUtilities.Keyword_Underlay);
            if (fromMaterial.IsKeywordEnabled(TMPro.ShaderUtilities.Keyword_Ratios)) toMaterial.EnableKeyword(TMPro.ShaderUtilities.Keyword_Ratios);
            if (fromMaterial.IsKeywordEnabled(TMPro.ShaderUtilities.Keyword_Outline)) toMaterial.EnableKeyword(TMPro.ShaderUtilities.Keyword_Outline);
        }
    }
}

#endif
