using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class FishingHut : Interaction
{
	public static List<FishingHut> FishingHuts = new List<FishingHut>();

	public Structure Structure;

	private Structures_FishingHut _StructureInfo;

	public GameObject FollowerPosition;

	private string sString;

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

	public Structures_FishingHut StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_FishingHut;
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
		FishingHuts.Add(this);
	}

	public override void GetLabel()
	{
		int count = StructureInfo.Inventory.Count;
		Interactable = count > 0;
		base.Label = sString + " <sprite name=\"icon_Fish\"> x" + count;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		FishingHuts.Remove(this);
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
		GetLabel();
		if (Activating && (StructureInfo.Inventory.Count <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > DistanceToTriggerDeposits))
		{
			Activating = false;
		}
		if ((Delay -= Time.deltaTime) < 0f && Activating)
		{
			InventoryItem inventoryItem = StructureInfo.Inventory[0];
			InventoryItem.ITEM_TYPE itemType = (InventoryItem.ITEM_TYPE)inventoryItem.type;
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
			ResourceCustomTarget.Create(state.gameObject, base.transform.position, itemType, delegate
			{
				GiveItem(itemType);
			});
			StructureInfo.Inventory.RemoveAt(0);
			Delay = 0.2f;
		}
	}

	private void GiveItem(InventoryItem.ITEM_TYPE type)
	{
		Inventory.AddItem((int)type, 1);
	}
}
