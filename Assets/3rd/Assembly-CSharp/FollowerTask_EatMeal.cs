using System;
using Spine;
using UnityEngine;

public class FollowerTask_EatMeal : FollowerTask
{
	public const int EAT_DURATION_GAME_MINUTES = 15;

	private int _mealID;

	private Structures_Meal _meal;

	private float _progress;

	private bool _mealRotten;

	private bool _mealEatenByPlayer;

	public StructureBrain.TYPES MealType;

	public static Action<int> OnEatRottenFood;

	private float satationAmount;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.EatMeal;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _meal.Data.Location;
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
			return _mealID;
		}
	}

	public FollowerTask_EatMeal(int mealID)
	{
		_mealID = mealID;
		_meal = StructureManager.GetStructureByID<Structures_Meal>(_mealID);
		MealType = _meal.Data.Type;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		Structures_Meal structureByID = StructureManager.GetStructureByID<Structures_Meal>(_mealID);
		if (structureByID != null)
		{
			structureByID.Data.Eaten = true;
		}
	}

	protected override int GetSubTaskCode()
	{
		return _mealID;
	}

	public override void ClaimReservations()
	{
		Structures_Meal structureByID = StructureManager.GetStructureByID<Structures_Meal>(_mealID);
		if (structureByID != null)
		{
			if (!structureByID.ReservedForTask)
			{
				HungerBar.ReservedSatiation += (float)CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(MealType)) / FollowerManager.GetTotalNonLockedFollowers();
			}
			structureByID.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		Structures_Meal structureByID = StructureManager.GetStructureByID<Structures_Meal>(_mealID);
		if (structureByID != null && !structureByID.Data.Eaten)
		{
			if (structureByID.ReservedForTask)
			{
				HungerBar.ReservedSatiation -= (float)CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(MealType)) / FollowerManager.GetTotalNonLockedFollowers();
			}
			structureByID.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		Structures_Meal structureByID = StructureManager.GetStructureByID<Structures_Meal>(_mealID);
		if (structureByID == null || structureByID.ReservedByPlayer || structureByID.Data.Eaten)
		{
			Abort();
			return;
		}
		_mealRotten = structureByID.Data.Rotten;
		structureByID.Data.Eaten = true;
		MealType = structureByID.Data.Type;
		_mealID = 0;
		if (_mealRotten)
		{
			Action<int> onEatRottenFood = OnEatRottenFood;
			if (onEatRottenFood != null)
			{
				onEatRottenFood(_brain.Info.ID);
			}
		}
		base.OnArrive();
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.GoingTo && LocationManager.GetLocationState(FollowerLocation.Base) == LocationState.Active)
		{
			Meal meal = FindMeal();
			if (meal != null && meal.TakenByPlayer)
			{
				_mealEatenByPlayer = true;
			}
			if (meal == null || meal.TakenByPlayer)
			{
				End();
				return;
			}
		}
		if (base.State == FollowerTaskState.Doing)
		{
			_brain.Stats.TargetBathroom = 30f;
			bool mealRotten = _mealRotten;
			_progress += deltaGameTime;
			if (_progress >= 15f)
			{
				_brain.Stats.Satiation += CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(MealType));
				HungerBar.ReservedSatiation -= (float)CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(MealType)) / FollowerManager.GetTotalNonLockedFollowers();
				End();
			}
		}
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		if (base.State == FollowerTaskState.Doing && !_mealEatenByPlayer)
		{
			_brain.Stats.Satiation += CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(MealType));
			HungerBar.ReservedSatiation -= (float)CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(MealType)) / FollowerManager.GetTotalNonLockedFollowers();
			AddThought();
			ObjectiveManager.CompleteEatMealObjective(MealType, _brain.Info.ID);
			Structures_Meal structureByID = StructureManager.GetStructureByID<Structures_Meal>(_mealID);
			if (structureByID != null)
			{
				structureByID.Data.Eaten = true;
			}
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

	public override void OnFinaliseBegin(Follower follower)
	{
		string animation = GetMealReaction(MealType);
		if (HasMealQuest(MealType))
		{
			animation = "Food/food-finish-good";
		}
		FollowerTask personalOverrideTask = _brain.GetPersonalOverrideTask();
		if (personalOverrideTask != null && personalOverrideTask.Type == FollowerTaskType.EatMeal)
		{
			_brain.ClearPersonalOverrideTaskProvider();
		}
		if (_mealEatenByPlayer)
		{
			return;
		}
		follower.TimedAnimation(animation, 1.8f, delegate
		{
			AddThought();
			Complete();
			ObjectiveManager.CompleteEatMealObjective(MealType, _brain.Info.ID);
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

	public override void SimFinaliseBegin(SimFollower simFollower)
	{
		AddThought();
		ObjectiveManager.CompleteEatMealObjective(MealType, _brain.Info.ID);
		FollowerTask personalOverrideTask = _brain.GetPersonalOverrideTask();
		if (personalOverrideTask != null && personalOverrideTask.Type == FollowerTaskType.EatMeal)
		{
			_brain.ClearPersonalOverrideTaskProvider();
		}
		base.SimFinaliseBegin(simFollower);
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Structures_Meal structureByID = StructureManager.GetStructureByID<Structures_Meal>(_mealID);
		if (structureByID != null && structureByID.Data != null)
		{
			return structureByID.Data.Position;
		}
		return follower.Brain.LastPosition;
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

	private void SetMealSkin(Follower follower)
	{
		Skin skin = new Skin("MealSkin");
		skin.AddSkin(follower.Spine.skeleton.Skin);
		skin.AddSkin(follower.Spine.Skeleton.Data.FindSkin(CookingData.GetMealSkin(MealType)));
		follower.OverridingOutfit = true;
		follower.Spine.skeleton.SetSkin(skin);
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
		case StructureBrain.TYPES.MEAL_GREAT_MIXED:
		case StructureBrain.TYPES.MEAL_GREAT_MEAT:
			return "Food/food-finish-good";
		default:
			return "Food/food-finish";
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("Food/food_eat", true);
		SetMealSkin(follower);
		satationAmount = InventoryItem.FoodSatitation(CookingData.GetMealFromStructureType(MealType));
		Interaction component = follower.GetComponent<interaction_FollowerInteraction>();
		if ((bool)component)
		{
			component.enabled = false;
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
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

	private void AddThought()
	{
		Debug.Log("ADD THOUGHT!");
		CookingData.DoMealEffect(CookingData.GetMealFromStructureType(MealType), _brain);
		if (_brain.CurrentOverrideTaskType == FollowerTaskType.EatMeal)
		{
			_brain.ClearPersonalOverrideTaskProvider();
		}
		if (_mealRotten)
		{
			StructureBrain.TYPES mealType = MealType;
			if (mealType == StructureBrain.TYPES.MEAL_FOLLOWER_MEAT)
			{
				if (_brain.HasTrait(FollowerTrait.TraitType.Cannibal))
				{
					_brain.AddThought(Thought.AteRottenFollowerMealCannibal);
				}
				else
				{
					_brain.AddThought(Thought.AteRottenFollowerMeal);
				}
			}
			else
			{
				_brain.AddThought(Thought.AteRottenMeal);
			}
		}
		else
		{
			switch (MealType)
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
			case StructureBrain.TYPES.MEAL_GREAT_MIXED:
			case StructureBrain.TYPES.MEAL_GREAT_MEAT:
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
		}
		switch (MealType)
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

	private Meal FindMeal()
	{
		Meal result = null;
		foreach (Meal meal in Meal.Meals)
		{
			if (meal != null && meal.StructureInfo != null && meal.StructureInfo.ID == _mealID)
			{
				result = meal;
				break;
			}
		}
		return result;
	}
}
