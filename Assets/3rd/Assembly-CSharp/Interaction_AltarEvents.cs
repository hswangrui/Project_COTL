using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_AltarEvents : Interaction
{
	public GameObject PlayerPosition;

	public int FollowersNeeded = 3;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.SacrificeFollower;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		StartCoroutine(WaitForPlayerPosition());
	}

	private IEnumerator WaitForPlayerPosition()
	{
		state.GetComponent<PlayerFarming>().GoToAndStop(PlayerPosition);
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(state.gameObject, 10f);
		while (Vector3.Distance(PlayerPosition.transform.position, state.transform.position) > 0.3f)
		{
			yield return null;
		}
		state.facingAngle = -90f;
		yield return new WaitForSeconds(0.2f);
		GameManager.GetInstance().OnConversationEnd();
		new List<StructuresData>();
	}
}
