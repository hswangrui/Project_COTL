// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlendModes
{
    /// <summary>
    /// Applies blend mode effect to an object with a supported component.
    /// </summary>
    /// <remarks>
    /// Supported components are the components, for which a <see cref="ComponentExtension"/> is implemented.
    /// </remarks>
    [AddComponentMenu("Effects/Blend Mode"), ExecuteInEditMode]
    public sealed class BlendModeEffect : MonoBehaviour
    {
        /// <summary>
        /// Current shader family of the object's render material.
        /// </summary>
        public string ShaderFamily { get => shaderFamily; set => SetShaderFamily(value); }
        /// <summary>
        /// Current blend mode.
        /// </summary>
        public BlendMode BlendMode { get => blendMode; set => SetBlendMode(value); }
        /// <summary>
        /// Current render mode.
        /// </summary>
        public RenderMode RenderMode { get => renderMode; set => SetRenderMode(value); }
        /// <summary>
        /// Current mask mode.
        /// </summary>
        public MaskMode MaskMode { get => maskMode; set => SetMaskMode(value); }
        /// <summary>
        /// Current mask behaviour.
        /// </summary>
        public MaskBehaviour MaskBehaviour { get => maskBehaviour; set => SetMaskBehaviour(value); }
        /// <summary>
        /// Color used to blend with the object's texture.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public Color OverlayColor { get => overlayColor; set => SetOverlayColor(value); }
        /// <summary>
        /// Texture used to blend with the object's texture.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public Texture OverlayTexture { get => overlayTexture; set => SetOverlayTexture(value); }
        /// <summary>
        /// Current UV offset of the <see cref="OverlayTexture"/>.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public Vector2 OverlayTextureOffset { get => overlayTextureOffset; set => SetOverlayTextureOffset(value); }
        /// <summary>
        /// Current UV scale of the <see cref="OverlayTexture"/>.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public Vector2 OverlayTextureScale { get => overlayTextureScale; set => SetOverlayTextureScale(value); }
        /// <summary>
        /// Whether `Framebuffer` optimization is currently enabled.
        /// Has effect only when <see cref="RenderMode.SelfWithScreen"/> is used.
        /// </summary>
        public bool FramebufferEnabled { get => framebufferEnabled; set => SetFramebufferEnabled(value); }
        /// <summary>
        /// Whether `Unified Grab` optimization is currently enabled.
        /// Has effect only when <see cref="RenderMode.SelfWithScreen"/> is used.
        /// </summary>
        public bool UnifiedGrabEnabled { get => unifiedGrabEnabled; set => SetUnifiedGrabEnabled(value); }
        /// <summary>
        /// Whether to share material instances with the same shader family and blend mode type.
        /// </summary>
        public bool ShareMaterial { get => shareMaterial; set => SetShareMaterial(value); }
        /// <summary>
        /// Whether a component extension for the current object is initialized.
        /// </summary>
        public bool IsComponentExtensionValid => сomponentExtension != null && сomponentExtension.IsValidFor(gameObject);

        [SerializeField] private string shaderFamily = null;
        [SerializeField] private BlendMode blendMode = default;
        [SerializeField] private RenderMode renderMode = default;
        [SerializeField] private MaskMode maskMode = default;
        [SerializeField] private MaskBehaviour maskBehaviour = default;
        [SerializeField] private Color overlayColor = Color.white;
        [SerializeField] private Texture overlayTexture = null;
        [SerializeField] private Vector2 overlayTextureOffset = Vector2.zero;
        [SerializeField] private Vector2 overlayTextureScale = Vector2.one;
        [SerializeField] private bool framebufferEnabled = false;
        [SerializeField] private bool unifiedGrabEnabled = false;
        [SerializeField] private bool shareMaterial = false;
        [SerializeField] private Material blendMaterial = null;
        [SerializeField] private ComponentExtensionState componentExtensionState = new ComponentExtensionState();

        private const int defaultStencilId = 1;
        private static ShaderResources shaderResources => cachedShaderResources ? cachedShaderResources : (cachedShaderResources = ShaderResources.Load());
        private static ShaderResources cachedShaderResources;

        private bool isMaterialDirty = true;
        private ComponentExtension сomponentExtension;

        /// <summary>
        /// Whether a shader family with the provided name is installed and supported by the <see cref="ComponentExtension"/>.
        /// </summary>
        public bool IsShaderFamilySupported (string shaderFamily)
        {
            if (string.IsNullOrEmpty(shaderFamily) || сomponentExtension is null || !shaderResources) return false;
            return сomponentExtension.SupportedShaderFamilies.Contains(shaderFamily) && shaderResources.GetShaderFamilies().Contains(shaderFamily);
        }

        /// <summary>
        /// Gets currently used component extension.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component extension to get.</typeparam>
        public TComponent GetComponentExtension<TComponent> () where TComponent : ComponentExtension
        {
            return сomponentExtension as TComponent;
        }

        /// <summary>
        /// Sets current shader family of the object's render material.
        /// </summary>
        public void SetShaderFamily (string shaderFamily)
        {
            if (this.shaderFamily == shaderFamily) return;
            this.shaderFamily = shaderFamily;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets current blend mode.
        /// </summary>
        public void SetBlendMode (BlendMode blendMode)
        {
            if (this.blendMode == blendMode) return;
            this.blendMode = blendMode;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets current render mode.
        /// </summary>
        public void SetRenderMode (RenderMode renderMode)
        {
            if (this.renderMode == renderMode) return;
            this.renderMode = renderMode;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets current mask mode.
        /// </summary>
        public void SetMaskMode (MaskMode maskMode)
        {
            if (this.maskMode == maskMode) return;
            this.maskMode = maskMode;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets current mask behaviour.
        /// </summary>
        public void SetMaskBehaviour (MaskBehaviour maskBehaviour)
        {
            if (this.maskBehaviour == maskBehaviour) return;
            this.maskBehaviour = maskBehaviour;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets color to blend with the object's texture.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public void SetOverlayColor (Color color)
        {
            if (overlayColor == color) return;
            overlayColor = color;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets texture to blend with the object's texture.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public void SetOverlayTexture (Texture texture)
        {
            if (overlayTexture == texture) return;
            overlayTexture = texture;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets current UV offset of the <see cref="OverlayTexture"/>.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public void SetOverlayTextureOffset (Vector2 offset)
        {
            if (overlayTextureOffset == offset) return;
            overlayTextureOffset = offset;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets current scale of the <see cref="OverlayTexture"/>.
        /// Has effect only when <see cref="RenderMode.TextureWithSelf"/> is used.
        /// </summary>
        public void SetOverlayTextureScale (Vector2 scale)
        {
            if (overlayTextureScale == scale) return;
            overlayTextureScale = scale;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets whether `Framebuffer` optimization is currently enabled.
        /// Has effect only when <see cref="RenderMode.SelfWithScreen"/> is used.
        /// </summary>
        public void SetFramebufferEnabled (bool enabled)
        {
            if (framebufferEnabled == enabled) return;
            framebufferEnabled = enabled;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets whether `Unified Grab` optimization is currently enabled.
        /// Has effect only when <see cref="RenderMode.SelfWithScreen"/> is used.
        /// </summary>
        public void SetUnifiedGrabEnabled (bool enabled)
        {
            if (unifiedGrabEnabled == enabled) return;
            unifiedGrabEnabled = enabled;
            SetMaterialDirty();
        }

        /// <summary>
        /// Sets whether to share material instances with the same shader family and blend mode type.
        /// </summary>
        public void SetShareMaterial (bool value)
        {
            if (shareMaterial == value) return;
            shareMaterial = value;
            SetMaterialProperties(true);
        }

        /// <summary>
        /// Forces to apply material properties on next update.
        /// </summary>
        public void SetMaterialDirty ()
        {
            isMaterialDirty = true;
        }

        /// <summary>
        /// Initializes the attached component extension.
        /// </summary>
        public void InitializeComponentExtension ()
        {
            var forceCreateMaterial = false;
            if (!IsComponentExtensionValid)
            {
                сomponentExtension = ComponentExtension.CreateForObject(gameObject, componentExtensionState);
                forceCreateMaterial = true;
            }
            if (сomponentExtension is null) return; // No supported component types found on the extended object.
            сomponentExtension.OnEffectEnabled();
            if (string.IsNullOrEmpty(ShaderFamily))
                ShaderFamily = сomponentExtension.DefaultShaderFamily;
            SetMaterialProperties(forceCreateMaterial);
        }

        private void SetMaterialProperties (bool forceCreateMaterial = false)
        {
            if (!enabled || сomponentExtension is null || !shaderResources) return;
            if (!IsShaderFamilySupported(ShaderFamily)) return;

            var maskEnabled = MaskMode != MaskMode.Disabled;
            var shaderName = ShaderUtilities.BuildShaderName(ShaderFamily, RenderMode, maskEnabled, FramebufferEnabled, UnifiedGrabEnabled);
            if (!shaderResources.ShaderExists(shaderName)) return;

            var shareMaterial = ShareMaterial && сomponentExtension.AllowMaterialSharing();

            if (shareMaterial)
            {
                var shader = shaderResources.GetShaderByName(shaderName);
                blendMaterial = SharedMaterials.GetSharedFor(shader, this, out bool sharedMaterialCreated);
                if (sharedMaterialCreated) сomponentExtension.OnEffectMaterialCreated(blendMaterial);
            }
            else if (!blendMaterial || blendMaterial.shader.name != shaderName || forceCreateMaterial) // Re-use current blend material when possible.
            {
                var shader = shaderResources.GetShaderByName(shaderName);
                blendMaterial = CreateBlendMaterial(shader);
                сomponentExtension.OnEffectMaterialCreated(blendMaterial);
            }

            ShaderUtilities.SelectBlendModeKeyword(blendMaterial, BlendMode);

            var propertyBlock = shareMaterial ? сomponentExtension.GetPropertyBlock() : null;
            if (RenderMode == RenderMode.TextureWithSelf) SetOverlayMaterialProperties(blendMaterial, propertyBlock);
            if (maskEnabled) SetMaskMaterialProperties(blendMaterial, propertyBlock);
            if (propertyBlock != null) сomponentExtension.SetPropertyBlock(propertyBlock);

            сomponentExtension.ApplyShaderProperties(blendMaterial);
            сomponentExtension.SetRenderMaterial(blendMaterial);

            isMaterialDirty = false;
        }

        private void SetOverlayMaterialProperties(Material material, MaterialPropertyBlock block)
        {
            if (block != null)
            {
                block.SetColor(ShaderUtilities.OverlayColorPropertyId, OverlayColor);
                block.SetTexture(ShaderUtilities.OverlayTexturePropertyId, !OverlayTexture ? Texture2D.whiteTexture : OverlayTexture); // Can't set null here.
                block.SetVector(ShaderUtilities.OverlayTextureSTPropertyId, new Vector4(OverlayTextureScale.x, OverlayTextureScale.y, OverlayTextureOffset.x, OverlayTextureOffset.y));
            }
            else
            {
                material.SetColor(ShaderUtilities.OverlayColorPropertyId, OverlayColor);
                material.SetTexture(ShaderUtilities.OverlayTexturePropertyId, OverlayTexture);
                material.SetTextureOffset(ShaderUtilities.OverlayTexturePropertyId, OverlayTextureOffset);
                material.SetTextureScale(ShaderUtilities.OverlayTexturePropertyId, OverlayTextureScale);
            }
        }

        private void SetMaskMaterialProperties (Material material, MaterialPropertyBlock block)
        {
            var blendStencilComp = MaskMode == MaskMode.NothingButMask ? (int)CompareFunction.Equal : (int)CompareFunction.NotEqual;
            var normalStencilComp = MaskBehaviour == MaskBehaviour.Cutout ? (int)CompareFunction.Never : MaskMode == MaskMode.NothingButMask ? (int)CompareFunction.NotEqual : (int)CompareFunction.Equal;

            if (block != null)
            {
                block.SetFloat(ShaderUtilities.BlendStencilCompPropertyId, blendStencilComp);
                block.SetFloat(ShaderUtilities.NormalStencilCompPropertyId, normalStencilComp);
                block.SetFloat(ShaderUtilities.StencilIdPropertyId, defaultStencilId);
            }
            else
            {
                material.SetFloat(ShaderUtilities.BlendStencilCompPropertyId, blendStencilComp);
                material.SetFloat(ShaderUtilities.NormalStencilCompPropertyId, normalStencilComp);
                material.SetFloat(ShaderUtilities.StencilIdPropertyId, defaultStencilId);
            }
        }

        private static Material CreateBlendMaterial (Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            return material;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreloadShaderResources ()
        {
            cachedShaderResources = ShaderResources.Load();
        }

        private void Reset ()
        {
            InitializeComponentExtension();
        }

        private void Awake ()
        {
            InitializeComponentExtension();
        }

        private void OnEnable ()
        {
            InitializeComponentExtension();
        }

        private void OnDisable ()
        {
            сomponentExtension?.OnEffectDisabled();
        }

        private void OnValidate ()
        {
            // Update material properties when changing serialized fields with editor GUI.
            SetMaterialDirty();
        }

        private void OnDidApplyAnimationProperties ()
        {
            // Update material properties when changing serialized fields with Unity animation.
            SetMaterialDirty();
        }

        private void OnRenderImage (RenderTexture source, RenderTexture destination)
        {
            сomponentExtension?.OnEffectRenderImage(source, destination);
        }

        private void Update ()
        {
            if (isMaterialDirty) SetMaterialProperties();

            сomponentExtension?.OnEffectUpdated();
        }
    }
}
