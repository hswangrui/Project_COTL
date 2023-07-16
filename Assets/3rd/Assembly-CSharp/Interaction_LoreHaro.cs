using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_LoreHaro : Interaction
{
	private string convoText = "Conversation_NPC/Haro/Conversation_{0}/Line{1}";

	private string convoFinalText = "Conversation_NPC/Haro/Conversation_Final/Line{0}";

	private List<ConversationEntry> entries = new List<ConversationEntry>();

	public UnityEvent Callback;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 3f;
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
		for (int i = 1; LocalizationManager.GetTermData(string.Format(convoText, DataManager.Instance.HaroConversationIndex, i)) != null; i++)
		{
			string termToSpeak = string.Format(convoText, DataManager.Instance.HaroConversationIndex, i);
			ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, termToSpeak);
			conversationEntry.CharacterName = "NAMES/Haro";
			conversationEntry.soundPath = "event:/dialogue/haro/standard_haro";
			entries.Add(conversationEntry);
		}
		MMConversation.Play(new ConversationObject(entries, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		DataManager.Instance.HaroConversationIndex++;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}
}
