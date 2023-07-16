// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlendModes
{
    /// <summary>
    /// Used to store references to the shaders we might need at runtime.
    /// </summary>
    public sealed class ShaderResources : ScriptableObject
    {
        [Tooltip("Shaders currently included in the build.")]
        [SerializeField] private List<Shader> shaders = new List<Shader>();
        [Tooltip("Addtional shader paths to use when resolving available shader extensions. Should be relative to the project root, eg: `Assets/Shaders`.")]
        [SerializeField] private List<string> additionalPaths = new List<string>();

        private const string resourcesAssetPath = "BlendModes/ShaderResources";
        private static string[] cachedShaderFamilies;

        /// <summary>
        /// Loads a <see cref="ShaderResources"/> asset instance from the project resources.
        /// </summary>
        public static ShaderResources Load ()
        {
            return Resources.Load<ShaderResources>(resourcesAssetPath);
        }

        /// <summary>
        /// Asynchronously loads a <see cref="ShaderResources"/> asset instance from the project resources.
        /// </summary>
        public static ResourceRequest LoadAsync ()
        {
            return Resources.LoadAsync<ShaderResources>(resourcesAssetPath);
        }

        /// <summary>
        /// Returns unique available shaders. 
        /// </summary>
        public HashSet<Shader> GetShaders ()
        {
            return new HashSet<Shader>(shaders);
        }

        /// <summary>
        /// Resolves a shader by it's name (when available).
        /// </summary>
        public Shader GetShaderByName (string shaderName)
        {
            return shaders.Find(s => s.name == shaderName);
        }

        /// <summary>
        /// Whether a shader with the provided name is available.
        /// </summary>
        public bool ShaderExists (string shaderName)
        {
            return shaders.Exists(s => s.name == shaderName);
        }

        /// <summary>
        /// Returns available shader families. 
        /// </summary>
        public string[] GetShaderFamilies ()
        {
            if (cachedShaderFamilies != null) return cachedShaderFamilies;
            cachedShaderFamilies = GetShaders().Select(s => GetShaderFamily(s.name)).ToArray();
            return cachedShaderFamilies;
        }

        /// <summary>
        /// Whether a shader family with the provided name has a grab variant (supports <see cref="RenderMode.SelfWithScreen"/>).
        /// </summary>
        public bool FamilyImplementsGrab (string shaderFamily)
        {
            return shaders.Exists(s => GetShaderFamily(s.name) == shaderFamily && GetShaderVariant(s.name) == "Grab");
        }

        /// <summary>
        /// Whether a shader family with the provided name has an overlay variant (supports <see cref="RenderMode.TextureWithSelf"/>).
        /// </summary>
        public bool FamilyImplementsOverlay (string shaderFamily)
        {
            return shaders.Exists(s => GetShaderFamily(s.name) == shaderFamily && GetShaderVariant(s.name) == "Overlay");
        }

        /// <summary>
        /// Whether a shader family with the provided name supports <see cref="BlendModeEffect.UnifiedGrabEnabled"/> optimization.
        /// </summary>
        public bool FamilyImplementsUnifiedGrab (string shaderFamily)
        {
            return shaders.Exists(s => GetShaderFamily(s.name) == shaderFamily && GetShaderVariant(s.name) == "UnifiedGrab");
        }

        /// <summary>
        /// Whether a shader family with the provided name supports <see cref="BlendModeEffect.FramebufferEnabled"/> optimization.
        /// </summary>
        public bool FamilyImplementsFramebuffer (string shaderFamily)
        {
            return shaders.Exists(s => GetShaderFamily(s.name) == shaderFamily && GetShaderVariant(s.name) == "Framebuffer");
        }

        /// <summary>
        /// Whether a shader family with the provided name supports masking feature.
        /// </summary>
        public bool FamilyImplementsMasking (string shaderFamily)
        {
            return shaders.Exists(s => GetShaderFamily(s.name) == shaderFamily && GetShaderVariant(s.name).EndsWith("Masked"));
        }

        public void AddShader (Shader shader)
        {
            if (shaders.Exists(s => s.name == shader.name)) return;
            shaders.Add(shader);
            cachedShaderFamilies = null;
        }

        public void RemoveShader (Shader shader)
        {
            shaders.RemoveAll(s => s.name == shader.name);
            cachedShaderFamilies = null;
        }

        public void RemoveAllShaders ()
        {
            shaders.Clear();
            cachedShaderFamilies = null;
        }

        public List<string> GetAdditionalPaths ()
        {
            return additionalPaths;
        }

        private static string GetShaderFamily (string shaderName)
        {
            return shaderName.Split('/')[2];
        }

        private static string GetShaderVariant (string shaderName)
        {
            return shaderName.Split('/')[3]; 
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreloadShaderFamilies ()
        {
            var shaderResources = Load();
            if (!shaderResources) return;
            shaderResources.GetShaderFamilies();
        }
    }
}
