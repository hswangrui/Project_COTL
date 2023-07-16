using UnityEngine;

[ExecuteInEditMode]
public class SetSortingLayer_ : BaseMonoBehaviour
{
	public Renderer MyRenderer;

	public string MySortingLayer;

	public int MySortingOrderInLayer;

	private void Start()
	{
		if (MyRenderer == null)
		{
			MyRenderer = GetComponent<Renderer>();
		}
		SetLayer();
	}

	public void SetLayer()
	{
		if (MyRenderer == null)
		{
			MyRenderer = GetComponent<Renderer>();
		}
		MyRenderer.sortingLayerName = MySortingLayer;
		MyRenderer.sortingOrder = MySortingOrderInLayer;
	}
}
