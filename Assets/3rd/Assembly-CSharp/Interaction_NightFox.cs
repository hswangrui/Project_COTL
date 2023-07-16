using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MMTools;
using Spine.Unity;
using src.Extensions;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class Interaction_NightFox : Interaction
{
	public enum EncounterTypes
	{
		Fish,
		Follower,
		FollowerOrHeart,
		Ratau
	}

	public Interaction_KeyPiece KeyPiecePrefab;

	[SerializeField]
	private GameObject goopFloorParticle;

	private SkeletonAnimation goopSkeleton;

	public SimpleSetCamera SimpleSetCamera;

	public GameObject MoonIcon;

	public BiomeLightingSettings LightingSettings;

	public OverrideLightingProperties overrideLightingProperties;

	public Vector3 RatauPosition;

	public Vector3 RatauEndPosition;

	public int RatauScale = 1;

	public SkeletonAnimation Spine;

	private bool Available;

	private string sPeerInToTheDarkness;

	private bool Activating;

	public GameObject PlayerPosition;

	private List<MMTools.Response> responses;

	private List<ConversationEntry> conversationEntries;

	private List<ConversationEntry> conversationOnlyQuestionEntries;

	private int StoneCount;

	public bool ChooseFollowerOverHeart = true;

	private int FollowerCount;

	private List<FollowerManager.SpawnedFollower> SpawnedFollowers = new List<FollowerManager.SpawnedFollower>();

	public GameObject RatauPrefab;

	private ColorGrading colorGrading;

	public EncounterTypes EncounterType
	{
		get
		{
			return (EncounterTypes)DataManager.Instance.CurrentFoxEncounter;
		}
	}

	public static FollowerLocation CurrentFoxLocation
	{
		get
		{
			return DataManager.Instance.CurrentFoxLocation;
		}
		set
		{
			DataManager.Instance.CurrentFoxLocation = value;
		}
	}

	private void Start()
	{
		BiomeConstants.Instance.ppv.profile.TryGetSettings<ColorGrading>(out colorGrading);
		UpdateLocalisation();
		goopSkeleton = goopFloorParticle.GetComponent<SkeletonAnimation>();
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
	}

	private void OnPlayerLocationSet()
	{
		if ((CurrentFoxLocation == FollowerLocation.None || CurrentFoxLocation == PlayerFarming.Location) && !DataManager.Instance.FoxCompleted.Contains(PlayerFarming.Location))
		{
			Spine.gameObject.SetActive(false);
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
			OnNewPhaseStarted();
		}
		else
		{
			Deactivate(false);
		}
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
	}

	private void Deactivate(bool DelayMoonIcon)
	{
		base.gameObject.SetActive(false);
		SimpleSetCamera.AutomaticallyActivate = false;
		bool flag = MoonIcon != null;
		if (DelayMoonIcon)
		{
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval(5f);
			sequence.Append(MoonIcon.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
			{
				MoonIcon.SetActive(false);
			}));
			sequence.Play();
		}
		else
		{
			MoonIcon.SetActive(false);
		}
	}

	private void OnNewPhaseStarted()
	{
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			Available = true;
		}
		else
		{
			Available = false;
			if (MoonIcon != null)
			{
				MoonIcon.transform.localScale = Vector3.one * 1.15f;
				MoonIcon.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBounce);
			}
		}
		base.HasChanged = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sPeerInToTheDarkness = ScriptLocalization.Interactions.PeerInToTheDarkness;
	}

	public override void GetLabel()
	{
		if (Available && !Activating)
		{
			base.Label = sPeerInToTheDarkness;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Activating = true;
		PlayerFarming.Instance.GoToAndStop(PlayerPosition.transform.position, null, false, false, delegate
		{
			StartCoroutine(AppearRoutine());
		});
	}

	private bool CanAfford()
	{
		switch (EncounterType)
		{
		case EncounterTypes.Fish:
		{
			int num = 0;
			foreach (InventoryItem item in Inventory.items)
			{
				if (InventoryItem.IsFish((InventoryItem.ITEM_TYPE)item.type) && item.type != 33)
				{
					num += item.quantity;
				}
			}
			return num >= 1;
		}
		case EncounterTypes.Follower:
			return FollowerBrain.AllAvailableFollowerBrains().Count > 0;
		case EncounterTypes.FollowerOrHeart:
			return FollowerBrain.AllAvailableFollowerBrains().Count >= 2;
		case EncounterTypes.Ratau:
			return true;
		default:
			return false;
		}
	}

	private IEnumerator AppearRoutine()
	{
		CurrentFoxLocation = PlayerFarming.Location;
		AudioManager.Instance.SetMusicRoomID(1, "shore_id");
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = Utils.GetAngle(state.transform.position, Spine.transform.position);
		SimulationManager.Pause();
		SimpleSetCamera.AutomaticallyActivate = false;
		SimpleSetCamera.DectivateDistance = 0.5f;
		SimpleSetCamera.Reset();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(goopFloorParticle.gameObject, 6f);
		ShowGoop();
		yield return new WaitForSeconds(1f);
		Spine.AnimationState.SetAnimation(0, "enter", false);
		Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		Spine.gameObject.SetActive(true);
		yield return new WaitForSeconds(2.6666667f);
		GetQuestionAndResponses();
		if (DataManager.Instance.FoxIntroductions.Contains(PlayerFarming.Location))
		{
			PoseQuestion();
		}
		else
		{
			IntroductionConversation();
		}
	}

	private void GetQuestionAndResponses()
	{
		responses = new List<MMTools.Response>();
		int encounterType = (int)EncounterType;
		string text = (CanAfford() ? "Conversation_NPC/Fox/Response_Yes" : "Conversation_NPC/Fox/Meeting/Response_CantAfford");
		responses.Add(new MMTools.Response(text, delegate
		{
			StartCoroutine(CanAfford() ? Agree() : CantAfford());
		}, text));
		text = "Conversation_NPC/Fox/Response_No";
		responses.Add(new MMTools.Response(text, delegate
		{
			StartCoroutine(Disagree());
		}, text));
		conversationEntries = new List<ConversationEntry>();
		int num = -1;
		text = "Conversation_NPC/Fox/Meeting" + encounterType + "/";
		while (LocalizationManager.GetTermData(text + ++num) != null)
		{
			Debug.Log(text + "  " + num);
			ConversationEntry conversationEntry = new ConversationEntry(Spine.gameObject, text + num);
			conversationEntries.Add(conversationEntry);
			conversationEntry.soundPath = "event:/dialogue/the_night/standard_the_night";
		}
		conversationOnlyQuestionEntries = new List<ConversationEntry>();
		conversationOnlyQuestionEntries.Add(conversationEntries[conversationEntries.Count - 1]);
	}

	private void IntroductionConversation()
	{
		Debug.Log("IntroductionConversation");
		DataManager.Instance.FoxIntroductions.Add(PlayerFarming.Location);
		ConversationObject conversationObject = new ConversationObject(conversationEntries, responses, null);
		foreach (ConversationEntry conversationEntry in conversationEntries)
		{
			conversationEntry.soundPath = "event:/dialogue/the_night/standard_the_night";
		}
		MMConversation.Play(conversationObject, false, true, false);
		CameraIncludePlayer();
	}

	private void PoseQuestion()
	{
		ConversationObject conversationObject = new ConversationObject(conversationOnlyQuestionEntries, responses, null);
		foreach (ConversationEntry conversationOnlyQuestionEntry in conversationOnlyQuestionEntries)
		{
			conversationOnlyQuestionEntry.soundPath = "event:/dialogue/the_night/standard_the_night";
		}
		MMConversation.Play(conversationObject, false, true, false);
		CameraIncludePlayer();
	}

	private IEnumerator Agree()
	{
		yield return StartCoroutine(GiveItem());
		yield return new WaitForEndOfFrame();
		List<ConversationEntry> list = new List<ConversationEntry>();
		int encounterType = (int)EncounterType;
		int num = -1;
		string text = "Conversation_NPC/Fox/Meeting" + encounterType + "/Answer_A/";
		while (LocalizationManager.GetTermData(text + ++num) != null)
		{
			Debug.Log(text + "  " + num);
			ConversationEntry conversationEntry = new ConversationEntry(Spine.gameObject, text + num);
			if (encounterType == 3)
			{
				if (num == 0 || num == 1 || num == 2 || num == 3)
				{
					conversationEntry.soundPath = "event:/dialogue/the_night/laugh_the_night";
					conversationEntry.Animation = "talk-laugh";
				}
				else
				{
					conversationEntry.soundPath = "event:/dialogue/the_night/standard_the_night";
				}
			}
			else
			{
				conversationEntry.soundPath = "event:/dialogue/the_night/standard_the_night";
			}
			list.Add(conversationEntry);
		}
		MMConversation.Play(new ConversationObject(list, null, delegate
		{
			StartCoroutine(AgreeCallback());
		}), false, true, false, true, true, true);
		CameraIncludePlayer();
	}

	private IEnumerator AgreeCallback()
	{
		yield return new WaitForEndOfFrame();
		SimpleSetCamera.Reset();
		Debug.Log("AGREE!");
		Debug.Log("EncounterType: " + EncounterType);
		if (EncounterType == EncounterTypes.Ratau)
		{
			yield return StartCoroutine(GiveFinalRewards());
		}
		GameManager.GetInstance().RemoveAllFromCamera();
		Interaction_KeyPiece KeyPiece = UnityEngine.Object.Instantiate(KeyPiecePrefab, Spine.transform.position + new Vector3(0f, -0.2f, -1.25f), Quaternion.identity, base.transform.parent);
		KeyPiece.transform.localScale = Vector3.zero;
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(KeyPiece.gameObject, 6f);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short", Spine.transform.position);
		Spine.AnimationState.SetAnimation(0, "give-key", false);
		Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		KeyPiece.transform.DOScale(Vector3.one, 1f).SetEase(Ease.InBack).OnComplete(delegate
		{
			KeyPiece.transform.DOMove(state.transform.position + Vector3.back * 0.5f, 1f).SetEase(Ease.InBack).OnComplete(delegate
			{
				GameManager.GetInstance().OnConversationEnd(false);
				KeyPiece.OnInteract(state);
				StartCoroutine(ExitRoutine());
			});
		});
		SimulationManager.UnPause();
		DataManager.Instance.FoxCompleted.Add(PlayerFarming.Location);
		CurrentFoxLocation = FollowerLocation.None;
		DataManager.Instance.CurrentFoxEncounter++;
	}

	private IEnumerator GiveFinalRewards()
	{
		Debug.Log("Give final rewards!");
		bool Waiting = true;
		FollowerSkinCustomTarget.Create(Spine.transform.position, PlayerFarming.Instance.transform.position, 0.5f, "Nightwolf", delegate
		{
			Waiting = false;
		});
		while (Waiting)
		{
			yield return null;
		}
	}

	private IEnumerator ExitRoutine()
	{
		switch (SceneManager.GetActiveScene().name)
		{
		case "Hub-Shore":
			if (DataManager.Instance.Lighthouse_Lit)
			{
				AudioManager.Instance.SetMusicRoomID(6, "shore_id");
			}
			else
			{
				AudioManager.Instance.SetMusicRoomID(0, "shore_id");
			}
			break;
		case "Mushroom Research Site":
			AudioManager.Instance.SetMusicRoomID(3, "shore_id");
			break;
		case "Midas Cave":
			AudioManager.Instance.SetMusicRoomID(4, "shore_id");
			break;
		case "Dungeon Decoration Shop 1":
			AudioManager.Instance.SetMusicRoomID(5, "shore_id");
			break;
		default:
			AudioManager.Instance.SetMusicRoomID(0, "shore_id");
			break;
		}
		Spine.AnimationState.SetAnimation(0, "exit", false);
		yield return new WaitForSeconds(0.1f);
		Vector3 position = base.transform.position;
		AudioManager.Instance.PlayOneShot("event:/fishing/splash", position);
		yield return new WaitForSeconds(0.1f);
		AudioManager.Instance.PlayOneShot("event:/fishing/splash", position);
		yield return new WaitForSeconds(0.1f);
		AudioManager.Instance.PlayOneShot("event:/fishing/splash", position);
		yield return new WaitForSeconds(0.7f);
		ShowGoop();
		yield return new WaitForSeconds(1f);
		Activating = false;
		if (EncounterType >= EncounterTypes.Ratau)
		{
			yield return new WaitForSeconds(1f);
			if (DoctrineUpgradeSystem.TrySermonsStillAvailable() && DoctrineUpgradeSystem.TryGetStillDoctrineStone() && DataManager.Instance.GetVariable(DataManager.Variables.FirstDoctrineStone))
			{
				int i = 3;
				while (true)
				{
					int num = i - 1;
					i = num;
					if (num < 0)
					{
						break;
					}
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.DOCTRINE_STONE, 1, Spine.transform.position).GetComponent<Interaction_DoctrineStone>().MagnetToPlayer();
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
		Deactivate(true);
	}

	private IEnumerator CantAfford()
	{
		yield return new WaitForEndOfFrame();
		List<ConversationEntry> list = new List<ConversationEntry>();
		EncounterTypes encounterType = EncounterType;
		int num = -1;
		string text = "Conversation_NPC/Fox/Answer_CantAfford/";
		while (LocalizationManager.GetTermData(text + ++num) != null)
		{
			Debug.Log(text + "  " + num);
			ConversationEntry conversationEntry = new ConversationEntry(Spine.gameObject, text + num);
			list.Add(conversationEntry);
			conversationEntry.soundPath = "event:/dialogue/the_night/standard_the_night";
		}
		MMConversation.Play(new ConversationObject(list, null, delegate
		{
			StartCoroutine(DisagreeCallback());
		}), true, true, true, true, true, true);
		CameraIncludePlayer();
	}

	private IEnumerator Disagree()
	{
		yield return new WaitForEndOfFrame();
		List<ConversationEntry> list = new List<ConversationEntry>();
		int encounterType = (int)EncounterType;
		int num = -1;
		string text = "Conversation_NPC/Fox/Meeting" + encounterType + "/Answer_B/";
		while (LocalizationManager.GetTermData(text + ++num) != null)
		{
			Debug.Log(text + "  " + num);
			ConversationEntry conversationEntry = new ConversationEntry(Spine.gameObject, text + num);
			list.Add(conversationEntry);
			conversationEntry.soundPath = "event:/dialogue/the_night/standard_the_night";
		}
		MMConversation.Play(new ConversationObject(list, null, delegate
		{
			StartCoroutine(DisagreeCallback());
		}), true, true, true, true, true, true);
		CameraIncludePlayer();
	}

	private IEnumerator DisagreeCallback()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(0.5f);
		Spine.AnimationState.SetAnimation(0, "exit", false);
		yield return new WaitForSeconds(1f);
		ShowGoop();
		yield return new WaitForSeconds(1.6666667f);
		SimulationManager.UnPause();
		SimpleSetCamera.DectivateDistance = 1f;
		SimpleSetCamera.AutomaticallyActivate = true;
		AudioManager.Instance.SetMusicRoomID(0, "shore_id");
		Spine.gameObject.SetActive(false);
		Debug.Log("DISAGREE!");
		Activating = false;
	}

	private void CameraIncludePlayer()
	{
		MMConversation.ControlCamera = false;
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddToCamera(Spine.gameObject);
		GameManager.GetInstance().AddPlayerToCamera();
	}

	private IEnumerator GiveItem()
	{
		switch (EncounterType)
		{
		case EncounterTypes.Fish:
			yield return StartCoroutine(GiveFishRoutine());
			break;
		case EncounterTypes.Follower:
			yield return StartCoroutine(GiveFollowerRoutine());
			break;
		case EncounterTypes.FollowerOrHeart:
			yield return StartCoroutine(AskFollowerOrHeart());
			if (ChooseFollowerOverHeart)
			{
				yield return StartCoroutine(GiveFollowerRoutine());
			}
			else
			{
				yield return StartCoroutine(GiveHalfHeartRoutine());
			}
			break;
		case EncounterTypes.Ratau:
			yield return StartCoroutine(GiveRatauRoutine());
			break;
		}
	}

	private IEnumerator AskFollowerOrHeart()
	{
		GameObject g = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Choice Indicator"), GameObject.FindWithTag("Canvas").transform) as GameObject;
		ChoiceIndicator choice = g.GetComponent<ChoiceIndicator>();
		choice.Offset = new Vector3(0f, -350f);
		choice.Show("<sprite name=\"icon_UIHeartHalf\">", "Inventory/HALF_HEART", "<sprite name=\"icon_Followers\">x " + RequiredFollowers(), "Inventory/FOLLOWERS", delegate
		{
			ChooseFollowerOverHeart = false;
		}, delegate
		{
			ChooseFollowerOverHeart = true;
		}, state.transform.position);
		while (g != null)
		{
			choice.UpdatePosition(state.transform.position);
			yield return null;
		}
	}

	private IEnumerator GiveFishRoutine()
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		foreach (InventoryItem item in Inventory.items)
		{
			if (InventoryItem.IsFish((InventoryItem.ITEM_TYPE)item.type) && item.type != 33)
			{
				list.Add((InventoryItem.ITEM_TYPE)item.type);
			}
		}
		InventoryItem.ITEM_TYPE chosenFish = InventoryItem.ITEM_TYPE.NONE;
		UIItemSelectorOverlayController uIItemSelectorOverlayController = ShowItemSelector(list, "thenight_fish");
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			chosenFish = chosenItem;
		});
		while (chosenFish == InventoryItem.ITEM_TYPE.NONE)
		{
			yield return null;
		}
		bool waiting = true;
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", Spine.gameObject);
		ResourceCustomTarget.Create(Spine.gameObject, state.transform.position, chosenFish, delegate
		{
			waiting = false;
		});
		Inventory.ChangeItemQuantity((int)chosenFish, -1);
		while (waiting)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", base.gameObject);
	}

	private IEnumerator GiveHeartRoutine()
	{
		InventoryItem.ITEM_TYPE chosenHeart = InventoryItem.ITEM_TYPE.NONE;
		UIItemSelectorOverlayController uIItemSelectorOverlayController = ShowItemSelector(new List<InventoryItem.ITEM_TYPE> { InventoryItem.ITEM_TYPE.MONSTER_HEART }, "thenight_heart");
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			chosenHeart = chosenItem;
		});
		while (chosenHeart == InventoryItem.ITEM_TYPE.NONE)
		{
			yield return null;
		}
		bool waiting = true;
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", Spine.gameObject);
		ResourceCustomTarget.Create(Spine.gameObject, state.transform.position, chosenHeart, delegate
		{
			waiting = false;
		});
		Inventory.ChangeItemQuantity((int)chosenHeart, -1);
		while (waiting)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.gameObject);
	}

	private void FollowerTest()
	{
		Spine.gameObject.SetActive(true);
		GetQuestionAndResponses();
		StartCoroutine(Agree());
	}

	private float RequiredFollowers()
	{
		switch (EncounterType)
		{
		case EncounterTypes.Follower:
			return 1f;
		case EncounterTypes.FollowerOrHeart:
			return 2f;
		default:
			return 0f;
		}
	}

	private IEnumerator GiveFollowerRoutine()
	{
		SpawnedFollowers = new List<FollowerManager.SpawnedFollower>();
		FollowerCount = 0;
		FollowerSelect();
		while ((float)FollowerCount < RequiredFollowers())
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		FadeRedIn();
		AudioManager.Instance.PlayOneShot("event:/Stings/thenight_sacrifice_followers", PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNext(Spine.gameObject, 5f);
		CameraManager.instance.ShakeCameraForDuration(0.7f, 1.2f, 2.1333334f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		foreach (FollowerManager.SpawnedFollower spawnedFollower in SpawnedFollowers)
		{
			spawnedFollower.Follower.Spine.AnimationState.SetAnimation(1, "fox-sacrifice", false);
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(2.1333334f);
		FadeRedAway();
		foreach (FollowerManager.SpawnedFollower spawnedFollower2 in SpawnedFollowers)
		{
			FollowerManager.CleanUpCopyFollower(spawnedFollower2);
		}
		yield return new WaitForSeconds(1f);
	}

	private void FollowerSelect()
	{
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
	//	followerSelectInstance.VotingType = TwitchVoting.VotingType.SACRIFICE_TO_NIGHT_FOX;
		followerSelectInstance.Show(FollowerBrain.AllAvailableFollowerBrains(), null, false, UpgradeSystem.Type.Count, true, false);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
			StartCoroutine(SpawnFollower(followerInfo.ID));
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
	}

	private IEnumerator SpawnFollower(int ID)
	{
		yield return new WaitForSeconds(0.5f);
		float f = 360f / RequiredFollowers() * (float)FollowerCount * ((float)Math.PI / 180f);
		float num = 1.5f;
		Vector3 Position = new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
		Debug.Log("(ID) " + ID);
		Debug.Log("FollowerManager.FindFollowerInfo(ID) " + FollowerManager.FindFollowerInfo(ID));
		FollowerManager.SpawnedFollower spawnedFollower = FollowerManager.SpawnCopyFollower(FollowerManager.FindFollowerInfo(ID), state.transform.position + Vector3.down, Spine.transform, PlayerFarming.Location);
		SpawnedFollowers.Add(spawnedFollower);
		CameraManager.shakeCamera(1f);
		AudioManager.Instance.PlayOneShot("event:/player/standard_jump_spin_float", spawnedFollower.Follower.gameObject);
		spawnedFollower.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		spawnedFollower.Follower.Spine.AnimationState.SetAnimation(1, "fox-spawn", false);
		spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "fox-floating", true, 0f);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CureDissenter, ID);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.KillFollower, ID);
		FollowerManager.RemoveFollowerBrain(ID);
		yield return new WaitForSeconds(1f);
		spawnedFollower.Follower.transform.DOMove(Spine.transform.position + Position, 2f).SetEase(Ease.InOutQuad);
		AudioManager.Instance.PlayOneShot("event:/player/float_follower", spawnedFollower.Follower.gameObject);
		yield return new WaitForSeconds(2.5f);
		if ((float)(++FollowerCount) < RequiredFollowers())
		{
			FollowerSelect();
		}
	}

	private void TestHalfHeart()
	{
		StartCoroutine(GiveHalfHeartRoutine());
	}

	private IEnumerator GiveHalfHeartRoutine()
	{
		yield return null;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "gameover", false);
		AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/select_hearts", PlayerFarming.Instance.transform.position);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", PlayerFarming.Instance.transform.position);
		yield return new WaitForSeconds(3f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		CameraManager.shakeCamera(2f);
		AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", PlayerFarming.Instance.transform.position);
		BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "red", "burst_big");
		bool Waiting = true;
		ResourceCustomTarget.Create(Spine.gameObject, state.transform.position, InventoryItem.ITEM_TYPE.HALF_HEART, delegate
		{
			Waiting = false;
		});
		while (Waiting)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.gameObject);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "idle", true);
		HealthPlayer healthPlayer = UnityEngine.Object.FindObjectOfType<HealthPlayer>();
		healthPlayer.totalHP -= 1f;
		healthPlayer.HP = healthPlayer.totalHP;
		DataManager.Instance.PLAYER_HEALTH_MODIFIED--;
	}

	private void TestRat()
	{
		StartCoroutine(GiveRatauRoutine());
	}

	private IEnumerator GiveRatauRoutine()
	{
		yield return null;
		AudioManager.Instance.PlayOneShot("event:/Stings/sacrifice_ratau", PlayerFarming.Instance.transform.position);
		DataManager.Instance.RatauKilled = true;
		ObjectiveManager.FailDefeatKnucklebones("NAMES/Ratau");
		AudioManager.Instance.PlayOneShot("event:/player/standard_jump_spin_float", PlayerFarming.Instance.transform.position);
		SkeletonAnimation rat = UnityEngine.Object.Instantiate(RatauPrefab, RatauPosition, Quaternion.identity).GetComponentInChildren<SkeletonAnimation>();
		rat.AnimationState.SetAnimation(0, "ratau-sacrifice/start", false);
		rat.AnimationState.AddAnimation(0, "ratau-sacrifice/loop", true, 0f);
		rat.skeleton.ScaleX = RatauScale;
		yield return new WaitForSeconds(2.25f);
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(rat.gameObject, "Conversation_NPC/Fox/SeeRatau/1"));
		list[0].CharacterName = "NAMES/Ratau";
		list[0].Offset = new Vector3(0f, 2f, 0f);
		list[0].soundPath = "event:/dialogue/ratau/standard_ratau";
		list[0].Animation = "";
		list[0].SkeletonData = rat;
		MMConversation.Play(new ConversationObject(list, null, null), false, true, false, true, true, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return null;
		AudioManager.Instance.PlayOneShot("event:/dialogue/ratau/ratau_sacrificed", rat.gameObject);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "ratau-sacrifice/start", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "ratau-sacrifice/loop", true, 0f);
		Debug.Log("a PlayerFarming.Instance.Spine.AnimationState: " + PlayerFarming.Instance.Spine.AnimationState);
		Spine.AnimationState.SetAnimation(0, "ratau-sacrifice/start", false);
		Spine.AnimationState.AddAnimation(0, "ratau-sacrifice/loop", true, 0f);
		yield return new WaitForSeconds(1.5666667f);
		rat.transform.DOMove(RatauEndPosition, 3f);
		AudioManager.Instance.PlayOneShot("event:/player/float_follower", rat.gameObject);
		yield return new WaitForSeconds(3f);
		FadeRedIn();
		GameManager.GetInstance().AddToCamera(Spine.gameObject);
		GameManager.GetInstance().CameraSetTargetZoom(5f);
		CameraManager.instance.ShakeCameraForDuration(0.7f, 1.2f, 1.5f);
		AudioManager.Instance.PlayOneShot("event:/small_portal/close", PlayerFarming.Instance.transform.position);
		rat.AnimationState.SetAnimation(0, "ratau-sacrifice/end", false);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "ratau-sacrifice/end", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		Spine.AnimationState.SetAnimation(0, "ratau-sacrifice/end", false);
		Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		yield return new WaitForSeconds(2.3333333f);
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.3f);
		FadeRedAway();
		yield return new WaitForSeconds(2f);
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawWireSphere(RatauPosition, 0.5f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(RatauEndPosition, 0.5f);
	}

	public void FadeRedAway()
	{
		LightingManager.Instance.inOverride = false;
		LightingManager.Instance.overrideSettings = null;
		LightingManager.Instance.transitionDurationMultiplier = 1f;
		LightingManager.Instance.lerpActive = false;
		LightingManager.Instance.UpdateLighting(true);
	}

	private void FadeRedIn()
	{
		LightingManager.Instance.inOverride = true;
		LightingSettings.overrideLightingProperties = overrideLightingProperties;
		LightingManager.Instance.overrideSettings = LightingSettings;
		LightingManager.Instance.transitionDurationMultiplier = 0f;
		LightingManager.Instance.UpdateLighting(true);
	}

	public void ShowGoop()
	{
		if (!goopFloorParticle.gameObject.activeSelf)
		{
			goopFloorParticle.gameObject.SetActive(true);
			goopFloorParticle.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "leader-start", false);
			goopFloorParticle.GetComponent<SkeletonAnimation>().AnimationState.AddAnimation(0, "leader-loop", true, 0f);
			AudioManager.Instance.PlayOneShot("event:/enemy/summoned", base.transform.position);
			goopFloorParticle.GetComponent<SimpleSpineDeactivateAfterPlay>().enabled = false;
		}
		else
		{
			StartCoroutine(HideGoopDelayIE());
		}
	}

	private IEnumerator HideGoopDelayIE()
	{
		yield return new WaitForSeconds(1f);
		goopFloorParticle.GetComponent<SimpleSpineDeactivateAfterPlay>().enabled = true;
	}

	private UIItemSelectorOverlayController ShowItemSelector(List<InventoryItem.ITEM_TYPE> items, string key)
	{
		UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(items, new ItemSelector.Params
		{
			Key = key,
			Context = ItemSelector.Context.Give,
			Offset = new Vector2(0f, 200f),
			HideOnSelection = true,
			PreventCancellation = true
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
		uIItemSelectorOverlayController.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController.OnHidden, (Action)delegate
		{
			itemSelector = null;
		});
		return itemSelector;
	}
}
