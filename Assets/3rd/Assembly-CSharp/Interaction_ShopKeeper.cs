using System.Collections.Generic;
using I2.Loc;
using MMTools;

public class Interaction_ShopKeeper : Interaction
{
	private bool spoken;

	private void Start()
	{
		UpdateLocalisation();
		ActivateDistance = 2f;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		base.Label = ScriptLocalization.Interactions.Talk;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!spoken)
		{
			MMConversation.Play(new ConversationObject(new List<ConversationEntry>
			{
				new ConversationEntry(base.gameObject, "Hello, yes we are open From 9-5 come back than")
			}, null, null));
			spoken = true;
			base.Label = "";
		}
	}

	private void TellMeMore()
	{
		MMConversation.Play(new ConversationObject(new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, "Okay great buddy, just untie the rope and i'll give you all the bananas you've ever wanted!!")
		}, null, null));
	}

	private void EndConversation()
	{
		MMConversation.Play(new ConversationObject(new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, "You will regret that...")
		}, null, null));
	}
}
