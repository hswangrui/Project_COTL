// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace BlendModes
{
    /// <summary>
    /// Manages shared materials used by <see cref="BlendModeEffect"/>.
    /// </summary>
    public static class SharedMaterials 
    {
        private static List<Material> sharedMaterials = new List<Material>();

        /// <summary>
        /// Based on the current requirements of the provided <see cref="BlendModeEffect"/> object
        /// and shader, retrieves existing or creates a new shared material.
        /// </summary>
        public static Material GetSharedFor (Shader shader, BlendModeEffect blendModeEffect, out bool materialCreated)
        {
            materialCreated = false;
            var blendModeKeyword = blendModeEffect.BlendMode.ToShaderKeyword();
            for (int i = 0; i < sharedMaterials.Count; i++)
            {
                var material = sharedMaterials[i];
                if (!material) continue;
                if (material.shader == shader && material.IsKeywordEnabled(blendModeKeyword))
                    return material;
            }
            materialCreated = true;
            return CreateSharedMaterial(shader);
        }

        /// <summary>
        /// Destroys all the created shared materials.
        /// </summary>
        public static void DestroySharedMaterials ()
        {
            for (int i = 0; i < sharedMaterials.Count; i++)
            {
                var material = sharedMaterials[i];
                if (!material) continue;
                if (Application.isPlaying) Object.Destroy(material);
                else Object.DestroyImmediate(material);
            }
            sharedMaterials.Clear();
        }

        private static Material CreateSharedMaterial (Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.HideAndDontSave;
            sharedMaterials.Add(material);
            return material;
        }
    }
}
