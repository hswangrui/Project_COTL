using DG.Tweening;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	[RequireComponent(typeof(Image))]
	[RequireComponent(typeof(CanvasGroup))]
	public class InputIdentifier : BaseMonoBehaviour
	{
		[SerializeField]
		private ControllerType _controllerType;

		[SerializeField]
		private Platform _platform;

		[SerializeField]
		[ActionIdProperty(typeof(GamepadTemplate))]
		private int _button;

		[SerializeField]
		private KeyboardKeyCode _keyboardKeyCode;

		[SerializeField]
		private MouseInputElement _mouseInputElement;

		[SerializeField]
		private Image _image;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		public int Button
		{
			get
			{
				return _button;
			}
		}

		public KeyboardKeyCode KeyboardKeyCode
		{
			get
			{
				return _keyboardKeyCode;
			}
		}

		public ControllerType ControllerType
		{
			get
			{
				return _controllerType;
			}
		}

		public MouseInputElement MouseInputElement
		{
			get
			{
				return _mouseInputElement;
			}
		}

		private void OnValidate()
		{
			if (_image == null)
			{
				_image = GetComponent<Image>();
			}
			if (_canvasGroup == null)
			{
				_canvasGroup = GetComponent<CanvasGroup>();
			}
		}

		public void Show(bool instant = false)
		{
			_canvasGroup.DOKill();
			if (instant)
			{
				_canvasGroup.alpha = 1f;
			}
			else
			{
				_canvasGroup.DOFade(1f, 0.1f).SetUpdate(true);
			}
		}

		public void Hide(bool instant = false)
		{
			_canvasGroup.DOKill();
			if (instant)
			{
				_canvasGroup.alpha = 0f;
			}
			else
			{
				_canvasGroup.DOFade(0f, 0.1f).SetUpdate(true);
			}
		}
	}
}
