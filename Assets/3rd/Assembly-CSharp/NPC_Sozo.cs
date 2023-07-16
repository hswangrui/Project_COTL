using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.Menus.DoctrineChoicesMenu;
using MMTools;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

public class NPC_Sozo : Interaction
{
	[SerializeField]
	private GameObject Shrine;

	[Space]
	[SerializeField]
	private Interaction_SimpleConversation convo0;

	[SerializeField]
	private Interaction_SimpleConversation convo1;

	[SerializeField]
	private Interaction_SimpleConversation convo2;

	[SerializeField]
	private Interaction_SimpleConversation convo3;

	[SerializeField]
	private Interaction_SimpleConversation convo4;

	[SerializeField]
	private Interaction_SimpleConversation convo5;

	[SerializeField]
	private Interaction_SimpleConversation convo6;

	[SerializeField]
	private Interaction_SimpleConversation convo7;

	[SerializeField]
	private Interaction_SimpleConversation convo8;

	[SerializeField]
	private Interaction_SimpleConversation convo9;

	[SerializeField]
	private Interaction_SimpleConversation convo10;

	[SerializeField]
	private Interaction_SimpleConversation convoBeforeDeath;

	[SerializeField]
	private SimpleBarkRepeating buyingShroomsBark;

	[Space]
	[SerializeField]
	private SkeletonAnimation SozoSpine;

	[SerializeField]
	private GameObject deadParticles;

	[Space]
	[SerializeField]
	private SkeletonAnimation worshipper1;

	[SerializeField]
	private SkeletonAnimation worshipper2;

	[SerializeField]
	private SkeletonAnimation worshipper3;

	[SerializeField]
	private SkeletonAnimation worshipper4;

	[Space]
	[SerializeField]
	private GameObject pos1;

	[SerializeField]
	private GameObject pos2;

	[SerializeField]
	private GameObject pos3;

	[SerializeField]
	private GameObject pos4;

	[SerializeField]
	private GameObject mushrooms;

	[Space]
	[SerializeField]
	private SimpleSetCamera simpleSetCamera;

	private string sLabel;

	private Coroutine acceptRoutine;

	private const int pricePerMushroom = 5;

	private const int requiredMushrooms1 = 10;

	private const int requiredMushrooms2 = 20;

	public GameObject uINewCard;

	private bool Activated;

	public Interaction_KeyPiece KeyPiecePrefab;

	private int StoryProgress
	{
		get
		{
			return DataManager.Instance.SozoStoryProgress;
		}
		set
		{
			DataManager.Instance.SozoStoryProgress = value;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Start();
		AudioManager.Instance.SetMusicRoomID(1, "drum_layer");
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		AudioManager.Instance.SetMusicRoomID(0, "drum_layer");
	}

	private void Start()
	{
		Shrine.SetActive(false);
		convo0.gameObject.SetActive(false);
		convo1.gameObject.SetActive(false);
		convo2.gameObject.SetActive(false);
		convo3.gameObject.SetActive(false);
		convo4.gameObject.SetActive(false);
		convo5.gameObject.SetActive(false);
		convo6.gameObject.SetActive(false);
		convo7.gameObject.SetActive(false);
		convo8.gameObject.SetActive(false);
		convo9.gameObject.SetActive(false);
		convo10.gameObject.SetActive(false);
		convoBeforeDeath.gameObject.SetActive(false);
		buyingShroomsBark.gameObject.SetActive(false);
		switch (StoryProgress)
		{
		case -1:
		case 0:
			convo0.gameObject.SetActive(true);
			break;
		case 5:
			Shrine.SetActive(true);
			SozoSpine.AnimationState.SetAnimation(0, "tripping-balls", true);
			break;
		}
		if (DataManager.Instance.SozoDead)
		{
			SozoSpine.AnimationState.SetAnimation(0, "dead", true);
			deadParticles.SetActive(true);
			Interactable = false;
		}
		UpdateLocalisation();
	}

	public override void OnInteract(StateMachine state)
	{
		if (StoryProgress == 1)
		{
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL) >= 10)
			{
				StartCoroutine(GiveMushrooms(10));
			}
			else
			{
				StartCoroutine(NeedMoreMushroomsIE());
			}
		}
		else if (StoryProgress == 2)
		{
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL) >= 20)
			{
				StartCoroutine(GiveMushrooms(20));
			}
			else
			{
				StartCoroutine(NeedMoreMushroomsIE());
			}
		}
		else if (StoryProgress == 3)
		{
			if (DataManager.Instance.PerformedMushroomRitual)
			{
				StartCoroutine(GiveDecorationRoutine());
			}
		}
		else if (StoryProgress == 4)
		{
			if (DataManager.Instance.BuiltMushroomDecoration)
			{
				StartCoroutine(EndQuestRoutine());
			}
		}
		else if (StoryProgress >= 5)
		{
			convoBeforeDeath.Play();
		}
		base.OnInteract(state);
	}

	private void TestDecoration()
	{
		if (!DataManager.Instance.PerformedMushroomRitual)
		{
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitSozo", Objectives.CustomQuestTypes.SozoReturn));
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SozoPerformRitual);
		}
		DataManager.Instance.PerformedMushroomRitual = true;
		StoryProgress = 3;
	}

	private IEnumerator GiveDecorationRoutine()
	{
		StoryProgress = 4;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SozoReturn);
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(2f, 0f), base.gameObject);
		if (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		convo6.gameObject.SetActive(true);
		convo6.CallOnConversationEnd = false;
		convo6.Callback.AddListener(delegate
		{
			GiveKeyPiece(delegate
			{
				convo6.gameObject.SetActive(false);
				convo7.gameObject.SetActive(true);
				convo7.Play();
				convo7.CallOnConversationEnd = false;
				convo7.Callback.AddListener(delegate
				{
					DataManager.Instance.SozoDecorationQuestActive = true;
					StructureBrain.TYPES DecorationType = StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE;
					StructuresData.CompleteResearch(DecorationType);
					StructuresData.SetRevealed(DecorationType);
					UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
					uINewItemOverlayController.pickedBuilding = DecorationType;
					uINewItemOverlayController.Show(UINewItemOverlayController.TypeOfCard.Decoration, base.transform.position, false);
					uINewItemOverlayController.OnHidden = (Action)Delegate.Combine(uINewItemOverlayController.OnHidden, (Action)delegate
					{
						convo8.gameObject.SetActive(true);
						convo8.Play();
						convo8.CallOnConversationEnd = true;
						convo8.Callback.AddListener(delegate
						{
							ObjectiveManager.Add(new Objectives_BuildStructure("Objectives/GroupTitles/VisitSozo", DecorationType));
						});
					});
				});
			});
		});
		convo6.Play();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (DataManager.Instance.SozoBeforeDeath)
		{
			DataManager.Instance.SozoDead = true;
		}
	}

	private void TestEndQuest()
	{
		convo0.gameObject.SetActive(false);
		if (!DataManager.Instance.BuiltMushroomDecoration)
		{
			DataManager.Instance.BuiltMushroomDecoration = true;
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitSozo", Objectives.CustomQuestTypes.SozoReturn));
		}
		StoryProgress = 4;
	}

	private IEnumerator EndQuestRoutine()
	{
		Activated = true;
		StoryProgress = 5;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SozoReturn);
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(2f, 0f), base.gameObject);
		if (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		convo9.gameObject.SetActive(true);
		convo9.CallOnConversationEnd = false;
		convo9.Play();
		convo9.Callback.AddListener(delegate
		{
			GiveKeyPiece(delegate
			{
				convo10.gameObject.SetActive(true);
				convo10.CallOnConversationEnd = true;
				convo10.Play();
				convo10.Callback.AddListener(delegate
				{
					SozoSpine.AnimationState.SetAnimation(0, "talk-mushroom", true);
					RevealShrine();
				});
			});
		});
	}

	public void RevealShrine()
	{
		Shrine.gameObject.SetActive(true);
		Interaction_RatauShrine r = Shrine.GetComponentInChildren<Interaction_RatauShrine>();
		Vector3 localPosition = Shrine.transform.localPosition;
		Shrine.transform.localPosition = localPosition + Vector3.forward;
		Shrine.transform.DOLocalMove(localPosition, 2f).SetEase(Ease.OutBack).OnComplete(delegate
		{
			Vector3 localScale = r.transform.localScale;
			r.transform.localScale = Vector3.zero;
			r.transform.DOScale(localScale, 0.5f).SetEase(Ease.OutBack);
			CultFaithManager.AddThought(Thought.Cult_PledgedToYou, -1, 1f);
		});
	}

	public override void GetLabel()
	{
		AutomaticallyInteract = false;
		if (Activated)
		{
			base.Label = "";
		}
		else if (StoryProgress > 0)
		{
			if (StoryProgress < 3)
			{
				base.Label = ScriptLocalization.Interactions.Give + ": " + InventoryItem.CapacityString(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, (StoryProgress == 1) ? 10 : 20);
			}
			else if (StoryProgress == 3 && DataManager.Instance.PerformedMushroomRitual)
			{
				AutomaticallyInteract = true;
				base.Label = ScriptLocalization.Interactions.Talk;
			}
			else if (StoryProgress == 4 && DataManager.Instance.BuiltMushroomDecoration)
			{
				AutomaticallyInteract = true;
				base.Label = ScriptLocalization.Interactions.Talk;
			}
			else if (StoryProgress == 5 && !DataManager.Instance.SozoDead)
			{
				AutomaticallyInteract = true;
				base.Label = ScriptLocalization.Interactions.Talk;
			}
			else
			{
				base.Label = "";
			}
		}
		else
		{
			base.Label = "";
		}
	}

	public void IntroConvoComplete()
	{
		StoryProgress = 1;
	}

	public void PlayerHasDoneRitual()
	{
		StoryProgress = 4;
		GiveKeyPiece(null);
	}

	private IEnumerator NeedMoreMushroomsIE()
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, "Conversation_NPC/Sozo/StillNotEnoughShrooms/0")
		{
			CharacterName = ScriptLocalization.NAMES.Sozo,
			soundPath = "event:/dialogue/sozo/sozo_evil",
			Animation = "talk-welcome",
			DefaultAnimation = "animation"
		};
		list.Add(conversationEntry);
		conversationEntry = ConversationEntry.Clone(conversationEntry);
		conversationEntry.TermToSpeak = "Conversation_NPC/Sozo/StillNotEnoughShrooms/1";
		conversationEntry.Animation = "talk-laugh";
		conversationEntry.soundPath = "event:/dialogue/sozo/sozo_evil";
		list.Add(conversationEntry);
		conversationEntry = ConversationEntry.Clone(conversationEntry);
		conversationEntry.TermToSpeak = "Conversation_NPC/Sozo/StillNotEnoughShrooms/2";
		conversationEntry.Animation = "talk";
		conversationEntry.soundPath = "event:/dialogue/sozo/sozo_standard";
		list.Add(conversationEntry);
		MMConversation.Play(new ConversationObject(list, null, null), false);
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
	}

	private IEnumerator SellMushrooms()
	{
		bool activiating = true;
		bool firstPress = true;
		float delay = 0f;
		int count = 0;
		while (activiating)
		{
			if (delay <= 0f)
			{
				if (Inventory.GetItemQuantity(29) > 0)
				{
					AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
					ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, null);
					Inventory.ChangeItemQuantity(29, -1);
					count++;
				}
				else
				{
					activiating = false;
				}
				delay = (firstPress ? 0.5f : 0.1f);
				firstPress = false;
			}
			delay -= Time.deltaTime;
			if (InputManager.Gameplay.GetInteractButtonUp())
			{
				activiating = false;
			}
			else
			{
				yield return null;
			}
		}
		int amount = Mathf.Min(5, count);
		for (int i = 0; i < amount; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, base.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			yield return new WaitForSeconds(0.05f);
		}
		Inventory.AddItem(20, count);
	}

	private IEnumerator GiveMushrooms(int requiredAmount)
	{
		if (acceptRoutine != null)
		{
			StopCoroutine(acceptRoutine);
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BringSozoMushrooms);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BringSozoMushrooms2);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SozoReturn);
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(2f, 0f), base.gameObject);
		if (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		Inventory.ChangeItemQuantity(29, -requiredAmount);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
		GameManager.GetInstance().AddToCamera(base.gameObject);
		yield return new WaitForSeconds(1f);
		for (int i = 0; i < requiredAmount; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, null);
			yield return new WaitForSeconds(0.2f - 0.2f * (float)(i / requiredAmount));
		}
		yield return new WaitForSeconds(1f);
		StoryProgress++;
		if (StoryProgress == 2)
		{
			convo2.gameObject.SetActive(true);
			convo2.CallOnConversationEnd = false;
			convo2.Callback.AddListener(delegate
			{
				Debug.Log("CALL BACK 2!");
				GiveKeyPiece(delegate
				{
					GameManager.GetInstance().StartCoroutine(DelayGiveObjective(Objectives.CustomQuestTypes.BringSozoMushrooms2, 2));
				});
			});
			convo2.Play();
		}
		else if (StoryProgress == 3)
		{
			convo3.gameObject.SetActive(true);
			convo3.Play();
		}
	}

	public void Ritual()
	{
		StartCoroutine(RitualIE());
	}

	private IEnumerator RitualIE()
	{
		convo3.gameObject.SetActive(false);
		yield return new WaitForEndOfFrame();
		worshipper1.gameObject.SetActive(true);
		worshipper2.gameObject.SetActive(true);
		worshipper3.gameObject.SetActive(true);
		worshipper4.gameObject.SetActive(true);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 12f);
		worshipper1.transform.DOMove(pos1.transform.position, 5f);
		worshipper2.transform.DOMove(pos2.transform.position, 5f);
		worshipper3.transform.DOMove(pos3.transform.position, 5f);
		worshipper4.transform.DOMove(pos4.transform.position, 5f);
		yield return new WaitForSeconds(2.5f);
		simpleSetCamera.Play();
		yield return new WaitForSeconds(2.5f);
		mushrooms.SetActive(true);
		mushrooms.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f);
		SozoSpine.AnimationState.SetAnimation(0, "talk-yes", true);
		yield return new WaitForSeconds(1f);
		worshipper1.AnimationState.SetAnimation(0, "ritual", true);
		worshipper2.AnimationState.SetAnimation(0, "ritual", true);
		worshipper3.AnimationState.SetAnimation(0, "ritual", true);
		worshipper4.AnimationState.SetAnimation(0, "ritual", true);
		float num = 0f;
		StartCoroutine(GiveShrooms(worshipper1.gameObject, 5f, num));
		num += 0.1f;
		StartCoroutine(GiveShrooms(worshipper2.gameObject, 5f, num));
		num += 0.1f;
		StartCoroutine(GiveShrooms(worshipper3.gameObject, 5f, num));
		num += 0.1f;
		StartCoroutine(GiveShrooms(worshipper4.gameObject, 5f, num));
		BiomeConstants.Instance.PsychedelicFadeIn(5f);
		AudioManager.Instance.SetMusicPsychedelic(1f);
		yield return new WaitForSeconds(5f);
		mushrooms.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(delegate
		{
			mushrooms.SetActive(false);
			mushrooms.transform.localScale = Vector3.one;
		});
		yield return new WaitForSeconds(1f);
		MMConversation.CURRENT_CONVERSATION = new ConversationObject(null, null, null, new List<DoctrineResponse>
		{
			new DoctrineResponse(SermonCategory.Special, 4, true, null)
		});
		UIDoctrineChoicesMenuController doctrineChoicesInstance = MonoSingleton<UIManager>.Instance.DoctrineChoicesMenuTemplate.Instantiate();
		doctrineChoicesInstance.Show();
		while (doctrineChoicesInstance.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Brainwashing);
		worshipper1.AnimationState.SetAnimation(0, "worship", true);
		worshipper2.AnimationState.SetAnimation(0, "worship", true);
		worshipper3.AnimationState.SetAnimation(0, "worship", true);
		worshipper4.AnimationState.SetAnimation(0, "worship", true);
		worshipper1.transform.DOMove(pos1.transform.position + Vector3.down * 7f, 2.5f);
		worshipper2.transform.DOMove(pos2.transform.position + Vector3.down * 7f, 2.5f);
		worshipper3.transform.DOMove(pos3.transform.position + Vector3.down * 7f, 2.5f);
		worshipper4.transform.DOMove(pos4.transform.position + Vector3.down * 7f, 2.5f);
		BiomeConstants.Instance.PsychedelicFadeOut(1.5f);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.SetMusicPsychedelic(0f);
		SozoSpine.AnimationState.SetAnimation(0, "animation", true);
		simpleSetCamera.Reset();
		yield return new WaitForSeconds(1.5f);
		worshipper1.gameObject.SetActive(false);
		worshipper2.gameObject.SetActive(false);
		worshipper3.gameObject.SetActive(false);
		worshipper4.gameObject.SetActive(false);
		convo4.gameObject.SetActive(true);
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		convo4.Play();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		GiveKeyPiece(delegate
		{
			GameManager.GetInstance().StartCoroutine(DelayGiveObjective(Objectives.CustomQuestTypes.SozoPerformRitual, 3));
		});
	}

	private IEnumerator DelayGiveObjective(Objectives.CustomQuestTypes Quest, int Story)
	{
		Activated = true;
		yield return new WaitForSeconds(1.5f);
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitSozo", Quest));
		StoryProgress = Story;
		Start();
		Activated = false;
	}

	private IEnumerator GiveShrooms(GameObject follower, float totalTime, float delay)
	{
		yield return new WaitForSeconds(delay);
		int randomCoins = 10;
		float increment = (totalTime - delay) / (float)randomCoins;
		for (int i = 0; i < randomCoins; i++)
		{
			mushrooms.transform.DOPunchScale(Vector3.one * 0.1f, increment - 0.05f);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", mushrooms.transform.position);
			ResourceCustomTarget.Create(follower.gameObject, mushrooms.transform.position + Vector3.forward, InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, null);
			yield return new WaitForSeconds(increment);
		}
	}

	public void GiveKeyPiece(Action Callback)
	{
		StartCoroutine(GiveKeyPieceRoutine(Callback));
	}

	private IEnumerator GiveKeyPieceRoutine(Action Callback)
	{
		yield return null;
		Interaction_KeyPiece KeyPiece = UnityEngine.Object.Instantiate(KeyPiecePrefab, base.transform.position + Vector3.back * 0.5f, Quaternion.identity, base.transform.parent);
		KeyPiece.transform.localScale = Vector3.zero;
		KeyPiece.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(KeyPiece.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		KeyPiece.transform.DOMove(PlayerFarming.Instance.transform.position + Vector3.back * 0.5f, 1f).SetEase(Ease.InBack);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd(false);
		KeyPiece.OnInteract(PlayerFarming.Instance.state);
		KeyPiece.Callback = Callback;
	}
}
