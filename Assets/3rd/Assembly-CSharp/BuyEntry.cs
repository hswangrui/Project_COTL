using System;

[Serializable]
public class BuyEntry
{
	public bool Bought;

	public bool Decoration;

	public bool TarotCard;

	public TarotCards.Card Card;

	public StructureBrain.TYPES decorationToBuy;

	public InventoryItem.ITEM_TYPE itemToBuy;

	public InventoryItem.ITEM_TYPE costType;

	public int itemCost;

	public int quantity = 1;

	public int GroupID = -1;

	public bool SingleQuantityItem;

	public bool pickedForSale;

	public BuyEntry()
	{
	}

	public BuyEntry(InventoryItem.ITEM_TYPE itemToBuy, InventoryItem.ITEM_TYPE CostType, int itemCost, int quantity = 1)
	{
		this.itemToBuy = itemToBuy;
		this.itemCost = itemCost;
		costType = CostType;
		this.quantity = quantity;
	}

	public BuyEntry(StructureBrain.TYPES decorationToBuy, InventoryItem.ITEM_TYPE CostType, int itemCost, int quantity = 1)
	{
		this.decorationToBuy = decorationToBuy;
		this.itemCost = itemCost;
		costType = CostType;
		Decoration = true;
	}

	private void Start()
	{
		pickedForSale = false;
	}
}
