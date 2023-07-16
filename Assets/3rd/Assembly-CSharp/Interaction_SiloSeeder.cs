using System;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_SiloSeeder : Interaction
{
	public static List<Interaction_SiloSeeder> SiloSeeders = new List<Interaction_SiloSeeder>();

	public Structure Structure;

	private Structures_SiloSeed _StructureInfo;

	public Canvas CapacityIndicatorCanvas;

	public Image CapacityIndicator;

	public GameObject DepositIndicatorPrefab;

	public List<GameObject> FullStates;

	private List<InventoryItem> _itemsInTheAir;

	private float _delay;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_SiloSeed StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_SiloSeed;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 2f;
		CapacityIndicatorCanvas.gameObject.SetActive(false);
		_itemsInTheAir = new List<InventoryItem>();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		UpdateCapacityIndicators();
		SiloSeeders.Add(this);
	}

	public override void OnDisableInteraction()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		base.OnDisableInteraction();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SiloSeeders.Remove(this);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(InventoryItem.AllSeeds, new ItemSelector.Params
		{
			Key = "plant_seeds",
			Context = ItemSelector.Context.Add,
			Offset = new Vector2(0f, 250f),
			RequiresDiscovery = true,
			HideOnSelection = false,
			ShowEmpty = true
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			Debug.Log(string.Format("ItemToDeposit {0}", chosenItem).Colour(Color.yellow));
			InventoryItem item = new InventoryItem(chosenItem, 1);
			_itemsInTheAir.Add(item);
			Inventory.ChangeItemQuantity((int)chosenItem, -1);
			AudioManager.Instance.PlayOneShot("event:/material/footstep_grass", base.transform.position);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, chosenItem, DepositItem);
			Interactable = (float)GetCompostAndAirCount() < StructureBrain.Capacity;
			if (!Interactable)
			{
				itemSelector.Hide();
				base.HasChanged = true;
			}
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
		uIItemSelectorOverlayController2.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnHidden, (Action)delegate
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			itemSelector = null;
			Interactable = true;
			base.HasChanged = true;
		});
	}

	public override void GetLabel()
	{
		Interactable = (float)GetCompostAndAirCount() < StructureBrain.Capacity;
		base.Label = (((float)GetCompostAndAirCount() >= StructureBrain.Capacity) ? ScriptLocalization.Interactions.Full : ScriptLocalization.Interactions_Bank.Deposit);
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		CapacityIndicatorCanvas.gameObject.SetActive(true);
		UpdateCapacityIndicators();
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		CapacityIndicatorCanvas.gameObject.SetActive(false);
	}

	private void DepositItem()
	{
		Structure.DepositInventory(_itemsInTheAir[0]);
		_itemsInTheAir.RemoveAt(0);
		UpdateCapacityIndicators();
	}

	public void UpdateCapacityIndicators()
	{
		float num = (float)GetCompostCount() / StructureBrain.Capacity;
		CapacityIndicator.fillAmount = num;
		int num2 = -1;
		if (GetCompostCount() > 0)
		{
			num2 = Mathf.FloorToInt(num * (float)(FullStates.Count - 1));
		}
		for (int i = 0; i < FullStates.Count; i++)
		{
			FullStates[i].SetActive(i == num2);
		}
	}

	private int GetCompostCount()
	{
		int num = 0;
		foreach (InventoryItem item in StructureInfo.Inventory)
		{
			num += item.quantity;
		}
		return num;
	}

	private int GetCompostAndAirCount()
	{
		return GetCompostCount() + _itemsInTheAir.Count;
	}
}
