using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(VFXImpactFramePPSRenderer), PostProcessEvent.AfterStack, "VFXImpactFrame", true)]
public sealed class VFXImpactFramePPSSettings : PostProcessEffectSettings
{
	[Tooltip("ContrastAmount")]
	public FloatParameter _ContrastAmount = new FloatParameter
	{
		value = 101.33f
	};

	[Tooltip("PosterizeAmount")]
	public FloatParameter _PosterizeAmount = new FloatParameter
	{
		value = 101.33f
	};

	[Tooltip("ImpactFrame")]
	public TextureParameter _ImpactFrame = new TextureParameter();

	[Tooltip("Texture 1")]
	public TextureParameter _Texture1 = new TextureParameter();

	[Tooltip("BlendAmount")]
	public FloatParameter _BlendAmount = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("DistortionUV")]
	public FloatParameter _DistortionUV = new FloatParameter
	{
		value = 0.25f
	};
}
