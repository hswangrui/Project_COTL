using System;
using System.Collections.Generic;

public class Structures_Refinery : StructureBrain, ITaskProvider
{
	public Action OnCompleteRefining;

	public float RefineryDuration(InventoryItem.ITEM_TYPE ItemType)
	{
		float num = ((Data.Type == TYPES.REFINERY) ? 1f : 0.75f);
		if (ItemType == InventoryItem.ITEM_TYPE.BLACK_GOLD)
		{
			return 480f / (5f * num) * 0.5f;
		}
		return 480f / (5f * num);
	}

	public static List<StructuresData.ItemCost> GetCost(InventoryItem.ITEM_TYPE Item)
	{
		switch (Item)
		{
		case InventoryItem.ITEM_TYPE.LOG_REFINED:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.LOG, 3)
			};
		case InventoryItem.ITEM_TYPE.STONE_REFINED:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.STONE, 3)
			};
		case InventoryItem.ITEM_TYPE.ROPE:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.GRASS, 10)
			};
		case InventoryItem.ITEM_TYPE.BLACK_GOLD:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.GOLD_NUGGET, 7)
			};
		case InventoryItem.ITEM_TYPE.GOLD_REFINED:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10)
			};
		default:
			return new List<StructuresData.ItemCost>
			{
				new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.NONE, 0)
			};
		}
	}

	public static int GetAmount(InventoryItem.ITEM_TYPE item)
	{
		if (item == InventoryItem.ITEM_TYPE.BLACK_GOLD)
		{
			return 5;
		}
		return 1;
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
		if (activity == ScheduledActivity.Work && !ReservedForTask && Data.QueuedResources.Count > 0)
		{
			FollowerTask_Refinery followerTask_Refinery = new FollowerTask_Refinery(Data.ID);
			tasks.Add(followerTask_Refinery.Priorty, followerTask_Refinery);
		}
	}

	public void RefineryDeposit()
	{
		Data.Progress = 0f;
		DepositItem(Data.QueuedResources[0], GetAmount(Data.QueuedResources[0]));
		Data.QueuedResources.RemoveAt(0);
		Action onCompleteRefining = OnCompleteRefining;
		if (onCompleteRefining != null)
		{
			onCompleteRefining();
		}
	}
}
