using System;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interaction_CollectResourceChest : Interaction
{
	public GameObject ChestOpen;

	public GameObject ChestClosed;

	public Structure Structure;

	private StructureBrain _StructureInfo;

	private string sString;

	private Vector3 PunchScale = new Vector3(0.1f, 0.1f, 0.1f);

	private bool Activating;

	private GameObject Player;

	private float Delay;

	public float DistanceToTriggerDeposits = 5f;

	private float delayBetweenChecks;

	private const float DELAY_DELTA = 0.4f;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public StructureBrain StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain;
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
		UpdateLocalisation();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		ContinuouslyHold = true;
		if (StructureBrain != null)
		{
			StructureBrain structureBrain = StructureBrain;
			structureBrain.OnItemDeposited = (Action)Delegate.Combine(structureBrain.OnItemDeposited, new Action(DepositItem));
		}
		UpdateChest();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			StructureBrain structureBrain = StructureBrain;
			structureBrain.OnItemDeposited = (Action)Delegate.Remove(structureBrain.OnItemDeposited, new Action(DepositItem));
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.ReceiveDevotion;
	}

	private void OnBrainAssigned()
	{
		StructureBrain structureBrain = StructureBrain;
		structureBrain.OnItemDeposited = (Action)Delegate.Combine(structureBrain.OnItemDeposited, new Action(DepositItem));
		UpdateChest();
	}

	public override void GetLabel()
	{
		if (StructureBrain == null)
		{
			base.Label = "";
			return;
		}
		if (StructureInfo.Inventory.Count <= 0)
		{
			Interactable = false;
			base.Label = "";
			return;
		}
		Interactable = true;
		base.Label = string.Join(" ", sString, CostFormatter.FormatCosts(StructureInfo.Inventory, true, true));
	}

	public void DepositItem()
	{
		base.transform.DOKill();
		base.transform.localScale = Vector3.one;
		base.transform.DOPunchScale(new Vector3(0.2f, 0.1f), 1f, 5);
		if (!ChestOpen.activeSelf)
		{
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_open");
		}
		UpdateChest();
	}

	private void UpdateChest()
	{
		if (StructureBrain != null && StructureBrain.Data != null && StructureBrain.Data.Inventory != null && StructureBrain.Data.Inventory.Count > 0)
		{
			ChestOpen.SetActive(true);
			ChestClosed.SetActive(false);
		}
		else
		{
			ChestOpen.SetActive(false);
			ChestClosed.SetActive(true);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating && StructureBrain.Data.Inventory.Count > 0)
		{
			base.OnInteract(state);
			Activating = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		if ((delayBetweenChecks -= Time.deltaTime) >= 0f && !InputManager.Gameplay.GetInteractButtonHeld())
		{
			Activating = false;
			return;
		}
		delayBetweenChecks = 0.4f;
		if ((Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		if (Activating && (StructureInfo.Inventory.Count <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > DistanceToTriggerDeposits))
		{
			Activating = false;
		}
		if ((Delay -= Time.deltaTime) < 0f && Activating)
		{
			InventoryItem inventoryItem = StructureInfo.Inventory[0];
			InventoryItem.ITEM_TYPE type = (InventoryItem.ITEM_TYPE)inventoryItem.type;
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.transform.position);
			for (int i = 0; i < Mathf.Min(inventoryItem.quantity, 5); i++)
			{
				ResourceCustomTarget.Create(Player.gameObject, base.transform.position, type, null);
			}
			GiveItem(type, inventoryItem.quantity);
			StructureInfo.Inventory.RemoveAt(0);
			base.transform.DOKill();
			base.transform.localScale = Vector3.one;
			base.transform.DOPunchScale(PunchScale, 1f);
			if (StructureBrain.Data.Inventory.Count <= 0 && ChestOpen.activeSelf)
			{
				AudioManager.Instance.PlayOneShot("event:/chests/chest_small_land");
			}
			UpdateChest();
			Delay = 0.1f;
			if (StructureInfo.Inventory.Count <= 0 && StructureInfo.Exhausted)
			{
				StructureBrain.Remove();
			}
		}
	}

	private void GiveItem(InventoryItem.ITEM_TYPE type, int amount)
	{
		Inventory.AddItem((int)type, amount);
	}
}
