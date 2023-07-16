using System.Collections;
using UnityEngine;

public class MooshTextures : BaseMonoBehaviour
{
	public Material material;

	public float lerpValue = 0.5f;

	public RenderTexture renderTextureA;

	public RenderTexture renderTextureB;

	public RenderTexture renderTextureC;

	public AmplifyColorBase amplifoo;

	private IEnumerator Start()
	{
		while (renderTextureA == null)
		{
			renderTextureA = (RenderTexture)amplifoo.MaskTexture;
			yield return null;
		}
		renderTextureA = (RenderTexture)amplifoo.MaskTexture;
		material.SetTexture("_RenderTex_1", renderTextureA);
		material.SetTexture("_RenderTex_2", renderTextureB);
		material.SetFloat("_Lerp_Fade_1", lerpValue);
		yield return null;
	}

	private void OnPreRender()
	{
		if (renderTextureA == null)
		{
			renderTextureA = (RenderTexture)amplifoo.MaskTexture;
			material.SetTexture("_RenderTex_1", renderTextureA);
		}
		material.SetFloat("_Lerp_Fade_1", lerpValue);
		Graphics.Blit(renderTextureC, renderTextureC, material, -1);
	}
}
