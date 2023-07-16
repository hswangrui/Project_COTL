using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(Desaturate_StencilRTMaskPPSRenderer), PostProcessEvent.BeforeStack, "Desaturate_StencilRTMask", true)]
public sealed class Desaturate_StencilRTMaskPPSSettings : PostProcessEffectSettings
{
	[Tooltip("Intensity")]
	[Range(0f, 1f)]
	public FloatParameter _Intensity = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("Contrast")]
	public FloatParameter _Contrast = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("Brightness")]
	[Range(-1f, 1f)]
	public FloatParameter _Brightness = new FloatParameter
	{
		value = 0f
	};
}
