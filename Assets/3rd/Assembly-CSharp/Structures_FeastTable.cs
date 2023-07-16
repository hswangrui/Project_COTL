using System.Collections.Generic;

public class Structures_FeastTable : StructureBrain, ITaskProvider
{
	public const int MAX_SLOT_COUNT = 40;

	public bool[] SlotReserved = new bool[40];

	public bool HasAvailableSlot()
	{
		bool result = false;
		for (int i = 0; i < 40; i++)
		{
			if (!SlotReserved[i])
			{
				result = true;
				break;
			}
		}
		return result;
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
			for (int i = 0; i < 40; i++)
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

	public int SlotsReserved()
	{
		int num = 0;
		bool[] slotReserved = SlotReserved;
		for (int i = 0; i < slotReserved.Length; i++)
		{
			if (slotReserved[i])
			{
				num++;
			}
		}
		return num;
	}

	public void ReleaseSlot(int slotIndex)
	{
		SlotReserved[slotIndex] = false;
	}

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
		if (Data.IsGatheringActive)
		{
			for (int i = 0; i < 40; i++)
			{
				FollowerTask_EatFeastTable followerTask_EatFeastTable = new FollowerTask_EatFeastTable(Data.ID);
				tasks.Add(followerTask_EatFeastTable.Priorty, followerTask_EatFeastTable);
			}
		}
	}
}
