using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KitchenRecipe : BaseMonoBehaviour
{
	public TextMeshProUGUI RecipeText;

	private GameObject LockPosition;

	public Vector3 Offset;

	private Canvas canvas;

	private StructuresData kitchenData;

	public CanvasGroup canvasGroup;

	private RectTransform rectTransform;

	private Camera mainCamera;

	public List<Image> PieChartPiece = new List<Image>();

	public List<float> PieChartPieceValues = new List<float>();

	public List<InventoryItem> LocalInventory = new List<InventoryItem>();

	private bool Hiding = true;

	private void Awake()
	{
		mainCamera = Camera.main;
	}

	public void Play(GameObject LockPosition, StructuresData kitchenData)
	{
		this.LockPosition = LockPosition;
		canvas = GetComponentInParent<Canvas>();
		rectTransform = GetComponent<RectTransform>();
		this.kitchenData = kitchenData;
		base.gameObject.SetActive(true);
		Hiding = false;
		int num = -1;
		while (++num < PieChartPiece.Count)
		{
			PieChartPiece[num].fillAmount = 0f;
		}
	}

	private InventoryItem GetItem(int type)
	{
		foreach (InventoryItem item in LocalInventory)
		{
			if (item.type == type)
			{
				return item;
			}
		}
		return null;
	}

	private void GetRecipe()
	{
		float num = 0f;
		LocalInventory = new List<InventoryItem>();
		foreach (InventoryItem item2 in kitchenData.Inventory)
		{
			InventoryItem item = GetItem(item2.type);
			if (item != null)
			{
				item.quantity += item2.quantity;
			}
			else
			{
				InventoryItem inventoryItem = new InventoryItem();
				inventoryItem.Init(item2.type, item2.quantity);
				LocalInventory.Add(inventoryItem);
			}
			num += (float)item2.quantity;
		}
		PieChartPieceValues = new List<float>();
		int num2 = -1;
		float num3 = 0f;
		while (++num2 < LocalInventory.Count)
		{
			num3 += (float)LocalInventory[num2].quantity / num;
			PieChartPieceValues.Add(num3);
		}
		if (LocalInventory.Count <= 0)
		{
			RecipeText.text = "Empty";
			return;
		}
		int num4 = int.MinValue;
		InventoryItem.ITEM_TYPE type = InventoryItem.ITEM_TYPE.MEAT;
		foreach (InventoryItem item3 in LocalInventory)
		{
			if (item3.quantity > num4)
			{
				num4 = item3.quantity;
				type = (InventoryItem.ITEM_TYPE)item3.type;
			}
		}
		RecipeText.text = InventoryItem.Name(type) + " Stew";
	}

	private void Update()
	{
		if (LockPosition != null)
		{
			Vector3 position = mainCamera.WorldToScreenPoint(LockPosition.transform.position) + Offset * canvas.scaleFactor;
			rectTransform.position = position;
		}
		int num = -1;
		while (++num < PieChartPieceValues.Count)
		{
			PieChartPiece[num].fillAmount = Mathf.Lerp(PieChartPiece[num].fillAmount, PieChartPieceValues[num], Time.deltaTime * 10f);
		}
		if (!Hiding)
		{
			GetRecipe();
			if (canvasGroup.alpha < 1f)
			{
				canvasGroup.alpha += Time.deltaTime * 5f;
			}
		}
		else if (canvasGroup.alpha > 0f)
		{
			canvasGroup.alpha -= 5f * Time.deltaTime;
		}
		else if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void Hide()
	{
		Hiding = true;
	}
}
