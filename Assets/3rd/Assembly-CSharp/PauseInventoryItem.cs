using System;
using System.Collections;
using Lamb.UI.Alerts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseInventoryItem : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public Image InventoryItemImage;

	public InventoryItemDisplay inventoryItemDisplay;

	public TextMeshProUGUI AmountText;

	public TextMeshProUGUI AmountText2;

	public InventoryItem.ITEM_TYPE Type;

	public Material normalMaterial;

	public Material bWMaterial;

	[Space]
	[SerializeField]
	private GameObject recipeObject;

	[SerializeField]
	private Image[] faithBars;

	[SerializeField]
	private Image[] hungerBars;

	[SerializeField]
	public MMButton _button;

	[SerializeField]
	private InventoryAlert _alert;

	[SerializeField]
	private CanvasGroup canvasGroup;

	public int Quantity { get; set; }

	public MMButton Button
	{
		get
		{
			return _button;
		}
	}

	public InventoryItem.ITEM_TYPE ItemType
	{
		get
		{
			return Type;
		}
	}

	private void Start()
	{
	}

	public void Init(InventoryItem.ITEM_TYPE type, int Quantity, bool showQuantity = true, int Quantity2 = 0, bool showQuantity2 = false)
	{
		if (type != 0)
		{
			Type = type;
			InventoryItemImage.sprite = inventoryItemDisplay.GetImage(type);
			if (AmountText != null)
			{
				AmountText.gameObject.SetActive(showQuantity);
				AmountText.text = Quantity.ToString();
			}
			if (AmountText2 != null)
			{
				AmountText2.gameObject.SetActive(showQuantity2);
				AmountText2.text = "(" + Quantity2 + ")";
			}
			InventoryItemImage.enabled = true;
			if (Quantity <= 0)
			{
				SetGrey();
			}
			else
			{
				SetWhite();
			}
		}
		else
		{
			Type = type;
			InventoryItemImage.enabled = false;
			AmountText.text = "";
			Selectable component = GetComponent<Selectable>();
			if ((bool)component)
			{
				component.interactable = false;
			}
		}
		if ((bool)recipeObject)
		{
			recipeObject.SetActive(false);
		}
		if (_alert != null)
		{
			_alert.Configure(Type);
		}
		this.Quantity = Quantity;
	}

	public void ShowQuantity2(string s)
	{
		AmountText2.gameObject.SetActive(true);
		AmountText2.text = s;
	}

	public void ShowRecipe()
	{
		recipeObject.SetActive(true);
		for (int i = 0; i < hungerBars.Length; i++)
		{
			hungerBars[i].enabled = i < CookingData.GetSatationAmount(Type);
		}
		for (int j = 0; j < faithBars.Length; j++)
		{
			faithBars[j].enabled = j < CookingData.GetFaithAmount(Type);
		}
	}

	public void SetGrey()
	{
		InventoryItemImage.material = bWMaterial;
		if (AmountText != null)
		{
			AmountText.color = StaticColors.RedColor;
		}
	}

	public void SetWhite()
	{
		InventoryItemImage.material = normalMaterial;
		if (AmountText != null)
		{
			AmountText.color = StaticColors.OffWhiteColor;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		_alert.TryRemoveAlert();
	}

	public void FadeIn(float delay, Action andThen = null)
	{
		StopAllCoroutines();
		StartCoroutine(DoFade(delay, andThen));
	}

	private IEnumerator DoFade(float delay, Action andThen)
	{
		canvasGroup.alpha = 0f;
		yield return new WaitForSecondsRealtime(delay);
		float progress = 0f;
		float duration = 0.2f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.unscaledDeltaTime);
			if (!(num < duration))
			{
				break;
			}
			canvasGroup.alpha = progress / duration;
			yield return null;
		}
		canvasGroup.alpha = 1f;
		if (andThen != null)
		{
			andThen();
		}
	}
}
