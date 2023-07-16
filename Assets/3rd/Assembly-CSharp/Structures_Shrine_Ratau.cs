using System;
using UnityEngine;

public class Structures_Shrine_Ratau : StructureBrain
{
	public Action<int> OnSoulsGained;

	public int SoulMax
	{
		get
		{
			FollowerLocation location = PlayerFarming.Location;
			if ((uint)(location - 79) <= 1u)
			{
				return 30;
			}
			return 15;
		}
	}

	public float TimeBetweenSouls
	{
		get
		{
			return 600f;
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
}
