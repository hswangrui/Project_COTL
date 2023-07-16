using BlendModes;
using UnityEngine;

[ExtendedComponent(typeof(ParticleSystemRenderer))]
public class Godrays_BlendModeExtension : RendererExtension<ParticleSystemRenderer>
{
	private static ShaderProperty[] cachedDefaultProperties;

	public override string[] GetSupportedShaderFamilies()
	{
		return new string[2] { "ParticlesAdditive", "ParticlesHsbc" };
	}

	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		object obj = cachedDefaultProperties;
		if (obj == null)
		{
			obj = new ShaderProperty[7]
			{
				new ShaderProperty("_MainTex", ShaderPropertyType.Texture, Texture2D.whiteTexture),
				new ShaderProperty("_TintColor", ShaderPropertyType.Color, Color.white),
				new ShaderProperty("_InvFade", ShaderPropertyType.Float, 1f),
				new ShaderProperty("_Hue", ShaderPropertyType.Float, 0),
				new ShaderProperty("_Saturation", ShaderPropertyType.Float, 0),
				new ShaderProperty("_Brightness", ShaderPropertyType.Float, 0),
				new ShaderProperty("_Contrast", ShaderPropertyType.Float, 0)
			};
			cachedDefaultProperties = (ShaderProperty[])obj;
		}
		return (ShaderProperty[])obj;
	}

	protected override string GetDefaultShaderName()
	{
		return "Particles/Standard Unlit";
	}
}
