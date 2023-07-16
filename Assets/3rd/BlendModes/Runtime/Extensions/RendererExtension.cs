// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace BlendModes
{
    public abstract class RendererExtension<TComponent> : ComponentExtension where TComponent : Renderer
    {
        protected string DefaultShaderName => GetDefaultShaderName();

        private static Material defaultMaterial;

        public override void OnEffectEnabled ()
        {
            if (!defaultMaterial)
            {
                var defaultShader = Shader.Find(DefaultShaderName);
                if (!defaultShader) { Debug.LogWarning($"Failed to find `{DefaultShaderName}` default shader."); return; }
                defaultMaterial = new Material(defaultShader);
            }
        }

        public override void OnEffectDisabled ()
        {
            SetRendererMaterials(defaultMaterial);
        }

        public override Material GetRenderMaterial ()
        {
            if (!IsExtendedComponentValid) return null;

            var materials = GetExtendedComponent<TComponent>().sharedMaterials;
            return materials != null && materials.Length > 0 ? materials[0] : null;
        }

        public override void SetRenderMaterial (Material renderMaterial)
        {
            SetRendererMaterials(renderMaterial);
        }

        protected abstract string GetDefaultShaderName ();

        protected virtual void SetRendererMaterials (Material material)
        {
            if (!IsExtendedComponentValid) return;

            var extendedComponent = GetExtendedComponent<TComponent>();
            if (extendedComponent.sharedMaterials == null)
                extendedComponent.sharedMaterials = new[] { material };
            else
            {
                var materials = new Material[extendedComponent.sharedMaterials.Length];
                for (var i = 0; i < materials.Length; i++)
                    materials[i] = material;
                extendedComponent.sharedMaterials = materials;
            }
        }
    }
}
