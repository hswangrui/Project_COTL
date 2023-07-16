// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace BlendModes
{
    [System.Serializable]
    public class ComponentExtensionState
    {
        public Component ExtendedComponent { get => extendedComponent; set => extendedComponent = value; }
        public ShaderProperty[] ShaderProperties { get => shaderProperties; set => shaderProperties = value; }

        [SerializeField] private Component extendedComponent = null;
        [SerializeField] private ShaderProperty[] shaderProperties = new ShaderProperty[0];
    }
}
