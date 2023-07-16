using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class HideSaveicon : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	public static HideSaveicon instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		base.gameObject.SetActive(false);
	}

	public void StartRoutineSave(Action callback)
	{
		base.gameObject.SetActive(true);
		_canvasGroup.DOKill();
		_canvasGroup.alpha = 1f;
		StopAllCoroutines();
		StartCoroutine(Saving(callback));
	}

	private IEnumerator Saving(Action callback)
	{
		yield return new WaitForEndOfFrame();
		callback();
		yield return new WaitForSecondsRealtime(0.25f);
		_canvasGroup.DOFade(0f, 1f).SetUpdate(true);
		yield return new WaitForSecondsRealtime(1f);
		base.gameObject.SetActive(false);
	}
}
