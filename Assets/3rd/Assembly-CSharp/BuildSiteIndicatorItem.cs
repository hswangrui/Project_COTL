using UnityEngine.UI;

public class BuildSiteIndicatorItem : BaseMonoBehaviour
{
	public InventoryItemDisplay item;

	public Text Text;

	public void Init(InventoryItem.ITEM_TYPE Type, int Quantity)
	{
		item.SetImage(Type);
		Text.text = Quantity.ToString();
	}
}
