// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlendModes
{
    /// <summary>
    /// Collection of static utilities to work with <see cref="Shader"/> and associated objects.
    /// </summary>
    public static class ShaderUtilities
    {
        public const string OverlayColorPropertyName = "_BLENDMODES_OverlayColor";
        public const string OverlayTexturePropertyName = "_BLENDMODES_OverlayTexture";
        public const string OverlayTextureSTPropertyName = OverlayTexturePropertyName + "_ST";
        public const string StencilIdPropertyName = "_BLENDMODES_StencilId";
        public const string BlendStencilCompPropertyName = "_BLENDMODES_BlendStencilComp";
        public const string NormalStencilCompPropertyName = "_BLENDMODES_NormalStencilComp";

        public static readonly int OverlayColorPropertyId = Shader.PropertyToID(OverlayColorPropertyName);
        public static readonly int OverlayTexturePropertyId = Shader.PropertyToID(OverlayTexturePropertyName);
        public static readonly int OverlayTextureSTPropertyId = Shader.PropertyToID(OverlayTextureSTPropertyName);
        public static readonly int StencilIdPropertyId = Shader.PropertyToID(StencilIdPropertyName);
        public static readonly int BlendStencilCompPropertyId = Shader.PropertyToID(BlendStencilCompPropertyName);
        public static readonly int NormalStencilCompPropertyId = Shader.PropertyToID(NormalStencilCompPropertyName);

        private const string modeKeywordPrefix = "BLENDMODES_MODE_";
        private static Dictionary<BlendMode, string> cachedShaderKeywords;

        /// <summary>
        /// Builds full shader name based on required shader family, render mode, mask support and optimizations.
        /// </summary>
        public static string BuildShaderName (string shaderFamily, RenderMode renderMode, bool maskEnabled, bool framebufferEnabled, bool unifiedGrabEnabled)
        {
            var shaderName = string.Empty;

            switch (renderMode)
            {
                case RenderMode.SelfWithScreen:
                    if (framebufferEnabled && Application.platform == RuntimePlatform.IPhonePlayer) shaderName = "Framebuffer";
                    else if (unifiedGrabEnabled) shaderName = "UnifiedGrab";
                    else shaderName = "Grab";
                    break;
                case RenderMode.TextureWithSelf:
                    shaderName = "Overlay";
                    break;
            }

            if (maskEnabled) shaderName += "Masked";

            return $"Hidden/BlendModes/{shaderFamily}/{shaderName}";
        }

        /// <summary>
        /// Returns shader keyword representation of the provided blend mode.
        /// </summary>
        public static string ToShaderKeyword (this BlendMode blendMode)
        {
            if (cachedShaderKeywords != null)
                return cachedShaderKeywords[blendMode];

            cachedShaderKeywords = new Dictionary<BlendMode, string>();
            foreach (var value in (BlendMode[])Enum.GetValues(typeof(BlendMode)))
                cachedShaderKeywords[value] = string.Concat(modeKeywordPrefix, value).ToUpperInvariant();
            return cachedShaderKeywords[blendMode];
        }

        /// <summary>
        /// Enables required blend mode keyword for the provided material while disabling all the other blend mode keywords.
        /// </summary>
        public static void SelectBlendModeKeyword (Material material, BlendMode blendMode)
        {
            foreach (var keyword in material.shaderKeywords)
                if (keyword.StartsWith(modeKeywordPrefix, StringComparison.InvariantCulture))
                    material.DisableKeyword(keyword);
            material.EnableKeyword(blendMode.ToShaderKeyword());
        }

        /// <summary>
        /// Copies material property value from one material to another when the property is defined in both materials.
        /// </summary>
        public static void TransferMaterialProperty (int propertyId, ShaderPropertyType propertyType, Material fromMaterial, Material toMaterial)
        {
            if (!fromMaterial || !toMaterial) return;
            if (!fromMaterial.HasProperty(propertyId) || !toMaterial.HasProperty(propertyId)) return;

            switch (propertyType)
            {
                case ShaderPropertyType.Color:
                    toMaterial.SetColor(propertyId, fromMaterial.GetColor(propertyId));
                    break;
                case ShaderPropertyType.Vector:
                    toMaterial.SetVector(propertyId, fromMaterial.GetVector(propertyId));
                    break;
                case ShaderPropertyType.Float:
                    toMaterial.SetFloat(propertyId, fromMaterial.GetFloat(propertyId));
                    break;
                case ShaderPropertyType.Texture:
                    toMaterial.SetTexture(propertyId, fromMaterial.GetTexture(propertyId));
                    break;
            }
        }

        /// <summary>
        /// Copies material property value from one material to another when the property is defined in both materials.
        /// </summary>
        public static void TransferMaterialProperty (string propertyName, ShaderPropertyType propertyType, Material fromMaterial, Material toMaterial)
        {
            var propertyId = Shader.PropertyToID(propertyName);
            TransferMaterialProperty(propertyId, propertyType, fromMaterial, toMaterial);
        }

        /// <summary>
        /// Checks whether provided value object is of a type.
        /// </summary>
        public static bool CheckPropertyValueType (object value, ShaderPropertyType type)
        {
            if (value == null) return false;

            switch (type)
            {
                case ShaderPropertyType.Color:
                    return value is Color;
                case ShaderPropertyType.Vector:
                    return value is Vector4;
                case ShaderPropertyType.Float:
                    return value is float;
                case ShaderPropertyType.Texture:
                    return value is Texture;
                default: return false;
            }
        }

        /// <summary>
        /// Attempts to set provided property value object to the material.
        /// Will silently fail if the property is of a wrong type or doesn't exist.
        /// </summary>
        public static void SetProperty (this Material material, ShaderPropertyType type, string name, object value)
        {
            if (!material || !material.HasProperty(name) || !CheckPropertyValueType(value, type)) return;

            switch (type)
            {
                case ShaderPropertyType.Color:
                    material.SetColor(name, (Color)value);
                    break;
                case ShaderPropertyType.Vector:
                    material.SetVector(name, (Vector4)value);
                    break;
                case ShaderPropertyType.Float:
                    material.SetFloat(name, (float)value);
                    break;
                case ShaderPropertyType.Texture:
                    material.SetTexture(name, (Texture)value);
                    break;
            }
        }

        /// <summary>
        /// Attempts to get a value of the property with the provided name from the material.
        /// </summary>
        public static object GetProperty (this Material material, ShaderPropertyType type, string name)
        {
            if (!material || !material.HasProperty(name)) return null;

            switch (type)
            {
                case ShaderPropertyType.Color:
                    return material.GetColor(name);
                case ShaderPropertyType.Vector:
                    return material.GetVector(name);
                case ShaderPropertyType.Float:
                    return material.GetFloat(name);
                case ShaderPropertyType.Texture:
                    return material.GetTexture(name);
                default: return null;
            }
        }
    }
}
