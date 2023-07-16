using TMPro;
using UnityEngine;

public class GetAllFontImageNameIcons : MonoBehaviour
{
	public TextMeshProUGUI text;

	private string s = "";

	public void GetText()
	{
		for (int i = 0; i < 117; i++)
		{
			string iconByType = FontImageNames.GetIconByType((InventoryItem.ITEM_TYPE)i);
			s += iconByType;
		}
		text.text = s;
	}
}
