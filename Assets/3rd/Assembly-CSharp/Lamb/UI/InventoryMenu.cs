using System.Collections.Generic;
using System.Linq;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class InventoryMenu : UISubmenuBase
	{
		[Header("Inventory Menu")]
		[SerializeField]
		private MMScrollRect _scrollRect;

		[Header("Currencies")]
		[SerializeField]
		private RectTransform _currenciesContainer;

		[SerializeField]
		private GameObject _noCurrencyText;

		[Header("Food")]
		[SerializeField]
		private RectTransform _foodContainer;

		[SerializeField]
		private GameObject _noFoodText;

		[Header("Items")]
		[SerializeField]
		private RectTransform _itemsContainer;

		[SerializeField]
		private GameObject _noItemsText;

		[Header("Templates")]
		[SerializeField]
		private GenericInventoryItem _inventoryItemTemplate;

		private List<GenericInventoryItem> _currencies = new List<GenericInventoryItem>();

		private List<GenericInventoryItem> _food = new List<GenericInventoryItem>();

		private List<GenericInventoryItem> _items = new List<GenericInventoryItem>();

		private List<InventoryItem.ITEM_TYPE> _currencyFilter = new List<InventoryItem.ITEM_TYPE>
		{
			InventoryItem.ITEM_TYPE.GOLD_NUGGET,
			InventoryItem.ITEM_TYPE.BLACK_GOLD,
			InventoryItem.ITEM_TYPE.GOLD_REFINED,
			InventoryItem.ITEM_TYPE.LOG,
			InventoryItem.ITEM_TYPE.LOG_REFINED,
			InventoryItem.ITEM_TYPE.STONE,
			InventoryItem.ITEM_TYPE.STONE_REFINED,
			InventoryItem.ITEM_TYPE.BONE,
			InventoryItem.ITEM_TYPE.SHELL,
			InventoryItem.ITEM_TYPE.MONSTER_HEART,
			InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE,
			InventoryItem.ITEM_TYPE.TALISMAN,
			InventoryItem.ITEM_TYPE.BEHOLDER_EYE,
			InventoryItem.ITEM_TYPE.GOD_TEAR_FRAGMENT,
			InventoryItem.ITEM_TYPE.GOD_TEAR
		};

		private List<InventoryItem.ITEM_TYPE> _foodSorter = new List<InventoryItem.ITEM_TYPE>
		{
			InventoryItem.ITEM_TYPE.GRASS,
			InventoryItem.ITEM_TYPE.SEED,
			InventoryItem.ITEM_TYPE.BERRY,
			InventoryItem.ITEM_TYPE.SEED_PUMPKIN,
			InventoryItem.ITEM_TYPE.PUMPKIN,
			InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER,
			InventoryItem.ITEM_TYPE.CAULIFLOWER,
			InventoryItem.ITEM_TYPE.SEED_BEETROOT,
			InventoryItem.ITEM_TYPE.BEETROOT,
			InventoryItem.ITEM_TYPE.POOP,
			InventoryItem.ITEM_TYPE.MEAT_MORSEL,
			InventoryItem.ITEM_TYPE.MEAT,
			InventoryItem.ITEM_TYPE.FOLLOWER_MEAT,
			InventoryItem.ITEM_TYPE.FISH,
			InventoryItem.ITEM_TYPE.FISH_SMALL,
			InventoryItem.ITEM_TYPE.FISH_BIG,
			InventoryItem.ITEM_TYPE.FISH_CRAB,
			InventoryItem.ITEM_TYPE.FISH_LOBSTER,
			InventoryItem.ITEM_TYPE.FISH_OCTOPUS,
			InventoryItem.ITEM_TYPE.FISH_SWORDFISH,
			InventoryItem.ITEM_TYPE.FISH_BLOWFISH,
			InventoryItem.ITEM_TYPE.FISH_SQUID
		};

		private List<InventoryItem.ITEM_TYPE> _itemsSorter = new List<InventoryItem.ITEM_TYPE>
		{
			InventoryItem.ITEM_TYPE.SEED_FLOWER_RED,
			InventoryItem.ITEM_TYPE.FLOWER_RED,
			InventoryItem.ITEM_TYPE.SEED_MUSHROOM,
			InventoryItem.ITEM_TYPE.MUSHROOM_SMALL,
			InventoryItem.ITEM_TYPE.CRYSTAL,
			InventoryItem.ITEM_TYPE.SPIDER_WEB,
			InventoryItem.ITEM_TYPE.Necklace_1,
			InventoryItem.ITEM_TYPE.Necklace_2,
			InventoryItem.ITEM_TYPE.Necklace_3,
			InventoryItem.ITEM_TYPE.Necklace_4,
			InventoryItem.ITEM_TYPE.Necklace_5,
			InventoryItem.ITEM_TYPE.Necklace_Demonic,
			InventoryItem.ITEM_TYPE.Necklace_Loyalty,
			InventoryItem.ITEM_TYPE.Necklace_Missionary,
			InventoryItem.ITEM_TYPE.Necklace_Gold_Skull,
			InventoryItem.ITEM_TYPE.Necklace_Light,
			InventoryItem.ITEM_TYPE.Necklace_Dark,
			InventoryItem.ITEM_TYPE.GIFT_SMALL,
			InventoryItem.ITEM_TYPE.GIFT_MEDIUM
		};

		protected override void OnShowStarted()
		{
			_scrollRect.enabled = false;
			if (_items.Count + _food.Count + _currencies.Count == 0)
			{
				Populate(Inventory.items, _currencies, _currenciesContainer, _noCurrencyText, _currencyFilter, null, _currencyFilter);
				Populate(Inventory.items, _food, _foodContainer, _noFoodText, _foodSorter, null, _foodSorter);
				Populate(Inventory.items, _items, _itemsContainer, _noItemsText, _itemsSorter, null, _itemsSorter);
				if (_currencies.Count > 0)
				{
					OverrideDefault(_currencies[0].Button);
				}
				else if (_food.Count > 0)
				{
					OverrideDefault(_food[0].Button);
				}
				else if (_items.Count > 0)
				{
					OverrideDefault(_items[0].Button);
				}
				ActivateNavigation();
			}
			_scrollRect.enabled = true;
			_scrollRect.normalizedPosition = Vector2.one;
		}

		private void Populate(List<InventoryItem> items, List<GenericInventoryItem> destination, RectTransform container, GameObject noText, List<InventoryItem.ITEM_TYPE> filter = null, List<InventoryItem.ITEM_TYPE> ignore = null, List<InventoryItem.ITEM_TYPE> sorting = null)
		{
			List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
			foreach (InventoryItem item in items)
			{
				if ((filter == null || filter.Contains((InventoryItem.ITEM_TYPE)item.type)) && (ignore == null || !ignore.Contains((InventoryItem.ITEM_TYPE)item.type)))
				{
					list.Add((InventoryItem.ITEM_TYPE)item.type);
				}
			}
			if (sorting != null)
			{
				list = list.OrderBy((InventoryItem.ITEM_TYPE x) => sorting.IndexOf(x)).ToList();
			}
			foreach (InventoryItem.ITEM_TYPE item2 in list)
			{
				MakeItem(item2, container, destination);
			}
			noText.SetActive(destination.Count == 0);
		}

		private GenericInventoryItem MakeItem(InventoryItem.ITEM_TYPE itemType, Transform container, List<GenericInventoryItem> destination)
		{
			GenericInventoryItem genericInventoryItem = GameObjectExtensions.Instantiate(_inventoryItemTemplate, container);
			genericInventoryItem.Button.Confirmable = false;
			genericInventoryItem.Configure(itemType);
			destination.Add(genericInventoryItem);
			return genericInventoryItem;
		}
	}
}
