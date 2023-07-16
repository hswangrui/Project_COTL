/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif
#define SPINE_OPTIONAL_MATERIALOVERRIDE

// Contributed by: Lost Polygon

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
#if NEW_PREFAB_SYSTEM
    [ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
    public class SkeletonRendererCustomMaterials : MonoBehaviour
    {

        [Serializable]
        public class SlotMaterialOverride : IEquatable<SlotMaterialOverride>
        {
            public bool overrideDisabled;

            [SpineSlot("", "", false, true, false)]
            public string slotName;

            public Material material;

            public bool Equals(SlotMaterialOverride other)
            {
                if (overrideDisabled == other.overrideDisabled && slotName == other.slotName)
                {
                    return material == other.material;
                }
                return false;
            }
        }

        [Serializable]
        public struct AtlasMaterialOverride : IEquatable<AtlasMaterialOverride>
        {
            public bool overrideDisabled;

            public Material originalMaterial;

            public Material replacementMaterial;

            public bool Equals(AtlasMaterialOverride other)
            {
                if (overrideDisabled == other.overrideDisabled && originalMaterial == other.originalMaterial)
                {
                    return replacementMaterial == other.replacementMaterial;
                }
                return false;
            }
        }

        public SkeletonRenderer skeletonRenderer;

        [SerializeField]
        public List<SlotMaterialOverride> customSlotMaterials = new List<SlotMaterialOverride>();

        [SerializeField]
        protected List<AtlasMaterialOverride> customMaterialOverrides = new List<AtlasMaterialOverride>();

        private void SetCustomSlotMaterials()
        {
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            for (int i = 0; i < customSlotMaterials.Count; i++)
            {
                SlotMaterialOverride slotMaterialOverride = customSlotMaterials[i];
                if (!slotMaterialOverride.overrideDisabled && !string.IsNullOrEmpty(slotMaterialOverride.slotName))
                {
                    Slot key = skeletonRenderer.skeleton.FindSlot(slotMaterialOverride.slotName);
                    skeletonRenderer.CustomSlotMaterials[key] = slotMaterialOverride.material;
                }
            }
        }

        private void RemoveCustomSlotMaterials()
        {
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            for (int i = 0; i < customSlotMaterials.Count; i++)
            {
                SlotMaterialOverride slotMaterialOverride = customSlotMaterials[i];
                if (!string.IsNullOrEmpty(slotMaterialOverride.slotName))
                {
                    Slot key = skeletonRenderer.skeleton.FindSlot(slotMaterialOverride.slotName);
                    Material value;
                    if (skeletonRenderer.CustomSlotMaterials.TryGetValue(key, out value) && !(value != slotMaterialOverride.material))
                    {
                        skeletonRenderer.CustomSlotMaterials.Remove(key);
                    }
                }
            }
        }

        private void SetCustomMaterialOverrides()
        {
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            for (int i = 0; i < customMaterialOverrides.Count; i++)
            {
                AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
                if (!atlasMaterialOverride.overrideDisabled)
                {
                    skeletonRenderer.CustomMaterialOverride[atlasMaterialOverride.originalMaterial] = atlasMaterialOverride.replacementMaterial;
                }
            }
        }

        public void SetCustomMaterialOverrides(bool forceOverrideDisabled)
        {
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            for (int i = 0; i < customMaterialOverrides.Count; i++)
            {
                AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
                if (!forceOverrideDisabled)
                {
                    skeletonRenderer.CustomMaterialOverride[atlasMaterialOverride.originalMaterial] = atlasMaterialOverride.replacementMaterial;
                }
            }
        }

        private void RemoveCustomMaterialOverrides()
        {
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            for (int i = 0; i < customMaterialOverrides.Count; i++)
            {
                AtlasMaterialOverride atlasMaterialOverride = customMaterialOverrides[i];
                Material value;
                if (skeletonRenderer.CustomMaterialOverride.TryGetValue(atlasMaterialOverride.originalMaterial, out value) && !(value != atlasMaterialOverride.replacementMaterial))
                {
                    skeletonRenderer.CustomMaterialOverride.Remove(atlasMaterialOverride.originalMaterial);
                }
            }
        }

        private void OnEnable()
        {
            if (skeletonRenderer == null)
            {
                skeletonRenderer = GetComponent<SkeletonRenderer>();
            }
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            skeletonRenderer.Initialize(false);
            SetCustomMaterialOverrides();
            SetCustomSlotMaterials();
        }

        public void UpdateMaterials()
        {
            if (skeletonRenderer == null)
            {
                skeletonRenderer = GetComponent<SkeletonRenderer>();
            }
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            skeletonRenderer.Initialize(false);
            SetCustomMaterialOverrides();
            SetCustomSlotMaterials();
        }

        private void OnDisable()
        {
            if (skeletonRenderer == null)
            {
                Debug.LogError("skeletonRenderer == null");
                return;
            }
            RemoveCustomMaterialOverrides();
            RemoveCustomSlotMaterials();
        }
    }
}
