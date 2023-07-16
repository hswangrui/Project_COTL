using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GrowAndFade : BaseMonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public Image image;

	public Color StartColor;

	public Color TargetColor;

	public float ScaleSpeed = 1f;

	public float Duration = 1f;

	private void Start()
	{
		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = false;
		}
		if (image != null)
		{
			image.enabled = false;
		}
	}

	public void Play()
	{
		StartCoroutine(PlayRoutine());
	}

	private IEnumerator PlayRoutine()
	{
		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = true;
		}
		if (image != null)
		{
			image.enabled = true;
		}
		float Progress = 0f;
		float Scale = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Color color = Color.Lerp(StartColor, TargetColor, Progress / Duration);
			if (spriteRenderer != null)
			{
				spriteRenderer.color = color;
			}
			if (image != null)
			{
				image.color = color;
			}
			Scale += Time.deltaTime * ScaleSpeed;
			base.transform.localScale = Vector3.one * Scale;
			yield return null;
		}
		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = false;
		}
		if (image != null)
		{
			image.enabled = false;
		}
	}
}
