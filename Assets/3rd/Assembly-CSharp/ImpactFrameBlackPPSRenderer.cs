using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class ImpactFrameBlackPPSRenderer : PostProcessEffectRenderer<ImpactFrameBlackPPSSettings>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("ImpactFrameBlack"));
		if (base.settings._Texture0.value != null)
		{
			propertySheet.properties.SetTexture("_Texture0", base.settings._Texture0);
		}
		propertySheet.properties.SetFloat("_LerpAmount", base.settings._LerpAmount);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
