using TMPro;
using UnityEngine;

public class UISlideIndicator : BaseMonoBehaviour
{
	public TextMeshProUGUI priceTxt;

	public GameObject Locked;

	public GameObject Unlocked;

	public Color canAffordColor;

	public Color cantAffordColor;

	private int _price;

	public void setPrice(int price)
	{
		_price = price;
		string text = price.ToString();
		priceTxt.text = text;
		checkUnlocked();
	}

	private void checkUnlocked()
	{
		if (DataManager.Instance.Followers.Count < _price)
		{
			locked();
		}
		else
		{
			unlocked();
		}
	}

	private void locked()
	{
		priceTxt.color = cantAffordColor;
		Locked.SetActive(true);
		Unlocked.SetActive(false);
	}

	private void unlocked()
	{
		priceTxt.color = cantAffordColor;
		Locked.SetActive(false);
		Unlocked.SetActive(true);
	}
}
