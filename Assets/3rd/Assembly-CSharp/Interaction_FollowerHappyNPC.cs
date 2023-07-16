using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MMTools;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

public class Interaction_FollowerHappyNPC : Interaction
{
	[SerializeField]
	private Follower followerPrefab;

	public SkeletonAnimation NPCSpine;

	private const int cost = 35;

	private bool firstInteraction = true;

	private List<SimFollower> simFollowers;

	private bool Activated;

	public override void GetLabel()
	{
		if (simFollowers == null)
		{
			simFollowers = FollowerManager.SimFollowersAtLocation(FollowerLocation.Base);
		}
		if (Activated)
		{
			base.Label = "";
			return;
		}
		string text = "";
		if (!Activated && (firstInteraction || simFollowers.Count > 0))
		{
			Interactable = true;
			text = ((!firstInteraction) ? ScriptLocalization.FollowerInteractions.MakeDemand : ScriptLocalization.Interactions.Talk);
		}
		else if (simFollowers.Count <= 0)
		{
			text = ScriptLocalization.Interactions.NoFollowers;
			Interactable = false;
		}
		base.Label = text;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (firstInteraction)
		{
			StartCoroutine(FirstChatIE());
			Activated = true;
		}
		else
		{
			StartCoroutine(InteractIE());
			Activated = true;
		}
	}

	private IEnumerator FirstChatIE()
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/HappyFollowerNPC/Line1"));
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/HappyFollowerNPC/Line2"));
		list[0].CharacterName = LocalizationManager.GetTranslation("NAMES/MothNPC");
		list[0].Offset = new Vector3(0f, 2f, 0f);
		list[0].soundPath = "event:/dialogue/moth/moth";
		list[1].CharacterName = LocalizationManager.GetTranslation("NAMES/MothNPC");
		list[1].Offset = new Vector3(0f, 2f, 0f);
		list[1].soundPath = "event:/dialogue/moth/moth";
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		MMConversation.Play(new ConversationObject(list, null, delegate
		{
		}));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		NPCSpine.AnimationState.SetAnimation(0, "talk1", false);
		NPCSpine.AnimationState.AddAnimation(0, "idle", true, 0f);
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		Activated = false;
		firstInteraction = false;
		GetLabel();
		base.HasChanged = true;
		OnInteract(state);
	}

	private IEnumerator InteractIE()
	{
		PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.left * 2f);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		//followerSelectInstance.VotingType = TwitchVoting.VotingType.FOLLOWER_TO_GAIN_XP;
		List<SimFollower> list = FollowerManager.SimFollowersAtLocation(FollowerLocation.Base);
		list.Sort((SimFollower a, SimFollower b) => b.Brain.Info.XPLevel.CompareTo(a.Brain.Info.XPLevel));
		followerSelectInstance.Show(list);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnShow = (Action)Delegate.Combine(uIFollowerSelectMenuController.OnShow, (Action)delegate
		{
			foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
			{
				followerInfoBox.ShowFaithGain(FollowerBrain.AdorationsAndActions[FollowerBrain.AdorationActions.HappyFollowerNPC], followerInfoBox.followBrain.Stats.MAX_ADORATION);
			}
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController2.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			FollowerManager.SpawnedFollower spawnedFollower = FollowerManager.SpawnCopyFollower(followerInfo, base.transform.position + Vector3.right * 2f, base.transform.parent, PlayerFarming.Location);
			Follower follower = spawnedFollower.Follower;
			follower.Init(spawnedFollower.FollowerBrain, new FollowerOutfit(followerInfo));
			follower.Brain.CheckChangeState();
			follower.gameObject.SetActive(true);
			follower.Interaction_FollowerInteraction.enabled = false;
			follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
			follower.ShowAllFollowerIcons();
			follower.State.LookAngle = Utils.GetAngle(follower.transform.position, base.transform.position);
			follower.State.facingAngle = Utils.GetAngle(follower.transform.position, base.transform.position);
			NPCSpine.AnimationState.AddAnimation(0, "dance", true, 0f);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/enemy_charmed", base.gameObject);
			follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			follower.Spine.AnimationState.SetAnimation(0, "spawn-in", false);
			follower.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			StartCoroutine(GiveFollowerReward(spawnedFollower, follower));
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().OnConversationEnd();
			Activated = false;
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController4 = followerSelectInstance;
		uIFollowerSelectMenuController4.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController4.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
		yield return new WaitForEndOfFrame();
	}

	private IEnumerator RefundCoins()
	{
		float increment = 1f / 35f;
		for (int i = 0; i < 35; i++)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position);
			yield return new WaitForSeconds(increment);
		}
	}

	private IEnumerator GiveFollowerReward(FollowerManager.SpawnedFollower spawnedFollower, Follower follower)
	{
		GameManager.GetInstance().OnConversationNext(follower.gameObject, 4f);
		yield return new WaitForSeconds(1f);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		spawnedFollower.FollowerBrain.AddThought(Thought.HappyFromDungeonNPC);
		spawnedFollower.FollowerFakeBrain.AddThought(Thought.HappyFromDungeonNPC);
		follower.GetComponentInChildren<BarControllerNonUI>(true).SetBarSize(follower.Brain.Stats.Adoration / follower.Brain.Stats.MAX_ADORATION, false, true);
		bool waiting = true;
		spawnedFollower.FollowerFakeBrain.AddAdoration(spawnedFollower.Follower, FollowerBrain.AdorationActions.HappyFollowerNPC, delegate
		{
			waiting = false;
		});
		AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
		while (waiting)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.gameObject);
		follower.Spine.AnimationState.AddAnimation(0, "Reactions/react-happy1", false, 0f);
		string convoResult = "Conversation_NPC/HappyFollowerNPC/Success_Line1";
		follower.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(1.5f);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		follower.SimpleAnimator.enabled = false;
		yield return new WaitForEndOfFrame();
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.Spine.AnimationState.ClearTracks();
		follower.Spine.AnimationState.SetAnimation(0, "spawn-out", false);
		follower.HideAllFollowerIcons();
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, convoResult));
		list[0].CharacterName = LocalizationManager.GetTranslation("NAMES/MothNPC");
		list[0].Offset = new Vector3(0f, 2f, 0f);
		MMConversation.Play(new ConversationObject(list, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		NPCSpine.AnimationState.SetAnimation(0, "talk2", false);
		NPCSpine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(1f);
		follower.gameObject.SetActive(false);
		FollowerManager.CleanUpCopyFollower(spawnedFollower);
	}
}
