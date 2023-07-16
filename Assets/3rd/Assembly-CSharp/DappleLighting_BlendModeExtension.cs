using System.Linq;
using BlendModes;
using UnityEngine;

[ExtendedComponent(typeof(MeshRenderer))]
public class DappleLighting_BlendModeExtension : MeshRendererExtension
{
	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		return base.GetDefaultShaderProperties().Concat(new ShaderProperty[11]
		{
			new ShaderProperty("_TexA", ShaderPropertyType.Texture, Texture2D.blackTexture),
			new ShaderProperty("_InvertOn", ShaderPropertyType.Float, 0),
			new ShaderProperty("_ShadowColor", ShaderPropertyType.Color, new Color(0.5f, 0.5f, 0.7f, 1f)),
			new ShaderProperty("_LightThreshold", ShaderPropertyType.Float, 0.5),
			new ShaderProperty("_LightClamp", ShaderPropertyType.Float, 0.5),
			new ShaderProperty("_BloomAmnt", ShaderPropertyType.Float, 1),
			new ShaderProperty("_DistortAmnt", ShaderPropertyType.Float, 0.5f),
			new ShaderProperty("_WaveSpeed", ShaderPropertyType.Float, 1),
			new ShaderProperty("_WaveScale", ShaderPropertyType.Float, 1),
			new ShaderProperty("_SrcBlend", ShaderPropertyType.Float, 1),
			new ShaderProperty("_DstBlend", ShaderPropertyType.Float, 0)
		}).ToArray();
	}

	public override string[] GetSupportedShaderFamilies()
	{
		return base.GetSupportedShaderFamilies().Concat(new string[2] { "DappleLighting", "DecalSpriteLighting" }).ToArray();
	}
}
