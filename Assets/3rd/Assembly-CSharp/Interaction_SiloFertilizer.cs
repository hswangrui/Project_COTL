using System;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_SiloFertilizer : Interaction
{
	public static List<Interaction_SiloFertilizer> SiloFertilizers = new List<Interaction_SiloFertilizer>();

	public Structure Structure;

	private Structures_SiloFertiliser _StructureInfo;

	public Canvas CapacityIndicatorCanvas;

	public Image CapacityIndicator;

	public GameObject DepositIndicatorPrefab;

	public List<GameObject> FullStates;

	private bool _activating;

	private float _delay;

	private int _currentlyDepositing;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_SiloFertiliser StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_SiloFertiliser;
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
		CapacityIndicatorCanvas.gameObject.SetActive(false);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		UpdateCapacityIndicators();
		SiloFertilizers.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SiloFertilizers.Remove(this);
	}


	private  void AddFertilizer(Action onFull)
	{
		_currentlyDepositing++;
		InventoryItem.ITEM_TYPE iTEM_TYPE = InventoryItem.ITEM_TYPE.POOP;
		InventoryItem item = new InventoryItem(iTEM_TYPE, 1);
		ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, iTEM_TYPE, delegate
		{
			DepositItem(item);
		});
		Inventory.ChangeItemQuantity((int)iTEM_TYPE, -1);
		if (AtCapacity())
		{
			onFull?.Invoke();
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (AtCapacity())
		{
			return;
		}
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(new List<InventoryItem.ITEM_TYPE> { InventoryItem.ITEM_TYPE.POOP }, new ItemSelector.Params
		{
			Key = "fertiliser",
			Context = ItemSelector.Context.Add,
			Offset = new Vector2(0f, 250f),
			ShowEmpty = true
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate
		{
			AddFertilizer(delegate
			{
				itemSelector.Hide();
			});
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
		uIItemSelectorOverlayController2.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnHidden, (Action)delegate
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			itemSelector = null;
			Interactable = !AtCapacity() && HasRequiredItem();
			base.HasChanged = true;
			GetLabel();
		});
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.POOP) > 0)
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	public override void GetLabel()
	{
		if (AtCapacity())
		{
			base.Label = ScriptLocalization.Interactions.Full;
			return;
		}
		base.Label = string.Join(" ", ScriptLocalization.Interactions_Bank.Deposit, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.POOP, 1));
	}

	private bool AtCapacity()
	{
		return (float)(Structure.Inventory.Count + _currentlyDepositing) >= StructureBrain.Capacity;
	}

	private bool HasRequiredItem()
	{
		return Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.POOP) > 0;
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		CapacityIndicatorCanvas.gameObject.SetActive(true);
		UpdateCapacityIndicators();
		Interactable = !AtCapacity() && HasRequiredItem();
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		CapacityIndicatorCanvas.gameObject.SetActive(false);
	}

	private void DepositItem(InventoryItem item)
	{
		_currentlyDepositing--;
		Structure.DepositInventory(item);
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
}
