using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class RefineryItemInfoCard : ItemInfoCard
	{
		[SerializeField]
		private TextMeshProUGUI _itemCost;

		public override void Configure(InventoryItem.ITEM_TYPE config)
		{
			_itemCost.text = GetCostText(config);
			base.Configure(config);
		}

		public string GetCostText(InventoryItem.ITEM_TYPE type)
		{
			string text = "";
			List<StructuresData.ItemCost> cost = Structures_Refinery.GetCost(type);
			for (int i = 0; i < cost.Count; i++)
			{
				int itemQuantity = Inventory.GetItemQuantity((int)cost[i].CostItem);
				int costValue = cost[i].CostValue;
				text += ((costValue > itemQuantity) ? "<color=#ff0000>" : "<color=#FEF0D3>");
				text += FontImageNames.GetIconByType(cost[i].CostItem);
				text = text + itemQuantity + "</color> / " + costValue.ToString() + "  ";
			}
			Debug.Log("GetCostText: " + text);
			return text;
		}
	}
}
