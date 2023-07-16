using System.Linq;
using BlendModes;
using UnityEngine;
using UnityEngine.UI;

[ExtendedComponent(typeof(Image))]
public class LightLeak : UIImageExtension
{
	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		return base.GetDefaultShaderProperties().Concat(new ShaderProperty[14]
		{
			new ShaderProperty("_SourceRenderTextureTex_17", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_BlurHQ_Intensity_2", ShaderPropertyType.Float, 1),
			new ShaderProperty("_BlurHQ_Intensity_1", ShaderPropertyType.Float, 1),
			new ShaderProperty("_FillColor_Color_3", ShaderPropertyType.Color, 1),
			new ShaderProperty("_FillColor_Color_2", ShaderPropertyType.Color, 1),
			new ShaderProperty("_FillColor_Color_1", ShaderPropertyType.Color, 1),
			new ShaderProperty("PositionUV_X_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("PositionUV_Y_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("_TurnAlphaToBlack_Fade_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("_TurnAlphaToBlack_Fade_2", ShaderPropertyType.Float, 0),
			new ShaderProperty("_MaskRGBA_Fade_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("_MaskRGBA_Fade_2", ShaderPropertyType.Float, 0),
			new ShaderProperty("_HdrCreate_Value_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("SpriteFade", ShaderPropertyType.Float, 1)
		}).ToArray();
	}

	public override string[] GetSupportedShaderFamilies()
	{
		return base.GetSupportedShaderFamilies().Concat(new string[1] { "LightLeak" }).ToArray();
	}
}
