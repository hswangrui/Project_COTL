using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_LumberjackStation : StructureBrain, ITaskProvider
{
	public Action OnExhauted;

	public int DURATION_GAME_MINUTES
	{
		get
		{
			switch (Data.Type)
			{
			case TYPES.LUMBERJACK_STATION:
				return 120;
			case TYPES.LUMBERJACK_STATION_2:
				return 75;
			case TYPES.BLOODSTONE_MINE:
				return 120;
			case TYPES.BLOODSTONE_MINE_2:
				return 75;
			default:
				return 0;
			}
		}
	}

	public int ResourceMax
	{
		get
		{
			return 9999;
		}
	}

	public int LifeSpawn
	{
		get
		{
			switch (Data.Type)
			{
			case TYPES.LUMBERJACK_STATION:
				return 50;
			case TYPES.LUMBERJACK_STATION_2:
				return 100;
			case TYPES.BLOODSTONE_MINE:
				return 50;
			case TYPES.BLOODSTONE_MINE_2:
				return 100;
			default:
				return 0;
			}
		}
	}

	public FollowerTask GetOverrideTask(FollowerBrain brain)
	{
		return null;
	}

	public bool CheckOverrideComplete()
	{
		return true;
	}

	public void IncreaseAge()
	{
		Data.Age++;
		if (Data.Age >= LifeSpawn)
		{
			Data.Exhausted = true;
			Data.Progress = 0f;
			Action onExhauted = OnExhauted;
			if (onExhauted != null)
			{
				onExhauted();
			}
		}
	}

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity == ScheduledActivity.Work && !ReservedForTask && !Data.Exhausted && Data.Inventory.Count < ResourceMax)
		{
			FollowerTask_ResourceStation followerTask_ResourceStation = new FollowerTask_ResourceStation(Data.ID);
			tasks.Add(followerTask_ResourceStation.Priorty, followerTask_ResourceStation);
		}
	}

	public Structures_Tree GetNextTree()
	{
		Structures_Tree result = null;
		float num = 30f;
		foreach (Structures_Tree item in StructureManager.GetAllStructuresOfType<Structures_Tree>(Data.Location))
		{
			if (!item.ReservedForTask && !item.TreeChopped)
			{
				float num2 = Vector3.Distance(Data.Position, item.Data.Position);
				if (num2 < num)
				{
					result = item;
					num = num2;
				}
			}
		}
		return result;
	}

	public Structures_LumberMine GetNextMine()
	{
		Structures_LumberMine result = null;
		float num = 30f;
		foreach (Structures_LumberMine item in StructureManager.GetAllStructuresOfType<Structures_LumberMine>(Data.Location))
		{
			if (item.RemainingLumber > 0)
			{
				float num2 = Vector3.Distance(Data.Position, item.Data.Position);
				if (num2 < num)
				{
					result = item;
					num = num2;
				}
			}
		}
		return result;
	}
}
