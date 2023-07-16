using System;
using System.Collections;
using I2.Loc;
using MMBiomeGeneration;
using MMTools;
using src.Extensions;
using src.UI;
using TMPro;
using Unify;
using UnityEngine;

namespace Lamb.UI.PauseMenu
{
	public class UIPauseMenuController : UIMenuBase
	{
		[Header("Buttons")]
		[SerializeField]
		private MMButton _resumeButton;

		[SerializeField]
		private MMButton _saveButton;

		[SerializeField]
		private MMButton _settingsButton;

		[SerializeField]
		private MMButton _twitchSettingsButton;

		[SerializeField]
		private MMButton _photoModeButton;

		[SerializeField]
		private MMButton _helpButton;

		[SerializeField]
		private MMButton _mainMenuButton;

		[SerializeField]
		private MMButton _quitButton;

		[Header("Sidebar")]
		[SerializeField]
		private MMButton _bugReportButton;

		[SerializeField]
		private MMButton _discordButton;

		[Header("Other")]
		[SerializeField]
		private TextMeshProUGUI _seedText;

		[SerializeField]
		private TextMeshProUGUI _saveButtonText;

		[SerializeField]
		private UINavigatorFollowElement _buttonHighlight;

		[SerializeField]
		private ButtonHighlightController _buttonHighlightController;

		private bool saved;

		public override void Awake()
		{
			base.Awake();
			SetActiveStateForMenu(false);
		}

		private void Start()
		{
			MMVibrate.StopRumble();
			AudioManager.Instance.PauseActiveLoops();
			_resumeButton.onClick.AddListener(OnResumeButtonPressed);
			_saveButton.onClick.AddListener(OnSaveButtonPressed);
			_settingsButton.onClick.AddListener(OnSettingsButtonPressed);
			_helpButton.onClick.AddListener(OnHelpButtonPressed);
			_mainMenuButton.onClick.AddListener(OnMainMenuButtonPressed);
			_quitButton.onClick.AddListener(OnQuitButtonPressed);
			_photoModeButton.onClick.AddListener(OnPhotoModePressed);
			_twitchSettingsButton.onClick.AddListener(OnTwitchSettingsButtonPressed);
			MMButton twitchSettingsButton = _twitchSettingsButton;
			twitchSettingsButton.OnSelected = (Action)Delegate.Combine(twitchSettingsButton.OnSelected, new Action(_buttonHighlightController.SetAsTwitch));
			MMButton twitchSettingsButton2 = _twitchSettingsButton;
			twitchSettingsButton2.OnDeselected = (Action)Delegate.Combine(twitchSettingsButton2.OnDeselected, new Action(_buttonHighlightController.SetAsRed));
			_seedText.text = ((BiomeGenerator.Instance == null) ? "" : BiomeGenerator.Instance.Seed.ToString());
			_bugReportButton.onClick.AddListener(delegate
			{
				Push(MonoSingleton<UIManager>.Instance.BugReportingOverlayTemplate);
			});
			_discordButton.onClick.AddListener(delegate
			{
				Application.OpenURL("https://discord.com/invite/massivemonster");
			});
			StartCoroutine(UpdateLoop());
		}

		private void OnEnable()
		{
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
			saved = false;
			SaveAndLoad.OnSaveCompleted = (Action)Delegate.Combine(SaveAndLoad.OnSaveCompleted, new Action(OnSaveFinalized));
			SaveAndLoad.OnSaveError = (Action<MMReadWriteError>)Delegate.Combine(SaveAndLoad.OnSaveError, new Action<MMReadWriteError>(OnSaveError));
		}

		private void OnDisable()
		{
			SessionManager.OnSessionEnd = (SessionManager.SessionEventDelegate)Delegate.Remove(SessionManager.OnSessionEnd, new SessionManager.SessionEventDelegate(OnSessionEnd));
			SaveAndLoad.OnSaveCompleted = (Action)Delegate.Remove(SaveAndLoad.OnSaveCompleted, new Action(OnSaveFinalized));
			SaveAndLoad.OnSaveError = (Action<MMReadWriteError>)Delegate.Remove(SaveAndLoad.OnSaveError, new Action<MMReadWriteError>(OnSaveError));
		}

		private IEnumerator UpdateLoop()
		{
			bool closable = true;
			while (true)
			{
				yield return null;
				if (closable && _canvasGroup.interactable && InputManager.Gameplay.GetPauseButtonDown())
				{
					break;
				}
				closable = UIMenuBase.ActiveMenus.Count == 1;
			}
			OnCancelButtonInput();
		}

		private void Update()
		{
			Time.timeScale = 0f;
		}

		private void OnSessionEnd(Guid sessionGuid, User sessionUser)
		{
			Hide(true);
		}

		private void OnResumeButtonPressed()
		{
			Hide();
		}

		private void OnSaveButtonPressed()
		{
			_saveButtonText.text = ScriptLocalization.UI.Saving;
			SaveAndLoad.Save();
		}

		private void OnSaveFinalized()
		{
			_saveButtonText.text = ScriptLocalization.UI.Saved;
			saved = true;
		}

		private void OnSaveError(MMReadWriteError error)
		{
			_saveButtonText.text = ScriptLocalization.UI_SaveError.Title;
			saved = true;
			UIMenuConfirmationWindow uIMenuConfirmationWindow = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_SaveError.Title, ScriptLocalization.UI_SaveError.Description.NewLine(), true);
			uIMenuConfirmationWindow.Show();
			PushInstance(uIMenuConfirmationWindow);
		}

		private void OnPhotoModePressed()
		{
			Hide(true);
			PhotoModeManager.EnablePhotoMode();
		}

		private void OnSettingsButtonPressed()
		{
			Push(MonoSingleton<UIManager>.Instance.SettingsMenuControllerTemplate);
		}

		private void OnTwitchSettingsButtonPressed()
		{
			Push(MonoSingleton<UIManager>.Instance.TwitchSettingsMenuController);
		}

		private void OnHelpButtonPressed()
		{
			Push(MonoSingleton<UIManager>.Instance.TutorialMenuTemplate);
		}

		private void OnMainMenuButtonPressed()
		{
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_Generic.Quit, (!saved) ? ScriptLocalization.UI.ConfirmQuitSaveWarning : ScriptLocalization.UI.ConfirmQuit);
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, new Action(LoadMainMenu));
		}

		private void LoadMainMenu()
		{
			SetActiveStateForMenu(false);
			SimulationManager.Pause();
			DeviceLightingManager.Reset();
			FollowerManager.Reset();
			StructureManager.Reset();
			UIDynamicNotificationCenter.Reset();
			//TwitchManager.Abort();
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 1f, "", delegate
			{
				Hide(true);
			});
		}

		private void OnQuitButtonPressed()
		{
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_Generic.Quit, (!saved) ? ScriptLocalization.UI.ConfirmQuitSaveWarning : ScriptLocalization.UI.ConfirmQuit);
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, (Action)delegate
			{
				Application.Quit();
			});
		}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			MMVibrate.StopRumble();
			UIManager.PlayAudio("event:/ui/pause");
		}

		protected override void SetActiveStateForMenu(bool state)
		{
			base.SetActiveStateForMenu(state);
			if (state)
			{
				_saveButton.interactable = BiomeGenerator.Instance == null && !MonoSingleton<UIManager>.Instance.ForceDisableSaving;
				_helpButton.interactable = DataManager.Instance.RevealedTutorialTopics.Count > 0;
			}
		}

		protected override void OnHideStarted()
		{
			AudioManager.Instance.ResumeActiveLoops();
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/unpause");
			_buttonHighlight.enabled = false;
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnPush()
		{
			_buttonHighlight.enabled = false;
		}

		protected override void OnRelease()
		{
			_buttonHighlight.enabled = true;
		}
	}
}
