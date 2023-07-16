using System.Collections.Generic;
using UnityEngine;

public class CameraDistanceAlpha : BaseMonoBehaviour
{
	public List<SpriteRenderer> Renderers = new List<SpriteRenderer>();

	public float Multiplier = 1f;

	private Color color;

	private void Update()
	{
		if (Camera.main == null)
		{
			return;
		}
		foreach (SpriteRenderer renderer in Renderers)
		{
			color = renderer.color;
			color.a = Mathf.Lerp(0f, 1f, Mathf.Abs(Camera.main.transform.position.x - renderer.transform.position.x) * Multiplier);
			renderer.color = color;
		}
	}
}
