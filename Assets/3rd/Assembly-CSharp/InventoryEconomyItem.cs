using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryEconomyItem : BaseMonoBehaviour
{
	public TextMeshProUGUI AmountOfResource;

	public TextMeshProUGUI ResourceIcon;

	public GameObject ProgressBar;

	public Image ProgressBar_Progress;

	private InventoryItem.ITEM_TYPE Type;

	private Coroutine cLerpBarRoutine;

	private void Start()
	{
	}

	public void Init(InventoryItem.ITEM_TYPE type)
	{
		Type = type;
		ResourceIcon.text = FontImageNames.GetIconWhiteByType(type);
		UpdateResources();
		if (Inventory.GetResourceCapacity(type) == -1f)
		{
			ProgressBar.SetActive(false);
		}
	}

	public void UpdateResources()
	{
		AmountOfResource.text = "";
		int length = Inventory.GetItemQuantity((int)Type).ToString().Length;
		int num = 4 - length;
		string text = "<color=#6e6d69>0</color>";
		for (int i = 0; i < num; i++)
		{
			AmountOfResource.text = (AmountOfResource.text += text);
		}
		AmountOfResource.text += Inventory.GetItemQuantity((int)Type);
		if (Type == InventoryItem.ITEM_TYPE.INGREDIENTS)
		{
			AmountOfResource.text += "<size=15>Kj</size>";
		}
		if (!ProgressBar.activeSelf)
		{
			return;
		}
		if (ProgressBar_Progress.fillAmount != Inventory.GetResourceCapacity(Type))
		{
			if (cLerpBarRoutine != null)
			{
				StopCoroutine(cLerpBarRoutine);
			}
			cLerpBarRoutine = StartCoroutine(LerpBarRoutine());
		}
		if (!Inventory.CheckCapacityFull(Type))
		{
			ProgressBar_Progress.color = StaticColors.RedColor;
		}
		else
		{
			ProgressBar_Progress.color = StaticColors.OffWhiteColor;
		}
	}

	private IEnumerator LerpBarRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		float StartPosition = ProgressBar_Progress.fillAmount;
		float Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ProgressBar_Progress.fillAmount = Mathf.Lerp(StartPosition, Inventory.GetResourceCapacity(Type), Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		ProgressBar_Progress.fillAmount = Inventory.GetResourceCapacity(Type);
	}
}
