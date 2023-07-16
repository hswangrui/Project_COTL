using System;
using System.Collections.Generic;
using UnityEngine;

public class Structures_BerryBush : StructureBrain, ITaskProvider
{
	public Action OnRegrowTree;

	public Action OnTreeProgressChanged;

	public Action<bool> OnTreeComplete;

	public bool IsCrop = true;

	public int CropID = -1;

	public bool BerryPicked
	{
		get
		{
			return Data.Progress >= Data.ProgressTarget;
		}
	}

	public void PickBerries(float berryDamage = 1f, bool dropLoot = true)
	{
		if (BerryPicked)
		{
			return;
		}
		Data.Progress += berryDamage;
		Action onTreeProgressChanged = OnTreeProgressChanged;
		if (onTreeProgressChanged != null)
		{
			onTreeProgressChanged();
		}
		if (!BerryPicked)
		{
			return;
		}
		Action<bool> onTreeComplete = OnTreeComplete;
		if (onTreeComplete != null)
		{
			onTreeComplete(dropLoot);
		}
		Data.Watered = WeatherSystemController.Instance.IsRaining;
		if (PlayerFarming.Location != Data.Location)
		{
			Structures_FarmerPlot structureByID = StructureManager.GetStructureByID<Structures_FarmerPlot>(CropID);
			if (structureByID != null)
			{
				structureByID.Harvest();
			}
			Remove();
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

	public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity == ScheduledActivity.Work && !ReservedForTask && !BerryPicked && !IsCrop)
		{
			FollowerTask_Forage followerTask_Forage = new FollowerTask_Forage(Data.ID);
			tasks.Add(followerTask_Forage.Priorty, followerTask_Forage);
		}
	}

	public void DropBerries(Vector3 Position, bool fromCrop = false, InventoryItem.ITEM_TYPE seedType = InventoryItem.ITEM_TYPE.NONE)
	{
		UnityEngine.Random.Range(0, 100);
		foreach (InventoryItem.ITEM_TYPE berry in GetBerries())
		{
			InventoryItem.Spawn(berry, 1, Position);
		}
	}

	public void AddBerriesToChest(FollowerLocation Location)
	{
		if (Location == PlayerFarming.Location)
		{
			return;
		}
		List<Structures_CollectedResourceChest> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_CollectedResourceChest>(Location);
		if (allStructuresOfType.Count <= 0)
		{
			return;
		}
		foreach (InventoryItem.ITEM_TYPE berry in GetBerries())
		{
			allStructuresOfType[0].AddItem(berry, 1);
		}
	}

	public List<InventoryItem.ITEM_TYPE> GetBerries()
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		int num = UnityEngine.Random.Range(Data.LootCountToDropRange.x, Data.LootCountToDropRange.y);
		if (IsCrop)
		{
			num = UnityEngine.Random.Range(Data.CropLootCountToDropRange.x, Data.CropLootCountToDropRange.y);
			Structures_FarmerPlot structureByID = StructureManager.GetStructureByID<Structures_FarmerPlot>(CropID);
			if (structureByID != null && structureByID.HasFertilized())
			{
				num += UnityEngine.Random.Range(1, 3);
			}
		}
		for (int i = 0; i < num; i++)
		{
			list.Add(Data.MultipleLootToDrop[0]);
		}
		if (!IsCrop)
		{
			list.Add(Data.MultipleLootToDrop[1]);
			if (UnityEngine.Random.value < 0.75f)
			{
				list.Add(Data.MultipleLootToDrop[1]);
			}
		}
		else if (IsCrop && UnityEngine.Random.value < 0.8f)
		{
			list.Add(Data.MultipleLootToDrop[1]);
		}
		return list;
	}
}
