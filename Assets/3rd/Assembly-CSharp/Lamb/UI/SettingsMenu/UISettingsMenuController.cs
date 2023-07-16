using System;
using I2.Loc;
using Lamb.UI.Settings;
using src.UI;
using UnityEngine;

namespace Lamb.UI.SettingsMenu
{
	public class UISettingsMenuController : UIMenuBase
	{
		[Header("Menus")]
		[SerializeField]
		private GameSettings _gameSettings;

		[SerializeField]
		private GraphicsSettings _graphicsSettings;

		[SerializeField]
		private AccessibilitySettings _accessibilitySettings;

		[SerializeField]
		private AudioSettings _audioSettings;

		[SerializeField]
		private ControlSettings _controlSettings;

		protected override void OnShowStarted()
		{
			UIManager.PlayAudio("event:/ui/open_menu");
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				SaveAndApply();
				Hide();
			}
		}

		private void Update()
		{
			if (_canvasGroup.interactable && InputManager.UI.GetResetAllSettingsButtonDown())
			{
				ResetAll();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void ResetAll()
		{
			UIMenuConfirmationWindow uiMenuConfirmationWindowInstance = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uiMenuConfirmationWindowInstance.Configure(LocalizationManager.GetTranslation("UI/Settings/Graphics/RevertToDefaultHeader"), LocalizationManager.GetTranslation("UI/Settings/Graphics/RevertToDefault"));
			UIMenuConfirmationWindow uIMenuConfirmationWindow = uiMenuConfirmationWindowInstance;
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, (Action)delegate
			{
				_gameSettings.Reset();
				_graphicsSettings.Reset();
				_accessibilitySettings.Reset();
				_audioSettings.Reset();
				_controlSettings.Reset();
				_gameSettings.Configure(SettingsManager.Settings.Game, DataManager.Instance.MetaData);
				_graphicsSettings.Configure(SettingsManager.Settings.Graphics);
				_accessibilitySettings.Configure(SettingsManager.Settings.Accessibility);
				_audioSettings.Configure(SettingsManager.Settings.Audio);
				_controlSettings.Configure(SettingsManager.Settings.Control);
				SaveAndApply();
			});
			UIMenuConfirmationWindow uIMenuConfirmationWindow2 = uiMenuConfirmationWindowInstance;
			uIMenuConfirmationWindow2.OnHidden = (Action)Delegate.Combine(uIMenuConfirmationWindow2.OnHidden, (Action)delegate
			{
				uiMenuConfirmationWindowInstance = null;
			});
		}

		private void SaveAndApply()
		{
			if (PlayerFarming.Instance != null)
			{
				DifficultyManager.ForceDifficulty(DataManager.Instance.MetaData.Difficulty);
			}
			Singleton<SettingsManager>.Instance.SaveSettings();
			Singleton<SettingsManager>.Instance.ApplySettings();
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
		}
	}
}
