using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TargetMarker : BaseMonoBehaviour
{
	private SpriteRenderer spriterenderer;

	private float xScale = 1f;

	private float yScale = 1f;

	private float xScaleSpeed;

	private float yScaleSpeed;

	private float zOffset;

	private void Start()
	{
		spriterenderer = GetComponent<SpriteRenderer>();
		spriterenderer.color -= new Color(0f, 0f, 0f, 1f);
	}

	public void reveal(Vector3 position)
	{
		spriterenderer.color = Color.white;
		xScale = 2f;
		yScale = 1.5f;
		base.transform.position = position;
	}

	private void Update()
	{
		xScale += (1f - xScale) / 10f;
		yScale += (1f - yScale) / 5f;
		Vector3 localScale = new Vector3(xScale, yScale, 1f);
		base.gameObject.transform.localScale = localScale;
		if (spriterenderer.color.a > 0f)
		{
			spriterenderer.color -= new Color(0f, 0f, 0f, 0.05f);
			if (spriterenderer.color.a <= 0f)
			{
				spriterenderer.color -= new Color(1f, 1f, 1f, 0f);
			}
		}
	}
}
