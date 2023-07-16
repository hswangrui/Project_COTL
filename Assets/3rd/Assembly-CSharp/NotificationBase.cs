using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

public abstract class NotificationBase : MonoBehaviour
{
	public enum Flair
	{
		None,
		Positive,
		Negative
	}

	[SerializeField]
	protected RectTransform _rectTransform;

	[SerializeField]
	protected RectTransform _contentRectTransform;

	[SerializeField]
	protected TextMeshProUGUI _description;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private GameObject _positiveFlair;

	[SerializeField]
	private GameObject _negativeFlair;

	protected bool Shown;

	protected Vector2 _onScreenPosition = new Vector2(0f, 0f);

	protected Vector2 _offScreenPosition = new Vector2(-800f, 0f);

	protected abstract float _onScreenDuration { get; }

	protected abstract float _showHideDuration { get; }

	protected virtual void Configure(Flair flair)
	{
		_contentRectTransform.anchoredPosition = _offScreenPosition;
		Localize();
		Show();
		_positiveFlair.SetActive(flair == Flair.Positive);
		_negativeFlair.SetActive(flair == Flair.Negative);
	}

	protected virtual void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += Localize;
	}

	protected void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= Localize;
	}

	protected abstract void Localize();

	protected void Show(bool instant = false, Action andThen = null)
	{
		StopAllCoroutines();
		if (instant)
		{
			_contentRectTransform.anchoredPosition = _onScreenPosition;
			_canvasGroup.alpha = 1f;
			Shown = true;
		}
		else
		{
			StartCoroutine(DoShow(andThen));
		}
	}

	protected virtual IEnumerator DoShow(Action andThen = null)
	{
		Shown = true;
		_contentRectTransform.DOKill();
		_canvasGroup.DOKill();
		_contentRectTransform.DOAnchorPosX(_onScreenPosition.x, _showHideDuration).SetEase(Ease.OutBack);
		_canvasGroup.DOFade(1f, _showHideDuration * 0.5f);
		yield return new WaitForSeconds(0.5f);
		if (andThen != null)
		{
			andThen();
		}
		yield return HoldOnScreen();
		Hide();
	}

	protected virtual IEnumerator HoldOnScreen()
	{
		float timer = _onScreenDuration;
		while (timer > 0f)
		{
			if (HUD_Manager.Instance != null && !HUD_Manager.Instance.Hidden && !LetterBox.IsPlaying)
			{
				timer -= Time.deltaTime;
			}
			yield return null;
		}
	}

	protected void Hide(bool instant = false, Action andThen = null)
	{
		StopAllCoroutines();
		if (instant)
		{
			_contentRectTransform.anchoredPosition = _offScreenPosition;
			_canvasGroup.alpha = 0f;
			Shown = false;
			if (andThen != null)
			{
				andThen();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			StartCoroutine(DoHide());
		}
	}

	protected virtual IEnumerator DoHide(Action andThen = null)
	{
		Shown = false;
		_contentRectTransform.DOKill();
		_canvasGroup.DOKill();
		_contentRectTransform.DOAnchorPosX(_offScreenPosition.x, _showHideDuration).SetEase(Ease.InBack);
		_canvasGroup.DOFade(0f, _showHideDuration * 0.5f).SetDelay(_showHideDuration * 0.5f);
		yield return new WaitForSeconds(0.5f);
		if (andThen != null)
		{
			andThen();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected virtual void OnDestroy()
	{
		NotificationCentre.Notifications.Remove(this);
	}
}
