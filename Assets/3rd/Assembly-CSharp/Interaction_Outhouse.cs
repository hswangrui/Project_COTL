using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_Outhouse : Interaction
{
	public static List<Interaction_Outhouse> Outhouses = new List<Interaction_Outhouse>();

	public Structure _structure;

	public Transform OutsideFollowerPosition;

	public Transform InsideFollowerPosition;

	public Transform WaitingFollowerPosition;

	public GameObject DoorOpen;

	public GameObject DoorClosed;

	public ItemGauge ItemGauge;

	private Structures_Outhouse _StructureInfo;

	private string sString;

	private bool Activating;

	private GameObject Player;

	private float Delay;

	public float DistanceToTriggerDeposits = 5f;

	public GameObject FullOuthouse;

	public GameObject NormalOuthouse;

	public StructuresData StructureInfo
	{
		get
		{
			return _structure.Structure_Info;
		}
	}

	public Structures_Outhouse StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = _structure.Brain as Structures_Outhouse;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public bool IsFull
	{
		get
		{
			if (StructureBrain == null)
			{
				return false;
			}
			return StructureBrain.IsFull;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Outhouses.Add(this);
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Outhouses.Remove(this);
	}

	private void Start()
	{
		UpdateLocalisation();
		Structure structure = _structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (_structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnBrainAssigned()
	{
		Structure structure = _structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureBrain brain = _structure.Brain;
		brain.OnItemDeposited = (Action)Delegate.Combine(brain.OnItemDeposited, new Action(OnItemDeposited));
		StructureInfo.FollowerID = -1;
		OnItemDeposited();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.ReceiveDevotion;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_structure.Brain != null)
		{
			StructureBrain brain = _structure.Brain;
			brain.OnItemDeposited = (Action)Delegate.Remove(brain.OnItemDeposited, new Action(OnItemDeposited));
		}
	}

	public override void GetLabel()
	{
		if (StructureBrain == null)
		{
			base.Label = "";
			return;
		}
		int poopCount = StructureBrain.GetPoopCount();
		Interactable = poopCount > 0;
		base.Label = string.Join(" ", sString, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.POOP, poopCount, true, true));
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			Activating = true;
		}
	}

	private new void Update()
	{
		if ((Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		if (Activating && (StructureBrain.GetPoopCount() <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > DistanceToTriggerDeposits))
		{
			Activating = false;
		}
		if (StructureBrain.IsFull && !FullOuthouse.activeSelf)
		{
			FullOuthouse.SetActive(true);
			NormalOuthouse.SetActive(false);
		}
		else if (!StructureBrain.IsFull && FullOuthouse.activeSelf)
		{
			FullOuthouse.SetActive(false);
			NormalOuthouse.SetActive(true);
		}
		if ((Delay -= Time.deltaTime) < 0f && Activating)
		{
			InventoryItem inventoryItem = StructureInfo.Inventory[0];
			InventoryItem.ITEM_TYPE itemType = (InventoryItem.ITEM_TYPE)inventoryItem.type;
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			ResourceCustomTarget.Create(state.gameObject, base.transform.position, itemType, delegate
			{
				GiveItem(itemType);
			});
			if (--inventoryItem.quantity <= 0)
			{
				StructureInfo.Inventory.RemoveAt(0);
			}
			GetLabel();
			base.HasChanged = true;
			Delay = 0.2f;
		}
	}

	private void OnItemDeposited()
	{
		ItemGauge.SetPosition((float)StructureBrain.GetPoopCount() / (float)Structures_Outhouse.Capacity(StructureInfo.Type));
	}

	private void GiveItem(InventoryItem.ITEM_TYPE type)
	{
		Inventory.AddItem((int)type, 1);
		OnItemDeposited();
	}
}
