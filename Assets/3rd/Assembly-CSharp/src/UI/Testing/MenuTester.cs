using System;
using System.Collections.Generic;
using System.Linq;
using Lamb.UI;
using Lamb.UI.AltarMenu;
using Lamb.UI.BuildMenu;
using Lamb.UI.DeathScreen;
using Lamb.UI.FollowerInteractionWheel;
using Lamb.UI.FollowerSelect;
using Lamb.UI.KitchenMenu;
using Lamb.UI.MainMenu;
using Lamb.UI.Menus.DoctrineChoicesMenu;
using Lamb.UI.Menus.PlayerMenu;
using Lamb.UI.Mission;
using Lamb.UI.PauseMenu;
using Lamb.UI.PrisonerMenu;
using Lamb.UI.RefineryMenu;
using Lamb.UI.Rituals;
using Lamb.UI.SermonWheelOverlay;
using Lamb.UI.SettingsMenu;
using Lamb.UI.UpgradeMenu;
using Lamb.UI.VideoMenu;
using Map;
using MMBiomeGeneration;
using MMTools;
using src.Extensions;
using src.UI.Menus;
using src.UI.Menus.CryptMenu;
using src.UI.Overlays.TutorialOverlay;
using src.Utilities;
using UI.Menus.TwitchMenu;
using UnityEngine;

namespace src.UI.Testing
{
	public class MenuTester : MonoBehaviour
	{
		private UIMenuBase _testInstance;

		[SerializeField]
		private UIMainMenuController _mainMenuTemplate;

		[SerializeField]
		private UISettingsMenuController _settingsMenuTemplate;

		[SerializeField]
		private UIPauseMenuController _pauseMenuTemplate;

		[SerializeField]
		private UICreditsMenuController _creditsMenuTemplate;

		[SerializeField]
		private UIBuildMenuController _buildMenuTemplate;

		[SerializeField]
		private UIFollowerSelectMenuController _followerSelectMenuTemplate;

		[SerializeField]
		private UIDeadFollowerSelectMenu _deadFollowerSelectMenuTemplate;

		[SerializeField]
		private UICryptMenuController _cryptMenuControllerTemplate;

		[SerializeField]
		private UIFollowerIndoctrinationMenuController _followerIndoctrinationMenuTemplate;

		[SerializeField]
		private UICultNameMenuController _uiCultNameMenuTemplate;

		[SerializeField]
		private UIPauseDetailsMenuController _pauseDetailsMenuTemplate;

		[SerializeField]
		private UIFollowerFormsMenuController _followerFormsMenuTemplate;

		[SerializeField]
		private UITarotCardsMenuController _tarotCardsMenuTemplate;

		[SerializeField]
		private UIRelicMenuController _relicMenuControllerTemplate;

		[SerializeField]
		private UIUpgradeShopMenuController _upgradeShopMenuTemplate;

		[SerializeField]
		private UIWorldMapMenuController _worldMapMenuController;

		[SerializeField]
		private UITutorialMenuController _tutorialMenuTemplate;

		[SerializeField]
		private UIUpgradeTreeMenuController _upgradeTreeTemplate;

		[SerializeField]
		private UIUpgradePlayerTreeMenuController _playerUpgradeTreeTemplate;

		[SerializeField]
		private UIRefineryMenuController _refineryMenuControllerTemplate;

		[SerializeField]
		private UICookingFireMenuController _cookingFireMenuTemplate;

		[SerializeField]
		private UIKitchenMenuController _kitchenMenuTemplate;

		[SerializeField]
		private UIFollowerSummaryMenuController _followerSummaryMenuTemplate;

		[SerializeField]
		private UIVideoExportMenuController _uiVideoExportMenuController;

		[SerializeField]
		private UISandboxMenuController _uiSandboxMenuControllerTemplate;

		[SerializeField]
		private UIPrisonMenuController _prisonMenuTemplate;

		[SerializeField]
		private UIPrisonerMenuController _prisonerMenuTemplate;

		[SerializeField]
		private UIMissionaryMenuController _missionaryMenuTemplate;

		[SerializeField]
		private UIMissionMenuController _missionMenuTemplate;

		[SerializeField]
		private UIDemonSummonMenuController _demonSummonMenuTemplate;

		[SerializeField]
		private UIDemonMenuController _demonMenuTemplate;

		[SerializeField]
		private UIMenuConfirmationWindow _confirmationWindowTemplate;

		[SerializeField]
		private UIConfirmationCountdownWindow _confirmationCountdownWindowTemplate;

		[SerializeField]
		private UISaveErrorMenuController _saveErrorWindowTemplate;

		[SerializeField]
		private UIDoctrineMenuController _doctrineMenuTemplate;

		[SerializeField]
		private UIAltarMenuController _altarMenuTemplate;

		[SerializeField]
		private UIDoctrineChoicesMenuController _doctrineChoicesMenuTemplate;

		[SerializeField]
		private UIRitualsMenuController _ritualMenuTemplate;

		[SerializeField]
		private UIHeartsOfTheFaithfulChoiceMenuController _heartsOfTheFaithfulChoiceMenuController;

		[SerializeField]
		private UIPlayerUpgradesMenuController _playerUpgradesMenuControllerTemplate;

		[SerializeField]
		private UISermonWheelController _sermonWheelTemplate;

		[SerializeField]
		private UIFollowerInteractionWheelOverlayController _followerInteractionWheelTemplate;

		[SerializeField]
		private UICurseWheelController _curseWheelOverlayTemplate;

		[SerializeField]
		private UIDeathScreenOverlayController _deathScreenOverlayTemplate;

		[SerializeField]
		private UIKeyScreenOverlayController _keyScreenTemplate;

		[SerializeField]
		private UIWeaponWheelController _weaponWheelOverlayTemplate;

		[SerializeField]
		private UIUpgradeUnlockOverlayControllerBase _ugpradeUnlockOverlaytemplate;

		[SerializeField]
		private UITutorialOverlayController _tutorialOverlayTemplate;

		[SerializeField]
		private UIRecipeConfirmationOverlayController _recipeConfirmationOverlayTemplate;

		[SerializeField]
		private UIItemSelectorOverlayController _itemSelectorOverlayTemplate;

		[SerializeField]
		private UIDifficultySelectorOverlayController _difficultyOverlayTemplate;

		[SerializeField]
		private UITarotChoiceOverlayController _tarotChoiceOverlayTemplate;

		[SerializeField]
		private UIFleeceTarotRewardOverlayController fleeceTarotRewardOverlayController;

		[SerializeField]
		private UIControlBindingOverlayController _controlBindingOverlayController;

		[SerializeField]
		private UIAdventureMapOverlayController _adventureMapTemplate;

		[SerializeField]
		private UIRoadmapOverlayController _roadmapOverlayController;

		[SerializeField]
		private UIBugReportingOverlayController _bugReportingOverlayController;

		[SerializeField]
		private UIMysticShopOverlayController _mysticShopOverlayController;

		[SerializeField]
		private UIPhotoGalleryMenuController _photoGalleryMenuController;

		[SerializeField]
		private UIEditPhotoOverlayController _editPhotoOverlayController;

		[SerializeField]
		private UIKnuckleBonesController _knuckleBonesTemplate;

		[SerializeField]
		private KnucklebonesPlayerConfiguration[] _opponentConfigurations;

		[SerializeField]
		private UIKnucklebonesBettingSelectionController _kbBettingTemplate;

		[SerializeField]
		private UIKnucklebonesOpponentSelectionController _kbOpponentTemplate;

		[SerializeField]
		private UITwitchTotemWheel _twitchTotemWheelTemplate;

		[SerializeField]
		private UITwitchSettingsMenuController _uiTwitchMenuSettingsController;

		public void TestMainMenu()
		{
			ShowTestMenu(_mainMenuTemplate);
		}

		public void TestSettingsMenu()
		{
			ShowTestMenu(_settingsMenuTemplate);
		}

		public void TestPauseMenu()
		{
			ShowTestMenu(_pauseMenuTemplate);
		}

		public void TestCreditsMenu()
		{
			ShowTestMenu(_creditsMenuTemplate);
		}

		public void TestBuidMenu()
		{
			ShowTestMenu(_buildMenuTemplate);
		}

		public void TestBuildMenuReveal()
		{
			StructureBrain.TYPES tYPES = AestheticCategory.AllStructures().RandomElement();
			DataManager.Instance.UnlockedStructures.Add(tYPES);
			UIBuildMenuController uIBuildMenuController = _buildMenuTemplate.Instantiate();
			uIBuildMenuController.Show(tYPES);
			SetInstance(uIBuildMenuController);
		}

		public void TestFollowerSelectmenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			List<FollowerInfo> list = new List<FollowerInfo>();
			for (int i = 0; i < 10; i++)
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				followerInfo.Traits.Add(array.RandomElement());
				list.Add(followerInfo);
			}
			UIFollowerSelectMenuController uIFollowerSelectMenuController = _followerSelectMenuTemplate.Instantiate();
			uIFollowerSelectMenuController.Show(list);
			uIFollowerSelectMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIFollowerSelectMenuController;
		}

		public void TestDeadFollowerSelect()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			List<FollowerInfo> list = new List<FollowerInfo>();
			for (int i = 0; i < 10; i++)
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				followerInfo.Traits.Add(array.RandomElement());
				list.Add(followerInfo);
			}
			UIDeadFollowerSelectMenu uIDeadFollowerSelectMenu = _deadFollowerSelectMenuTemplate.Instantiate();
			uIDeadFollowerSelectMenu.Show(list, null, false, false);
			_testInstance = uIDeadFollowerSelectMenu;
		}

		public void TestCryptMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			List<FollowerInfo> list = new List<FollowerInfo>();
			for (int i = 0; i < 10; i++)
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				followerInfo.Traits.Add(array.RandomElement());
				list.Add(followerInfo);
			}
			UICryptMenuController uICryptMenuController = _cryptMenuControllerTemplate.Instantiate();
			uICryptMenuController.Show(list, null, false, false);
			_testInstance = uICryptMenuController;
		}

		public void TestFollowerIndoctrinationMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.None);
			followerInfo.Traits.Add(array.RandomElement());
			FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(followerInfo);
			Follower follower = FollowerManager.FollowerPrefab.Instantiate();
			follower.name = "Follower " + orCreateBrain.Info.Name;
			follower.Init(orCreateBrain, orCreateBrain.CreateOutfit());
			follower.transform.position = Vector3.zero;
			UIFollowerIndoctrinationMenuController uIFollowerIndoctrinationMenuController = _followerIndoctrinationMenuTemplate.Instantiate();
			uIFollowerIndoctrinationMenuController.Show(follower);
			uIFollowerIndoctrinationMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerIndoctrinationMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
				UnityEngine.Object.Destroy(follower.gameObject);
			});
			_testInstance = uIFollowerIndoctrinationMenuController;
		}

		public void TestCultNameMenu()
		{
			ShowTestMenu(_uiCultNameMenuTemplate);
		}

		public void TestPauseDetailsMenu()
		{
			ShowTestMenu(_pauseDetailsMenuTemplate);
		}

		public void TestFollowerFormsMenu()
		{
			ShowTestMenu(_followerFormsMenuTemplate);
		}

		public void TestTarotCardsMenu()
		{
			ShowTestMenu(_tarotCardsMenuTemplate);
		}

		public void TestTarotCardsMenuNewUnlock()
		{
			if (_tarotCardsMenuTemplate != null && _testInstance == null)
			{
				Time.timeScale = 0f;
				TarotCards.Card card = DataManager.AllTrinkets.RandomElement();
				TarotCards.UnlockTrinket(card);
				UITarotCardsMenuController uITarotCardsMenuController = _tarotCardsMenuTemplate.Instantiate();
				uITarotCardsMenuController.Show(card);
				_testInstance = uITarotCardsMenuController;
				UIMenuBase testInstance = _testInstance;
				testInstance.OnHide = (Action)Delegate.Combine(testInstance.OnHide, (Action)delegate
				{
					_testInstance = null;
					Time.timeScale = 1f;
				});
			}
		}

		public void TestTarotCardsMenuNewUnlock2()
		{
			if (!(_tarotCardsMenuTemplate != null) || !(_testInstance == null))
			{
				return;
			}
			List<TarotCards.Card> list = new List<TarotCards.Card>();
			for (int i = 0; i < 10; i++)
			{
				TarotCards.Card item = DataManager.AllTrinkets.RandomElement();
				while (list.Contains(item))
				{
					item = DataManager.AllTrinkets.RandomElement();
				}
				list.Add(item);
			}
			Time.timeScale = 0f;
			UITarotCardsMenuController uITarotCardsMenuController = _tarotCardsMenuTemplate.Instantiate();
			uITarotCardsMenuController.Show(list.ToArray());
			_testInstance = uITarotCardsMenuController;
			UIMenuBase testInstance = _testInstance;
			testInstance.OnHide = (Action)Delegate.Combine(testInstance.OnHide, (Action)delegate
			{
				_testInstance = null;
				Time.timeScale = 1f;
			});
		}

		public void TestRelicsMenu()
		{
			ShowTestMenu(_relicMenuControllerTemplate);
		}

		public void TestRelicMenuNewUnlock()
		{
			if (_relicMenuControllerTemplate != null && _testInstance == null)
			{
				Time.timeScale = 0f;
				RelicType relicType = Enum.GetValues(typeof(RelicType)).Cast<RelicType>().ToArray()
					.RandomElement();
				DataManager.Instance.PlayerFoundRelics.Add(relicType);
				UIRelicMenuController uIRelicMenuController = _relicMenuControllerTemplate.Instantiate();
				uIRelicMenuController.Show(relicType);
				_testInstance = uIRelicMenuController;
				UIMenuBase testInstance = _testInstance;
				testInstance.OnHide = (Action)Delegate.Combine(testInstance.OnHide, (Action)delegate
				{
					_testInstance = null;
					Time.timeScale = 1f;
				});
			}
		}

		private void TestRelicMenuNewUnlocks()
		{
			if (_relicMenuControllerTemplate != null && _testInstance == null)
			{
				List<RelicData> list = new List<RelicData>();
				list.Add(EquipmentManager.RelicData.RandomElement());
				list.Add(EquipmentManager.RelicData.RandomElement());
				list.Add(EquipmentManager.RelicData.RandomElement());
				list.Add(EquipmentManager.RelicData.RandomElement());
				DataManager.Instance.PlayerFoundRelics.Add(list[0].RelicType);
				DataManager.Instance.PlayerFoundRelics.Add(list[1].RelicType);
				DataManager.Instance.PlayerFoundRelics.Add(list[2].RelicType);
				DataManager.Instance.PlayerFoundRelics.Add(list[3].RelicType);
				UIRelicMenuController uIRelicMenuController = _relicMenuControllerTemplate.Instantiate();
				uIRelicMenuController.Show(list);
				_testInstance = uIRelicMenuController;
				UIMenuBase testInstance = _testInstance;
				testInstance.OnHide = (Action)Delegate.Combine(testInstance.OnHide, (Action)delegate
				{
					_testInstance = null;
					Time.timeScale = 1f;
				});
			}
		}

		public void TestUpgradeShopMenu()
		{
			ShowTestMenu(_upgradeShopMenuTemplate);
		}

		public void TestWorldMapMenu()
		{
			DataManager.Instance.DiscoverLocation(FollowerLocation.Base);
			ShowTestMenu(_worldMapMenuController);
		}

		public void TestWorldMapMenuReveal()
		{
			if (_worldMapMenuController != null && _testInstance == null)
			{
				FollowerLocation followerLocation = UIWorldMapMenuController.UnlockableMapLocations.RandomElement();
				DataManager.Instance.DiscoverLocation(FollowerLocation.Base);
				DataManager.Instance.DiscoverLocation(followerLocation);
				Time.timeScale = 0f;
				UIWorldMapMenuController uIWorldMapMenuController = _worldMapMenuController.Instantiate();
				uIWorldMapMenuController.Show(followerLocation);
				_testInstance = uIWorldMapMenuController;
				UIMenuBase testInstance = _testInstance;
				testInstance.OnHide = (Action)Delegate.Combine(testInstance.OnHide, (Action)delegate
				{
					_testInstance = null;
					Time.timeScale = 1f;
				});
			}
		}

		public void TestWorldMapMenuReReveal()
		{
			if (_worldMapMenuController != null && _testInstance == null)
			{
				FollowerLocation followerLocation = UIWorldMapMenuController.UnlockableMapLocations.RandomElement();
				DataManager.Instance.DiscoverLocation(FollowerLocation.Base);
				DataManager.Instance.DiscoverLocation(followerLocation);
				Time.timeScale = 0f;
				UIWorldMapMenuController uIWorldMapMenuController = _worldMapMenuController.Instantiate();
				uIWorldMapMenuController.Show(followerLocation, true);
				_testInstance = uIWorldMapMenuController;
				UIMenuBase testInstance = _testInstance;
				testInstance.OnHide = (Action)Delegate.Combine(testInstance.OnHide, (Action)delegate
				{
					_testInstance = null;
					Time.timeScale = 1f;
				});
			}
		}

		public void TestTutorialMenu()
		{
			ShowTestMenu(_tutorialMenuTemplate);
		}

		public void TestUpgradeTree()
		{
			ShowTestMenu(_upgradeTreeTemplate);
		}

		public void TestPlayerUpgradeTree()
		{
			ShowTestMenu(_playerUpgradeTreeTemplate);
		}

		public void TestRefineryMenu()
		{
			UIRefineryMenuController uIRefineryMenuController = _refineryMenuControllerTemplate.Instantiate();
			uIRefineryMenuController.Show(new StructuresData(), null);
			SetInstance(uIRefineryMenuController);
		}

		public void TestCookingFireMenu()
		{
			StructuresData kitchenData = new StructuresData();
			UICookingFireMenuController uICookingFireMenuController = _cookingFireMenuTemplate.Instantiate();
			uICookingFireMenuController.Show(kitchenData);
			SetInstance(uICookingFireMenuController);
		}

		public void TestKitchenMenu()
		{
			StructuresData kitchenData = new StructuresData();
			UIKitchenMenuController uIKitchenMenuController = _kitchenMenuTemplate.Instantiate();
			uIKitchenMenuController.Show(kitchenData);
			SetInstance(uIKitchenMenuController);
		}

		public void TestFollowerSummaryMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.None);
			followerInfo.Traits.Add(array.RandomElement());
			FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(followerInfo);
			Follower follower = FollowerManager.FollowerPrefab.Instantiate();
			follower.name = "Follower " + orCreateBrain.Info.Name;
			follower.Init(orCreateBrain, orCreateBrain.CreateOutfit());
			follower.transform.position = Vector3.zero;
			UIFollowerSummaryMenuController uIFollowerSummaryMenuController = _followerSummaryMenuTemplate.Instantiate();
			uIFollowerSummaryMenuController.Show(follower);
			uIFollowerSummaryMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerSummaryMenuController.OnHidden, (Action)delegate
			{
				UnityEngine.Object.Destroy(follower.gameObject);
			});
			SetInstance(uIFollowerSummaryMenuController);
		}

		public void TestVideoExportMenu()
		{
			UIVideoExportMenuController uIVideoExportMenuController = _uiVideoExportMenuController.Instantiate();
			uIVideoExportMenuController.Show();
			SetInstance(uIVideoExportMenuController);
		}

		public void TestSandboxMenu()
		{
			DataManager.Instance.OnboardedBossRush = true;
			ShowTestMenu(_uiSandboxMenuControllerTemplate);
		}

		public void TestUnlockBossRushSandbox()
		{
			DataManager.Instance.OnboardedBossRush = false;
			DataManager.Instance.SandboxProgression.Clear();
			DungeonSandboxManager.ProgressionSnapshot progressionForScenario = DungeonSandboxManager.GetProgressionForScenario(DungeonSandboxManager.ScenarioType.DungeonMode, PlayerFleeceManager.FleeceType.Default);
			progressionForScenario.CompletedRuns = 1;
			progressionForScenario.CompletionSeen = 0;
			ShowTestMenu(_uiSandboxMenuControllerTemplate);
		}

		public void TestSandboxMultipleItems()
		{
			DataManager.Instance.OnboardedBossRush = true;
			DataManager.Instance.SandboxProgression.Clear();
			DungeonSandboxManager.ProgressionSnapshot progressionForScenario = DungeonSandboxManager.GetProgressionForScenario(DungeonSandboxManager.ScenarioType.DungeonMode, PlayerFleeceManager.FleeceType.Default);
			progressionForScenario.CompletedRuns = 6;
			progressionForScenario.CompletionSeen = 0;
			ShowTestMenu(_uiSandboxMenuControllerTemplate);
		}

		public void TestPrisonMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			List<FollowerInfo> list = new List<FollowerInfo>();
			for (int i = 0; i < 10; i++)
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				followerInfo.Traits.Add(array.RandomElement());
				list.Add(followerInfo);
			}
			UIPrisonMenuController uIPrisonMenuController = _prisonMenuTemplate.Instantiate();
			uIPrisonMenuController.Show(list);
			uIPrisonMenuController.OnHidden = (Action)Delegate.Combine(uIPrisonMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIPrisonMenuController;
		}

		public void TestPrisonerMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
			followerInfo.Traits.Add(array.RandomElement());
			UIPrisonerMenuController uIPrisonerMenuController = _prisonerMenuTemplate.Instantiate();
			uIPrisonerMenuController.Show(followerInfo, null);
			uIPrisonerMenuController.OnHidden = (Action)Delegate.Combine(uIPrisonerMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIPrisonerMenuController;
		}

		public void TestMissionaryMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			List<FollowerInfo> list = new List<FollowerInfo>();
			for (int i = 0; i < 10; i++)
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				followerInfo.Traits.Add(array.RandomElement());
				list.Add(followerInfo);
			}
			UIMissionaryMenuController uIMissionaryMenuController = _missionaryMenuTemplate.Instantiate();
			uIMissionaryMenuController.Show(list);
			uIMissionaryMenuController.OnHidden = (Action)Delegate.Combine(uIMissionaryMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIMissionaryMenuController;
		}

		public void TestMissionMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
			followerInfo.Traits.Add(array.RandomElement());
			followerInfo.MissionaryTimestamp = 3f;
			UIMissionMenuController uIMissionMenuController = _missionMenuTemplate.Instantiate();
			uIMissionMenuController.Show(new List<int>(followerInfo.ID));
			uIMissionMenuController.OnHidden = (Action)Delegate.Combine(uIMissionMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIMissionMenuController;
		}

		public void TestDemonSummonMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			List<FollowerInfo> list = new List<FollowerInfo>();
			for (int i = 0; i < 10; i++)
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				followerInfo.Traits.Add(array.RandomElement());
				list.Add(followerInfo);
			}
			UIDemonSummonMenuController uIDemonSummonMenuController = _demonSummonMenuTemplate.Instantiate();
			uIDemonSummonMenuController.Show(list);
			uIDemonSummonMenuController.OnHidden = (Action)Delegate.Combine(uIDemonSummonMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIDemonSummonMenuController;
		}

		public void TestDemonMenu()
		{
			FollowerTrait.TraitType[] array = Enum.GetValues(typeof(FollowerTrait.TraitType)).Cast<FollowerTrait.TraitType>().ToArray();
			FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
			followerInfo.Traits.Add(array.RandomElement());
			UIDemonMenuController uIDemonMenuController = _demonMenuTemplate.Instantiate();
			uIDemonMenuController.Show(new List<int> { followerInfo.ID });
			uIDemonMenuController.OnHidden = (Action)Delegate.Combine(uIDemonMenuController.OnHidden, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIDemonMenuController;
		}

		public void TestConfirmationMenu()
		{
			ShowTestMenu(_confirmationWindowTemplate);
		}

		public void TestConfirmationCountdownMenu()
		{
			UIConfirmationCountdownWindow confirmationCountdownWindowInstance = _confirmationCountdownWindowTemplate.Instantiate();
			confirmationCountdownWindowInstance.Show();
			confirmationCountdownWindowInstance.Configure("Are you sure", "Are you sure you'd like to keep these settings? Reverting settings in {0}", 15);
			UIConfirmationCountdownWindow uIConfirmationCountdownWindow = confirmationCountdownWindowInstance;
			uIConfirmationCountdownWindow.OnConfirm = (Action)Delegate.Combine(uIConfirmationCountdownWindow.OnConfirm, (Action)delegate
			{
				Debug.Log("Settings confirmed!".Colour(Color.yellow));
			});
			UIConfirmationCountdownWindow uIConfirmationCountdownWindow2 = confirmationCountdownWindowInstance;
			uIConfirmationCountdownWindow2.OnCancel = (Action)Delegate.Combine(uIConfirmationCountdownWindow2.OnCancel, (Action)delegate
			{
				Debug.Log("Settings reverted!".Colour(Color.yellow));
			});
			UIConfirmationCountdownWindow uIConfirmationCountdownWindow3 = confirmationCountdownWindowInstance;
			uIConfirmationCountdownWindow3.OnHidden = (Action)Delegate.Combine(uIConfirmationCountdownWindow3.OnHidden, (Action)delegate
			{
				confirmationCountdownWindowInstance = null;
			});
		}

		public void TestSaveErrorWindow()
		{
			ShowTestMenu(_saveErrorWindowTemplate);
		}

		public void TestDoctrineMenu()
		{
			ShowTestMenu(_doctrineMenuTemplate);
		}

		public void TestAltarMenu()
		{
			ShowTestMenu(_altarMenuTemplate);
		}

		public void TestDoctrineChoicesMenu()
		{
			MMConversation.CURRENT_CONVERSATION = new ConversationObject(null, null, null);
			MMConversation.CURRENT_CONVERSATION.DoctrineResponses = new List<DoctrineResponse>
			{
				new DoctrineResponse(SermonCategory.Afterlife, 1, true, null),
				new DoctrineResponse(SermonCategory.Afterlife, 1, false, null)
			};
			ShowTestMenu(_doctrineChoicesMenuTemplate);
		}

		public void TestDoctrineChoiceSingle()
		{
			MMConversation.CURRENT_CONVERSATION = new ConversationObject(null, null, null);
			MMConversation.CURRENT_CONVERSATION.DoctrineResponses = new List<DoctrineResponse>
			{
				new DoctrineResponse(SermonCategory.Afterlife, 1, true, null)
			};
			ShowTestMenu(_doctrineChoicesMenuTemplate);
		}

		public void TestRitualsMenu()
		{
			ShowTestMenu(_ritualMenuTemplate);
		}

		public void TestRitualsMenuUnlockSequence()
		{
			UpgradeSystem.Type type = UpgradeSystem.SecondaryRituals.RandomElement();
			UpgradeSystem.UnlockAbility(type);
			UIRitualsMenuController uIRitualsMenuController = _ritualMenuTemplate.Instantiate();
			uIRitualsMenuController.Show(type);
			SetInstance(uIRitualsMenuController);
		}

		public void TestRitualChoiceMenu()
		{
			ShowTestMenu(_heartsOfTheFaithfulChoiceMenuController);
		}

		public void TestPlayerUpgrades()
		{
			ShowTestMenu(_playerUpgradesMenuControllerTemplate);
		}

		public void TestPlayerUpgradesCrystal()
		{
			UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController = _playerUpgradesMenuControllerTemplate.Instantiate();
			uIPlayerUpgradesMenuController.ShowCrystalUnlock();
			SetInstance(uIPlayerUpgradesMenuController);
		}

		public void TestPlayerUpgradesFleeces()
		{
			DataManager.Instance.DeathCatBeaten = true;
			UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController = _playerUpgradesMenuControllerTemplate.Instantiate();
			uIPlayerUpgradesMenuController.ShowNewFleecesUnlocked(new PlayerFleeceManager.FleeceType[4]
			{
				PlayerFleeceManager.FleeceType.CurseInsteadOfWeapon,
				PlayerFleeceManager.FleeceType.OneHitKills,
				PlayerFleeceManager.FleeceType.HollowHeal,
				PlayerFleeceManager.FleeceType.NoRolling
			});
			SetInstance(uIPlayerUpgradesMenuController);
		}

		public void TestPurgatoryCompletedFleece()
		{
			DataManager.Instance.UnlockedFleeces.Add(1000);
			DataManager.Instance.DeathCatBeaten = true;
			UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController = _playerUpgradesMenuControllerTemplate.Instantiate();
			uIPlayerUpgradesMenuController.ShowNewFleecesUnlocked(new PlayerFleeceManager.FleeceType[1] { PlayerFleeceManager.FleeceType.GodOfDeath });
			SetInstance(uIPlayerUpgradesMenuController);
		}

		public void TestSermonWheelOverlay()
		{
			UISermonWheelController uISermonWheelController = _sermonWheelTemplate.Instantiate();
			uISermonWheelController.Show(InventoryItem.ITEM_TYPE.DOCTRINE_STONE);
			SetInstance(uISermonWheelController);
		}

		public void TestSermonWheelOverlayCrystal()
		{
			UISermonWheelController uISermonWheelController = _sermonWheelTemplate.Instantiate();
			uISermonWheelController.Show(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE);
			SetInstance(uISermonWheelController);
		}

		public void TestFollowerInteractionWheelOverlay()
		{
			if (_followerInteractionWheelTemplate != null && _testInstance == null)
			{
				CommandItem item = new CommandItem
				{
					Command = FollowerCommands.CutTrees
				};
				CommandItem item2 = new CommandItem
				{
					Command = FollowerCommands.ClearRubble
				};
				CommandItem item3 = new CommandItem
				{
					Command = FollowerCommands.ClearWeeds
				};
				CommandItem item4 = new CommandItem
				{
					Command = FollowerCommands.Refiner_2
				};
				List<CommandItem> subCommands = new List<CommandItem> { item, item, item2, item2, item3, item3, item4, item4 };
				CommandItem item5 = new CommandItem
				{
					Command = FollowerCommands.GiveWorkerCommand_2,
					SubCommands = subCommands
				};
				CommandItem item6 = new CommandItem
				{
					Command = FollowerCommands.Talk
				};
				List<CommandItem> commandItems = new List<CommandItem> { item5, item6 };
				UIFollowerInteractionWheelOverlayController uIFollowerInteractionWheelOverlayController = _followerInteractionWheelTemplate.Instantiate();
				uIFollowerInteractionWheelOverlayController.Show(null, commandItems);
				uIFollowerInteractionWheelOverlayController.OnHide = (Action)Delegate.Combine(uIFollowerInteractionWheelOverlayController.OnHide, (Action)delegate
				{
					_testInstance = null;
				});
				_testInstance = uIFollowerInteractionWheelOverlayController;
			}
		}

		public void TestCurseWheelOverlay()
		{
			ShowTestMenu(_curseWheelOverlayTemplate);
		}

		public void TestDeathScreenOverlay()
		{
			DataManager.Instance.dungeonVisitedRooms = new List<NodeType>
			{
				NodeType.FirstFloor,
				NodeType.DungeonFloor,
				NodeType.DungeonFloor,
				NodeType.DungeonFloor,
				NodeType.Boss
			};
			DataManager.Instance.itemsDungeon = new List<InventoryItem>
			{
				new InventoryItem(InventoryItem.ITEM_TYPE.BONE, 10),
				new InventoryItem(InventoryItem.ITEM_TYPE.GRASS, 10),
				new InventoryItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10),
				new InventoryItem(InventoryItem.ITEM_TYPE.BLOOD_STONE, 10)
			};
			UIDeathScreenOverlayController uIDeathScreenOverlayController = _deathScreenOverlayTemplate.Instantiate();
			uIDeathScreenOverlayController.Show(UIDeathScreenOverlayController.Results.Killed, 5);
			uIDeathScreenOverlayController.OnHide = (Action)Delegate.Combine(uIDeathScreenOverlayController.OnHide, (Action)delegate
			{
				_testInstance = null;
			});
			_testInstance = uIDeathScreenOverlayController;
		}

		public void TestKeyScreenOverlay()
		{
			Inventory.KeyPieces++;
			ShowTestMenu(_keyScreenTemplate);
		}

		public void TestWeaponWheelOverlay()
		{
			ShowTestMenu(_weaponWheelOverlayTemplate);
		}

		public void TestUpgradeUnlockOverlay()
		{
			ShowTestMenu(_ugpradeUnlockOverlaytemplate);
		}

		public void TestTutorialOverlay()
		{
			TutorialTopic topic = Enum.GetValues(typeof(TutorialTopic)).Cast<TutorialTopic>().ToArray()
				.RandomElement();
			UITutorialOverlayController uITutorialOverlayController = _tutorialOverlayTemplate.Instantiate();
			uITutorialOverlayController.Show(topic);
			SetInstance(uITutorialOverlayController);
		}

		public void TestRecipeConfirmationOverlay()
		{
			UIRecipeConfirmationOverlayController uIRecipeConfirmationOverlayController = _recipeConfirmationOverlayTemplate.Instantiate();
			uIRecipeConfirmationOverlayController.Show(CookingData.GetAllMeals().RandomElement());
			SetInstance(uIRecipeConfirmationOverlayController);
		}

		public void TestRecipeConfirmationOverlay2()
		{
			UIRecipeConfirmationOverlayController uIRecipeConfirmationOverlayController = _recipeConfirmationOverlayTemplate.Instantiate();
			uIRecipeConfirmationOverlayController.Show(CookingData.GetAllMeals().RandomElement(), true);
			SetInstance(uIRecipeConfirmationOverlayController);
		}

		public void TestItemSelectorOverlay()
		{
			List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
			list.Add(CookingData.GetAllFoods().RandomElement());
			list.Add(CookingData.GetAllFoods().RandomElement());
			list.Add(CookingData.GetAllFoods().RandomElement());
			list.Add(CookingData.GetAllFoods().RandomElement());
			list.Add(CookingData.GetAllFoods().RandomElement());
			UIItemSelectorOverlayController uIItemSelectorOverlayController = _itemSelectorOverlayTemplate.Instantiate();
			uIItemSelectorOverlayController.Show(list, new ItemSelector.Params
			{
				Key = "sell",
				Context = ItemSelector.Context.Add,
				Offset = new Vector2(0f, 100f)
			});
			SetInstance(uIItemSelectorOverlayController);
		}

		public void TestDifficultyOverlay()
		{
			ShowTestMenu(_difficultyOverlayTemplate);
		}

		public void TestTarotChoice()
		{
			_tarotChoiceOverlayTemplate.Instantiate().Show(new TarotCards.TarotCard(TarotCards.Card.Hearts1, 0), new TarotCards.TarotCard(TarotCards.Card.DiseasedHeart, 0));
			SetInstance(_tarotChoiceOverlayTemplate);
		}

		public void TestFleeceTarots()
		{
			fleeceTarotRewardOverlayController.Instantiate().Show(new TarotCards.TarotCard(TarotCards.Card.Hearts1, 0), new TarotCards.TarotCard(TarotCards.Card.DiseasedHeart, 0), new TarotCards.TarotCard(TarotCards.Card.AttackRate, 0), new TarotCards.TarotCard(TarotCards.Card.MovementSpeed, 0));
			SetInstance(_tarotChoiceOverlayTemplate);
		}

		public void TestControlBindingOverlay()
		{
			ShowTestMenu(_controlBindingOverlayController);
		}

		public void TestAdventureMapOverlay()
		{
			UIAdventureMapOverlayController uIAdventureMapOverlayController = _adventureMapTemplate.Instantiate();
			BiomeGenerator.Instance.Seed = UnityEngine.Random.Range(-10000, 10000);
			MapManager.Instance.MapGenerated = false;
			global::Map.Map map = MapGenerator.GetMap(MapManager.Instance.DungeonConfig);
			uIAdventureMapOverlayController.Show(map);
			SetInstance(uIAdventureMapOverlayController);
		}

		public void TestRoadmapOverlay()
		{
			ShowTestMenu(_roadmapOverlayController);
		}

		public void TestBugReportingOverlay()
		{
			ShowTestMenu(_bugReportingOverlayController);
		}

		public void TestMysticShopOverlay()
		{
			WeightedCollection<InventoryItem.ITEM_TYPE> weightedCollection = new WeightedCollection<InventoryItem.ITEM_TYPE>();
			for (int i = 0; i < UnityEngine.Random.Range(3, 5); i++)
			{
				weightedCollection.Add(Enum.GetValues(typeof(InventoryItem.ITEM_TYPE)).Cast<InventoryItem.ITEM_TYPE>().ToList()
					.RandomElement(), 5f);
			}
			UIMysticShopOverlayController uIMysticShopOverlayController = _mysticShopOverlayController.Instantiate();
			uIMysticShopOverlayController.Show(weightedCollection);
			SetInstance(uIMysticShopOverlayController);
			ShowTestMenu(_mysticShopOverlayController);
		}

		public void TestPhotoModeGallery()
		{
			ShowTestMenu(_photoGalleryMenuController);
		}

		public void TestEditPhotoOverlay()
		{
			ShowTestMenu(_editPhotoOverlayController);
		}

		public void TestKnucklebones()
		{
			if (_knuckleBonesTemplate != null && _testInstance == null)
			{
				UIKnuckleBonesController uIKnuckleBonesController = _knuckleBonesTemplate.Instantiate();
				uIKnuckleBonesController.Show(new KnucklebonesOpponent
				{
					Config = _opponentConfigurations[0]
				}, 100);
				uIKnuckleBonesController.OnHide = (Action)Delegate.Combine(uIKnuckleBonesController.OnHide, (Action)delegate
				{
					_testInstance = null;
				});
				_testInstance = uIKnuckleBonesController;
			}
		}

		public void TestKnuckleBonesBettingWindow()
		{
			if (_knuckleBonesTemplate != null && _testInstance == null)
			{
				UIKnucklebonesBettingSelectionController uIKnucklebonesBettingSelectionController = _kbBettingTemplate.Instantiate();
				uIKnucklebonesBettingSelectionController.Show(_opponentConfigurations[0]);
				uIKnucklebonesBettingSelectionController.OnHide = (Action)Delegate.Combine(uIKnucklebonesBettingSelectionController.OnHide, (Action)delegate
				{
					_testInstance = null;
				});
				_testInstance = uIKnucklebonesBettingSelectionController;
			}
		}

		public void TestKnuckleBonesOpponentWindow()
		{
			if (_kbOpponentTemplate != null && _testInstance == null)
			{
				UIKnucklebonesOpponentSelectionController uIKnucklebonesOpponentSelectionController = _kbOpponentTemplate.Instantiate();
				uIKnucklebonesOpponentSelectionController.Show(_opponentConfigurations);
				uIKnucklebonesOpponentSelectionController.OnHide = (Action)Delegate.Combine(uIKnucklebonesOpponentSelectionController.OnHide, (Action)delegate
				{
					_testInstance = null;
				});
				_testInstance = uIKnucklebonesOpponentSelectionController;
			}
		}

		public void TestTotemWheel()
		{
			UITwitchTotemWheel uITwitchTotemWheel = _twitchTotemWheelTemplate.Instantiate();
			List<UIRandomWheel.Segment> list = new List<UIRandomWheel.Segment>();
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.LOG
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.MEAT
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.FOLLOWERS
			});
			list.Add(new UIRandomWheel.Segment
			{
				probability = 0.2f,
				reward = InventoryItem.ITEM_TYPE.STONE
			});
			list.Shuffle();
			list.Insert(0, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
			list.Insert(2, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
			list.Insert(4, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
			list.Insert(6, new UIRandomWheel.Segment
			{
				probability = 0.05f,
				reward = InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION
			});
			uITwitchTotemWheel.Show(list.ToArray());
			SetInstance(uITwitchTotemWheel);
		}

		public void TestTwitchSettingsMenu()
		{
			UITwitchSettingsMenuController uITwitchSettingsMenuController = _uiTwitchMenuSettingsController.Instantiate();
			uITwitchSettingsMenuController.Show();
			SetInstance(uITwitchSettingsMenuController);
		}

		private void Start()
		{
			Singleton<SettingsManager>.Instance.LoadAndApply();
			SaveAndLoad.Load(5);
		}

		private void ShowTestMenu(UIMenuBase menu)
		{
			if (!(_testInstance != null))
			{
				UIMenuBase uIMenuBase = menu.Instantiate();
				uIMenuBase.Show();
				SetInstance(uIMenuBase);
			}
		}

		private void SetInstance(UIMenuBase menu)
		{
			if (menu != null && _testInstance == null)
			{
				Time.timeScale = 0f;
				_testInstance = menu;
				UIMenuBase testInstance = _testInstance;
				testInstance.OnHide = (Action)Delegate.Combine(testInstance.OnHide, (Action)delegate
				{
					_testInstance = null;
					Time.timeScale = 1f;
				});
			}
		}

		public void Hide()
		{
			if (_testInstance != null)
			{
				_testInstance.Hide();
			}
		}

		public void ClearSandbox()
		{
			DataManager.Instance.SandboxProgression.Clear();
		}

		public void AllFleeces()
		{
			DataManager.Instance.UnlockedFleeces.Add(0);
			DataManager.Instance.UnlockedFleeces.Add(1);
			DataManager.Instance.UnlockedFleeces.Add(2);
			DataManager.Instance.UnlockedFleeces.Add(3);
			DataManager.Instance.UnlockedFleeces.Add(4);
			DataManager.Instance.UnlockedFleeces.Add(5);
		}

		public void ObjectivesTest()
		{
			ObjectiveManager.Add(new Objectives_CollectItem("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.FLOWER_RED, 10, false, FollowerLocation.Dungeon1_1, 4800f));
			ObjectiveManager.Add(new Objectives_RecruitCursedFollower("Objectives/GroupTitles/Quest", Thought.Ill, UnityEngine.Random.Range(2, 4)));
			ObjectiveManager.Add(new Objectives_CookMeal("Objectives/GroupTitles/Quest", InventoryItem.ITEM_TYPE.MEAL_GREAT, 3, 9600f));
			ObjectiveManager.Add(new Objectives_PerformRitual("Objectives/GroupTitles/Quest", UpgradeSystem.Type.Ritual_WorkThroughNight, -1, 0, 4800f));
		}

		public void UnlockRecipesMinimal()
		{
			DataManager.Instance.RecipesDiscovered.Add(CookingData.GetAllMeals().RandomElement());
			DataManager.Instance.RecipesDiscovered.Add(CookingData.GetAllMeals().RandomElement());
			DataManager.Instance.RecipesDiscovered.Add(CookingData.GetAllMeals().RandomElement());
			DataManager.Instance.RecipesDiscovered.Add(CookingData.GetAllMeals().RandomElement());
			DataManager.Instance.RecipesDiscovered.Add(CookingData.GetAllMeals().RandomElement());
			Inventory.AddItem(CookingData.GetAllFoods().RandomElement(), 3);
			Inventory.AddItem(CookingData.GetAllFoods().RandomElement(), 3);
			Inventory.AddItem(CookingData.GetAllFoods().RandomElement(), 3);
			Inventory.AddItem(CookingData.GetAllFoods().RandomElement(), 3);
			Inventory.AddItem(CookingData.GetAllFoods().RandomElement(), 3);
		}

		public void UnlockRecipes()
		{
			InventoryItem.ITEM_TYPE[] allMeals = CookingData.GetAllMeals();
			foreach (InventoryItem.ITEM_TYPE item in allMeals)
			{
				if (!DataManager.Instance.RecipesDiscovered.Contains(item))
				{
					DataManager.Instance.RecipesDiscovered.Add(item);
				}
			}
			allMeals = CookingData.GetAllFoods();
			for (int i = 0; i < allMeals.Length; i++)
			{
				Inventory.AddItem(allMeals[i], 3);
			}
		}

		public void RandomAbilityPoints()
		{
			DataManager.Instance.AbilityPoints += UnityEngine.Random.Range(5, 10);
			DataManager.Instance.DiscipleAbilityPoints += UnityEngine.Random.Range(5, 10);
		}

		public void RandomTutorials()
		{
			List<TutorialTopic> list = Enum.GetValues(typeof(TutorialTopic)).Cast<TutorialTopic>().ToArray()
				.ToList();
			int num = UnityEngine.Random.Range(0, list.Count / 2);
			for (int i = 0; i < num; i++)
			{
				TutorialTopic tutorialTopic = list.RandomElement();
				list.Remove(tutorialTopic);
				DataManager.Instance.TryRevealTutorialTopic(tutorialTopic);
			}
		}

		public void AllTutorials()
		{
			TutorialTopic[] array = Enum.GetValues(typeof(TutorialTopic)).Cast<TutorialTopic>().ToArray();
			foreach (TutorialTopic topic in array)
			{
				DataManager.Instance.TryRevealTutorialTopic(topic);
			}
		}

		public void RandomLocations()
		{
			for (int i = 0; (float)i < (float)UIWorldMapMenuController.UnlockableMapLocations.Length * 0.5f; i++)
			{
				FollowerLocation followerLocation = UIWorldMapMenuController.UnlockableMapLocations.RandomElement();
				while (!DataManager.Instance.DiscoverLocation(followerLocation))
				{
					followerLocation = UIWorldMapMenuController.UnlockableMapLocations.RandomElement();
				}
				DataManager.Instance.DiscoveredLocations.Add(followerLocation);
			}
		}

		public void AllLocations()
		{
			FollowerLocation[] unlockableMapLocations = UIWorldMapMenuController.UnlockableMapLocations;
			foreach (FollowerLocation location in unlockableMapLocations)
			{
				DataManager.Instance.DiscoverLocation(location);
			}
		}

		public void DoctrineDataTest()
		{
			SaveAndLoad.ResetSave(0, true);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 4, true));
			SaveAndLoad.Save();
		}

		public void DoctrineDataTest2()
		{
			SaveAndLoad.ResetSave(0, true);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 2, false));
			SaveAndLoad.Save();
		}

		public void AllDoctrinesFirstChoice()
		{
			SaveAndLoad.ResetSave(0, true);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 4, true));
			SaveAndLoad.Save();
		}

		public void AllDoctrinesSecondChoice()
		{
			SaveAndLoad.ResetSave(0, true);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 4, false));
			SaveAndLoad.Save();
		}

		public void AllDoctrines()
		{
			SaveAndLoad.ResetSave(0, true);
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 1, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 2, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 3, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 4, true));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Food, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.WorkAndWorship, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Possession, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Afterlife, 4, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 1, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 2, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 3, false));
			DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.LawAndOrder, 4, false));
			SaveAndLoad.Save();
		}

		public void RitualDataTest()
		{
			SaveAndLoad.ResetSave(0, true);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_HeartsOfTheFaithful1);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_HeartsOfTheFaithful2);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_HeartsOfTheFaithful3);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_UnlockWeapon);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_UnlockCurse);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Sacrifice);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Reindoctrinate);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_ConsumeFollower);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FasterBuilding);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Enlightenment);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_WorkThroughNight);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Holiday);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AlmsToPoor);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_DonationRitual);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Fast);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Feast);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_HarvestRitual);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FishingRitual);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Ressurect);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Funeral);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Fightpit);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Wedding);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AssignFaithEnforcer);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AssignTaxCollector);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Ascend);
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FirePit);
			SaveAndLoad.Save();
		}

		public void UnlockAllStructures()
		{
			CheatConsole.AllBuildingsUnlocked = true;
		}

		public void UnlockSkins()
		{
			for (int i = 0; i < 10; i++)
			{
				DataManager.SetFollowerSkinUnlocked(WorshipperData.Instance.Characters.RandomElement().Skin[0].Skin);
			}
		}

		public void UnlockAllSkins()
		{
			foreach (WorshipperData.SkinAndData character in WorshipperData.Instance.Characters)
			{
				DataManager.SetFollowerSkinUnlocked(character.Skin[0].Skin);
			}
		}

		public void RandomTarotCards()
		{
			List<TarotCards.Card> list = new List<TarotCards.Card>();
			int num = UnityEngine.Random.Range(5, 10);
			while (list.Count < num)
			{
				TarotCards.Card card = DataManager.AllTrinkets.RandomElement();
				if (!list.Contains(card) && !DataManager.TarotCardBlacklist.Contains(card))
				{
					list.Add(card);
					TarotCards.UnlockTrinket(card);
				}
			}
		}

		public void AllTarotCards()
		{
			foreach (TarotCards.Card item in Enum.GetValues(typeof(TarotCards.Card)).Cast<TarotCards.Card>())
			{
				TarotCards.UnlockTrinket(item);
			}
		}

		public void FillInventory()
		{
			foreach (InventoryItem.ITEM_TYPE value in Enum.GetValues(typeof(InventoryItem.ITEM_TYPE)))
			{
				Inventory.AddItem((int)value, 5);
			}
		}

		public void RandomiseCursesWeapons()
		{
			RandomCurses();
			new List<TarotCards.Card>();
			List<TarotCards.Card> list = new List<TarotCards.Card>();
			int num = UnityEngine.Random.Range((int)((float)DataManager.AllTrinkets.Count / 2f), DataManager.AllTrinkets.Count);
			while (list.Count < num)
			{
				TarotCards.Card card = DataManager.AllTrinkets.RandomElement();
				if (!list.Contains(card) && !DataManager.TarotCardBlacklist.Contains(card))
				{
					list.Add(card);
					TrinketManager.AddTrinket(new TarotCards.TarotCard(card, 0));
				}
			}
		}

		public void RandomCurses()
		{
			new List<TarotCards.Card>();
		}

		public void AllCurses()
		{
		}

		public void BonesAndCoins()
		{
			Inventory.AddItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1000);
			Inventory.AddItem(InventoryItem.ITEM_TYPE.BONE, 1000);
		}

		public void AbilityPoints()
		{
			DataManager.Instance.AbilityPoints += 100;
		}

		public void AllRelics()
		{
			foreach (RelicType value in Enum.GetValues(typeof(RelicType)))
			{
				DataManager.Instance.PlayerFoundRelics.Add(value);
			}
		}

		public void SaveData()
		{
			SaveAndLoad.Save();
		}

		public void ResetData()
		{
			SaveAndLoad.ResetSave(5, true);
			DataManager.Instance.UnlockedFleeces.Add(0);
			DataManager.Instance.UnlockedFleeces.Add(1);
			DataManager.Instance.UnlockedFleeces.Add(2);
			DataManager.Instance.UnlockedFleeces.Add(3);
			Singleton<SettingsManager>.Instance.ApplySettings();
		}
	}
}
