using System;
using UnityEngine;

namespace Lamb.UI
{
	public class UISaveErrorMenuController : UIMenuBase
	{
		public Action OnContinueButtonPressed;

		public Action OnRetryButtonPressed;

		[SerializeField]
		private MMButton _continueButton;

		[SerializeField]
		private MMButton _retryButton;

		[SerializeField]
		private UINavigatorFollowElement _buttonHighlight;

		private void Start()
		{
			_continueButton.onClick.AddListener(OnContinueButtonClicked);
			_retryButton.onClick.AddListener(OnRetryButtonClicked);
		}

		private void OnContinueButtonClicked()
		{
			Action onContinueButtonPressed = OnContinueButtonPressed;
			if (onContinueButtonPressed != null)
			{
				onContinueButtonPressed();
			}
			Hide();
		}

		private void OnRetryButtonClicked()
		{
			Action onRetryButtonPressed = OnRetryButtonPressed;
			if (onRetryButtonPressed != null)
			{
				onRetryButtonPressed();
			}
			Hide();
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
