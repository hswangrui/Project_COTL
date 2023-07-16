using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Map;
using MMTools;
using UnityEngine;

public class Interaction_RandomiseMap : Interaction
{
	private const int cost = 15;

	private bool firstInteraction = true;

	public override void GetLabel()
	{
		string text = "";
		if (Interactable)
		{
			text = (firstInteraction ? ScriptLocalization.Interactions.Talk : ((Inventory.GetItemQuantity(20) >= 15) ? (ScriptLocalization.Interactions.Give + " " + 15 + " " + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.BLACK_GOLD)) : (ScriptLocalization.Interactions.CantAfford + " <color=red>" + 15 + " " + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.BLACK_GOLD))));
		}
		base.Label = text;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		if (firstInteraction)
		{
			StartCoroutine(FirstChatIE());
		}
		else if (Inventory.GetItemQuantity(20) >= 15)
		{
			StartCoroutine(InteractIE());
		}
	}

	private IEnumerator FirstChatIE()
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, "Conversation_NPC/RandomiseMapNPC/Line1"));
		list[0].CharacterName = "NAMES/Ratau";
		list[0].Offset = new Vector3(0f, 2f, 0f);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		MMConversation.Play(new ConversationObject(list, null, null));
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		firstInteraction = false;
		Interactable = true;
		GetLabel();
		base.HasChanged = true;
	}

	private IEnumerator InteractIE()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.RandomiseNextNodes();
		MapManager.Instance.CloseMap();
		while (adventureMapOverlayController.IsHiding)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
	}
}
