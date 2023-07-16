using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_Bed : StructureBrain
{
	public delegate void BedEvent();

	public static BedEvent OnBedCollapsedStatic;

	public static BedEvent OnBedRebuiltStatic;

	public int SoulMax = 10;

	public Action<int> OnSoulsGained;

	public virtual int SlotCount
	{
		get
		{
			return 1;
		}
	}

	public bool IsCollapsed
	{
		get
		{
			return Data.IsCollapsed;
		}
		set
		{
			Data.IsCollapsed = value;
		}
	}

	public virtual int Level
	{
		get
		{
			return 1;
		}
	}

	public virtual float ChanceToCollapse
	{
		get
		{
			if (WeatherSystemController.Instance.IsRaining)
			{
				return 0.2f;
			}
			return 0.15f;
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

	public virtual TYPES CollapsedType
	{
		get
		{
			return TYPES.BED_1_COLLAPSED;
		}
	}

	public event BedEvent OnBedCollapsed;

	public override void Init(StructuresData data)
	{
		base.Init(data);
		for (int num = Data.MultipleFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (FollowerInfo.GetInfoByID(Data.MultipleFollowerIDs[num]) == null)
			{
				Data.MultipleFollowerIDs.RemoveAt(num);
			}
		}
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(Data.FollowerID);
		if (infoByID != null)
		{
			infoByID.DwellingSlot = Mathf.Clamp(infoByID.DwellingSlot, 0, SlotCount - 1);
		}
		else
		{
			Data.FollowerID = -1;
		}
	}

	public override void OnAdded()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
		for (int i = 0; i < Data.MultipleFollowerIDs.Count; i++)
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(Data.MultipleFollowerIDs[i]);
			if (infoByID != null && infoByID.DwellingID != Data.ID)
			{
				infoByID.DwellingID = Data.ID;
			}
		}
		for (int num = Data.FollowersClaimedSlots.Count - 1; num >= 0; num--)
		{
			if (FollowerInfo.GetInfoByID(Data.FollowersClaimedSlots[num]) == null)
			{
				if (Data.FollowerID == Data.FollowersClaimedSlots[num])
				{
					Data.FollowerID = -1;
				}
				Data.MultipleFollowerIDs.Remove(Data.FollowersClaimedSlots[num]);
				Data.FollowersClaimedSlots.Remove(Data.FollowersClaimedSlots[num]);
			}
		}
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
	}

	private void OnNewDayStarted()
	{
		Data.Age++;
		if (Data.Age > 2 && UnityEngine.Random.value < ChanceToCollapse)
		{
			Collapse();
		}
	}

	public void Collapse()
	{
		if (Data.FollowerID != -1)
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(Data.FollowerID);
			if (infoByID != null && FollowerBrain.GetOrCreateBrain(infoByID).CurrentTaskType == FollowerTaskType.SleepBedRest)
			{
				return;
			}
		}
		CultFaithManager.AddThought(Thought.BedCollapsed, -1, 1f);
		IsCollapsed = true;
		BedEvent onBedCollapsed = this.OnBedCollapsed;
		if (onBedCollapsed != null)
		{
			onBedCollapsed();
		}
		BedEvent onBedCollapsedStatic = OnBedCollapsedStatic;
		if (onBedCollapsedStatic != null)
		{
			onBedCollapsedStatic();
		}
	}

	public void Rebuild()
	{
		IsCollapsed = false;
		BedEvent onBedRebuiltStatic = OnBedRebuiltStatic;
		if (onBedRebuiltStatic != null)
		{
			onBedRebuiltStatic();
		}
	}

	public void CheckForAndClearDuplicateBeds()
	{
		List<int> list = new List<int>();
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			Dwelling.DwellingAndSlot dwellingAndSlot = allBrain.GetDwellingAndSlot();
			if (dwellingAndSlot != null && dwellingAndSlot.ID == Data.ID)
			{
				if (list.Contains(dwellingAndSlot.dwellingslot))
				{
					allBrain.ClearDwelling();
				}
				else
				{
					list.Add(dwellingAndSlot.dwellingslot);
				}
			}
		}
	}

	public bool CheckIfSlotIsOccupied(int slot)
	{
		bool result = false;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			Dwelling.DwellingAndSlot dwellingAndSlot = allBrain.GetDwellingAndSlot();
			if (dwellingAndSlot != null && dwellingAndSlot.ID == Data.ID && dwellingAndSlot.dwellingslot == slot)
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
