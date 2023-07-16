// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


// Can't conditionally compile the whole script, as it breaks Unity serialization.

using UnityEngine;
using UnityEngine.Rendering;
#if BLENDMODES_LWRP
using UnityEngine.Rendering.LWRP;
#endif

namespace BlendModes
{
    public class RenderBlendModeEffect
        #if BLENDMODES_LWRP
        : ScriptableRendererFeature
        #endif
    {
        #if BLENDMODES_LWRP
        private class RenderBlendModeEffectPass : ScriptableRenderPass
        {
            private const string commandBufferName = nameof(RenderBlendModeEffectPass);
            // Corresponds to `Tags { "LightMode" = "BlendModeEffect" }` in the shaders.
            private static readonly ShaderTagId shaderTag = new ShaderTagId("BlendModeEffect");
            private static readonly int tempRTPropertyId = Shader.PropertyToID("_BLENDMODES_TempRT");
            private static readonly int grabTexturePropertyId = Shader.PropertyToID("_BLENDMODES_LwrpGrabTexture");

            public RenderBlendModeEffectPass ()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            }

            public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // Grab screen texture.
                var cmd = CommandBufferPool.Get(commandBufferName);
                cmd.GetTemporaryRT(tempRTPropertyId, renderingData.cameraData.cameraTargetDescriptor);
                cmd.Blit(BuiltinRenderTextureType.CameraTarget, tempRTPropertyId);
                cmd.SetGlobalTexture(grabTexturePropertyId, tempRTPropertyId);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // Draw objects using blend mode effect material.
                var drawingSettings = CreateDrawingSettings(shaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }

            public override void FrameCleanup (CommandBuffer cmd)
            {
                base.FrameCleanup(cmd);

                cmd.ReleaseTemporaryRT(tempRTPropertyId);
            }
        }

        private RenderBlendModeEffectPass grabScreenPass;

        public override void Create ()
        {
            grabScreenPass = new RenderBlendModeEffectPass();
        }

        public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(grabScreenPass);
        }
        #endif
    }
}
