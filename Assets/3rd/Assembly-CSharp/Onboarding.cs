using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using I2.Loc;
using Lamb.UI;
using MMTools;
using Spine.Unity;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class Onboarding : BaseMonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<ObjectivesData, bool> _003C_003E9__23_0;

		public static Action _003C_003E9__29_0;

		public static Action _003C_003E9__33_4;

		internal bool _003CStart_003Eb__23_0(ObjectivesData obj)
		{
			return obj is Objectives_CollectItem;
		}

		internal void _003COpenDoor_003Eb__29_0()
		{
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/GoToDungeon", Objectives.CustomQuestTypes.GoToDungeon));
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/GoToDungeon", Objectives.CustomQuestTypes.GetMoreGoldFromDungeon));
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/GoToDungeon", Objectives.CustomQuestTypes.GetNewFollowersFromDungeon));
		}

		internal void _003COnObjectiveComplete_003Eb__33_4()
		{
			PlayerFarming.Instance.EndGoToAndStop();
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		}

		internal void _003COnObjectiveComplete_003Eg__CompletedSermon_007C33_2(bool SetPlayerToIdle, bool ShowHUD)
		{
			DataManager.Instance.GivenSermonQuest = true;
			MMConversation.OnConversationEnd -= _003C_003E9._003COnObjectiveComplete_003Eg__CompletedSermon_007C33_2;
		}
	}

	public static Onboarding Instance;

	public GameObject Rat1Indoctrinate;

	public GameObject Rat2Shrine;

	public GameObject Rat2Food;

	public GameObject Rat3Devotion;

	public GameObject Rat4GoToDungeon;

	public GameObject Rat5PauseScreen;

	public GameObject Rat6Meditation;

	public GameObject Rat7MessageBoard;

	public GameObject RatReturnWithFolloer;

	public GameObject RatBonesAndRitual;

	public GameObject RatPreachSermon;

	public GameObject RatMonsterHeart;

	public GameObject RatLoyalty;

	public GameObject RatRitaul;

	public SkeletonAnimation RatauSpine;

	private float timeBetweenFollowerQuestCheck = 10f;

	private float timestamp;

	public GameObject UITutorialPrefab;

	public static DataManager.OnboardingPhase CurrentPhase
	{
		get
		{
			return DataManager.Instance.CurrentOnboardingPhase;
		}
		set
		{
			DataManager.Instance.CurrentOnboardingPhase = value;
		}
	}

	private void OnEnable()
	{
		Instance = this;
		ObjectiveManager.OnObjectiveGroupCompleted = (Action<string>)Delegate.Combine(ObjectiveManager.OnObjectiveGroupCompleted, new Action<string>(OnObjectiveComplete));
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		FollowerRecruit.OnNewRecruit = (Action)Delegate.Combine(FollowerRecruit.OnNewRecruit, new Action(OnNewRecruit));
		DoctrineController.OnUnlockedFirstRitual = (Action)Delegate.Combine(DoctrineController.OnUnlockedFirstRitual, new Action(OnUnlockedFirstRitaul));
	}

	private void OnUnlockedFirstRitaul()
	{
		HideAll();
		DataManager.Instance.BonesEnabled = true;
		BaseGoopDoor.Instance.UnblockGoopDoor();
		RatRitaul.SetActive(true);
	}

	private void Start()
	{
		Debug.Log("Onboarding Start");
		HideAll();
		if (!DataManager.Instance.InTutorial && !DataManager.Instance.Tutorial_First_Indoctoring)
		{
			CurrentPhase = DataManager.OnboardingPhase.Indoctrinate;
			TimeManager.PauseGameTime = true;
			Rat1Indoctrinate.SetActive(true);
			DataManager.Instance.AllowSaving = true;
			BaseGoopDoor.Instance.BlockGoopDoor();
		}
		else if (CurrentPhase == DataManager.OnboardingPhase.Indoctrinate)
		{
			DataManager.Instance.InTutorial = true;
		}
		if (DataManager.Instance.OnboardingFinished && !DataManager.Instance.DiscoveredLocations.Contains(FollowerLocation.Hub1_RatauOutside))
		{
			DataManager.Instance.DiscoverLocation(FollowerLocation.Hub1_RatauOutside);
		}
		if (ObjectiveManager.GroupExists("Objectives/GroupTitles/DeclareDoctrine") || ObjectiveManager.GroupExists("Objectives/GroupTitles/Temple"))
		{
			string loc = (ObjectiveManager.GroupExists("Objectives/GroupTitles/DeclareDoctrine") ? "Objectives/Custom/DeclareDoctrine" : "Objectives/GroupTitles/Temple");
			BaseGoopDoor.Instance.BlockGoopDoor(loc);
		}
		if (ObjectiveManager.GroupExists("Objectives/GroupTitles/GoToDungeon") && DataManager.Instance.Followers_Recruit.Count > 0 && DataManager.Instance.KilledBosses.Count > 0)
		{
			int num = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) - StructuresData.GetCost(StructureBrain.TYPES.SHRINE)[0].CostValue;
			if (num < 0)
			{
				num *= -1;
				Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, num);
			}
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GetMoreGoldFromDungeon);
			Debug.Log("YOU ARE BACK!!");
			CurrentPhase = DataManager.OnboardingPhase.IndoctrinateBerriesAllowed;
			TimeManager.PauseGameTime = true;
			DataManager.Instance.UnlockBaseTeleporter = false;
			BaseGoopDoor.Instance.BlockGoopDoor(ScriptLocalization.Interactions.IndoctrinateBeforeLeaving);
			DataManager.Instance.AllowBuilding = false;
			DataManager.Instance.BuildShrineEnabled = true;
			RatReturnWithFolloer.SetActive(true);
		}
		int itemQuantity = Inventory.GetItemQuantity(9);
		if (ObjectiveManager.GroupExists("Objectives/GroupTitles/BonesAndRitual"))
		{
			Debug.Log("ONBOARDING SET UP FOR BONES AND RITUAL");
			List<StructuresData.ItemCost> cost = UpgradeSystem.GetCost(UpgradeSystem.Type.Ritual_FirePit);
			int num2 = 0;
			foreach (StructuresData.ItemCost item in cost)
			{
				if (item.CostItem == InventoryItem.ITEM_TYPE.BONE)
				{
					num2 = item.CostValue;
				}
			}
			Objectives_CollectItem objectives_CollectItem = ObjectiveManager.GetAllObjectivesOfGroup("Objectives/GroupTitles/BonesAndRitual").First((ObjectivesData obj) => obj is Objectives_CollectItem) as Objectives_CollectItem;
			if (objectives_CollectItem != null && objectives_CollectItem.Count < 25)
			{
				Inventory.AddItem(InventoryItem.ITEM_TYPE.BONE, num2 - itemQuantity);
				objectives_CollectItem.Count = objectives_CollectItem.Target;
			}
			itemQuantity = Inventory.GetItemQuantity(9);
			Debug.Log("CurrentBones: " + itemQuantity + "   BoneCost: " + num2);
			if (itemQuantity >= num2)
			{
				BaseGoopDoor.Instance.BlockGoopDoor("Objectives/Custom/PerformAnyRitual");
			}
		}
		if (FollowerBrain.AllBrains.Count > 1 && CurrentPhase != DataManager.OnboardingPhase.Shrine)
		{
			CurrentPhase = DataManager.OnboardingPhase.Shrine;
		}
		StartCoroutine(WaitForPlayerFarmingToExist());
	}

	private IEnumerator WaitForPlayerFarmingToExist()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		while (MMConversation.isPlaying || LetterBox.IsPlaying)
		{
			yield return null;
		}
		if (DataManager.Instance.CompletedObjectivesHistory.Count > 0 && !ObjectiveManager.GroupExists(DataManager.Instance.CompletedObjectivesHistory[0].GroupId) && !DataManager.Instance.OnboardingFinished)
		{
			OnObjectiveComplete(DataManager.Instance.CompletedObjectivesHistory[0].GroupId);
		}
	}

	private void Update()
	{
		if (TimeManager.TotalElapsedGameTime > timestamp)
		{
			timestamp = TimeManager.TotalElapsedGameTime + timeBetweenFollowerQuestCheck;
			TryGiveFollowerOnboardingQuest();
		}
	}

	private void OnDisable()
	{
		ObjectiveManager.OnObjectiveGroupCompleted = (Action<string>)Delegate.Remove(ObjectiveManager.OnObjectiveGroupCompleted, new Action<string>(OnObjectiveComplete));
		FollowerRecruit.OnNewRecruit = (Action)Delegate.Remove(FollowerRecruit.OnNewRecruit, new Action(OnNewRecruit));
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		DoctrineController.OnUnlockedFirstRitual = (Action)Delegate.Remove(DoctrineController.OnUnlockedFirstRitual, new Action(OnUnlockedFirstRitaul));
	}

	public void EndTutorial()
	{
		HUD_Manager.Instance.BaseDetailsTransition.MoveBackInFunction();
	}

	public void EndTutorial2()
	{
		TimeManager.PauseGameTime = false;
		if (HUD_Manager.Instance != null)
		{
			HUD_Manager.Instance.TimeTransitions.MoveBackInFunction();
		}
		DataManager.Instance.UnlockBaseTeleporter = true;
		BaseGoopDoor.Instance.UnblockGoopDoor();
	}

	public void OpenDoor()
	{
		DataManager.Instance.InTutorial = true;
		BaseGoopDoor.Instance.PlayOpenDoorSequence(delegate
		{
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/GoToDungeon", Objectives.CustomQuestTypes.GoToDungeon));
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/GoToDungeon", Objectives.CustomQuestTypes.GetMoreGoldFromDungeon));
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/GoToDungeon", Objectives.CustomQuestTypes.GetNewFollowersFromDungeon));
		});
		EndTutorial();
	}

	private void HideAll()
	{
		int num = -1;
		while (++num < base.transform.childCount)
		{
			base.transform.GetChild(num).gameObject.SetActive(false);
		}
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (structure.Type == StructureBrain.TYPES.BUILD_SITE && structure.ToBuildType == StructureBrain.TYPES.SHRINE)
		{
			foreach (Follower follower in Follower.Followers)
			{
				follower.Brain.CompleteCurrentTask();
			}
		}
		StructureBrain.TYPES type = structure.Type;
		int num = 27;
		if (structure.Type == StructureBrain.TYPES.TEMPLE)
		{
			bool onboardingFinished = DataManager.Instance.OnboardingFinished;
		}
	}

	private IEnumerator DelayedSetActive(GameObject obj, float delay = 0.5f)
	{
		yield return new WaitForSeconds(delay);
		HideAll();
		obj.SetActive(true);
		obj.gameObject.GetComponent<Interaction_SimpleConversation>().Play();
	}

	private void OnObjectiveComplete(string groupID)
	{
		if (UIMenuBase.ActiveMenus.Count > 0)
		{
			UIMenuBase uIMenuBase = UIMenuBase.ActiveMenus[0];
			uIMenuBase.OnHidden = (Action)Delegate.Combine(uIMenuBase.OnHidden, (Action)delegate
			{
				OnObjectiveComplete(groupID);
			});
			return;
		}
		switch (groupID)
		{
		case "Objectives/Custom/GetFollowerUpgradePoint":
			break;
		case "Objectives/GroupTitles/RecruitFollower":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/Food"))
			{
				StartCoroutine(WaitForConversationToFinish(delegate
				{
					HideAll();
					Rat2Food.SetActive(true);
					Rat2Food.gameObject.GetComponent<Interaction_SimpleConversation>().Play();
					DataManager.Instance.AllowBuilding = true;
				}));
			}
			break;
		case "Objectives/GroupTitles/Food":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/BuildCookingFire"))
			{
				ObjectiveManager.Add(new Objectives_BuildStructure("Objectives/GroupTitles/BuildCookingFire", StructureBrain.TYPES.COOKING_FIRE));
			}
			break;
		case "Objectives/GroupTitles/BuildCookingFire":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/CookFirstMeal"))
			{
				if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Food))
				{
					UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Food);
					uITutorialOverlayController.OnHide = (Action)Delegate.Combine(uITutorialOverlayController.OnHide, new Action(HungerBar.Instance.Reveal));
				}
				ObjectiveManager.Add(new Objectives_CollectItem("Objectives/GroupTitles/CookFirstMeal", InventoryItem.ITEM_TYPE.BERRY, CookingData.GetRecipe(InventoryItem.ITEM_TYPE.MEAL_BERRIES)[0][0].quantity));
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/CookFirstMeal", Objectives.CustomQuestTypes.CookFirstMeal));
			}
			break;
		case "Objectives/GroupTitles/CookFirstMeal":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/GoToDungeon"))
			{
				DataManager.Instance.RatExplainDungeon = false;
				HideAll();
				Rat4GoToDungeon.SetActive(true);
				Rat4GoToDungeon.gameObject.GetComponent<Interaction_SimpleConversation>().Play();
			}
			break;
		case "Objectives/GroupTitles/UnlockSacredKnowledge":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/CollectDivineInspiration"))
			{
				CurrentPhase = DataManager.OnboardingPhase.Done;
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/CollectDivineInspiration", Objectives.CustomQuestTypes.CollectDivineInspiration));
			}
			break;
		case "Objectives/GroupTitles/GoToDungeon":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/RepairTheShrine"))
			{
				Debug.Log("COMPLETED GO TO DUNGEON QUEST!");
				HideAll();
				Rat2Shrine.SetActive(true);
				Rat2Shrine.gameObject.GetComponent<Interaction_SimpleConversation>().Play();
				DataManager.Instance.AllowBuilding = true;
			}
			break;
		case "Objectives/GroupTitles/RepairTheShrine":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/UnlockSacredKnowledge"))
			{
				CurrentPhase = DataManager.OnboardingPhase.Shrine;
				DataManager.Instance.AllowBuilding = false;
				StartCoroutine(DelayedSetActive(Rat3Devotion));
			}
			break;
		case "Objectives/GroupTitles/CollectDivineInspiration":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/Temple"))
			{
				ObjectiveManager.Add(new Objectives_BuildStructure("Objectives/GroupTitles/Temple", StructureBrain.TYPES.TEMPLE));
			}
			break;
		case "Objectives/GroupTitles/Temple":
			if (ObjectiveManager.GroupExists("Objectives/GroupTitles/PreachSermon"))
			{
				break;
			}
			Debug.Log("UNLOCK PREACH SERMON!");
			HideAll();
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
			StartCoroutine(WaitForPlayerToStopMeditiating(delegate
			{
				RatPreachSermon.SetActive(true);
				RatPreachSermon.GetComponent<Interaction_SimpleConversation>().Play();
				PlayerFarming.Instance.GoToAndStop(RatPreachSermon.transform.position + Vector3.right * 1.5f, RatPreachSermon, false, true);
				StartCoroutine(WaitForConversationToFinish(delegate
				{
					PlayerFarming.Instance.EndGoToAndStop();
					PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
				}));
			}));
			break;
		case "Objectives/GroupTitles/PreachSermon":
			if (!DataManager.Instance.GivenSermonQuest && !ObjectiveManager.GroupExists("Objectives/GroupTitles/BonesAndRitual"))
			{
				HideAll();
				RatBonesAndRitual.SetActive(true);
				MMConversation.OnConversationEnd += _003C_003Ec._003C_003E9._003COnObjectiveComplete_003Eg__CompletedSermon_007C33_2;
				if (PlayerFarming.Location == FollowerLocation.Base)
				{
					RatBonesAndRitual.transform.position += Vector3.down * 2f;
				}
			}
			break;
		case "Objectives/GroupTitles/DeclareDoctrine":
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/BonesAndRitual"))
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BonesAndRitual", Objectives.CustomQuestTypes.GoToDungeon), true);
				ObjectiveManager.Add(new Objectives_CollectItem("Objectives/GroupTitles/BonesAndRitual", InventoryItem.ITEM_TYPE.BONE, 25), true);
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BonesAndRitual", Objectives.CustomQuestTypes.PerformAnyRitual), true);
			}
			break;
		case "Objectives/GroupTitles/BonesAndRitual":
			if (!DataManager.Instance.ShowLoyaltyBars && DataManager.Instance.HasPerformedRitual)
			{
				if (PlayerFarming.Location == FollowerLocation.Base)
				{
					DataManager.Instance.DiscoverLocation(FollowerLocation.Hub1_RatauOutside);
				}
				if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/Disciple"))
				{
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.BlessAFollower), true);
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.GiveGift), true);
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.Disciple), true);
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.LoyaltyCollectReward), true);
				}
				DataManager.Instance.GivenLoyaltyQuestDay = TimeManager.CurrentDay;
				DataManager.Instance.OnboardingFinished = true;
				DataManager.Instance.ShowLoyaltyBars = true;
			}
			if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/DeclareDoctrine"))
			{
				BaseGoopDoor.Instance.UnblockGoopDoor();
			}
			break;
		}
	}

	private IEnumerator WaitForPlayerToStopGoToAndStopping(Action callback)
	{
		Debug.Log("PlayerFarming.Instance.GoToAndStopping " + PlayerFarming.Instance.GoToAndStopping);
		yield return null;
		Debug.Log("PlayerFarming.Instance.GoToAndStopping " + PlayerFarming.Instance.GoToAndStopping);
		while (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Meditate)
		{
			yield return null;
		}
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		Debug.Log("TO ACTIVATE");
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator WaitForPlayerToStopMeditiating(Action callback)
	{
		yield return null;
		while (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Meditate)
		{
			yield return null;
		}
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator WaitForConversationToFinish(Action callback)
	{
		while (LetterBox.IsPlaying)
		{
			yield return null;
		}
		if (callback != null)
		{
			callback();
		}
	}

	public void ShowNewBuildingsAvailable()
	{
		DataManager.Instance.NewBuildings = true;
		Action onBuildingUnlocked = UpgradeSystem.OnBuildingUnlocked;
		if (onBuildingUnlocked != null)
		{
			onBuildingUnlocked();
		}
	}

	private void OnNewRecruit()
	{
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.IndoctrinateNewRecruit);
		Debug.Log("A");
		if (CurrentPhase != 0 && CurrentPhase != DataManager.OnboardingPhase.Indoctrinate && CurrentPhase != DataManager.OnboardingPhase.IndoctrinateBerriesAllowed && CurrentPhase != 0 && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Temple))
		{
			Debug.Log("B");
			CurrentPhase = DataManager.OnboardingPhase.Devotion;
			DataManager.Instance.AllowBuilding = true;
			DataManager.Instance.NewBuildings = true;
		}
		DataManager.Instance.Tutorial_First_Indoctoring = true;
	}

	public void MakeRatauAppearInDungeonGiveCurses()
	{
		DataManager.Instance.RatauToGiveCurseNextRun = true;
		DataManager.Instance.ShowHaroDoctrineStoneRoom = true;
	}

	public void AllowShrineBuild()
	{
	}

	public void LoyaltyRoutine()
	{
		StartCoroutine(LoyaltyRoutineIE());
	}

	private IEnumerator LoyaltyRoutineIE()
	{
		yield return new WaitForEndOfFrame();
		DataManager.Instance.ShowLoyaltyBars = true;
		Interaction_TempleAltar.Instance.FrontWall.SetActive(false);
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(RatLoyalty, 5f);
		GameManager.GetInstance().CameraSetOffset(RatLoyalty.GetComponent<Interaction_SimpleConversation>().CameraOffset);
		FollowerBrain followerBrain = null;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID))
			{
				followerBrain = allBrain;
				break;
			}
		}
		bool waiting = true;
		AstarPath currentPath = AstarPath.active;
		AstarPath.active = null;
		FollowerManager.SpawnedFollower spawnedFollower = FollowerManager.SpawnCopyFollower(followerBrain._directInfoAccess, ChurchFollowerManager.Instance.DoorPosition.position, ChurchFollowerManager.Instance.transform, FollowerLocation.Church);
		spawnedFollower.Follower.GoTo(RatLoyalty.transform.position + Vector3.right * 2f, delegate
		{
			waiting = false;
		});
		yield return new WaitForEndOfFrame();
		while (waiting)
		{
			yield return null;
		}
		spawnedFollower.Follower.FacePosition(RatLoyalty.transform.position);
		yield return new WaitForSeconds(0.5f);
		waiting = true;
		if (RatauSpine != null)
		{
			RatauSpine.AnimationState.SetAnimation(0, "give-item", false);
			RatauSpine.AnimationState.AddAnimation(0, "idle", false, 0f);
		}
		spawnedFollower.Follower.AdorationUI.Show();
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", RatLoyalty.transform.position);
		ResourceCustomTarget.Create(spawnedFollower.Follower.gameObject, RatLoyalty.transform.position, InventoryItem.ITEM_TYPE.GIFT_SMALL, delegate
		{
			AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", spawnedFollower.Follower.transform.position);
			spawnedFollower.FollowerFakeBrain.AddAdoration(spawnedFollower.Follower, FollowerBrain.AdorationActions.Gift, null);
			spawnedFollower.FollowerBrain.AddAdoration(spawnedFollower.Follower, FollowerBrain.AdorationActions.Gift, null);
			spawnedFollower.Follower.TimedAnimation("Reactions/react-enlightened1", 2f, delegate
			{
				waiting = false;
			});
		});
		while (waiting)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		List<ConversationEntry> c = new List<ConversationEntry>
		{
			new ConversationEntry(RatLoyalty, "Conversation_NPC/Ratau/Base/Loyalty/4", "talk-excited"),
			new ConversationEntry(RatLoyalty, "Conversation_NPC/Ratau/Spells/1/2")
		};
		c[0].CharacterName = ScriptLocalization.NAMES.Ratau;
		c[1].CharacterName = ScriptLocalization.NAMES.Ratau;
		c[0].soundPath = "event:/dialogue/ratau/standard_ratau";
		c[1].soundPath = "event:/dialogue/ratau/standard_ratau";
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.LevellingUp))
		{
			UITutorialOverlayController tutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.LevellingUp);
			UITutorialOverlayController uITutorialOverlayController = tutorialOverlay;
			uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
			{
				tutorialOverlay = null;
			});
			while (tutorialOverlay != null)
			{
				yield return null;
			}
		}
		MMConversation.Play(new ConversationObject(c, null, delegate
		{
			UnlockMapLocation component = GetComponent<UnlockMapLocation>();
			component.Callback.AddListener(delegate
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitRatau", Objectives.CustomQuestTypes.VisitRatau));
				StartCoroutine(ActivateTeleporterRoutine());
			});
			component.Play();
		}));
		spawnedFollower.Follower.GoTo(ChurchFollowerManager.Instance.DoorPosition.position, delegate
		{
			FollowerManager.CleanUpCopyFollower(spawnedFollower);
		});
		yield return new WaitForEndOfFrame();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		RatLoyalty.GetComponent<spineChangeAnimationSimple>().Play();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		Interaction_TempleAltar.Instance.FrontWall.SetActive(true);
		AstarPath.active = currentPath;
		if (!ObjectiveManager.GroupExists("Objectives/GroupTitles/Disciple"))
		{
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.BlessAFollower), true);
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.GiveGift), true);
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.Disciple), true);
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/Disciple", Objectives.CustomQuestTypes.LoyaltyCollectReward), true);
		}
		BaseGoopDoor.Instance.UnblockGoopDoor();
		DataManager.Instance.GivenLoyaltyQuestDay = TimeManager.CurrentDay;
		DataManager.Instance.OnboardingFinished = true;
	}

	private IEnumerator ActivateTeleporterRoutine()
	{
		while (PlayerFarming.Location != FollowerLocation.Base)
		{
			yield return null;
		}
		Interaction_BaseTeleporter.Instance.ActivateRoutine();
	}

	private void ShowMissionBoard()
	{
		DataManager.Instance.Tutorial_Mission_Board = true;
		DataManager.Instance.MissionShrineUnlocked = true;
		Rat7MessageBoard.SetActive(true);
		Rat7MessageBoard.gameObject.GetComponent<Interaction_SimpleConversation>().Play();
	}

	public void AddMission()
	{
		UnityEngine.Object.FindObjectOfType<Interaction_MissionShrine>().AddNewMission();
	}

	public bool FirstFollower()
	{
		if (DataManager.Instance.Followers_Recruit.Count == 0)
		{
			string randomUnlockedSkin = DataManager.GetRandomUnlockedSkin();
			FollowerInfo item = FollowerInfo.NewCharacter(FollowerLocation.Base, randomUnlockedSkin);
			DataManager.SetFollowerSkinUnlocked(randomUnlockedSkin);
			DataManager.Instance.Followers_Recruit.Add(item);
		}
		if (!BiomeBaseManager.Instance.SpawnExistingRecruits || DataManager.Instance.Followers_Recruit.Count <= 1)
		{
			BiomeBaseManager.Instance.SpawnExistingRecruits = true;
			FollowerManager.SpawnExistingRecruits(BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position);
			UnityEngine.Object.FindObjectOfType<FollowerRecruit>().ManualTriggerAnimateIn();
			return true;
		}
		return false;
	}

	public void CreateFollowers()
	{
		StartCoroutine(CreateFollowersRoutine());
	}

	private IEnumerator CreateFollowersRoutine()
	{
		if (!BiomeBaseManager.Instance.SpawnExistingRecruits || DataManager.Instance.Followers_Recruit.Count <= 0)
		{
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew(true, true, true);
			GameManager.GetInstance().OnConversationNext(BiomeBaseManager.Instance.RecruitSpawnLocation, 6f);
			yield return new WaitForSeconds(0.5f);
			FirstFollower();
			yield return new WaitForSeconds(2f);
			GameManager.GetInstance().OnConversationNext(BiomeBaseManager.Instance.RecruitSpawnLocation, 8f);
			yield return new WaitForSeconds(1f);
			GameManager.GetInstance().OnConversationEnd();
		}
		else
		{
			FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
			DataManager.Instance.Followers_Recruit.Add(followerInfo);
			DataManager.SetFollowerSkinUnlocked(followerInfo.SkinName);
		}
	}

	public void PlayShrineTutorial()
	{
		UnityEngine.Object.Instantiate(UITutorialPrefab, GameObject.FindWithTag("Canvas").transform);
	}

	public void ShowBaseFaith()
	{
		StartCoroutine(ShowBaseFaithRoutine());
	}

	private IEnumerator ShowBaseFaithRoutine()
	{
		yield return new WaitForSeconds(1f);
		HUD_Manager.Instance.BaseDetailsTransition.MoveBackInFunction();
	}

	public void ShowPausePrompt()
	{
		Debug.Log("PAUSE PROMPT!");
		StartCoroutine(PausePromptRoutine());
	}

	private IEnumerator PausePromptRoutine()
	{
		yield return null;
		if (PlayerFarming.Instance.GoToAndStopping)
		{
			PlayerFarming.Instance.EndGoToAndStop();
		}
		yield return null;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		GameObject g = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UI Control Prompt Tutorial Menu Screen"), GameObject.FindWithTag("Canvas").transform) as GameObject;
		CanvasGroup ControlsHUD = g.GetComponent<CanvasGroup>();
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				ControlsHUD.alpha = Mathf.Lerp(0f, 1f, Progress / Duration);
				yield return null;
				continue;
			}
			break;
		}
		while (!InputManager.Gameplay.GetMenuButtonHeld())
		{
			yield return null;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(g);
		HideAll();
		Rat6Meditation.SetActive(true);
		Rat6Meditation.gameObject.GetComponent<Interaction_SimpleConversation>().Play();
	}

	public void ShowMeditatePrompt()
	{
		Debug.Log("MEDITATE PROMPT!");
		StartCoroutine(ShowMeditatePromptRoutine());
	}

	private IEnumerator ShowMeditatePromptRoutine()
	{
		yield return null;
		if (PlayerFarming.Instance.GoToAndStopping)
		{
			PlayerFarming.Instance.EndGoToAndStop();
		}
		yield return null;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAction0;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "idle", true);
		GameObject g = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UI Control Prompt Tutorial Meditate"), GameObject.FindWithTag("Canvas").transform) as GameObject;
		CanvasGroup ControlsHUD = g.GetComponent<CanvasGroup>();
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				ControlsHUD.alpha = Mathf.Lerp(0f, 1f, Progress / Duration);
				yield return null;
				continue;
			}
			break;
		}
		while (!InputManager.Gameplay.GetCurseButtonHeld())
		{
			yield return null;
		}
		UnityEngine.Object.Destroy(g);
		HideAll();
	}

	public void RevealCultFaith()
	{
		if ((bool)CultFaithManager.Instance)
		{
			CultFaithManager.Instance.Reveal();
		}
	}

	public void RevealCultHunger()
	{
		HungerBar.Instance.Reveal();
	}

	private void TryGiveFollowerOnboardingQuest()
	{
		if (PlayerFarming.Location != FollowerLocation.Base || DataManager.Instance.Followers.Count == 0)
		{
			return;
		}
		List<FollowerInfo> list = new List<FollowerInfo>(DataManager.Instance.Followers);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!FollowerBrain.CanFollowerGiveQuest(list[num]))
			{
				list.RemoveAt(num);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		if (DataManager.Instance.CurrentOnboardingFollowerID != -1)
		{
			Follower follower = FollowerManager.FindFollowerByID(DataManager.Instance.CurrentOnboardingFollowerID);
			if (follower == null || !FollowerBrain.CanContinueToGiveQuest(follower.Brain._directInfoAccess))
			{
				DataManager.Instance.CurrentOnboardingFollowerID = -1;
			}
		}
		if (TimeManager.IsNight || TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerQuestGivenTime < 480f || DataManager.Instance.CurrentOnboardingFollowerID != -1)
		{
			return;
		}
		foreach (FollowerInfo item in list)
		{
			if (CanGiveMealQuest(item) || CanGiveCleanUpQuest(item) || CanGiveHousingQuest(item) || CanGiveFarmingQuest(item) || CanGiveCultNameQuest(item) || CanGiveIllnessQuest(item) || CanGiveDissentQuest(item) || CanGiveResourceYardQuest(item) || CanGiveCrisisOfFaithQuest() || CanGiveSermonQuest() || CanGiveRaiseFaithQuest() || CanGiveOldQuest(item) || CanGiveHalloweenQuest() || CanGiveDeathCatAymAndBaalSecret(item) || CanGiveShamuraAymAndBaalSecret(item) || CanGiveLeshyRelicQuest(item) || CanGiveHeketRelicQuest(item) || CanGiveKallamarRelicQuest(item) || CanGiveShamuraRelicQuest(item))
			{
				DataManager.Instance.CurrentOnboardingFollowerID = item.ID;
				FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(item);
				if (orCreateBrain != null)
				{
					orCreateBrain.HardSwapToTask(new FollowerTask_GetAttention(Follower.ComplaintType.GiveOnboarding, false));
				}
				DataManager.Instance.LastFollowerQuestGivenTime = TimeManager.TotalElapsedGameTime;
				break;
			}
		}
	}

	public List<ObjectivesData> GetOnboardingQuests(int followerID)
	{
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(followerID);
		List<ObjectivesData> list = new List<ObjectivesData>();
		if (CanGiveHalloweenQuest())
		{
			list.Add(OnboardingQuestAssigned(new Objectives_PerformRitual("SeasonalEvents/Halloween/Onboarding/Title", UpgradeSystem.Type.Ritual_Halloween), infoByID));
			DataManager.Instance.OnboardedHalloween = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/HalloweenEvent/PerformRitualRequest";
		}
		else if (CanGiveMealQuest(infoByID))
		{
			list.Add(OnboardingQuestAssigned(new Objectives_Custom("Objectives/GroupTitles/MakeMoreFood", Objectives.CustomQuestTypes.CookFirstMeal), infoByID));
			DataManager.Instance.OnboardedMakingMoreFood = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/MakeMoreFood";
		}
		else if (CanGiveCleanUpQuest(infoByID))
		{
			int count = Interaction_Poop.Poops.Count;
			int count2 = Vomit.Vomits.Count;
			if (count > 0)
			{
				list.Add(OnboardingQuestAssigned(new Objectives_RemoveStructure("Objectives/GroupTitles/CleanUpBase", StructureBrain.TYPES.POOP), infoByID));
			}
			if (count2 > 0)
			{
				list.Add(OnboardingQuestAssigned(new Objectives_RemoveStructure("Objectives/GroupTitles/CleanUpBase", StructureBrain.TYPES.VOMIT), infoByID));
			}
			DataManager.Instance.OnboardedCleaningBase = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/CleanUpBase";
		}
		else if (CanGiveHousingQuest(infoByID))
		{
			if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Beds))
			{
				list.Add(OnboardingQuestAssigned(new Objectives_UnlockUpgrade("Objectives/GroupTitles/BuildHouse", UpgradeSystem.Type.Building_Beds), infoByID));
			}
			list.Add(OnboardingQuestAssigned(new Objectives_BuildStructure("Objectives/GroupTitles/BuildHouse", StructureBrain.TYPES.BED), infoByID));
			DataManager.Instance.OnboardedBuildingHouse = true;
			DataManager.Instance.OnboardedHomeless = true;
			UIDynamicNotificationCenter.HomelessFollowerData.CheckAll();
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/BuildHouse";
		}
		else if (CanGiveFarmingQuest(infoByID))
		{
			if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Farms))
			{
				list.Add(OnboardingQuestAssigned(new Objectives_UnlockUpgrade("Objectives/GroupTitles/BuildFarm", UpgradeSystem.Type.Building_Farms), infoByID));
			}
			list.Add(OnboardingQuestAssigned(new Objectives_BuildStructure("Objectives/GroupTitles/BuildFarm", StructureBrain.TYPES.FARM_PLOT), infoByID));
			list.Add(OnboardingQuestAssigned(new Objectives_Custom("Objectives/GroupTitles/BuildFarm", Objectives.CustomQuestTypes.PlantCrops), infoByID));
			list.Add(OnboardingQuestAssigned(new Objectives_Custom("Objectives/GroupTitles/BuildFarm", Objectives.CustomQuestTypes.WaterCrops), infoByID));
			DataManager.Instance.OnboardedBuildFarm = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/BuildFarm";
		}
		else if (CanGiveCultNameQuest(infoByID))
		{
			list.Add(OnboardingQuestAssigned(new Objectives_Custom("Objectives/Custom/NameCult", Objectives.CustomQuestTypes.NameCult), infoByID));
			DataManager.Instance.OnboardedCultName = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/NameCult";
		}
		else if (CanGiveResourceYardQuest(infoByID))
		{
			if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_Lumberyard))
			{
				list.Add(OnboardingQuestAssigned(new Objectives_UnlockUpgrade("Objectives/GroupTitles/BuildResourceYard", UpgradeSystem.Type.Economy_Lumberyard), infoByID));
			}
			if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_Mine))
			{
				list.Add(OnboardingQuestAssigned(new Objectives_UnlockUpgrade("Objectives/GroupTitles/BuildResourceYard", UpgradeSystem.Type.Economy_Mine), infoByID));
			}
			list.Add(OnboardingQuestAssigned(new Objectives_BuildStructure("Objectives/GroupTitles/BuildResourceYard", StructureBrain.TYPES.LUMBERJACK_STATION), infoByID));
			list.Add(OnboardingQuestAssigned(new Objectives_BuildStructure("Objectives/GroupTitles/BuildResourceYard", StructureBrain.TYPES.BLOODSTONE_MINE), infoByID));
			DataManager.Instance.OnboardedResourceYard = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/BuildResourceYard";
		}
		else if (CanGiveIllnessQuest(infoByID))
		{
			bool flag = false;
			foreach (ObjectivesData objective in DataManager.Instance.Objectives)
			{
				if (objective.Type == Objectives.TYPES.REMOVE_STRUCTURES && (((Objectives_RemoveStructure)objective).StructureType == StructureBrain.TYPES.POOP || ((Objectives_RemoveStructure)objective).StructureType == StructureBrain.TYPES.VOMIT))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (Interaction_Poop.Poops.Count > 0)
				{
					list.Add(OnboardingQuestAssigned(new Objectives_RemoveStructure("Objectives/GroupTitles/SickFollower", StructureBrain.TYPES.POOP), infoByID));
				}
				if (Vomit.Vomits.Count > 0)
				{
					list.Add(OnboardingQuestAssigned(new Objectives_RemoveStructure("Objectives/GroupTitles/SickFollower", StructureBrain.TYPES.VOMIT), infoByID));
				}
			}
			DataManager.Instance.LastFollowerToBecomeIll = TimeManager.TotalElapsedGameTime;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/SickFollower";
			DataManager.Instance.OnboardedSickFollower = true;
			list.Add(OnboardingQuestAssigned(new Objectives_BedRest("Objectives/GroupTitles/SickFollower", infoByID.Name), infoByID));
			list.Add(OnboardingQuestAssigned(new Objectives_Custom("Objectives/GroupTitles/SickFollower", Objectives.CustomQuestTypes.FollowerRecoverIllness, infoByID.ID), infoByID));
		}
		else if (CanGiveDissentQuest(infoByID))
		{
			DataManager.Instance.OnboardedDissenter = true;
			DataManager.Instance.LastFollowerToStartDissenting = TimeManager.TotalElapsedGameTime;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboarding/CureDissenter";
			ObjectivesData objectivesData = new Objectives_Custom("Objectives/GroupTitles/CureDissenter", Objectives.CustomQuestTypes.CureDissenter, infoByID.ID);
			list.Add(OnboardingQuestAssigned(objectivesData, infoByID));
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (!FollowerManager.FollowerLocked(allBrain.Info.ID) && allBrain.Info.ID != infoByID.ID)
				{
					objectivesData.Follower = allBrain.Info.ID;
					break;
				}
			}
		}
		else if (CanGiveOldQuest(infoByID))
		{
			DataManager.Instance.OnboardedOldFollower = true;
			DataManager.Instance.LastFollowerToReachOldAge = TimeManager.TotalElapsedGameTime;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/FollowerOnboardOldAge";
			list.Add(OnboardingQuestAssigned(new Objectives_Custom("Objectives/GroupTitles/Temple", Objectives.CustomQuestTypes.None, -1, 2400f), infoByID));
		}
		else if (CanGiveCrisisOfFaithQuest())
		{
			list.Add(OnboardingQuestAssigned(new Objectives_Custom("Objectives/GroupTitles/CrisisOfFaith", Objectives.CustomQuestTypes.CrisisOfFaith, -1, 2400f), infoByID));
			DataManager.Instance.CurrentOnboardingFollowerTerm = "FollowerInteractions/GiveQuest/CrisisOfFaith";
			DataManager.Instance.TimeSinceLastCrisisOfFaithQuest = TimeManager.TotalElapsedGameTime;
		}
		else if (CanGiveDeathCatAymAndBaalSecret(infoByID))
		{
			list.Add(null);
			DataManager.Instance.DeathCatBaalAndAymSecret = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/DeathCat_AymAndBaal/0";
		}
		else if (CanGiveShamuraAymAndBaalSecret(infoByID))
		{
			list.Add(null);
			DataManager.Instance.ShamuraBaalAndAymSecret = true;
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/Shaumra_AymAndBaal/0";
		}
		else if (CanGiveLeshyRelicQuest(infoByID))
		{
			Objective_FindRelic objective_FindRelic = new Objective_FindRelic("Objectives/GroupTitles/FindRelic", FollowerLocation.Dungeon1_1, RelicType.DamageOnTouch_Familiar);
			objective_FindRelic.Follower = infoByID.ID;
			objective_FindRelic.FailLocked = true;
			objective_FindRelic.CompleteTerm = "Dungeon1_1/QuestCompleted";
			list.Add(objective_FindRelic);
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/Dungeon1_1/GiveQuest/0";
		}
		else if (CanGiveHeketRelicQuest(infoByID))
		{
			Objective_FindRelic objective_FindRelic2 = new Objective_FindRelic("Objectives/GroupTitles/FindRelic", FollowerLocation.Dungeon1_2, RelicType.IncreaseDamageForDuration);
			objective_FindRelic2.Follower = infoByID.ID;
			objective_FindRelic2.FailLocked = true;
			objective_FindRelic2.CompleteTerm = "Dungeon1_2/QuestCompleted";
			list.Add(objective_FindRelic2);
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/Dungeon1_2/GiveQuest/0";
		}
		else if (CanGiveKallamarRelicQuest(infoByID))
		{
			Objective_FindRelic objective_FindRelic3 = new Objective_FindRelic("Objectives/GroupTitles/FindRelic", FollowerLocation.Dungeon1_3, RelicType.SpawnCombatFollower);
			objective_FindRelic3.Follower = infoByID.ID;
			objective_FindRelic3.FailLocked = true;
			objective_FindRelic3.CompleteTerm = "Dungeon1_3/QuestCompleted";
			list.Add(objective_FindRelic3);
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/Dungeon1_3/GiveQuest/0";
		}
		else if (CanGiveShamuraRelicQuest(infoByID))
		{
			Objective_FindRelic objective_FindRelic4 = new Objective_FindRelic("Objectives/GroupTitles/FindRelic", FollowerLocation.Dungeon1_4, RelicType.GungeonBlank);
			objective_FindRelic4.Follower = infoByID.ID;
			objective_FindRelic4.FailLocked = true;
			objective_FindRelic4.CompleteTerm = "Dungeon1_4/QuestCompleted";
			list.Add(objective_FindRelic4);
			DataManager.Instance.CurrentOnboardingFollowerTerm = "Conversation_NPC/Dungeon1_4/GiveQuest/0";
		}
		return list;
	}

	private ObjectivesData OnboardingQuestAssigned(ObjectivesData quest, FollowerInfo follower)
	{
		FollowerBrain.GetOrCreateBrain(follower);
		quest.AutoRemoveQuestOnceComplete = false;
		quest.Follower = follower.ID;
		return quest;
	}

	private bool CanGiveMealQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedMakingMoreFood && DataManager.Instance.MealsCooked < 5 && StructureManager.GetAllStructuresOfType<Structures_Meal>().Count <= 0 && follower.Satiation < 30f)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveCleanUpQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedCleaningBase)
		{
			int count = Interaction_Poop.Poops.Count;
			int count2 = Vomit.Vomits.Count;
			if (count + count2 > 2)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanGiveHousingQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedBuildingHouse && !FollowerBrain.GetOrCreateBrain(follower).HasHome && follower.DaysSleptOutside >= 2 && follower.DwellingID == Dwelling.NO_HOME)
		{
			foreach (ObjectivesData objective in DataManager.Instance.Objectives)
			{
				if (objective is Objectives_BuildStructure && ((Objectives_BuildStructure)objective).StructureType == StructureBrain.TYPES.BED)
				{
					return false;
				}
			}
			foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
			{
				if (completedObjective is Objectives_BuildStructure && ((Objectives_BuildStructure)completedObjective).StructureType == StructureBrain.TYPES.BED)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private bool CanGiveFarmingQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedBuildFarm && TimeManager.CurrentDay > 3 && StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.FARM_PLOT).Count == 0 && StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.SHRINE).Count != 0)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveCultNameQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedCultName && TimeManager.CurrentDay > 2)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveIllnessQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedSickFollower && 1f - IllnessBar.Count / IllnessBar.Max < 0.25f && TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToBecomeIll > 600f / DifficultyManager.GetTimeBetweenIllnessMultiplier())
		{
			return true;
		}
		return false;
	}

	private bool CanGiveDissentQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedDissenter && CultFaithManager.CurrentFaith / 85f < 0.25f && TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToStartDissenting > 600f / DifficultyManager.GetTimeBetweenDissentingMultiplier())
		{
			return true;
		}
		return false;
	}

	private bool CanGiveOldQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedOldFollower && follower.Age >= follower.LifeExpectancy && TimeManager.TotalElapsedGameTime - DataManager.Instance.LastFollowerToReachOldAge > 600f / DifficultyManager.GetTimeBetweenOldAgeMultiplier())
		{
			return true;
		}
		return false;
	}

	private bool CanGiveResourceYardQuest(FollowerInfo follower)
	{
		if (!DataManager.Instance.OnboardedResourceYard && TimeManager.CurrentDay > 12 && !DataManager.Instance.HistoryOfStructures.Contains(StructureBrain.TYPES.LUMBERJACK_STATION) && !DataManager.Instance.HistoryOfStructures.Contains(StructureBrain.TYPES.BLOODSTONE_MINE))
		{
			return true;
		}
		return false;
	}

	private bool CanGiveCrisisOfFaithQuest()
	{
		if (DataManager.Instance.OnboardingFinished && !DataManager.Instance.OnboardedCrisisOfFaith && TimeManager.CurrentDay > 5 && CultFaithManager.CurrentFaith <= 5f && TimeManager.TotalElapsedGameTime - DataManager.Instance.TimeSinceLastCrisisOfFaithQuest > 3600f && DataManager.Instance.TimeSinceFaithHitEmpty != -1f && TimeManager.TotalElapsedGameTime - DataManager.Instance.TimeSinceFaithHitEmpty > 240f)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveSermonQuest()
	{
		if (DataManager.Instance.OnboardingFinished && !DataManager.Instance.OnboardedSermon && TimeManager.CurrentDay > 5 && TimeManager.CurrentDay < 10 && CultFaithManager.CurrentFaith <= 42.5f && TimeManager.CurrentDay - DataManager.Instance.PreviousSermonDayIndex > 1)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveFaithOfFlockQuest()
	{
		if (!DataManager.Instance.OnboardedFaithOfFlock && DataManager.Instance.LastDaySincePlayerUpgrade != -1 && (TimeManager.CurrentDay - DataManager.Instance.LastDaySincePlayerUpgrade > 5 || (DataManager.Instance.playerDeathsInARow >= 2 && TimeManager.CurrentDay < 10)))
		{
			return true;
		}
		return false;
	}

	private bool CanGiveRaiseFaithQuest()
	{
		if (DataManager.Instance.OnboardingFinished && !DataManager.Instance.OnboardedRaiseFaith && CultFaithManager.CurrentFaith <= 42.5f && DataManager.Instance.GivenLoyaltyQuestDay != -1 && TimeManager.CurrentDay - DataManager.Instance.GivenLoyaltyQuestDay >= 1 && TimeManager.CurrentDay - DataManager.Instance.GivenLoyaltyQuestDay < 5)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveHalloweenQuest()
	{
		if (DataManager.Instance.OnboardingFinished && !DataManager.Instance.OnboardedHalloween && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ritual_Halloween) && SeasonalEventManager.IsSeasonalEventActive(SeasonalEventType.Halloween) && !FollowerBrainStats.IsBloodMoon && DataManager.Instance.LastHalloween == float.MinValue)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveDeathCatAymAndBaalSecret(FollowerInfo info)
	{
		if (DataManager.Instance.HasBaalSkin && DataManager.Instance.HasAymSkin && info.ID == FollowerManager.DeathCatID && !DataManager.Instance.DeathCatBaalAndAymSecret)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveShamuraAymAndBaalSecret(FollowerInfo info)
	{
		if (DataManager.Instance.HasBaalSkin && DataManager.Instance.HasAymSkin && info.ID == FollowerManager.ShamuraID && !DataManager.Instance.ShamuraBaalAndAymSecret)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveLeshyRelicQuest(FollowerInfo info)
	{
		if (info.ID == FollowerManager.LeshyID && !ObjectiveManager.HasCustomObjective(Objectives.TYPES.FIND_RELIC) && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.DamageOnTouch_Familiar) && DataManager.Instance.OnboardedRelics)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveHeketRelicQuest(FollowerInfo info)
	{
		if (info.ID == FollowerManager.HeketID && !ObjectiveManager.HasCustomObjective(Objectives.TYPES.FIND_RELIC) && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.IncreaseDamageForDuration) && DataManager.Instance.OnboardedRelics)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveKallamarRelicQuest(FollowerInfo info)
	{
		if (info.ID == FollowerManager.KallamarID && !ObjectiveManager.HasCustomObjective(Objectives.TYPES.FIND_RELIC) && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.SpawnCombatFollower) && DataManager.Instance.OnboardedRelics)
		{
			return true;
		}
		return false;
	}

	private bool CanGiveShamuraRelicQuest(FollowerInfo info)
	{
		if (info.ID == FollowerManager.ShamuraID && !ObjectiveManager.HasCustomObjective(Objectives.TYPES.FIND_RELIC) && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.GungeonBlank) && DataManager.Instance.OnboardedRelics)
		{
			return true;
		}
		return false;
	}
}
