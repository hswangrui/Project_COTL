using System.Linq;
using BlendModes;
using UnityEngine;

[ExtendedComponent(typeof(SpriteRenderer))]
public class GroundDetails : SpriteRendererExtension
{
	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		return base.GetDefaultShaderProperties().Concat(new ShaderProperty[10]
		{
			new ShaderProperty("_MainTex", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("AnimatedOffsetUV_X_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_Y_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_ZoomX_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_ZoomY_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_Speed_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("_NewTex_1", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_Threshold_Fade_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("_MaskRGBA_Fade_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("SpriteFade", ShaderPropertyType.Float, 1f)
		}).ToArray();
	}

	public override string[] GetSupportedShaderFamilies()
	{
		return base.GetSupportedShaderFamilies().Concat(new string[1] { "GroundDetails" }).ToArray();
	}
}
