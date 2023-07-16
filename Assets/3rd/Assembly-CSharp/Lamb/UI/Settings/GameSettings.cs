using System;
using I2.Loc;
using Unify;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Settings
{
	public class GameSettings : UISubmenuBase
	{
		[Header("Settings")]
		[SerializeField]
		private GameObject _difficultyLockIcon;

		[SerializeField]
		private MMSelectable_HorizontalSelector _difficultySelectable;

		[SerializeField]
		private MMSelectable_Toggle _permadeathToggle;

		[SerializeField]
		private MMHorizontalSelector _difficultySelector;

		[SerializeField]
		private Slider _rumbleSlider;

		[SerializeField]
		private MMToggle _disableTutorialsSwitch;

		[SerializeField]
		private MMHorizontalSelector _languageSelector;

		[SerializeField]
		private Localize _rumbleLocalize;

		[SerializeField]
		private MMHorizontalSelector _gamepadPrompts;

		[SerializeField]
		private MMToggle _showFollowerNames;

		private string _cachedLanguage;

		private string[] _gamepadPromptsContent = new string[5] { "UI/Settings/Graphics/ControlPrompts/Auto", "UI/Settings/Graphics/ControlPrompts/Xbox", "UI/Settings/Controls/Controller_PLAYSTATION4", "UI/Settings/Controls/Controller_PLAYSTATION5", "UI/Settings/Graphics/ControlPrompts/Switch" };

		public override void Awake()
		{
		}

		protected override void SetActiveStateForMenu(GameObject target, bool state)
		{
			base.SetActiveStateForMenu(target, state);
			if (PlayerFarming.Instance != null)
			{
				_difficultySelectable.Interactable = DataManager.Instance.DifficultyChosen && !DataManager.Instance.PermadeDeathActive;
				_difficultyLockIcon.SetActive(DataManager.Instance.PermadeDeathActive);
				_permadeathToggle.gameObject.SetActive(DataManager.Instance.PermadeDeathActive);
				_permadeathToggle.Interactable = false;
			}
			else
			{
				_difficultySelectable.Interactable = false;
				_permadeathToggle.gameObject.SetActive(false);
				_difficultyLockIcon.SetActive(false);
			}
		}

		protected override void OnShowStarted()
		{
			if (SettingsManager.Settings != null)
			{
				if ((_difficultySelectable.Interactable || DataManager.Instance.PermadeDeathActive) && DataManager.Instance.DifficultyChosen)
				{
					_difficultySelector.LocalizeContent = true;
					_difficultySelector.PrefillContent(DifficultyManager.GetDifficultyLocalisation());
				}
				else
				{
					_difficultySelector.PrefillContent("---");
				}
				_languageSelector.LocalizeContent = true;
				_languageSelector.PrefillContent(LanguageUtilities.AllLanguagesLocalizations);
				_gamepadPrompts.LocalizeContent = true;
				_gamepadPrompts.PrefillContent(_gamepadPromptsContent);
				MMHorizontalSelector difficultySelector = _difficultySelector;
				difficultySelector.OnSelectionChanged = (Action<int>)Delegate.Combine(difficultySelector.OnSelectionChanged, new Action<int>(OnDifficultyChanged));
				_rumbleSlider.onValueChanged.AddListener(OnRumbleIntensityChanged);
				MMToggle disableTutorialsSwitch = _disableTutorialsSwitch;
				disableTutorialsSwitch.OnValueChanged = (Action<bool>)Delegate.Combine(disableTutorialsSwitch.OnValueChanged, new Action<bool>(OnTutorialSwitchValueChanged));
				MMHorizontalSelector languageSelector = _languageSelector;
				languageSelector.OnSelectionChanged = (Action<int>)Delegate.Combine(languageSelector.OnSelectionChanged, new Action<int>(OnLanguageChanged));
				MMHorizontalSelector gamepadPrompts = _gamepadPrompts;
				gamepadPrompts.OnSelectionChanged = (Action<int>)Delegate.Combine(gamepadPrompts.OnSelectionChanged, new Action<int>(OnGamepadPromptsChanged));
				MMToggle showFollowerNames = _showFollowerNames;
				showFollowerNames.OnValueChanged = (Action<bool>)Delegate.Combine(showFollowerNames.OnValueChanged, new Action<bool>(OnShowFollowerNamesChanged));
				if (UnifyManager.platform == UnifyManager.Platform.PS4 || UnifyManager.platform == UnifyManager.Platform.PS5)
				{
					_rumbleLocalize.Term = "UI/Settings/Game/GamepadRumbleIntensity_PLAYSTATION";
				}
				else if (UnifyManager.platform == UnifyManager.Platform.XboxOne)
				{
					_rumbleLocalize.Term = "UI/Settings/Game/GamepadRumbleIntensity_XBOX";
				}
				else if (UnifyManager.platform == UnifyManager.Platform.Switch)
				{
					_rumbleLocalize.Term = "UI/Settings/Game/GamepadRumbleIntensity_SWITCH";
				}
				Configure(SettingsManager.Settings.Game, DataManager.Instance.MetaData);
			}
		}

		public void Configure(SettingsData.GameSettings gameSettings, MetaData metaData)
		{
			if (PlayerFarming.Instance != null)
			{
				_difficultySelector.ContentIndex = metaData.Difficulty;
			}
			else
			{
				_difficultySelector.ContentIndex = 0;
			}
			_rumbleSlider.value = gameSettings.RumbleIntensity * 100f;
			_disableTutorialsSwitch.Value = !gameSettings.ShowTutorials;
			_languageSelector.ContentIndex = GetLanguageIndex(gameSettings.Language);
			_gamepadPrompts.ContentIndex = gameSettings.GamepadPrompts;
			_showFollowerNames.Value = gameSettings.ShowFollowerNames;
			_cachedLanguage = gameSettings.Language;
		}

		public void Reset()
		{
			DataManager.Instance.MetaData = MetaData.Default(DataManager.Instance);
			SettingsManager.Settings.Game = new SettingsData.GameSettings
			{
				Language = _cachedLanguage
			};
		}

		private int GetLanguageIndex(string language)
		{
			return LanguageUtilities.AllLanguages.IndexOf(language);
		}

		private void OnRumbleIntensityChanged(float rumbleIntensity)
		{
			rumbleIntensity /= 100f;
			SettingsManager.Settings.Game.RumbleIntensity = rumbleIntensity;
			MMVibrate.SetHapticsIntensity(rumbleIntensity);
			RumbleManager.Instance.Rumble();
			Debug.Log(string.Format("GameSettings - Change rumble intensity to {0}", rumbleIntensity));
		}

		private void OnTutorialSwitchValueChanged(bool value)
		{
			SettingsManager.Settings.Game.ShowTutorials = !value;
			Debug.Log(string.Format("GameSettings - Change tutorial value to {0}", !value));
		}

		private void OnLanguageChanged(int index)
		{
			string text = LanguageUtilities.AllLanguages[index];
			SettingsManager.Settings.Game.Language = text;
			LocalizationManager.CurrentLanguage = text;
			_cachedLanguage = text;
			//if (TwitchAuthentication.IsAuthenticated)
			//{
			//	TwitchRequest.SendEBSData();
			//}
			Debug.Log("GameSettings - Change Language to " + text);
		}

		private void OnDifficultyChanged(int index)
		{
			DifficultyManager.Difficulty difficulty = DifficultyManager.AllAvailableDifficulties()[index];
			DifficultyManager.ForceDifficulty(difficulty);
			DataManager.Instance.MetaData.Difficulty = index;
			Debug.Log(string.Format("GameSettings - Change Difficulty to {0}", difficulty).Colour(Color.yellow));
		}

		private void OnGamepadPromptsChanged(int index)
		{
			Debug.Log(string.Format("GameSettings - Change Gamepad Prompts to {0}", index).Colour(Color.yellow));
			SettingsManager.Settings.Game.GamepadPrompts = index;
			ControlSettingsUtilities.UpdateGamepadPrompts();
		}

		private void OnShowFollowerNamesChanged(bool value)
		{
			Debug.Log(string.Format("GameSettings - Change Show Follower Names to {0}", value).Colour(Color.yellow));
			SettingsManager.Settings.Game.ShowFollowerNames = value;
			GameplaySettingsUtilities.UpdateShowFollowerNamesSetting(value);
		}
	}
}
