using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(AmplifyPostEffectRenderer), PostProcessEvent.BeforeStack, "AmplifyPostProcess", true)]
public sealed class AmplifyPostEffect : PostProcessEffectSettings
{
	public FloatParameter Exposure = new FloatParameter
	{
		value = 1.15f
	};

	public RenderTextureParameter MaskTexture = new RenderTextureParameter
	{
		value = null
	};

	public TextureParameter LutTexture = new TextureParameter
	{
		value = null,
		defaultState = TextureParameterDefault.Lut2D
	};

	public TextureParameter LutHighlightTexture = new TextureParameter
	{
		value = null,
		defaultState = TextureParameterDefault.Lut2D
	};

	[Range(0f, 1f)]
	public FloatParameter BlendAmount = new FloatParameter
	{
		value = 0.5f
	};

	public TextureParameter LutBlendTexture = new TextureParameter
	{
		value = null,
		defaultState = TextureParameterDefault.Lut2D
	};

	public TextureParameter LutHighlightBlendTexture = new TextureParameter
	{
		value = null,
		defaultState = TextureParameterDefault.Lut2D
	};
}
