using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_Bonfire : Interaction
{
	private SimpleInventory PlayerInventory;

	public List<InventoryItem.ITEM_TYPE> AllowedItemTypesToCook = new List<InventoryItem.ITEM_TYPE>();

	public GameObject FoodDispenser;

	public GameObject FoodDispenserPosition;

	public Structure structure;

	public int MaxWood = 10;

	public GameObject firePitOn;

	public GameObject firePitOff;

	public float FireUsageTime;

	private float _FireUsageTime;

	public int amountOfWood;

	private string sAddFuel;

	private float Delay;

	private GameObject Player;

	private bool Activating;

	private float DistanceToTriggerDeposits = 5f;

	private List<InventoryItem> ToDeposit = new List<InventoryItem>();

	public override void OnEnableInteraction()
	{
		ContinuouslyHold = true;
		base.OnEnableInteraction();
		updateFire();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
	}

	public override void GetLabel()
	{
		if (Inventory.GetItemQuantity(1) > 0 && structure.Inventory.Count < MaxWood)
		{
			base.Label = sAddFuel;
		}
		else
		{
			base.Label = "";
		}
	}

	private void Start()
	{
		_FireUsageTime = FireUsageTime;
		UpdateLocalisation();
	}

	private void updateFire()
	{
		if (structure.Inventory.Count > 0)
		{
			firePitOn.SetActive(true);
		}
		else
		{
			firePitOn.SetActive(false);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sAddFuel = ScriptLocalization.Interactions.AddFuel;
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
		amountOfWood = structure.Inventory.Count;
		if (!((Player = GameObject.FindWithTag("Player")) == null))
		{
			updateFire();
			if (Activating && (structure.Inventory.Count + ToDeposit.Count >= MaxWood || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > DistanceToTriggerDeposits))
			{
				Activating = false;
			}
			if ((Delay -= Time.deltaTime) < 0f && Activating)
			{
				Delay = 0.2f;
			}
			if (structure.Inventory.Count > 0 && (_FireUsageTime -= Time.deltaTime) < 0f)
			{
				consumeWood();
				updateFire();
				_FireUsageTime = FireUsageTime;
			}
		}
	}

	private void DepositItem()
	{
		structure.DepositInventory(ToDeposit[0]);
		ToDeposit.RemoveAt(0);
	}

	public void consumeWood()
	{
		structure.Inventory.RemoveAt(0);
	}
}
