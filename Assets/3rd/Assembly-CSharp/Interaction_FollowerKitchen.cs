using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.KitchenMenu;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_FollowerKitchen : Interaction_Kitchen
{
	public static List<Interaction_FollowerKitchen> FollowerKitchens = new List<Interaction_FollowerKitchen>();

	[SerializeField]
	private Canvas uiCanvas;

	[SerializeField]
	private Image uiProgress;

	[SerializeField]
	private TextMeshProUGUI uiText;

	[SerializeField]
	public Interaction_FoodStorage foodStorage;

	[SerializeField]
	private GameObject cookPosition;

	private List<InventoryItem> _itemsInTheAir = new List<InventoryItem>();

	private InventoryItem.ITEM_TYPE currentMeal;

	public GameObject CookPosition
	{
		get
		{
			return cookPosition;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		FollowerKitchens.Add(this);
		if (structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		FollowerKitchens.Remove(this);
		if (structure != null && structure.Brain != null)
		{
			((Structures_Kitchen)structure.Brain).OnMealFinishedCooking -= UpdateCurrentMeal;
			Structure.OnItemDeposited = (Structure.StructureInventoryChanged)Delegate.Remove(Structure.OnItemDeposited, new Structure.StructureInventoryChanged(OnItemDeposited));
		}
	}

	protected override void OnBrainAssigned()
	{
		base.OnBrainAssigned();
		((Structures_Kitchen)structure.Brain).OnMealFinishedCooking += UpdateCurrentMeal;
		Structure.OnItemDeposited = (Structure.StructureInventoryChanged)Delegate.Combine(Structure.OnItemDeposited, new Structure.StructureInventoryChanged(OnItemDeposited));
		UpdateCurrentMeal();
		Structures_Kitchen structures_Kitchen = (Structures_Kitchen)structure.Brain;
		if (!structures_Kitchen.IsContainingFoodStorage)
		{
			structures_Kitchen.FoodStorage = new Structures_FoodStorage(0);
			structures_Kitchen.FoodStorage.Data = StructuresData.GetInfoByType(StructureBrain.TYPES.FOOD_STORAGE, 0);
			structures_Kitchen.FoodStorage.Data.QueuedMeals = new List<QueuedMeal>();
			foreach (InventoryItem.ITEM_TYPE queuedResource in structures_Kitchen.Data.QueuedResources)
			{
				structures_Kitchen.FoodStorage.DepositItemUnstacked(queuedResource);
			}
		}
		if (currentMeal != 0)
		{
			CookingMealAnimation.AnimationState.SetAnimation(0, "cook", true);
		}
		foodStorage.Structure.Brain = structures_Kitchen.FoodStorage;
		foodStorage.UpdateFoodDisplayed();
	}

	private void OnItemDeposited(Structure structure, InventoryItem item)
	{
		if (structure == base.structure)
		{
			UpdateCurrentMeal();
		}
	}

	public void UpdateCurrentMeal()
	{
		if (structure.Structure_Info.CurrentCookingMeal != null)
		{
			currentMeal = structure.Structure_Info.CurrentCookingMeal.MealType;
		}
		else if (((Structures_Kitchen)structure.Brain).GetBestPossibleMeal() != null)
		{
			currentMeal = ((Structures_Kitchen)structure.Brain).GetBestPossibleMeal().MealType;
		}
		else
		{
			currentMeal = InventoryItem.ITEM_TYPE.NONE;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.StructureInfo != null)
		{
			if (currentMeal != 0)
			{
				DisplayUI();
			}
			else
			{
				uiCanvas.gameObject.SetActive(false);
			}
		}
		CookingOn.SetActive(currentMeal != InventoryItem.ITEM_TYPE.NONE);
		CookingOff.SetActive(currentMeal == InventoryItem.ITEM_TYPE.NONE);
		CookingMealAnimation.gameObject.SetActive(currentMeal != InventoryItem.ITEM_TYPE.NONE);
		CookingMealAnimation.transform.position = foodStorage.itemDisplays[Mathf.Clamp(structure.Brain.Data.QueuedResources.Count, 0, foodStorage.StructureBrain.Capacity - 1)].transform.position;
	}

	public void DisplayUI()
	{
		uiCanvas.gameObject.SetActive(true);
		InventoryItem.ITEM_TYPE type = InventoryItem.ITEM_TYPE.NONE;
		if (structure.Structure_Info.CurrentCookingMeal != null)
		{
			type = structure.Structure_Info.CurrentCookingMeal.MealType;
		}
		else if (((Structures_Kitchen)structure.Brain).GetBestPossibleMeal() != null)
		{
			type = ((Structures_Kitchen)structure.Brain).GetBestPossibleMeal().MealType;
		}
		uiText.text = FontImageNames.GetIconByType(type);
		uiProgress.fillAmount = ((base.StructureInfo.CurrentCookingMeal != null) ? (base.StructureInfo.CurrentCookingMeal.CookedTime / base.StructureInfo.CurrentCookingMeal.CookingDuration) : 0f);
	}

	protected override void ShowMenu()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		GameManager.GetInstance().OnConversationNew();
		HUD_Manager.Instance.Hide(false, 0);
		Time.timeScale = 0f;
		UIFollowerKitchenMenuController menuController = MonoSingleton<UIManager>.Instance.FollowerKitchenMenuControllerTemplate.Instantiate();
		menuController.Show(base.StructureInfo, this);
		UIFollowerKitchenMenuController uIFollowerKitchenMenuController = menuController;
		uIFollowerKitchenMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerKitchenMenuController.OnHidden, (Action)delegate
		{
			menuController = null;
			base.HasChanged = true;
			Time.timeScale = 1f;
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			foreach (Follower item in FollowerManager.ActiveLocationFollowers())
			{
				item.Brain.CheckChangeTask();
			}
			GameManager.GetInstance().OnConversationEnd();
		});
		UIFollowerKitchenMenuController uIFollowerKitchenMenuController2 = menuController;
		uIFollowerKitchenMenuController2.OnItemQueued = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIFollowerKitchenMenuController2.OnItemQueued, (Action<InventoryItem.ITEM_TYPE>)delegate
		{
			UpdateCurrentMeal();
		});
		UIFollowerKitchenMenuController uIFollowerKitchenMenuController3 = menuController;
		uIFollowerKitchenMenuController3.OnItemRemovedFromQueue = (Action<InventoryItem.ITEM_TYPE, int>)Delegate.Combine(uIFollowerKitchenMenuController3.OnItemRemovedFromQueue, (Action<InventoryItem.ITEM_TYPE, int>)delegate(InventoryItem.ITEM_TYPE item, int index)
		{
			if (index == 0)
			{
				base.StructureInfo.CurrentCookingMeal = null;
			}
			UpdateCurrentMeal();
		});
	}

	private void DepositItem()
	{
		structure.DepositInventory(_itemsInTheAir[0]);
		_itemsInTheAir.RemoveAt(0);
	}

	public override void MealFinishedCooking()
	{
		if (currentMeal == InventoryItem.ITEM_TYPE.NONE)
		{
			if (structure.Structure_Info.CurrentCookingMeal != null)
			{
				currentMeal = structure.Structure_Info.CurrentCookingMeal.MealType;
			}
			else if (((Structures_Kitchen)structure.Brain).GetBestPossibleMeal() != null)
			{
				currentMeal = ((Structures_Kitchen)structure.Brain).GetBestPossibleMeal().MealType;
			}
			else
			{
				currentMeal = InventoryItem.ITEM_TYPE.NONE;
			}
		}
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CookFirstMeal);
		DataManager.Instance.CookedFirstFood = true;
		structure.Brain.Data.QueuedResources.Add(currentMeal);
		if (foodStorage != null && base.StructureInfo != null)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			foodStorage.StructureBrain.DepositItemUnstacked(currentMeal);
			foodStorage.UpdateFoodDisplayed();
			foodStorage.itemDisplays[structure.Brain.Data.QueuedResources.Count - 1].transform.DOPunchScale(Vector3.one * 0.25f, 0.25f).SetEase(Ease.OutBounce);
			CookingMealAnimation.GetComponent<MeshRenderer>().enabled = false;
		}
		foreach (Follower follower in Follower.Followers)
		{
			if (follower.Brain.CurrentTaskType != FollowerTaskType.Cook)
			{
				follower.Brain.CheckChangeTask();
			}
		}
		CookingData.CookedMeal(currentMeal);
		DataManager.Instance.MealsCooked++;
		ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
		base.StructureInfo.CurrentCookingMeal = null;
		base.StructureInfo.Fuel -= 10;
		if (((Structures_Kitchen)structure.Brain).FoodStorage.Data.QueuedResources.Count < foodStorage.StructureBrain.Capacity - 1)
		{
			CookingMealAnimation.AnimationState.SetAnimation(0, "start", false);
			CookingMealAnimation.AnimationState.AddAnimation(0, "cook", true, 0f);
			StartCoroutine(Delay(0.1f, delegate
			{
				CookingMealAnimation.GetComponent<MeshRenderer>().enabled = true;
			}));
		}
	}

	private IEnumerator Delay(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}
}
