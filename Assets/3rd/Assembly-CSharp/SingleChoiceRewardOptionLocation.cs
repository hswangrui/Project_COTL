using System.Collections.Generic;
using UnityEngine;

public class SingleChoiceRewardOptionLocation : SingleChoiceRewardOption
{
	[SerializeField]
	private List<BuyEntry> itemOptionsDungeon1;

	[SerializeField]
	private List<BuyEntry> itemOptionsDungeon2;

	[SerializeField]
	private List<BuyEntry> itemOptionsDungeon3;

	[SerializeField]
	private List<BuyEntry> itemOptionsDungeon4;

	public override List<BuyEntry> GetOptions()
	{
		Debug.Log("PlayerFarming.Location: " + PlayerFarming.Location);
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			return itemOptionsDungeon1;
		case FollowerLocation.Dungeon1_2:
			return itemOptionsDungeon2;
		case FollowerLocation.Dungeon1_3:
			return itemOptionsDungeon3;
		case FollowerLocation.Dungeon1_4:
			return itemOptionsDungeon4;
		default:
			return itemOptionsDungeon1;
		}
	}
}
