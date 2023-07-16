using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lamb.UI
{
	public sealed class ItemSelector
	{
		public enum Context
		{
			Give,
			Sell,
			Buy,
			Plant,
			Add,
			SetLabel
		}

		public struct Params
		{
			public string Key;

			public Context Context;

			public bool HideOnSelection;

			public Vector2 Offset;

			public bool HideQuantity;

			public bool ShowEmpty;

			public bool RequiresDiscovery;

			public bool PreventCancellation;

			public bool ShowCoins;
		}

		[Serializable]
		public sealed class Category
		{
			public string Key;

			public List<InventoryItem.ITEM_TYPE> TrackedItems = new List<InventoryItem.ITEM_TYPE>();

			public InventoryItem.ITEM_TYPE MostRecentItem;
		}

		public const string kFishermanKey = "fisherman";

		public const string kSellItemsKey = "sell";

		public const string kPlantSeedsKey = "plant_seeds";

		public const string kStoreSeedsKey = "store_seeds";

		public const string kStoreFoodsKey = "store_foods";

		public const string kAddFuelKey = "addfuel";

		public const string kTheNightFishKey = "thenight_fish";

		public const string kTheNightHeartKey = "thenight_heart";

		public const string kSeedShopKey = "seed_shop";

		public const string kFarmPlotKey = "farm_plot";

		public const string kFertiliserKey = "fertiliser";

		private ItemSelector()
		{
		}
	}
}
