using TMPro;

public class InventoryDisplay : BaseMonoBehaviour
{
	public TextMeshPro Text;

	private void Update()
	{
		if (Text != null)
		{
			Text.text = Inventory.TotalItems().ToString();
		}
	}
}
