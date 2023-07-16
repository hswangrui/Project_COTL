using System;
using System.Collections;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;
using DG.Tweening;
using I2.Loc;
using MMTools;
using src.Extensions;
using src.UI;
using src.UINavigator;
using UnityEngine;
//using static MenuAdController;

namespace Lamb.UI.MainMenu
{
    public class UIMainMenuController : UIMenuBase
    {
        [Header("Menus")]
    [SerializeField]
    private MainMenu _mainMenu;

    [SerializeField]
    private LoadMenu _loadMenu;

    [SerializeField]
    private DeleteMenu _deleteMenu;

    [Header("Other")]
    [SerializeField]
    private UIMenuControlPrompts _controlPrompts;

    [SerializeField]
    private UINavigatorFollowElement _navigatorHighlight;

    [SerializeField]
    private CanvasGroup ad;

    [Header("Sidebar")]
    [SerializeField]
    private MMButton _discordButton;

    private UIMenuBase _currentMenu;

    public MainMenu MainMenu => _mainMenu;

    public LoadMenu LoadMenu => _loadMenu;

    public DeleteMenu DeleteMenu => _deleteMenu;

    public override void Awake()
    {
        base.Awake();
        _controlPrompts.HideCancelButton();
        _controlPrompts.HideAcceptButton();
        MainMenu mainMenu = _mainMenu;
        mainMenu.OnShow = (Action)Delegate.Combine(mainMenu.OnShow, (Action)delegate
        {
            _controlPrompts.ShowAcceptButton();
        });
        MMConversation.CURRENT_CONVERSATION = null;
    }

    public void Start()
    {
        _currentMenu = _mainMenu;
        UIMenuBase.ActiveMenus.Add(this);
        if (CheatConsole.IN_DEMO)
        {
            DemoWatermark.Play();
        }
        MainMenu mainMenu = _mainMenu;
        mainMenu.OnPlayButtonPressed = (Action)Delegate.Combine(mainMenu.OnPlayButtonPressed, (Action)delegate
        {
            ad.DOFade(0f, 0.25f);
            SetActiveStateForMenu(ad.gameObject, state: false);
            if (CheatConsole.IN_DEMO)
            {
                SaveAndLoad.SAVE_SLOT = 10;
                AudioManager.Instance.StopCurrentMusic();
                UIManager.PlayAudio("event:/ui/heretics_defeated");
                DifficultyManager.ForceDifficulty(DifficultyManager.Difficulty.Medium);
                DeviceLightingManager.Reset();
                FollowerManager.Reset();
                SimulationManager.Pause();
                StructureManager.Reset();
              //  TwitchManager.Abort();
                UIDynamicNotificationCenter.Reset();
                GameManager.GoG_Initialised = false;
                MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Game Biome Intro", 5f, "", delegate
                {
                    SaveAndLoad.ResetSave(SaveAndLoad.SAVE_SLOT, newGame: true);
                    DataManager.Instance.SetTutorialVariables();
                });
                SetActiveStateForMenu(state: false);
                MonoSingleton<UINavigatorNew>.Instance.Clear();
            }
            else
            {
                PerformMenuTransition(_mainMenu, _loadMenu);
                _controlPrompts.ShowCancelButton();
                _controlPrompts.ShowAcceptButton();
            }
        });
        LoadMenu loadMenu = _loadMenu;
        loadMenu.OnBackButtonPressed = (Action)Delegate.Combine(loadMenu.OnBackButtonPressed, (Action)delegate
        {
            PerformMenuTransition(_loadMenu, _mainMenu);
            _controlPrompts.HideCancelButton();
            _controlPrompts.ShowAcceptButton();
            ad.DOFade(1f, 0.25f).SetDelay(0.25f);
            SetActiveStateForMenu(ad.gameObject, state: true);
        });
        LoadMenu loadMenu2 = _loadMenu;
        loadMenu2.OnDeleteButtonPressed = (Action)Delegate.Combine(loadMenu2.OnDeleteButtonPressed, (Action)delegate
        {
            PerformMenuTransition(_loadMenu, _deleteMenu);
        });
        DeleteMenu deleteMenu = _deleteMenu;
        deleteMenu.OnBackButtonPressed = (Action)Delegate.Combine(deleteMenu.OnBackButtonPressed, (Action)delegate
        {
            PerformMenuTransition(_deleteMenu, _loadMenu);
        });
        _discordButton.onClick.AddListener(delegate
        {
            Application.OpenURL("https://discord.com/invite/massivemonster");
        });
    }

    private void PerformMenuTransition(UISubmenuBase from, UISubmenuBase to)
    {
        _currentMenu = to;
        from.Hide();
        to.Show();
    }

    protected override void OnPush()
    {
        _navigatorHighlight.enabled = false;
    }

    protected override void OnRelease()
    {
        _navigatorHighlight.enabled = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UIMenuBase.ActiveMenus.Remove(this);
    }

    public IEnumerator PreloadMetadata()
    {
        COTLDataReadWriter<MetaData> metaDataPreloader = new COTLDataReadWriter<MetaData>();
        COTLDataReadWriter<DataManager> saveDataLoader = new COTLDataReadWriter<DataManager>();
        List<int> metaErrors = new List<int>();
        for (int slot = 0; slot < 3; slot++)
        {
            int currentSlot = slot;
            string filename = SaveAndLoad.MakeMetaSlot(currentSlot);
            bool loaded;
            MetaData mData;
            if (SaveAndLoad.SaveExist(currentSlot))
            {
                loaded = false;
                mData = default(MetaData);
                if (metaDataPreloader.FileExists(filename))
                {
                    metaDataPreloader.OnReadCompleted = (Action<MetaData>)Delegate.Combine(metaDataPreloader.OnReadCompleted, new Action<MetaData>(MetaDataLoaded));
                    metaDataPreloader.OnReadError = (Action<MMReadWriteError>)Delegate.Combine(metaDataPreloader.OnReadError, new Action<MMReadWriteError>(MetaDataReadError));
                    metaDataPreloader.Read(filename);
                }
                else
                {
                    metaErrors.Add(slot);
                    loaded = true;
                }
                while (!loaded)
                {
                    yield return null;
                }
                if (!metaErrors.Contains(currentSlot))
                {
                    _loadMenu.SetupSlot(currentSlot, mData);
                    _deleteMenu.SetupSlot(currentSlot, mData);
                }
                else
                {
                    _loadMenu.SetupSlot(currentSlot);
                    _deleteMenu.SetupSlot(currentSlot);
                }
                metaDataPreloader.OnWriteCompleted = null;
                metaDataPreloader.OnReadCompleted = null;
                metaDataPreloader.OnReadError = null;
                saveDataLoader.OnReadCompleted = null;
            }
            else
            {
                _loadMenu.SetupSlot(currentSlot);
                _deleteMenu.SetupSlot(currentSlot);
            }
            void MetaDataLoaded(MetaData metaData)
            {
                mData = metaData;
                loaded = true;
            }
            void MetaDataReadError(MMReadWriteError readWriteError)
            {
                metaErrors.Add(currentSlot);
                loaded = true;
            }
        }
        if (metaErrors.Count > 0)
        {
            yield return RestoreFiles(metaErrors, metaDataPreloader, saveDataLoader);
        }
    }

    private IEnumerator RestoreFiles(List<int> markedForRestoration, COTLDataReadWriter<MetaData> metaDataPreloader, COTLDataReadWriter<DataManager> saveDataLoader)
    {
        UIMenuConfirmationWindow uIMenuConfirmationWindow = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
        uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_Popups_Error.GenericHeader, ScriptLocalization.UI_Popups_Error.LoadingErrorDescription, acceptOnly: true);
        uIMenuConfirmationWindow.Show();
        PushInstance(uIMenuConfirmationWindow);
        yield return uIMenuConfirmationWindow.YieldUntilHide();
        UILoadingOverlayController loadingOverlayController = Push(MonoSingleton<UIManager>.Instance.LoadingOverlayControllerTemplate);
        yield return loadingOverlayController.YieldUntilShown();
        int filesNotRestored = 0;
        bool loaded = true;
        foreach (int slot in markedForRestoration)
        {
            loaded = false;
            loadingOverlayController.Message = string.Format(ScriptLocalization.UI_Popups_SaveRestore.RestoringProgress, slot + 1, markedForRestoration.Count, 1, 4);
            yield return new WaitForSecondsRealtime(0.05f);
            string metaSlot = SaveAndLoad.MakeMetaSlot(slot);
            if (metaDataPreloader.FileExists(metaSlot))
            {
                metaDataPreloader.Delete(metaSlot);
            }
            metaDataPreloader.OnWriteCompleted = (Action)Delegate.Combine(metaDataPreloader.OnWriteCompleted, (Action)delegate
            {
                loaded = true;
            });
            yield return new WaitForSecondsRealtime(0.25f);
            string saveSlot = SaveAndLoad.MakeSaveSlot(slot);
            DataManager saveFile = null;
            COTLDataReadWriter<DataManager> cOTLDataReadWriter = saveDataLoader;
            cOTLDataReadWriter.OnReadCompleted = (Action<DataManager>)Delegate.Combine(cOTLDataReadWriter.OnReadCompleted, (Action<DataManager>)delegate (DataManager datamanager)
            {
                loaded = true;
                saveFile = datamanager;
            });
            COTLDataReadWriter<DataManager> cOTLDataReadWriter2 = saveDataLoader;
            cOTLDataReadWriter2.OnReadError = (Action<MMReadWriteError>)Delegate.Combine(cOTLDataReadWriter2.OnReadError, (Action<MMReadWriteError>)delegate
            {
                loaded = true;
                int num = filesNotRestored;
                filesNotRestored = num + 1;
                saveDataLoader.Delete(saveSlot);
            });
            loadingOverlayController.Message = string.Format(ScriptLocalization.UI_Popups_SaveRestore.RestoringProgress, slot, markedForRestoration.Count, 2, 4);
            yield return new WaitForSecondsRealtime(0.05f);
            saveDataLoader.Read(saveSlot);
            while (!loaded)
            {
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.25f);
            if (saveFile != null)
            {
                loadingOverlayController.Message = string.Format(ScriptLocalization.UI_Popups_SaveRestore.RestoringProgress, slot, markedForRestoration.Count, 3, 4);
                yield return new WaitForSecondsRealtime(0.05f);
                MetaData metaData = MetaData.Default(saveFile);
                _loadMenu.SetupSlot(slot, metaData);
                _deleteMenu.SetupSlot(slot, metaData);
                metaDataPreloader.Write(metaData, metaSlot, encrypt: true, backup: false);
            }
            while (!loaded)
            {
                yield return null;
            }
            loadingOverlayController.Message = string.Format(ScriptLocalization.UI_Popups_SaveRestore.RestoringProgress, slot, markedForRestoration.Count, 4, 4);
            yield return new WaitForSecondsRealtime(0.25f);
            metaDataPreloader.OnWriteCompleted = null;
            metaDataPreloader.OnReadCompleted = null;
            metaDataPreloader.OnReadError = null;
            saveDataLoader.OnReadCompleted = null;
        }
        loadingOverlayController.Hide();
        if (filesNotRestored > 0)
        {
            UIMenuConfirmationWindow uIMenuConfirmationWindow2 = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
            uIMenuConfirmationWindow2.Configure(ScriptLocalization.UI_Popups_Error.Restoration, ScriptLocalization.UI_Popups_Error.RestorationErrorDescription, acceptOnly: true);
            uIMenuConfirmationWindow2.Show();
            PushInstance(uIMenuConfirmationWindow2);
            yield return uIMenuConfirmationWindow2.YieldUntilHidden();
        }
        else
        {
            UIMenuConfirmationWindow uIMenuConfirmationWindow3 = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
            uIMenuConfirmationWindow3.Configure(ScriptLocalization.UI_Popups_Success.RestorationComplete, ScriptLocalization.UI_Popups_Success.RestorationCompleteDescription, acceptOnly: true);
            uIMenuConfirmationWindow3.Show();
            PushInstance(uIMenuConfirmationWindow3);
            yield return uIMenuConfirmationWindow3.YieldUntilHidden();
        }
    }

    private void Update()
    {
        ad.gameObject.SetActive(LocalizationManager.CurrentLanguageCode == "en");
    }
}
}