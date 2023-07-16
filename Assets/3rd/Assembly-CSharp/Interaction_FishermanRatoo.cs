using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using MMTools;
using UnityEngine;

public class Interaction_FishermanRatoo : Interaction
{
	public Interaction_SimpleConversation IntroConversation;

	public Interaction_SimpleConversation FishGotAwayConversation;

	public Interaction_SimpleConversation FishGotAwayConversation2;

	public Interaction_SimpleConversation FishGotAwayConversation3;

	public Interaction_SimpleConversation QuestCompleteConversation;

	public GameObject TraderInteraction;

	private int FishGotAwayCount;

	public Interaction_SimpleConversation CaughtFirstFishConversation;

	public Interaction_Fishing FishingSpot;

	private string sLabel;

	private Coroutine acceptFishRoutine;

	private bool Waiting;

	public Interaction_KeyPiece KeyPiecePrefab;

	private int Progress
	{
		get
		{
			return DataManager.Instance.RatooFishingProgress;
		}
		set
		{
			DataManager.Instance.RatooFishingProgress = value;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.Give;
	}

	private void Start()
	{
		base.HasChanged = true;
		Interaction_Fishing fishingSpot = FishingSpot;
		fishingSpot.OnCatchFish = (Action)Delegate.Remove(fishingSpot.OnCatchFish, new Action(CaughtFish));
		Interaction_Fishing fishingSpot2 = FishingSpot;
		fishingSpot2.OnFishEscaped = (Action)Delegate.Remove(fishingSpot2.OnFishEscaped, new Action(FishEscaped));
		StopAllCoroutines();
		if (GetRemainingFishCount() <= 0)
		{
			Progress = 3;
		}
		switch (Progress)
		{
		case 0:
			IntroConversation.gameObject.SetActive(true);
			IntroConversation.Callback.AddListener(delegate
			{
				GiveObjective();
				int progress = Progress + 1;
				Progress = progress;
				Start();
			});
			break;
		case 1:
		{
			FishGotAwayCount = 0;
			Interaction_Fishing fishingSpot3 = FishingSpot;
			fishingSpot3.OnCatchFish = (Action)Delegate.Combine(fishingSpot3.OnCatchFish, new Action(CaughtFish));
			Interaction_Fishing fishingSpot4 = FishingSpot;
			fishingSpot4.OnFishEscaped = (Action)Delegate.Combine(fishingSpot4.OnFishEscaped, new Action(FishEscaped));
			break;
		}
		case 3:
			TraderInteraction.SetActive(true);
			break;
		}
		UpdateLocalisation();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		acceptFishRoutine = null;
	}

	public override void GetLabel()
	{
		if (Progress == 2)
		{
			base.Label = sLabel;
		}
		else
		{
			base.Label = "";
		}
	}

	public void GiveObjective()
	{
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/CatchFish", Objectives.CustomQuestTypes.CatchFish), true);
	}

	private void CompleteObjective()
	{
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CatchFish);
	}

	private void CaughtFish()
	{
		CaughtFirstFishConversation.CallOnConversationEnd = false;
		CaughtFirstFishConversation.gameObject.SetActive(true);
		CaughtFirstFishConversation.Callback.AddListener(delegate
		{
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
			CompleteObjective();
			int progress = Progress + 1;
			Progress = progress;
			Start();
			StartCoroutine(CaughtFishGiveTarot());
		});
	}

	private IEnumerator CaughtFishGiveTarot()
	{
		yield return null;
		TarotCustomTarget.Create(base.transform.position + Vector3.back, PlayerFarming.Instance.transform.position, 1f, TarotCards.Card.NeptunesCurse, delegate
		{
			GameManager.GetInstance().OnConversationEnd();
			StartCoroutine(IGiveAllFishObjects());
		});
	}

	private IEnumerator IGiveAllFishObjects()
	{
		yield return new WaitForSeconds(0.5f);
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/FishingQuest", Objectives.CustomQuestTypes.CatchSquid));
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/FishingQuest", Objectives.CustomQuestTypes.CatchOctopus));
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/FishingQuest", Objectives.CustomQuestTypes.CatchLobster));
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/FishingQuest", Objectives.CustomQuestTypes.CatchCrab));
	}

	private void FishEscaped()
	{
		Interaction_SimpleConversation interaction_SimpleConversation = null;
		switch (++FishGotAwayCount)
		{
		case 1:
			interaction_SimpleConversation = FishGotAwayConversation;
			break;
		case 2:
			interaction_SimpleConversation = FishGotAwayConversation2;
			break;
		case 3:
			interaction_SimpleConversation = FishGotAwayConversation3;
			break;
		default:
		{
			Interaction_Fishing fishingSpot = FishingSpot;
			fishingSpot.OnFishEscaped = (Action)Delegate.Remove(fishingSpot.OnFishEscaped, new Action(FishEscaped));
			return;
		}
		}
		interaction_SimpleConversation.gameObject.SetActive(true);
		interaction_SimpleConversation.Callback.AddListener(delegate
		{
		});
	}

	private List<InventoryItem.ITEM_TYPE> GetRemainingFishList()
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		if (!DataManager.Instance.RatooFishing_FISH_CRAB)
		{
			list.Add(InventoryItem.ITEM_TYPE.FISH_CRAB);
		}
		if (!DataManager.Instance.RatooFishing_FISH_LOBSTER)
		{
			list.Add(InventoryItem.ITEM_TYPE.FISH_LOBSTER);
		}
		if (!DataManager.Instance.RatooFishing_FISH_OCTOPUS)
		{
			list.Add(InventoryItem.ITEM_TYPE.FISH_OCTOPUS);
		}
		if (!DataManager.Instance.RatooFishing_FISH_SQUID)
		{
			list.Add(InventoryItem.ITEM_TYPE.FISH_SQUID);
		}
		return list;
	}

	private int GetRemainingFishCount()
	{
		int num = 0;
		if (!DataManager.Instance.RatooFishing_FISH_CRAB)
		{
			num++;
		}
		if (!DataManager.Instance.RatooFishing_FISH_LOBSTER)
		{
			num++;
		}
		if (!DataManager.Instance.RatooFishing_FISH_OCTOPUS)
		{
			num++;
		}
		if (!DataManager.Instance.RatooFishing_FISH_SQUID)
		{
			num++;
		}
		return num;
	}

	public override void OnInteract(StateMachine state)
	{
		Debug.Log("Progress: " + Progress);
		if (Progress == 2)
		{
			base.OnInteract(state);
			Interactable = false;
			state.CURRENT_STATE = StateMachine.State.InActive;
			state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
			HUD_Manager.Instance.Hide(false, 0);
			CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
			cameraFollowTarget.SetOffset(new Vector3(0f, 4.5f, 2f));
			cameraFollowTarget.AddTarget(base.gameObject, 1f);
			List<InventoryItem.ITEM_TYPE> remainingFishList = GetRemainingFishList();
			UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(remainingFishList, new ItemSelector.Params
			{
				Key = "fisherman" + remainingFishList.Count,
				Context = ItemSelector.Context.Give,
				Offset = new Vector2(0f, 100f),
				HideOnSelection = true,
				ShowEmpty = true
			});
			UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
			uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
			{
				StopAllCoroutines();
				StartCoroutine(GiveItem(chosenItem));
			});
			UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
			uIItemSelectorOverlayController2.OnCancel = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnCancel, (Action)delegate
			{
				HUD_Manager.Instance.Show(0);
			});
			UIItemSelectorOverlayController uIItemSelectorOverlayController3 = itemSelector;
			uIItemSelectorOverlayController3.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController3.OnHidden, (Action)delegate
			{
				cameraFollowTarget.SetOffset(Vector3.zero);
				cameraFollowTarget.RemoveTarget(base.gameObject);
				state.CURRENT_STATE = StateMachine.State.Idle;
				itemSelector = null;
				Interactable = true;
				base.HasChanged = true;
			});
		}
	}

	private IEnumerator GiveItem(InventoryItem.ITEM_TYPE toGive)
	{
		yield return null;
		PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(-2.5f, 0.8f), base.gameObject);
		if (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
		GameManager.GetInstance().AddToCamera(base.gameObject);
		yield return new WaitForSeconds(1f);
		Waiting = true;
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
		ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, toGive, delegate
		{
			Waiting = false;
		});
		while (Waiting)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", base.gameObject);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		Waiting = true;
		GetConversation(toGive);
		while (Waiting)
		{
			yield return null;
		}
		yield return StartCoroutine(GiveKeyPieceRoutine());
		while (Interaction_KeyPiece.Instance != null)
		{
			yield return null;
		}
		Inventory.ChangeItemQuantity((int)toGive, -1);
		switch (toGive)
		{
		case InventoryItem.ITEM_TYPE.FISH_LOBSTER:
			DataManager.Instance.RatooFishing_FISH_LOBSTER = true;
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CatchLobster);
			break;
		case InventoryItem.ITEM_TYPE.FISH_CRAB:
			DataManager.Instance.RatooFishing_FISH_CRAB = true;
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CatchCrab);
			break;
		case InventoryItem.ITEM_TYPE.FISH_SQUID:
			DataManager.Instance.RatooFishing_FISH_SQUID = true;
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CatchSquid);
			break;
		case InventoryItem.ITEM_TYPE.FISH_OCTOPUS:
			DataManager.Instance.RatooFishing_FISH_OCTOPUS = true;
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CatchOctopus);
			break;
		}
		Debug.Log("GetRemainingFishCount(): " + GetRemainingFishCount());
		if (GetRemainingFishCount() <= 0)
		{
			Progress++;
			QuestCompleteConversation.gameObject.SetActive(true);
			QuestCompleteConversation.Callback.AddListener(delegate
			{
				Start();
			});
		}
		else
		{
			acceptFishRoutine = null;
			Start();
		}
	}

	private IEnumerator GiveKeyPieceRoutine()
	{
		yield return null;
		Interaction_KeyPiece KeyPiece = UnityEngine.Object.Instantiate(KeyPiecePrefab, base.transform.position + Vector3.back * 0.75f, Quaternion.identity, base.transform.parent);
		KeyPiece.transform.localScale = Vector3.zero;
		KeyPiece.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
		GameManager.GetInstance().OnConversationNext(KeyPiece.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(1f);
		KeyPiece.transform.DOMove(PlayerFarming.Instance.transform.position + Vector3.back * 0.5f, 1f).SetEase(Ease.InBack);
		yield return new WaitForSeconds(1f);
		yield return null;
		GameManager.GetInstance().OnConversationEnd(false);
		KeyPiece.OnInteract(PlayerFarming.Instance.state);
	}

	private void GetConversation(InventoryItem.ITEM_TYPE itemType)
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		int num = -1;
		string text = "Conversation_NPC/Fisherman/Caught_" + itemType.ToString() + "/";
		while (LocalizationManager.GetTermData(text + ++num) != null)
		{
			ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, text + num);
			conversationEntry.CharacterName = ScriptLocalization.NAMES.Fisherman;
			conversationEntry.soundPath = "event:/dialogue/hallowed_shores/fisherman/standard_fisherman";
			list.Add(conversationEntry);
		}
		MMConversation.Play(new ConversationObject(list, null, delegate
		{
			Waiting = false;
		}), false, true, false, true, true, true);
	}
}
