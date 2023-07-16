using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ImpactFrameBlackPPSRenderer), PostProcessEvent.AfterStack, "ImpactFrameBlack", true)]
public sealed class ImpactFrameBlackPPSSettings : PostProcessEffectSettings
{
	[Tooltip("Texture 0")]
	public TextureParameter _Texture0 = new TextureParameter();

	[Tooltip("LerpAmount")]
	public FloatParameter _LerpAmount = new FloatParameter
	{
		value = 1f
	};
}
