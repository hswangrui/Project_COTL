using System;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using src.Extensions;
using src.Managers;
using src.UI;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.MainMenu
{
	public class LoadMenu : UISubmenuBase
	{
		public Action OnDeleteButtonPressed;

		public Action OnBackButtonPressed;

		[SerializeField]
		private SaveSlotButtonBase[] _saveSlots;

		[SerializeField]
		private Button _deleteButton;

		[SerializeField]
		private Button _backButton;

		private SaveSlotButtonBase lastSelectedSlot;

		private void Start()
		{

			Debug.Log("OpenLoadMenu");
			_deleteButton.onClick.AddListener(OnDeleteButtonClicked);
			_backButton.onClick.AddListener(OnBackButtonClicked);
		}

		public void SetupSlot(int slot)
		{
            Debug.Log("SetupSlot:"+ slot);
            _saveSlots[slot].SetupSaveSlot(slot);
		}

		public void SetupSlot(int slot, MetaData metaData)
		{
            Debug.Log("SetupSlot:"+ slot +"MetaData:" +metaData.ToString());
            _saveSlots[slot].SetupSaveSlot(slot, metaData);
		}

		protected override void OnShowStarted()
		{
			Debug.Log("OnShowStarted");
			SaveSlotButtonBase[] saveSlots = _saveSlots;
			foreach (SaveSlotButtonBase obj in saveSlots)
			{
				Debug.Log(obj.Button.name);
				obj.OnSaveSlotPressed += OnTryLoadSaveSlot;

            }
		}

		protected override void OnHideStarted()
		{
			SaveSlotButtonBase[] saveSlots = _saveSlots;
			foreach (SaveSlotButtonBase obj in saveSlots)
			{
				obj.OnSaveSlotPressed -= OnTryLoadSaveSlot;
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Action onBackButtonPressed = OnBackButtonPressed;
				if (onBackButtonPressed != null)
				{
					onBackButtonPressed();
				}
			}
		}

		private void OnTryLoadSaveSlot(int slotIndex)
		{
			Debug.Log("µã»÷´æµµ:" + slotIndex);
			MonoSingleton<UINavigatorNew>.Instance.LockInput = (MonoSingleton<UINavigatorNew>.Instance.LockNavigation = true);
			lastSelectedSlot = _saveSlots[slotIndex];
			DeviceLightingManager.StopAll();
			DeviceLightingManager.TransitionLighting(Color.white, Color.black, 1f);
			if (SaveAndLoad.SaveExist(slotIndex))
			{
				ContinueGame(slotIndex);
			}
			else
			{
				PlayNewGame(slotIndex);
			}
		}

		private void PlayNewGame(int saveSlot)
		{
			SaveAndLoad.SAVE_SLOT = saveSlot;
			if (PersistenceManager.HasBeatenGame())
			{
				MonoSingleton<UINavigatorNew>.Instance.LockInput = (MonoSingleton<UINavigatorNew>.Instance.LockNavigation = false);
				bool didBeginGame = false;
				UINewGameOptionsMenuController uINewGameOptionsMenuController = Push(MonoSingleton<UIManager>.Instance.NewGameOptionsMenuTemplate);
				uINewGameOptionsMenuController.OnOptionsAccepted = (Action<UINewGameOptionsMenuController.NewGameOptions>)Delegate.Combine(uINewGameOptionsMenuController.OnOptionsAccepted, (Action<UINewGameOptionsMenuController.NewGameOptions>)delegate(UINewGameOptionsMenuController.NewGameOptions result)
				{
					didBeginGame = true;
					Debug.Log("Start a new game with the specified options");
					SetActiveStateForMenu(false);
					BeginNewGame(result);
				});
				uINewGameOptionsMenuController.OnHide = (Action)Delegate.Combine(uINewGameOptionsMenuController.OnHide, (Action)delegate
				{
					if (didBeginGame)
					{
						MonoSingleton<UINavigatorNew>.Instance.LockInput = (MonoSingleton<UINavigatorNew>.Instance.LockNavigation = true);
						SetActiveStateForMenu(false);
						MonoSingleton<UINavigatorNew>.Instance.Clear();
					}
					_canvasGroup.interactable = true;
				});
				_canvasGroup.interactable = false;
			}
			else
			{
				BeginNewGame(new UINewGameOptionsMenuController.NewGameOptions
				{
					PermadeathMode = false,
					QuickStart = false,
					DifficultyIndex = DifficultyManager.AllAvailableDifficulties().IndexOf(DifficultyManager.Difficulty.Medium)
				});
			}
		}

		private void BeginNewGame(UINewGameOptionsMenuController.NewGameOptions newGameOptions)
		{
			GameManager.CurrentDungeonLayer = 1;
			GameManager.CurrentDungeonFloor = 1;
			GameManager.DungeonUseAllLayers = false;
			AudioManager.Instance.StopCurrentMusic();
			UIManager.PlayAudio("event:/ui/heretics_defeated");
			QuoteScreenController.Init(new List<QuoteScreenController.QuoteTypes>
			{
				QuoteScreenController.QuoteTypes.IntroQuote,
				QuoteScreenController.QuoteTypes.IntroQuote2
			}, delegate
			{
				MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Game Biome Intro", 1f, "", null);
			}, delegate
			{
				MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Game Biome Intro", 5f, "", null);
			});
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "QuoteScreen", 5f, "", delegate
			{
				SaveAndLoad.ResetSave(SaveAndLoad.SAVE_SLOT, true);
				DataManager.Instance.SetTutorialVariables();
				DataManager.Instance.PermadeDeathActive = newGameOptions.PermadeathMode;
				DataManager.Instance.MetaData.Difficulty = newGameOptions.DifficultyIndex;
				DifficultyManager.ForceDifficulty(DataManager.Instance.MetaData.Difficulty);
				MonoSingleton<UINavigatorNew>.Instance.LockInput = (MonoSingleton<UINavigatorNew>.Instance.LockNavigation = false);
			});
		}

		private void ContinueGame(int saveSlot)
		{
			SetActiveStateForMenu(false);
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			UIManager.PlayAudio("event:/ui/heretics_defeated");
			SaveAndLoad.SAVE_SLOT = saveSlot;
			AudioManager.Instance.StopCurrentMusic();
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Base Biome 1", 3f, "", ContinueGameCallback);
		}

		private void ContinueGameCallback()
		{
			AudioManager.Instance.StopCurrentMusic();
			SaveAndLoad.Load(SaveAndLoad.SAVE_SLOT);
			MonoSingleton<UINavigatorNew>.Instance.LockInput = (MonoSingleton<UINavigatorNew>.Instance.LockNavigation = false);
		}

		private void OnLoadSuccess()
		{
			Debug.Log("Load success!");
			AudioManager.Instance.StopCurrentMusic();
			MMTransition.Play(MMTransition.TransitionType.LoadAndFadeOut, MMTransition.Effect.BlackFade, "Base Biome 1", 3f, string.Empty, null);
			SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(OnLoadSuccess));
			SaveAndLoad.OnLoadError = (Action<MMReadWriteError>)Delegate.Remove(SaveAndLoad.OnLoadError, new Action<MMReadWriteError>(OnLoadError));
		}

		private void OnLoadError(MMReadWriteError error)
		{
			MonoSingleton<UINavigatorNew>.Instance.LockInput = false;
			UIMenuConfirmationWindow errorWindow = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
			errorWindow.Configure(ScriptLocalization.UI_Popups_Error.GenericHeader, ScriptLocalization.UI_Popups_Error.SaveCorrupted, true);
			errorWindow.Show();
			errorWindow.Canvas.sortingOrder = 1000;
			PushInstance(errorWindow);
			UIMenuConfirmationWindow uIMenuConfirmationWindow = errorWindow;
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, (Action)delegate
			{
				errorWindow.Hide();
			});
			UIMenuConfirmationWindow uIMenuConfirmationWindow2 = errorWindow;
			uIMenuConfirmationWindow2.OnHide = (Action)Delegate.Combine(uIMenuConfirmationWindow2.OnHide, (Action)delegate
			{
				MonoSingleton<UINavigatorNew>.Instance.Clear();
				MMTransition.IsPlaying = true;
				MMTransition.ResumePlay(delegate
				{
					SetActiveStateForMenu(true);
					MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(lastSelectedSlot.gameObject.GetComponent<IMMSelectable>());
				});
			});
			SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(OnLoadSuccess));
			SaveAndLoad.OnLoadError = (Action<MMReadWriteError>)Delegate.Remove(SaveAndLoad.OnLoadError, new Action<MMReadWriteError>(OnLoadError));
		}

		private void OnDeleteButtonClicked()
		{
			OverrideDefaultOnce(_deleteButton);
			Action onDeleteButtonPressed = OnDeleteButtonPressed;
			if (onDeleteButtonPressed != null)
			{
				onDeleteButtonPressed();
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
	}
}
