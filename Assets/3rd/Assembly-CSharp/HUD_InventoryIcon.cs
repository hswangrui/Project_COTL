using TMPro;
using UnityEngine;

public class HUD_InventoryIcon : BaseMonoBehaviour
{
	public InventoryItem.ITEM_TYPE Type;

	public InventoryItemDisplay inventoryItem;

	public TextMeshProUGUI Text;

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
		set
		{
			_rectTransform = value;
		}
	}

	public void InitFromType(InventoryItem.ITEM_TYPE Type)
	{
		this.Type = Type;
		InitFromType();
	}

	public void InitFromType()
	{
		InventoryItem inventoryItem = Inventory.GetItemByType((int)Type);
		if (inventoryItem == null)
		{
			inventoryItem = new InventoryItem();
			inventoryItem.Init((int)Type, 0);
		}
		Init(inventoryItem);
	}

	public void Init(InventoryItem item)
	{
		Type = (InventoryItem.ITEM_TYPE)item.type;
		inventoryItem.SetImage((InventoryItem.ITEM_TYPE)item.type);
		int quantity = item.quantity;
		Text.text = ((quantity < 2) ? "" : item.quantity.ToString());
		Inventory.GetItemByType((int)Type);
		if (item.type != 0)
		{
			Text.text = quantity.ToString();
		}
	}
}
