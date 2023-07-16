// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace BlendModes
{
    public abstract class MaskableGraphicExtension<TComponent> : ComponentExtension where TComponent : MaskableGraphic
    {
        protected Material DefaultMaterial => GetDefaultMaterial();

        public override void OnEffectEnabled ()
        {
            if (!IsExtendedComponentValid) return;

            GetExtendedComponent<TComponent>().RegisterDirtyMaterialCallback(SetBlendMaterialDirty);
        }

        public override void OnEffectDisabled ()
        {
            if (!IsExtendedComponentValid) return;

            GetExtendedComponent<TComponent>().UnregisterDirtyMaterialCallback(SetBlendMaterialDirty);
            GetExtendedComponent<TComponent>().material = DefaultMaterial;
        }

        public override Material GetRenderMaterial ()
        {
            if (!IsExtendedComponentValid) return null;

            return GetExtendedComponent<TComponent>().materialForRendering;
        }

        public override void SetRenderMaterial (Material renderMaterial)
        {
            if (!IsExtendedComponentValid) return;

            var component = GetExtendedComponent<TComponent>();
            component.material = renderMaterial ?? DefaultMaterial;

            // A hack to force the new material to be applied when UI mask is active.
            component.enabled = false;
            component.enabled = true;
        }

        protected virtual Material GetDefaultMaterial ()
        {
            if (!IsExtendedComponentValid) return null;

            return GetExtendedComponent<TComponent>().defaultMaterial;
        }
    }
}
