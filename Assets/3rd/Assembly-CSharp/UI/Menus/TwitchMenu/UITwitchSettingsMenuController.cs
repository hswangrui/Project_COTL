using System;
using I2.Loc;
using Lamb.UI;
using src.Data;
using src.UI;
using TMPro;
using UnityEngine;

namespace UI.Menus.TwitchMenu
{
	public class UITwitchSettingsMenuController : UIMenuBase
	{
		[Header("Admin")]
		[SerializeField]
		private MMButton connectionButton;

		[SerializeField]
		private TMP_Text _connectionText;

		[SerializeField]
		private MMButton _integrationConfiguration;

		[Header("Settings")]
		[SerializeField]
		private MMToggle _helpHinderToggle;

		[SerializeField]
		private MMSlider _helpHinderFrequency;

		[SerializeField]
		private MMToggle _enableTotem;

		[SerializeField]
		private MMToggle _showTwitchFollowerNames;

		[SerializeField]
		private MMToggle _twitchMessages;

		public override void Awake()
		{
			base.Awake();
			connectionButton.onClick.AddListener(OnConnectionButtonPressed);
			_integrationConfiguration.onClick.AddListener(OnIntegrationConfigurationButtonClicked);
			Configure(DataManager.Instance.TwitchSettings);
			MMToggle helpHinderToggle = _helpHinderToggle;
			helpHinderToggle.OnValueChanged = (Action<bool>)Delegate.Combine(helpHinderToggle.OnValueChanged, new Action<bool>(OnHelpHinderToggleValueChanged));
			_helpHinderFrequency.onValueChanged.AddListener(OnHelpHinderFrequencyValueChanged);
			MMToggle enableTotem = _enableTotem;
			enableTotem.OnValueChanged = (Action<bool>)Delegate.Combine(enableTotem.OnValueChanged, new Action<bool>(OnEnableTotemValueChanged));
			MMToggle showTwitchFollowerNames = _showTwitchFollowerNames;
			showTwitchFollowerNames.OnValueChanged = (Action<bool>)Delegate.Combine(showTwitchFollowerNames.OnValueChanged, new Action<bool>(OnShowTwitchFollowersValueChanged));
			MMToggle twitchMessages = _twitchMessages;
			twitchMessages.OnValueChanged = (Action<bool>)Delegate.Combine(twitchMessages.OnValueChanged, new Action<bool>(OnTwitchMessagesValueChanged));
			_helpHinderFrequency.minValue = 20f;
			_helpHinderFrequency.maxValue = 30f;
			_helpHinderFrequency.GetCustomDisplayFormat = SetSliderText;
			//if (!string.IsNullOrEmpty(TwitchManager.ChannelName) && TwitchAuthentication.IsAuthenticated)
			//{
			//	_connectionText.text = TwitchManager.ChannelName + " - " + ScriptLocalization.UI_Twitch.SignOut;
			//}
			//else
			//{
			//	_connectionText.text = ScriptLocalization.UI_Twitch.Connect;
			//}
		}

		private void Configure(TwitchSettings twitchSettings)
		{
			_helpHinderToggle.Value = twitchSettings.HelpHinderEnabled;
			_helpHinderFrequency.value = twitchSettings.HelpHinderFrequency;
			_enableTotem.Value = twitchSettings.TotemEnabled;
			_showTwitchFollowerNames.Value = twitchSettings.FollowerNamesEnabled;
			_twitchMessages.Value = twitchSettings.TwitchMessagesEnabled;
		}

		private void Update()
		{
			if (InputManager.UI.GetResetAllSettingsButtonDown())
			{
				ResetAll();
			}
		}

		private void ResetAll()
		{
			UIMenuConfirmationWindow uiMenuConfirmationWindowInstance = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uiMenuConfirmationWindowInstance.Configure(LocalizationManager.GetTranslation("UI/Settings/Graphics/RevertToDefaultHeader"), LocalizationManager.GetTranslation("UI/Settings/Graphics/RevertToDefault"));
			UIMenuConfirmationWindow uIMenuConfirmationWindow = uiMenuConfirmationWindowInstance;
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, (Action)delegate
			{
				DataManager.Instance.TwitchSettings = new TwitchSettings();
				Configure(DataManager.Instance.TwitchSettings);
			});
			UIMenuConfirmationWindow uIMenuConfirmationWindow2 = uiMenuConfirmationWindowInstance;
			uIMenuConfirmationWindow2.OnHidden = (Action)Delegate.Combine(uIMenuConfirmationWindow2.OnHidden, (Action)delegate
			{
				uiMenuConfirmationWindowInstance = null;
			});
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private string SetSliderText(float value)
		{
			return string.Format("{0} mins", value);
		}

		private void OnConnectionButtonPressed()
		{
			//if (!TwitchAuthentication.IsAuthenticated)
			//{
			//	TwitchAuthentication.RequestLogIn(delegate
			//	{
			//		if (!string.IsNullOrEmpty(TwitchManager.ChannelName))
			//		{
			//			_connectionText.text = TwitchManager.ChannelName + " - " + ScriptLocalization.UI_Twitch.SignOut;
			//		}
			//	});
			//}
			//else
			//{
			//	TwitchAuthentication.SignOut();
			//	_connectionText.text = ScriptLocalization.UI_Twitch.Connect;
			//}
		}

		private void OnIntegrationConfigurationButtonClicked()
		{
			Application.OpenURL("https://dashboard.twitch.tv/extensions/wph0p912gucvcee0114kfoukn319db");
		}

		private void OnHelpHinderToggleValueChanged(bool value)
		{
		//	TwitchManager.HelpHinderEnabled = value;
		}

		private void OnHelpHinderFrequencyValueChanged(float value)
		{
			//TwitchManager.HelpHinderFrequency = value;
		}

		private void OnEnableTotemValueChanged(bool value)
		{
		//	TwitchManager.TotemEnabled = value;
		}

		private void OnShowTwitchFollowersValueChanged(bool value)
		{
		//	TwitchManager.FollowerNamesEnabled = value;
		}

		private void OnTwitchMessagesValueChanged(bool value)
		{
			//TwitchManager.MessagesEnabled = value;
		}
	}
}
