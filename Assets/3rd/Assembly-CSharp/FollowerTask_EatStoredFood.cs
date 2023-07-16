using Spine;
using UnityEngine;

public class FollowerTask_EatStoredFood : FollowerTask
{
	public const int EAT_DURATION_GAME_MINUTES = 15;

	private int _foodStorageID;

	private Structures_Kitchen _kitchen;

	public InventoryItem.ITEM_TYPE _foodType;

	private bool _reservationHeld;

	private float _progress;

	private float satationAmount;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.EatStoredFood;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _kitchen.Data.Location;
		}
	}

	public override bool BlockTaskChanges
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
			return _foodStorageID;
		}
	}

	public FollowerTask_EatStoredFood(int foodStorageID, InventoryItem.ITEM_TYPE foodType)
	{
		Debug.Log("Follower eat stored food");
		_foodStorageID = foodStorageID;
		_kitchen = StructureManager.GetStructureByID<Structures_Kitchen>(_foodStorageID);
		_foodType = foodType;
	}

	protected override int GetSubTaskCode()
	{
		return _foodStorageID;
	}

	public override void ClaimReservations()
	{
		if (_kitchen != null && (_kitchen.FoodStorage.TryClaimFoodReservation(_foodType) || _kitchen.FoodStorage.TryClaimFoodReservation(out _foodType)))
		{
			_reservationHeld = true;
			HungerBar.ReservedSatiation += (float)CookingData.GetSatationAmount(_foodType) / FollowerManager.GetTotalNonLockedFollowers();
		}
	}

	public override void ReleaseReservations()
	{
		if (_kitchen != null && _foodType != 0 && _reservationHeld)
		{
			_kitchen.FoodStorage.ReleaseFoodReservation(_foodType);
			_reservationHeld = false;
			HungerBar.ReservedSatiation -= (float)CookingData.GetSatationAmount(_foodType) / FollowerManager.GetTotalNonLockedFollowers();
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		if (base.State == FollowerTaskState.Doing)
		{
			_brain.Stats.Satiation += CookingData.GetSatationAmount(_foodType);
			HungerBar.ReservedSatiation -= (float)CookingData.GetSatationAmount(_foodType) / FollowerManager.GetTotalNonLockedFollowers();
			AddThought();
			ObjectiveManager.CompleteEatMealObjective(CookingData.GetStructureFromMealType(_foodType), _brain.Info.ID);
		}
		if (FollowerManager.FindFollowerByID(_brain.Info.ID) != null)
		{
			Interaction component = FollowerManager.FindFollowerByID(_brain.Info.ID).GetComponent<interaction_FollowerInteraction>();
			if ((bool)component)
			{
				component.enabled = true;
			}
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		if (!_kitchen.FoodStorage.TryEatReservedFood(_foodType))
		{
			End();
			return;
		}
		_kitchen.Data.QueuedResources.Remove(_foodType);
		_reservationHeld = false;
		base.OnArrive();
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			_brain.Stats.TargetBathroom = 30f;
			_progress += deltaGameTime;
			if (_progress >= 15f)
			{
				_brain.Stats.Satiation += CookingData.GetSatationAmount(_foodType);
				HungerBar.ReservedSatiation -= (float)CookingData.GetSatationAmount(_foodType) / FollowerManager.GetTotalNonLockedFollowers();
				End();
			}
		}
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Interaction_FollowerKitchen interaction_FollowerKitchen = FindFollowerKitchen();
		if (interaction_FollowerKitchen != null)
		{
			return interaction_FollowerKitchen.foodStorage.transform.position + (Vector3)Random.insideUnitCircle * Random.Range(0.25f, 0.65f);
		}
		return _kitchen.Data.Position + (Vector3)Random.insideUnitCircle * Random.Range(1f, 1.5f);
	}

	private Interaction_FollowerKitchen FindFollowerKitchen()
	{
		foreach (Interaction_FollowerKitchen followerKitchen in Interaction_FollowerKitchen.FollowerKitchens)
		{
			if (followerKitchen.StructureInfo.ID == _kitchen.Data.ID)
			{
				return followerKitchen;
			}
		}
		return null;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			follower.SetBodyAnimation("Food/food_eat", true);
			SetMealSkin(follower);
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("Food/food_eat", true);
		satationAmount = InventoryItem.FoodSatitation(_foodType);
		SetMealSkin(follower);
		Interaction component = follower.GetComponent<interaction_FollowerInteraction>();
		if ((bool)component)
		{
			component.enabled = false;
		}
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		string animation = GetMealReaction(CookingData.GetStructureFromMealType(_foodType));
		if (HasMealQuest(CookingData.GetStructureFromMealType(_foodType)))
		{
			animation = "Food/food-finish-good";
		}
		follower.TimedAnimation(animation, 1.8f, delegate
		{
			AddThought();
			Complete();
			ObjectiveManager.CompleteEatMealObjective(CookingData.GetStructureFromMealType(_foodType), _brain.Info.ID);
			if (follower != null)
			{
				Interaction component = follower.GetComponent<interaction_FollowerInteraction>();
				if ((bool)component)
				{
					component.enabled = true;
				}
			}
		});
	}

	private string GetMealReaction(StructureBrain.TYPES type)
	{
		switch (type)
		{
		case StructureBrain.TYPES.MEAL_GRASS:
			if (!_brain.HasTrait(FollowerTrait.TraitType.GrassEater))
			{
				return "Food/food-finish-bad";
			}
			return "Food/food-finish";
		case StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
			if (!_brain.HasTrait(FollowerTrait.TraitType.Cannibal))
			{
				return "Food/food-finish-bad";
			}
			return "Food/food-finish";
		case StructureBrain.TYPES.MEAL_POOP:
			return "Food/food-finish-bad";
		case StructureBrain.TYPES.MEAL:
		case StructureBrain.TYPES.MEAL_MEAT:
		case StructureBrain.TYPES.MEAL_BAD_FISH:
			return "Food/food-finish";
		case StructureBrain.TYPES.MEAL_GREAT:
		case StructureBrain.TYPES.MEAL_GOOD_FISH:
		case StructureBrain.TYPES.MEAL_GREAT_FISH:
			return "Food/food-finish-good";
		default:
			return "Food/food-finish";
		}
	}

	private void SetMealSkin(Follower follower)
	{
		Skin skin = new Skin("MealSkin");
		skin.AddSkin(follower.Spine.skeleton.Skin);
		skin.AddSkin(follower.Spine.Skeleton.Data.FindSkin(CookingData.GetMealSkin(CookingData.GetStructureFromMealType(_foodType))));
		follower.OverridingOutfit = true;
		follower.Spine.skeleton.SetSkin(skin);
	}

	public override void Cleanup(Follower follower)
	{
		follower.OverridingOutfit = false;
		follower.SetOutfit(follower.Outfit.CurrentOutfit, false);
		if (follower != null)
		{
			Interaction component = follower.GetComponent<interaction_FollowerInteraction>();
			if ((bool)component)
			{
				component.enabled = true;
			}
		}
		base.Cleanup(follower);
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
		ObjectiveManager.CompleteEatMealObjective(CookingData.GetStructureFromMealType(_foodType), _brain.Info.ID);
		AddThought();
	}

	private bool HasMealQuest(StructureBrain.TYPES mealType)
	{
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			if (objective is Objectives_EatMeal && ((Objectives_EatMeal)objective).Follower == _brain.Info.ID && ((Objectives_EatMeal)objective).MealType == mealType)
			{
				return true;
			}
		}
		return false;
	}

	private void AddThought()
	{
		CookingData.DoMealEffect(_foodType, _brain);
		if (_brain.CurrentOverrideTaskType == FollowerTaskType.EatMeal)
		{
			_brain.ClearPersonalOverrideTaskProvider();
		}
		switch (CookingData.GetStructureFromMealType(_foodType))
		{
		case StructureBrain.TYPES.MEAL_MUSHROOMS:
			_brain.Stats.Brainwash(_brain);
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain != _brain)
				{
					if (allBrain.HasTrait(FollowerTrait.TraitType.MushroomEncouraged))
					{
						allBrain.AddThought(Thought.FollowerBrainwashedSubstanceEncouraged);
					}
					else if (allBrain.HasTrait(FollowerTrait.TraitType.MushroomBanned))
					{
						allBrain.AddThought(Thought.FollowerBrainwashedSubstanceBanned);
					}
					else
					{
						allBrain.AddThought(Thought.FollowerBrainwashed);
					}
				}
			}
			break;
		case StructureBrain.TYPES.MEAL:
			_brain.AddThought(Thought.AteMeal);
			break;
		case StructureBrain.TYPES.MEAL_BAD_FISH:
			_brain.AddThought(Thought.AteBadMealFish);
			break;
		case StructureBrain.TYPES.MEAL_MEAT:
			_brain.AddThought(Thought.AteGoodMeal);
			break;
		case StructureBrain.TYPES.MEAL_GOOD_FISH:
			_brain.AddThought(Thought.AteGoodMealFish);
			break;
		case StructureBrain.TYPES.MEAL_GRASS:
			if (_brain.HasTrait(FollowerTrait.TraitType.GrassEater))
			{
				_brain.AddThought(Thought.AteGrassMealGrassEater);
			}
			else
			{
				_brain.AddThought(Thought.AteSpecialMealBad);
			}
			break;
		case StructureBrain.TYPES.MEAL_POOP:
			_brain.AddThought(Thought.AtePoopMeal);
			break;
		case StructureBrain.TYPES.MEAL_GREAT:
		case StructureBrain.TYPES.MEAL_GREAT_FISH:
			_brain.AddThought(Thought.AteSpecialMealGood);
			break;
		case StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
			if (_brain.HasTrait(FollowerTrait.TraitType.Cannibal))
			{
				_brain.AddThought(Thought.AteFollowerMealCannibal);
			}
			else if (_brain.Info.CursedState == Thought.Zombie)
			{
				_brain.AddThought(Thought.ZombieAteMeal);
			}
			else
			{
				_brain.AddThought(Thought.AteFollowerMeal);
			}
			break;
		}
		switch (CookingData.GetStructureFromMealType(_foodType))
		{
		case StructureBrain.TYPES.MEAL_POOP:
			CultFaithManager.AddThought(Thought.AtePoopMeal, _brain.Info.ID, (!HasMealQuest(StructureBrain.TYPES.MEAL_POOP)) ? 1 : 0);
			break;
		case StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
			if (!DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Cannibal))
			{
				CultFaithManager.AddThought(Thought.Cult_AteFollowerMeat, _brain.Info.ID, (!HasMealQuest(StructureBrain.TYPES.MEAL_FOLLOWER_MEAT)) ? 1 : 0);
			}
			break;
		case StructureBrain.TYPES.MEAL_GRASS:
			if (!DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.GrassEater))
			{
				CultFaithManager.AddThought(Thought.Cult_AteGrassMeal, _brain.Info.ID, 1f);
			}
			break;
		case StructureBrain.TYPES.MEAL_GREAT:
			CultFaithManager.AddThought(Thought.Cult_AteGreatMeal, _brain.Info.ID, 1f);
			break;
		case StructureBrain.TYPES.MEAL_GREAT_FISH:
			CultFaithManager.AddThought(Thought.Cult_AteGreatFishMeal, _brain.Info.ID, 1f);
			break;
		}
	}
}
