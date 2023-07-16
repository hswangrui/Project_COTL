using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraControls : BaseMonoBehaviour
{
	public AmplifyColorEffect lightingColor;

	public AmplifyColorEffect shadowColor;

	public Material material;

	public RenderTexture renderTexture1;

	public RenderTexture renderTexture2;

	public Material AddDepthMaterial;

	public bool invertTexture = true;

	public bool addDepth = true;

	private void OnEnable()
	{
		if ((bool)Camera.main)
		{
			if (addDepth)
			{
				Camera.main.depthTextureMode |= DepthTextureMode.Depth;
			}
			else
			{
				Camera.main.depthTextureMode = DepthTextureMode.None;
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)Camera.main)
		{
			Camera.main.depthTextureMode = DepthTextureMode.None;
		}
	}

	private void OnPreRender()
	{
		if (invertTexture && material != null)
		{
			material.SetTexture("_RenderTex_1", renderTexture1);
			material.SetTexture("_RenderTex_2", renderTexture2);
			Graphics.Blit(renderTexture2, renderTexture2, material, -1);
		}
	}

	private void Start()
	{
		StartCoroutine(FindPlayer());
	}

	private void Update()
	{
	}

	private IEnumerator FindPlayer()
	{
		if (SceneManager.GetActiveScene().name == "Game - Possess Followers")
		{
			while (PlayerSpirit.Instance == null)
			{
				yield return new WaitForSeconds(1f);
			}
		}
		else
		{
			while (PlayerFarming.Instance == null)
			{
				yield return new WaitForSeconds(1f);
			}
		}
	}
}
