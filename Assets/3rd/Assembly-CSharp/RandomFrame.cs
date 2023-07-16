using System.Collections.Generic;
using UnityEngine;

public class RandomFrame : BaseMonoBehaviour
{
	public List<Sprite> frames = new List<Sprite>();

	public int frame = -1;

	public Material mat;

	public void Start()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (mat != null)
		{
			component.material = mat;
		}
		if (frame == -1)
		{
			component.sprite = frames[Random.Range(0, frames.Count)];
		}
		else
		{
			component.sprite = frames[frame];
		}
	}
}
