using TMPro;

public class SiloDisplay : BaseMonoBehaviour
{
	public TextMeshPro Text;

	private Structure structure;

	private void Start()
	{
		structure = GetComponent<Structure>();
	}

	private void Update()
	{
		if (Text != null)
		{
			Text.text = structure.Inventory.Count.ToString();
		}
	}
}
