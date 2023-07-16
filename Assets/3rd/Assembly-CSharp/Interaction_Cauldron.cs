using System;
using System.Collections;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.KitchenMenu;
using src.Extensions;
using UnityEngine;

public class Interaction_Cauldron : Interaction
{
	public delegate void ItemEvent(float normAmount);

	public Structure structure;

	private float Delay;

	private GameObject Player;

	private bool beingMoved;

	public Collider2D Collider;

	private EventInstance loopingSoundInstance;

	private Vector3 mealLocation;

	private PickUp _pickUp;

	public event ItemEvent OnFoodModified;

	private void Awake()
	{
		ActivateDistance = 1.5f;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		UpdateLocalisation();
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		PlacementRegion.OnBuildingBeganMoving += OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced += OnBuildingPlaced;
	}

	private void OnBuildingBeganMoving(int structureID)
	{
		Structure obj = structure;
		int? obj2;
		if ((object)obj == null)
		{
			obj2 = null;
		}
		else
		{
			StructuresData structure_Info = obj.Structure_Info;
			obj2 = ((structure_Info != null) ? new int?(structure_Info.ID) : null);
		}
		if (structureID == obj2)
		{
			beingMoved = true;
		}
	}

	private void OnBuildingPlaced(int structureID)
	{
		Structure obj = structure;
		int? obj2;
		if ((object)obj == null)
		{
			obj2 = null;
		}
		else
		{
			StructuresData structure_Info = obj.Structure_Info;
			obj2 = ((structure_Info != null) ? new int?(structure_Info.ID) : null);
		}
		if (structureID == obj2)
		{
			beingMoved = false;
		}
	}

	protected virtual void OnBrainAssigned()
	{
		if (structure.Type == StructureBrain.TYPES.COOKING_FIRE)
		{
			DataManager.Instance.HasBuiltCookingFire = true;
		}
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		UpdatePathfinding();
	}

	private void UpdatePathfinding()
	{
		if (Collider != null)
		{
			AstarPath.active.UpdateGraphs(Collider.bounds);
		}
		else
		{
			Debug.Log("DIDNT WORK");
		}
	}

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.Cook;
	}

	public override void OnDisableInteraction()
	{
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		PlacementRegion.OnBuildingBeganMoving -= OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced -= OnBuildingPlaced;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		ShowMenu();
	}

	protected virtual void ShowMenu()
	{
		UICookingFireMenuController cookingFireMenu = MonoSingleton<UIManager>.Instance.ShowCookingFireMenu(structure.Structure_Info);
		UICookingFireMenuController uICookingFireMenuController = cookingFireMenu;
		uICookingFireMenuController.OnConfirm = (Action)Delegate.Combine(uICookingFireMenuController.OnConfirm, (Action)delegate
		{
			StartCoroutine(Cook());
		});
		UICookingFireMenuController uICookingFireMenuController2 = cookingFireMenu;
		uICookingFireMenuController2.OnHidden = (Action)Delegate.Combine(uICookingFireMenuController2.OnHidden, (Action)delegate
		{
			cookingFireMenu = null;
		});
	}

	private void SetMealPos()
	{
		mealLocation = _pickUp.transform.position;
	}

	protected IEnumerator Cook()
	{
		Interactable = false;
		float CookingDuration = 1.5f;
		InventoryItem.ITEM_TYPE MealToCreate = InventoryItem.ITEM_TYPE.NONE;
		CookingData.CookedMeal(MealToCreate);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().AddPlayerToCamera();
		loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/cooking/cooking_loop", base.gameObject, true);
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.2f, CookingDuration);
		if (CookingDuration > 0.5f)
		{
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		}
		yield return new WaitForSecondsRealtime(CookingDuration);
		if (CookingData.TryDiscoverRecipe(MealToCreate))
		{
			AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.PlayOneShot("event:/cooking/add_food_ingredient", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
			PlayerFarming.Instance.simpleSpineAnimator.Animate("reactions/react-happy", 0, true);
			yield return new WaitForSecondsRealtime(1.5f);
			UIRecipeConfirmationOverlayController recipeConfirmationInstance = MonoSingleton<UIManager>.Instance.RecipeConfirmationTemplate.Instantiate();
			recipeConfirmationInstance.Show(MealToCreate, true);
			UIRecipeConfirmationOverlayController uIRecipeConfirmationOverlayController = recipeConfirmationInstance;
			uIRecipeConfirmationOverlayController.OnHidden = (Action)Delegate.Combine(uIRecipeConfirmationOverlayController.OnHidden, (Action)delegate
			{
				recipeConfirmationInstance = null;
			});
			MonoSingleton<UIManager>.Instance.SetMenuInstance(recipeConfirmationInstance);
			while (recipeConfirmationInstance != null)
			{
				yield return null;
			}
		}
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		structure.Structure_Info.Inventory.Clear();
		AudioManager.Instance.PlayOneShot("event:/cooking/meal_cooked", base.transform.position);
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		structure.Brain.UpdateFuel(10);
		DataManager.Instance.MealsCooked++;
		ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
		if (!DataManager.Instance.CookedFirstFood)
		{
			DataManager.Instance.CookedFirstFood = true;
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				allBrain.CompleteCurrentTask();
				allBrain.SetPersonalOverrideTask(FollowerTaskType.EatMeal);
			}
		}
		foreach (FollowerBrain allBrain2 in FollowerBrain.AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain2.Info.ID, true))
			{
				allBrain2.CheckChangeTask();
			}
		}
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CookFirstMeal);
		base.HasChanged = true;
		Interactable = true;
	}
}
