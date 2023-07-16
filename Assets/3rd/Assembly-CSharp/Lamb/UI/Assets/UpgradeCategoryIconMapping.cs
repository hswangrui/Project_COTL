using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Upgrade Category Icon Mapping", menuName = "Massive Monster/Upgrade Category Icon Mapping", order = 1)]
	public class UpgradeCategoryIconMapping : ScriptableObject
	{
		[Serializable]
		public class UpgradeCategoryColour
		{
			public UpgradeSystem.Category Category;

			public Color Colour;
		}

		[SerializeField]
		private List<UpgradeCategoryColour> _itemImages;

		private Dictionary<UpgradeSystem.Category, Color> _itemMap;

		private void Initialise()
		{
			_itemMap = new Dictionary<UpgradeSystem.Category, Color>();
			foreach (UpgradeCategoryColour itemImage in _itemImages)
			{
				if (!_itemMap.ContainsKey(itemImage.Category))
				{
					_itemMap.Add(itemImage.Category, itemImage.Colour);
				}
			}
		}

		public Color GetColor(UpgradeSystem.Category type)
		{
			if (_itemMap == null)
			{
				Initialise();
			}
			Color value;
			if (_itemMap.TryGetValue(type, out value))
			{
				return value;
			}
			return StaticColors.OffWhiteColor;
		}

		public static string GetIcon(UpgradeSystem.Category category)
		{
			switch (category)
			{
			case UpgradeSystem.Category.ECONOMY:
				return "\uf81d";
			case UpgradeSystem.Category.FOLLOWERS:
				return "\uf683";
			case UpgradeSystem.Category.FAITH:
				return "\uf684";
			case UpgradeSystem.Category.ASTHETIC:
				return "\uf890";
			case UpgradeSystem.Category.DEATH:
				return "\uf714";
			case UpgradeSystem.Category.POOP:
				return "\uf619";
			case UpgradeSystem.Category.FARMING:
				return "\uf864";
			case UpgradeSystem.Category.SLEEP:
				return "\uf236";
			case UpgradeSystem.Category.ILLNESS:
				return "\ue074";
			case UpgradeSystem.Category.P_CURSE:
				return "\uf6b8";
			case UpgradeSystem.Category.P_FERVOR:
				return "\uf7e4";
			case UpgradeSystem.Category.P_HEALTH:
				return "\uf004";
			case UpgradeSystem.Category.COMBAT:
			case UpgradeSystem.Category.P_WEAPON:
				return "\uf71c";
			case UpgradeSystem.Category.P_STRENGTH:
				return "\uf6de";
			default:
				return "";
			}
		}
	}
}
