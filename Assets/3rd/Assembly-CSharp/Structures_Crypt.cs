using System;
using UnityEngine;

public class Structures_Crypt : StructureBrain
{
	public Action<int> OnSoulsGained;

	public override bool IsFull
	{
		get
		{
			return Data.MultipleFollowerIDs.Count >= DEAD_BODY_SLOTS;
		}
	}

	public int DEAD_BODY_SLOTS
	{
		get
		{
			return GetCapacity(Data.Type);
		}
	}

	public int SoulMax
	{
		get
		{
			return 10 * FollowersFuneralCount();
		}
	}

	public float TimeBetweenSouls
	{
		get
		{
			return 360f;
		}
	}

	public int SoulCount
	{
		get
		{
			Data.SoulCount = Mathf.Clamp(Data.SoulCount, 0, SoulMax);
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

	public static int GetCapacity(TYPES Type)
	{
		switch (Type)
		{
		case TYPES.CRYPT_3:
			return 12;
		case TYPES.CRYPT_2:
			return 8;
		default:
			return 5;
		}
	}

	public void DepositBody(int followerID)
	{
		if (!Data.MultipleFollowerIDs.Contains(followerID))
		{
			Data.MultipleFollowerIDs.Add(followerID);
		}
	}

	public void WithdrawBody(int followerID)
	{
		if (Data.MultipleFollowerIDs.Contains(followerID))
		{
			Data.MultipleFollowerIDs.Remove(followerID);
		}
	}

	public int FollowersFuneralCount()
	{
		int num = 0;
		for (int i = 0; i < Data.MultipleFollowerIDs.Count; i++)
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(Data.MultipleFollowerIDs[i], true);
			if (infoByID != null && infoByID.HadFuneral)
			{
				num++;
			}
		}
		return num;
	}
}
