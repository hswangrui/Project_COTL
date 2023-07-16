using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class StencilLighting_MaskEffect : BaseMonoBehaviour
{
	private Camera LightingCamera;

	private AmplifyPostEffect amplifyPostEffect;

	private PostProcessVolume ppv;

	private Light mainDirLight;

	private string mainDirLightTag = "MainDirLight";

	public static bool DisableRender;

	private static RenderTexture _lightingRenderTexture;

	private Resolution _currentResolution;

	private void Start()
	{
		if (GameObject.FindGameObjectWithTag(mainDirLightTag) != null)
		{
			mainDirLight = GameObject.FindGameObjectWithTag(mainDirLightTag).GetComponent<Light>();
		}
		LightingCamera = Camera.main.transform.GetChild(0).GetComponent<Camera>();
		UpdateRenderTexture();
		if (LightingManager.Instance != null)
		{
			ppv = LightingManager.Instance.ppv;
			ppv.profile.TryGetSettings<AmplifyPostEffect>(out amplifyPostEffect);
			if (!amplifyPostEffect)
			{
				amplifyPostEffect = ppv.profile.AddSettings<AmplifyPostEffect>();
			}
		}
	}

	private void UpdateRenderTexture()
	{
		int width = Screen.width;
		int height = Screen.height;
		if (_currentResolution.width != width || _currentResolution.height != height)
		{
			DestroyRenderTexture();
			_currentResolution.width = width;
			_currentResolution.height = height;
			_lightingRenderTexture = new RenderTexture(new RenderTextureDescriptor(_currentResolution.width, _currentResolution.height, RenderTextureFormat.Default, 16, 0))
			{
				name = "LightingCam_Mask" + Time.frameCount
			};
		}
	}

	public void OnDestroy()
	{
		DestroyRenderTexture();
	}

	private void DestroyRenderTexture()
	{
		if (_lightingRenderTexture != null)
		{
			_lightingRenderTexture.Release();
			_lightingRenderTexture.DiscardContents();
			Object.Destroy(_lightingRenderTexture);
			_lightingRenderTexture = null;
		}
	}

	[ImageEffectOpaque]
	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (DisableRender)
		{
			Graphics.Blit(source, destination);
		}
		else if (!(LightingCamera == null) && !(amplifyPostEffect == null) && !(mainDirLight == null))
		{
			LightingCamera.enabled = false;
			mainDirLight.shadows = LightShadows.None;
			UpdateRenderTexture();
			LightingCamera.targetTexture = _lightingRenderTexture;
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = _lightingRenderTexture;
			GL.Clear(false, true, Color.clear);
			RenderTexture.active = active;
			LightingCamera.SetTargetBuffers(_lightingRenderTexture.colorBuffer, source.depthBuffer);
			LightingCamera.Render();
			amplifyPostEffect.MaskTexture.value = _lightingRenderTexture;
			mainDirLight.shadows = LightShadows.Soft;
			Graphics.Blit(source, destination);
		}
	}
}
