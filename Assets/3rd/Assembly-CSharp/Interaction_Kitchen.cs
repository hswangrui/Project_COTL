using System;
using System.Collections;
using System.Collections.Generic;
using Lamb.UI;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

public class Interaction_Kitchen : Interaction_Cauldron
{
	public class QueuedMeal
	{
		public InventoryItem.ITEM_TYPE MealType;

		public List<InventoryItem> Ingredients = new List<InventoryItem>();

		public float CookingDuration;

		public float CookedTime = -1f;
	}

	public Transform SpawnMealPosition;

	public GameObject CookingOn;

	public GameObject CookingOff;

	public SkeletonAnimation CookingMealAnimation;

	public static List<Interaction_Kitchen> Kitchens = new List<Interaction_Kitchen>();

	private UICookingMinigameOverlayController _uiCookingMinigameOverlayController;

	public StructuresData StructureInfo
	{
		get
		{
			return structure.Structure_Info;
		}
	}

	private void ShowCooking(bool Show)
	{
		CookingOn.SetActive(Show);
		CookingOff.SetActive(!Show);
	}

	private void Start()
	{
		Kitchens.Add(this);
		ShowCooking(false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Kitchens.Remove(this);
	}

	protected override void ShowMenu()
	{
		UIKitchenMenuController kitchenMenuInstance = MonoSingleton<UIManager>.Instance.ShowKitchenMenu(structure.Structure_Info);
		UIKitchenMenuController uIKitchenMenuController = kitchenMenuInstance;
		uIKitchenMenuController.OnHidden = (Action)Delegate.Combine(uIKitchenMenuController.OnHidden, (Action)delegate
		{
			kitchenMenuInstance = null;
		});
		UIKitchenMenuController uIKitchenMenuController2 = kitchenMenuInstance;
		uIKitchenMenuController2.OnConfirm = (Action)Delegate.Combine(uIKitchenMenuController2.OnConfirm, new Action(CookAll));
	}

	private void CookAll()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone);
		ShowCooking(true);
		CookingMealAnimation.AnimationState.SetAnimation(0, "start", false);
		CookingMealAnimation.AnimationState.AddAnimation(0, "cook", true, 0f);
		PlayerFarming.Instance.GoToAndStop(StructureInfo.Position + new Vector3(0.1f, 2.5f), base.transform.parent.gameObject, false, false, delegate
		{
			_uiCookingMinigameOverlayController = MonoSingleton<UIManager>.Instance.CookingMinigameOverlayControllerTemplate.Instantiate();
			_uiCookingMinigameOverlayController.Initialise(StructureInfo, this);
			UICookingMinigameOverlayController uiCookingMinigameOverlayController = _uiCookingMinigameOverlayController;
			uiCookingMinigameOverlayController.OnCook = (Action)Delegate.Combine(uiCookingMinigameOverlayController.OnCook, new Action(OnCook));
			UICookingMinigameOverlayController uiCookingMinigameOverlayController2 = _uiCookingMinigameOverlayController;
			uiCookingMinigameOverlayController2.OnUnderCook = (Action)Delegate.Combine(uiCookingMinigameOverlayController2.OnUnderCook, new Action(OnUnderCook));
			UICookingMinigameOverlayController uiCookingMinigameOverlayController3 = _uiCookingMinigameOverlayController;
			uiCookingMinigameOverlayController3.OnBurn = (Action)Delegate.Combine(uiCookingMinigameOverlayController3.OnBurn, new Action(OnBurn));
			state.CURRENT_STATE = StateMachine.State.CustomAction0;
		});
	}

	private void OnCook()
	{
		AudioManager.Instance.PlayOneShot("event:/cooking/meal_cooked", base.transform.position);
		MealFinishedCooking();
		foreach (Follower follower in Follower.Followers)
		{
			if (!FollowerManager.FollowerLocked(follower.Brain.Info.ID, true))
			{
				follower.Brain.CheckChangeTask();
			}
		}
		if (StructureInfo.QueuedMeals.Count <= 0)
		{
			EndCooking();
		}
	}

	private void OnUnderCook()
	{
		AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", base.transform.position);
		structure.Structure_Info.QueuedMeals[0].MealType = InventoryItem.ITEM_TYPE.MEAL_BURNED;
		MealFinishedCooking();
		if (StructureInfo.QueuedMeals.Count <= 0)
		{
			EndCooking();
		}
	}

	private void OnBurn()
	{
		AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", base.transform.position);
		structure.Structure_Info.QueuedMeals[0].MealType = InventoryItem.ITEM_TYPE.MEAL_BURNED;
		MealFinishedCooking();
		if (StructureInfo.QueuedMeals.Count <= 0)
		{
			EndCooking();
		}
	}

	private void EndCooking()
	{
		StartCoroutine(DelayHideCooking());
		UICookingMinigameOverlayController uiCookingMinigameOverlayController = _uiCookingMinigameOverlayController;
		uiCookingMinigameOverlayController.OnCook = (Action)Delegate.Remove(uiCookingMinigameOverlayController.OnCook, new Action(OnCook));
		UICookingMinigameOverlayController uiCookingMinigameOverlayController2 = _uiCookingMinigameOverlayController;
		uiCookingMinigameOverlayController2.OnUnderCook = (Action)Delegate.Remove(uiCookingMinigameOverlayController2.OnUnderCook, new Action(OnUnderCook));
		UICookingMinigameOverlayController uiCookingMinigameOverlayController3 = _uiCookingMinigameOverlayController;
		uiCookingMinigameOverlayController3.OnBurn = (Action)Delegate.Remove(uiCookingMinigameOverlayController3.OnBurn, new Action(OnBurn));
		_uiCookingMinigameOverlayController = null;
		GameManager.GetInstance().OnConversationEnd();
	}

	private IEnumerator DelayHideCooking()
	{
		yield return new WaitForSeconds(0.2f);
		ShowCooking(false);
	}

	private void SkinGiven()
	{
		GameManager.GetInstance().OnConversationEnd();
	}

	private IEnumerator CreatePoopSkin()
	{
		yield return new WaitForSeconds(0.2f);
		ShowCooking(false);
		if ((bool)_uiCookingMinigameOverlayController)
		{
			UICookingMinigameOverlayController uiCookingMinigameOverlayController = _uiCookingMinigameOverlayController;
			uiCookingMinigameOverlayController.OnCook = (Action)Delegate.Remove(uiCookingMinigameOverlayController.OnCook, new Action(OnCook));
			UICookingMinigameOverlayController uiCookingMinigameOverlayController2 = _uiCookingMinigameOverlayController;
			uiCookingMinigameOverlayController2.OnUnderCook = (Action)Delegate.Remove(uiCookingMinigameOverlayController2.OnUnderCook, new Action(OnUnderCook));
			UICookingMinigameOverlayController uiCookingMinigameOverlayController3 = _uiCookingMinigameOverlayController;
			uiCookingMinigameOverlayController3.OnBurn = (Action)Delegate.Remove(uiCookingMinigameOverlayController3.OnBurn, new Action(OnBurn));
			_uiCookingMinigameOverlayController.Close();
		}
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 12f);
		yield return new WaitForSeconds(1f);
		CameraManager.instance.ShakeCameraForDuration(0f, 1f, 3f);
		MMVibrate.RumbleContinuous(0f, 1f);
		for (int i = 0; i < UnityEngine.Random.Range(10, 20); i++)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.POOP, 1, SpawnMealPosition.position, UnityEngine.Random.Range(9, 11), delegate(PickUp pickUp)
			{
				Meal component = pickUp.GetComponent<Meal>();
				component.CreateStructureLocation = StructureInfo.Location;
				component.CreateStructureOnStop = true;
				AudioManager.Instance.PlayOneShot("event:/followers/poop_pop");
			});
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.2f));
		}
		MMVibrate.StopRumble();
		AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win");
		FollowerSkinCustomTarget.Create(SpawnMealPosition.position, PlayerFarming.Instance.transform.position, 1f, "Poop", SkinGiven);
	}

	public virtual void MealFinishedCooking()
	{
		InventoryItem.ITEM_TYPE num = ((structure.Structure_Info.QueuedMeals.Count > 0) ? structure.Structure_Info.QueuedMeals[0].MealType : structure.Structure_Info.CurrentCookingMeal.MealType);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CookFirstMeal);
		DataManager.Instance.CookedFirstFood = true;
		if (num == InventoryItem.ITEM_TYPE.MEAL_POOP)
		{
			DataManager.Instance.PoopMealsCreated++;
			if (DataManager.Instance.PoopMealsCreated == UnityEngine.Random.Range(5, 12) && !DataManager.GetFollowerSkinUnlocked("Poop"))
			{
				StartCoroutine(CreatePoopSkin());
			}
			else if (DataManager.Instance.PoopMealsCreated >= 12 && !DataManager.GetFollowerSkinUnlocked("Poop"))
			{
				StartCoroutine(CreatePoopSkin());
			}
		}
		InventoryItem.Spawn(num, 1, SpawnMealPosition.position, UnityEngine.Random.Range(9, 11), delegate(PickUp pickUp)
		{
			Meal component = pickUp.GetComponent<Meal>();
			component.CreateStructureLocation = StructureInfo.Location;
			component.CreateStructureOnStop = true;
		});
		CookingData.CookedMeal(num);
		DataManager.Instance.MealsCooked++;
		ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
		if ((bool)CookingMealAnimation)
		{
			CookingMealAnimation.AnimationState.SetAnimation(0, "start", false);
			CookingMealAnimation.AnimationState.AddAnimation(0, "cook", true, 0f);
		}
		if (StructureInfo.QueuedMeals.Count > 0)
		{
			StructureInfo.QueuedMeals.RemoveAt(0);
		}
		StructureInfo.CurrentCookingMeal = null;
		StructureInfo.Fuel -= 10;
	}
}
