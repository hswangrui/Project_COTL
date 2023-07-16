using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_AddFuel : Interaction
{
	public delegate void FuelEvent(float fuel);

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_0
	{
		public UIItemSelectorOverlayController itemSelector;

		public StateMachine state;

		public Interaction_AddFuel _003C_003E4__this;

		public Action _003C_003E9__2;

		internal void _003COnInteract_003Eb__1(InventoryItem.ITEM_TYPE chosenItem)
		{
			_003COnInteract_003Eg__AddFuel_007C0(chosenItem, delegate
			{
				itemSelector.Hide();
			});
		}

		internal void _003COnInteract_003Eb__2()
		{
			itemSelector.Hide();
		}

		internal void _003COnInteract_003Eb__3()
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			itemSelector = null;
			_003C_003E4__this.Interactable = !_003C_003E4__this.structure.Structure_Info.FullyFueled;
			_003C_003E4__this.HasChanged = true;
		}

		internal void _003COnInteract_003Eg__AddFuel_007C0(InventoryItem.ITEM_TYPE itemType, Action onFull)
		{
			Debug.Log(string.Format("Deposit {0} to add fuel", itemType).Colour(Color.yellow));
			if (itemType == InventoryItem.ITEM_TYPE.LOG)
			{
				AudioManager.Instance.PlayOneShot("event:/cooking/add_wood", _003C_003E4__this.transform.position);
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/material/footstep_bush", _003C_003E4__this.transform.position);
			}
			ResourceCustomTarget.Create(_003C_003E4__this.gameObject, PlayerFarming.Instance.transform.position, itemType, null);
			Inventory.ChangeItemQuantity((int)itemType, -1);
			_003C_003E4__this.structure.Structure_Info.Fuel = Mathf.Clamp(_003C_003E4__this.structure.Structure_Info.Fuel + InventoryItem.FuelWeight(itemType), 0, _003C_003E4__this.structure.Structure_Info.MaxFuel);
			FuelEvent onFuelModified = _003C_003E4__this.OnFuelModified;
			if (onFuelModified != null)
			{
				onFuelModified((float)_003C_003E4__this.structure.Structure_Info.Fuel / (float)_003C_003E4__this.structure.Structure_Info.MaxFuel);
			}
			if (_003C_003E4__this.structure.Structure_Info.Fuel >= _003C_003E4__this.structure.Structure_Info.MaxFuel)
			{
				AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", _003C_003E4__this.transform.position);
				_003C_003E4__this.structure.Structure_Info.FullyFueled = true;
				UnityEvent onFireFullyFueld = _003C_003E4__this.onFireFullyFueld;
				if (onFireFullyFueld != null)
				{
					onFireFullyFueld.Invoke();
				}
				FuelEvent onFuelModified2 = _003C_003E4__this.OnFuelModified;
				if (onFuelModified2 != null)
				{
					onFuelModified2(1f);
				}
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.AddFuelToCookingFire);
				if (onFull != null)
				{
					onFull();
				}
			}
		}
	}

	[SerializeField]
	private Structure structure;

	[SerializeField]
	private List<InventoryItem.ITEM_TYPE> fuel = new List<InventoryItem.ITEM_TYPE>();

	[SerializeField]
	public string fuelKey = "ordinary_fuel";

	public int MaxFuel = 10;

	[Space]
	[SerializeField]
	private bool onlyDepleteWhenFullyFueled;

	[SerializeField]
	private bool hideIfEmpty;

	public Interaction OtherInteraction;

	[Space]
	[SerializeField]
	private UnityEvent onFireOn;

	[SerializeField]
	private UnityEvent onFireOff;

	[SerializeField]
	private UnityEvent onFireFullyFueld;

	[SerializeField]
	private Vector3 fuelBarOffset;

	[SerializeField]
	private GameObject noFuelIcon;

	private Coroutine addFuelRoutine;

	private UIAddFuel fuelUI;

	private bool beingMoved;

	private bool activiating;

	private bool firstPress;

	private float delay;

	private int oldFuelAmount = -1;

	public bool StringForHealingBay;

	public Structure Structure
	{
		get
		{
			return structure;
		}
	}

	public event FuelEvent OnFuelModified;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (fuelUI == null)
		{
			fuelUI = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/UI Add Fuel"), GameObject.FindGameObjectWithTag("Canvas").transform).GetComponent<UIAddFuel>();
		}
		fuelUI.offset = fuelBarOffset;
		PlacementRegion.OnBuildingBeganMoving += OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced += OnBuildingPlaced;
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		structure.Structure_Info.onlyDepleteWhenFullyFueled = onlyDepleteWhenFullyFueled;
		structure.Structure_Info.MaxFuel = MaxFuel;
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureBrain brain = structure.Brain;
		brain.OnFuelModified = (Action<float>)Delegate.Combine(brain.OnFuelModified, new Action<float>(OnBrainFuelModified));
	}

	private void OnBrainFuelModified(float Delta)
	{
		FuelEvent onFuelModified = this.OnFuelModified;
		if (onFuelModified != null)
		{
			onFuelModified(Delta);
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		PlacementRegion.OnBuildingBeganMoving -= OnBuildingBeganMoving;
		PlacementRegion.OnBuildingPlaced -= OnBuildingPlaced;
		if ((bool)structure)
		{
			Structure obj = structure;
			obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		if ((bool)structure && structure.Brain != null)
		{
			StructureBrain brain = structure.Brain;
			brain.OnFuelModified = (Action<float>)Delegate.Remove(brain.OnFuelModified, new Action<float>(OnBrainFuelModified));
		}
		activiating = false;
		UIAddFuel uIAddFuel = fuelUI;
		if ((object)uIAddFuel != null)
		{
			uIAddFuel.Hide();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!(structure != null) || structure.Structure_Info == null || structure.Structure_Info.Fuel == oldFuelAmount)
		{
			return;
		}
		if (structure.Structure_Info.Fuel > 0)
		{
			UnityEvent unityEvent = onFireOn;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
		else
		{
			structure.Structure_Info.FullyFueled = false;
			UnityEvent unityEvent2 = onFireOff;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke();
			}
			Interactable = true;
		}
		if (structure.Structure_Info.FullyFueled)
		{
			UnityEvent unityEvent3 = onFireFullyFueld;
			if (unityEvent3 != null)
			{
				unityEvent3.Invoke();
			}
		}
		oldFuelAmount = structure.Structure_Info.Fuel;
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
		if (structureID == obj2 && (bool)fuelUI)
		{
			fuelUI.Hide();
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
		if (structureID == obj2 && (bool)fuelUI)
		{
			beingMoved = false;
		}
	}

	public override void GetLabel()
	{
		base.GetLabel();
		if (!structure.Structure_Info.FullyFueled)
		{
			if (StringForHealingBay)
			{
				base.Label = ScriptLocalization.Interactions.AddIngredients;
			}
			else if (fuel.Count == 1)
			{
				base.Label = ScriptLocalization.Interactions.AddFuel + " " + CostFormatter.FormatCost(fuel[0], 1);
			}
			else
			{
				base.Label = ScriptLocalization.Interactions.AddFuel;
			}
		}
		else
		{
			base.Label = ScriptLocalization.Interactions.Full;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		_003C_003Ec__DisplayClass34_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass34_0();
		CS_0024_003C_003E8__locals0.state = state;
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		if (structure.Structure_Info.FullyFueled)
		{
			return;
		}
		CS_0024_003C_003E8__locals0.state.CURRENT_STATE = StateMachine.State.InActive;
		CS_0024_003C_003E8__locals0.state.facingAngle = Utils.GetAngle(CS_0024_003C_003E8__locals0.state.transform.position, base.transform.position);
		CS_0024_003C_003E8__locals0.itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(fuel, new ItemSelector.Params
		{
			Key = fuelKey,
			Context = ItemSelector.Context.Add,
			Offset = new Vector2(0f, 150f),
			ShowEmpty = true
		});
		UIItemSelectorOverlayController itemSelector = CS_0024_003C_003E8__locals0.itemSelector;
		itemSelector.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(itemSelector.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			CS_0024_003C_003E8__locals0._003COnInteract_003Eg__AddFuel_007C0(chosenItem, delegate
			{
				CS_0024_003C_003E8__locals0.itemSelector.Hide();
			});
		});
		UIItemSelectorOverlayController itemSelector2 = CS_0024_003C_003E8__locals0.itemSelector;
		itemSelector2.OnHidden = (Action)Delegate.Combine(itemSelector2.OnHidden, (Action)delegate
		{
			CS_0024_003C_003E8__locals0.state.CURRENT_STATE = StateMachine.State.Idle;
			CS_0024_003C_003E8__locals0.itemSelector = null;
			CS_0024_003C_003E8__locals0._003C_003E4__this.Interactable = !CS_0024_003C_003E8__locals0._003C_003E4__this.structure.Structure_Info.FullyFueled;
			CS_0024_003C_003E8__locals0._003C_003E4__this.HasChanged = true;
		});
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		Interactable = !structure.Structure_Info.FullyFueled;
		if (fuelUI.IsShowing || !Interactable)
		{
			return;
		}
		fuelUI.Show(this);
		if (!noFuelIcon)
		{
			return;
		}
		noFuelIcon.transform.DOKill();
		if (noFuelIcon.transform.localScale != Vector3.zero)
		{
			noFuelIcon.transform.localScale = Vector3.one;
			noFuelIcon.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(delegate
			{
				noFuelIcon.gameObject.SetActive(false);
			});
		}
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		if (fuelUI.IsShowing)
		{
			fuelUI.Hide();
			if ((bool)noFuelIcon && !Structure.Brain.Data.FullyFueled)
			{
				noFuelIcon.transform.DOKill();
				noFuelIcon.transform.localScale = Vector3.zero;
				noFuelIcon.gameObject.SetActive(true);
				noFuelIcon.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
			}
		}
	}
}
