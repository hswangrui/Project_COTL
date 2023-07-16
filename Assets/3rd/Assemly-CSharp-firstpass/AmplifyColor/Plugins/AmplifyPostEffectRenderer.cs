using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class AmplifyPostEffectRenderer : PostProcessEffectRenderer<AmplifyPostEffect>
{
	public const int LutSize = 32;

	public const int LutWidth = 1024;

	public const int LutHeight = 32;

	private Shader shaderBlendCache;

	private Shader shaderMask;

	private Shader shaderMaskDual;

	private Shader shaderMaskBlend;

	private Shader shaderMaskBlendDual;

	private Shader shaderProcessOnly;

	private RenderTexture blendCacheLut;

	private RenderTexture blendCacheLutHighlight;

	private Texture2D defaultLut;

	private Material materialBlendCache;

	private bool blending;

	private bool isMaterialCreated;

	public Texture2D DefaultLut
	{
		get
		{
			if (!(defaultLut == null))
			{
				return defaultLut;
			}
			return RuntimeUtilities.GetLutStrip(32);
		}
	}

	private void SetUpShader()
	{
		shaderBlendCache = Shader.Find("Hidden/Amplify Color/BlendCache");
		shaderMask = Shader.Find("Hidden/Amplify Color/PPE_Mask");
		shaderMaskDual = Shader.Find("Hidden/Amplify Color/PPE_MaskDual");
		shaderMaskBlend = Shader.Find("Hidden/Amplify Color/PPE_MaskBlend");
		shaderMaskBlendDual = Shader.Find("Hidden/Amplify Color/PPE_MaskBlendDual");
		shaderProcessOnly = Shader.Find("Hidden/Amplify Color/PPE_ProcessOnly");
	}

	private void CreateMaterials()
	{
		if (!isMaterialCreated)
		{
			isMaterialCreated = true;
			SafeRelease(ref materialBlendCache);
			materialBlendCache = new Material(shaderBlendCache);
		}
	}

	private void SafeRelease<T>(ref T obj) where T : Object
	{
		Object.DestroyImmediate(obj);
		obj = null;
	}

	public static bool ValidateLutDimensions(Texture lut)
	{
		bool result = true;
		if (lut != null)
		{
			if (lut.width / lut.height != lut.height)
			{
				Debug.LogWarning("[AmplifyColor] Lut " + lut.name + " has invalid dimensions.");
				result = false;
			}
			else if (lut.anisoLevel != 0)
			{
				lut.anisoLevel = 0;
			}
		}
		return result;
	}

	private void CreateHelperTextures()
	{
	}

	public override void Render(PostProcessRenderContext context)
	{
		SetUpShader();
		CreateMaterials();
		CreateHelperTextures();
		bool num = ValidateLutDimensions(base.settings.LutTexture.value);
		bool flag = ValidateLutDimensions(base.settings.LutBlendTexture.value);
		bool flag2 = ValidateLutDimensions(base.settings.LutHighlightTexture.value);
		bool flag3 = ValidateLutDimensions(base.settings.LutHighlightBlendTexture.value);
		bool flag4 = base.settings.LutTexture.value == null && base.settings.LutBlendTexture.value == null && base.settings.LutHighlightTexture.value == null && base.settings.LutHighlightBlendTexture.value == null;
		Texture texture = ((base.settings.LutTexture.value == null) ? RuntimeUtilities.GetLutStrip(32) : base.settings.LutTexture.value);
		Texture value = base.settings.LutBlendTexture.value;
		Texture texture2 = ((base.settings.LutHighlightTexture.value == null) ? RuntimeUtilities.GetLutStrip(32) : base.settings.LutHighlightTexture.value);
		Texture value2 = base.settings.LutHighlightBlendTexture.value;
		bool flag5 = QualitySettings.activeColorSpace == ColorSpace.Linear;
		bool allowHDR = context.camera.allowHDR;
		int num2 = 0;
		if (allowHDR)
		{
			num2 += 2;
			num2 += (flag5 ? 1 : 0);
		}
		else
		{
			num2 += (flag5 ? 1 : 0);
		}
		bool flag6 = (float)base.settings.BlendAmount != 0f || blending;
		bool flag7 = flag6 || (flag6 && value != null);
		bool flag8 = flag7;
		bool flag9 = flag6 || (flag6 && value2 != null);
		bool flag10 = flag9;
		Shader shader = ((!num || !flag || !flag2 || !flag3 || flag4) ? shaderProcessOnly : ((flag7 || flag9) ? ((!(base.settings.MaskTexture.value != null)) ? shaderProcessOnly : (flag9 ? shaderMaskBlendDual : shaderMaskBlend)) : ((!(base.settings.MaskTexture.value != null)) ? shaderProcessOnly : ((base.settings.LutHighlightTexture.value != null) ? shaderMaskDual : shaderMask))));
		PropertySheet propertySheet = context.propertySheets.Get(shader);
		propertySheet.ClearKeywords();
		propertySheet.properties.SetVector("_ScreenDims", new Vector2(Screen.width, Screen.height));
		propertySheet.properties.SetVector("_StereoScale", new Vector4(1f, 1f, 0f, 0f));
		propertySheet.properties.SetFloat("_Exposure", base.settings.Exposure.value);
		propertySheet.properties.SetFloat("_LerpAmount", base.settings.BlendAmount.value);
		if (base.settings.MaskTexture.value != null)
		{
			propertySheet.properties.SetTexture("_MaskTex", base.settings.MaskTexture.value);
		}
		if (!flag4)
		{
			if (flag8 || flag10)
			{
				RenderTextureDescriptor desc = new RenderTextureDescriptor(1024, 32, RenderTextureFormat.ARGB32);
				desc.useMipMap = false;
				blendCacheLut = RenderTexture.GetTemporary(desc);
				blendCacheLut.name = "BlendCacheLut";
				blendCacheLut.wrapMode = TextureWrapMode.Clamp;
				blendCacheLut.anisoLevel = 0;
				blendCacheLutHighlight = RenderTexture.GetTemporary(desc);
				blendCacheLutHighlight.name = "BlendCacheLutHighlight";
				blendCacheLutHighlight.wrapMode = TextureWrapMode.Clamp;
				blendCacheLutHighlight.anisoLevel = 0;
				materialBlendCache.SetFloat("_LerpAmount", base.settings.BlendAmount.value);
				if (flag8)
				{
					materialBlendCache.SetVector("_StereoScale", new Vector4(1f, 1f, 0f, 0f));
					materialBlendCache.SetTexture("_RgbTex", texture);
					materialBlendCache.SetTexture("_LerpRgbTex", (value != null) ? value : defaultLut);
					Graphics.Blit(texture, blendCacheLut, materialBlendCache);
					propertySheet.properties.SetTexture("_RgbBlendCacheTex", blendCacheLut);
				}
				if (flag10)
				{
					materialBlendCache.SetVector("_StereoScale", new Vector4(1f, 1f, 0f, 0f));
					materialBlendCache.SetTexture("_RgbTex", texture2);
					materialBlendCache.SetTexture("_LerpRgbTex", (value2 != null) ? value2 : defaultLut);
					Graphics.Blit(texture, blendCacheLutHighlight, materialBlendCache);
					propertySheet.properties.SetTexture("_HighlightBlendCacheTex", blendCacheLutHighlight);
				}
			}
			else
			{
				if (texture != null)
				{
					propertySheet.properties.SetTexture("_RgbTex", texture);
				}
				if (value != null)
				{
					propertySheet.properties.SetTexture("_LerpRgbTex", value);
				}
				if (texture2 != null)
				{
					propertySheet.properties.SetTexture("_HighlightTex", texture2);
				}
				if (value2 != null)
				{
					propertySheet.properties.SetTexture("_LerpHighlightTex", value2);
				}
			}
		}
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, num2);
		if (flag8)
		{
			RenderTexture.ReleaseTemporary(blendCacheLut);
		}
		if (flag10)
		{
			RenderTexture.ReleaseTemporary(blendCacheLutHighlight);
		}
	}
}
