using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ASEPostProcessFilmGrainPPSRenderer), PostProcessEvent.AfterStack, "ASEPostProcessFilmGrain", true)]
public sealed class ASEPostProcessFilmGrainPPSSettings : PostProcessEffectSettings
{
	[Tooltip("Strength")]
	public FloatParameter _Strength = new FloatParameter
	{
		value = 15f
	};

	[Tooltip("ColorMaskRange")]
	[Range(0f, 1f)]
	public FloatParameter _ColorMaskRange = new FloatParameter
	{
		value = 0.1f
	};

	[Tooltip("ColorMaskFuzziness")]
	[Range(0f, 1f)]
	public FloatParameter _ColorMaskFuzziness = new FloatParameter
	{
		value = 0.1f
	};

	[Tooltip("ColorToMask")]
	public ColorParameter _ColorToMask = new ColorParameter
	{
		value = new Color(0f, 0f, 0f, 0f)
	};
}
