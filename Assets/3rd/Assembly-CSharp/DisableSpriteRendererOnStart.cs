using UnityEngine;

public class DisableSpriteRendererOnStart : BaseMonoBehaviour
{
	public SpriteRenderer spriterenderer;

	public bool grabSpriteRenderer;

	private void Start()
	{
		if (grabSpriteRenderer)
		{
			spriterenderer = base.gameObject.GetComponent<SpriteRenderer>();
		}
		spriterenderer.enabled = false;
	}
}
