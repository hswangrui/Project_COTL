using System;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.RefineryMenu;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_Refinery : Interaction
{
	public enum State
	{
		Available,
		InProgress
	}

	private float Delay;

	public Canvas UICanvas;

	public Image UIProgress;

	public TextMeshProUGUI UIText;

	public TextMeshProUGUI UIQuantityText;

	public static List<Interaction_Refinery> Refineries = new List<Interaction_Refinery>();

	public Structure Structure;

	private Structures_Refinery _StructureInfo;

	public GameObject FollowerPosition;

	private bool beingMoved;

	public GameObject OnEffects;

	private string sDeposit;

	public State CurrentState;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Refinery StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Refinery;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void Start()
	{
		UICanvas.gameObject.SetActive(false);
		UpdateLocalisation();
	}

	public override void OnEnableInteraction()
	{
		ActivateDistance = 2.5f;
		base.OnEnableInteraction();
		Refineries.Add(this);
		PlacementRegion.OnBuildingBeganMoving += OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced += OnBuildingPlaced;
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			OnBrainAssigned();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Refineries.Remove(this);
		PlacementRegion.OnBuildingBeganMoving -= OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced -= OnBuildingPlaced;
		if ((bool)Structure)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		if (StructureBrain != null)
		{
			Structures_Refinery structureBrain = StructureBrain;
			structureBrain.OnCompleteRefining = (Action)Delegate.Remove(structureBrain.OnCompleteRefining, new Action(OnCompleteRefining));
		}
	}

	private void OnBrainAssigned()
	{
		Structures_Refinery structureBrain = StructureBrain;
		structureBrain.OnCompleteRefining = (Action)Delegate.Combine(structureBrain.OnCompleteRefining, new Action(OnCompleteRefining));
		CheckPhase();
	}

	private void OnBuildingBeganMoving(int structureID)
	{
		Structure structure = Structure;
		int? obj;
		if ((object)structure == null)
		{
			obj = null;
		}
		else
		{
			StructuresData structure_Info = structure.Structure_Info;
			obj = ((structure_Info != null) ? new int?(structure_Info.ID) : null);
		}
		if (structureID == obj)
		{
			beingMoved = true;
		}
	}

	private void OnBuildingPlaced(int structureID)
	{
		Structure structure = Structure;
		int? obj;
		if ((object)structure == null)
		{
			obj = null;
		}
		else
		{
			StructuresData structure_Info = structure.Structure_Info;
			obj = ((structure_Info != null) ? new int?(structure_Info.ID) : null);
		}
		if (structureID == obj)
		{
			beingMoved = false;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sDeposit = ScriptLocalization.Interactions.SanctifyResources;
	}

	private void CheckPhase()
	{
		if (StructureInfo.QueuedResources.Count > 0)
		{
			CurrentState = State.InProgress;
		}
		else
		{
			CurrentState = State.Available;
		}
	}

	public void OnCompleteRefining()
	{
		if (StructureInfo.QueuedResources.Count <= 0)
		{
			UICanvas.gameObject.SetActive(false);
			OnEffects.SetActive(false);
			CurrentState = State.Available;
		}
	}

	public override void GetLabel()
	{
		base.Label = sDeposit;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		GameManager.GetInstance().OnConversationNew();
		HUD_Manager.Instance.Hide(false, 0);
		Time.timeScale = 0f;
		UIRefineryMenuController uIRefineryMenuController = MonoSingleton<UIManager>.Instance.RefineryMenuTemplate.Instantiate();
		uIRefineryMenuController.Show(StructureInfo, this);
		uIRefineryMenuController.OnHide = (Action)Delegate.Combine(uIRefineryMenuController.OnHide, (Action)delegate
		{
			HUD_Manager.Instance.Show(0);
		});
		uIRefineryMenuController.OnHidden = (Action)Delegate.Combine(uIRefineryMenuController.OnHidden, (Action)delegate
		{
			Time.timeScale = 1f;
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			foreach (Follower item in FollowerManager.ActiveLocationFollowers())
			{
				item.Brain.CheckChangeTask();
			}
			GameManager.GetInstance().OnConversationEnd();
		});
		uIRefineryMenuController.OnItemQueued = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIRefineryMenuController.OnItemQueued, (Action<InventoryItem.ITEM_TYPE>)delegate
		{
			CurrentState = State.InProgress;
		});
	}

	protected override void Update()
	{
		base.Update();
		if (PlayerFarming.Instance == null)
		{
			return;
		}
		if (StructureBrain == null)
		{
			Debug.Log("Structure Brain Null!");
			return;
		}
		switch (CurrentState)
		{
		case State.Available:
			UICanvas.gameObject.SetActive(false);
			UIText.text = "";
			UIQuantityText.text = "";
			UIProgress.fillAmount = 0f;
			OnEffects.SetActive(false);
			break;
		case State.InProgress:
			DisplayUI();
			break;
		}
		if (Delay > 0f)
		{
			Delay = Mathf.Clamp(Delay - Time.deltaTime, 0f, float.MaxValue);
		}
	}

	private void DisplayUI()
	{
		if (!UICanvas.gameObject.activeSelf)
		{
			UICanvas.gameObject.SetActive(true);
		}
		OnEffects.SetActive(true);
		if (StructureInfo.QueuedResources.Count > 0)
		{
			UIText.text = FontImageNames.GetIconByType(StructureInfo.QueuedResources[0]);
			UIQuantityText.text = "x" + StructureInfo.QueuedResources.Count;
			UIProgress.fillAmount = StructureInfo.Progress / ((Structures_Refinery)Structure.Brain).RefineryDuration(StructureInfo.QueuedResources[0]);
		}
	}
}
