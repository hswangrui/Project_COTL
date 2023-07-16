using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class CritterJellyfish : UnitObject
{
	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string idleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string swimAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string scareAnimation;

	private float DangerDistance = 2f;

	public CircleCollider2D CircleCollider;

	private Animator animator;

	private float Timer;

	private float TargetAngle;

	private float vz;

	public GameObject Shadow;

	private void Start()
	{
		state = GetComponent<StateMachine>();
		if (CircleCollider == null)
		{
			CircleCollider = GetComponent<CircleCollider2D>();
		}
		animator = GetComponentInChildren<Animator>();
		Timer = UnityEngine.Random.Range(2, 4);
		if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x * -1f, 1f, 1f);
		}
		DangerDistance = 2.5f;
	}

	public void SetFleeing()
	{
		if (state == null)
		{
			state = GetComponent<StateMachine>();
		}
		state.CURRENT_STATE = StateMachine.State.Fleeing;
	}

	public override void Update()
	{
		health.HandleFrozenTime();
		if (PlayerRelic.TimeFrozen)
		{
			return;
		}
		base.Update();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			LookForDanger();
			break;
		case StateMachine.State.CustomAction0:
			LookForDanger();
			if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = UnityEngine.Random.Range(2, 4);
				state.CURRENT_STATE = StateMachine.State.Idle;
				if (animator != null)
				{
					animator.SetTrigger("IDLE");
				}
				if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
				{
					base.transform.localScale = new Vector3(base.transform.localScale.x * -1f, 1f, 1f);
				}
			}
			break;
		case StateMachine.State.Fleeing:
			vx = 0.3f * Mathf.Cos(TargetAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			vy = 0.3f * Mathf.Sin(TargetAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			vz -= 0.015f * Mathf.Sin(Time.deltaTime);
			base.transform.position = base.transform.position + new Vector3(vx, vy, (Time.deltaTime == 0f) ? 0f : vz);
			break;
		}
	}

	private void LookForDanger()
	{
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team == Health.Team.PlayerTeam && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DangerDistance && allUnit.team != 0 && !allUnit.untouchable)
			{
				TargetAngle = Utils.GetAngle(allUnit.transform.position, base.transform.position);
				Spine.AnimationState.SetAnimation(0, scareAnimation, false);
				Spine.AnimationState.AddAnimation(0, swimAnimation, true, 0f);
				if (animator != null)
				{
					animator.SetTrigger("FLY");
				}
				AudioManager.Instance.PlayOneShot("event:/enemy/fly_spawn", base.gameObject);
				state.CURRENT_STATE = StateMachine.State.Fleeing;
				base.transform.localScale = new Vector3((!(TargetAngle < 90f) || !(TargetAngle > -90f)) ? 1 : (-1), 1f, 1f);
				StartCoroutine(DisableCollider());
			}
		}
	}

	private IEnumerator DisableCollider()
	{
		yield return new WaitForSeconds(0.2f);
		CircleCollider.enabled = false;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, DangerDistance, Color.white);
	}
}
