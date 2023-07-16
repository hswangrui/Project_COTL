using UnityEngine;

public class invertTextures : BaseMonoBehaviour
{
	public Material material;

	public RenderTexture renderTexture1;

	public RenderTexture renderTexture2;

	private void OnPreRender()
	{
		material.SetTexture("_RenderTex_1", renderTexture1);
		material.SetTexture("_RenderTex_2", renderTexture2);
		Graphics.Blit(renderTexture2, renderTexture2, material, -1);
	}
}
