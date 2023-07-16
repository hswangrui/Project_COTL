using System.Collections.Generic;
using UnityEngine;

public class shopKeeperManager : BaseMonoBehaviour
{
	public bool DailyShop;

	public FollowerLocation Location;

	private bool gotOne;

	public GameObject[] itemSlots;

	public List<BuyEntry> ItemsForSale;

	private bool containsInt;

	private int randomInt;

	public bool DecorationsForSale;

	public bool TarotCardShop;

	public bool AddCoolDownToNewItems;

	public GameObject SoldOutSign;

	private GameObject SoldOutSignObject;

	public GameObject BoughtItemBark;

	public GameObject NormalBark;

	public GameObject CantAffordBark;

	private List<StructureBrain.TYPES> availableUnlocks = new List<StructureBrain.TYPES>();

	private List<int> PickedItems = new List<int>();

	private bool FoundOne;

	private int LoopCount;

	[SerializeField]
	private Vector3 SoldOutOffset = Vector3.zero;

	public ShopLocationTracker shop { get; set; }

	private void Start()
	{
		if (TarotCardShop)
		{
			InitTarotShop();
		}
		else if (!DecorationsForSale && !DailyShop)
		{
			InitShop();
		}
		else if (DecorationsForSale)
		{
			GetAvailableDecorations();
		}
		else
		{
			InitDailyShop();
		}
		if ((bool)BoughtItemBark)
		{
			BoughtItemBark.SetActive(false);
		}
		if ((bool)CantAffordBark)
		{
			CantAffordBark.SetActive(false);
		}
	}

	private void GetAvailableDecorations()
	{
		Debug.Log("Init Decoration Shop");
		availableUnlocks.Clear();
		availableUnlocks = new List<StructureBrain.TYPES>();
		Debug.Log("Player Farming Location: " + Location);
		foreach (StructureBrain.TYPES item in DataManager.Instance.GetDecorationListFromLocation(Location))
		{
			availableUnlocks.Add(item);
		}
		if (DataManager.Instance.GetDecorationListFromCategory(DataManager.DecorationType.All).Count < 3)
		{
			foreach (StructureBrain.TYPES item2 in DataManager.Instance.GetDecorationListFromCategory(DataManager.DecorationType.All))
			{
				availableUnlocks.Add(item2);
			}
		}
		InitShopDecoration();
	}

	private bool ItemAlreadyForSale(StructureBrain.TYPES type)
	{
		GameObject[] array = itemSlots;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetComponent<Interaction_BuyItem>().itemForSale.decorationToBuy == type)
			{
				return true;
			}
		}
		return false;
	}

	private void InitDailyShop()
	{
		Debug.Log("Init Daily Shop");
		if (!DataManager.Instance.CheckShopExists(Location, base.gameObject.name))
		{
			shop = new ShopLocationTracker(Location);
			DataManager.Instance.Shops.Add(shop);
			Debug.Log("Create Shop");
			shop.lastDayUsed = -1;
			shop.shopKeeperName = base.gameObject.name;
		}
		else
		{
			shop = DataManager.Instance.GetShop(Location, base.gameObject.name);
		}
		if (shop.lastDayUsed != TimeManager.CurrentDay)
		{
			Debug.Log("Create Shop items");
			shop.itemsForSale.Clear();
			for (int i = 0; i < itemSlots.Length; i++)
			{
				PickedItems.Clear();
				Interaction_BuyItem component = itemSlots[i].GetComponent<Interaction_BuyItem>();
				InventoryItemDisplay component2 = itemSlots[i].GetComponent<InventoryItemDisplay>();
				gotOne = false;
				int num = 0;
				while (!gotOne)
				{
					num++;
					if (num > 30)
					{
						Debug.Log("Cant Find Item for Sale that isnt picked");
						if (SoldOutSign != null)
						{
							SoldOutSignObject = Object.Instantiate(SoldOutSign, component.transform.position + SoldOutOffset, new Quaternion(0f, 0f, 0f, 0f), base.gameObject.transform);
							Object.Destroy(component.gameObject);
						}
						return;
					}
					randomInt = Random.Range(0, ItemsForSale.Count);
					if (!ItemsForSale[randomInt].pickedForSale)
					{
						Debug.Log("Found False");
						component.itemForSale = ItemsForSale[randomInt];
						component.GetCost();
						ItemsForSale[randomInt].pickedForSale = true;
						if (ItemsForSale[randomInt].quantity == 0)
						{
							ItemsForSale[randomInt].quantity = 1;
						}
						if (component2 != null && ItemsForSale[i].itemToBuy != 0)
						{
							component2.SetImage(ItemsForSale[randomInt].itemToBuy);
						}
						component.itemForSale.quantity = ItemsForSale[randomInt].quantity;
						if (component.itemForSale.quantity > 1)
						{
							component.updateQuantity();
						}
						shop.itemsForSale.Add(component.itemForSale);
						component.shopKeeperManager = this;
						gotOne = true;
					}
				}
			}
			shop.lastDayUsed = TimeManager.CurrentDay;
			DataManager.Instance.UpdateShop(shop);
			return;
		}
		Debug.Log("Shop Exists, amount of items: " + shop.itemsForSale.Count);
		for (int j = 0; j < itemSlots.Length; j++)
		{
			if (!shop.itemsForSale[j].Bought)
			{
				InventoryItemDisplay component3 = itemSlots[j].GetComponent<InventoryItemDisplay>();
				Interaction_BuyItem component4 = itemSlots[j].GetComponent<Interaction_BuyItem>();
				component4.itemForSale = new BuyEntry(shop.itemsForSale[j].itemToBuy, shop.itemsForSale[j].costType, shop.itemsForSale[j].itemCost, shop.itemsForSale[j].quantity);
				Debug.Log("BUY ITEM: " + component4.itemForSale.decorationToBuy);
				component4.GetCost();
				component4.shopKeeperManager = this;
				component3.SetImage(shop.itemsForSale[j].itemToBuy);
				component4.updateQuantity();
			}
			else
			{
				itemSlots[j].SetActive(false);
				if (SoldOutSign != null)
				{
					SoldOutSignObject = Object.Instantiate(SoldOutSign, itemSlots[j].transform.position + SoldOutOffset, new Quaternion(0f, 0f, 0f, 0f), base.gameObject.transform);
				}
			}
		}
	}

	private void CreateSoldOutSign(GameObject buyItem)
	{
		if (!(SoldOutSign == null))
		{
			SoldOutSignObject = Object.Instantiate(SoldOutSign, buyItem.transform.position + SoldOutOffset, new Quaternion(0f, 0f, 0f, 0f), base.gameObject.transform);
			Object.Destroy(buyItem.gameObject);
		}
	}

	private void InitShopDecoration()
	{
		if (!DataManager.Instance.CheckShopExists(Location, base.gameObject.name))
		{
			shop = new ShopLocationTracker(Location);
			DataManager.Instance.Shops.Add(shop);
			Debug.Log("Create Shop");
			shop.lastDayUsed = -1;
			shop.shopKeeperName = base.gameObject.name;
		}
		else
		{
			shop = DataManager.Instance.GetShop(Location, base.gameObject.name);
		}
		if (shop.lastDayUsed != TimeManager.CurrentDay)
		{
			Debug.Log("Init Dec Shop: Havent picked items for the day");
			shop.itemsForSale.Clear();
			PickedItems.Clear();
			LoopCount = itemSlots.Length;
			int num = availableUnlocks.Count;
			for (int i = 0; i < LoopCount; i++)
			{
				Interaction_BuyItem component = itemSlots[i].GetComponent<Interaction_BuyItem>();
				Debug.Log("Unlocks Count: " + availableUnlocks.Count);
				Debug.Log("Unlocks Count: " + LoopCount);
				if (num > 0)
				{
					FoundOne = false;
					int num2 = 0;
					while (!FoundOne)
					{
						num2++;
						if (num2 > 60)
						{
							CreateSoldOutSign(component.gameObject);
							Debug.Log("Cant Find Item for Sale that isnt picked");
							break;
						}
						randomInt = Random.Range(0, availableUnlocks.Count);
						Debug.Log("Random Int = " + randomInt);
						Debug.Log("Available Unlocks Count = " + availableUnlocks.Count);
						if (!DataManager.Instance.UnlockedStructures.Contains(availableUnlocks[randomInt]))
						{
							FoundOne = true;
						}
						else
						{
							FoundOne = false;
						}
						if (PickedItems.Count > 0 && PickedItems.Contains(randomInt))
						{
							FoundOne = false;
						}
					}
					if (FoundOne)
					{
						PickedItems.Add(randomInt);
						component.itemForSale.Decoration = true;
						DataManager.DecorationType decorationType = DataManager.GetDecorationType(availableUnlocks[randomInt]);
						component.itemForSale = new BuyEntry(availableUnlocks[randomInt], DataManager.GetDecorationTypeCost(decorationType).costType, DataManager.GetDecorationTypeCost(decorationType).costAmount);
						Debug.Log("BUY ITEM: " + component.itemForSale.decorationToBuy);
						component.GetCost();
						component.GetDecoration();
						if (component.itemForSale.quantity == 0)
						{
							component.itemForSale.quantity = 1;
						}
						shop.itemsForSale.Add(component.itemForSale);
						shop.lastDayUsed = TimeManager.CurrentDay;
						num--;
					}
				}
				else
				{
					CreateSoldOutSign(component.gameObject);
				}
			}
			return;
		}
		Debug.Log("Init Dec Shop: Picked items for the day");
		for (int j = 0; j < itemSlots.Length; j++)
		{
			if (j < shop.itemsForSale.Count && !shop.itemsForSale[j].Bought)
			{
				Interaction_BuyItem component2 = itemSlots[j].GetComponent<Interaction_BuyItem>();
				component2.itemForSale.Decoration = true;
				component2.itemForSale = new BuyEntry(shop.itemsForSale[j].decorationToBuy, shop.itemsForSale[j].costType, shop.itemsForSale[j].itemCost);
				Debug.Log("BUY ITEM: " + component2.itemForSale.decorationToBuy);
				component2.GetCost();
				component2.GetDecoration();
			}
			else
			{
				itemSlots[j].SetActive(false);
				if (SoldOutSign != null)
				{
					itemSlots[j].gameObject.SetActive(false);
					SoldOutSignObject = Object.Instantiate(SoldOutSign, itemSlots[j].transform.position + SoldOutOffset, new Quaternion(0f, 0f, 0f, 0f), base.gameObject.transform);
				}
			}
		}
	}

	private void InitTarotShop()
	{
		Debug.Log("Init Tarot Shop");
		for (int i = 0; i < itemSlots.Length; i++)
		{
			if (itemSlots[i] == null)
			{
				continue;
			}
			Interaction_BuyItem component = itemSlots[i].GetComponent<Interaction_BuyItem>();
			InventoryItemDisplay component2 = itemSlots[i].GetComponent<InventoryItemDisplay>();
			if (!DataManager.TrinketUnlocked(ItemsForSale[i].Card))
			{
				component.itemForSale.Card = ItemsForSale[i].Card;
				component.itemForSale = ItemsForSale[i];
				component.GetCost();
				ItemsForSale[i].pickedForSale = true;
				if (ItemsForSale[randomInt].quantity == 0)
				{
					ItemsForSale[randomInt].quantity = 1;
				}
				if (component2 != null && ItemsForSale[i].itemToBuy != 0)
				{
					component2.SetImage(ItemsForSale[i].itemToBuy);
				}
				component.itemForSale.quantity = ItemsForSale[i].quantity;
				if (component.itemForSale.quantity > 1)
				{
					component.updateQuantity();
				}
			}
			else
			{
				component.AlreadyBought();
				if (SoldOutSign != null)
				{
					component.gameObject.SetActive(false);
					SoldOutSignObject = Object.Instantiate(SoldOutSign, component.transform.position + SoldOutOffset, new Quaternion(0f, 0f, 0f, 0f), base.gameObject.transform);
				}
			}
		}
	}

	public void DoublePrices()
	{
		GameObject[] array = itemSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<Interaction_BuyItem>().DoublePrice = true;
		}
	}

	public void InitShop()
	{
		Debug.Log("Init Normal Shop");
		for (int i = 0; i < itemSlots.Length; i++)
		{
			Interaction_BuyItem component = itemSlots[i].GetComponent<Interaction_BuyItem>();
			InventoryItemDisplay component2 = itemSlots[i].GetComponent<InventoryItemDisplay>();
			gotOne = false;
			int num = 0;
			while (!gotOne)
			{
				num++;
				if (num > 30)
				{
					Debug.Log("Cant Find Item for Sale that isnt picked");
					break;
				}
				randomInt = Random.Range(0, ItemsForSale.Count);
				if (!ItemsForSale[randomInt].pickedForSale)
				{
					Debug.Log("Found False");
					component.itemForSale = ItemsForSale[randomInt];
					component.GetCost();
					ItemsForSale[randomInt].pickedForSale = true;
					if (ItemsForSale[randomInt].quantity == 0)
					{
						ItemsForSale[randomInt].quantity = 1;
					}
					if (component2 != null && ItemsForSale[i].itemToBuy != 0)
					{
						component2.SetImage(ItemsForSale[randomInt].itemToBuy);
					}
					component.itemForSale.quantity = ItemsForSale[randomInt].quantity;
					if (component.itemForSale.quantity > 1)
					{
						component.updateQuantity();
					}
					gotOne = true;
				}
			}
		}
	}
}
