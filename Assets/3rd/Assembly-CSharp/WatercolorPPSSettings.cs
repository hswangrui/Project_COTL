using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(WatercolorPPSRenderer), PostProcessEvent.AfterStack, "Watercolor", true)]
public sealed class WatercolorPPSSettings : PostProcessEffectSettings
{
	[Tooltip("Sprite Texture")]
	public TextureParameter _MainTex = new TextureParameter();

	[Tooltip("Tint")]
	public ColorParameter _Color = new ColorParameter
	{
		value = new Color(1f, 1f, 1f, 1f)
	};

	[Tooltip("Stencil Comparison")]
	public FloatParameter _StencilComp = new FloatParameter
	{
		value = 8f
	};

	[Tooltip("Stencil ID")]
	public FloatParameter _Stencil = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Stencil Operation")]
	public FloatParameter _StencilOp = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("Stencil Write Mask")]
	public FloatParameter _StencilWriteMask = new FloatParameter
	{
		value = 255f
	};

	[Tooltip("Stencil Read Mask")]
	public FloatParameter _StencilReadMask = new FloatParameter
	{
		value = 255f
	};

	[Tooltip("Color Mask")]
	public FloatParameter _ColorMask = new FloatParameter
	{
		value = 15f
	};

	[Tooltip("Use Alpha Clip")]
	public FloatParameter _UseUIAlphaClip = new FloatParameter
	{
		value = 0f
	};

	[Tooltip("BlotchMultiply")]
	public FloatParameter _BlotchMultiply = new FloatParameter
	{
		value = 4.003983f
	};

	[Tooltip("BlotchSubtract")]
	public FloatParameter _BlotchSubtract = new FloatParameter
	{
		value = 2f
	};

	[Tooltip("Texture 1")]
	public TextureParameter _Texture1 = new TextureParameter();

	[Tooltip("MovementSpeed")]
	public FloatParameter _MovementSpeed = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("MovementDirection")]
	public Vector4Parameter _MovementDirection = new Vector4Parameter
	{
		value = new Vector4(0f, 1f, 0f, 0f)
	};

	[Tooltip("CloudDensity")]
	public FloatParameter _CloudDensity = new FloatParameter
	{
		value = 1f
	};

	[Tooltip("Texture 0")]
	public TextureParameter _Texture0 = new TextureParameter();

	[Tooltip("TilingUV")]
	public Vector4Parameter _TilingUV = new Vector4Parameter
	{
		value = new Vector4(1f, 1f, 0f, 0f)
	};

	[Tooltip("FadeOffset")]
	public FloatParameter _FadeOffset = new FloatParameter
	{
		value = 0f
	};
}
