using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_MissionaryComplete : Interaction
{
	private Follower follower;

	private void Start()
	{
		GetComponent<Follower>().HideAllFollowerIcons();
	}

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.Talk;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		follower = GetComponent<Follower>();
		follower.State.LookAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
		follower.State.facingAngle = follower.State.LookAngle;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, -1f));
		HUD_Manager.Instance.Hide(false, 0);
		SimulationManager.Pause();
		if (follower.Brain._directInfoAccess.MissionaryRewards.Length != 0)
		{
			GameManager.GetInstance().StartCoroutine(SuccessIE());
		}
		else
		{
			GameManager.GetInstance().StartCoroutine(FailureIE());
		}
		Interactable = false;
	}

	private IEnumerator SuccessIE()
	{
		string text = "\n";
		InventoryItem[] missionaryRewards = follower.Brain._directInfoAccess.MissionaryRewards;
		foreach (InventoryItem inventoryItem in missionaryRewards)
		{
			text = text + inventoryItem.quantity + " " + FontImageNames.GetIconByType((InventoryItem.ITEM_TYPE)inventoryItem.type) + " ";
		}
		text += "\n";
		int num = Random.Range(1, 4);
		List<ConversationEntry> list = new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, LocalizationManager.GetTranslation(string.Format("Conversation_NPC/Follower/Missionary/Success{0}", num))),
			new ConversationEntry(base.gameObject, string.Format(LocalizationManager.GetTranslation("Conversation_NPC/Follower/Missionary/Success_Line2"), text))
		};
		list[0].CharacterName = follower.Brain._directInfoAccess.Name;
		list[0].Offset = new Vector3(0f, 1f, 0f);
		list[1].CharacterName = follower.Brain._directInfoAccess.Name;
		list[1].Offset = new Vector3(0f, 1f, 0f);
		foreach (ConversationEntry item2 in list)
		{
			item2.soundPath = "event:/dialogue/followers/general_talk";
		}
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		MMConversation.Play(new ConversationObject(list, null, null), true, true, true, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		if (follower.Brain._directInfoAccess.MissionaryRewards[0].type == 85)
		{
			FollowerInfo f = FollowerInfo.NewCharacter(FollowerLocation.Base);
			DataManager.SetFollowerSkinUnlocked(f.SkinName);
			if (BiomeBaseManager.Instance.SpawnExistingRecruits && DataManager.Instance.Followers_Recruit.Count > 0)
			{
				DataManager.Instance.Followers_Recruit.Add(f);
			}
			else
			{
				BiomeBaseManager.Instance.SpawnExistingRecruits = false;
				yield return new WaitForEndOfFrame();
				GameManager.GetInstance().OnConversationNew(true, true, true);
				GameManager.GetInstance().OnConversationNext(BiomeBaseManager.Instance.RecruitSpawnLocation, 6f);
				yield return new WaitForSeconds(0.5f);
				DataManager.Instance.Followers_Recruit.Add(f);
				FollowerManager.SpawnExistingRecruits(BiomeBaseManager.Instance.RecruitSpawnLocation.transform.position);
				Object.FindObjectOfType<FollowerRecruit>().ManualTriggerAnimateIn();
				BiomeBaseManager.Instance.SpawnExistingRecruits = true;
				yield return new WaitForSeconds(2f);
				GameManager.GetInstance().OnConversationNext(BiomeBaseManager.Instance.RecruitSpawnLocation, 8f);
				yield return new WaitForSeconds(1f);
				GameManager.GetInstance().OnConversationEnd();
			}
		}
		else
		{
			GameManager.GetInstance().OnConversationNew(true, true, true);
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
			int total = 0;
			missionaryRewards = follower.Brain._directInfoAccess.MissionaryRewards;
			foreach (InventoryItem inventoryItem2 in missionaryRewards)
			{
				total += inventoryItem2.quantity;
			}
			InventoryItem[] missionaryRewards2 = follower.Brain._directInfoAccess.MissionaryRewards;
			foreach (InventoryItem item in missionaryRewards2)
			{
				float increment = 1f / (float)total;
				for (int i = 0; i < item.quantity; i++)
				{
					AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
					ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, base.transform.position, (InventoryItem.ITEM_TYPE)item.type, null);
					Inventory.AddItem(item.type, 1, true);
					yield return new WaitForSeconds(increment);
				}
			}
			GameManager.GetInstance().OnConversationEnd();
		}
		SimulationManager.UnPause();
		follower.Brain._directInfoAccess.WakeUpDay = -1;
		follower.Brain.CompleteCurrentTask();
		follower.SetOutfit(FollowerOutfitType.Follower, false);
		GetComponent<Follower>().ShowAllFollowerIcons();
		Finish();
		follower.Brain.MakeExhausted();
	}

	private IEnumerator FailureIE()
	{
		int num = Random.Range(1, 4);
		List<ConversationEntry> list = new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, string.Format("Conversation_NPC/Follower/Missionary/Fail{0}", num))
		};
		list[0].CharacterName = follower.Brain._directInfoAccess.Name;
		list[0].Offset = new Vector3(0f, 1f, 0f);
		foreach (ConversationEntry item in list)
		{
			item.soundPath = "event:/dialogue/followers/general_talk";
		}
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		MMConversation.Play(new ConversationObject(list, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		DeadWorshipper worshipper = null;
		follower.Die(NotificationCentre.NotificationType.Died, true, 1, "dead", delegate(GameObject obj)
		{
			worshipper = obj.GetComponent<DeadWorshipper>();
			worshipper.SetOutfit(FollowerOutfitType.Sherpa);
		});
		Finish();
		yield return new WaitForSeconds(2f);
		SimulationManager.UnPause();
		worshipper.SetOutfit(FollowerOutfitType.Follower);
	}

	private void Finish()
	{
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>(FollowerLocation.Base);
		if (allStructuresOfType.Count > 0)
		{
			allStructuresOfType[0].Data.MultipleFollowerIDs.Remove(follower.Brain.Info.ID);
		}
		follower.Brain._directInfoAccess.MissionaryRewards = null;
		follower.Brain._directInfoAccess.MissionaryFinished = false;
		foreach (Interaction_Missionaries missionary in Interaction_Missionaries.Missionaries)
		{
			missionary.UpdateSlots();
		}
		DataManager.Instance.Followers_OnMissionary_IDs.Remove(follower.Brain.Info.ID);
		SimulationManager.UnPause();
		Time.timeScale = 1f;
		follower.Spine.UseDeltaTime = true;
		PlayerFarming.Instance.Spine.UseDeltaTime = true;
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		base.enabled = false;
	}
}
