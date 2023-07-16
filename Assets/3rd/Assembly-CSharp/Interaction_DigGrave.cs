using System.Collections;
using UnityEngine;

public class Interaction_DigGrave : Interaction
{
	public GameObject GraveMound;

	public GameObject GraveHole;

	public GameObject ZombiePrefab;

	private bool Activated;

	private string sString;

	private void Start()
	{
		UpdateLocalisation();
		HoldToInteract = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sString);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Activated = true;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
		PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3((base.transform.position.x < state.transform.position.x) ? 0.4f : (-0.4f), 0.2f), base.gameObject, false, false, delegate
		{
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
			PlayerFarming.Instance.TimedAction(3f, delegate
			{
				StartCoroutine(InteractRoutine());
			}, "actions/dig");
		});
	}

	private IEnumerator InteractRoutine()
	{
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		switch (Random.Range(0, 6))
		{
		case 0:
		case 1:
			switch (Random.Range(0, 7))
			{
			case 0:
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 1, GraveHole.transform.position, 0f);
				break;
			case 1:
			case 2:
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.HALF_BLUE_HEART, 1, GraveHole.transform.position, 0f);
				break;
			case 3:
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.RED_HEART, 1, GraveHole.transform.position, 0f);
				break;
			case 4:
			case 5:
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.HALF_HEART, 1, GraveHole.transform.position, 0f);
				break;
			}
			break;
		case 2:
		case 3:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, Random.Range(3, 6), GraveHole.transform.position);
			break;
		case 4:
		case 5:
		case 6:
		{
			FormationFighter component = Object.Instantiate(ZombiePrefab, base.transform.position, Quaternion.identity, base.transform.parent).GetComponent<FormationFighter>();
			component.GraveSpawn();
			component.CombatTask.CannotLoseTarget = true;
			break;
		}
		}
		GraveMound.SetActive(false);
		GraveHole.SetActive(true);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
	}
}
