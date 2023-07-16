using System;
using UnityEngine;

public class Structures_HarvestTotem : StructureBrain
{
	public Action<int> OnSoulsGained;

	public int SoulMax
	{
		get
		{
			return 3;
		}
	}

	public float TimeBetweenSouls
	{
		get
		{
			return 1200f;
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
