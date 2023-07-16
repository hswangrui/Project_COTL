using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class Desaturate_StencilRTMaskPPSRenderer : PostProcessEffectRenderer<Desaturate_StencilRTMaskPPSSettings>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Desaturate_StencilRTMask"));
		propertySheet.properties.SetFloat("_Intensity", base.settings._Intensity);
		propertySheet.properties.SetFloat("_Contrast", base.settings._Contrast);
		propertySheet.properties.SetFloat("_Brightness", base.settings._Brightness);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
