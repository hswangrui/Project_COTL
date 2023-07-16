using System;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentManager
{
	public static Action<RelicType> OnRelicUnlocked;

	private static WeaponData[] WeaponsData;

	private static CurseData[] CursesData;

	private static RelicData[] relicData;

	private const string WeaponsPath = "Data/Equipment Data/Weapons";

	private const string CursesPath = "Data/Equipment Data/Curses";

	private const string RelicsPath = "Data/Equipment Data/Relics";

	private static RelicType PrevRandomRelic = RelicType.None;

	public static RelicType NextRandomRelic = RelicType.None;

	private static readonly List<RelicType> RandomRelics = new List<RelicType>
	{
		RelicType.SpawnTentacle,
		RelicType.GungeonBlank,
		RelicType.SpawnBombs,
		RelicType.LightningStrike,
		RelicType.FiftyFiftyGamble,
		RelicType.FillUpFervour,
		RelicType.FreezeTime,
		RelicType.ProjectileRing,
		RelicType.PoisonAll,
		RelicType.RandomEnemyIntoCritter,
		RelicType.SpawnDemon,
		RelicType.SpawnCombatFollower,
		RelicType.RerollCurse,
		RelicType.RerollWeapon
	};

	private static readonly List<RelicType> RandomBlessedRelics = new List<RelicType>
	{
		RelicType.GungeonBlank,
		RelicType.FreezeTime,
		RelicType.LightningStrike_Blessed,
		RelicType.FiftyFiftyGamble_Blessed,
		RelicType.HeartConversion_Blessed,
		RelicType.FillUpFervour,
		RelicType.FreezeAll
	};

	private static readonly List<RelicType> RandomDamnedRelics = new List<RelicType>
	{
		RelicType.SpawnTentacle,
		RelicType.SpawnBombs,
		RelicType.LightningStrike_Dammed,
		RelicType.FiftyFiftyGamble_Dammed,
		RelicType.HeartConversion_Dammed,
		RelicType.ProjectileRing,
		RelicType.PoisonAll
	};

	public static RelicData[] RelicData
	{
		get
		{
			if (relicData == null)
			{
				relicData = Resources.LoadAll<RelicData>("Data/Equipment Data/Relics");
			}
			return relicData;
		}
	}

	public static EquipmentData GetEquipmentData(EquipmentType equipmentType)
	{
		EquipmentData weaponData = GetWeaponData(equipmentType);
		if ((bool)weaponData)
		{
			return weaponData;
		}
		return GetCurseData(equipmentType);
	}

	public static WeaponData GetWeaponData(EquipmentType weaponType)
	{
		if (WeaponsData == null)
		{
			WeaponsData = Resources.LoadAll<WeaponData>("Data/Equipment Data/Weapons");
		}
		WeaponData[] weaponsData = WeaponsData;
		foreach (WeaponData weaponData in weaponsData)
		{
			if (weaponData.EquipmentType == weaponType)
			{
				return weaponData;
			}
		}
		return null;
	}

	public static bool IsPoisonWeapon(EquipmentType weaponType)
	{
		if (weaponType != EquipmentType.Axe_Poison && weaponType != EquipmentType.Dagger_Poison && weaponType != EquipmentType.Gauntlet_Poison && weaponType != EquipmentType.Hammer_Poison)
		{
			return weaponType == EquipmentType.Sword_Poison;
		}
		return true;
	}

	public static CurseData GetCurseData(EquipmentType curseType)
	{
		if (CursesData == null)
		{
			CursesData = Resources.LoadAll<CurseData>("Data/Equipment Data/Curses");
		}
		CurseData[] cursesData = CursesData;
		foreach (CurseData curseData in cursesData)
		{
			if (curseData.EquipmentType == curseType)
			{
				return curseData;
			}
		}
		return null;
	}

	public static RelicData GetRelicData(RelicType relicType)
	{
		RelicData[] array = RelicData;
		foreach (RelicData relicData in array)
		{
			if (relicData.RelicType == relicType)
			{
				return relicData;
			}
		}
		return null;
	}

	public static List<RelicData> GetRelicData(List<RelicType> relicTypes)
	{
		List<RelicData> list = new List<RelicData>();
		foreach (RelicType relicType in relicTypes)
		{
			list.Add(GetRelicData(relicType));
		}
		return list;
	}

	public static bool GetRelicSingleUse(RelicType relicType)
	{
		RelicData[] array = RelicData;
		foreach (RelicData relicData in array)
		{
			if (relicData.RelicType == relicType && relicData.InteractionType == RelicInteractionType.Fragile)
			{
				return true;
			}
		}
		return false;
	}

	public static Sprite GetRelicIcon(RelicType relicType)
	{
		RelicData[] array = RelicData;
		foreach (RelicData relicData in array)
		{
			if (relicData.RelicType == relicType)
			{
				return relicData.UISprite;
			}
		}
		return null;
	}

	public static Sprite GetRelicIconOutline(RelicType relicType)
	{
		RelicData[] array = RelicData;
		foreach (RelicData relicData in array)
		{
			if (relicData.RelicType == relicType)
			{
				return relicData.UISpriteOutline;
			}
		}
		return null;
	}

	public static void PickRandomRelicData(bool includeSpawnedRelicsThisRun, RelicSubType subType = RelicSubType.Any)
	{
		int num = 100;
		PrevRandomRelic = NextRandomRelic;
		while (PrevRandomRelic == NextRandomRelic && --num > 0)
		{
			switch (subType)
			{
			case RelicSubType.Any:
				NextRandomRelic = RandomRelics[UnityEngine.Random.Range(0, RandomRelics.Count)];
				break;
			case RelicSubType.Blessed:
				NextRandomRelic = RandomBlessedRelics[UnityEngine.Random.Range(0, RandomBlessedRelics.Count)];
				break;
			case RelicSubType.Dammed:
				NextRandomRelic = RandomDamnedRelics[UnityEngine.Random.Range(0, RandomDamnedRelics.Count)];
				break;
			}
		}
	}

	public static RelicData GetRandomRelicData(bool includeSpawnedRelicsThisRun, RelicSubType subType = RelicSubType.Any)
	{
		List<RelicData> list = new List<RelicData>(RelicData);
		if (list.Count <= 1)
		{
			Debug.Log("RELIC FALLBACK HIT");
			return GetRelicData(RelicType.LightningStrike);
		}
		float num = 0.2f;
		if (UnityEngine.Random.value < num)
		{
			if (UnityEngine.Random.value > 0.5f && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Relics_Blessed_1))
			{
				subType = RelicSubType.Blessed;
			}
			else if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Relics_Dammed_1))
			{
				subType = RelicSubType.Dammed;
			}
		}
		int num2;
		for (num2 = list.Count - 1; num2 >= 0; num2--)
		{
			if (DungeonSandboxManager.Active && list[num2].RequiresCult)
			{
				list.RemoveAt(num2);
			}
			else if (subType == RelicSubType.Dammed && !list[num2].RelicType.ToString().Contains("Dammed"))
			{
				list.RemoveAt(num2);
			}
			else if (subType == RelicSubType.Blessed && !list[num2].RelicType.ToString().Contains("Blessed"))
			{
				list.RemoveAt(num2);
			}
			else if (!includeSpawnedRelicsThisRun && DataManager.Instance.SpawnedRelicsThisRun.Contains(list[num2].RelicType))
			{
				list.RemoveAt(num2);
			}
			else if (!GameManager.DungeonUseAllLayers && (list[num2].RelicType == RelicType.TeleportToBoss || list[num2].RelicType == RelicType.RandomTeleport))
			{
				list.RemoveAt(num2);
			}
			else if ((list[num2].RelicType.ToString().Contains("Dammed") && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Relics_Dammed_1)) || (list[num2].RelicType.ToString().Contains("Blessed") && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Relics_Blessed_1)))
			{
				list.RemoveAt(num2);
			}
			else if ((DataManager.Instance.ForceBlessedRelic && !list[num2].RelicType.ToString().Contains("Blessed")) || (DataManager.Instance.ForceDammedRelic && !list[num2].RelicType.ToString().Contains("Dammed")))
			{
				list.RemoveAt(num2);
			}
			else if (!DataManager.Instance.PlayerFoundRelics.Contains(list[num2].RelicType))
			{
				list.RemoveAt(num2);
			}
			else if ((list[num2].RelicType == RelicType.DestroyTarotDealDamge || list[num2].RelicType == RelicType.DestroyTarotDealDamge_Blessed || list[num2].RelicType == RelicType.DestroyTarotDealDamge_Dammed) && DataManager.Instance.PlayerRunTrinkets.Count <= 0)
			{
				list.RemoveAt(num2);
			}
			else if (list[num2].RelicType == RelicType.ProjectileRing && DataManager.Instance.PlayerRunTrinkets.Count <= 1)
			{
				list.RemoveAt(num2);
			}
			else if (list[num2].RelicType == RelicType.Enlarge && TrinketManager.HasTrinket(TarotCards.Card.WalkThroughBlocks))
			{
				list.RemoveAt(num2);
			}
			else if (list[num2].RelicType == RelicType.RerollWeapon && DataManager.Instance.PlayerFleece == 6)
			{
				list.RemoveAt(num2);
			}
		}
		Debug.Log(num2 + " relicPool count: " + list.Count);
		if (list.Count <= 0)
		{
			Debug.Log("RELIC FALLBACK HIT");
			return GetRelicData(RelicType.LightningStrike);
		}
		float num3 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float weight = list[i].Weight;
			if (float.IsPositiveInfinity(weight))
			{
				return list[i];
			}
			if (weight >= 0f && !float.IsNaN(weight))
			{
				num3 += weight;
			}
		}
		float value = UnityEngine.Random.value;
		float num4 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			float weight = list[i].Weight;
			if (!float.IsNaN(weight) && !(weight <= 0f))
			{
				num4 += weight / (float)list.Count;
				if (num4 >= value)
				{
					Debug.Log("Chosen: " + list[i]);
					return list[i];
				}
			}
		}
		return GetRandomRelicData(includeSpawnedRelicsThisRun, subType);
	}

	public static void UnlockRelics(UpgradeSystem.Type upgrade)
	{
		RelicData[] array = RelicData;
		foreach (RelicData relicData in array)
		{
			if (relicData.UpgradeType == upgrade && !DataManager.Instance.PlayerFoundRelics.Contains(relicData.RelicType))
			{
				DataManager.UnlockRelic(relicData.RelicType);
				Action<RelicType> onRelicUnlocked = OnRelicUnlocked;
				if (onRelicUnlocked != null)
				{
					onRelicUnlocked(relicData.RelicType);
				}
			}
		}
	}

	public static List<RelicData> GetRelicsForUpgradeType(UpgradeSystem.Type upgrade)
	{
		List<RelicData> list = new List<RelicData>();
		RelicData[] array = RelicData;
		foreach (RelicData relicData in array)
		{
			if (upgrade == relicData.UpgradeType)
			{
				list.Add(relicData);
			}
		}
		return list;
	}
}
