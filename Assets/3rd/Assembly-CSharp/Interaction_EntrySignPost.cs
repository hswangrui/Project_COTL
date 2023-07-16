using System;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_EntrySignPost : Interaction
{
	[Serializable]
	public class LocationAndSprite
	{
		public FollowerLocation Location;

		public Sprite Sprite;

		public GameObject Rubble;
	}

	[SerializeField]
	private GameObject cameraOffset;

	[SerializeField]
	private FollowerLocation Location;

	[SerializeField]
	private SpriteRenderer SpriteFront;

	[SerializeField]
	private SpriteRenderer SpriteBack;

	private bool LocationHasBeenSet;

	public List<LocationAndSprite> LocationAndSprites = new List<LocationAndSprite>();

	private string sLabel;

	private string sBrokenLabel;

	private bool BrokenSign;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (GameManager.CurrentDungeonFloor != 1)
		{
			base.gameObject.SetActive(false);
		}
		if (!LocationHasBeenSet)
		{
			LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
	}

	private void OnPlayerLocationSet()
	{
		LocationHasBeenSet = true;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
		foreach (LocationAndSprite locationAndSprite in LocationAndSprites)
		{
			if (locationAndSprite.Location == PlayerFarming.Location)
			{
				SpriteFront.sprite = locationAndSprite.Sprite;
				SpriteBack.sprite = locationAndSprite.Sprite;
				break;
			}
		}
		if (DataManager.Instance.SignPostsRead.Contains(PlayerFarming.Location))
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		sLabel = ScriptLocalization.Interactions.Read;
	}

	public override void GetLabel()
	{
		if (BrokenSign)
		{
			base.Label = "";
			return;
		}
		if (string.IsNullOrEmpty(sLabel))
		{
			UpdateLocalisation();
		}
		base.Label = sLabel;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (!DataManager.Instance.SignPostsRead.Contains(PlayerFarming.Location))
		{
			DataManager.Instance.SignPostsRead.Add(PlayerFarming.Location);
		}
		if (!DataManager.Instance.SignPostsRead.Contains(PlayerFarming.Location))
		{
			DataManager.Instance.SignPostsRead.Add(PlayerFarming.Location);
		}
		string term = "";
		switch (Location)
		{
		case FollowerLocation.Dungeon1_1:
			term = ScriptLocalization.NAMES_CultLeaders.Dungeon1;
			break;
		case FollowerLocation.Dungeon1_2:
			term = ScriptLocalization.NAMES_CultLeaders.Dungeon2;
			break;
		case FollowerLocation.Dungeon1_3:
			term = ScriptLocalization.NAMES_CultLeaders.Dungeon3;
			break;
		case FollowerLocation.Dungeon1_4:
			term = ScriptLocalization.NAMES_CultLeaders.Dungeon4;
			break;
		}
		List<ConversationEntry> list = new List<ConversationEntry>
		{
			new ConversationEntry(cameraOffset, ScriptLocalization.Conversation_NPC_NewDungeonSign._0)
		};
		string translation = LocalizationManager.GetTranslation(ScriptLocalization.Conversation_NPC_NewDungeonSign._0);
		if (GameManager.Layer2)
		{
			translation = LocalizationManager.GetTranslation("Conversation_NPC/Milestones/PostGame");
		}
		list[0].TermToSpeak = string.Format(translation, "<color=#FFD201>" + LocalizationManager.GetTranslation(term) + "</color>");
		if (BrokenSign)
		{
			string termToSpeak = list[0].TermToSpeak;
			int startIndex = termToSpeak.IndexOf("will be");
			list[0].TermToSpeak = "..." + termToSpeak.Substring(startIndex);
		}
		MMConversation.Play(new ConversationObject(list, null, null), true, true, true, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 350f;
	}

	public void OnDie()
	{
		MMConversation mmConversation = MMConversation.mmConversation;
		if ((object)mmConversation != null)
		{
			mmConversation.Close();
		}
		foreach (LocationAndSprite locationAndSprite in LocationAndSprites)
		{
			if (locationAndSprite.Location == PlayerFarming.Location)
			{
				locationAndSprite.Rubble.SetActive(true);
			}
		}
		BrokenSign = true;
	}
}
