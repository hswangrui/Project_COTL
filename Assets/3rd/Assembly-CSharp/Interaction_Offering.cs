using System;
using System.Collections;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

public class Interaction_Offering : Interaction
{
	public GameObject PlayerGoTo;

	public DataManager.Variables VariableOnComplete;

	private string sLabel;

	private bool Activated;

	public EnemyRounds enemyRounds;

	private InventoryItem.ITEM_TYPE Resource;

	public List<InventoryItem.ITEM_TYPE> Resources = new List<InventoryItem.ITEM_TYPE>();

	private InventoryItemDisplay[] inventoryItems;

	private void Start()
	{
		HoldToInteract = true;
		if (BiomeGenerator.Instance != null)
		{
			Resource = Resources[BiomeGenerator.Instance.CurrentRoom.RandomSeed.Next(0, Resources.Count)];
		}
		else
		{
			Resource = Resources[UnityEngine.Random.Range(0, Resources.Count)];
		}
		UpdateLocalisation();
		inventoryItems = GetComponentsInChildren<InventoryItemDisplay>();
		InventoryItemDisplay[] array = inventoryItems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetImage(Resource);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sLabel);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Activated = true;
		StartCombat();
	}

	private IEnumerator InteractRoutine()
	{
		int ResourcesToGive = 10;
		int NumResouces = ResourcesToGive;
		InventoryItemDisplay[] array = inventoryItems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
		while (true)
		{
			int i = NumResouces - 1;
			NumResouces = i;
			if (i >= 0)
			{
				AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.transform.position);
				ResourceCustomTarget.Create(state.gameObject, base.transform.position, Resource, (NumResouces == 0) ? new Action(CompleteCollection) : new Action(GiveResource));
				yield return new WaitForSeconds(0.1f + 0.2f * (float)(NumResouces / ResourcesToGive));
				continue;
			}
			break;
		}
	}

	private void GiveResource()
	{
		Inventory.AddItem((int)Resource, 1);
	}

	private void StartCombat()
	{
		StartCoroutine(StartCombatRoutine());
	}

	private IEnumerator StartCombatRoutine()
	{
		BlockingDoor.CloseAll();
		state.CURRENT_STATE = StateMachine.State.Idle;
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.OfferingCombat);
		yield return new WaitForSeconds(2f);
		enemyRounds.BeginCombat(false, CombatComplete);
	}

	public void CombatComplete()
	{
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.StandardAmbience);
		StartCoroutine(DelayGoTo());
	}

	private IEnumerator DelayGoTo()
	{
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 8f);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.GoToAndStop(PlayerGoTo, base.gameObject, false, false, CollectResources);
	}

	private void CollectResources()
	{
		StartCoroutine(InteractRoutine());
	}

	private void CompleteCollection()
	{
		GameManager.GetInstance().OnConversationEnd();
		state.CURRENT_STATE = StateMachine.State.Idle;
		BlockingDoor.OpenAll();
		DataManager.Instance.SetVariable(VariableOnComplete, true);
	}
}
