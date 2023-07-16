using System.Linq;
using BlendModes;
using UnityEngine;
using UnityEngine.UI;

[ExtendedComponent(typeof(Image))]
public class WatercolorImageExtension : UIImageExtension
{
	public override ShaderProperty[] GetDefaultShaderProperties()
	{
		return base.GetDefaultShaderProperties().Concat(new ShaderProperty[27]
		{
			new ShaderProperty("_MainTex", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_Color", ShaderPropertyType.Color, new Color(1f, 1f, 1f, 1f)),
			new ShaderProperty("_StencilComp", ShaderPropertyType.Float, 8),
			new ShaderProperty("_Stencil", ShaderPropertyType.Float, 0),
			new ShaderProperty("_StencilOp", ShaderPropertyType.Float, 0),
			new ShaderProperty("_StencilWriteMask", ShaderPropertyType.Float, 255),
			new ShaderProperty("_StencilReadMask", ShaderPropertyType.Float, 255),
			new ShaderProperty("_ColorMask", ShaderPropertyType.Float, 15),
			new ShaderProperty("_UseUIAlphaClip", ShaderPropertyType.Float, 0),
			new ShaderProperty("_BlotchMultiply", ShaderPropertyType.Float, 4.003983),
			new ShaderProperty("_BlotchSubtract", ShaderPropertyType.Float, 2),
			new ShaderProperty("_Texture1", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_MovementSpeed", ShaderPropertyType.Float, 2),
			new ShaderProperty("_MovementDirection", ShaderPropertyType.Vector, new Vector4(0f, 1f, 0f, 0f)),
			new ShaderProperty("_CloudDensity", ShaderPropertyType.Float, 1),
			new ShaderProperty("_Texture0", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_TilingUV", ShaderPropertyType.Vector, new Vector4(1f, 1f, 0f, 0f)),
			new ShaderProperty("_Rotation", ShaderPropertyType.Float, 0),
			new ShaderProperty("_PosX", ShaderPropertyType.Float, 0),
			new ShaderProperty("_PosY", ShaderPropertyType.Float, 0),
			new ShaderProperty("_PowerExp", ShaderPropertyType.Float, 0),
			new ShaderProperty("_MaskMainTex", ShaderPropertyType.Float, 0),
			new ShaderProperty("_TintCOlor", ShaderPropertyType.Color, new Color(1f, 1f, 1f, 1f)),
			new ShaderProperty("_PositionOffset", ShaderPropertyType.Vector, new Vector4(0f, 0f, 0f, 0f)),
			new ShaderProperty("_MultiplyTextureAlpha", ShaderPropertyType.Float, 1),
			new ShaderProperty("_Mask", ShaderPropertyType.Texture, Texture2D.whiteTexture),
			new ShaderProperty("_texcoord", ShaderPropertyType.Texture, Texture2D.whiteTexture)
		}).ToArray();
	}

	public override string[] GetSupportedShaderFamilies()
	{
		return base.GetSupportedShaderFamilies().Concat(new string[1] { "WatercolorImageExtension" }).ToArray();
	}
}
