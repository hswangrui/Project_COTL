using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class LumberjackStation : Interaction
{
	public static List<LumberjackStation> LumberjackStations = new List<LumberjackStation>();

	public Structure Structure;

	public GameObject NormalBuilding;

	public GameObject ExhaustedBuilding;

	private Structures_LumberjackStation _StructureInfo;

	public GameObject FollowerPosition;

	public GameObject ChestPosition;

	public GameObject ChestOpen;

	public GameObject ChestClosed;

	public GameObject ItemIndicator;

	private string sString;

	private Vector3 PunchScale = new Vector3(0.1f, 0.1f, 0.1f);

	private bool Activating;

	private GameObject Player;

	private float Delay;

	public float DistanceToTriggerDeposits = 5f;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_LumberjackStation StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_LumberjackStation;
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
		ContinuouslyHold = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.ReceiveDevotion;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		LumberjackStations.Add(this);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (StructureBrain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_LumberjackStation structureBrain = StructureBrain;
		structureBrain.OnExhauted = (Action)Delegate.Combine(structureBrain.OnExhauted, new Action(OnExhausted));
		if (StructureBrain.Data.Exhausted)
		{
			OnExhausted();
		}
		UpdateChest();
	}

	public void DepositItem()
	{
		ChestPosition.transform.DOKill();
		ChestPosition.transform.localScale = Vector3.one;
		ChestPosition.transform.DOPunchScale(PunchScale, 1f);
		if (Structure != null && (Structure.Type == global::StructureBrain.TYPES.BLOODSTONE_MINE || Structure.Type == global::StructureBrain.TYPES.BLOODSTONE_MINE_2))
		{
			AudioManager.Instance.PlayOneShot("event:/building/place_building_spot", base.transform.position);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/cooking/add_wood", base.transform.position);
		}
		UpdateChest();
	}

	private void UpdateChest(bool playSFX = true)
	{
		if (StructureBrain.Data.Inventory.Count > 0)
		{
			ItemIndicator.SetActive(true);
			ChestOpen.SetActive(true);
			ChestClosed.SetActive(false);
			return;
		}
		if (!ChestClosed.activeSelf)
		{
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_land", base.transform.position);
		}
		ItemIndicator.SetActive(false);
		ChestOpen.SetActive(false);
		ChestClosed.SetActive(true);
	}

	private void MakeExhausted()
	{
		StructureInfo.Age = 500;
		StructureBrain.IncreaseAge();
	}

	private void OnExhausted()
	{
		NormalBuilding.SetActive(false);
		ExhaustedBuilding.SetActive(true);
		ExhaustedBuilding.transform.DOPunchScale(new Vector3(0.3f, 0.1f), 0.5f, 5, 0.5f);
		if (Structure != null && (Structure.Type == global::StructureBrain.TYPES.BLOODSTONE_MINE || Structure.Type == global::StructureBrain.TYPES.BLOODSTONE_MINE_2))
		{
			NotificationCentre.Instance.PlayGenericNotification("Notifications/BuildingOutOfResources_Stonemine");
		}
		else
		{
			NotificationCentre.Instance.PlayGenericNotification("Notifications/BuildingOutOfResources_Lumberjack");
		}
	}

	public override void GetLabel()
	{
		if (StructureBrain == null)
		{
			base.Label = "";
			return;
		}
		int count = StructureInfo.Inventory.Count;
		Interactable = count > 0;
		base.Label = string.Join(" ", sString, CostFormatter.FormatCost(StructureInfo.LootToDrop, count, true, true));
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if ((bool)Structure)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		if (StructureBrain != null)
		{
			Structures_LumberjackStation structureBrain = StructureBrain;
			structureBrain.OnExhauted = (Action)Delegate.Remove(structureBrain.OnExhauted, new Action(OnExhausted));
		}
		LumberjackStations.Remove(this);
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
		if (Player == null)
		{
			Player = GameObject.FindWithTag("Player");
			if (Player == null)
			{
				return;
			}
		}
		if (Activating && (StructureInfo.Inventory.Count <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > DistanceToTriggerDeposits))
		{
			Activating = false;
		}
		if (!((Delay -= Time.deltaTime) < 0f) || !Activating)
		{
			return;
		}
		InventoryItem inventoryItem = StructureInfo.Inventory[0];
		InventoryItem.ITEM_TYPE itemType = (InventoryItem.ITEM_TYPE)inventoryItem.type;
		ResourceCustomTarget.Create(Player.gameObject, base.transform.position, itemType, delegate
		{
			GiveItem(itemType);
		});
		AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.transform.position);
		StructureInfo.Inventory.RemoveAt(0);
		ChestPosition.transform.DOKill();
		ChestPosition.transform.localScale = Vector3.one;
		ChestPosition.transform.DOPunchScale(PunchScale, 1f);
		UpdateChest();
		Delay = 0.1f;
		if (StructureInfo.Inventory.Count <= 0 && StructureInfo.Exhausted)
		{
			Vector3 vector = base.transform.position + Vector3.up * 2f;
			BiomeConstants.Instance.EmitSmokeExplosionVFX(vector);
			if (Structure.Type == global::StructureBrain.TYPES.BLOOD_STONE || Structure.Type == global::StructureBrain.TYPES.BLOODSTONE_MINE_2)
			{
				AudioManager.Instance.PlayOneShot("event:/material/stone_break", vector);
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/material/wood_break", vector);
			}
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
			StructureBrain.Remove();
		}
	}

	private void GiveItem(InventoryItem.ITEM_TYPE type)
	{
		Inventory.AddItem((int)type, 1);
	}
}
