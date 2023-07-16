using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMBiomeGeneration;
using MMTools;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_DungeonStronger : Interaction
{
	private string convoText = "Conversation_NPC/Haro/Dungeon{0}/{1}";

	private string convoText_postGame = "Conversation_NPC/Haro/Special/Encounter_{0}/{1}";

	private List<ConversationEntry> entries = new List<ConversationEntry>();

	public UnityEvent Callback;

	public Interaction_WeaponSelectionPodium[] Weapons;

	private string text
	{
		get
		{
			if ((PlayerFarming.Location == FollowerLocation.Dungeon1_1 && !DataManager.Instance.HaroOnbardedHarderDungeon1) || (PlayerFarming.Location == FollowerLocation.Dungeon1_2 && !DataManager.Instance.HaroOnbardedHarderDungeon2) || (PlayerFarming.Location == FollowerLocation.Dungeon1_3 && !DataManager.Instance.HaroOnbardedHarderDungeon3) || (PlayerFarming.Location == FollowerLocation.Dungeon1_4 && !DataManager.Instance.HaroOnbardedHarderDungeon4))
			{
				return convoText;
			}
			if (GameManager.Layer2)
			{
				return convoText_postGame;
			}
			return convoText;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 3f;
		AutomaticallyInteract = true;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
	}

	private void OnPlayerLocationSet()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
		if (RevealSelf())
		{
			LockDoors();
			HideWeapons();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
	}

	private bool RevealSelf()
	{
		if (DungeonSandboxManager.Active)
		{
			return false;
		}
		if (GameManager.Layer2)
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon1_PostGame)
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			case FollowerLocation.Dungeon1_2:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon2_PostGame)
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			case FollowerLocation.Dungeon1_3:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon3_PostGame)
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			case FollowerLocation.Dungeon1_4:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon4_PostGame)
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			}
		}
		else
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon1 && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			case FollowerLocation.Dungeon1_2:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon2 && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2))
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			case FollowerLocation.Dungeon1_3:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon3 && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3))
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			case FollowerLocation.Dungeon1_4:
				if (!DataManager.Instance.HaroOnbardedHarderDungeon4 && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
				{
					return GameManager.CurrentDungeonFloor <= 1;
				}
				return false;
			}
		}
		return false;
	}

	private void SetVariable()
	{
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			DataManager.Instance.HaroOnbardedHarderDungeon1_PostGame = (DataManager.Instance.HaroOnbardedHarderDungeon1 = true);
			break;
		case FollowerLocation.Dungeon1_2:
			DataManager.Instance.HaroOnbardedHarderDungeon2_PostGame = (DataManager.Instance.HaroOnbardedHarderDungeon2 = true);
			break;
		case FollowerLocation.Dungeon1_3:
			DataManager.Instance.HaroOnbardedHarderDungeon3_PostGame = (DataManager.Instance.HaroOnbardedHarderDungeon3 = true);
			break;
		case FollowerLocation.Dungeon1_4:
			DataManager.Instance.HaroOnbardedHarderDungeon4_PostGame = (DataManager.Instance.HaroOnbardedHarderDungeon4 = true);
			break;
		}
	}

	private int DungeonNumber()
	{
		if ((PlayerFarming.Location == FollowerLocation.Dungeon1_1 && !DataManager.Instance.HaroOnbardedHarderDungeon1) || (PlayerFarming.Location == FollowerLocation.Dungeon1_2 && !DataManager.Instance.HaroOnbardedHarderDungeon2) || (PlayerFarming.Location == FollowerLocation.Dungeon1_3 && !DataManager.Instance.HaroOnbardedHarderDungeon3) || (PlayerFarming.Location == FollowerLocation.Dungeon1_4 && !DataManager.Instance.HaroOnbardedHarderDungeon4) || (PlayerFarming.Location == FollowerLocation.Dungeon1_1 && !DataManager.Instance.HaroOnbardedHarderDungeon1_PostGame && GameManager.Layer2) || (PlayerFarming.Location == FollowerLocation.Dungeon1_2 && !DataManager.Instance.HaroOnbardedHarderDungeon2_PostGame && GameManager.Layer2) || (PlayerFarming.Location == FollowerLocation.Dungeon1_3 && !DataManager.Instance.HaroOnbardedHarderDungeon3_PostGame && GameManager.Layer2) || (PlayerFarming.Location == FollowerLocation.Dungeon1_4 && !DataManager.Instance.HaroOnbardedHarderDungeon4_PostGame && GameManager.Layer2))
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				return 1;
			case FollowerLocation.Dungeon1_2:
				return 2;
			case FollowerLocation.Dungeon1_3:
				return 3;
			case FollowerLocation.Dungeon1_4:
				return 4;
			}
		}
		return 0;
	}

	private string LocationName()
	{
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			return ScriptLocalization.NAMES_Places.Dungeon1_1;
		case FollowerLocation.Dungeon1_2:
			return ScriptLocalization.NAMES_Places.Dungeon1_2;
		case FollowerLocation.Dungeon1_3:
			return ScriptLocalization.NAMES_Places.Dungeon1_3;
		case FollowerLocation.Dungeon1_4:
			return ScriptLocalization.NAMES_Places.Dungeon1_4;
		default:
			return "";
		}
	}

	private void HideWeapons()
	{
		Interaction_WeaponSelectionPodium[] weapons = Weapons;
		foreach (Interaction_WeaponSelectionPodium interaction_WeaponSelectionPodium in weapons)
		{
			if ((bool)interaction_WeaponSelectionPodium)
			{
				interaction_WeaponSelectionPodium.gameObject.SetActive(false);
			}
		}
	}

	private void RevealWeapons()
	{
		Interaction_WeaponSelectionPodium[] weapons = Weapons;
		foreach (Interaction_WeaponSelectionPodium interaction_WeaponSelectionPodium in weapons)
		{
			if ((bool)interaction_WeaponSelectionPodium)
			{
				interaction_WeaponSelectionPodium.gameObject.SetActive(true);
			}
		}
		Interaction_Chest.ChestEvent onChestRevealed = Interaction_Chest.OnChestRevealed;
		if (onChestRevealed != null)
		{
			onChestRevealed();
		}
	}

	private void LockDoors()
	{
		BiomeGenerator.OnBiomeChangeRoom -= LockDoors;
		RoomLockController.CloseAll();
	}

	public override void GetLabel()
	{
		base.Label = (Interactable ? ScriptLocalization.Interactions.Talk : "");
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		StartCoroutine(InteractIE());
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
		for (int i = 0; LocalizationManager.GetTermData(string.Format(text, DungeonNumber(), i)) != null; i++)
		{
			string termToSpeak = string.Format(text, DungeonNumber(), i);
			ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, termToSpeak);
			conversationEntry.CharacterName = "NAMES/Haro";
			conversationEntry.soundPath = "event:/dialogue/haro/standard_haro";
			entries.Add(conversationEntry);
		}
		MMConversation.Play(new ConversationObject(entries, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
		yield return new WaitForSeconds(0.5f);
		NotificationCentreScreen.Play(string.Format(LocalizationManager.GetTranslation("Notifications/EnemiesStronger"), "<color=#FFD201>" + LocationName() + "</color>"));
		SetVariable();
		GameManager.GetInstance().StartCoroutine(DelayWeaponsReveal());
	}

	private IEnumerator DelayWeaponsReveal()
	{
		yield return new WaitForSeconds(3f);
		RevealWeapons();
	}
}
