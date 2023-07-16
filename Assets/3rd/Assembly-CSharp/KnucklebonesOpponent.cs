using System;
using Spine.Unity;

[Serializable]
public class KnucklebonesOpponent
{
	public enum OppnentTags
	{
		Ratau,
		Flinky,
		Klunko,
		Shrumy
	}

	public OppnentTags Tag;

	public KnucklebonesPlayerConfiguration Config;

	public SkeletonAnimation Spine;

	public Interaction_SimpleConversation FirstWinConvo;

	public Interaction_SimpleConversation WinConvo;

	public Interaction_SimpleConversation LoseConvo;

	public Interaction_SimpleConversation DrawConvo;

	public TarotCards.Card TarotCardReward;
}
