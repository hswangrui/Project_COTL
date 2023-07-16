using System;
using System.Collections.Generic;
using UnityEngine;

public class WildLife : UnitObject
{
	private const int V = 1;

	private float Delay;

	private Vector3 TargetPosition;

	private float LookAround;

	private float FacingAngle;

	public float FleeSpeed = 0.08f;

	public float DefaultSpeed = 0.05f;

	private float FleeTimer;

	private float Timer;

	private bool eaten;

	private bool CheckCollisions;

	public static List<WildLife> wildlife = new List<WildLife>();

	public GameObject VisionCone;

	private GameObject EscapeDoor;

	public override void OnEnable()
	{
		base.OnEnable();
		wildlife.Add(this);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		wildlife.Remove(this);
	}

	public override void Update()
	{
		base.Update();
		if (EscapeDoor != null && Vector2.Distance(base.transform.position, EscapeDoor.transform.position) <= 1f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			state.facingAngle = FacingAngle + 45f * Mathf.Cos(LookAround += 0.005f * GameManager.DeltaTime);
			speed += (0f - speed) / 7f;
			if ((Delay -= Time.deltaTime) < 0f)
			{
				Delay = UnityEngine.Random.Range(3, 5);
				if (!eaten && UnityEngine.Random.Range(0f, 1f) < 0.5f)
				{
					CheckCollisions = false;
					Timer = 0f;
					ChangeState(StateMachine.State.CustomAction0);
					eaten = true;
				}
				else
				{
					NewPosition();
					givePath(TargetPosition);
					eaten = false;
				}
			}
			LookOutForDanger();
			break;
		case StateMachine.State.RaiseAlarm:
			speed += (0f - speed) / 7f;
			if ((Timer += Time.deltaTime) > 0.5f)
			{
				givePath(TargetPosition);
				ChangeState(StateMachine.State.Fleeing);
			}
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 2f)
			{
				ChangeState(StateMachine.State.Idle);
			}
			if (Timer > 0.5f && !CheckCollisions)
			{
				EatGrass();
				CheckCollisions = true;
			}
			break;
		case StateMachine.State.Moving:
			FacingAngle = state.facingAngle;
			LookAround = 90f;
			LookOutForDanger();
			break;
		case StateMachine.State.Fleeing:
			if ((FleeTimer -= Time.deltaTime) < 0f)
			{
				FleeTimer = 1f;
				TargetPosition = EscapeDoor.transform.position;
				givePath(TargetPosition);
			}
			break;
		}
		vx = speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
		vy = speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
	}

	private void LookOutForDanger()
	{
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team == Health.Team.PlayerTeam && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < 5f && allUnit.team != 0 && !allUnit.untouchable)
			{
				float angle = Utils.GetAngle(base.transform.position, allUnit.gameObject.transform.position);
				if (angle < state.facingAngle + 90f && angle > state.facingAngle - 90f)
				{
					TargetEnemy = allUnit;
					Flee(true);
				}
			}
		}
	}

	private void ChangeState(StateMachine.State newState)
	{
		Timer = 0f;
		maxSpeed = DefaultSpeed;
		VisionCone.SetActive(false);
		switch (newState)
		{
		case StateMachine.State.Idle:
			VisionCone.SetActive(true);
			break;
		case StateMachine.State.RaiseAlarm:
			AlertOthers(base.gameObject);
			break;
		case StateMachine.State.Fleeing:
			maxSpeed = FleeSpeed;
			break;
		}
		state.CURRENT_STATE = newState;
	}

	public void Flee(bool FirstToSeeThreat)
	{
		NavigateRooms instance = NavigateRooms.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		EscapeDoor = null;
		List<GameObject> list = new List<GameObject>();
		float num = float.MaxValue;
		if (instance.North.activeSelf)
		{
			list.Add(instance.North);
		}
		if (instance.East.activeSelf)
		{
			list.Add(instance.East);
		}
		if (instance.South.activeSelf)
		{
			list.Add(instance.South);
		}
		if (instance.West.activeSelf)
		{
			list.Add(instance.West);
		}
		foreach (GameObject item in list)
		{
			float num2 = Vector3.Distance(base.transform.position, item.transform.position);
			if (num2 < num)
			{
				EscapeDoor = item;
				num = num2;
			}
		}
		TargetPosition = EscapeDoor.transform.position;
		if (FirstToSeeThreat)
		{
			ChangeState(StateMachine.State.RaiseAlarm);
			return;
		}
		givePath(TargetPosition);
		ChangeState(StateMachine.State.Fleeing);
	}

	private void AlertOthers(GameObject Warner)
	{
		foreach (WildLife item in wildlife)
		{
			if (item.state.CURRENT_STATE != StateMachine.State.Fleeing && item != Warner && Vector3.Distance(item.transform.position, Warner.transform.position) < 5f)
			{
				Flee(false);
			}
		}
	}

	private void EatGrass()
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position + new Vector3(0.5f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), 0f), 0.5f);
		foreach (Collider2D collider2D in array)
		{
			if (collider2D.gameObject.GetComponent<Grass>() != null)
			{
				UnityEngine.Object.Destroy(collider2D.gameObject);
			}
		}
	}

	private void NewPosition()
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle * 3f;
		TargetPosition = base.transform.position + new Vector3(vector.x, vector.y, 0f);
		TargetPosition = (Vector3)AstarPath.active.GetNearest(TargetPosition).node.position;
	}
}
