using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class Interaction_DungeonChallenge : Interaction
{
	[SerializeField]
	private SkeletonAnimation Spine;

	[SerializeField]
	private bool giveReward;

	private ObjectivesData completedObjective;

	private string sLabel = "";

	private bool rewardGiven;

	private void Start()
	{
		if (giveReward)
		{
			AutomaticallyInteract = true;
		}
		bool flag = false;
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			if (completedObjective is Objectives_RoomChallenge)
			{
				flag = true;
				this.completedObjective = completedObjective;
				break;
			}
		}
		if (giveReward && !flag)
		{
			base.gameObject.SetActive(false);
		}
		sLabel = ScriptLocalization.Interactions.DungeonChallenge;
		StartCoroutine(SetAnimation());
	}

	private IEnumerator SetAnimation()
	{
		while (Spine == null || Spine.AnimationState == null)
		{
			yield return null;
		}
		Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
	}

	public override void GetLabel()
	{
		base.Label = (Interactable ? sLabel : "");
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		if (giveReward)
		{
			StartCoroutine(GiveRewardIE());
		}
		else
		{
			StartCoroutine(GiveChallengeIE());
		}
	}

	private IEnumerator GiveRewardIE()
	{
		PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.left * 2f);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/Challenge/Reward/Line1"));
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/Challenge/Reward/Line2"));
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/Challenge/Reward/Line3"));
		list[0].CharacterName = "NAMES/Ratoo";
		list[0].Offset = new Vector3(0f, 2f, 0f);
		list[1].CharacterName = "NAMES/Ratoo";
		list[1].Offset = new Vector3(0f, 2f, 0f);
		list[2].CharacterName = "NAMES/Ratoo";
		list[2].Offset = new Vector3(0f, 2f, 0f);
		list[0].soundPath = "event:/dialogue/ratoo/standard_ratoo";
		list[1].soundPath = "event:/dialogue/ratoo/standard_ratoo";
		list[2].soundPath = "event:/dialogue/ratoo/standard_ratoo";
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		MMConversation.Play(new ConversationObject(list, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		GenerateRoom.ConnectionTypes connectionType = BiomeGenerator.Instance.CurrentRoom.N_Room.ConnectionType;
		GenerateRoom.ConnectionTypes connectionType2 = BiomeGenerator.Instance.CurrentRoom.S_Room.ConnectionType;
		GenerateRoom.ConnectionTypes connectionType3 = BiomeGenerator.Instance.CurrentRoom.W_Room.ConnectionType;
		GenerateRoom.ConnectionTypes connectionType4 = BiomeGenerator.Instance.CurrentRoom.E_Room.ConnectionType;
		InventoryItem.ITEM_TYPE iTEM_TYPE = InventoryItem.ITEM_TYPE.BLACK_GOLD;
		while (iTEM_TYPE == InventoryItem.ITEM_TYPE.BLACK_GOLD || iTEM_TYPE == InventoryItem.ITEM_TYPE.GOLD_NUGGET)
		{
			iTEM_TYPE = RewardsItem.Instance.ReturnItemType(RewardsItem.Instance.GetGoodReward(false, true));
		}
		if (!DataManager.TrinketUnlocked(TarotCards.Card.DecreaseRelicCharge) && DataManager.Instance.OnboardedRelics)
		{
			GameManager.GetInstance().OnConversationNew();
			TarotCustomTarget tarotCustomTarget = TarotCustomTarget.Create(base.transform.position, PlayerFarming.Instance.transform.position, 1f, TarotCards.Card.DecreaseRelicCharge, delegate
			{
				GameManager.GetInstance().OnConversationEnd();
			});
			GameManager.GetInstance().OnConversationNext(tarotCustomTarget.gameObject);
		}
		else
		{
			InventoryItem.Spawn(iTEM_TYPE, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(5f, 250f);
		}
		sLabel = "";
		ObjectiveManager.ObjectiveRemoved(completedObjective);
		rewardGiven = true;
		Spine.AnimationState.SetAnimation(0, "exit", false);
		yield return new WaitForSeconds(2f);
		if (giveReward)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator GiveChallengeIE()
	{
		PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.left * 2f);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		Health.team2.Clear();
		ObjectivesData randomDungeonChallenge = Quests.GetRandomDungeonChallenge();
		ObjectiveManager.Add(randomDungeonChallenge);
		string termToSpeak = "";
		if (randomDungeonChallenge.Type == Objectives.TYPES.NO_DAMAGE)
		{
			termToSpeak = "Conversation_NPC/Challenge/Line2_NoDamage";
		}
		else if (randomDungeonChallenge.Type == Objectives.TYPES.NO_CURSES)
		{
			termToSpeak = "Conversation_NPC/Challenge/Line2_NoCurses";
		}
		else if (randomDungeonChallenge.Type == Objectives.TYPES.NO_DODGE)
		{
			termToSpeak = "Conversation_NPC/Challenge/Line2_NoDodging";
		}
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/Challenge/Line1"));
		list.Add(new ConversationEntry(base.gameObject, termToSpeak));
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/Challenge/Line3"));
		list[0].CharacterName = "NAMES/Ratoo";
		list[0].Offset = new Vector3(0f, 2f, 0f);
		list[1].CharacterName = "NAMES/Ratoo";
		list[1].Offset = new Vector3(0f, 2f, 0f);
		list[2].CharacterName = "NAMES/Ratoo";
		list[2].Offset = new Vector3(0f, 2f, 0f);
		list[0].soundPath = "event:/dialogue/ratoo/standard_ratoo";
		list[1].soundPath = "event:/dialogue/ratoo/standard_ratoo";
		list[2].soundPath = "event:/dialogue/ratoo/standard_ratoo";
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		MMConversation.Play(new ConversationObject(list, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (giveReward && rewardGiven)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
