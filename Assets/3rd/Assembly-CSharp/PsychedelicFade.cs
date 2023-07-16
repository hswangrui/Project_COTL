using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PsychedelicFade : BaseMonoBehaviour
{
	public Image image;

	private float Duration = 2f;

	private float Delay;

	private bool UseDeltaTime = true;

	public void FadeIn(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true, Action onComplete = null)
	{
		image.color = new Color(0f, 0f, 0f, 0f);
		image.enabled = true;
		image.DOKill();
		image.DOFade(1f, _Duration).SetUpdate(UseDeltaTime).SetDelay(_Delay)
			.OnComplete(delegate
			{
				Action action = onComplete;
				if (action != null)
				{
					action();
				}
			});
	}

	public void FadeOut(float _Duration = 2f, float _Delay = 0f, bool UseDeltaTime = true, Action onComplete = null)
	{
		image.enabled = true;
		image.DOKill();
		image.DOFade(0f, _Duration).SetUpdate(UseDeltaTime).SetDelay(_Delay)
			.OnComplete(delegate
			{
				Action action = onComplete;
				if (action != null)
				{
					action();
				}
				base.gameObject.SetActive(false);
			});
	}

	private void OnEnable()
	{
		image.enabled = false;
	}
}
