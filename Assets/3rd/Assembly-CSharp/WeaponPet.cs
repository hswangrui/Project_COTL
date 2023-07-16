using System;
using Spine.Unity;
using UnityEngine;

public class WeaponPet : BaseMonoBehaviour
{
	private GameObject Master;

	private StateMachine MasterState;

	private StateMachine state;

	private float TargetAngle;

	private Vector3 MoveVector;

	private float Speed;

	private float vx;

	private float vy;

	private float Bobbing;

	private float SpineVZ;

	private float SpineVY;

	public SpriteRenderer spriteRenderer;

	public SkeletonAnimation spine;

	public Transform ChainPoint;

	private float AttackProgress;

	private Vector3 AttackPosition;

	private float AttackAngle;

	private float AttackWait;

	private float Timer;

	private void Start()
	{
		state = GetComponent<StateMachine>();
	}

	private void Update()
	{
		if (Master == null)
		{
			Master = GameObject.FindGameObjectWithTag("Player");
			if (Master == null)
			{
				return;
			}
			base.transform.position = Master.transform.position;
			MasterState = Master.GetComponent<StateMachine>();
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			Speed += (0f - Speed) / (10f * GameManager.DeltaTime);
			if (Vector2.Distance(base.transform.position, Master.transform.position) > 1.5f)
			{
				TargetAngle = Utils.GetAngle(base.transform.position, Master.transform.position);
				state.facingAngle = TargetAngle;
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			vx = Speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			vy = Speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			base.transform.position = base.transform.position + new Vector3(vx, vy);
			break;
		case StateMachine.State.Moving:
			TargetAngle = Utils.GetAngle(base.transform.position, Master.transform.position);
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			Speed += (4.5f - Speed) / (15f * GameManager.DeltaTime);
			if (Vector2.Distance(base.transform.position, Master.transform.position) < 1.5f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			vx = Speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			vy = Speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			base.transform.position = base.transform.position + new Vector3(vx, vy);
			break;
		case StateMachine.State.SignPostAttack:
		{
			AttackProgress += 0.1f;
			AttackProgress = 0f;
			float num = 180f * AttackProgress + AttackAngle;
			base.transform.position = AttackPosition + new Vector3(1f * Mathf.Cos(num), 1f * Mathf.Sin(num));
			break;
		}
		case StateMachine.State.Attacking:
		{
			AttackProgress += 0.05f;
			float num = 360f * (AttackProgress / 1f) + AttackAngle;
			base.transform.position = AttackPosition + new Vector3(2f * Mathf.Cos(num * ((float)Math.PI / 180f)), 2f * Mathf.Sin(num * ((float)Math.PI / 180f)));
			if (AttackProgress >= 1f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
		case StateMachine.State.AimDodge:
			vx = 20f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			vy = 20f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			base.transform.position = base.transform.position + new Vector3(vx, vy);
			if ((Timer += Time.deltaTime) > 0.1f)
			{
				state.CURRENT_STATE = StateMachine.State.Dodging;
			}
			break;
		case StateMachine.State.Dodging:
			spine.transform.localPosition = Vector3.Lerp(spine.transform.localPosition, new Vector3(0f, 0f, 0f), 25f * Time.deltaTime);
			SpineVZ = spine.transform.localPosition.z;
			SpineVY = spine.transform.localPosition.y;
			break;
		}
		spine.skeleton.ScaleX = ((!(Master.transform.position.x > base.transform.position.x)) ? 1 : (-1));
		spine.transform.eulerAngles = new Vector3(-60f, 0f, vx * -100f);
		if (state.CURRENT_STATE != StateMachine.State.AimDodge && state.CURRENT_STATE != StateMachine.State.Dodging)
		{
			SpineVZ = Mathf.Lerp(SpineVZ, -1.5f, 5f * Time.deltaTime);
			SpineVY = Mathf.Lerp(SpineVY, 0.5f, 5f * Time.deltaTime);
			spine.transform.localPosition = new Vector3(0f, SpineVY, SpineVZ + 0.1f * Mathf.Cos(Bobbing += 5f * Time.deltaTime));
		}
	}

	public void DoAttack(Vector3 AttackPosition, float AttackAngle)
	{
		AttackProgress = 0f;
		this.AttackPosition = AttackPosition;
		this.AttackAngle = AttackAngle;
		AttackWait = 0f;
		state.CURRENT_STATE = StateMachine.State.Attacking;
	}

	public void PrepareAttack(Vector3 AttackPosition, float AttackAngle)
	{
		AttackProgress = 0f;
		this.AttackPosition = AttackPosition;
		this.AttackAngle = AttackAngle;
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
	}

	public void DoChainDodge(Vector3 StartPosition, float DodgeAngle)
	{
		Timer = 0f;
		state.CURRENT_STATE = StateMachine.State.AimDodge;
		base.transform.position = StartPosition;
		state.facingAngle = DodgeAngle;
	}

	public void EndDodge()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
