using System.Collections;
using UnityEngine;

public class DevotionCounterOverlay : BaseMonoBehaviour
{
	private RectTransform rectTransform;

	private HUD_Souls HUD_Souls;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		HUD_Souls = GetComponent<HUD_Souls>();
		base.gameObject.SetActive(false);
	}

	public void Play()
	{
		base.gameObject.SetActive(true);
		StopAllCoroutines();
		StartCoroutine(PlayRoutine());
	}

	public void Hide()
	{
		StopAllCoroutines();
		StartCoroutine(HideRoutine());
	}

	private IEnumerator PlayRoutine()
	{
		float Duration = 0.5f;
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(0f, -150f), new Vector2(0f, 150f), Mathf.SmoothStep(0f, 1f, Progress / Duration));
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator HideRoutine()
	{
		bool StillCounting = HUD_Souls.CurrentDelta > 0;
		while (HUD_Souls.CurrentDelta > 0)
		{
			yield return null;
		}
		yield return new WaitForSeconds((!StillCounting) ? 2 : 0);
		float Duration = 0.4f;
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(0f, 150f), new Vector2(0f, -150f), Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		base.gameObject.SetActive(false);
	}
}
