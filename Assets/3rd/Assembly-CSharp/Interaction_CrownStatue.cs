using System.Collections.Generic;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_CrownStatue : Interaction
{
	[SerializeField]
	private GameObject cameraOffset;

	private string sLabel;

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.Read;
	}

	public override void GetLabel()
	{
		if (string.IsNullOrEmpty(sLabel))
		{
			UpdateLocalisation();
		}
		base.Label = sLabel;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		MMConversation.Play(new ConversationObject(new List<ConversationEntry>
		{
			new ConversationEntry(cameraOffset, "Conversation_NPC/DoorRoomStatue/0"),
			new ConversationEntry(cameraOffset, "Conversation_NPC/DoorRoomStatue/1")
		}, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
	}
}
