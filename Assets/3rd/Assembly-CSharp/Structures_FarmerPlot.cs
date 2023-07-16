using System;
using System.Text;
using UnityEngine;

public class Structures_FarmerPlot : StructureBrain
{
	public const int FertilizerRequired = 1;

	public bool ReservedForWatering;

	public bool ReservedForSeeding;

	public bool ReservedForFertilizing;

	public Action OnGrowthStageChanged;

	public Action OnBirdAttack;

	public int CropStatesCount
	{
		get
		{
			return CropController.CropStatesForSeedType((InventoryItem.ITEM_TYPE)GetPlantedSeed().type);
		}
	}

	public float growthTime
	{
		get
		{
			return CropController.CropGrowthTimes((InventoryItem.ITEM_TYPE)GetPlantedSeed().type);
		}
	}

	public bool IsFullyGrown
	{
		get
		{
			if (HasPlantedSeed())
			{
				return Data.GrowthStage >= growthTime;
			}
			return false;
		}
	}

	public bool CanPlantSeed()
	{
		return !HasPlantedSeed();
	}

	public bool CanWater()
	{
		if (HasPlantedSeed() && !Data.Watered)
		{
			return !IsFullyGrown;
		}
		return false;
	}

	public bool NearbyScarecrow()
	{
		BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (boxCollider2D == null)
		{
			boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
		}
		boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
		foreach (Structures_Scarecrow item in StructureManager.GetAllStructuresOfType<Structures_Scarecrow>())
		{
			float num = Vector3.Distance(Data.Position, item.Data.Position);
			float num2 = Scarecrow.EFFECTIVE_DISTANCE(item.Data.Type);
			Vector3 position = item.Data.Position;
			boxCollider2D.transform.position = position;
			boxCollider2D.size = Vector2.one * num2;
			if (boxCollider2D.OverlapPoint(position) && num < num2)
			{
				return true;
			}
		}
		return false;
	}

	public bool NearbyHarvestTotem()
	{
		float eFFECTIVE_DISTANCE = HarvestTotem.EFFECTIVE_DISTANCE;
		BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (boxCollider2D == null)
		{
			boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
		}
		boxCollider2D.size = Vector2.one * HarvestTotem.EFFECTIVE_DISTANCE;
		boxCollider2D.transform.position = Data.Position;
		boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
		foreach (Structures_HarvestTotem item in StructureManager.GetAllStructuresOfType<Structures_HarvestTotem>())
		{
			float num = Vector3.Distance(Data.Position, item.Data.Position);
			Vector3 position = item.Data.Position;
			if (boxCollider2D.OverlapPoint(position) && num < eFFECTIVE_DISTANCE)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanFertilize()
	{
		if (Data.Watered)
		{
			return !HasFertilized();
		}
		return false;
	}

	public bool CanPickCrop()
	{
		return IsFullyGrown;
	}

	public override void OnAdded()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	public override void OnRemoved()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void OnNewPhaseStarted()
	{
		if (!IsFullyGrown && HasPlantedSeed() && !NearbyScarecrow() && (double)UnityEngine.Random.value <= 0.015)
		{
			if (FarmPlot.GetFarmPlot(Data.ID) != null)
			{
				Data.HasBird = true;
				Action onBirdAttack = OnBirdAttack;
				if (onBirdAttack != null)
				{
					onBirdAttack();
				}
			}
			else
			{
				Data.HasBird = true;
			}
		}
		if (Data.Watered)
		{
			if (!Data.HasBird)
			{
				Data.GrowthStage += 1f;
			}
			if (NearbyHarvestTotem() && !Data.HasBird)
			{
				Data.GrowthStage += 0.5f;
			}
			if (HasFertilized() && !Data.BenefitedFromFertilizer && !IsFullyGrown)
			{
				Data.BenefitedFromFertilizer = true;
			}
			if (IsFullyGrown && !Data.HasBird && HasFertilized() && (DataManager.Instance.FirstTimeFertilizing || UnityEngine.Random.Range(0, 3) == 0))
			{
				Data.GrowthStage += 1f;
				DataManager.Instance.FirstTimeFertilizing = false;
			}
			if (Data.Watered && ++Data.WateredCount >= 5)
			{
				Data.Watered = WeatherSystemController.Instance.IsRaining;
			}
			Action onGrowthStageChanged = OnGrowthStageChanged;
			if (onGrowthStageChanged != null)
			{
				onGrowthStageChanged();
			}
		}
	}

	public void PlantSeed(InventoryItem.ITEM_TYPE type)
	{
		Data.BenefitedFromFertilizer = false;
		DepositItem(type);
		Data.GrowthStage = 0f;
		Vector2Int harvestsPerSeedRange = CropController.GetHarvestsPerSeedRange(type);
		Data.RemainingHarvests = UnityEngine.Random.Range(harvestsPerSeedRange.x, harvestsPerSeedRange.y + 1);
	}

	public bool HasPlantedSeed()
	{
		return GetPlantedSeed() != null;
	}

	public InventoryItem GetPlantedSeed()
	{
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type != 39 && item.type > 0)
			{
				return item;
			}
		}
		return null;
	}

	public void AddFertilizer(InventoryItem.ITEM_TYPE type)
	{
		DepositItem(type);
	}

	public int RemainingFertilizerRequired()
	{
		InventoryItem fertilizer = GetFertilizer();
		int num = ((fertilizer != null) ? fertilizer.quantity : 0);
		return 1 - num;
	}

	public bool HasFertilized()
	{
		return RemainingFertilizerRequired() <= 0;
	}

	public InventoryItem GetFertilizer()
	{
		InventoryItem result = null;
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type == 39)
			{
				result = item;
			}
		}
		return result;
	}

	public void Harvest()
	{
		InventoryItem fertilizer = GetFertilizer();
		if (fertilizer != null)
		{
			Data.Inventory.Remove(fertilizer);
		}
		Data.Watered = WeatherSystemController.Instance.IsRaining;
		Data.WateredCount = 0;
		Data.GrowthStage = 0f;
		Data.Inventory.Clear();
	}

	public override void ToDebugString(StringBuilder sb)
	{
		base.ToDebugString(sb);
		InventoryItem plantedSeed = GetPlantedSeed();
		InventoryItem.ITEM_TYPE iTEM_TYPE = ((plantedSeed != null) ? ((InventoryItem.ITEM_TYPE)plantedSeed.type) : InventoryItem.ITEM_TYPE.NONE);
		InventoryItem fertilizer = GetFertilizer();
		int num = ((fertilizer != null) ? fertilizer.quantity : 0);
		sb.AppendLine(string.Format("Type: {0}, Growth: {1}, Fertilizer: {2}/{3}, Harvests: {4}", iTEM_TYPE, Data.GrowthStage, num, 1, Data.RemainingHarvests));
	}

	public void ForceFullyGrown()
	{
		if (GetPlantedSeed() != null)
		{
			Data.HasBird = false;
			Data.GrowthStage = growthTime;
			Action onGrowthStageChanged = OnGrowthStageChanged;
			if (onGrowthStageChanged != null)
			{
				onGrowthStageChanged();
			}
		}
	}

	public InventoryItem.ITEM_TYPE GetPrioritisedSeedType()
	{
		StructureBrain structureBrain = null;
		float num = 5f;
		BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (boxCollider2D == null)
		{
			boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
		}
		boxCollider2D.size = Vector2.one * 5f;
		boxCollider2D.transform.position = Data.Position + Vector3.up * 0.7f;
		boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
		foreach (StructureBrain item in StructureManager.GetAllStructuresOfType(TYPES.FARM_PLOT_SIGN))
		{
			float num2 = Vector3.Distance(Data.Position, item.Data.Position);
			Vector3 position = item.Data.Position;
			if (boxCollider2D.OverlapPoint(position) && num2 < num)
			{
				structureBrain = item;
				num = num2;
			}
		}
		if (structureBrain != null && structureBrain.Data.SignPostItem != 0)
		{
			return InventoryItem.GetSeedType(structureBrain.Data.SignPostItem);
		}
		return InventoryItem.ITEM_TYPE.NONE;
	}
}
