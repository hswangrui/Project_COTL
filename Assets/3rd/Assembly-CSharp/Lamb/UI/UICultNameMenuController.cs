using System;
using DG.Tweening;
using I2.Loc;
using src.UI;
using UnityEngine;

namespace Lamb.UI
{
	public class UICultNameMenuController : UIMenuBase
	{
		public Action<string> OnNameConfirmed;

		[Header("Cult Name Menu")]
		[SerializeField]
		protected MMInputField _nameInputField;

		[SerializeField]
		private MMButton _confirmButton;

		[SerializeField]
		private RectTransform _confirmButtonRectTransform;

		[SerializeField]
		private RectTransform _emptyTextTransform;

		[SerializeField]
		private ButtonHighlightController _buttonHighlight;

		[SerializeField]
		private RectTransform _buttonHighlightRectTransform;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private GameObject _renameDisclaimer;

		private bool _editing;

		private Vector2 _buttonHighlightSizeDelta;

		private Vector3 _confirmButtonOrigin;

		private Vector3 _emptyTextOrigin;

		private bool _cancellable;

		public override void Awake()
		{
			base.Awake();
			_buttonHighlight.SetAsRed();
			_nameInputField.characterLimit = 16;
			_buttonHighlightSizeDelta = _buttonHighlightRectTransform.sizeDelta;
			_nameInputField.text = LocalizationManager.GetTranslation("NAMES/Place/Cult");
			MMInputField nameInputField = _nameInputField;
			nameInputField.OnEndedEditing = (Action<string>)Delegate.Combine(nameInputField.OnEndedEditing, new Action<string>(OnEndedEditing));
			MMInputField nameInputField2 = _nameInputField;
			nameInputField2.OnStartedEditing = (Action)Delegate.Combine(nameInputField2.OnStartedEditing, new Action(OnStartedEditing));
			MMInputField nameInputField3 = _nameInputField;
			nameInputField3.OnSelected = (Action)Delegate.Combine(nameInputField3.OnSelected, new Action(OnNameInputSelected));
			_confirmButton.onClick.AddListener(OnConfirmButtonClicked);
			MMButton confirmButton = _confirmButton;
			confirmButton.OnConfirmDenied = (Action)Delegate.Combine(confirmButton.OnConfirmDenied, new Action(ShakeConfirmButton));
			MMButton confirmButton2 = _confirmButton;
			confirmButton2.OnSelected = (Action)Delegate.Combine(confirmButton2.OnSelected, new Action(OnConfirmButtonSelected));
			_confirmButtonOrigin = _confirmButtonRectTransform.anchoredPosition;
			_emptyTextTransform.gameObject.SetActive(false);
			_emptyTextOrigin = _emptyTextTransform.anchoredPosition;
		}

		public void Show(string prefillText, bool cancellable, bool showDisclaimer)
		{
			_nameInputField.text = prefillText;
			Show(cancellable, showDisclaimer);
		}

		public void Show(bool cancellable, bool showDisclaimer)
		{
			_cancellable = cancellable;
			if (!_cancellable)
			{
				_controlPrompts.HideCancelButton();
			}
			_renameDisclaimer.SetActive(showDisclaimer);
			Show();
		}

		protected override void OnShowCompleted()
		{
			base.OnShowCompleted();
			Time.timeScale = 0f;
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			Time.timeScale = 1f;
		}

		private void OnNameInputSelected()
		{
			_buttonHighlightRectTransform.sizeDelta = _buttonHighlightSizeDelta;
		}

		private void OnStartedEditing()
		{
			_emptyTextTransform.gameObject.SetActive(false);
		}

		private void OnEndedEditing(string text)
		{
			_confirmButton.Confirmable = !string.IsNullOrWhiteSpace(text);
			_emptyTextTransform.gameObject.SetActive(!_confirmButton.Confirmable);
		}

		private void OnConfirmButtonSelected()
		{
			_confirmButton.Confirmable = !string.IsNullOrWhiteSpace(_nameInputField.text);
			_buttonHighlightRectTransform.sizeDelta = new Vector2(320f, 72f);
		}

		private void OnConfirmButtonClicked()
		{
			Action<string> onNameConfirmed = OnNameConfirmed;
			if (onNameConfirmed != null)
			{
				onNameConfirmed(_nameInputField.text);
			}
			Hide();
		}

		private void ShakeConfirmButton()
		{
			_emptyTextTransform.DOKill();
			_confirmButtonRectTransform.DOKill();
			_confirmButtonRectTransform.anchoredPosition = _confirmButtonOrigin;
			_emptyTextTransform.anchoredPosition = _emptyTextOrigin;
			_confirmButtonRectTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
			_emptyTextTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		public override void OnCancelButtonInput()
		{
			base.OnCancelButtonInput();
			if (_canvasGroup.interactable && _cancellable && !_nameInputField.isFocused)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
