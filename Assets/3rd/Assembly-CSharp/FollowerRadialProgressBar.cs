using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerRadialProgressBar : BaseMonoBehaviour
{
	private bool _shown;

	public SpriteRenderer RadialProgress;

	public SpriteRenderer RadialProgressFlashWhite;

	public List<GameObject> Images = new List<GameObject>();

	public Follower follower;

	private Coroutine cFlashRoutine;

	private void Awake()
	{
		RadialProgress.material.SetFloat("_Angle", 90f);
		SetVisibility();
	}

	public void Hide()
	{
		_shown = false;
		SetVisibility();
	}

	public void Show()
	{
		_shown = true;
		SetVisibility();
	}

	private void SetVisibility()
	{
		RadialProgress.gameObject.SetActive(_shown);
		foreach (GameObject image in Images)
		{
			image.SetActive(_shown);
		}
		RadialProgressFlashWhite.color = new Color(1f, 1f, 1f, 0f);
	}

	public void UpdateBar(float normalisedValue)
	{
		if (follower.Brain.Info != null && follower.Brain.CurrentTask != null)
		{
			RadialProgress.material.SetFloat("_Arc1", 360f - normalisedValue * 360f);
			RadialProgress.material.SetFloat("_Arc2", 0f);
		}
	}

	public void Flash()
	{
		if (cFlashRoutine != null)
		{
			StopCoroutine(cFlashRoutine);
		}
		cFlashRoutine = StartCoroutine(FlashRoutine());
	}

	private IEnumerator FlashRoutine()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		Color Transparent = new Color(1f, 1f, 1f, 0f);
		RadialProgressFlashWhite.color = Color.white;
		yield return new WaitForSeconds(0.2f);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				RadialProgressFlashWhite.color = Color.Lerp(Color.white, Transparent, Progress / Duration);
				yield return null;
				continue;
			}
			break;
		}
	}
}
