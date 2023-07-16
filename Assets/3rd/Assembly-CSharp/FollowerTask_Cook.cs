using UnityEngine;

public class FollowerTask_Cook : FollowerTask
{
	private int structureID;

	private Structures_Kitchen kitchenStructure;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Cook;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override float Priorty
	{
		get
		{
			return 30f;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override bool BlockSocial
	{
		get
		{
			return true;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return structureID;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.Chef)
		{
			if (!ShouldCook())
			{
				return PriorityCategory.Low;
			}
			return PriorityCategory.WorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_Cook(int structureID)
	{
		this.structureID = structureID;
		kitchenStructure = StructureManager.GetStructureByID<Structures_Kitchen>(structureID);
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	public override void ClaimReservations()
	{
		kitchenStructure.ReservedForTask = true;
	}

	public override void ReleaseReservations()
	{
		kitchenStructure.ReservedForTask = false;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State != FollowerTaskState.Doing)
		{
			return;
		}
		if (kitchenStructure == null)
		{
			kitchenStructure = StructureManager.GetStructureByID<Structures_Kitchen>(structureID);
		}
		if (kitchenStructure.Data.CurrentCookingMeal == null)
		{
			Interaction_Kitchen.QueuedMeal bestPossibleMeal = kitchenStructure.GetBestPossibleMeal();
			if (bestPossibleMeal == null)
			{
				End();
			}
			else
			{
				kitchenStructure.Data.CurrentCookingMeal = bestPossibleMeal;
			}
		}
		else if (kitchenStructure.Data.CurrentCookingMeal.CookedTime >= kitchenStructure.Data.CurrentCookingMeal.CookingDuration)
		{
			MealFinishedCooking();
		}
		else
		{
			kitchenStructure.Data.CurrentCookingMeal.CookedTime += deltaGameTime * _brain.Info.ProductivityMultiplier;
		}
	}

	private void MealFinishedCooking()
	{
		DataManager.Instance.MealsCooked++;
		ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
		kitchenStructure.Data.QueuedMeals.RemoveAt(0);
		foreach (InventoryItem ingredient in kitchenStructure.Data.CurrentCookingMeal.Ingredients)
		{
			kitchenStructure.RemoveItems((InventoryItem.ITEM_TYPE)ingredient.type, ingredient.quantity);
		}
		Interaction_FollowerKitchen interaction_FollowerKitchen = FindKitchen();
		if ((bool)interaction_FollowerKitchen)
		{
			interaction_FollowerKitchen.MealFinishedCooking();
		}
		else
		{
			kitchenStructure.Data.QueuedResources.Add(kitchenStructure.Data.CurrentCookingMeal.MealType);
			kitchenStructure.FoodStorage.DepositItemUnstacked(kitchenStructure.Data.CurrentCookingMeal.MealType);
			CookingData.CookedMeal(kitchenStructure.Data.CurrentCookingMeal.MealType);
			ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
			kitchenStructure.Data.CurrentCookingMeal = null;
		}
		kitchenStructure.MealCooked();
		if (!ShouldCook() || kitchenStructure.GetAllPossibleMeals() <= 0)
		{
			Complete();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return GetKitchenPosition();
	}

	private Vector3 GetKitchenPosition()
	{
		Interaction_FollowerKitchen interaction_FollowerKitchen = FindKitchen();
		if (interaction_FollowerKitchen != null)
		{
			return interaction_FollowerKitchen.CookPosition.transform.position;
		}
		return kitchenStructure.Data.Position + new Vector3(0f, 2.121f);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (kitchenStructure != null)
		{
			follower.SetHat(HatType.Chef);
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.SetHat(HatType.Chef);
		follower.FacePosition(FindKitchen().CookingMealAnimation.transform.position);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("cook", true);
	}

	public override void Cleanup(Follower follower)
	{
		follower.SetHat(HatType.None);
		base.Cleanup(follower);
	}

	private Interaction_FollowerKitchen FindKitchen()
	{
		foreach (Interaction_FollowerKitchen followerKitchen in Interaction_FollowerKitchen.FollowerKitchens)
		{
			if (followerKitchen.StructureInfo.ID == structureID)
			{
				return followerKitchen;
			}
		}
		return null;
	}

	private bool ShouldCook()
	{
		if (kitchenStructure.FoodStorage.Data.Inventory.Count < kitchenStructure.FoodStorage.Capacity - 1)
		{
			return true;
		}
		return false;
	}
}
