using System;
using System.Collections;
using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Menus
{
	public class KBInstructionsScreen : UISubmenuBase
	{
		public Action OnContinueButtonPressed;

		public Action OnBackButtonPressed;

		[Header("Instructions Menu")]
		[SerializeField]
		private Button _continueButton;

		[Header("Misc")]
		[SerializeField]
		private UINavigatorFollowElement _buttonHighlight;

		private void Start()
		{
			_continueButton.onClick.AddListener(OnContinueButtonClicked);
		}

		protected override void OnShowStarted()
		{
			DataManager.Instance.ShownKnuckleboneTutorial = true;
		}

		private void OnContinueButtonClicked()
		{
			Action onContinueButtonPressed = OnContinueButtonPressed;
			if (onContinueButtonPressed != null)
			{
				onContinueButtonPressed();
			}
		}

		private void OnBackButtonClicked()
		{
			Action onBackButtonPressed = OnBackButtonPressed;
			if (onBackButtonPressed != null)
			{
				onBackButtonPressed();
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				OnBackButtonClicked();
			}
		}

		protected override void SetActiveStateForMenu(bool state)
		{
			_buttonHighlight.enabled = state;
			base.SetActiveStateForMenu(state);
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
