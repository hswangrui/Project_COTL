using System.Collections.Generic;
using UnityEngine;

public abstract class Structures_Research : StructureBrain, ITaskProvider
{
	public abstract bool[] SlotReserved { get; }

	public bool HasAvailableSlot()
	{
		bool result = false;
		for (int i = 0; i < SlotReserved.Length; i++)
		{
			if (!SlotReserved[i])
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public int GetAvailableSlotCount()
	{
		int num = 0;
		for (int i = 0; i < SlotReserved.Length; i++)
		{
			if (!SlotReserved[i])
			{
				num++;
			}
		}
		return num;
	}

	public int GetReservedSlotCount()
	{
		int num = 0;
		for (int i = 0; i < SlotReserved.Length; i++)
		{
			if (SlotReserved[i])
			{
				num++;
			}
		}
		return num;
	}

	public bool TryClaimSlot(ref int slotIndex)
	{
		bool flag = false;
		if (slotIndex >= 0 && !SlotReserved[slotIndex])
		{
			SlotReserved[slotIndex] = true;
			flag = true;
		}
		if (!flag)
		{
			for (int i = 0; i < SlotReserved.Length; i++)
			{
				if (!SlotReserved[i])
				{
					slotIndex = i;
					SlotReserved[i] = true;
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			slotIndex = -1;
		}
		return flag;
	}

	public void ReleaseSlot(int slotIndex)
	{
		SlotReserved[slotIndex] = false;
	}

	public abstract Vector3 GetResearchPosition(int slotIndex);

	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return null;
	}

	public bool CheckOverrideComplete()
	{
		return true;
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity != 0 || !StructuresData.GetAnyResearchExists())
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < SlotReserved.Length; i++)
		{
			if (!SlotReserved[i])
			{
				num++;
				FollowerTask_Research followerTask_Research = new FollowerTask_Research(Data.ID, (num == SlotReserved.Length) ? 15f : 8f);
				tasks.Add(followerTask_Research.Priorty, followerTask_Research);
			}
		}
	}
}
