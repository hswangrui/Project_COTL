using System.Linq;
using BlendModes;
using UnityEngine;
using UnityEngine.UI;

[ExtendedComponent(typeof(Image))]
public class PlayerShadowOverlay : UIImageExtension
{
	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		return base.GetDefaultShaderProperties().Concat(new ShaderProperty[12]
		{
			new ShaderProperty("_MainTex", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_SourceRenderTextureTex_9(RGB)", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("RenderTex_1(RGB)", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_BlurHQ_Intensity_1", ShaderPropertyType.Float, 1),
			new ShaderProperty("_AlphaIntensity_Fade_1", ShaderPropertyType.Color, 1),
			new ShaderProperty("PositionUV_X_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("_TurnAlphaToBlack_Fade_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("_MaskAlpha_Fade_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("_AlphaIntensity_Fade_2", ShaderPropertyType.Color, 1),
			new ShaderProperty("PositionUV_X_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("PositionUV_Y_1", ShaderPropertyType.Float, 0),
			new ShaderProperty("SpriteFade", ShaderPropertyType.Float, 1)
		}).ToArray();
	}

	public override string[] GetSupportedShaderFamilies()
	{
		return base.GetSupportedShaderFamilies().Concat(new string[1] { "PlayerShadowOverlay" }).ToArray();
	}
}
