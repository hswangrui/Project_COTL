using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

public abstract class NotificationDynamicBase : MonoBehaviour
{
	private const float kDuration = 3f;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private MMSelectable _selectable;

	[SerializeField]
	private Image _progressBar;

	[SerializeField]
	private GameObject _extraWarningSignal;

	private DynamicNotificationData _data;

	private float _progress;

	private bool _enabledWarning;

	private Vector3 _punchAmount = new Vector3(0.1f, 0.1f, 0.1f);

	private CanvasGroup _canvasGroup;

	public DynamicNotificationData Data
	{
		get
		{
			return _data;
		}
	}

	public RectTransform Container
	{
		get
		{
			return _container;
		}
	}

	public MMSelectable Selectable
	{
		get
		{
			return _selectable;
		}
	}

	public bool Closing { get; private set; }

	public abstract Color FullColour { get; }

	public abstract Color EmptyColour { get; }

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	public virtual void Configure(DynamicNotificationData data)
	{
		_data = data;
		DynamicNotificationData data2 = _data;
		data2.DataChanged = (Action)Delegate.Combine(data2.DataChanged, new Action(OnDataChanged));
		OnDataChanged();
		Show();
	}

	private void OnEnable()
	{
		_extraWarningSignal.SetActive(false);
		if (_data != null)
		{
			DynamicNotificationData data = _data;
			data.DataChanged = (Action)Delegate.Combine(data.DataChanged, new Action(OnDataChanged));
		}
	}

	private void OnDisable()
	{
		if (_data != null)
		{
			DynamicNotificationData data = _data;
			data.DataChanged = (Action)Delegate.Remove(data.DataChanged, new Action(OnDataChanged));
		}
	}

	private void Update()
	{
		_progressBar.fillAmount = Data.CurrentProgress;
		_progressBar.color = GetColor(Data.CurrentProgress);
		if (Data.CurrentProgress > 0.5f)
		{
			if ((_progress += Time.deltaTime) > 3f)
			{
				if (!_enabledWarning)
				{
					_extraWarningSignal.SetActive(true);
					_enabledWarning = true;
				}
				_container.DOKill();
				_container.DOPunchScale(_punchAmount, 1f);
				_progress = 0f;
			}
		}
		else if (_enabledWarning)
		{
			_extraWarningSignal.SetActive(false);
			_enabledWarning = false;
		}
		if (LetterBox.IsPlaying && _canvasGroup.alpha == 1f)
		{
			_canvasGroup.DOKill();
			_canvasGroup.DOFade(0f, 1f);
		}
		else if (!LetterBox.IsPlaying && _canvasGroup.alpha == 0f)
		{
			_canvasGroup.DOKill();
			_canvasGroup.DOFade(1f, 1f);
		}
	}

	protected virtual Color GetColor(float norm)
	{
		return Color.Lerp(EmptyColour, FullColour, norm);
	}

	private void OnDataChanged()
	{
		if (!Data.IsEmpty)
		{
			UpdateIcon();
			return;
		}
		UpdateIcon();
		if (!Closing)
		{
			Hide();
		}
	}

	protected abstract void UpdateIcon();

	protected virtual void Show()
	{
		StartCoroutine(DoShow());
	}

	private IEnumerator DoShow()
	{
		float progress = 0f;
		float duration = 0.3f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime);
			if (!(num < duration))
			{
				break;
			}
			_container.localScale = Vector3.one * Mathf.SmoothStep(2f, 1f, progress / duration);
			yield return null;
		}
		_container.localScale = Vector3.one;
	}

	protected virtual void Hide()
	{
		Closing = true;
		StartCoroutine(DoHide());
	}

	private IEnumerator DoHide()
	{
		Closing = true;
		float progress = 0f;
		float duration = 0.5f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime);
			if (!(num < duration))
			{
				break;
			}
			_container.localScale = Vector3.one * Mathf.SmoothStep(1f, 0f, progress / duration);
			yield return null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected virtual void OnDestroy()
	{
		UIDynamicNotificationCenter.NotificationsDynamic.Remove(this);
	}
}
