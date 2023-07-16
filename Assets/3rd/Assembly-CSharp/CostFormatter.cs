using System.Collections.Generic;

public class CostFormatter
{
	public static string FormatCosts(List<InventoryItem> items, bool showQuantity = true, bool ignoreAffordability = false)
	{
		List<StructuresData.ItemCost> list = new List<StructuresData.ItemCost>();
		foreach (InventoryItem item in items)
		{
			list.Add(new StructuresData.ItemCost((InventoryItem.ITEM_TYPE)item.type, item.quantity));
		}
		return FormatCosts(list, showQuantity, ignoreAffordability);
	}

	public static string FormatCosts(List<StructuresData.ItemCost> itemCosts, bool showQuantity = true, bool ignoreAffordability = false)
	{
		return FormatCosts(itemCosts.ToArray(), showQuantity, ignoreAffordability);
	}

	public static string FormatCosts(StructuresData.ItemCost[] itemCosts, bool showQuantity = true, bool ignoreAffordability = false)
	{
		string text = string.Empty;
		foreach (StructuresData.ItemCost itemCost in itemCosts)
		{
			text += FormatCost(itemCost, showQuantity, ignoreAffordability);
		}
		return text;
	}

	public static string FormatCost(StructuresData.ItemCost itemCost, bool showQuantity = true, bool ignoreAffordability = false)
	{
		return FormatCost(itemCost.CostItem, itemCost.CostValue, Inventory.GetItemQuantity(itemCost.CostItem), showQuantity, ignoreAffordability);
	}

	public static string FormatCost(InventoryItem.ITEM_TYPE itemType, int cost, bool showQuantity = true, bool ignoreAffordability = false)
	{
		return FormatCost(itemType, cost, Inventory.GetItemQuantity(itemType), showQuantity, ignoreAffordability);
	}

	public static string FormatCost(InventoryItem.ITEM_TYPE itemType, int cost, int quantity, bool showQuantity = true, bool ignoreAffordability = false)
	{
		string text = (FontImageNames.GetIconByType(itemType) + " " + cost).Bold();
		string str = ("(" + quantity + ")").Size(-2);
		if (cost > quantity && !ignoreAffordability)
		{
			return text.Colour("#FD1D03") + " " + (showQuantity ? str.Colour("#BA1400") : string.Empty);
		}
		return text + " " + (showQuantity ? str.Colour(StaticColors.GreyColor) : string.Empty);
	}
}
