using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FMOD.Studio;
using Lamb.UI.AltarMenu;
using Lamb.UI.BuildMenu;
using Lamb.UI.DeathScreen;
using Lamb.UI.FollowerInteractionWheel;
using Lamb.UI.FollowerSelect;
using Lamb.UI.KitchenMenu;
using Lamb.UI.Menus.DoctrineChoicesMenu;
using Lamb.UI.Menus.PlayerMenu;
using Lamb.UI.Mission;
using Lamb.UI.PauseMenu;
using Lamb.UI.PrisonerMenu;
using Lamb.UI.RefineryMenu;
using Lamb.UI.Rituals;
using Lamb.UI.SermonWheelOverlay;
using Lamb.UI.SettingsMenu;
using Lamb.UI.VideoMenu;
using Map;
using MMTools;
using src.Extensions;
using src.UI;
using src.UI.Items;
using src.UI.Menus;
using src.UI.Menus.CryptMenu;
using src.UI.Menus.ShareHouseMenu;
using src.UI.Overlays.TutorialOverlay;
using src.UI.Overlays.TwitchFollowerVotingOverlay;
using src.UI.Prompts;
using UI.Menus.TwitchMenu;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;
using EventInstance = FMOD.Studio.EventInstance;

namespace Lamb.UI
{
    [DefaultExecutionOrder(-100)]
    public class UIManager : MonoSingleton<UIManager>
    {
        private const string kAssetPath = "Assets/UI/Prefabs/UIManager.prefab";

        public bool ForceBlockMenus;

        public bool ForceDisableSaving;

        private UIMenuBase _currentInstance;

        private float _previousClipPlane = 100f;

        private Camera _currentMain;

        private Texture2D _currentCursorTexture;

        private Resolution _currentResolution;

        private int _cursorSize;

        private Dictionary<GameObject, AsyncOperationHandle> _addressablesTracker = new Dictionary<GameObject, AsyncOperationHandle>();

        [Header("Cursor")]
        [SerializeField]
        private Texture2D _cursor;

        [Header("Direct References")]
        [SerializeField]
        private UISettingsMenuController _settingsMenuTemplate;

        [SerializeField]
        private UIMenuConfirmationWindow _confirmationWindowTemplate;

        [SerializeField]
        private UIConfirmationCountdownWindow _confirmationCountdownWindowTemplate;

        [Header("Controllers")]
        [SerializeField]
        private AssetReferenceGameObject _xboxControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _ps4ControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _ps5ControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _switchProControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _switchJoyConsControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _switchJoyConsDetachedControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _switchHandheldControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _keyboardTemplate;

        [SerializeField]
        private AssetReferenceGameObject _mouseTemplate;

        [Header("Menu Templates")]
        [SerializeField]
        private AssetReferenceGameObject _creditsMenuControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _roadmapOverlayControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _pauseMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _pauseDetailsMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _indoctrinationMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _tarotCardsMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _worldMapTemplate;

        [SerializeField]
        private AssetReferenceGameObject _upgradeTreeTemplate;

        [SerializeField]
        private AssetReferenceGameObject _upgradePlayerTreeTemplate;

        [SerializeField]
        private AssetReferenceGameObject _cookingFireTemplate;

        [SerializeField]
        private AssetReferenceGameObject _kitchenMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerSummaryMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _playerUpgradesMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _altarMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerSelectMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _shareHouseMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _deadFollowerSelectMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _doctrineMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _doctrineChoicesMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _ritualsMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _sermonWheelTemplate;

        [SerializeField]
        private AssetReferenceGameObject _demonSummonMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _demonMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _prisonMenuControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _prisonerMenuControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _missionaryMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _missionMenuControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _refineryMenuControllerTemplate;

        [SerializeField]
        private AssetReferenceGameObject _tutorialMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _buildMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerFormsMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _appearanceMenuFormTemplate;

        [SerializeField]
        private AssetReferenceGameObject _appearanceMenuColourTemplate;

        [SerializeField]
        private AssetReferenceGameObject _appearanceMenuVariantTemplate;

        [SerializeField]
        private AssetReferenceGameObject _videoExportMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _sacrificeFollowerMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _newGameOptionsMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerKitchenMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _relicMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _photoGalleryMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _editPhotoMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _sandboxMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _cryptMenuTemplate;

        [Header("Overlays")]
        [SerializeField]
        private AssetReferenceGameObject _bugReportingOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _keyScreenOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _tutorialOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _recipeConfirmationOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _itemSelectorTemplate;

        [SerializeField]
        private AssetReferenceGameObject _difficultySelectorTemplate;

        [SerializeField]
        private AssetReferenceGameObject _tarotChoiceOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _fleeceTarotRewardOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerInteractionWheelTemplate;

        [SerializeField]
        private AssetReferenceGameObject _newItemOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _cultNameMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _mysticSellerNameMenuTemplate;

        [SerializeField]
        private AssetReferenceGameObject _deathScreenOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _adventureMapOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _fishingOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _cookingMinigameOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _loadingOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _bindingOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _dropdownOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _takePhotoOverlayTemplate;

        [SerializeField]
        private AssetReferenceGameObject _mysticShopOverlayTemplate;

        [Header("Knucklebones")]
        [SerializeField]
        private AssetReferenceGameObject _knucklebonesTemplate;

        [SerializeField]
        private AssetReferenceGameObject _knucklebonesBettingSelectorTemplate;

        [SerializeField]
        private AssetReferenceGameObject _knucklebonesOpponentSelectorTemplate;

        [Header("Other")]
        [SerializeField]
        private AssetReferenceGameObject _inventoryPromptTemplate;

        [SerializeField]
        private AssetReferenceGameObject _relicPickupPromptTemplate;

        [SerializeField]
        private AssetReferenceGameObject _weaponPickupPromptTemplate;

        [Header("Pooling")]
        [SerializeField]
        private AssetReferenceGameObject _followerInformationBoxTemplate;

        [SerializeField]
        private AssetReferenceGameObject _missionaryFollowerItemTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerColourItemTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerFormItemTemplate;

        [SerializeField]
        private AssetReferenceGameObject _followerVariantItemTemplate;

        [SerializeField]
        private AssetReferenceGameObject _adventureMapNodeTemplate;

        [SerializeField]
        private AssetReferenceGameObject _demonFollowerItemTemplate;

        [SerializeField]
        private AssetReferenceGameObject _deadFollowerItemTemplate;

        [SerializeField]
        private AssetReferenceGameObject _relicItemTemplate;

        [SerializeField]
        private AssetReferenceGameObject _photoItemTemplate;

        [Header("Twitch")]
        [SerializeField]
        private AssetReferenceGameObject _twitchFollowerSelect;

        [SerializeField]
        private AssetReferenceGameObject _twitchTotemWheel;

        [SerializeField]
        private AssetReferenceGameObject _twitchSettingsTemplate;

        [SerializeField]
        private AssetReferenceGameObject _twitchInformationBoxTemplate;

        [SerializeField]
        private AssetReferenceGameObject _twitchVotingOverlayTemplate;

        public bool MenusBlocked
        {
            get
            {
                if (!ForceBlockMenus && !MMTransition.IsPlaying && !(Time.timeScale < 0.25f) && !GameManager.InMenu)
                {
                    return UIMenuBase.ActiveMenus.Count > 0;
                }
                return true;
            }
        }

        public bool IsPaused { get; private set; }

        public UIPauseMenuController PauseMenuController { get; private set; }

        public UISettingsMenuController SettingsMenuControllerTemplate => _settingsMenuTemplate;

        public UICreditsMenuController CreditsMenuControllerTemplate { get; private set; }

        public UIPauseDetailsMenuController PauseDetailsMenuTemplate { get; private set; }

        public FollowerInformationBox FollowerInformationBoxTemplate { get; private set; }

        public UIFollowerIndoctrinationMenuController IndoctrinationMenuTemplate { get; private set; }

        public UIUpgradeTreeMenuController UpgradeTreeMenuTemplate { get; private set; }

        public UIUpgradePlayerTreeMenuController UpgradePlayerTreeMenuTemplate { get; private set; }

        public UIWorldMapMenuController WorldMapTemplate { get; private set; }

        public UITarotCardsMenuController TarotCardsMenuTemplate { get; private set; }

        public UICookingFireMenuController CookingFireMenuTemplate { get; private set; }

        public UIKitchenMenuController KitchenMenuTemplate { get; private set; }

        public UITutorialOverlayController TutorialOverlayTemplate { get; private set; }

        public UIKeyScreenOverlayController KeyScreenTemplate { get; private set; }

        public UIAltarMenuController AltarMenuTemplate { get; private set; }

        public UIPlayerUpgradesMenuController PlayerUpgradesMenuTemplate { get; private set; }

        public UIItemSelectorOverlayController ItemSelectorOverlayTemplate { get; private set; }

        public UIRecipeConfirmationOverlayController RecipeConfirmationTemplate { get; private set; }

        public UIDifficultySelectorOverlayController DifficultySelectorTemplate { get; private set; }

        public UIInventoryPromptOverlay InventoryPromptTemplate { get; private set; }

        public UIFollowerInteractionWheelOverlayController FollowerInteractionWheelTemplate { get; private set; }

        public UITarotChoiceOverlayController TarotChoiceOverlayTemplate { get; private set; }

        public UIFleeceTarotRewardOverlayController FleeceTarotChoiceOverlayTemplate { get; private set; }

        public UIFollowerSummaryMenuController FollowerSummaryMenuTemplate { get; private set; }

        public UIFollowerSelectMenuController FollowerSelectMenuTemplate { get; private set; }

        public UIDeadFollowerSelectMenu DeadFollowerSelectMenuTemplate { get; private set; }

        public UIDoctrineMenuController DoctrineMenuTemplate { get; private set; }

        public UIDoctrineChoicesMenuController DoctrineChoicesMenuTemplate { get; private set; }

        public UIRitualsMenuController RitualsMenuTemplate { get; private set; }

        public UISermonWheelController SermonWheelTemplate { get; private set; }

        public UIDemonSummonMenuController DemonSummonTemplate { get; private set; }

        public UIDemonMenuController DemonMenuTemplate { get; private set; }

        public UIPrisonMenuController PrisonMenuTemplate { get; private set; }

        public UIPrisonerMenuController PrisonerMenuTemplate { get; private set; }

        public UIMissionaryMenuController MissionaryMenuTemplate { get; private set; }

        public UIMissionMenuController MissionMenuTemplate { get; private set; }

        public UICultNameMenuController CultNameMenuTemplate { get; private set; }

        public UIMysticSellerNameMenuController MysticSellerNameMenuTemplate { get; private set; }

        public UIKnuckleBonesController KnucklebonesTemplate { get; private set; }

        public UIKnucklebonesBettingSelectionController KnucklebonesBettingSelectionTemplate { get; private set; }

        public UIKnucklebonesOpponentSelectionController KnucklebonesOpponentSelectionTemplate { get; private set; }

        public UIRefineryMenuController RefineryMenuTemplate { get; private set; }

        public UITutorialMenuController TutorialMenuTemplate { get; private set; }

        public UIMenuConfirmationWindow ConfirmationWindowTemplate => _confirmationWindowTemplate;

        public UIConfirmationCountdownWindow ConfirmationCountdownTemplate => _confirmationCountdownWindowTemplate;

        public UIDeathScreenOverlayController DeathScreenOverlayTemplate { get; private set; }

        public UIBuildMenuController BuildMenuTemplate { get; private set; }

        public UIFollowerFormsMenuController FollowerFormsMenuTemplate { get; private set; }

        public UINewItemOverlayController NewItemOverlayTemplate { get; private set; }

        public UIAppearanceMenuController_Form AppearanceMenuFormTemplate { get; private set; }

        public UIAppearanceMenuController_Colour AppearanceMenuColourTemplate { get; private set; }

        public UIAppearanceMenuController_Variant AppearanceMenuVariantTemplate { get; private set; }

        public IndoctrinationColourItem FollowerColourItemTemplate { get; private set; }

        public IndoctrinationFormItem FollowerFormItemTemplate { get; private set; }

        public IndoctrinationVariantItem FollowerVariantItemTemplate { get; private set; }

        public UIAdventureMapOverlayController AdventureMapOverlayTemplate { get; private set; }

        public AdventureMapNode AdventureMapNodeTemplate { get; private set; }

        public UIRoadmapOverlayController RoadmapOverlayControllerTemplate { get; private set; }

        public UIVideoExportMenuController VideoExportTemplate { get; private set; }

        public UIFishingOverlayController FishingOverlayControllerTemplate { get; private set; }

        public UICookingMinigameOverlayController CookingMinigameOverlayControllerTemplate { get; private set; }

        public MissionaryFollowerItem MissionaryFollowerItemTemplate { get; private set; }

        public DemonFollowerItem DemonFollowerItemTemplate { get; private set; }

        public DeadFollowerInformationBox DeadFollowerInformationBox { get; private set; }

        public UIFollowerKitchenMenuController FollowerKitchenMenuControllerTemplate { get; private set; }

        public UITwitchFollowerSelectOverlayController TwitchFollowerSelectOverlayController { get; private set; }

        public UITwitchTotemWheel TwitchTotemWheelController { get; private set; }

        public UITwitchSettingsMenuController TwitchSettingsMenuController { get; private set; }

        public TwitchInformationBox TwitchInformationBox { get; private set; }

      //  public UITwitchFollowerVotingOverlayController TwitchFollowerVotingOverlayController { get; private set; }

        public UIFollowerSelectMenuController SacrificeFollowerMenuTemplate { get; private set; }

        public UILoadingOverlayController LoadingOverlayControllerTemplate { get; private set; }

        public UIBugReportingOverlayController BugReportingOverlayTemplate { get; private set; }

        public UIControlBindingOverlayController BindingOverlayControllerTemplate { get; private set; }

        public UIRelicPickupPromptController RelicPickupPromptControllerTemplate { get; private set; }

        public UIWeaponPickupPromptController WeaponPickPromptControllerTemplate { get; private set; }

        public UIDropdownOverlayController DropdownOverlayControllerTemplate { get; private set; }

        public UINewGameOptionsMenuController NewGameOptionsMenuTemplate { get; private set; }

        public InputController XboxControllerTemplate { get; private set; }

        public InputController PS4ControllerTemplate { get; private set; }

        public InputController PS5ControllerTemplate { get; private set; }

        public InputController SwitchJoyConsTemplate { get; private set; }

        public InputController SwitchJoyConsDockedTemplate { get; private set; }

        public InputController SwitchHandheldTemplate { get; private set; }

        public InputController SwitchProControllerTemplate { get; private set; }

        public InputController KeyboardTemplate { get; private set; }

        public InputController MouseTemplate { get; private set; }

        public UIRelicMenuController RelicMenuTemplate { get; private set; }

        public RelicItem RelicItemTemplate { get; private set; }

        public UITakePhotoOverlayController TakePhotoOverlayTemplate { get; private set; }

        public UIPhotoGalleryMenuController PhotoGalleryMenuTemplate { get; private set; }

        public UIEditPhotoOverlayController EditPhotoMenuTemplate { get; private set; }

        public PhotoInformationBox PhotoInformationBoxTemplate { get; private set; }

        public UISandboxMenuController SandboxMenuTemplate { get; private set; }

        public UIShareHouseMenuController ShareHouseMenuTemplate { get; private set; }

        public UICryptMenuController CryptMenuTemplate { get; private set; }

        public UIMysticShopOverlayController MysticShopOverlayTemplate { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadUIManager()
        {
            AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Assets/UI/Prefabs/UIManager.prefab");
            asyncOperationHandle.Completed += delegate (AsyncOperationHandle<GameObject> asyncOperation)
            {
                asyncOperation.Result.GetComponent<UIManager>().Instantiate();
                Debug.Log("UI Manager Instantiated!".Colour(Color.cyan));
            };
        }

        public override void Awake()
        {
            base.Awake();
            LoadLifecyclePersistentAssets();
        }

        public async System.Threading.Tasks.Task LoadMainMenuAssets()
        {
            CreditsMenuControllerTemplate = await LoadAsset<UICreditsMenuController>(_creditsMenuControllerTemplate);
            RoadmapOverlayControllerTemplate = await LoadAsset<UIRoadmapOverlayController>(_roadmapOverlayControllerTemplate);
            LoadingOverlayControllerTemplate = await LoadAsset<UILoadingOverlayController>(_loadingOverlayTemplate);
            NewGameOptionsMenuTemplate = await LoadAsset<UINewGameOptionsMenuController>(_newGameOptionsMenuTemplate);
            DifficultySelectorTemplate = await LoadAsset<UIDifficultySelectorOverlayController>(_difficultySelectorTemplate);
        }

        public void UnloadMainMenuAssets()
        {
            UnloadAsset(CreditsMenuControllerTemplate);
            UnloadAsset(RoadmapOverlayControllerTemplate);
            UnloadAsset(LoadingOverlayControllerTemplate);
            UnloadAsset(NewGameOptionsMenuTemplate);
            UnloadAsset(DifficultySelectorTemplate);
        }

        public async System.Threading.Tasks.Task LoadKnucklebonesAssets()
        {
            KnucklebonesTemplate = await LoadAsset<UIKnuckleBonesController>(_knucklebonesTemplate);
            KnucklebonesBettingSelectionTemplate = await LoadAsset<UIKnucklebonesBettingSelectionController>(_knucklebonesBettingSelectorTemplate);
            KnucklebonesOpponentSelectionTemplate = await LoadAsset<UIKnucklebonesOpponentSelectionController>(_knucklebonesOpponentSelectorTemplate);
        }

        public void UnloadKnucklebonesAssets()
        {
            UnloadAsset(KnucklebonesTemplate);
            UnloadAsset(KnucklebonesBettingSelectionTemplate);
            UnloadAsset(KnucklebonesOpponentSelectionTemplate);
        }

        public async System.Threading.Tasks.Task LoadWorldMap()
        {
            WorldMapTemplate = await LoadAsset<UIWorldMapMenuController>(_worldMapTemplate);
        }

        public void UnloadWorldMap()
        {
            UnloadAsset(WorldMapTemplate);
        }

        public async System.Threading.Tasks.Task LoadLifecyclePersistentAssets()
        {
            DropdownOverlayControllerTemplate = await LoadAsset<UIDropdownOverlayController>(_dropdownOverlayTemplate);
            BindingOverlayControllerTemplate = await LoadAsset<UIControlBindingOverlayController>(_bindingOverlayTemplate);
            KeyboardTemplate = await LoadAsset<InputController>(_keyboardTemplate);
            MouseTemplate = await LoadAsset<InputController>(_mouseTemplate);
            XboxControllerTemplate = await LoadAsset<InputController>(_xboxControllerTemplate);
            PS4ControllerTemplate = await LoadAsset<InputController>(_ps4ControllerTemplate);
            PS5ControllerTemplate = await LoadAsset<InputController>(_ps5ControllerTemplate);
            SwitchProControllerTemplate = await LoadAsset<InputController>(_switchProControllerTemplate);
            SwitchJoyConsTemplate = await LoadAsset<InputController>(_switchJoyConsDetachedControllerTemplate);
            SwitchJoyConsDockedTemplate = await LoadAsset<InputController>(_switchJoyConsControllerTemplate);
            SwitchHandheldTemplate = await LoadAsset<InputController>(_switchHandheldControllerTemplate);
        }

        public async System.Threading.Tasks.Task LoadPersistentGameAssets()
        {
            PauseMenuController = await LoadAsset<UIPauseMenuController>(_pauseMenuTemplate);
            PauseDetailsMenuTemplate = await LoadAsset<UIPauseDetailsMenuController>(_pauseDetailsMenuTemplate);
            TutorialOverlayTemplate = await LoadAsset<UITutorialOverlayController>(_tutorialOverlayTemplate);
            TutorialMenuTemplate = await LoadAsset<UITutorialMenuController>(_tutorialMenuTemplate);
            FollowerSummaryMenuTemplate = await LoadAsset<UIFollowerSummaryMenuController>(_followerSummaryMenuTemplate);
            FollowerInformationBoxTemplate = await LoadAsset<FollowerInformationBox>(_followerInformationBoxTemplate);
            FollowerSelectMenuTemplate = await LoadAsset<UIFollowerSelectMenuController>(_followerSelectMenuTemplate);
            DeathScreenOverlayTemplate = await LoadAsset<UIDeathScreenOverlayController>(_deathScreenOverlayTemplate);
            TarotCardsMenuTemplate = await LoadAsset<UITarotCardsMenuController>(_tarotCardsMenuTemplate);
            KeyScreenTemplate = await LoadAsset<UIKeyScreenOverlayController>(_keyScreenOverlayTemplate);
            ItemSelectorOverlayTemplate = await LoadAsset<UIItemSelectorOverlayController>(_itemSelectorTemplate);
            NewItemOverlayTemplate = await LoadAsset<UINewItemOverlayController>(_newItemOverlayTemplate);
            TakePhotoOverlayTemplate = await LoadAsset<UITakePhotoOverlayController>(_takePhotoOverlayTemplate);
            PhotoGalleryMenuTemplate = await LoadAsset<UIPhotoGalleryMenuController>(_photoGalleryMenuTemplate);
            EditPhotoMenuTemplate = await LoadAsset<UIEditPhotoOverlayController>(_editPhotoMenuTemplate);
            PhotoInformationBoxTemplate = await LoadAsset<PhotoInformationBox>(_photoItemTemplate);
            BuildMenuTemplate = await LoadAsset<UIBuildMenuController>(_buildMenuTemplate);
            BugReportingOverlayTemplate = await LoadAsset<UIBugReportingOverlayController>(_bugReportingOverlayTemplate);
            TwitchSettingsMenuController = await LoadAsset<UITwitchSettingsMenuController>(_twitchSettingsTemplate);
           // TwitchFollowerVotingOverlayController = await LoadAsset<UITwitchFollowerVotingOverlayController>(_twitchVotingOverlayTemplate);
            RelicMenuTemplate = await LoadAsset<UIRelicMenuController>(_relicMenuTemplate);
            RelicItemTemplate = await LoadAsset<RelicItem>(_relicItemTemplate);
            await LoadWorldMap();
        }

        public void UnloadPersistentGameAssets()
        {
            UnloadAsset(PauseMenuController);
            UnloadAsset(PauseDetailsMenuTemplate);
            UnloadAsset(TutorialOverlayTemplate);
            UnloadAsset(TutorialMenuTemplate);
            UnloadAsset(FollowerSummaryMenuTemplate);
            UnloadAsset(FollowerInformationBoxTemplate);
            UnloadAsset(FollowerSelectMenuTemplate);
            UnloadAsset(DeathScreenOverlayTemplate);
            UnloadAsset(TarotCardsMenuTemplate);
            UnloadAsset(KeyScreenTemplate);
            UnloadAsset(ItemSelectorOverlayTemplate);
            UnloadAsset(NewItemOverlayTemplate);
            UnloadAsset(TakePhotoOverlayTemplate);
            UnloadAsset(PhotoGalleryMenuTemplate);
            UnloadAsset(EditPhotoMenuTemplate);
            UnloadAsset(PhotoInformationBoxTemplate);
            UnloadAsset(BuildMenuTemplate);
            UnloadAsset(BugReportingOverlayTemplate);
            UnloadAsset(RelicMenuTemplate);
            UnloadAsset(RelicItemTemplate);
            UnloadWorldMap();
            UnloadAsset(TwitchSettingsMenuController);
           // UnloadAsset(TwitchFollowerVotingOverlayController);
        }

        public async System.Threading.Tasks.Task LoadBaseAssets()
        {
            SeasonalEventData activeEvent = SeasonalEventManager.GetActiveEvent();
            if (activeEvent != null)
            {
                await activeEvent.LoadUIAssets();
            }
            FollowerInteractionWheelTemplate = await LoadAsset<UIFollowerInteractionWheelOverlayController>(_followerInteractionWheelTemplate);
            IndoctrinationMenuTemplate = await LoadAsset<UIFollowerIndoctrinationMenuController>(_indoctrinationMenuTemplate);
            SacrificeFollowerMenuTemplate = await LoadAsset<UIFollowerSelectMenuController>(_sacrificeFollowerMenuTemplate);
            UpgradeTreeMenuTemplate = await LoadAsset<UIUpgradeTreeMenuController>(_upgradeTreeTemplate);
            UpgradePlayerTreeMenuTemplate = await LoadAsset<UIUpgradePlayerTreeMenuController>(_upgradePlayerTreeTemplate);
            KitchenMenuTemplate = await LoadAsset<UIKitchenMenuController>(_kitchenMenuTemplate);
            PlayerUpgradesMenuTemplate = await LoadAsset<UIPlayerUpgradesMenuController>(_playerUpgradesMenuTemplate);
            AltarMenuTemplate = await LoadAsset<UIAltarMenuController>(_altarMenuTemplate);
            DoctrineMenuTemplate = await LoadAsset<UIDoctrineMenuController>(_doctrineMenuTemplate);
            DeadFollowerSelectMenuTemplate = await LoadAsset<UIDeadFollowerSelectMenu>(_deadFollowerSelectMenuTemplate);
            FollowerFormsMenuTemplate = await LoadAsset<UIFollowerFormsMenuController>(_followerFormsMenuTemplate);
            DoctrineChoicesMenuTemplate = await LoadAsset<UIDoctrineChoicesMenuController>(_doctrineChoicesMenuTemplate);
            RitualsMenuTemplate = await LoadAsset<UIRitualsMenuController>(_ritualsMenuTemplate);
            SermonWheelTemplate = await LoadAsset<UISermonWheelController>(_sermonWheelTemplate);
            DemonSummonTemplate = await LoadAsset<UIDemonSummonMenuController>(_demonSummonMenuTemplate);
            DemonMenuTemplate = await LoadAsset<UIDemonMenuController>(_demonMenuTemplate);
            PrisonMenuTemplate = await LoadAsset<UIPrisonMenuController>(_prisonMenuControllerTemplate);
            PrisonerMenuTemplate = await LoadAsset<UIPrisonerMenuController>(_prisonerMenuControllerTemplate);
            MissionaryMenuTemplate = await LoadAsset<UIMissionaryMenuController>(_missionaryMenuTemplate);
            MissionMenuTemplate = await LoadAsset<UIMissionMenuController>(_missionMenuControllerTemplate);
            RefineryMenuTemplate = await LoadAsset<UIRefineryMenuController>(_refineryMenuControllerTemplate);
            CultNameMenuTemplate = await LoadAsset<UICultNameMenuController>(_cultNameMenuTemplate);
            MysticSellerNameMenuTemplate = await LoadAsset<UIMysticSellerNameMenuController>(_mysticSellerNameMenuTemplate);
            AppearanceMenuColourTemplate = await LoadAsset<UIAppearanceMenuController_Colour>(_appearanceMenuColourTemplate);
            AppearanceMenuFormTemplate = await LoadAsset<UIAppearanceMenuController_Form>(_appearanceMenuFormTemplate);
            AppearanceMenuVariantTemplate = await LoadAsset<UIAppearanceMenuController_Variant>(_appearanceMenuVariantTemplate);
            CookingMinigameOverlayControllerTemplate = await LoadAsset<UICookingMinigameOverlayController>(_cookingMinigameOverlayTemplate);
            if (!DataManager.Instance.DifficultyChosen)
            {
                DifficultySelectorTemplate = await LoadAsset<UIDifficultySelectorOverlayController>(_difficultySelectorTemplate);
            }
            MissionaryFollowerItemTemplate = await LoadAsset<MissionaryFollowerItem>(_missionaryFollowerItemTemplate);
            FollowerColourItemTemplate = await LoadAsset<IndoctrinationColourItem>(_followerColourItemTemplate);
            FollowerFormItemTemplate = await LoadAsset<IndoctrinationFormItem>(_followerFormItemTemplate);
            FollowerVariantItemTemplate = await LoadAsset<IndoctrinationVariantItem>(_followerVariantItemTemplate);
            DemonFollowerItemTemplate = await LoadAsset<DemonFollowerItem>(_demonFollowerItemTemplate);
            DeadFollowerInformationBox = await LoadAsset<DeadFollowerInformationBox>(_deadFollowerItemTemplate);
            FollowerKitchenMenuControllerTemplate = await LoadAsset<UIFollowerKitchenMenuController>(_followerKitchenMenuTemplate);
            SandboxMenuTemplate = await LoadAsset<UISandboxMenuController>(_sandboxMenuTemplate);
            ShareHouseMenuTemplate = await LoadAsset<UIShareHouseMenuController>(_shareHouseMenuTemplate);
            CryptMenuTemplate = await LoadAsset<UICryptMenuController>(_cryptMenuTemplate);
            MysticShopOverlayTemplate = await LoadAsset<UIMysticShopOverlayController>(_mysticShopOverlayTemplate);
            TwitchFollowerSelectOverlayController = await LoadAsset<UITwitchFollowerSelectOverlayController>(_twitchFollowerSelect);
            TwitchTotemWheelController = await LoadAsset<UITwitchTotemWheel>(_twitchTotemWheel);
            TwitchInformationBox = await LoadAsset<TwitchInformationBox>(_twitchInformationBoxTemplate);
        }

        public void UnloadBaseAssets()
        {
            Debug.Log("Unload base assets".Colour(Color.red));
            SeasonalEventData activeEvent = SeasonalEventManager.GetActiveEvent();
            if (activeEvent != null)
            {
                activeEvent.UnloadUIAssets();
            }
            UnloadAsset(IndoctrinationMenuTemplate);
            UnloadAsset(SacrificeFollowerMenuTemplate);
            UnloadAsset(UpgradeTreeMenuTemplate);
            UnloadAsset(UpgradePlayerTreeMenuTemplate);
            UnloadAsset(KitchenMenuTemplate);
            UnloadAsset(PlayerUpgradesMenuTemplate);
            UnloadAsset(AltarMenuTemplate);
            UnloadAsset(AppearanceMenuColourTemplate);
            UnloadAsset(AppearanceMenuFormTemplate);
            UnloadAsset(AppearanceMenuVariantTemplate);
            UnloadAsset(CultNameMenuTemplate);
            UnloadAsset(MysticSellerNameMenuTemplate);
            UnloadAsset(DoctrineMenuTemplate);
            UnloadAsset(FollowerFormsMenuTemplate);
            UnloadAsset(DoctrineChoicesMenuTemplate);
            UnloadAsset(RitualsMenuTemplate);
            UnloadAsset(SermonWheelTemplate);
            UnloadAsset(DemonSummonTemplate);
            UnloadAsset(DemonMenuTemplate);
            UnloadAsset(PrisonMenuTemplate);
            UnloadAsset(DeadFollowerSelectMenuTemplate);
            UnloadAsset(DeadFollowerInformationBox);
            UnloadAsset(PrisonerMenuTemplate);
            UnloadAsset(MissionaryMenuTemplate);
            UnloadAsset(MissionMenuTemplate);
            UnloadAsset(RefineryMenuTemplate);
            UnloadAsset(CookingMinigameOverlayControllerTemplate);
            UnloadAsset(DifficultySelectorTemplate);
            UnloadAsset(MissionaryFollowerItemTemplate);
            UnloadAsset(FollowerColourItemTemplate);
            UnloadAsset(FollowerFormItemTemplate);
            UnloadAsset(FollowerVariantItemTemplate);
            UnloadAsset(DemonFollowerItemTemplate);
            UnloadAsset(FollowerInteractionWheelTemplate);
            UnloadAsset(ShareHouseMenuTemplate);
            UnloadAsset(CryptMenuTemplate);
            UnloadAsset(MysticShopOverlayTemplate);
            UnloadAsset(FollowerKitchenMenuControllerTemplate);
            UnloadAsset(SandboxMenuTemplate);
            UnloadAsset(TwitchFollowerSelectOverlayController);
            UnloadAsset(TwitchTotemWheelController);
            UnloadAsset(TwitchInformationBox);
        }

        public async System.Threading.Tasks.Task LoadDungeonAssets()
        {
            InventoryPromptTemplate = await LoadAsset<UIInventoryPromptOverlay>(_inventoryPromptTemplate);
            AdventureMapOverlayTemplate = await LoadAsset<UIAdventureMapOverlayController>(_adventureMapOverlayTemplate);
            TarotChoiceOverlayTemplate = await LoadAsset<UITarotChoiceOverlayController>(_tarotChoiceOverlayTemplate);
            FleeceTarotChoiceOverlayTemplate = await LoadAsset<UIFleeceTarotRewardOverlayController>(_fleeceTarotRewardOverlayTemplate);
            AdventureMapNodeTemplate = await LoadAsset<AdventureMapNode>(_adventureMapNodeTemplate);
            RelicPickupPromptControllerTemplate = await LoadAsset<UIRelicPickupPromptController>(_relicPickupPromptTemplate);
            WeaponPickPromptControllerTemplate = await LoadAsset<UIWeaponPickupPromptController>(_weaponPickupPromptTemplate);
        }

        public void UnloadDungeonAssets()
        {
            UnloadAsset(InventoryPromptTemplate);
            UnloadAsset(AdventureMapOverlayTemplate);
            UnloadAsset(TarotChoiceOverlayTemplate);
            UnloadAsset(FleeceTarotChoiceOverlayTemplate);
            UnloadAsset(AdventureMapNodeTemplate);
            UnloadAsset(RelicPickupPromptControllerTemplate);
            UnloadAsset(WeaponPickPromptControllerTemplate);
        }

        public async System.Threading.Tasks.Task LoadHubShoreAssets()
        {
            FishingOverlayControllerTemplate = await LoadAsset<UIFishingOverlayController>(_fishingOverlayTemplate);
        }

        public void UnloadHubShoreAssets()
        {
            UnloadAsset(FishingOverlayControllerTemplate);
        }

        public static async Task<T> LoadAsset<T>(AssetReferenceGameObject assetReferenceGameObject) where T : MonoBehaviour
        {
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.LoadAssetAsync<GameObject>(assetReferenceGameObject);
            await asyncOperation.Task;
            T component = asyncOperation.Result.GetComponent<T>();
            if ((UnityEngine.Object)component != (UnityEngine.Object)null)
            {
                if (MonoSingleton<UIManager>.Instance._addressablesTracker.ContainsKey(component.gameObject))
                {
                    throw new Exception("UIManager - LoadAsset - Prefab " + component.name + " already loaded! Check asset loading!");
                }
                MonoSingleton<UIManager>.Instance._addressablesTracker.Add(component.gameObject, asyncOperation);
                return component;
            }
            return null;
        }

        public static void UnloadAsset<T>(T asset) where T : MonoBehaviour
        {
            if (!((UnityEngine.Object)asset == (UnityEngine.Object)null))
            {
                if (MonoSingleton<UIManager>.Instance._addressablesTracker.TryGetValue(asset.gameObject, out var value))
                {
                    MonoSingleton<UIManager>.Instance._addressablesTracker.Remove(asset.gameObject);
                    Addressables.Release(value);
                }
                asset = null;
            }
        }

        public override void Start()
        {
            base.Start();
            base.transform.SetParent(null);
            _cursorSize = _cursor.width;
            UpdateCursorIfRequired();
        }

        private void UpdateCursorIfRequired()
        {
            if (_currentResolution.width != Screen.width || _currentResolution.height != Screen.height)
            {
                _currentResolution.width = Screen.width;
                _currentResolution.height = Screen.height;
                if (_currentCursorTexture != null)
                {
                    UnityEngine.Object.Destroy(_currentCursorTexture);
                    _currentCursorTexture = null;
                }
                float num = Mathf.Min((float)_currentResolution.height / 1440f, 1f);
                int num2 = Mathf.RoundToInt((float)_cursorSize * num);
                RenderTexture renderTexture = new RenderTexture(num2, num2, 24)
                {
                    name = "Cursor_Render_Texture" + Time.frameCount
                };
                RenderTexture active = RenderTexture.active;
                RenderTexture.active = renderTexture;
                Graphics.Blit(_cursor, renderTexture);
                _currentCursorTexture = new Texture2D(num2, num2, TextureFormat.RGBA32, mipChain: false)
                {
                    name = "Cursor_Texture" + Time.frameCount
                };
                _currentCursorTexture.ReadPixels(new Rect(0f, 0f, num2, num2), 0, 0, recalculateMipMaps: false);
                _currentCursorTexture.Apply();
                RenderTexture.active = active;
                renderTexture.Release();
                renderTexture.DiscardContents();
                UnityEngine.Object.Destroy(renderTexture);
                renderTexture = null;
                Cursor.SetCursor(_currentCursorTexture, new Vector2(18f * num, 6f * num), CursorMode.ForceSoftware);
            }
        }

        public void Update()
        {
            GameManager instance = GameManager.GetInstance();
            if (!MenusBlocked && instance != null && _currentInstance == null)
            {
                if (InputManager.Gameplay.GetPauseButtonDown() && !CheatConsole.IN_DEMO)
                {
                    ShowPauseMenu();
                }
                else if (InputManager.Gameplay.GetMenuButtonDown() && DataManager.Instance.HadInitialDeathCatConversation)
                {
                    ShowDetailsMenu();
                }
            }
            UpdateCursorIfRequired();
        }

        public void ShowPauseMenu()
        {
            IsPaused = true;
            UIPauseMenuController uIPauseMenuController = SetMenu(PauseMenuController, 0f, filter: true);
            uIPauseMenuController.OnHidden = (Action)Delegate.Combine(uIPauseMenuController.OnHidden, (Action)delegate
            {
                IsPaused = false;
            });
        }

        public void ShowDetailsMenu()
        {
            IsPaused = true;
            UIPauseDetailsMenuController uIPauseDetailsMenuController = SetMenu(PauseDetailsMenuTemplate, 0f, filter: true);
            uIPauseDetailsMenuController.OnHidden = (Action)Delegate.Combine(uIPauseDetailsMenuController.OnHidden, (Action)delegate
            {
                IsPaused = false;
                if (!DataManager.Instance.ShownInventoryTutorial && UIInventoryPromptOverlay.Showing)
                {
                    GameManager.GetInstance().OnConversationEnd();
                    GameManager.GetInstance().AddPlayerToCamera();
                    DataManager.Instance.ShownInventoryTutorial = true;
                    CameraFollowTarget.Instance.SetOffset(Vector3.zero);
                    UIInventoryPromptOverlay.Showing = false;
                }
            });
        }

        public UIKeyScreenOverlayController ShowKeyScreen()
        {
            return SetMenu(KeyScreenTemplate);
        }

        public UIWorldMapMenuController ShowWorldMap()
        {
            return SetMenu(WorldMapTemplate);
        }

        public UITutorialOverlayController ShowTutorialOverlay(TutorialTopic topic, float delay = 0f)
        {
            UITutorialOverlayController uITutorialOverlayController = TutorialOverlayTemplate.Instantiate();
            uITutorialOverlayController.Show(topic, delay);
            return SetMenuInstance(uITutorialOverlayController) as UITutorialOverlayController;
        }

        public UIAltarMenuController ShowAltarMenu()
        {
            return SetMenu(AltarMenuTemplate);
        }

        public UIUpgradeTreeMenuController ShowUpgradeTree(Action closeCallback = null)
        {
            UIUpgradeTreeMenuController upgradeTreeInstance = UpgradeTreeMenuTemplate.Instantiate();
            HUD_Manager.Instance.Hide(Snap: true);
            upgradeTreeInstance.Show();
            UIUpgradeTreeMenuController uIUpgradeTreeMenuController = upgradeTreeInstance;
            uIUpgradeTreeMenuController.OnHidden = (Action)Delegate.Combine(uIUpgradeTreeMenuController.OnHidden, (Action)delegate
            {
                GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
                HUD_Manager.Instance.Show(0);
                closeCallback?.Invoke();
            });
            UIUpgradeTreeMenuController uIUpgradeTreeMenuController2 = upgradeTreeInstance;
            uIUpgradeTreeMenuController2.OnUpgradeUnlocked = (Action<UpgradeSystem.Type>)Delegate.Combine(uIUpgradeTreeMenuController2.OnUpgradeUnlocked, (Action<UpgradeSystem.Type>)delegate (UpgradeSystem.Type upgrade)
            {
                if (upgrade == UpgradeSystem.Type.Building_Temple2 || upgrade == UpgradeSystem.Type.Temple_III || upgrade == UpgradeSystem.Type.Temple_IV)
                {
                    upgradeTreeInstance.Hide();
                }
            });
            SetMenuInstance(upgradeTreeInstance);
            return upgradeTreeInstance;
        }

        public UIUpgradePlayerTreeMenuController ShowPlayerUpgradeTree()
        {
            UIUpgradePlayerTreeMenuController upgradeTreeInstance = UpgradePlayerTreeMenuTemplate.Instantiate();
            upgradeTreeInstance.Show();
            UIUpgradePlayerTreeMenuController uIUpgradePlayerTreeMenuController = upgradeTreeInstance;
            uIUpgradePlayerTreeMenuController.OnHidden = (Action)Delegate.Combine(uIUpgradePlayerTreeMenuController.OnHidden, (Action)delegate
            {
                GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
            });
            UIUpgradePlayerTreeMenuController uIUpgradePlayerTreeMenuController2 = upgradeTreeInstance;
            uIUpgradePlayerTreeMenuController2.OnUpgradeUnlocked = (Action<UpgradeSystem.Type>)Delegate.Combine(uIUpgradePlayerTreeMenuController2.OnUpgradeUnlocked, (Action<UpgradeSystem.Type>)delegate
            {
                upgradeTreeInstance.Hide();
            });
            SetMenuInstance(upgradeTreeInstance);
            return upgradeTreeInstance;
        }

        public UICookingFireMenuController ShowCookingFireMenu(StructuresData cookingFireData)
        {
            UICookingFireMenuController uICookingFireMenuController = CookingFireMenuTemplate.Instantiate();
            uICookingFireMenuController.Show(cookingFireData);
            SetMenuInstance(uICookingFireMenuController);
            return uICookingFireMenuController;
        }

        public UIKitchenMenuController ShowKitchenMenu(StructuresData kitchenData)
        {
            UIKitchenMenuController uIKitchenMenuController = KitchenMenuTemplate.Instantiate();
            uIKitchenMenuController.Show(kitchenData);
            SetMenuInstance(uIKitchenMenuController);
            return uIKitchenMenuController;
        }

        public UIFollowerSummaryMenuController ShowFollowerSummaryMenu(Follower follower)
        {
            UIFollowerSummaryMenuController uIFollowerSummaryMenuController = FollowerSummaryMenuTemplate.Instantiate();
            uIFollowerSummaryMenuController.Show(follower);
            SetMenuInstance(uIFollowerSummaryMenuController);
            return uIFollowerSummaryMenuController;
        }

        public UIPlayerUpgradesMenuController ShowPlayerUpgradesMenu()
        {
            UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController = PlayerUpgradesMenuTemplate.Instantiate();
            if (!DataManager.Instance.PostGameFleecesOnboarded && DataManager.Instance.DeathCatBeaten)
            {
                DataManager.Instance.PostGameFleecesOnboarded = true;
                uIPlayerUpgradesMenuController.ShowNewFleecesUnlocked(new PlayerFleeceManager.FleeceType[4]
                {
                PlayerFleeceManager.FleeceType.CurseInsteadOfWeapon,
                PlayerFleeceManager.FleeceType.OneHitKills,
                PlayerFleeceManager.FleeceType.HollowHeal,
                PlayerFleeceManager.FleeceType.NoRolling
                });
            }
            else
            {
                uIPlayerUpgradesMenuController.Show();
            }
            uIPlayerUpgradesMenuController.OnHidden = (Action)Delegate.Combine(uIPlayerUpgradesMenuController.OnHidden, (Action)delegate
            {
                GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
            });
            return SetMenuInstance(uIPlayerUpgradesMenuController, 1f) as UIPlayerUpgradesMenuController;
        }

        public UIItemSelectorOverlayController ShowItemSelector(List<InventoryItem.ITEM_TYPE> items, ItemSelector.Params parameters)
        {
            UIItemSelectorOverlayController uIItemSelectorOverlayController = ItemSelectorOverlayTemplate.Instantiate();
            uIItemSelectorOverlayController.Show(items, parameters);
            SetMenuInstance(uIItemSelectorOverlayController, 1f);
            return uIItemSelectorOverlayController;
        }

        public UIItemSelectorOverlayController ShowItemSelector(List<InventoryItem> items, ItemSelector.Params parameters)
        {
            UIItemSelectorOverlayController uIItemSelectorOverlayController = ItemSelectorOverlayTemplate.Instantiate();
            uIItemSelectorOverlayController.Show(items, parameters);
            SetMenuInstance(uIItemSelectorOverlayController, 1f);
            return uIItemSelectorOverlayController;
        }

        public UIDifficultySelectorOverlayController ShowDifficultySelector()
        {
            UIDifficultySelectorOverlayController uIDifficultySelectorOverlayController = DifficultySelectorTemplate.Instantiate();
            uIDifficultySelectorOverlayController.Show(cancellable: false, DataManager.Instance.PermadeDeathActive);
            SetMenuInstance(uIDifficultySelectorOverlayController);
            return uIDifficultySelectorOverlayController;
        }

        public UITarotChoiceOverlayController ShowTarotChoice(TarotCards.TarotCard card1, TarotCards.TarotCard card2)
        {
            UITarotChoiceOverlayController uITarotChoiceOverlayController = TarotChoiceOverlayTemplate.Instantiate();
            uITarotChoiceOverlayController.Show(card1, card2);
            SetMenuInstance(uITarotChoiceOverlayController, 1f);
            return uITarotChoiceOverlayController;
        }

        public UIFleeceTarotRewardOverlayController ShowFleeceTarotReward(TarotCards.TarotCard card1, TarotCards.TarotCard card2, TarotCards.TarotCard card3, TarotCards.TarotCard card4)
        {
            UIFleeceTarotRewardOverlayController uIFleeceTarotRewardOverlayController = FleeceTarotChoiceOverlayTemplate.Instantiate();
            uIFleeceTarotRewardOverlayController.Show(card1, card2, card3, card4);
            SetMenuInstance(uIFleeceTarotRewardOverlayController, 1f);
            return uIFleeceTarotRewardOverlayController;
        }

        public UIFollowerIndoctrinationMenuController ShowIndoctrinationMenu(Follower follower)
        {
            UIFollowerIndoctrinationMenuController uIFollowerIndoctrinationMenuController = IndoctrinationMenuTemplate.Instantiate();
            uIFollowerIndoctrinationMenuController.Show(follower);
            SetMenuInstance(uIFollowerIndoctrinationMenuController);
            return uIFollowerIndoctrinationMenuController;
        }

        public UINewItemOverlayController ShowNewItemOverlay()
        {
            UINewItemOverlayController uINewItemOverlayController = NewItemOverlayTemplate.Instantiate();
            SetMenuInstance(uINewItemOverlayController, 1f);
            return uINewItemOverlayController;
        }

        public UIDeathScreenOverlayController ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results result, int levels)
        {
            UIDeathScreenOverlayController uIDeathScreenOverlayController = DeathScreenOverlayTemplate.Instantiate();
            uIDeathScreenOverlayController.Show(result, levels);
            DoShowDeathScreen(uIDeathScreenOverlayController);
            return uIDeathScreenOverlayController;
        }

        public UIDeathScreenOverlayController ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results result)
        {
            UIDeathScreenOverlayController uIDeathScreenOverlayController = DeathScreenOverlayTemplate.Instantiate();
            uIDeathScreenOverlayController.Show(result);
            DoShowDeathScreen(uIDeathScreenOverlayController);
            return uIDeathScreenOverlayController;
        }

        private void DoShowDeathScreen(UIDeathScreenOverlayController deathScreenInstance)
        {
            SetMenuInstance(deathScreenInstance);
        }

        public UIAdventureMapOverlayController ShowAdventureMap(global::Map.Map map, bool disableInput)
        {
            UIAdventureMapOverlayController uIAdventureMapOverlayController = AdventureMapOverlayTemplate.Instantiate();
            uIAdventureMapOverlayController.Show(map, disableInput);
            SetMenuInstance(uIAdventureMapOverlayController, 0f, filter: true);
            return uIAdventureMapOverlayController;
        }

        public UIBuildMenuController ShowBuildMenu(StructureBrain.TYPES structureToReveal)
        {
            UIBuildMenuController uIBuildMenuController = BuildMenuTemplate.Instantiate();
            uIBuildMenuController.Show(structureToReveal);
            SetMenuInstance(uIBuildMenuController);
            return uIBuildMenuController;
        }

        private T SetMenu<T>(T template, float timeScale = 0f, bool filter = false) where T : UIMenuBase
        {
            T val = template.Instantiate();
            val.Show();
            SetMenuInstance(val, timeScale, filter);
            return val;
        }

        public UIMenuBase SetMenuInstance(UIMenuBase menu, float timeScale = 0f, bool filter = false)
        {
            if (_currentInstance == null)
            {
                if (filter)
                {
                    AudioManager.Instance.ToggleFilter(SoundParams.Filter, toggle: true);
                }
                Time.timeScale = timeScale;
                _currentInstance = menu;
                UIMenuBase currentInstance = _currentInstance;
                currentInstance.OnHidden = (Action)Delegate.Combine(currentInstance.OnHidden, (Action)delegate
                {
                    if (filter)
                    {
                        AudioManager.Instance.ToggleFilter(SoundParams.Filter, toggle: false);
                    }
                    Time.timeScale = 1f;
                    _currentInstance = null;
                    GameManager.InMenu = false;
                });
                return _currentInstance;
            }
            return null;
        }

        public static bool GetActiveMenu<T>(out T menu) where T : UIMenuBase
        {
            menu = GetActiveMenu<T>();
            if ((UnityEngine.Object)menu != (UnityEngine.Object)null)
            {
                return true;
            }
            return false;
        }

        public static T GetActiveMenu<T>() where T : UIMenuBase
        {
            foreach (UIMenuBase activeMenu in UIMenuBase.ActiveMenus)
            {
                if (activeMenu is T result)
                {
                    return result;
                }
            }
            return null;
        }

        public static void DeactivateActiveMenu()
        {
            foreach (UIMenuBase activeMenu in UIMenuBase.ActiveMenus)
            {
                activeMenu.Hide(immediate: true);
            }
        }

        public static void PlayAudio(string soundPath)
        {
            if (PlayerFarming.Instance != null)
            {
                AudioManager.Instance.PlayOneShot(soundPath, PlayerFarming.Instance.gameObject);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(soundPath);
            }
        }

        public static EventInstance CreateLoop(string soundPath)
        {
            if (PlayerFarming.Instance != null)
            {
                return AudioManager.Instance.CreateLoop(soundPath, PlayerFarming.Instance.gameObject, playLoop: true);
            }
            return AudioManager.Instance.CreateLoop(soundPath, playLoop: true);
        }
    }
}