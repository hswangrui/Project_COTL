using System.Collections.Generic;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using MMBiomeGeneration;
using src.Extensions;
using Unify;
using UnityEngine;

public class Inventory : BaseMonoBehaviour
{
	public delegate void InventoryUpdated();

	public delegate void GetFollowerToken();

	public delegate void ItemAddedToInventory(InventoryItem.ITEM_TYPE ItemType, int Delta);

	public delegate void ItemAddedToDungeonInventory(InventoryItem.ITEM_TYPE ItemType);

	public static InventoryUpdated OnInventoryUpdated;

	public static List<InventoryWeapon> weapons = new List<InventoryWeapon>
	{
		new InventoryWeapon(InventoryWeapon.ITEM_TYPE.SWORD, 100),
		new InventoryWeapon(InventoryWeapon.ITEM_TYPE.SEED_BAG, 100)
	};

	public static GetFollowerToken OnGetFollowerToken;

	public static int KeyPieces
	{
		get
		{
			return DataManager.Instance.CurrentKeyPieces;
		}
		set
		{
			DataManager.Instance.CurrentKeyPieces = value;
			if (DataManager.Instance.CurrentKeyPieces >= 4)
			{
				DataManager.Instance.CurrentKeyPieces = 0;
				AddItem(InventoryItem.ITEM_TYPE.TALISMAN, 1);
			}
		}
	}

	public static int TempleKeys
	{
		get
		{
			return GetItemQuantity(InventoryItem.ITEM_TYPE.TALISMAN);
		}
	}

	public static List<InventoryItem> itemsDungeon
	{
		get
		{
			return DataManager.Instance.itemsDungeon;
		}
		set
		{
			DataManager.Instance.itemsDungeon = value;
		}
	}

	public static List<InventoryItem> items
	{
		get
		{
			return DataManager.Instance.items;
		}
		set
		{
			DataManager.Instance.items = value;
		}
	}

	public static int CURRENT_WEAPON
	{
		get
		{
			return DataManager.Instance.CURRENT_WEAPON;
		}
		set
		{
			DataManager.Instance.CURRENT_WEAPON = value;
		}
	}

	public static int Souls
	{
		get
		{
			return DataManager.Instance.Souls;
		}
		set
		{
			DataManager.Instance.Souls = value;
		}
	}

	public static int BlackSouls
	{
		get
		{
			return DataManager.Instance.BlackSouls;
		}
		set
		{
			DataManager.Instance.BlackSouls = value;
		}
	}

	public static int FollowerTokens
	{
		get
		{
			return DataManager.Instance.FollowerTokens;
		}
		set
		{
			DataManager.Instance.FollowerTokens = value;
			GetFollowerToken onGetFollowerToken = OnGetFollowerToken;
			if (onGetFollowerToken != null)
			{
				onGetFollowerToken();
			}
		}
	}

	public static List<InventoryItem> Food
	{
		get
		{
			return DataManager.Instance.Food;
		}
	}

	public static event ItemAddedToInventory OnItemAddedToInventory;

	public static event ItemAddedToInventory OnItemRemovedFromInventory;

	public static event ItemAddedToDungeonInventory OnItemAddedToDungeonInventory;

	public static int TotalItems()
	{
		int num = 0;
		foreach (InventoryItem item in items)
		{
			num += item.quantity;
		}
		return num;
	}

	public static void AddItemDungeon(int type, int quantity)
	{
		if (quantity > 0)
		{
			InventoryItem dungeonItemByType = GetDungeonItemByType(type);
			if (dungeonItemByType != null)
			{
				dungeonItemByType.quantity += quantity;
			}
			else
			{
				InventoryItem inventoryItem = new InventoryItem();
				inventoryItem.Init(type, quantity);
				itemsDungeon.Add(inventoryItem);
			}
			if (Inventory.OnItemAddedToDungeonInventory != null)
			{
				Inventory.OnItemAddedToDungeonInventory((InventoryItem.ITEM_TYPE)type);
			}
			if (!DataManager.Instance.ShownInventoryTutorial && !UIInventoryPromptOverlay.Showing)
			{
				MonoSingleton<UIManager>.Instance.InventoryPromptTemplate.Instantiate();
			}
		}
	}

	public static void ClearDungeonItems(bool includeBlacklist = true)
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>
		{
			InventoryItem.ITEM_TYPE.BEHOLDER_EYE,
			InventoryItem.ITEM_TYPE.MONSTER_HEART
		};
		if (includeBlacklist)
		{
			itemsDungeon.Clear();
			return;
		}
		for (int num = itemsDungeon.Count - 1; num >= 0; num--)
		{
			if (!list.Contains((InventoryItem.ITEM_TYPE)itemsDungeon[num].type))
			{
				itemsDungeon.RemoveAt(num);
			}
		}
	}

	public static void RemoveDungeonItemsFromInventory()
	{
		for (int num = itemsDungeon.Count - 1; num >= 0; num--)
		{
			if (!UIDeathScreenOverlayController.ExcludeLootFromBonus.Contains((InventoryItem.ITEM_TYPE)itemsDungeon[num].type))
			{
				InventoryItem itemByType = GetItemByType(itemsDungeon[num].type);
				if (itemByType != null)
				{
					itemByType.quantity -= itemsDungeon[num].quantity;
				}
				if (Inventory.OnItemAddedToInventory != null)
				{
					Inventory.OnItemAddedToInventory((InventoryItem.ITEM_TYPE)itemsDungeon[num].type, -itemsDungeon[num].quantity);
				}
				itemsDungeon.RemoveAt(num);
			}
		}
		if (OnInventoryUpdated != null)
		{
			OnInventoryUpdated();
		}
	}

	public static void AddItem(InventoryItem.ITEM_TYPE type, int quantity, bool forceNormalInventory = false)
	{
		AddItem((int)type, quantity, forceNormalInventory);
	}

	public static void AddItem(int type, int quantity, bool ForceNormalInventory = false)
	{
		if (quantity > 0)
		{
			if (BiomeGenerator.Instance != null && !ForceNormalInventory)
			{
				AddItemDungeon(type, quantity);
			}
			InventoryItem inventoryItem = GetItemByType(type);
			if (inventoryItem != null)
			{
				inventoryItem.quantity += quantity;
			}
			else
			{
				inventoryItem = new InventoryItem();
				inventoryItem.Init(type, quantity);
				items.Add(inventoryItem);
			}
			if (inventoryItem != null && type != 20 && inventoryItem.quantity > 9999)
			{
				inventoryItem.quantity = 9999;
			}
			if (Inventory.OnItemAddedToInventory != null)
			{
				Inventory.OnItemAddedToInventory((InventoryItem.ITEM_TYPE)type, quantity);
			}
			if (OnInventoryUpdated != null)
			{
				OnInventoryUpdated();
			}
			if (type == 20 && GetItemQuantity(20) >= 666)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("666_GOLD"));
			}
		}
	}

	public static void ChangeItemQuantity(InventoryItem.ITEM_TYPE type, int quantity, int reserved = 0)
	{
		ChangeItemQuantity((int)type, quantity, reserved);
	}

	public static void ChangeItemQuantity(int type, int quantity, int reserved = 0)
	{
		if (quantity > 0)
		{
			AddItem(type, quantity);
			return;
		}
		switch (type)
		{
		case 85:
			return;
		case 116:
			if (PlayerDoctrineStone.Instance != null)
			{
				PlayerDoctrineStone.Instance.CompletedDoctrineStones += quantity;
			}
			else
			{
				DataManager.Instance.CompletedDoctrineStones += quantity;
			}
			return;
		case 10:
			if (PlayerFarming.Instance != null)
			{
				PlayerFarming.Instance.GetSoul(quantity);
			}
			else
			{
				Souls += quantity;
			}
			return;
		case 30:
			if (PlayerFarming.Instance != null)
			{
				PlayerFarming.Instance.GetBlackSoul(quantity, false);
			}
			else
			{
				BlackSouls += quantity;
			}
			return;
		}
		InventoryItem itemByType = GetItemByType(type);
		if (itemByType != null)
		{
			itemByType.quantity += quantity;
			itemByType.QuantityReserved += reserved;
			CheckQuantities();
		}
		ItemAddedToInventory onItemRemovedFromInventory = Inventory.OnItemRemovedFromInventory;
		if (onItemRemovedFromInventory != null)
		{
			onItemRemovedFromInventory((InventoryItem.ITEM_TYPE)type, quantity);
		}
		if (OnInventoryUpdated != null)
		{
			OnInventoryUpdated();
		}
	}

	public static void RemoveAll()
	{
		items = new List<InventoryItem>();
	}

	public static void SetItemQuantity(int type, int quantity)
	{
		InventoryItem itemByType = GetItemByType(type);
		if (itemByType != null)
		{
			itemByType.quantity = quantity;
			CheckQuantities();
		}
	}

	private static void CheckQuantities()
	{
		for (int i = 0; i < items.Count; i++)
		{
			InventoryItem inventoryItem = items[i];
			if (inventoryItem.quantity <= 0)
			{
				Debug.Log("remove inventory items");
				items.Remove(inventoryItem);
			}
		}
	}

	public static bool CheckCapacityFull(InventoryItem.ITEM_TYPE type)
	{
		switch (type)
		{
		case InventoryItem.ITEM_TYPE.LOG:
			if (GetItemQuantity(1) < DataManager.LogCapacity[DataManager.Instance.LogCapacityLevel])
			{
				return true;
			}
			return false;
		case InventoryItem.ITEM_TYPE.STONE:
			if (GetItemQuantity(2) < DataManager.StoneCapacity[DataManager.Instance.StoneCapacityLevel])
			{
				return true;
			}
			return false;
		case InventoryItem.ITEM_TYPE.MEALS:
			if (GetItemQuantity(67) < DataManager.FoodCapacity[DataManager.Instance.FoodCapacityLevel])
			{
				return true;
			}
			return false;
		case InventoryItem.ITEM_TYPE.INGREDIENTS:
			if (GetItemQuantity(68) < DataManager.IngredientsCapacity[DataManager.Instance.IngredientsCapacityLevel])
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public static float GetResourceCapacity(InventoryItem.ITEM_TYPE type)
	{
		switch (type)
		{
		case InventoryItem.ITEM_TYPE.LOG:
			return (float)GetItemQuantity(1) / (float)DataManager.LogCapacity[DataManager.Instance.LogCapacityLevel];
		case InventoryItem.ITEM_TYPE.STONE:
			return (float)GetItemQuantity(2) / (float)DataManager.StoneCapacity[DataManager.Instance.StoneCapacityLevel];
		case InventoryItem.ITEM_TYPE.INGREDIENTS:
			return (float)GetItemQuantity(68) / (float)DataManager.IngredientsCapacity[DataManager.Instance.IngredientsCapacityLevel];
		case InventoryItem.ITEM_TYPE.MEALS:
			return (float)GetItemQuantity(67) / (float)DataManager.FoodCapacity[DataManager.Instance.FoodCapacityLevel];
		default:
			return -1f;
		}
	}

	public static int GetItemQuantity(InventoryItem.ITEM_TYPE itemType)
	{
		return GetItemQuantity((int)itemType);
	}

	public static int GetItemQuantities(List<InventoryItem.ITEM_TYPE> items)
	{
		return GetItemQuantities(items.ToArray());
	}

	public static int GetItemQuantities(params InventoryItem.ITEM_TYPE[] items)
	{
		int num = 0;
		foreach (InventoryItem.ITEM_TYPE itemType in items)
		{
			num += GetItemQuantity(itemType);
		}
		return num;
	}

	public static int GetItemQuantity(int type)
	{
		switch (type)
		{
		case 85:
			return DataManager.Instance.Followers.Count;
		case 104:
			return UpgradeSystem.DisciplePoints;
		case 10:
			return Souls;
		case 30:
			return BlackSouls;
		case 116:
			return DataManager.Instance.CompletedDoctrineStones;
		default:
			foreach (InventoryItem item in items)
			{
				if (item.type == type)
				{
					return item.quantity;
				}
			}
			return 0;
		}
	}

	public static bool HasGift()
	{
		if (GetItemQuantity(45) <= 0 && GetItemQuantity(46) <= 0 && GetItemQuantity(47) <= 0 && GetItemQuantity(48) <= 0 && GetItemQuantity(49) <= 0 && GetItemQuantity(43) <= 0 && GetItemQuantity(124) <= 0 && GetItemQuantity(123) <= 0 && GetItemQuantity(122) <= 0 && GetItemQuantity(125) <= 0 && GetItemQuantity(126) <= 0 && GetItemQuantity(44) <= 0)
		{
			return GetItemQuantity(127) > 0;
		}
		return true;
	}

	public static InventoryItem GetItemByType(InventoryItem.ITEM_TYPE itemType)
	{
		return GetItemByType((int)itemType);
	}

	public static InventoryItem GetItemByType(int type)
	{
		foreach (InventoryItem item in items)
		{
			if (item.type == type)
			{
				return item;
			}
		}
		return null;
	}

	public static InventoryItem GetDungeonItemByType(int type)
	{
		foreach (InventoryItem item in itemsDungeon)
		{
			if (item.type == type)
			{
				return item;
			}
		}
		return null;
	}

	public static InventoryItem GetDungeonItemByTypeReturnObject(int type)
	{
		foreach (InventoryItem item in itemsDungeon)
		{
			if (item.type == type)
			{
				return item;
			}
		}
		return new InventoryItem
		{
			type = type,
			quantity = 0
		};
	}

	public static InventoryItem[] GetItemsByCategory(InventoryItem.ITEM_CATEGORIES category)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (InventoryItem item in items)
		{
			if (InventoryItem.GetItemCategory((InventoryItem.ITEM_TYPE)item.type) == category)
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public static int GetFoodAmount()
	{
		int num = 0;
		foreach (InventoryItem item in items)
		{
			CookingData.IngredientType ingredientType = CookingData.GetIngredientType((InventoryItem.ITEM_TYPE)item.type);
			if (CookingData.GetIngredientCategory(ingredientType) != 0 && CookingData.GetIngredientCategory(ingredientType) != CookingData.IngredientType.SPECIAL)
			{
				num += item.quantity;
			}
		}
		return num;
	}
}
