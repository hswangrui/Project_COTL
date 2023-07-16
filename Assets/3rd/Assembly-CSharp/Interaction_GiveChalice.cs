using System;
using System.Collections;
using UnityEngine;

public class Interaction_GiveChalice : Interaction
{
	private Worshipper w;

	private PlayerFarming playerFarming;

	private string sGiveChalice;

	public override void GetLabel()
	{
		base.Label = sGiveChalice;
	}

	private void Start()
	{
		w = GetComponent<Worshipper>();
		ActivateDistance = 2f;
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		playerFarming = state.GetComponent<PlayerFarming>();
		StartCoroutine(GoToAndGiveChalice());
	}

	private IEnumerator GoToAndGiveChalice()
	{
		GameObject gameObject = new GameObject();
		float angle = Utils.GetAngle(base.transform.position, Altar.Instance.CentrePoint.transform.position);
		gameObject.transform.position = base.transform.position + new Vector3(1.2f * Mathf.Cos(angle * ((float)Math.PI / 180f)), 1.2f * Mathf.Sin(angle * ((float)Math.PI / 180f)));
		playerFarming.GoToAndStop(gameObject, w.gameObject);
		while (playerFarming.GoToAndStopping)
		{
			yield return null;
		}
		playerFarming.TimedAction(1.8f, PlayerInactive, "present-chalice");
		w.TimedAnimation("accept-chalice", 4.5f, ChaliceComplete);
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(w.gameObject, 6f);
	}

	private void PlayerInactive()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
	}

	private void ChaliceComplete()
	{
		StartCoroutine(ChaliceCompleteWait());
	}

	private IEnumerator ChaliceCompleteWait()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(0.5f);
		w.Pray();
		UnityEngine.Object.Destroy(this);
	}
}
