using System;
using UnityEngine;

[Serializable]
public class BiomeVolume
{
	public enum ShaderTypes
	{
		Float,
		Color,
		Texture,
		Vector2
	}

	public enum ShaderNames
	{
		Null,
		UnscaledTime,
		StartItemsInWoodsColor,
		WindDirection,
		WindSpeed,
		WindDensity,
		LightingRenderTexture,
		Fog_ZOffset,
		Fog_GradientSpread,
		CloudDensity,
		CloudAlpha,
		Dither
	}

	public ShaderTypes types;

	public ShaderNames _ShaderNames;

	public string shaderName;

	public float valueToGoTo;

	public Color colorToGoTo;

	public Texture textureToGoTo;

	public Vector2 Vector2ToGoTo;

	private void Start()
	{
	}

	public string getName(ShaderNames s)
	{
		switch (s)
		{
		case ShaderNames.Null:
			return "";
		case ShaderNames.UnscaledTime:
			return "_GlobalTimeUnscaled";
		case ShaderNames.StartItemsInWoodsColor:
			return "_ItemInWoodsColor";
		case ShaderNames.WindDirection:
			return "_WindDiection";
		case ShaderNames.WindSpeed:
			return "_WindSpeed";
		case ShaderNames.WindDensity:
			return "_WindDensity";
		case ShaderNames.LightingRenderTexture:
			return "_Lighting_RenderTexture";
		case ShaderNames.Fog_ZOffset:
			return "_VerticalFog_ZOffset";
		case ShaderNames.Fog_GradientSpread:
			return "_VerticalFog_GradientSpread";
		case ShaderNames.CloudDensity:
			return "_CloudDensity";
		case ShaderNames.CloudAlpha:
			return "_CloudAlpha";
		case ShaderNames.Dither:
			return "_GlobalDitherIntensity";
		default:
			return "";
		}
	}

	public BiomeVolume(ShaderTypes types, ShaderNames shaderName, Vector2 valueToGoTo)
	{
		this.types = types;
		_ShaderNames = shaderName;
		this.shaderName = getName(shaderName);
		Vector2ToGoTo = valueToGoTo;
	}

	public BiomeVolume(ShaderTypes types, ShaderNames shaderName, float valueToGoTo)
	{
		this.types = types;
		_ShaderNames = shaderName;
		this.shaderName = getName(shaderName);
		this.valueToGoTo = valueToGoTo;
	}

	public BiomeVolume(ShaderTypes types, ShaderNames shaderName, Color valueToGoTo)
	{
		this.types = types;
		_ShaderNames = shaderName;
		this.shaderName = getName(shaderName);
		colorToGoTo = valueToGoTo;
	}

	public BiomeVolume(ShaderTypes types, ShaderNames shaderName, Texture valueToGoTo)
	{
		this.types = types;
		_ShaderNames = shaderName;
		this.shaderName = getName(shaderName);
		textureToGoTo = valueToGoTo;
	}
}
