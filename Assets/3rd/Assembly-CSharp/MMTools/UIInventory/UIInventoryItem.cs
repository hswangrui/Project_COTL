using TMPro;
using UnityEngine;

namespace MMTools.UIInventory
{
	public class UIInventoryItem : BaseMonoBehaviour
	{
		public InventoryItemDisplay inventoryItemDisplay;

		public TextMeshProUGUI Name;

		public TextMeshProUGUI Lore;

		public TextMeshProUGUI Description;

		public InventoryItem Item;

		private RectTransform _rectTransform;

		public RectTransform rectTransform
		{
			get
			{
				if (_rectTransform == null)
				{
					_rectTransform = GetComponent<RectTransform>();
				}
				return _rectTransform;
			}
		}

		public void Init(InventoryItem item)
		{
			if (item == null)
			{
				InitEmpty();
				return;
			}
			if (item.type == 0)
			{
				InitEmpty();
				return;
			}
			Item = item;
			inventoryItemDisplay.SetImage((InventoryItem.ITEM_TYPE)item.type);
			if (Name != null)
			{
				Name.text = InventoryItem.Name((InventoryItem.ITEM_TYPE)item.type);
			}
			if (Lore != null)
			{
				Lore.text = InventoryItem.Lore((InventoryItem.ITEM_TYPE)item.type);
			}
			if (Description != null)
			{
				Description.text = InventoryItem.Description((InventoryItem.ITEM_TYPE)item.type);
			}
		}

		public void InitEmpty()
		{
			inventoryItemDisplay.SetImage(InventoryItem.ITEM_TYPE.NONE);
			if (Name != null)
			{
				Name.text = "";
			}
			if (Lore != null)
			{
				Lore.text = "";
			}
			if (Description != null)
			{
				Description.text = "";
			}
		}
	}
}
