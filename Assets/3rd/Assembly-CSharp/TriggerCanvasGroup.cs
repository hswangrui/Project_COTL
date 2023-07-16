using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TriggerCanvasGroup : BaseMonoBehaviour
{
	public enum Mode
	{
		Timer,
		Enable,
		Disable
	}

	public delegate void Triggered();

	public Mode CurrentMode;

	public float FadeIn = 1f;

	public float FadeOut = 1f;

	public float WaitDuration = 5f;

	private bool Activated;

	public CanvasGroup CanvasGroup;

	public bool StartHidden = true;

	public Triggered OnTriggered;

	private void Start()
	{
		if (StartHidden && CanvasGroup != null)
		{
			CanvasGroup.alpha = 0f;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!Activated && collision.gameObject.tag == "Player")
		{
			Activated = true;
			Play();
		}
	}

	public void Play()
	{
		Triggered onTriggered = OnTriggered;
		if (onTriggered != null)
		{
			onTriggered();
		}
		if (!(CanvasGroup == null))
		{
			switch (CurrentMode)
			{
			case Mode.Timer:
				StartCoroutine(DoRoutine());
				break;
			case Mode.Enable:
				CanvasGroup.DOFade(1f, FadeIn);
				break;
			case Mode.Disable:
				CanvasGroup.DOFade(0f, FadeIn);
				break;
			}
		}
	}

	private IEnumerator EnableRoutine()
	{
		float Progress = 0f;
		float Duration = FadeIn;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			CanvasGroup.alpha = Mathf.Lerp(0f, 1f, Progress / Duration);
			yield return null;
		}
		CanvasGroup.alpha = 1f;
	}

	private IEnumerator DisableRoutine()
	{
		float Progress = 0f;
		float Duration = FadeOut;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			CanvasGroup.alpha = Mathf.Lerp(1f, 0f, Progress / Duration);
			yield return null;
		}
		CanvasGroup.alpha = 0f;
	}

	private IEnumerator DoRoutine()
	{
		CanvasGroup.DOFade(1f, FadeIn);
		yield return new WaitForSeconds(WaitDuration);
		CanvasGroup.DOFade(0f, FadeIn);
	}
}
