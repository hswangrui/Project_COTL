using System;
using DG.Tweening;
using UnityEngine;

namespace Lamb.UI
{
	public class MMToggle : MonoBehaviour
	{
		public Action<bool> OnValueChanged;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private RectTransform _handle;

		[SerializeField]
		private RectTransform _onPosition;

		[SerializeField]
		private RectTransform _offPosition;

		[SerializeField]
		private CanvasGroup _onGraphic;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		private bool _value = true;

		public bool Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value != _value)
				{
					_value = value;
					UpdateState();
				}
			}
		}

		public bool Interactable
		{
			get
			{
				return _button.Interactable;
			}
			set
			{
				if (_button.Interactable != value)
				{
					_button.Interactable = value;
					_canvasGroup.alpha = (value ? 1f : 0.5f);
				}
			}
		}

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public CanvasGroup CanvasGroup
		{
			get
			{
				return _canvasGroup;
			}
		}

		private void Awake()
		{
			_button.onClick.AddListener(DoToggle);
		}

		private void DoToggle()
		{
			_value = !_value;
			Action<bool> onValueChanged = OnValueChanged;
			if (onValueChanged != null)
			{
				onValueChanged(_value);
			}
			UpdateState();
		}

		public void Toggle()
		{
			DoToggle();
		}

		private void UpdateState(bool instant = false)
		{
			_handle.DOKill();
			_onGraphic.DOKill();
			if (!instant)
			{
				_handle.DOAnchorPos(_value ? _onPosition.anchoredPosition : _offPosition.anchoredPosition, 0.15f).SetEase(Ease.OutSine).SetUpdate(true);
				_onGraphic.DOFade(_value.ToInt(), 0.15f).SetEase(Ease.OutSine).SetUpdate(true);
			}
			else
			{
				_handle.anchoredPosition = (_value ? _onPosition.anchoredPosition : _offPosition.anchoredPosition);
				_onGraphic.alpha = _value.ToInt();
			}
		}
	}
}
