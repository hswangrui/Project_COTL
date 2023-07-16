using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_OfferingShrine : StructureBrain, ITaskProvider
{
	public Action<Vector3> OnCompleteOfferingShrine;

	public const float PRAY_DURATION = 30f;

	public List<InventoryItem.ITEM_TYPE> Offerings = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.BLACK_GOLD,
		InventoryItem.ITEM_TYPE.GOLD_NUGGET,
		InventoryItem.ITEM_TYPE.MEAT,
		InventoryItem.ITEM_TYPE.FISH_BIG,
		InventoryItem.ITEM_TYPE.FISH,
		InventoryItem.ITEM_TYPE.LOG,
		InventoryItem.ITEM_TYPE.STONE
	};

	public List<InventoryItem.ITEM_TYPE> RareOfferings = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.GOLD_REFINED,
		InventoryItem.ITEM_TYPE.LOG_REFINED,
		InventoryItem.ITEM_TYPE.STONE_REFINED
	};

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
		if (activity == ScheduledActivity.Work && !ReservedForTask && Data.Inventory.Count <= 0 && TimeManager.TotalElapsedGameTime - Data.LastPrayTime > 360f)
		{
			FollowerTask_OfferingShrine followerTask_OfferingShrine = new FollowerTask_OfferingShrine(Data.ID);
			tasks.Add(followerTask_OfferingShrine.Priorty, followerTask_OfferingShrine);
		}
	}

	public void CollectOffering()
	{
		Data.LastPrayTime = TimeManager.TotalElapsedGameTime;
	}

	public void Complete(Vector3 FollowerPosition)
	{
		Data.Progress = 0f;
		InventoryItem.ITEM_TYPE iTEM_TYPE = InventoryItem.ITEM_TYPE.NONE;
		int quantity;
		if (UnityEngine.Random.value <= 0.2f && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Economy_Refinery))
		{
			iTEM_TYPE = RareOfferings[UnityEngine.Random.Range(0, RareOfferings.Count)];
			quantity = UnityEngine.Random.Range(1, 3);
		}
		else
		{
			iTEM_TYPE = Offerings[UnityEngine.Random.Range(0, Offerings.Count)];
			quantity = UnityEngine.Random.Range(3, 5);
			if (iTEM_TYPE == InventoryItem.ITEM_TYPE.BLACK_GOLD)
			{
				quantity = UnityEngine.Random.Range(5, 9);
			}
			if (iTEM_TYPE == InventoryItem.ITEM_TYPE.GOLD_NUGGET)
			{
				quantity = UnityEngine.Random.Range(7, 14);
			}
		}
		DepositItem(iTEM_TYPE, quantity);
		Action<Vector3> onCompleteOfferingShrine = OnCompleteOfferingShrine;
		if (onCompleteOfferingShrine != null)
		{
			onCompleteOfferingShrine(FollowerPosition);
		}
	}
}
