using System;
using System.Collections;
using Lamb.UI;
using UnityEngine;

namespace src.UI.Menus
{
	public class KBMainMenu : UISubmenuBase
	{
		public Action OnPlayButtonPressed;

		public Action OnInstructionsButtonPressed;

		public Action OnQuitButtonPressed;

		[Header("Main Menu")]
		[SerializeField]
		private MMButton _playButton;

		[SerializeField]
		private MMButton _instructionsButton;

		[SerializeField]
		private MMButton _quitButton;

		[Header("Misc")]
		[SerializeField]
		private UINavigatorFollowElement _buttonHighlight;

		public void Start()
		{
			_playButton.onClick.AddListener(OnPlayButtonClicked);
			_instructionsButton.onClick.AddListener(OnInstructionsButtonClicked);
			_quitButton.onClick.AddListener(OnQuitButtonClicked);
		}

		private void OnPlayButtonClicked()
		{
			Action onPlayButtonPressed = OnPlayButtonPressed;
			if (onPlayButtonPressed != null)
			{
				onPlayButtonPressed();
			}
		}

		private void OnInstructionsButtonClicked()
		{
			OverrideDefaultOnce(_instructionsButton);
			Action onInstructionsButtonPressed = OnInstructionsButtonPressed;
			if (onInstructionsButtonPressed != null)
			{
				onInstructionsButtonPressed();
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_parent.CanvasGroup.interactable && base.CanvasGroup.interactable)
			{
				OnQuitButtonClicked();
			}
		}

		private void OnQuitButtonClicked()
		{
			_buttonHighlight.enabled = false;
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(KnucklebonesModel.GetLocalizedString("Exit"), KnucklebonesModel.GetLocalizedString("AreYouSure"));
			uIMenuConfirmationWindow.OnHide = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnHide, (Action)delegate
			{
				_buttonHighlight.enabled = true;
			});
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, (Action)delegate
			{
				Action onQuitButtonPressed = OnQuitButtonPressed;
				if (onQuitButtonPressed != null)
				{
					onQuitButtonPressed();
				}
			});
		}

		protected override IEnumerator DoShowAnimation()
		{
			while (_canvasGroup.alpha < 1f)
			{
				_canvasGroup.alpha += Time.deltaTime;
				yield return null;
			}
		}

		protected override IEnumerator DoHideAnimation()
		{
			while (_canvasGroup.alpha > 0f)
			{
				_canvasGroup.alpha -= Time.deltaTime;
				yield return null;
			}
		}
	}
}
