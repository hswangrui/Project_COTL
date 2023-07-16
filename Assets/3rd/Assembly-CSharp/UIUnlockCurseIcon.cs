using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnlockCurseIcon : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[Serializable]
	public class TypeAndImage
	{
		public TarotCards.Card Type;

		public Sprite IconSprite;
	}

	public TarotCards.Card Type;

	public Image Image;

	public Image SelectedIcon;

	public Image WhiteFlash;

	public Image OwnThisImage;

	public Material NormalMaterial;

	public Material BWMaterial;

	public Material redOutline;

	public Material greenOutline;

	public TMP_Text upgradeLevel;

	public TMP_Text upgradeLevelBackground;

	public GameObject costParent;

	public TMP_Text costText;

	public List<TypeAndImage> Icons = new List<TypeAndImage>();

	private InventoryItem[] cost;

	public bool Locked { get; private set; } = true;


	public void Init(TarotCards.Card Type)
	{
		this.Type = Type;
		foreach (TypeAndImage icon in Icons)
		{
			if (icon.Type == this.Type)
			{
				Image.sprite = icon.IconSprite;
			}
		}
		SelectedIcon.enabled = false;
		WhiteFlash.color = new Color(1f, 1f, 1f, 0f);
		OwnThisImage.enabled = true;
		SelectedIcon.material = redOutline;
		Image.material = BWMaterial;
		costText.text = "";
		costParent.SetActive(false);
		InventoryItem[] array = cost;
		foreach (InventoryItem inventoryItem in array)
		{
			string text = ((Inventory.GetItemQuantity(inventoryItem.type) >= inventoryItem.quantity) ? "<color=#f4ecd3>" : "<color=red>");
			costText.text += string.Format("<size=30>{0}</size> {1}{2}</color>/{3}", FontImageNames.GetIconByType((InventoryItem.ITEM_TYPE)inventoryItem.type), text, Inventory.GetItemQuantity(inventoryItem.type), inventoryItem.quantity);
		}
	}

	public void SetUnlocked()
	{
		OwnThisImage.enabled = false;
		SelectedIcon.material = greenOutline;
		Image.material = NormalMaterial;
	}

	public void OnSelect(BaseEventData eventData)
	{
		SelectedIcon.enabled = true;
		base.transform.DOScale(Vector3.one * 1.1f, 0.3f).SetEase(Ease.OutBack);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		SelectedIcon.enabled = false;
		base.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		costParent.SetActive(false);
	}
}
