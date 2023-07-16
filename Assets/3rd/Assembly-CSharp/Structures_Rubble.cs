using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_Rubble : StructureBrain, ITaskProvider
{
	private int availableSlotCount;

	public int RockSize;

	public Action<int> OnRemovalProgressChanged;

	public Action OnRemovalComplete;

	public float RemovalDurationInGameMinutes
	{
		get
		{
			return (RockSize == 0) ? 30 : 1000;
		}
	}

	public int RubbleDropAmount
	{
		get
		{
			switch (Data.LootToDrop)
			{
			case InventoryItem.ITEM_TYPE.STONE:
				if (RockSize != 0)
				{
					return 10;
				}
				return 1;
			case InventoryItem.ITEM_TYPE.BLOOD_STONE:
				return UnityEngine.Random.Range(3, 5);
			case InventoryItem.ITEM_TYPE.GOLD_NUGGET:
				return UnityEngine.Random.Range(2, 4);
			default:
				return 0;
			}
		}
	}

	public int AvailableSlotCount
	{
		get
		{
			return availableSlotCount;
		}
		set
		{
			availableSlotCount = value;
		}
	}

	public float RemovalProgress
	{
		get
		{
			return Data.Progress;
		}
		set
		{
			if (ProgressFinished)
			{
				return;
			}
			Data.Progress = value;
			if (ProgressFinished)
			{
				Remove();
				Action onRemovalComplete = OnRemovalComplete;
				if (onRemovalComplete != null)
				{
					onRemovalComplete();
				}
			}
		}
	}

	public bool ProgressFinished
	{
		get
		{
			return RemovalProgress >= RemovalDurationInGameMinutes;
		}
	}

	public void UpdateProgress(int followerID = -1)
	{
		Action<int> onRemovalProgressChanged = OnRemovalProgressChanged;
		if (onRemovalProgressChanged != null)
		{
			onRemovalProgressChanged(followerID);
		}
	}

	public Structures_Rubble(int rockSize)
	{
		RockSize = rockSize;
		AvailableSlotCount = ((RockSize == 0) ? 1 : 5);
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
		if (activity == ScheduledActivity.Work && !ProgressFinished)
		{
			for (int i = 0; i < AvailableSlotCount; i++)
			{
				FollowerTask_ClearRubble followerTask_ClearRubble = new FollowerTask_ClearRubble(Data.ID);
				tasks.Add(followerTask_ClearRubble.Priorty, followerTask_ClearRubble);
			}
		}
	}
}
