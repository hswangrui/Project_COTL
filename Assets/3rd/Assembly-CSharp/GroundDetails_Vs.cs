using System;
using System.Linq;
using BlendModes;
using UnityEngine;

[ExtendedComponent(typeof(SpriteRenderer))]
public class GroundDetails_Vs : SpriteRendererExtension
{
	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		return base.GetDefaultShaderProperties().Concat(new ShaderProperty[15]
		{
			new ShaderProperty("_MainTex", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("AnimatedOffsetUV_X_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_Y_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_ZoomX_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_ZoomY_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("AnimatedOffsetUV_Speed_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("_SourceNewTex_1", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_InnerGlowHQ_Intensity_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("_InnerGlowHQ_Size_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("_InnerGlowHQ_Color_1", ShaderPropertyType.Color, new ValueTuple<int, int, int, int>(1, 1, 0, 1)),
			new ShaderProperty("_ColorHSV_Hue_1", ShaderPropertyType.Float, 180f),
			new ShaderProperty("_ColorHSV_Saturation_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("_ColorHSV_Brightness_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("_Add_Fade_1", ShaderPropertyType.Float, 1f),
			new ShaderProperty("SpriteFade", ShaderPropertyType.Float, 1f)
		}).ToArray();
	}

	public override string[] GetSupportedShaderFamilies()
	{
		return base.GetSupportedShaderFamilies().Concat(new string[1] { "GroundDetails_Vs" }).ToArray();
	}
}
