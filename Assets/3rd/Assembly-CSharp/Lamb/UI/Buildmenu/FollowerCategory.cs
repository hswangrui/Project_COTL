using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI.BuildMenu
{
	public class FollowerCategory : BuildMenuCategory
	{
		public enum Category
		{
			Misc,
			Food,
			Items
		}

		[Header("Content")]
		[SerializeField]
		private RectTransform _miscContent;

		[SerializeField]
		private RectTransform _foodContent;

		[SerializeField]
		private RectTransform _itemsContent;

		[Header("Counts")]
		[SerializeField]
		private TextMeshProUGUI _miscUnlocked;

		[SerializeField]
		private TextMeshProUGUI _foodUnlocked;

		[SerializeField]
		private TextMeshProUGUI _itemsUnlocked;

		protected override void Populate()
		{
			Populate(GetStructuresForCategory(Category.Misc), _miscContent);
			Populate(GetStructuresForCategory(Category.Food), _foodContent);
			Populate(GetStructuresForCategory(Category.Items), _itemsContent);
			SetUnlockedText(_miscUnlocked, Category.Misc);
			SetUnlockedText(_foodUnlocked, Category.Food);
			SetUnlockedText(_itemsUnlocked, Category.Items);
		}

		private void SetUnlockedText(TextMeshProUGUI target, Category category)
		{
			int num = 0;
			int num2 = 0;
			foreach (StructureBrain.TYPES item in GetStructuresForCategory(category))
			{
				if (StructuresData.GetUnlocked(item))
				{
					num2++;
					num++;
				}
				else if (!StructuresData.HiddenUntilUnlocked(item))
				{
					num2++;
				}
			}
			target.text = string.Format(ScriptLocalization.UI.Collected, string.Format("{0}/{1}", num, num2));
		}

		public static List<StructureBrain.TYPES> GetStructuresForCategory(Category category)
		{
			switch (category)
			{
			case Category.Misc:
				return new List<StructureBrain.TYPES>
				{
					StructureBrain.TYPES.BED,
					StructureBrain.TYPES.BED_2,
					StructureBrain.TYPES.BED_3,
					StructureBrain.TYPES.SHARED_HOUSE,
					StructureBrain.TYPES.COOKING_FIRE,
					StructureBrain.TYPES.KITCHEN,
					StructureBrain.TYPES.BODY_PIT,
					StructureBrain.TYPES.GRAVE,
					StructureBrain.TYPES.OUTHOUSE,
					StructureBrain.TYPES.OUTHOUSE_2,
					StructureBrain.TYPES.JANITOR_STATION,
					StructureBrain.TYPES.PRISON,
					StructureBrain.TYPES.PROPAGANDA_SPEAKER,
					StructureBrain.TYPES.DEMON_SUMMONER,
					StructureBrain.TYPES.DEMON_SUMMONER_2,
					StructureBrain.TYPES.DEMON_SUMMONER_3,
					StructureBrain.TYPES.HEALING_BAY,
					StructureBrain.TYPES.HEALING_BAY_2,
					StructureBrain.TYPES.MORGUE_1,
					StructureBrain.TYPES.MORGUE_2,
					StructureBrain.TYPES.CRYPT_1,
					StructureBrain.TYPES.CRYPT_2,
					StructureBrain.TYPES.CRYPT_3
				};
			case Category.Food:
				return new List<StructureBrain.TYPES>
				{
					StructureBrain.TYPES.FARM_PLOT,
					StructureBrain.TYPES.FARM_STATION,
					StructureBrain.TYPES.FARM_STATION_II,
					StructureBrain.TYPES.SCARECROW,
					StructureBrain.TYPES.SCARECROW_2,
					StructureBrain.TYPES.COMPOST_BIN,
					StructureBrain.TYPES.COMPOST_BIN_DEAD_BODY,
					StructureBrain.TYPES.HARVEST_TOTEM,
					StructureBrain.TYPES.HARVEST_TOTEM_2,
					StructureBrain.TYPES.SILO_SEED,
					StructureBrain.TYPES.SILO_FERTILISER
				};
			case Category.Items:
				return new List<StructureBrain.TYPES>
				{
					StructureBrain.TYPES.LUMBERJACK_STATION,
					StructureBrain.TYPES.LUMBERJACK_STATION_2,
					StructureBrain.TYPES.BLOODSTONE_MINE,
					StructureBrain.TYPES.BLOODSTONE_MINE_2,
					StructureBrain.TYPES.REFINERY,
					StructureBrain.TYPES.REFINERY_2
				};
			default:
				return null;
			}
		}

		public static List<StructureBrain.TYPES> AllStructures()
		{
			List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
			list.AddRange(GetStructuresForCategory(Category.Misc));
			list.AddRange(GetStructuresForCategory(Category.Food));
			list.AddRange(GetStructuresForCategory(Category.Items));
			return list;
		}
	}
}
