using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildSitePlotProgressUI : BaseMonoBehaviour
{
	public static List<BuildSitePlotProgressUI> BuildSiteProgressUI = new List<BuildSitePlotProgressUI>();

	private float Scale;

	private float ScaleSpeed;

	private Canvas canvas;

	public Vector3 Offset;

	private float TargetScale = 0.5f;

	public CanvasGroup canvasGroup;

	private Camera camera;

	private Coroutine cFadeOutRoutine;

	public Image image;

	private void OnEnable()
	{
		canvas = GetComponentInParent<Canvas>();
		camera = Camera.main;
		BuildSiteProgressUI.Add(this);
	}

	private void OnDisable()
	{
		BuildSiteProgressUI.Remove(this);
	}

	public void SetPosition(Vector3 Position)
	{
		base.transform.position = camera.WorldToScreenPoint(Position + Offset);
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
		canvasGroup.alpha = 1f;
		base.transform.SetAsFirstSibling();
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			if (cFadeOutRoutine != null)
			{
				StopCoroutine(cFadeOutRoutine);
			}
			StartCoroutine(FadeOutRoutine());
		}
	}

	private IEnumerator FadeOutRoutine()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			canvasGroup.alpha = 1f - Progress / Duration;
			yield return null;
		}
		base.gameObject.SetActive(false);
	}

	private void LateUpdate()
	{
		base.transform.localScale = Vector3.one * Scale;
	}

	private void FixedUpdate()
	{
		ScaleSpeed += (TargetScale - Scale) * 0.3f;
		Scale += (ScaleSpeed *= 0.8f);
	}

	public void UpdateProgress(float Progress)
	{
		image.fillAmount = Progress;
	}

	public static void HideAll()
	{
		for (int num = BuildSiteProgressUI.Count - 1; num >= 0; num--)
		{
			if (BuildSiteProgressUI[num] != null)
			{
				BuildSiteProgressUI[num].gameObject.SetActive(false);
			}
		}
	}
}
