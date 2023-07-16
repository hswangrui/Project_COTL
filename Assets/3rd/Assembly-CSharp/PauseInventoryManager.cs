using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseInventoryManager : BaseMonoBehaviour
{
	public UI_NavigatorSimple UINav;

	public GameObject InventoryItemObject;

	public GameObject InventoryItemParent;

	public TextMeshProUGUI Title;

	public Image FeaturedIcon;

	public TextMeshProUGUI Description;

	public TextMeshProUGUI Lore;

	public InventoryItemDisplay ItemDisplayReference;

	public CanvasGroup canvas;

	public List<InventoryItem.ITEM_TYPE> Blacklist = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.SEEDS,
		InventoryItem.ITEM_TYPE.INGREDIENTS,
		InventoryItem.ITEM_TYPE.MEALS
	};

	private List<GameObject> Icons = new List<GameObject>();

	private bool WhiteListItem;

	private bool CanvasDisabled;

	private void OnChangeSelectionUnity(Selectable NewSelectable, Selectable PrevSelectable)
	{
		if (NewSelectable != null)
		{
			if (NewSelectable.GetComponent<PauseInventoryItem>().Type != 0)
			{
				InventoryItem.ITEM_TYPE type = NewSelectable.GetComponent<PauseInventoryItem>().Type;
				FeaturedIcon.enabled = true;
				Title.text = InventoryItem.LocalizedName(type);
				Description.text = InventoryItem.LocalizedDescription(type);
				Lore.text = InventoryItem.LocalizedLore(type);
				FeaturedIcon.sprite = ItemDisplayReference.GetImage(type);
			}
			else
			{
				Title.text = "No inventory items";
				Description.text = "";
				Lore.text = "";
				FeaturedIcon.enabled = false;
			}
		}
	}

	private void NoItems()
	{
		Title.text = "No inventory items";
		Description.text = "";
		Lore.text = "";
		FeaturedIcon.enabled = false;
	}

	private bool CheckOnBlacklist(InventoryItem.ITEM_TYPE type)
	{
		bool flag = false;
		foreach (InventoryItem.ITEM_TYPE item in Blacklist)
		{
			if (type == item)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return true;
		}
		return false;
	}

	private void OnEnable()
	{
		foreach (GameObject icon in Icons)
		{
			UnityEngine.Object.Destroy(icon);
		}
		InventoryItemObject.SetActive(false);
		UI_NavigatorSimple uINav = UINav;
		uINav.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Combine(uINav.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelectionUnity));
		Icons.Clear();
		WhiteListItem = true;
		List<InventoryItem> items = Inventory.items;
		if (items.Count == 0)
		{
			NoItems();
			return;
		}
		foreach (InventoryItem item in items)
		{
			if (!CheckOnBlacklist((InventoryItem.ITEM_TYPE)item.type))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(InventoryItemObject, InventoryItemParent.transform);
				gameObject.SetActive(true);
				gameObject.GetComponent<PauseInventoryItem>().Init((InventoryItem.ITEM_TYPE)item.type, item.quantity);
				Icons.Add(gameObject);
			}
		}
		while (Icons.Count < 21)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(InventoryItemObject, InventoryItemParent.transform);
			gameObject2.SetActive(true);
			gameObject2.GetComponent<PauseInventoryItem>().Init(InventoryItem.ITEM_TYPE.NONE, 0);
			Icons.Add(gameObject2);
		}
		Debug.Log(Icons[0]);
		Icons[0].GetComponent<Selectable>().Select();
		OnChangeSelectionUnity(Icons[0].GetComponent<Selectable>(), null);
	}

	private void OnDisable()
	{
		UI_NavigatorSimple uINav = UINav;
		uINav.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Remove(uINav.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelectionUnity));
	}

	private void Start()
	{
	}

	private void SetDefault()
	{
		if (Icons.Count > 0)
		{
			Debug.Log("Set Default: " + Icons[0]);
			UINav.selectable = Icons[0].GetComponent<Selectable>();
			Icons[0].GetComponent<Selectable>().Select();
		}
	}

	private void Update()
	{
		if (!canvas.interactable && !CanvasDisabled)
		{
			CanvasDisabled = true;
		}
		else if (CanvasDisabled && canvas.interactable)
		{
			SetDefault();
			CanvasDisabled = false;
		}
	}
}
