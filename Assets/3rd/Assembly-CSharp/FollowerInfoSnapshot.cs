using System;

[Serializable]
public class FollowerInfoSnapshot
{
	public string Name;

	public int SkinCharacter;

	public int SkinVariation;

	public int SkinColour;

	public string SkinName;

	public FollowerOutfitType Outfit;

	public InventoryItem.ITEM_TYPE Necklace;

	public float Illness;

	public float Rest;

	public bool Brainwashed;

	public bool Dissenter;

	public float CultFaith;

	public FollowerInfoSnapshot()
	{
	}

	public FollowerInfoSnapshot(FollowerInfo followerInfo)
	{
		Name = followerInfo.Name;
		SkinCharacter = followerInfo.SkinCharacter;
		SkinVariation = followerInfo.SkinVariation;
		SkinColour = followerInfo.SkinColour;
		SkinName = followerInfo.SkinName;
		Outfit = followerInfo.Outfit;
		Necklace = followerInfo.Necklace;
		Illness = followerInfo.Illness;
		Rest = followerInfo.Rest;
		Brainwashed = FollowerBrainStats.BrainWashed;
		Dissenter = followerInfo.HasThought(Thought.Dissenter);
		CultFaith = DataManager.Instance.CultFaith;
	}
}
