using System;
using System.Collections.Generic;
using I2.Loc;

public class WeaponUpgradeSystem
{
	public enum WeaponType
	{
		Sword,
		Dagger,
		Axe,
		Blunderbuss,
		Hammer
	}

	public enum WeaponUpgradeType
	{
		Level_1_A,
		Level_1_B,
		Level_2_A,
		Level_2_B,
		Level_2_C,
		Level_3,
		Level_4_A,
		Level_4_B,
		Level_5_A,
		Level_5_B,
		Level_6
	}

	[Serializable]
	public struct RequiredResources
	{
		public InventoryItem.ITEM_TYPE ItemType;

		public int Quantity;
	}

	public static List<string> UnlockedUpgrades
	{
		get
		{
			return DataManager.Instance.WeaponUnlockedUpgrades;
		}
		set
		{
			DataManager.Instance.WeaponUnlockedUpgrades = value;
		}
	}

	public static string GetLocalizedName(WeaponUpgradeType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("WeaponUpgradeSystem/{0}/Name", Type));
	}

	public static string GetLocalizedDescription(WeaponUpgradeType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("WeaponUpgradeSystem/{0}/Description", Type));
	}

	public static string GetLocalizedName(WeaponType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("WeaponUpgradeSystem/{0}/Name", Type));
	}

	public static string GetLocalizedDescription(WeaponType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("WeaponUpgradeSystem/{0}/Description", Type));
	}

	public static bool GetUnlocked(WeaponType weapon, WeaponUpgradeType upgradeType)
	{
		return UnlockedUpgrades.Contains(string.Format("{0}_{1}", weapon, upgradeType));
	}
}
