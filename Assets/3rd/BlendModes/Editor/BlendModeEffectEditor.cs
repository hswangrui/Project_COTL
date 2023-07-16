// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace BlendModes
{
    [CustomEditor(typeof(BlendModeEffect)), CanEditMultipleObjects]
    public class BlendModeEffectEditor : Editor
    {
        protected BlendModeEffect BlendModeEffect => target as BlendModeEffect;
        protected ComponentExtension ComponentExtension => BlendModeEffect.GetComponentExtension<ComponentExtension>();

        private static readonly GUIContent shaderFamilyContent = new GUIContent("Shader Family", "Shader family of the object's render material.");
        private static readonly GUIContent blendModeContent = new GUIContent("Blend Mode", "Algorithm to be used when blending render layers.");
        private static readonly GUIContent renderModeContent = new GUIContent("Render Mode", "The way in which blend mode effect is applied to the affected object:\n\n - Self With Screen: Blend object's texture with anything that was drawn before the object on screen. Similar to layer blending modes in Photoshop;\n\n - Texture With Self: Blend specified overlay color and texture with the object's texture. Similar to `Pattern Overlay` layer style in Photoshop.");
        private static readonly GUIContent overlayColorContent = new GUIContent("Overlay Color", "Color used to blend with the object's texture.");
        private static readonly GUIContent overlayTextureContent = new GUIContent("Overlay Texture", "Texture used to blend with the object's texture.");
        private static readonly GUIContent overlayTextureOffsetContent = new GUIContent("Overlay Offset", "UV offset of the overlay texture.");
        private static readonly GUIContent overlayTextureScaleContent = new GUIContent("Overlay Scale", "UV scale of the overlay texture.");
        private static readonly GUIContent maskModeContent = new GUIContent("Mask Mode", "The way in which blend mode effect is applied when interacting with the masked areas:\n\n - Disabled: Apply blend effect over the entire object (ignore masks);\n\n - Nothing But Mask: Apply blend effect over the areas affected by mask;\n\n - Everything But Mask: Apply blend effect over the areas not affected by mask.");
        private static readonly GUIContent maskBehaviourContent = new GUIContent("Mask Behaviour", "What should happen with the pixels affected by the mask:\n\n - Cutout: Pixels over the affected (masked) areas will be discarded;\n\n - Normal: Pixels over the affected (masked) areas will be rendered using default material pass (normal blend mode).");
        private static readonly GUIContent optimizationsContent = new GUIContent("Optimizations", "Available performance optimizations for the current instance of the effect.");
        private static readonly GUIContent framebufferEnabledContent = new GUIContent("Framebuffer", "Use `framebuffer_fetch` instruction instead of a grab pass to access the backbuffer. Significantly improves performance, but is only supported on selected devices (most notably PowerVR-based ones on iOS).");
        private static readonly GUIContent unifiedGrabEnabledContent = new GUIContent("Unified Grab", "Share a grab texture with the other instances, that have this optimization enabled. Significantly improves performance when multiple blend mode effects are used simultaneously, but the instances won't properly blend with each other.");
        private static readonly GUIContent shareMaterialContent = new GUIContent("Share Material", "Whether to share material instances with the same shader family and blend mode type. Can reduce draw calls count in some cases, but all the material properties will also be shared.");
        private static readonly int blendModeEnumLength = System.Enum.GetNames(typeof(BlendMode)).Length;

        private SerializedProperty shaderFamilyProperty;
        private SerializedProperty blendModeProperty;
        private SerializedProperty renderModeProperty;
        private SerializedProperty overlayColorProperty;
        private SerializedProperty overlayTextureProperty;
        private SerializedProperty overlayTextureOffsetProperty;
        private SerializedProperty overlayTextureScaleProperty;
        private SerializedProperty maskModeProperty;
        private SerializedProperty maskBehaviourProperty;
        private SerializedProperty framebufferEnabledProperty;
        private SerializedProperty unifiedGrabEnabledProperty;
        private SerializedProperty shareMaterialProperty;

        private ShaderPropertiesList shaderPropertiesList;

        private void OnEnable ()
        {
            if (BlendModeEffect)
            {
                BlendModeEffect.InitializeComponentExtension();
                var shaderPropertiesProperty = serializedObject.FindProperty("componentExtensionState").FindPropertyRelative("shaderProperties");
                shaderPropertiesList = new ShaderPropertiesList(serializedObject, shaderPropertiesProperty, BlendModeEffect.GetComponentExtension<ComponentExtension>());
            }

            shaderFamilyProperty = serializedObject.FindProperty("shaderFamily");
            blendModeProperty = serializedObject.FindProperty("blendMode");
            renderModeProperty = serializedObject.FindProperty("renderMode");
            overlayColorProperty = serializedObject.FindProperty("overlayColor");
            overlayTextureProperty = serializedObject.FindProperty("overlayTexture");
            overlayTextureOffsetProperty = serializedObject.FindProperty("overlayTextureOffset");
            overlayTextureScaleProperty = serializedObject.FindProperty("overlayTextureScale");
            maskModeProperty = serializedObject.FindProperty("maskMode");
            maskBehaviourProperty = serializedObject.FindProperty("maskBehaviour");
            framebufferEnabledProperty = serializedObject.FindProperty("framebufferEnabled");
            unifiedGrabEnabledProperty = serializedObject.FindProperty("unifiedGrabEnabled");
            shareMaterialProperty = serializedObject.FindProperty("shareMaterial");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            if (!BlendModeEffect.IsComponentExtensionValid)
            {
                EditorGUILayout.HelpBox($"`{BlendModeEffect.gameObject.name}` game object doesn't have a supported component.", MessageType.Warning);
                EditorGUILayout.HelpBox("You can add your own component extensions; see `Adding component extensions` in the docs for more info.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (!BlendModeEffect.IsShaderFamilySupported(BlendModeEffect.ShaderFamily))
            {
                ShaderFamilyGUI();
                InstallShaderExtensionGUI();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            BlendModeGUI();
            ShaderFamilyGUI();
            RenderModeGUI();
            MaskingGUI();
            OptimizationsGUI();

            if (shaderPropertiesList != null)
                shaderPropertiesList.DrawList();

            serializedObject.ApplyModifiedProperties();
        }

        private void InstallShaderExtensionGUI ()
        {
            if (!ExtensionManager.IsShaderExtensionAvailable(BlendModeEffect.ShaderFamily))
            {
                EditorGUILayout.HelpBox($"`{BlendModeEffect.ShaderFamily}` shader family is not available.", MessageType.Warning);
                EditorGUILayout.HelpBox("You can add your own shader extensions; see `Adding shader extensions` in the docs for more info.", MessageType.Info);
                return;
            }
            EditorGUILayout.HelpBox($"`{BlendModeEffect.ShaderFamily}` shader extension is available, but not installed.", MessageType.Info);
            if (GUILayout.Button("Install shader extension"))
            {
                ExtensionManager.InstallShaderExtension(BlendModeEffect.ShaderFamily);
                BlendModeEffect.InitializeComponentExtension();
            }
        }

        private void ShaderFamilyGUI ()
        {
            if (ComponentExtension == null || ComponentExtension.GetSupportedShaderFamilies().Length == 1) return;
            var oldIndex = ArrayUtility.IndexOf(ComponentExtension.SupportedShaderFamilies, shaderFamilyProperty.stringValue);
            var newIndex = EditorGUILayout.Popup(shaderFamilyContent, oldIndex, ComponentExtension.SupportedShaderFamilies);
            shaderFamilyProperty.stringValue = ComponentExtension.SupportedShaderFamilies[newIndex];
        }

        private void BlendModeGUI ()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(blendModeProperty, blendModeContent);
            var rect = EditorGUILayout.GetControlRect();
            rect.xMin += EditorGUIUtility.labelWidth;
            rect.width = rect.width / 2 - 1;
            if (GUI.Button(rect, "<<", EditorStyles.miniButton))
            {
                var blendModeIndex = blendModeProperty.enumValueIndex;
                blendModeIndex--;
                if (blendModeIndex < 0)
                    blendModeIndex = blendModeEnumLength - 1;
                blendModeProperty.enumValueIndex = blendModeIndex;
            }
            rect.x += rect.width + 2;
            if (GUI.Button(rect, ">>", EditorStyles.miniButton))
            {
                var blendModeIndex = blendModeProperty.enumValueIndex;
                blendModeIndex++;
                if (blendModeIndex >= blendModeEnumLength)
                    blendModeIndex = 0;
                blendModeProperty.enumValueIndex = blendModeIndex;
            }
            EditorGUILayout.Separator();
        }

        private void RenderModeGUI ()
        {
            var supportsGrab = ExtensionManager.ShaderResources.FamilyImplementsGrab(BlendModeEffect.ShaderFamily);
            var supportsOverlay = ExtensionManager.ShaderResources.FamilyImplementsOverlay(BlendModeEffect.ShaderFamily);

            if (!supportsOverlay) { renderModeProperty.enumValueIndex = (int)RenderMode.SelfWithScreen; return; }
            if (!supportsGrab) { renderModeProperty.enumValueIndex = (int)RenderMode.TextureWithSelf; OverlayModeGUI(); return; }

            EditorGUILayout.PropertyField(renderModeProperty, renderModeContent);
            if (BlendModeEffect.RenderMode == RenderMode.TextureWithSelf) OverlayModeGUI();
        }

        private void OverlayModeGUI ()
        {
            EditorGUILayout.PropertyField(overlayColorProperty, overlayColorContent);
            EditorGUILayout.PropertyField(overlayTextureProperty, overlayTextureContent);
            EditorGUILayout.PropertyField(overlayTextureOffsetProperty, overlayTextureOffsetContent);
            EditorGUILayout.PropertyField(overlayTextureScaleProperty, overlayTextureScaleContent);
        }

        private void MaskingGUI ()
        {
            if (!ExtensionManager.ShaderResources.FamilyImplementsMasking(BlendModeEffect.ShaderFamily))
            {
                maskModeProperty.enumValueIndex = (int)MaskMode.Disabled;
                return;
            }
            EditorGUILayout.PropertyField(maskModeProperty, maskModeContent);
            if (BlendModeEffect.MaskMode != MaskMode.Disabled)
                EditorGUILayout.PropertyField(maskBehaviourProperty, maskBehaviourContent);
        }

        private void OptimizationsGUI ()
        {
            if (BlendModeEffect.RenderMode == RenderMode.TextureWithSelf && !ComponentExtension.AllowMaterialSharing())
            {
                if (shareMaterialProperty.boolValue)
                {
                    shareMaterialProperty.boolValue = false;
                    BlendModeEffect.SetShareMaterial(false);
                }
                return;
            }

            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, optimizationsContent);
            rect.width = rect.width / 2;

            if (BlendModeEffect.RenderMode == RenderMode.TextureWithSelf)
            {
                EditorGUI.BeginChangeCheck();
                ToggleLeftGUI(rect, shareMaterialProperty, shareMaterialContent);
                if (EditorGUI.EndChangeCheck()) BlendModeEffect.SetShareMaterial(shareMaterialProperty.boolValue);
                return;
            }
            else if (shareMaterialProperty.boolValue)
            {
                shareMaterialProperty.boolValue = false;
                BlendModeEffect.SetShareMaterial(false);
            }

            var supportsUnifiedGrab = ExtensionManager.ShaderResources.FamilyImplementsUnifiedGrab(BlendModeEffect.ShaderFamily);
            var supportsFramebuffer = ExtensionManager.ShaderResources.FamilyImplementsFramebuffer(BlendModeEffect.ShaderFamily);
            if (!supportsUnifiedGrab && !supportsFramebuffer)
            {
                unifiedGrabEnabledProperty.boolValue = false;
                framebufferEnabledProperty.boolValue = false;
                return;
            }
            if (supportsUnifiedGrab)
            {
                ToggleLeftGUI(rect, unifiedGrabEnabledProperty, unifiedGrabEnabledContent);
                rect.x += rect.width;
            }
            else unifiedGrabEnabledProperty.boolValue = false;
            if (supportsFramebuffer) ToggleLeftGUI(rect, framebufferEnabledProperty, framebufferEnabledContent);
            else framebufferEnabledProperty.boolValue = false;
        }

        private static void ToggleLeftGUI (Rect rect, SerializedProperty property, GUIContent content)
        {
            var toggleValue = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            toggleValue = EditorGUI.ToggleLeft(rect, content, toggleValue);
            if (EditorGUI.EndChangeCheck())
                property.boolValue = property.hasMultipleDifferentValues ? true : !property.boolValue;
            EditorGUI.showMixedValue = false;
        }
    }
}
