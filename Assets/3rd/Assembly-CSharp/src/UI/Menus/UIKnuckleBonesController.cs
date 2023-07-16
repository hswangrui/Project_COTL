using System;
using System.Collections;
using Lamb.UI;
using src.UINavigator;
using UnityEngine;

namespace src.UI.Menus
{
	public class UIKnuckleBonesController : UIMenuBase
	{
		public enum KnucklebonesResult
		{
			Win,
			Loss,
			Draw
		}

		public Action<KnucklebonesResult> OnGameCompleted;

		public Action OnGameQuit;

		[Header("Knucklebones")]
		[SerializeField]
		private KBMainMenu _mainMenu;

		[SerializeField]
		private KBInstructionsScreen _instructionsScreen;

		[SerializeField]
		private KBGameScreen _gameScreen;

		[Header("Misc")]
		[SerializeField]
		private CanvasGroup _blackOverlayCanvasGroup;

		private bool _continueToPlay;

		private UIMenuBase _currentMenu;

		public override void Awake()
		{
			base.Awake();
			_blackOverlayCanvasGroup.alpha = 1f;
		}

		public void Show(KnucklebonesOpponent opponent, int bet, bool instant = false)
		{
			_gameScreen.Configure(opponent, bet);
			Show(instant);
		}

		protected override IEnumerator DoShowAnimation()
		{
			while (_blackOverlayCanvasGroup.alpha > 0f)
			{
				_blackOverlayCanvasGroup.alpha -= Time.unscaledDeltaTime;
				yield return null;
			}
		}

		protected override IEnumerator DoHideAnimation()
		{
			while (_blackOverlayCanvasGroup.alpha < 1f)
			{
				_blackOverlayCanvasGroup.alpha += Time.unscaledDeltaTime;
				yield return null;
			}
		}

		public void Start()
		{
			AudioManager.Instance.SetMusicRoomID(2, "ratau_home_id");
			_mainMenu.Show(true);
			_currentMenu = _mainMenu;
			KBMainMenu mainMenu = _mainMenu;
			mainMenu.OnPlayButtonPressed = (Action)Delegate.Combine(mainMenu.OnPlayButtonPressed, (Action)delegate
			{
				if (DataManager.Instance.ShownKnuckleboneTutorial)
				{
					MonoSingleton<UINavigatorNew>.Instance.Clear();
					TransitionTo(_gameScreen);
				}
				else
				{
					_continueToPlay = true;
					TransitionTo(_instructionsScreen);
				}
			});
			KBMainMenu mainMenu2 = _mainMenu;
			mainMenu2.OnInstructionsButtonPressed = (Action)Delegate.Combine(mainMenu2.OnInstructionsButtonPressed, (Action)delegate
			{
				TransitionTo(_instructionsScreen);
			});
			KBMainMenu mainMenu3 = _mainMenu;
			mainMenu3.OnQuitButtonPressed = (Action)Delegate.Combine(mainMenu3.OnQuitButtonPressed, new Action(OnMatchQuit));
			KBInstructionsScreen instructionsScreen = _instructionsScreen;
			instructionsScreen.OnContinueButtonPressed = (Action)Delegate.Combine(instructionsScreen.OnContinueButtonPressed, (Action)delegate
			{
				if (_continueToPlay)
				{
					MonoSingleton<UINavigatorNew>.Instance.Clear();
					TransitionTo(_gameScreen);
				}
				else
				{
					TransitionTo(_mainMenu);
				}
			});
			KBInstructionsScreen instructionsScreen2 = _instructionsScreen;
			instructionsScreen2.OnBackButtonPressed = (Action)Delegate.Combine(instructionsScreen2.OnBackButtonPressed, (Action)delegate
			{
				TransitionTo(_mainMenu);
			});
			KBGameScreen gameScreen = _gameScreen;
			gameScreen.OnMatchFinished = (Action<KnucklebonesResult>)Delegate.Combine(gameScreen.OnMatchFinished, new Action<KnucklebonesResult>(OnMatchFinished));
		}

		private void TransitionTo(UIMenuBase menu)
		{
			if (_currentMenu != menu)
			{
				PerformMenuTransition(_currentMenu, menu);
				_currentMenu = menu;
			}
		}

		private void PerformMenuTransition(UIMenuBase from, UIMenuBase to)
		{
			from.Hide();
			to.Show();
		}

		private void OnMatchFinished(KnucklebonesResult victory)
		{
			OnHidden = (Action)Delegate.Combine(OnHidden, (Action)delegate
			{
				Action<KnucklebonesResult> onGameCompleted = OnGameCompleted;
				if (onGameCompleted != null)
				{
					onGameCompleted(victory);
				}
			});
			Hide();
		}

		private void OnMatchQuit()
		{
			OnHidden = (Action)Delegate.Combine(OnHidden, (Action)delegate
			{
				Action onGameQuit = OnGameQuit;
				if (onGameQuit != null)
				{
					onGameQuit();
				}
			});
			Hide();
		}

		protected override void OnHideCompleted()
		{
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
