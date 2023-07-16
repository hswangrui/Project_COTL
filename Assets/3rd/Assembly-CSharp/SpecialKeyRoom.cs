using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MMTools;
using src.Extensions;
using UnityEngine;

public class SpecialKeyRoom : Interaction
{
	[SerializeField]
	private GameObject ratoo;

	[SerializeField]
	private GameObject spawnPosition;

	[SerializeField]
	private GameObject playerPosition;

	[TermsPopup("")]
	[SerializeField]
	private string characterName;

	[TermsPopup("")]
	[SerializeField]
	private string responseSacrifice;

	[TermsPopup("")]
	[SerializeField]
	private string responseIgnore;

	[Space]
	[SerializeField]
	private Interaction_KeyPiece keyPiecePrefab;

	public override void GetLabel()
	{
		base.GetLabel();
		base.Label = "T";
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		PlayerFarming.Instance.GoToAndStop(playerPosition, null, false, false, delegate
		{
			string line = GetLine();
			string termToSpeak = line + "1";
			string termToSpeak2 = line + "2";
			List<ConversationEntry> obj = new List<ConversationEntry>
			{
				new ConversationEntry(base.gameObject, termToSpeak),
				new ConversationEntry(base.gameObject, termToSpeak2)
			};
			string text = "Conversation_NPC/Ratoo/KeyRoom/AnswerA";
			string text2 = "Conversation_NPC/Ratoo/KeyRoom/AnswerB";
			List<MMTools.Response> responses = new List<MMTools.Response>
			{
				new MMTools.Response(text, delegate
				{
					StartCoroutine(SacrificeFollowerIE());
				}, text),
				new MMTools.Response(text2, delegate
				{
					StartCoroutine(IgnoreIE());
				}, text2)
			};
			obj[0].CharacterName = characterName;
			obj[1].CharacterName = characterName;
			MMConversation.Play(new ConversationObject(obj, responses, null), false);
		});
	}

	private string GetLine()
	{
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			return "Conversation_NPC/Ratoo/KeyRoom/Encounter1/";
		case FollowerLocation.Dungeon1_2:
			return "Conversation_NPC/Ratoo/KeyRoom/Encounter2/";
		case FollowerLocation.Dungeon1_3:
			return "Conversation_NPC/Ratoo/KeyRoom/Encounter3/";
		case FollowerLocation.Dungeon1_4:
			return "Conversation_NPC/Ratoo/KeyRoom/Encounter4/";
		default:
			return "";
		}
	}

	private IEnumerator SacrificeFollowerIE()
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		FollowerInfo selectedFollower = null;
		List<FollowerBrain> list = FollowerBrain.AllAvailableFollowerBrains();
		if (list.Count > 0)
		{
			UIFollowerSelectMenuController uIFollowerSelectMenuController = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
			//uIFollowerSelectMenuController.VotingType = TwitchVoting.VotingType.RITUAL_SACRIFICE;
			uIFollowerSelectMenuController.Show(list);
			uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo f)
			{
				selectedFollower = f;
			});
			while (selectedFollower == null)
			{
				yield return null;
			}
			FollowerManager.SpawnedFollower spawnedFollower = FollowerManager.SpawnCopyFollower(selectedFollower, spawnPosition.transform.position, base.transform, PlayerFarming.Location);
			spawnedFollower.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			spawnedFollower.Follower.SetBodyAnimation("fox-spawn", false);
			spawnedFollower.Follower.AddBodyAnimation("fox-sacrifice", false, 0f);
			GameManager.GetInstance().OnConversationNext(spawnedFollower.Follower.gameObject, 6f);
			yield return new WaitForSeconds(3f);
			FollowerManager.CleanUpCopyFollower(spawnedFollower);
			spawnedFollower.FollowerBrain.Die(NotificationCentre.NotificationType.SacrificedAwayFromCult);
			List<ConversationEntry> list2 = new List<ConversationEntry>();
			list2.Add(new ConversationEntry(base.gameObject, responseSacrifice));
			list2[0].CharacterName = characterName;
			MMConversation.Play(new ConversationObject(list2, null, null), false);
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
			Interaction_KeyPiece KeyPiece = UnityEngine.Object.Instantiate(keyPiecePrefab, ratoo.transform.position + new Vector3(0f, -0.2f, -1.25f), Quaternion.identity, base.transform.parent);
			KeyPiece.transform.localScale = Vector3.zero;
			bool waiting = true;
			KeyPiece.transform.DOScale(Vector3.one, 1f).SetEase(Ease.InBack).OnComplete(delegate
			{
				KeyPiece.transform.DOMove(PlayerFarming.Instance.transform.position + Vector3.back * 0.5f, 1f).SetEase(Ease.InBack).OnComplete(delegate
				{
					GameManager.GetInstance().OnConversationEnd(false);
					KeyPiece.OnInteract(PlayerFarming.Instance.state);
					waiting = false;
				});
			});
			while (waiting)
			{
				yield return null;
			}
			SetCompleteVariable();
			yield return new WaitForSeconds(4f);
		}
		else
		{
			List<ConversationEntry> list3 = new List<ConversationEntry>();
			list3.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/Ratoo/KeyRoom/NoFollowers"));
			list3[0].CharacterName = characterName;
			MMConversation.Play(new ConversationObject(list3, null, null), false);
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
		}
		GameManager.GetInstance().OnConversationEnd();
	}

	private IEnumerator IgnoreIE()
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, responseIgnore));
		list[0].CharacterName = characterName;
		MMConversation.Play(new ConversationObject(list, null, null), false);
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
	}

	public void SetCompleteVariable()
	{
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			DataManager.Instance.SetVariable(DataManager.Variables.DungeonKeyRoomCompleted1, true);
			break;
		case FollowerLocation.Dungeon1_2:
			DataManager.Instance.SetVariable(DataManager.Variables.DungeonKeyRoomCompleted2, true);
			break;
		case FollowerLocation.Dungeon1_3:
			DataManager.Instance.SetVariable(DataManager.Variables.DungeonKeyRoomCompleted3, true);
			break;
		case FollowerLocation.Dungeon1_4:
			DataManager.Instance.SetVariable(DataManager.Variables.DungeonKeyRoomCompleted4, true);
			break;
		}
	}
}
