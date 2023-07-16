using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.BuildMenu;
using src.Extensions;
using UnityEngine;

public class Interaction_PlacementRegion : Interaction
{
	private string sString;

	private string sTutorial;

	private string sShrineBuilding;

	public PlacementRegion placementRegion;

	public GameObject CameraTarget;

	public GameObject ActiveObject;

	public GameObject InactiveObject;

	public GameObject TutorialObject;

	private List<int> Costs = new List<int> { 0, 5, 10, 20, 30, 50 };

	public bool UseWhiteList;

	public List<StructureBrain.TYPES> WhiteList;

	public GameObject NewBuildingAvailableObject;

	public Animator newBuildingAnimator;

	public GameObject TechTree;

	public int Cost
	{
		get
		{
			int num = 0;
			foreach (PlacementRegion placementRegion in PlacementRegion.PlacementRegions)
			{
				if (placementRegion.StructureInfo.Purchased)
				{
					num++;
				}
			}
			return Costs[Mathf.Min(Costs.Count, num)];
		}
	}

	private void Start()
	{
		IgnoreTutorial = true;
		HasSecondaryInteraction = true;
		UpdateLocalisation();
		ActiveObject.SetActive(true);
		InactiveObject.SetActive(false);
		HasSecondaryInteraction = false;
		OnBuildingUnlocked();
	}

	private void OnBuildingUnlocked()
	{
		if (DataManager.Instance.NewBuildings)
		{
			NewBuildingAvailableObject.SetActive(true);
			return;
		}
		newBuildingAnimator.Play("Hide");
		StartCoroutine(WaitToTurnOffBuilding());
	}

	private IEnumerator WaitToTurnOffBuilding()
	{
		yield return new WaitForSeconds(1.5f);
		NewBuildingAvailableObject.SetActive(false);
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		UpgradeSystem.OnBuildingUnlocked = (Action)Delegate.Combine(UpgradeSystem.OnBuildingUnlocked, new Action(OnBuildingUnlocked));
	}

	public override void OnDisableInteraction()
	{
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
		UpgradeSystem.OnBuildingUnlocked = (Action)Delegate.Remove(UpgradeSystem.OnBuildingUnlocked, new Action(OnBuildingUnlocked));
	}

	private void OnStructuresPlaced()
	{
		if (Cost == 0 && placementRegion != null)
		{
			placementRegion.StructureInfo.Purchased = true;
		}
	}

	private void SetGameObjects()
	{
		if (placementRegion.StructureInfo.Purchased)
		{
			ActiveObject.SetActive(true);
			InactiveObject.SetActive(false);
		}
		else
		{
			ActiveObject.SetActive(false);
			InactiveObject.SetActive(true);
		}
	}

	public override void GetLabel()
	{
		if (!DataManager.Instance.AllowBuilding)
		{
			Interactable = false;
			base.Label = sTutorial;
		}
		else if (BuildSitePlot.StructureOfTypeUnderConstruction(StructureBrain.TYPES.SHRINE) || BuildSitePlotProject.StructureOfTypeUnderConstruction(StructureBrain.TYPES.SHRINE))
		{
			Interactable = false;
			base.Label = sShrineBuilding;
		}
		else
		{
			Interactable = true;
			base.Label = sString;
		}
	}

	public override void GetSecondaryLabel()
	{
		int shrineLevel = DataManager.Instance.ShrineLevel;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.Build;
		sTutorial = ScriptLocalization.Interactions.IndoctrinateFollowerBeforeBuilding;
		sShrineBuilding = ScriptLocalization.Interactions.ShrineUnderConstruction;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		state.CURRENT_STATE = StateMachine.State.InActive;
		DataManager.Instance.NewBuildings = false;
		UIBuildMenuController uIBuildMenuController = MonoSingleton<UIManager>.Instance.BuildMenuTemplate.Instantiate();
		uIBuildMenuController.Show();
		uIBuildMenuController.OnBuildingChosen = (Action<StructureBrain.TYPES>)Delegate.Combine(uIBuildMenuController.OnBuildingChosen, new Action<StructureBrain.TYPES>(PlaceBuilding));
		uIBuildMenuController.OnCancel = (Action)Delegate.Combine(uIBuildMenuController.OnCancel, (Action)delegate
		{
			HUD_Manager.Instance.Show(0);
			Time.timeScale = 1f;
			Cancel();
		});
		HUD_Manager.Instance.Hide(false, 0);
		Time.timeScale = 0f;
		if (TutorialObject != null)
		{
			TutorialObject.SetActive(false);
		}
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
	}

	private IEnumerator PurchaseRoutine()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 7f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= Cost)
			{
				break;
			}
			SoulCustomTarget.Create(base.gameObject, state.gameObject.transform.position, Color.white, null);
			PlayerFarming.Instance.GetSoul(-1);
			yield return new WaitForSeconds(0.1f - 0.1f * (float)(i / Cost));
		}
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, state.transform.position));
		placementRegion.StructureInfo.Purchased = true;
		SetGameObjects();
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
	}

	private void PlaceBuilding(StructureBrain.TYPES structureType)
	{
		NotificationCentreScreen.Instance.Stop();
		OnBuildingUnlocked();
		placementRegion.PlacementGameObject = TypeAndPlacementObjects.GetByType(structureType).PlacementObject;
		placementRegion.StructureType = structureType;
		placementRegion.Play();
	}

	private void Cancel()
	{
		OnBuildingUnlocked();
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
