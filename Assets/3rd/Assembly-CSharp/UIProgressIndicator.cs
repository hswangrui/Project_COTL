using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIProgressIndicator : MonoBehaviour, IPoolListener
{
	public Action OnHidden;

	[SerializeField]
	private SpriteRenderer _iconEffective;

	[SerializeField]
	private SpriteRenderer _iconIneffective;

	[SerializeField]
	private SpriteRenderer _progressBar;

	[SerializeField]
	private SpriteRenderer _background;

	private static readonly int _radialProgress = Shader.PropertyToID("_Progress");

	private Material _radialMaterial;

	private bool _showing;

	private bool _hiding;

	private Coroutine _showHideCoroutine;

	private Coroutine _progressCoroutine;

	private Color _invisible = new Color(1f, 1f, 1f, 0f);

	private Color _visible = Color.white;

	private float _ineffectiveTimestamp;

	public static List<UIProgressIndicator> ProgressIndicators { get; private set; } = new List<UIProgressIndicator>();


	private void Awake()
	{
		if (_radialMaterial == null)
		{
			_radialMaterial = new Material(_progressBar.material);
			_progressBar.material = _radialMaterial;
		}
		_iconIneffective.gameObject.SetActive(false);
		_iconEffective.color = _invisible;
		_iconIneffective.color = _invisible;
		_progressBar.color = _invisible;
		_background.color = _invisible;
	}

	private void OnEnable()
	{
		base.transform.localScale = Vector3.one;
		ProgressIndicators.Add(this);
	}

	private void OnDisable()
	{
		ProgressIndicators.Remove(this);
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(_radialMaterial);
		_radialMaterial = null;
	}

	public void Show(float progress, float duration = 0.5f)
	{
		if (!_showing && !LetterBox.IsPlaying)
		{
			_hiding = false;
			if (_showHideCoroutine != null)
			{
				StopCoroutine(_showHideCoroutine);
			}
			_showHideCoroutine = StartCoroutine(DoShow(duration));
			SetProgress(progress, 0f);
		}
	}

	private IEnumerator DoShow(float duration)
	{
		_showing = true;
		KillAllTweens();
		_iconEffective.DOColor(_visible, duration);
		_iconIneffective.DOColor(_visible, duration);
		_progressBar.DOColor(_visible, duration);
		_background.DOColor(_visible, duration);
		yield return new WaitForSeconds(duration);
		_showing = false;
	}

	public void Hide(float duration = 0.5f, float delay = 0.1f)
	{
		if (!_hiding)
		{
			_showing = false;
			if (_showHideCoroutine != null)
			{
				StopCoroutine(_showHideCoroutine);
			}
			_showHideCoroutine = StartCoroutine(DoHide(duration, delay));
		}
	}

	private IEnumerator DoHide(float duration, float delay)
	{
		_hiding = true;
		yield return new WaitForSeconds(delay);
		KillAllTweens();
		_iconEffective.DOColor(_invisible, duration).SetUpdate(true);
		_iconIneffective.DOColor(_invisible, duration).SetUpdate(true);
		_progressBar.DOColor(_invisible, duration).SetUpdate(true);
		_background.DOColor(_invisible, duration).SetUpdate(true);
		yield return new WaitForSecondsRealtime(duration);
		_hiding = false;
		Action onHidden = OnHidden;
		if (onHidden != null)
		{
			onHidden();
		}
		base.gameObject.Recycle();
	}

	public void SetProgress(float progress, float duration = 0.33f, bool ineffective = false)
	{
		if (_progressCoroutine != null)
		{
			StopCoroutine(_progressCoroutine);
		}
		_progressCoroutine = StartCoroutine(DoSetProgress(progress, duration));
		if (ineffective)
		{
			_iconEffective.gameObject.SetActive(false);
			_iconIneffective.gameObject.SetActive(true);
			_ineffectiveTimestamp = Time.time;
		}
		else if (Time.time - _ineffectiveTimestamp > 1f)
		{
			_iconEffective.gameObject.SetActive(true);
			_iconIneffective.gameObject.SetActive(false);
		}
	}

	private IEnumerator DoSetProgress(float progress, float duration)
	{
		if (_hiding)
		{
			Show(progress);
		}
		_radialMaterial.DOKill();
		if (duration <= 0f)
		{
			SetRadialWheelProgress(progress);
		}
		else
		{
			_radialMaterial.DOFloat(progress, _radialProgress, duration).SetEase(Ease.OutQuart).OnKill(delegate
			{
				SetRadialWheelProgress(progress);
			});
		}
		yield return new WaitForSeconds(duration);
		yield return new WaitForSeconds(5f);
		Hide();
	}

	private void LateUpdate()
	{
		if (LetterBox.IsPlaying)
		{
			Hide(0f);
		}
	}

	private void SetRadialWheelProgress(float progress)
	{
		_radialMaterial.SetFloat(_radialProgress, progress);
	}

	private void KillAllTweens()
	{
		_iconEffective.DOKill();
		_iconIneffective.DOKill();
		_progressBar.DOKill();
		_background.DOKill();
	}

	public void OnRecycled()
	{
		_showing = false;
		_hiding = false;
		KillAllTweens();
		_iconEffective.color = _invisible;
		_iconIneffective.color = _invisible;
		_progressBar.color = _invisible;
		_background.color = _invisible;
		_radialMaterial.DOKill();
		SetRadialWheelProgress(0f);
		StopAllCoroutines();
		_showHideCoroutine = null;
		_progressCoroutine = null;
		_iconEffective.gameObject.SetActive(true);
		_iconIneffective.gameObject.SetActive(false);
	}
}
