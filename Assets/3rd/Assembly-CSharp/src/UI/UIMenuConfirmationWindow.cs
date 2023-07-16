using System;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI
{
	public class UIMenuConfirmationWindow : UIMenuBase
	{
		public new Action OnCancel;

		public Action OnConfirm;

		[Header("Buttons")]
		[SerializeField]
		private Button _cancelButton;

		[SerializeField]
		private Button _confirmButton;

		[Header("Text")]
		[SerializeField]
		protected TextMeshProUGUI _headerText;

		[SerializeField]
		protected TextMeshProUGUI _bodyText;

		[Header("Misc")]
		[SerializeField]
		private UINavigatorFollowElement _buttonHighlight;

		public TextAlignmentOptions HeaderAlignment
		{
			get
			{
				return _headerText.alignment;
			}
			set
			{
				_headerText.alignment = value;
			}
		}

		public TextAlignmentOptions BodyAlignment
		{
			get
			{
				return _bodyText.alignment;
			}
			set
			{
				_bodyText.alignment = value;
			}
		}

		public override void Awake()
		{
			base.Awake();
			SetActiveStateForMenu(false);
		}

		public void Configure(string header, string body, bool acceptOnly = false)
		{
			_headerText.text = header;
			_bodyText.text = body;
			if (acceptOnly)
			{
				_cancelButton.gameObject.SetActive(false);
				OverrideDefault(_confirmButton);
			}
		}

		private void Start()
		{
			_cancelButton.onClick.AddListener(OnCancelClicked);
			_confirmButton.onClick.AddListener(OnConfirmClicked);
		}

		protected virtual void OnCancelClicked()
		{
			Action onCancel = OnCancel;
			if (onCancel != null)
			{
				onCancel();
			}
			Hide();
		}

		protected virtual void OnConfirmClicked()
		{
			Hide();
			Action onConfirm = OnConfirm;
			if (onConfirm != null)
			{
				onConfirm();
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				OnCancelClicked();
			}
		}

		protected override void OnShowStarted()
		{
			_buttonHighlight.enabled = true;
		}

		protected override void OnHideStarted()
		{
			_buttonHighlight.enabled = false;
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
