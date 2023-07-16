using System;
using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_AwaitRecruit : Interaction
{
	private Worshipper w;

	public float SacrificeDeathShakeTime = 4.8f;

	public float SacrificeTotalTime = 8f;

	public float RecruitTotalTime = 6f;

	private string sPassJudgement;

	private string sSacrifice;

	private bool completed;

	private SimpleSpineAnimator playerAnimator;

	private int WaitingForPositions;

	private void Start()
	{
		w = GetComponent<Worshipper>();
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sSacrifice = ScriptLocalization.Interactions.Sacrifice;
	}

	public override void GetLabel()
	{
		base.Label = (completed ? "" : sPassJudgement);
	}

	public override void OnInteract(StateMachine state)
	{
	}

	private void Recruit()
	{
		w.interaction_AwaitRecruit.enabled = false;
		StartCoroutine(RecruitFollower());
	}

	private IEnumerator RecruitFollower()
	{
		GameObject gameObject = new GameObject();
		float angle = Utils.GetAngle(base.transform.position, Altar.Instance.CentrePoint.transform.position);
		gameObject.transform.position = base.transform.position + new Vector3(1.5f * Mathf.Cos(angle * ((float)Math.PI / 180f)), 1.5f * Mathf.Sin(angle * ((float)Math.PI / 180f)));
		PlayerFarming playerFarming = state.GetComponent<PlayerFarming>();
		playerFarming.GoToAndStop(gameObject, w.gameObject);
		while (playerFarming.GoToAndStopping)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(w.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		SimpleSpineAnimator componentInChildren = state.GetComponentInChildren<SimpleSpineAnimator>();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		componentInChildren.Animate("recruit", 0, true);
		componentInChildren.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(RecruitTotalTime - 3f);
		w.state.CURRENT_STATE = StateMachine.State.Recruited;
		yield return new WaitForSeconds(4f);
		w.state.CURRENT_STATE = StateMachine.State.Idle;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
	}

	private void Sacrifice()
	{
		w.state.CURRENT_STATE = StateMachine.State.SacrificeRecruit;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(w.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		StartCoroutine(FollowerToAltar());
	}

	private IEnumerator FollowerToAltar()
	{
		StartCoroutine(WaitForPlayerToAltar());
		StartCoroutine(WaitForFollowerToAltar());
		while (WaitingForPositions < 2)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		StartCoroutine(SacrificeFollower());
	}

	private IEnumerator WaitForPlayerToAltar()
	{
		PlayerFarming playerFarming = state.GetComponent<PlayerFarming>();
		playerFarming.GoToAndStop(Altar.Instance.SacrificePositions[1].gameObject);
		while (playerFarming.GoToAndStopping)
		{
			yield return null;
		}
		state.facingAngle = Utils.GetAngle(Altar.Instance.SacrificePositions[1].transform.position, Altar.Instance.SacrificePositions[0].transform.position);
		WaitingForPositions++;
	}

	private IEnumerator WaitForFollowerToAltar()
	{
		w.GoToAndStop(Altar.Instance.SacrificePositions[0].gameObject, WaitState, Altar.Instance.SacrificePositions[1].gameObject, true);
		while (w.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		WaitingForPositions++;
	}

	private void WaitState()
	{
		w.state.CURRENT_STATE = StateMachine.State.AwaitRecruit;
	}

	private IEnumerator SacrificeFollower()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		w.state.CURRENT_STATE = StateMachine.State.SacrificeRecruit;
		playerAnimator = state.GetComponentInChildren<SimpleSpineAnimator>();
		playerAnimator.Animate("sacrifice", 0, false);
		playerAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(SacrificeDeathShakeTime);
		CameraManager.shakeCamera(0.3f, UnityEngine.Random.Range(0, 360));
		yield return new WaitForSeconds(SacrificeTotalTime - SacrificeDeathShakeTime);
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
