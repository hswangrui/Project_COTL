using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_Plimbo : Interaction
{
	public float InteractionDistance = 2.5f;

	public GameObject Shrine;

	public Interaction_SimpleConversation IntroConversation;

	public Interaction_SimpleConversation EndConversation;

	public Interaction_SimpleConversation Layer2Conversation;

	public Interaction_SimpleConversation Layer2EndConversation;

	private string sLabel;

	private bool ShowingResource;

	private bool Waiting;

	public Interaction_KeyPiece KeyPiecePrefab;

	private int StoryProgress
	{
		get
		{
			return DataManager.Instance.PlimboStoryProgress;
		}
		set
		{
			DataManager.Instance.PlimboStoryProgress = value;
		}
	}

	private void Start()
	{
		ActivateDistance = InteractionDistance;
		base.HasChanged = true;
		Shrine.SetActive(false);
		if (GameManager.Layer2 && StoryProgress == 5)
		{
			StoryProgress = 6;
		}
		switch (StoryProgress)
		{
		case 0:
			IntroConversation.gameObject.SetActive(true);
			IntroConversation.Callback.AddListener(delegate
			{
				StoryProgress = 1;
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitPlimbo", Objectives.CustomQuestTypes.PlimboEye1));
				Start();
			});
			break;
		case 6:
			Layer2Conversation.gameObject.SetActive(true);
			Layer2Conversation.Callback.AddListener(delegate
			{
				StoryProgress = 7;
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitPlimbo", Objectives.CustomQuestTypes.PlimboEye1));
				Start();
			});
			break;
		case 5:
			Shrine.SetActive(true);
			break;
		default:
			StartCoroutine(AcceptBeholderEye());
			break;
		}
		if (StoryProgress < 5)
		{
			Shrine.SetActive(false);
		}
		UpdateLocalisation();
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(101) > 0)
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = string.Join(" ", string.Format(ScriptLocalization.UI_ItemSelector_Context.Give, ScriptLocalization.Inventory.BEHOLDER_EYE), CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BEHOLDER_EYE, 1));
	}

	public override void GetLabel()
	{
		if ((StoryProgress > 0 && StoryProgress < 5) || (StoryProgress > 6 && StoryProgress < 11))
		{
			base.Label = sLabel;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (Inventory.GetItemQuantity(101) <= 0)
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
			return;
		}
		base.OnInteract(state);
		StopAllCoroutines();
		StartCoroutine(GiveBeholderEye());
	}

	private IEnumerator AcceptBeholderEye()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		while (InputManager.Gameplay.GetInteractButtonHeld())
		{
			yield return null;
		}
	}

	private IEnumerator GiveBeholderEye()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PlimboEye1);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PlimboEye2);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PlimboEye3);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PlimboEye4);
		PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(-0.5f, -2f), base.gameObject);
		if (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		ShowingResource = false;
		Inventory.ChangeItemQuantity(101, -1);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
		GameManager.GetInstance().AddToCamera(base.gameObject);
		yield return new WaitForSeconds(1f);
		Waiting = true;
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
		ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BEHOLDER_EYE, delegate
		{
			Waiting = false;
		});
		while (Waiting)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		Waiting = true;
		GetConversation();
		while (Waiting)
		{
			yield return null;
		}
		yield return StartCoroutine(GiveKeyPieceRoutine());
		while (Interaction_KeyPiece.Instance != null)
		{
			yield return null;
		}
		int storyProgress = StoryProgress + 1;
		StoryProgress = storyProgress;
		if (StoryProgress == 5)
		{
			EndConversation.gameObject.SetActive(true);
		}
		else if (StoryProgress == 11)
		{
			Layer2EndConversation.gameObject.SetActive(true);
		}
		else
		{
			Start();
		}
		yield return new WaitForSeconds(2f);
		switch (StoryProgress)
		{
		case 2:
		case 8:
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitPlimbo", Objectives.CustomQuestTypes.PlimboEye2));
			break;
		case 3:
		case 9:
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitPlimbo", Objectives.CustomQuestTypes.PlimboEye3));
			break;
		case 4:
		case 10:
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitPlimbo", Objectives.CustomQuestTypes.PlimboEye4));
			break;
		}
	}

	private void TestComplete()
	{
		StoryProgress = 5;
		EndConversation.gameObject.SetActive(true);
	}

	private void GetConversation()
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		int num = -1;
		string text = "Conversation_NPC/DecorationShop/BeholderEye" + StoryProgress + "/";
		while (LocalizationManager.GetTermData(text + ++num) != null)
		{
			ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, text + num);
			conversationEntry.CharacterName = ScriptLocalization.NAMES.DecorationShopSeller;
			conversationEntry.soundPath = "event:/dialogue/plimbo/standard_plimbo";
			conversationEntry.Animation = "talk";
			if (StoryProgress == 1)
			{
				switch (num)
				{
				case 0:
					conversationEntry.Animation = "talk-wink";
					break;
				case 1:
					conversationEntry.Animation = "talk";
					break;
				case 2:
					conversationEntry.Animation = "talk-laugh";
					break;
				default:
					conversationEntry.Animation = "talk";
					break;
				}
			}
			else if (StoryProgress == 2)
			{
				switch (num)
				{
				case 0:
					conversationEntry.Animation = "talk-excited";
					break;
				case 1:
					conversationEntry.Animation = "talk";
					break;
				case 2:
					conversationEntry.Animation = "talk";
					break;
				default:
					conversationEntry.Animation = "talk-wink";
					break;
				}
			}
			else if (StoryProgress == 3)
			{
				if (num == 0)
				{
					conversationEntry.Animation = "talk-excited";
				}
				else
				{
					conversationEntry.Animation = "talk";
				}
			}
			else if (StoryProgress == 4)
			{
				if (num == 0)
				{
					conversationEntry.Animation = "talk-excited";
				}
				else
				{
					conversationEntry.Animation = "talk-wink";
				}
			}
			list.Add(conversationEntry);
		}
		MMConversation.Play(new ConversationObject(list, null, delegate
		{
			Waiting = false;
		}), false, true, false);
	}

	private IEnumerator GiveKeyPieceRoutine()
	{
		yield return null;
		Interaction_KeyPiece KeyPiece = Object.Instantiate(KeyPiecePrefab, base.transform.position + Vector3.back * 1.2f, Quaternion.identity, base.transform.parent);
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

	public void RevealShrine()
	{
		StartCoroutine(RevealShrineIE());
	}

	private IEnumerator RevealShrineIE()
	{
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(Shrine.gameObject);
		Shrine.gameObject.SetActive(true);
		Interaction_RatauShrine r = Shrine.GetComponentInChildren<Interaction_RatauShrine>();
		Shrine.transform.localPosition = Vector3.forward;
		Shrine.transform.DOLocalMove(Vector3.zero, 2f).SetEase(Ease.OutBack).OnComplete(delegate
		{
			Vector3 localScale = r.transform.localScale;
			r.transform.localScale = Vector3.zero;
			r.transform.DOScale(localScale, 0.5f).SetEase(Ease.OutBack);
			CultFaithManager.AddThought(Thought.Cult_PledgedToYou, -1, 1f);
		});
		yield return new WaitForSeconds(3f);
		GameManager.GetInstance().OnConversationEnd();
	}
}
