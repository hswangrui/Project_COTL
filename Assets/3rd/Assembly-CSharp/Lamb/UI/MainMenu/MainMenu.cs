using System;
using I2.Loc;
using Lamb.UI.SettingsMenu;
using MMTools;
using src.Extensions;
using src.Managers;
using src.UI;
using src.UINavigator;
using Unify;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.MainMenu
{
	public class MainMenu : UISubmenuBase
	{
		public Action OnPlayButtonPressed;

		[Header("Buttons")]
		[SerializeField]
		private Button _playButton;

		[SerializeField]
		private Button _settingsButton;

		[SerializeField]
		private Button _creditsButton;

		[SerializeField]
		private Button _roadmapButton;

		[SerializeField]
		private Button _quitButton;

		[Header("Other")]
		[SerializeField]
		private UINavigatorFollowElement _buttonHighlight;

		[SerializeField]
		private GameObject _UserPicker;

		public bool WillRequirePostGameReveal;

		public void Start()
		{
			_playButton.onClick.AddListener(OnPlayButtonClicked);
			_settingsButton.onClick.AddListener(OnSettingsButtonClicked);
			_creditsButton.onClick.AddListener(OnCreditsButtonClicked);
			_roadmapButton.onClick.AddListener(OnRoadmapButtonClicked);
			_quitButton.onClick.AddListener(OnQuitButtonClicked);
			if (CheatConsole.IN_DEMO)
			{
				EnableDemo(true);
			}
			SessionManager.OnSessionStart = (SessionManager.SessionEventDelegate)Delegate.Combine(SessionManager.OnSessionStart, new SessionManager.SessionEventDelegate(ForceSelectPlayOnSessionStart));
		}

		public void ForceSelectPlayOnSessionStart(Guid sessionGuid, User sessionUser)
		{
			SetActiveStateForMenu(true);
		}

		public void EnableDemo(bool AlreadyInDemo)
		{
			_creditsButton.gameObject.SetActive(false);
			_settingsButton.gameObject.SetActive(false);
			_quitButton.gameObject.SetActive(false);
			if (!AlreadyInDemo)
			{
				UISettingsMenuController activeMenu = UIManager.GetActiveMenu<UISettingsMenuController>();
				if (activeMenu != null)
				{
					activeMenu.Hide(true);
				}
				MMTransition.StopCurrentTransition();
				MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 1f, "", delegate
				{
					MonoSingleton<UINavigatorNew>.Instance.Clear();
				});
			}
		}

		protected override void OnShowCompleted()
		{
			if (WillRequirePostGameReveal)
			{
				UIMenuConfirmationWindow uIMenuConfirmationWindow = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
				uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_PostGameUnlock.Header, ScriptLocalization.UI_PostGameUnlock.Description, true);
				uIMenuConfirmationWindow.Show();
				SetActiveStateForMenu(false);
				uIMenuConfirmationWindow.OnHidden = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnHidden, (Action)delegate
				{
					WillRequirePostGameReveal = false;
					PersistenceManager.PersistentData.PostGameRevealed = true;
					PersistenceManager.Save();
					SetActiveStateForMenu(true);
				});
			}
		}

		private void OnPlayButtonClicked()
		{
			OverrideDefaultOnce(_playButton);
			Action onPlayButtonPressed = OnPlayButtonPressed;
			if (onPlayButtonPressed != null)
			{
				onPlayButtonPressed();
			}
		}

		private void OnSettingsButtonClicked()
		{
			Push(MonoSingleton<UIManager>.Instance.SettingsMenuControllerTemplate);
		}

		private void OnCreditsButtonClicked()
		{
			Push(MonoSingleton<UIManager>.Instance.CreditsMenuControllerTemplate);
		}

		private void OnRoadmapButtonClicked()
		{
			Push(MonoSingleton<UIManager>.Instance.RoadmapOverlayControllerTemplate);
		}

		private void OnQuitButtonClicked()
		{
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI.Quit_Game, ScriptLocalization.UI.ConfirmQuit);
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, (Action)delegate
			{
				Application.Quit();
			});
		}

		protected override void OnPush()
		{
			_buttonHighlight.enabled = false;
		}

		protected override void OnRelease()
		{
			_buttonHighlight.enabled = true;
		}

		private void Update()
		{
			if (UnifyManager.platform == UnifyManager.Platform.GameCoreConsole || UnifyManager.platform == UnifyManager.Platform.GameCore)
			{
				if (_settingsButton.interactable && _creditsButton.interactable && _playButton.interactable)
				{
					_UserPicker.SetActive(true);
				}
				else
				{
					_UserPicker.SetActive(false);
				}
			}
		}
	}
}
