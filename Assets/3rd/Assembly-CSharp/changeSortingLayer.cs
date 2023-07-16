using UnityEngine;

public class changeSortingLayer : BaseMonoBehaviour
{
	public SpriteRenderer sprite;

	public string sortingLayer;

	private void Start()
	{
		sprite.sortingLayerName = sortingLayer;
	}
}
