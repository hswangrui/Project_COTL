using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_Farm : FollowerTask
{
	public const float WATERING_DURATION_GAME_MINUTES = 10f;

	public const float SEEDING_DURATION_GAME_MINUTES = 5f;

	public const float FERTILIZING_DURATION_GAME_MINUTES = 4f;

	private int _farmerStationID;

	private Structures_FarmerStation _farmerStation;

	private int _farmPlotID;

	private int _siloFertiziler;

	private int _siloSeeder;

	private int _cropID;

	private int _previousSiloFertiziler;

	private int _previousSiloSeeder;

	private bool targetFarmerStation;

	private float _progress;

	private float _gameTimeSinceLastProgress;

	private int seedTypeToPlant = -1;

	private bool holdingPoop;

	private Follower _follower;

	private Structures_BerryBush currentCrop;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Farm;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _farmerStation.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _farmerStationID;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override float Priorty
	{
		get
		{
			return 22f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (FollowerRole)
		{
		case FollowerRole.Farmer:
			return PriorityCategory.WorkPriority;
		case FollowerRole.Worshipper:
		case FollowerRole.Lumberjack:
		case FollowerRole.Monk:
			return PriorityCategory.Low;
		default:
			return PriorityCategory.Low;
		}
	}

	public FollowerTask_Farm(int farmerStationID)
	{
		_farmerStationID = farmerStationID;
		_farmerStation = StructureManager.GetStructureByID<Structures_FarmerStation>(_farmerStationID);
		_farmPlotID = 0;
		_siloFertiziler = 0;
		_siloSeeder = 0;
		targetFarmerStation = false;
	}

	protected override int GetSubTaskCode()
	{
		return _farmerStationID;
	}

	public override void ClaimReservations()
	{
		Structures_FarmerStation structureByID = StructureManager.GetStructureByID<Structures_FarmerStation>(_farmerStationID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = true;
		}
		Structures_FarmerPlot structureByID2 = StructureManager.GetStructureByID<Structures_FarmerPlot>(_farmPlotID);
		if (structureByID2 != null)
		{
			if (structureByID2.CanPlantSeed())
			{
				structureByID2.ReservedForSeeding = true;
			}
			else if (structureByID2.CanWater())
			{
				structureByID2.ReservedForWatering = true;
			}
			else if (structureByID2.CanFertilize())
			{
				structureByID2.ReservedForFertilizing = true;
			}
		}
		else
		{
			Structures_FarmerPlot structureByID3 = StructureManager.GetStructureByID<Structures_FarmerPlot>(_cropID);
			if (structureByID3 != null)
			{
				structureByID3.ReservedForTask = true;
			}
		}
	}

	public override void ReleaseReservations()
	{
		Structures_FarmerStation structureByID = StructureManager.GetStructureByID<Structures_FarmerStation>(_farmerStationID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = false;
		}
		Structures_FarmerPlot structureByID2 = StructureManager.GetStructureByID<Structures_FarmerPlot>(_farmPlotID);
		if (structureByID2 != null)
		{
			if (structureByID2.ReservedForSeeding)
			{
				structureByID2.ReservedForSeeding = false;
			}
			else if (structureByID2.ReservedForWatering)
			{
				structureByID2.ReservedForWatering = false;
			}
			else if (structureByID2.ReservedForFertilizing)
			{
				structureByID2.ReservedForFertilizing = false;
			}
		}
		else
		{
			Structures_FarmerPlot structureByID3 = StructureManager.GetStructureByID<Structures_FarmerPlot>(_cropID);
			if (structureByID3 != null)
			{
				structureByID3.ReservedForTask = false;
			}
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			float num = 1f;
			if (_cropID != 0 && currentCrop.Data.Progress < currentCrop.Data.ProgressTarget && currentCrop != null && currentCrop.BerryPicked)
			{
				_cropID = 0;
				_progress = 0f;
				currentCrop = null;
				ProgressTask();
			}
			_gameTimeSinceLastProgress += deltaGameTime * num;
			ProgressTask();
		}
	}

	public override void ProgressTask()
	{
		Structures_FarmerPlot structureByID = StructureManager.GetStructureByID<Structures_FarmerPlot>(_farmPlotID);
		if (structureByID == null)
		{
			currentCrop = StructureManager.GetStructureByID<Structures_BerryBush>(_cropID);
			if (currentCrop != null && !currentCrop.ReservedByPlayer)
			{
				currentCrop.PickBerries(_gameTimeSinceLastProgress, false);
				_gameTimeSinceLastProgress = 0f;
				if (!(currentCrop.Data.Progress >= currentCrop.Data.ProgressTarget))
				{
					return;
				}
				List<InventoryItem.ITEM_TYPE> berries = currentCrop.GetBerries();
				foreach (InventoryItem.ITEM_TYPE item in berries)
				{
					if (_brain != null && _brain._directInfoAccess != null && _brain._directInfoAccess.Inventory != null)
					{
						_brain._directInfoAccess.Inventory.Add(new InventoryItem(item, 1));
					}
				}
				List<Structures_BerryBush> cropsAtPosition = _farmerStation.GetCropsAtPosition(currentCrop.Data.Position);
				for (int num = cropsAtPosition.Count - 1; num >= 0; num--)
				{
					if (cropsAtPosition[num] != null)
					{
						cropsAtPosition[num].PickBerries(100f, false);
					}
				}
				_brain.GetXP(1f);
				_cropID = 0;
				currentCrop = null;
				targetFarmerStation = berries.Count > 0;
				UpdatePlot();
				Loop();
			}
			else
			{
				Loop();
			}
		}
		else if (structureByID.ReservedForSeeding)
		{
			_progress += _gameTimeSinceLastProgress;
			_gameTimeSinceLastProgress = 0f;
			if (_progress >= 5f)
			{
				_progress = 0f;
				if (structureByID.CanPlantSeed())
				{
					structureByID.Data.Inventory.Add(new InventoryItem((InventoryItem.ITEM_TYPE)seedTypeToPlant, 1));
					structureByID.PlantSeed((InventoryItem.ITEM_TYPE)seedTypeToPlant);
				}
				else
				{
					RefundSeed();
				}
				seedTypeToPlant = -1;
				structureByID.ReservedForSeeding = false;
				_brain.GetXP(1f);
				UpdatePlot();
				Loop();
			}
		}
		else if (structureByID.ReservedForWatering)
		{
			_progress += _gameTimeSinceLastProgress;
			_gameTimeSinceLastProgress = 0f;
			if (_progress >= 10f)
			{
				_progress = 0f;
				structureByID.Data.Watered = true;
				structureByID.Data.WateredCount = 0;
				structureByID.ReservedForWatering = false;
				_brain.GetXP(1f);
				UpdatePlot();
				Loop();
			}
		}
		else if (structureByID.ReservedForFertilizing)
		{
			_progress += _gameTimeSinceLastProgress;
			_gameTimeSinceLastProgress = 0f;
			if (_progress >= 4f)
			{
				_progress = 0f;
				if (structureByID.CanFertilize())
				{
					structureByID.Data.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.POOP, 1));
					structureByID.AddFertilizer(InventoryItem.ITEM_TYPE.POOP);
				}
				else
				{
					RefundPoop();
				}
				holdingPoop = false;
				structureByID.ReservedForFertilizing = false;
				_brain.GetXP(1f);
				UpdatePlot();
				Loop();
			}
		}
		else
		{
			Loop();
		}
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		if (seedTypeToPlant != -1)
		{
			RefundSeed();
		}
		if (holdingPoop)
		{
			RefundPoop();
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (seedTypeToPlant != -1)
		{
			RefundSeed();
		}
		if (holdingPoop)
		{
			RefundPoop();
		}
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
		if (seedTypeToPlant != -1)
		{
			RefundSeed();
		}
		if (holdingPoop)
		{
			RefundPoop();
		}
	}

	private void RefundSeed()
	{
		Structures_SiloSeed structureByID = StructureManager.GetStructureByID<Structures_SiloSeed>(_previousSiloSeeder);
		if (structureByID == null)
		{
			return;
		}
		structureByID.Data.Inventory.Add(new InventoryItem
		{
			type = seedTypeToPlant,
			quantity = 1
		});
		foreach (Interaction_SiloSeeder siloSeeder in Interaction_SiloSeeder.SiloSeeders)
		{
			if (siloSeeder.StructureBrain.Data.ID == _previousSiloSeeder)
			{
				siloSeeder.UpdateCapacityIndicators();
				break;
			}
		}
		seedTypeToPlant = -1;
	}

	private void RefundPoop()
	{
		Structures_SiloFertiliser structureByID = StructureManager.GetStructureByID<Structures_SiloFertiliser>(_previousSiloFertiziler);
		if (structureByID == null)
		{
			return;
		}
		structureByID.Data.Inventory.Add(new InventoryItem
		{
			type = 39,
			quantity = 1
		});
		foreach (Interaction_SiloFertilizer siloFertilizer in Interaction_SiloFertilizer.SiloFertilizers)
		{
			if (siloFertilizer.StructureBrain.Data.ID == _previousSiloFertiziler)
			{
				siloFertilizer.UpdateCapacityIndicators();
				break;
			}
		}
		holdingPoop = false;
	}

	private void UpdatePlot()
	{
		FarmPlot farmPlot = FindFarmPlot();
		if (farmPlot != null)
		{
			farmPlot.UpdateCropImage();
		}
	}

	private void Loop()
	{
		if (targetFarmerStation)
		{
			ClearDestination();
			_farmPlotID = 0;
			_siloFertiziler = 0;
			_siloSeeder = 0;
			SetState(FollowerTaskState.GoingTo);
			if ((bool)_follower)
			{
				_follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, GetMovementAnim());
			}
			return;
		}
		Structures_BerryBush nextUnpickedPlot = _farmerStation.GetNextUnpickedPlot();
		if (nextUnpickedPlot == null || _farmerStation.Data.Type != StructureBrain.TYPES.FARM_STATION_II)
		{
			Structures_FarmerPlot nextUnseededPlot = _farmerStation.GetNextUnseededPlot();
			if (nextUnseededPlot == null)
			{
				nextUnseededPlot = _farmerStation.GetNextUnwateredPlot();
				if (nextUnseededPlot == null)
				{
					nextUnseededPlot = _farmerStation.GetNextUnfertilizedPlot();
					if (nextUnseededPlot == null || Structures_SiloFertiliser.GetClosestFertiliser(_brain.LastPosition, _brain.Location) == null)
					{
						End();
						return;
					}
					ClearDestination();
					_farmPlotID = nextUnseededPlot.Data.ID;
					_cropID = 0;
					nextUnseededPlot.ReservedForFertilizing = true;
					NextTarget(nextUnseededPlot);
					SetState(FollowerTaskState.GoingTo);
				}
				else
				{
					ClearDestination();
					_farmPlotID = nextUnseededPlot.Data.ID;
					_cropID = 0;
					nextUnseededPlot.ReservedForWatering = true;
					SetState(FollowerTaskState.GoingTo);
				}
			}
			else
			{
				ClearDestination();
				_farmPlotID = nextUnseededPlot.Data.ID;
				_cropID = 0;
				nextUnseededPlot.ReservedForSeeding = true;
				NextTarget(nextUnseededPlot);
				SetState(FollowerTaskState.GoingTo);
			}
		}
		else
		{
			ClearDestination();
			_cropID = nextUnpickedPlot.Data.ID;
			_farmPlotID = 0;
			currentCrop = nextUnpickedPlot;
			nextUnpickedPlot.ReservedForTask = true;
			SetState(FollowerTaskState.GoingTo);
		}
	}

	private void NextTarget(Structures_FarmerPlot nextPlot)
	{
		if (nextPlot.ReservedForSeeding)
		{
			InventoryItem.ITEM_TYPE prioritisedSeedType = InventoryItem.ITEM_TYPE.NONE;
			Structures_FarmerPlot structureByID = StructureManager.GetStructureByID<Structures_FarmerPlot>(_farmPlotID);
			if (structureByID != null)
			{
				prioritisedSeedType = structureByID.GetPrioritisedSeedType();
			}
			Structures_SiloSeed closestSeeder = Structures_SiloSeed.GetClosestSeeder(_brain.LastPosition, _brain.Location, prioritisedSeedType);
			if (closestSeeder != null)
			{
				_siloSeeder = closestSeeder.Data.ID;
				_previousSiloSeeder = closestSeeder.Data.ID;
			}
		}
		else if (nextPlot.ReservedForFertilizing)
		{
			Structures_SiloFertiliser closestFertiliser = Structures_SiloFertiliser.GetClosestFertiliser(_brain.LastPosition, _brain.Location);
			if (closestFertiliser != null)
			{
				_siloFertiziler = closestFertiliser.Data.ID;
				_previousSiloFertiziler = closestFertiliser.Data.ID;
			}
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Vector3 result = default(Vector3);
		Structures_FarmerPlot structureByID = StructureManager.GetStructureByID<Structures_FarmerPlot>(_farmPlotID);
		Structures_SiloSeed structureByID2 = StructureManager.GetStructureByID<Structures_SiloSeed>(_siloSeeder);
		Structures_SiloFertiliser structureByID3 = StructureManager.GetStructureByID<Structures_SiloFertiliser>(_siloFertiziler);
		Structures_BerryBush structureByID4 = StructureManager.GetStructureByID<Structures_BerryBush>(_cropID);
		if (structureByID2 != null)
		{
			return structureByID2.Data.Position;
		}
		if (structureByID3 != null)
		{
			return structureByID3.Data.Position;
		}
		if (structureByID4 != null)
		{
			return structureByID4.Data.Position;
		}
		if (structureByID != null)
		{
			return structureByID.Data.Position + new Vector3(0f - Farm.FarmTileSize, 0f, 0f);
		}
		FarmStation farmStation = FindFarmStation();
		if (farmStation != null)
		{
			return farmStation.WorshipperPosition.transform.position;
		}
		Abort();
		return result;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (_farmPlotID != 0)
		{
			follower.SetHat(HatType.Farm);
		}
		_follower = follower;
	}

	private string GetMovementAnim()
	{
		using (List<InventoryItem>.Enumerator enumerator = _brain._directInfoAccess.Inventory.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch ((InventoryItem.ITEM_TYPE)enumerator.Current.type)
				{
				case InventoryItem.ITEM_TYPE.BERRY:
					return "Farming/run-berries";
				case InventoryItem.ITEM_TYPE.PUMPKIN:
					return "Farming/run-pumpkin";
				case InventoryItem.ITEM_TYPE.BEETROOT:
					return "Farming/run-beetroot";
				case InventoryItem.ITEM_TYPE.CAULIFLOWER:
					return "Farming/run-cauliflower";
				case InventoryItem.ITEM_TYPE.MUSHROOM_SMALL:
					return "Farming/run-mushroom";
				case InventoryItem.ITEM_TYPE.FLOWER_RED:
					return "Farming/run-redflower";
				}
			}
		}
		return "Farming/run-berries";
	}

	private string GetDepositFoodAnim()
	{
		using (List<InventoryItem>.Enumerator enumerator = _brain._directInfoAccess.Inventory.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch ((InventoryItem.ITEM_TYPE)enumerator.Current.type)
				{
				case InventoryItem.ITEM_TYPE.BERRY:
					return "Farming/add-berries";
				case InventoryItem.ITEM_TYPE.PUMPKIN:
					return "Farming/add-pumpkin";
				case InventoryItem.ITEM_TYPE.BEETROOT:
					return "Farming/add-beetroot";
				case InventoryItem.ITEM_TYPE.CAULIFLOWER:
					return "Farming/add-cauliflower";
				case InventoryItem.ITEM_TYPE.MUSHROOM_SMALL:
					return "Farming/add-mushroom";
				case InventoryItem.ITEM_TYPE.FLOWER_RED:
					return "Farming/add-redflower";
				}
			}
		}
		return "Farming/add-berries";
	}

	public override void OnDoingBegin(Follower follower)
	{
		_follower = follower;
		DoingBegin(follower);
	}

	public override void OnGoingToBegin(Follower follower)
	{
		base.OnGoingToBegin(follower);
		_follower = follower;
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		DoingBegin(null);
	}

	private void DoingBegin(Follower follower)
	{
		if ((bool)follower)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
		}
		if (_siloFertiziler != 0)
		{
			Structures_SiloFertiliser structureByID = StructureManager.GetStructureByID<Structures_SiloFertiliser>(_siloFertiziler);
			if (structureByID.Data.Inventory.Count <= 0)
			{
				End();
				return;
			}
			holdingPoop = true;
			structureByID.Data.Inventory[0].quantity--;
			if (structureByID.Data.Inventory[0].quantity <= 0)
			{
				structureByID.Data.Inventory.RemoveAt(0);
			}
			foreach (Interaction_SiloFertilizer siloFertilizer in Interaction_SiloFertilizer.SiloFertilizers)
			{
				if (siloFertilizer.StructureBrain.Data.ID == _siloFertiziler)
				{
					siloFertilizer.UpdateCapacityIndicators();
					break;
				}
			}
			if ((bool)follower)
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-poop");
			}
			_siloFertiziler = 0;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		else if (_siloSeeder != 0)
		{
			Structures_SiloSeed structureByID2 = StructureManager.GetStructureByID<Structures_SiloSeed>(_siloSeeder);
			if (structureByID2 == null || structureByID2.Data.Inventory.Count <= 0)
			{
				End();
				return;
			}
			InventoryItem.ITEM_TYPE iTEM_TYPE = InventoryItem.ITEM_TYPE.NONE;
			Structures_FarmerPlot structureByID3 = StructureManager.GetStructureByID<Structures_FarmerPlot>(_farmPlotID);
			if (structureByID3 != null)
			{
				iTEM_TYPE = structureByID3.GetPrioritisedSeedType();
			}
			InventoryItem inventoryItem = structureByID2.Data.Inventory[0];
			for (int num = structureByID2.Data.Inventory.Count - 1; num >= 0; num--)
			{
				if (structureByID2.Data.Inventory[num].type == (int)iTEM_TYPE && structureByID2.Data.Inventory[num].quantity > 0)
				{
					inventoryItem = structureByID2.Data.Inventory[num];
				}
			}
			inventoryItem.quantity--;
			seedTypeToPlant = inventoryItem.type;
			if (inventoryItem.quantity <= 0)
			{
				structureByID2.Data.Inventory.Remove(inventoryItem);
			}
			foreach (Interaction_SiloSeeder siloSeeder in Interaction_SiloSeeder.SiloSeeders)
			{
				if (siloSeeder.StructureBrain.Data.ID == _siloSeeder)
				{
					siloSeeder.UpdateCapacityIndicators();
					break;
				}
			}
			if ((bool)follower)
			{
				if (seedTypeToPlant == 8)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-seed");
				}
				else if (seedTypeToPlant == 70)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-seed-mushroom");
				}
				else if (seedTypeToPlant == 51)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-seed-pumpkin");
				}
				else if (seedTypeToPlant == 72)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-seed-redflower");
				}
				else if (seedTypeToPlant == 71)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-seed-whiteflower");
				}
				else if (seedTypeToPlant == 98)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-seed-beetroot");
				}
				else if (seedTypeToPlant == 103)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Farming/run-seed-cauliflower");
				}
			}
			_siloSeeder = 0;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		else if (_cropID != 0)
		{
			if ((bool)follower)
			{
				Structures_BerryBush structureByID4 = StructureManager.GetStructureByID<Structures_BerryBush>(_cropID);
				follower.FacePosition(structureByID4.Data.Position);
				string animation = "action";
				float progressTarget = structureByID4.Data.ProgressTarget;
				follower.TimedAnimation(animation, progressTarget, delegate
				{
					ProgressTask();
				});
			}
			else
			{
				ProgressTask();
			}
		}
		else if (targetFarmerStation)
		{
			if ((bool)follower)
			{
				FarmStation farmStation = FindFarmStation();
				follower.FacePosition(farmStation.transform.position);
			}
			targetFarmerStation = false;
			if ((bool)_follower)
			{
				_follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
			}
			if ((bool)follower)
			{
				float timer = 1.5f;
				ClearDestination();
				SetState(FollowerTaskState.Idle);
				follower.TimedAnimation(GetDepositFoodAnim(), timer, delegate
				{
					foreach (InventoryItem item in _brain._directInfoAccess.Inventory)
					{
						StructureManager.GetStructureByID<Structures_FarmerStation>(_farmerStationID).DepositItem((InventoryItem.ITEM_TYPE)item.type);
					}
					_brain._directInfoAccess.Inventory.Clear();
					ProgressTask();
				});
				return;
			}
			foreach (InventoryItem item2 in _brain._directInfoAccess.Inventory)
			{
				StructureManager.GetStructureByID<Structures_FarmerStation>(_farmerStationID).DepositItem((InventoryItem.ITEM_TYPE)item2.type);
			}
			_brain._directInfoAccess.Inventory.Clear();
			ProgressTask();
		}
		else if (_farmPlotID == 0)
		{
			if ((bool)follower)
			{
				follower.SetHat(HatType.Farm);
			}
			ProgressTask();
		}
		else if ((bool)follower)
		{
			FarmPlot farmPlot = FindFarmPlot();
			if (farmPlot != null)
			{
				follower.FacePosition(farmPlot.transform.position);
				string animation2 = "Farming-water";
				float timer2 = 3.5f;
				if (farmPlot.StructureBrain.ReservedForSeeding)
				{
					timer2 = 5f;
					if (seedTypeToPlant == 8)
					{
						animation2 = "Farming/add-seed";
					}
					else if (seedTypeToPlant == 70)
					{
						animation2 = "Farming/add-seed-mushroom";
					}
					else if (seedTypeToPlant == 51)
					{
						animation2 = "Farming/add-seed-pumpkin";
					}
					else if (seedTypeToPlant == 72)
					{
						animation2 = "Farming/add-seed-redflower";
					}
					else if (seedTypeToPlant == 71)
					{
						animation2 = "Farming/add-seed-whiteflower";
					}
					else if (seedTypeToPlant == 98)
					{
						animation2 = "Farming/add-seed-beetroot";
					}
					else if (seedTypeToPlant == 103)
					{
						animation2 = "Farming/add-seed-cauliflower";
					}
				}
				else if (farmPlot.StructureBrain.ReservedForFertilizing)
				{
					animation2 = "Farming/add-fertiliser";
					timer2 = 4f;
				}
				follower.TimedAnimation(animation2, timer2, delegate
				{
					ProgressTask();
				});
			}
			else
			{
				ProgressTask();
			}
		}
		else
		{
			ProgressTask();
		}
	}

	public override void Cleanup(Follower follower)
	{
		follower.SetHat(HatType.None);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
		if (seedTypeToPlant != -1)
		{
			RefundSeed();
		}
		if (holdingPoop)
		{
			RefundPoop();
		}
		base.Cleanup(follower);
	}

	private FarmStation FindFarmStation()
	{
		FarmStation result = null;
		foreach (FarmStation farmStation in FarmStation.FarmStations)
		{
			if (farmStation != null && farmStation.StructureInfo.ID == _farmerStationID)
			{
				result = farmStation;
				break;
			}
		}
		return result;
	}

	private FarmPlot FindFarmPlot()
	{
		FarmPlot result = null;
		foreach (FarmPlot farmPlot in FarmPlot.FarmPlots)
		{
			if (farmPlot != null && farmPlot.StructureInfo.ID == _farmPlotID)
			{
				result = farmPlot;
				break;
			}
		}
		return result;
	}
}
