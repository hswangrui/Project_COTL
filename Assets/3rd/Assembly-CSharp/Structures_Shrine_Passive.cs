using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_Shrine_Passive : StructureBrain
{
	public List<FollowerTask_Pray> Prayers = new List<FollowerTask_Pray>();

	public Action<int> OnSoulsGained;

	public int SoulMax
	{
		get
		{
			switch (Data.Type)
			{
			case TYPES.SHRINE_PASSIVE:
				return 10;
			case TYPES.SHRINE_PASSIVE_II:
				return 20;
			case TYPES.SHRINE_PASSIVE_III:
				return 40;
			default:
				return 0;
			}
		}
	}

	public float DevotionSpeedMultiplier
	{
		get
		{
			if (Data.FullyFueled && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Shrine_PassiveShrinesFlames))
			{
				return 1.4f;
			}
			return 1f;
		}
	}

	public int SoulCount
	{
		get
		{
			return Data.SoulCount;
		}
		set
		{
			int soulCount = SoulCount;
			Data.SoulCount = Mathf.Clamp(value, 0, SoulMax);
			if (SoulCount > soulCount)
			{
				Action<int> onSoulsGained = OnSoulsGained;
				if (onSoulsGained != null)
				{
					onSoulsGained(SoulCount - soulCount);
				}
			}
		}
	}

	public static float timeBetweenPrays(TYPES Type)
	{
		switch (Type)
		{
		case TYPES.SHRINE_PASSIVE:
			return 240f;
		case TYPES.SHRINE_PASSIVE_II:
			return 168f;
		case TYPES.SHRINE_PASSIVE_III:
			return 120f;
		default:
			return 240f;
		}
	}

	public static float Range(TYPES Type)
	{
		switch (Type)
		{
		case TYPES.SHRINE_PASSIVE:
			return 4f;
		case TYPES.SHRINE_PASSIVE_II:
			return 6f;
		case TYPES.SHRINE_PASSIVE_III:
			return 8f;
		default:
			return 10f;
		}
	}

	public bool PrayAvailable(TYPES Type)
	{
		if (TimeManager.TotalElapsedGameTime > Data.LastPrayTime + timeBetweenPrays(Type) / DevotionSpeedMultiplier)
		{
			return SoulCount < SoulMax;
		}
		return false;
	}

	public void SetFollowerPrayed()
	{
		Data.LastPrayTime = TimeManager.TotalElapsedGameTime;
	}
}
