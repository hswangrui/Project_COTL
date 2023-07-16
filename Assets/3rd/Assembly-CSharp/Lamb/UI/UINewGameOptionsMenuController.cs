using System;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class UINewGameOptionsMenuController : UIMenuBase
	{
		public struct NewGameOptions
		{
			public bool QuickStart;

			public int DifficultyIndex;

			public bool PermadeathMode;
		}

		public Action<NewGameOptions> OnOptionsAccepted;

		[SerializeField]
		private MMToggle _quickStartToggle;

		[SerializeField]
		private MMToggle _permadeathMode;

		[SerializeField]
		private MMButton _acceptButton;

		public override void Awake()
		{
			base.Awake();
			_quickStartToggle.Value = false;
			_permadeathMode.Value = false;
			MMToggle quickStartToggle = _quickStartToggle;
			quickStartToggle.OnValueChanged = (Action<bool>)Delegate.Combine(quickStartToggle.OnValueChanged, new Action<bool>(OnQuickStartToggleChanged));
			MMToggle permadeathMode = _permadeathMode;
			permadeathMode.OnValueChanged = (Action<bool>)Delegate.Combine(permadeathMode.OnValueChanged, new Action<bool>(OnPermadeathModeToggleChanged));
			_acceptButton.onClick.AddListener(OnAcceptButtonClicked);
		}

		private void OnQuickStartToggleChanged(bool value)
		{
		}

		private void OnPermadeathModeToggleChanged(bool value)
		{
		}

		private void OnAcceptButtonClicked()
		{
			if (_quickStartToggle.Value)
			{
				UIDifficultySelectorOverlayController uIDifficultySelectorOverlayController = MonoSingleton<UIManager>.Instance.DifficultySelectorTemplate.Instantiate();
				uIDifficultySelectorOverlayController.Show(true, _permadeathMode.Value);
				PushInstance(uIDifficultySelectorOverlayController);
				uIDifficultySelectorOverlayController.OnDifficultySelected = (Action<int>)Delegate.Combine(uIDifficultySelectorOverlayController.OnDifficultySelected, (Action<int>)delegate(int result)
				{
					Action<NewGameOptions> onOptionsAccepted2 = OnOptionsAccepted;
					if (onOptionsAccepted2 != null)
					{
						onOptionsAccepted2(new NewGameOptions
						{
							QuickStart = _quickStartToggle.Value,
							DifficultyIndex = result,
							PermadeathMode = _permadeathMode.Value
						});
					}
					Hide();
				});
			}
			else
			{
				Action<NewGameOptions> onOptionsAccepted = OnOptionsAccepted;
				if (onOptionsAccepted != null)
				{
					onOptionsAccepted(new NewGameOptions
					{
						QuickStart = _quickStartToggle.Value,
						DifficultyIndex = DifficultyManager.AllAvailableDifficulties().IndexOf(DifficultyManager.Difficulty.Medium),
						PermadeathMode = _permadeathMode.Value
					});
				}
				Hide();
			}
		}

		public override void OnCancelButtonInput()
		{
			base.OnCancelButtonInput();
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(this);
		}
	}
}
