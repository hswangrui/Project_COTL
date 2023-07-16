using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FMOD.Studio;
using I2.Loc;
using MMTools;
using UnityEngine;
using WebSocketSharp;

public class Interaction_FirstFreeFollower : Interaction_FollowerSpawn
{
	private SimpleBark simpleBark;

	private FMOD.Studio.EventInstance receiveLoop;

	private Villager_Info v_i;

	private string skin;

	private WorshipperInfoManager wim;

	protected override void Start()
	{
		base.Start();
		Play("", "", false);
		simpleBark = GetComponent<SimpleBark>();
		RandomiseBark();
		DataManager.Instance.GivenFreeDungeonFollower = true;
		skin = "";
		skin = DataManager.GetRandomLockedSkin();
		if (skin.IsNullOrEmpty())
		{
			skin = DataManager.GetRandomSkin();
		}
		UnityEngine.Debug.Log("?????????????????????????????????");
		UnityEngine.Debug.Log("skin: " + skin);
		_followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base, skin);
		if (_followerInfo.SkinName == "Giraffe")
		{
			_followerInfo.Name = LocalizationManager.GetTranslation("FollowerNames/Sparkles");
		}
		v_i = Villager_Info.NewCharacter(skin);
		wim = GetComponent<WorshipperInfoManager>();
		wim.SetV_I(v_i);
	}

	private void RandomiseBark()
	{
		int num = UnityEngine.Random.Range(0, 5);
		foreach (ConversationEntry entry in simpleBark.Entries)
		{
			string translation = LocalizationManager.GetTranslation("Conversation_NPC/FreeNPC/" + num);
			string term = "";
			switch (num)
			{
			case 2:
				switch (PlayerFarming.Location)
				{
				case FollowerLocation.Dungeon1_1:
					term = "NAMES/CultLeaders/Dungeon1";
					break;
				case FollowerLocation.Dungeon1_2:
					term = "NAMES/CultLeaders/Dungeon2";
					break;
				case FollowerLocation.Dungeon1_3:
					term = "NAMES/CultLeaders/Dungeon3";
					break;
				case FollowerLocation.Dungeon1_4:
					term = "NAMES/CultLeaders/Dungeon4";
					break;
				}
				break;
			case 3:
			case 4:
				switch (PlayerFarming.Location)
				{
				case FollowerLocation.Dungeon1_1:
					term = "NAMES/Places/Dungeon1_1";
					break;
				case FollowerLocation.Dungeon1_2:
					term = "NAMES/Places/Dungeon1_2";
					break;
				case FollowerLocation.Dungeon1_3:
					term = "NAMES/Places/Dungeon1_3";
					break;
				case FollowerLocation.Dungeon1_4:
					term = "NAMES/Places/Dungeon1_4";
					break;
				}
				break;
			}
			string arg = "<color=yellow>" + LocalizationManager.GetTranslation(term) + "</color>";
			entry.Speaker = Spine.gameObject;
			entry.SkeletonData = Spine;
			entry.TermToSpeak = string.Format(translation, arg);
		}
		simpleBark.Translate = false;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sRescue = ScriptLocalization.Interactions.Rescue;
	}

	public override void OnInteract(StateMachine state)
	{
		simpleBark.Close();
		simpleBark.enabled = false;
		Interactable = false;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		StartCoroutine(Delay(delegate
		{
			_003C_003En__0(state);
		}));
	}

	private IEnumerator Delay(Action callback)
	{
		yield return new WaitForSeconds(1f);
		if (callback != null)
		{
			callback();
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0(StateMachine state)
	{
		base.OnInteract(state);
	}
}
