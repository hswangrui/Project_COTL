using UnityEngine;

[ExecuteInEditMode]
public class ShaderToCamera : BaseMonoBehaviour
{
	public Material material;

	[Range(0f, 1f)]
	public float materialFloat = 1f;

	public string materialString;

	private void Update()
	{
		if (materialString != "")
		{
			material.SetFloat(materialString, materialFloat);
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (material != null)
		{
			Graphics.Blit(sourceTexture, destTexture, material);
		}
	}
}
