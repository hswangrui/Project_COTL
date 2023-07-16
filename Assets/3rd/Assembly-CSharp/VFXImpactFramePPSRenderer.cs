using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class VFXImpactFramePPSRenderer : PostProcessEffectRenderer<VFXImpactFramePPSSettings>
{
	public override void Render(PostProcessRenderContext context)
	{
		if (Shader.Find("VFX/ImpactFrame") != null)
		{
			PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("VFX/ImpactFrame"));
			propertySheet.properties.SetFloat("_ContrastAmount", base.settings._ContrastAmount);
			propertySheet.properties.SetFloat("_PosterizeAmount", base.settings._PosterizeAmount);
			if (base.settings._ImpactFrame.value != null)
			{
				propertySheet.properties.SetTexture("_ImpactFrame", base.settings._ImpactFrame);
			}
			if (base.settings._Texture1.value != null)
			{
				propertySheet.properties.SetTexture("_Texture1", base.settings._Texture1);
			}
			propertySheet.properties.SetFloat("_BlendAmount", base.settings._BlendAmount);
			propertySheet.properties.SetFloat("_DistortionUV", base.settings._DistortionUV);
			context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
		}
	}
}
