using System.Linq;
using BlendModes;
using UnityEngine;

[ExtendedComponent(typeof(MeshRenderer))]
public class DecalSpriteExtension : MeshRendererExtension
{
	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		return base.GetDefaultShaderProperties().Concat(new ShaderProperty[4]
		{
			new ShaderProperty("_MainTex", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_Color", ShaderPropertyType.Color, new Color(1f, 1f, 1f, 1f)),
			new ShaderProperty("_ProjectionScale", ShaderPropertyType.Float, 1),
			new ShaderProperty("_DoodleUVOn", ShaderPropertyType.Float, 0)
		}).ToArray();
	}

	public override string[] GetSupportedShaderFamilies()
	{
		return base.GetSupportedShaderFamilies().Concat(new string[1] { "DecalSpriteLighting" }).ToArray();
	}
}
