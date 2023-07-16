public class ChoiceReward
{
	public enum RewardType
	{
		Follower,
		Resource,
		CrownAbility,
		FollowerRole
	}

	public RewardType Type;

	public string Title;

	public string SubTitle;

	public FollowerInfo FollowerInfo;

	public int Paid;

	public int Cost;

	public InventoryItem.ITEM_TYPE Currency;

	public bool Locked;

	public InventoryItem.ITEM_TYPE ItemType;

	public int Quantity;

	public CrownAbilities.TYPE CrownAbility;

	public FollowerRole FollowerRole;

	public ChoiceReward()
	{
	}

	public ChoiceReward(int Cost, InventoryItem.ITEM_TYPE Currency, RewardType Type, FollowerInfo FollowerInfo)
	{
		this.Type = Type;
		this.FollowerInfo = FollowerInfo;
		this.Cost = Cost;
		Title = "New Follower";
		SubTitle = FollowerInfo.Name;
		this.Currency = Currency;
	}

	public ChoiceReward(int Cost, InventoryItem.ITEM_TYPE Currency, RewardType Type, InventoryItem.ITEM_TYPE ItemType, int Quantity)
	{
		this.Type = Type;
		this.ItemType = ItemType;
		this.Quantity = Quantity;
		this.Cost = Cost;
		Title = "Nature's Bounty";
		SubTitle = InventoryItem.Name(ItemType) + " x" + Quantity;
		this.Currency = Currency;
	}

	public ChoiceReward(int Cost, InventoryItem.ITEM_TYPE Currency, RewardType Type, CrownAbilities.TYPE CrownAbility)
	{
		this.Type = Type;
		this.CrownAbility = CrownAbility;
		Title = "Crown Ability";
		SubTitle = CrownAbilities.LocalisedName(CrownAbility);
		this.Cost = Cost;
		this.Currency = Currency;
	}

	public ChoiceReward(FollowerRole FollowerRole, RewardType Type, bool Locked = false)
	{
		this.Type = Type;
		this.FollowerRole = FollowerRole;
		this.Locked = Locked;
		Title = FollowerRole.ToString();
		if (FollowerRole == FollowerRole.Worshipper)
		{
			SubTitle = (Locked ? "Requires Shrine" : "");
		}
		else
		{
			SubTitle = "";
		}
		Cost = 0;
	}
}
