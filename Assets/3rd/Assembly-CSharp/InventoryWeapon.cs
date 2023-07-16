public class InventoryWeapon
{
	public enum ITEM_TYPE
	{
		SWORD,
		SHOVEL,
		SEED_BAG,
		WATERING_CAN
	}

	public ITEM_TYPE type;

	public int quantity = 1;

	public string name;

	private bool QuantityAsPercentage;

	public InventoryWeapon(ITEM_TYPE type, int quantity)
	{
		this.type = type;
		this.quantity = quantity;
		switch (type)
		{
		case ITEM_TYPE.SWORD:
			name = "Sword";
			QuantityAsPercentage = true;
			break;
		case ITEM_TYPE.SHOVEL:
			name = "Shovel";
			QuantityAsPercentage = false;
			break;
		case ITEM_TYPE.SEED_BAG:
			name = "Seed Bag";
			QuantityAsPercentage = true;
			break;
		case ITEM_TYPE.WATERING_CAN:
			name = "Watering Can";
			QuantityAsPercentage = true;
			break;
		}
	}

	public string GetQuantity()
	{
		return quantity + (QuantityAsPercentage ? "%" : "");
	}
}
