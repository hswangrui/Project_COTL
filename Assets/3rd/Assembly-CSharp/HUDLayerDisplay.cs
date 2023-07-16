using TMPro;

public class HUDLayerDisplay : BaseMonoBehaviour
{
	public TextMeshProUGUI Text;

	private void OnEnable()
	{
		OnTempleKeys();
	}

	private void OnDisable()
	{
	}

	private void OnTempleKeys()
	{
		Text.text = "<sprite name=\"icon_key\"> " + Inventory.TempleKeys;
	}
}
