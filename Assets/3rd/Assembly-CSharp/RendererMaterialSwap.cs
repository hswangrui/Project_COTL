using System;
using UnityEngine;

public class RendererMaterialSwap : BaseMonoBehaviour
{
	[Serializable]
	private struct RendererMaterialPair
	{
		public Renderer targetRenderer;

		public Material curMaterial;

		public Material newMaterial;
	}

	[SerializeField]
	private RendererMaterialPair[] rendMatPairs;

	public void SwapAll()
	{
		for (int i = 0; i < rendMatPairs.Length; i++)
		{
			if (rendMatPairs[i].targetRenderer != null && rendMatPairs[i].newMaterial != null)
			{
				rendMatPairs[i].targetRenderer.gameObject.SetActive(true);
				rendMatPairs[i].curMaterial = rendMatPairs[i].targetRenderer.sharedMaterial;
				rendMatPairs[i].targetRenderer.sharedMaterial = rendMatPairs[i].newMaterial;
				rendMatPairs[i].newMaterial = rendMatPairs[i].curMaterial;
				rendMatPairs[i].curMaterial = rendMatPairs[i].targetRenderer.sharedMaterial;
			}
		}
	}

	public void DisableAll()
	{
		for (int i = 0; i < rendMatPairs.Length; i++)
		{
			if (rendMatPairs[i].targetRenderer != null && rendMatPairs[i].newMaterial != null)
			{
				rendMatPairs[i].targetRenderer.gameObject.SetActive(false);
			}
		}
	}
}
