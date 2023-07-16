using System;
using DG.Tweening;
using I2.Loc;
using Lamb.UI.Alerts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIDifficultySelectorOverlayController : UIMenuBase
	{
		public Action<int> OnDifficultySelected;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[Header("Difficulty")]
		[SerializeField]
		private MMButton _easyButton;

		[SerializeField]
		private MMButton _mediumButton;

		[SerializeField]
		private MMButton _hardButton;

		[SerializeField]
		private MMButton _extraHardButton;

		[SerializeField]
		private Image _alertBadgeImage;

		[SerializeField]
		private AlertBadgeBase _alertBadge;

		[SerializeField]
		private TextMeshProUGUI _developerRecommendedText;

		[SerializeField]
		private TextMeshProUGUI _difficutlyDescriptionText;

		[Header("Messages")]
		[SerializeField]
		private GameObject _changeDisclaimer;

		[SerializeField]
		private GameObject _permadeathDislaimer;

		private bool _cancellable;

		public void Show(bool cancellable, bool permadeath, bool instant = false)
		{
			Show(instant);
			_cancellable = cancellable;
			_changeDisclaimer.SetActive(!permadeath);
			_permadeathDislaimer.SetActive(permadeath);
		}

		protected override void OnShowStarted()
		{
			if (!_cancellable)
			{
				_controlPrompts.HideCancelButton();
			}
			UIManager.PlayAudio("event:/ui/open_menu");
			_easyButton.onClick.AddListener(delegate
			{
				SelectDifficulty(0);
			});
			_mediumButton.onClick.AddListener(delegate
			{
				SelectDifficulty(1);
			});
			_hardButton.onClick.AddListener(delegate
			{
				SelectDifficulty(2);
			});
			_extraHardButton.onClick.AddListener(delegate
			{
				SelectDifficulty(3);
			});
			MMButton mediumButton = _mediumButton;
			mediumButton.OnSelected = (Action)Delegate.Combine(mediumButton.OnSelected, new Action(OnDeveloperRecommendedButtonSelected));
			MMButton mediumButton2 = _mediumButton;
			mediumButton2.OnDeselected = (Action)Delegate.Combine(mediumButton2.OnDeselected, new Action(OnDeveloperRecommendedButtonDeselected));
			MMButton easyButton = _easyButton;
			easyButton.OnSelected = (Action)Delegate.Combine(easyButton.OnSelected, (Action)delegate
			{
				UpdateDifficultyText(DifficultyManager.Difficulty.Easy);
			});
			MMButton mediumButton3 = _mediumButton;
			mediumButton3.OnSelected = (Action)Delegate.Combine(mediumButton3.OnSelected, (Action)delegate
			{
				UpdateDifficultyText(DifficultyManager.Difficulty.Medium);
			});
			MMButton hardButton = _hardButton;
			hardButton.OnSelected = (Action)Delegate.Combine(hardButton.OnSelected, (Action)delegate
			{
				UpdateDifficultyText(DifficultyManager.Difficulty.Hard);
			});
			MMButton extraHardButton = _extraHardButton;
			extraHardButton.OnSelected = (Action)Delegate.Combine(extraHardButton.OnSelected, (Action)delegate
			{
				UpdateDifficultyText(DifficultyManager.Difficulty.ExtraHard);
			});
			OverrideDefault(_mediumButton);
			ActivateNavigation();
		}

		private void UpdateDifficultyText(DifficultyManager.Difficulty difficulty)
		{
			switch (difficulty)
			{
			case DifficultyManager.Difficulty.Easy:
				_difficutlyDescriptionText.text = ScriptLocalization.UI_Settings_Game_Difficulty.EasyDescription;
				break;
			case DifficultyManager.Difficulty.Medium:
				_difficutlyDescriptionText.text = ScriptLocalization.UI_Settings_Game_Difficulty.MediumDescription;
				break;
			case DifficultyManager.Difficulty.Hard:
				_difficutlyDescriptionText.text = ScriptLocalization.UI_Settings_Game_Difficulty.HardDescription;
				break;
			case DifficultyManager.Difficulty.ExtraHard:
				_difficutlyDescriptionText.text = ScriptLocalization.UI_Settings_Game_Difficulty.ExtraHardDescription;
				break;
			}
		}

		private void OnDeveloperRecommendedButtonSelected()
		{
			_alertBadge.enabled = true;
			_alertBadgeImage.DOKill();
			_alertBadgeImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.1f).SetUpdate(true);
			_developerRecommendedText.DOKill();
			_developerRecommendedText.DOFade(1f, 0.1f).SetUpdate(true);
		}

		private void OnDeveloperRecommendedButtonDeselected()
		{
			_alertBadge.enabled = false;
			_alertBadgeImage.DOKill();
			_alertBadgeImage.DOColor(new Color(1f, 1f, 1f, 0f), 0.3f).SetUpdate(true);
			_developerRecommendedText.DOKill();
			_developerRecommendedText.DOFade(0f, 0.3f).SetUpdate(true);
		}

		private void SelectDifficulty(int difficultyIndex)
		{
			Action<int> onDifficultySelected = OnDifficultySelected;
			if (onDifficultySelected != null)
			{
				onDifficultySelected(difficultyIndex);
			}
			Hide();
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public override void OnCancelButtonInput()
		{
			if (_cancellable)
			{
				Hide();
			}
		}
	}
}
