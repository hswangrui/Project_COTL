using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class WatercolorPPSRenderer : PostProcessEffectRenderer<WatercolorPPSSettings>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Watercolor"));
		if (base.settings._MainTex.value != null)
		{
			propertySheet.properties.SetTexture("_MainTex", base.settings._MainTex);
		}
		propertySheet.properties.SetColor("_Color", base.settings._Color);
		propertySheet.properties.SetFloat("_StencilComp", base.settings._StencilComp);
		propertySheet.properties.SetFloat("_Stencil", base.settings._Stencil);
		propertySheet.properties.SetFloat("_StencilOp", base.settings._StencilOp);
		propertySheet.properties.SetFloat("_StencilWriteMask", base.settings._StencilWriteMask);
		propertySheet.properties.SetFloat("_StencilReadMask", base.settings._StencilReadMask);
		propertySheet.properties.SetFloat("_ColorMask", base.settings._ColorMask);
		propertySheet.properties.SetFloat("_UseUIAlphaClip", base.settings._UseUIAlphaClip);
		propertySheet.properties.SetFloat("_BlotchMultiply", base.settings._BlotchMultiply);
		propertySheet.properties.SetFloat("_BlotchSubtract", base.settings._BlotchSubtract);
		if (base.settings._Texture1.value != null)
		{
			propertySheet.properties.SetTexture("_Texture1", base.settings._Texture1);
		}
		propertySheet.properties.SetFloat("_MovementSpeed", base.settings._MovementSpeed);
		propertySheet.properties.SetVector("_MovementDirection", base.settings._MovementDirection);
		propertySheet.properties.SetFloat("_CloudDensity", base.settings._CloudDensity);
		if (base.settings._Texture0.value != null)
		{
			propertySheet.properties.SetTexture("_Texture0", base.settings._Texture0);
		}
		propertySheet.properties.SetVector("_TilingUV", base.settings._TilingUV);
		propertySheet.properties.SetFloat("_FadeOffset", base.settings._FadeOffset);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
