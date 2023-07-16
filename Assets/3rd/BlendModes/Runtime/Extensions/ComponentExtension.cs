// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlendModes
{
    /// <summary>
    /// Implementation provides APIs required to extend <see cref="BlendModes.BlendModeEffect"/> to support a specific <see cref="Component"/>.
    /// </summary>
    public abstract class ComponentExtension
    {
        /// <summary>
        /// Specifies which component type is extended by the implementing <see cref="ComponentExtension"/>.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        protected sealed class ExtendedComponentAttribute : Attribute
        {
            public Type ExtendedComponentType { get; private set; }

            public ExtendedComponentAttribute (Type extendedComponentType)
            {
                ExtendedComponentType = extendedComponentType;
            }
        }

        /// <summary>
        /// Names of the supported blend mode shader families. 
        /// </summary>
        public string[] SupportedShaderFamilies => GetSupportedShaderFamilies();
        /// <summary>
        /// Name of the blend mode shader family used by default. 
        /// </summary>
        public string DefaultShaderFamily => GetDefaultShaderFamily();
        /// <summary>
        /// Custom shader properties to be added when the component extension is created.
        /// </summary>
        public ShaderProperty[] DefaultShaderProperties => GetDefaultShaderProperties();
        /// <summary>
        /// Whether the extended component reference is valid.
        /// </summary>
        public bool IsExtendedComponentValid => state != null && state.ExtendedComponent;

        /// <summary>
        /// The master effect that uses this component extension.
        /// </summary>
        protected BlendModeEffect BlendModeEffect => IsExtendedComponentValid ? state.ExtendedComponent.GetComponent<BlendModeEffect>() : null;
        /// <summary>
        /// Custom shader properties of the extension component.
        /// </summary>
        protected ShaderProperty[] ShaderProperties => state != null ? state.ShaderProperties : new ShaderProperty[0];

        /// <summary>
        /// A map of [extended type] -> [extension types collection].
        /// </summary>
        /// <remarks>
        /// Multiple extension types per extended type are possible when user adds their own.
        /// </remarks>
        private static Dictionary<Type, List<Type>> extensionsMap;
        /// <summary>
        /// Serializable properties of the extension component.
        /// </summary>
        /// <remarks>
        /// The serialized data is kept with <see cref="BlendModeEffect"/> and is provided on construction.
        /// </remarks>
        private ComponentExtensionState state;

        /// <summary>
        /// Looks for a supported <see cref="Component"/> attached to the provided <see cref="GameObject"/>
        /// and creates a <see cref="ComponentExtension"/> instance of appropriate type.
        /// </summary>
        public static ComponentExtension CreateForObject (GameObject extendedObject, ComponentExtensionState state)
        {
            var extensionsMap = GetExtensionMap();
            var supportedTypes = extensionsMap.Keys;
            var extendedComponent = extendedObject.GetComponents<Component>().FirstOrDefault(c => supportedTypes.Contains(c.GetType()));
            if (extendedComponent == null) return null; // No supported component types found on the extended object.

            var componentExtensionType = ResolveComponentExtensionType(extendedComponent);
            if (componentExtensionType == null)
            {
                Debug.LogError($"Failed to resolve component extension type for '{extendedComponent.GetType().FullName}'.");
                return null;
            }

            var componentExtension = Activator.CreateInstance(componentExtensionType) as ComponentExtension;
            componentExtension.state = state;
            componentExtension.state.ExtendedComponent = extendedComponent;
            componentExtension.state.ShaderProperties = componentExtension.GetOverridedShaderProperties(); 
            componentExtension.OnEffectEnabled();
            return componentExtension;
        }

        /// <summary>
        /// Checks whether the extension references a valid component on the provided object.
        /// </summary>
        public virtual bool IsValidFor (GameObject gameObject)
        {
            return IsExtendedComponentValid && state.ExtendedComponent.gameObject == gameObject;
        }

        /// <summary>
        /// Returns name of the blend mode shader family to be used by default. 
        /// </summary>
        public virtual string GetDefaultShaderFamily ()
        {
            return SupportedShaderFamilies[0];
        }

        /// <summary>
        /// Returns custom shader properties to be added when the component extension is created.
        /// </summary>
        public virtual ShaderProperty[] GetDefaultShaderProperties ()
        {
            return new ShaderProperty[0];
        }

        /// <summary>
        /// Whether current render material has property with the provided name.
        /// </summary>
        public virtual bool MaterialHasProperty (string propertyName)
        {
            var material = GetRenderMaterial();
            return material && material.HasProperty(propertyName);
        }

        /// <summary>
        /// Returns a shader property with the provided name.
        /// </summary>
        public virtual ShaderProperty GetShaderProperty (string propertyName)
        {
            return ShaderProperties.FirstOrDefault(p => p.Name == propertyName);
        }

        /// <summary>
        /// Assigns <see cref="ShaderProperties"/> to the provided material.
        /// </summary>
        public virtual void ApplyShaderProperties (Material material)
        {
            if (material == null) return;

            foreach (var shaderProperty in ShaderProperties)
                shaderProperty.ApplyToMaterial(material);
        }

        /// <summary>
        /// Whether to allow sharing material instances with the same shader family and blend mode type.
        /// </summary>
        public virtual bool AllowMaterialSharing () { return true; }

        /// <summary>
        /// When supported by the extended component, returns material property block of render material.
        /// </summary>
        public virtual MaterialPropertyBlock GetPropertyBlock () { return null; }

        /// <summary>
        /// When supported by the extended component, applies material property block to the render material.
        /// Will be used instead of setting material propreties directly when <see cref="AllowMaterialSharing"/> is true.
        /// </summary>
        public virtual void SetPropertyBlock (MaterialPropertyBlock propertyBlock) { }

        /// <summary>
        /// Returns names of the supported blend mode shader families.
        /// </summary>
        public abstract string[] GetSupportedShaderFamilies ();

        /// <summary>
        /// Returns the material used for rendering by the extended component.
        /// </summary>
        public abstract Material GetRenderMaterial ();

        /// <summary>
        /// Assigns the material to be used for rendering by the extended component.
        /// </summary>
        public abstract void SetRenderMaterial (Material material);

        /// <summary>
        /// Invoked by the <see cref="BlendModes.BlendModeEffect"/> when a new blend mode material is created.
        /// </summary>
        public virtual void OnEffectMaterialCreated (Material material) { }

        /// <summary>
        /// Invoked by the <see cref="BlendModes.BlendModeEffect"/> when effect is (re)-enabled; 
        /// also invoked when the component extension is created.
        /// </summary>
        public virtual void OnEffectEnabled () { }

        /// <summary>
        /// Invoked by the <see cref="BlendModes.BlendModeEffect"/> when effect is disabled; 
        /// also invoked when the component extension is disabled.
        /// </summary>
        public virtual void OnEffectDisabled () { }

        /// <summary>
        /// Invoked by the <see cref="BlendModes.BlendModeEffect"/> when effect is updated (once per frame).
        /// </summary>
        public virtual void OnEffectUpdated () { }

        /// <summary>
        /// Invoked by the <see cref="BlendModes.BlendModeEffect"/> when `OnRenderImage` callback is received. 
        /// </summary>
        public virtual void OnEffectRenderImage (RenderTexture source, RenderTexture destination) { }

        /// <summary>
        /// Retrieves extended component of specific type. 
        /// Type should equal or be derived from <see cref="ExtendedComponentAttribute.ExtendedComponentType"/>.
        /// </summary>
        public TComponent GetExtendedComponent<TComponent> () where TComponent : Component
        {
            return state.ExtendedComponent as TComponent;
        }

        /// <summary>
        /// Forces the <see cref="BlendModeEffect"/> to apply material properties on next update.
        /// </summary>
        protected void SetBlendMaterialDirty ()
        {
            if (BlendModeEffect) BlendModeEffect.SetMaterialDirty();
        }

        /// <summary>
        /// Returns <see cref="DefaultShaderProperties"/> overrided by the current first render material and state existing properties.
        /// </summary>
        protected virtual ShaderProperty[] GetOverridedShaderProperties ()
        {
            var material = GetRenderMaterial();
            var properties = new List<ShaderProperty>();
            foreach (var defaultProperty in DefaultShaderProperties)
            {
                var property = default(ShaderProperty);
                if (state != null && state.ShaderProperties.Any(p => p.Name == defaultProperty.Name))
                    property = new ShaderProperty(state.ShaderProperties.First(p => p.Name == defaultProperty.Name));
                else
                {
                    property = new ShaderProperty(defaultProperty);
                    if (material && material.HasProperty(property.Name))
                        property.SetValue(material.GetProperty(property.Type, property.Name));
                }
                properties.Add(property);
            }
            return properties.ToArray();
        }

        private static Type ResolveComponentExtensionType (Component extendedComponent)
        {
            var extensionMap = GetExtensionMap();
            var extendedType = extendedComponent.GetType();
            var extensionTypes = extensionMap[extendedType];
            // Get first most derived type (considering all the extensions always derive from one base type).
            return extensionTypes.Where(t1 => !extensionTypes.Exists(t2 => t2.BaseType == t1)).FirstOrDefault();
        }

        private static Dictionary<Type, List<Type>> GetExtensionMap ()
        {
            if (extensionsMap != null) return extensionsMap;

            extensionsMap = new Dictionary<Type, List<Type>>();
            var assemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !IsDynamicAssembly(a)).SelectMany(a => a.GetExportedTypes());
            foreach (var type in assemblyTypes)
            {
                var attrs = type.GetCustomAttributes(typeof(ExtendedComponentAttribute), false);
                if (attrs.Length == 0) continue;

                var attr = attrs[0] as ExtendedComponentAttribute;
                if (!extensionsMap.ContainsKey(attr.ExtendedComponentType))
                    extensionsMap.Add(attr.ExtendedComponentType, new List<Type>());
                extensionsMap[attr.ExtendedComponentType].Add(type);
            }
            return extensionsMap;
        }

        private static bool IsDynamicAssembly (System.Reflection.Assembly assembly)
        {
            #if NET_4_6 || NET_STANDARD_2_0
            return assembly.IsDynamic;
            #else
            return assembly is System.Reflection.Emit.AssemblyBuilder;
            #endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreloadExtensionMap ()
        {
            GetExtensionMap();
        }
    }
}
