using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class ASEPostProcessFilmGrainPPSRenderer : PostProcessEffectRenderer<ASEPostProcessFilmGrainPPSSettings>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Hidden/Post Process/FilmGrain"));
		propertySheet.properties.SetFloat("_Strength", base.settings._Strength);
		propertySheet.properties.SetFloat("_ColorMaskRange", base.settings._ColorMaskRange);
		propertySheet.properties.SetFloat("_ColorMaskFuzziness", base.settings._ColorMaskFuzziness);
		propertySheet.properties.SetColor("_ColorToMask", base.settings._ColorToMask);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
