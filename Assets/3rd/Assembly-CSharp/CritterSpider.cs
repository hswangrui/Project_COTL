using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class CritterSpider : UnitObject
{
	public static List<CritterSpider> Spiders = new List<CritterSpider>();

	private float Timer;

	private float TargetAngle;

	public float WalkSpeed = 0.02f;

	public float RunSpeed = 0.07f;

	private float IgnorePlayer;

	private Animator animator;

	public SimpleInventory Inventory;

	public SkeletonAnimation Spine;

	private PickUp TargetPickUp;

	private bool Stealing;

	public GameObject TargetHost { get; set; }

	public override void OnEnable()
	{
		base.OnEnable();
		Spiders.Add(this);
		Start();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Spiders.Remove(this);
	}

	private void Start()
	{
		Timer = UnityEngine.Random.Range(1, 5);
		animator = GetComponentInChildren<Animator>();
		Inventory = GetComponentInChildren<SimpleInventory>();
	}

	public override void Update()
	{
		if (!PlayerRelic.TimeFrozen)
		{
			base.Update();
			if ((bool)TargetHost)
			{
				FollowerHost();
			}
			else if (!Stealing)
			{
				WonderFreely();
			}
			else
			{
				StealPickUp();
			}
		}
	}

	private void StealPickUp()
	{
		if (TargetPickUp == null)
		{
			Stealing = false;
			return;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			UsePathing = true;
			animator.speed = 1f;
			state.CURRENT_STATE = StateMachine.State.Moving;
			pathToFollow = new List<Vector3>();
			pathToFollow.Add(TargetPickUp.transform.position);
			currentWaypoint = 0;
			EndOfPath = (System.Action)Delegate.Combine(EndOfPath, new System.Action(ArriveAtPickUp));
			if (Spine != null)
			{
				Spine.AnimationState.SetAnimation(0, "run", true);
			}
			animator.SetTrigger("WALK");
			break;
		case StateMachine.State.Moving:
			animator.speed = 1f;
			maxSpeed = RunSpeed;
			if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				state.CURRENT_STATE = StateMachine.State.Moving;
				pathToFollow = new List<Vector3>();
				pathToFollow.Add(TargetPickUp.transform.position);
				currentWaypoint = 0;
			}
			break;
		case StateMachine.State.Fleeing:
			animator.speed = 1.7f;
			maxSpeed = RunSpeed;
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			break;
		}
	}

	private void ArriveAtPickUp()
	{
		if (TargetPickUp != null && !TargetPickUp.Activated)
		{
			Inventory.GiveItem(TargetPickUp.type);
			UnityEngine.Object.Destroy(TargetPickUp.gameObject);
			state.CURRENT_STATE = StateMachine.State.Fleeing;
			TargetAngle = UnityEngine.Random.Range(0, 360);
			UsePathing = false;
			if (Spine != null)
			{
				Spine.AnimationState.SetAnimation(0, "run", true);
			}
			animator.SetTrigger("WALK");
		}
		else
		{
			if (Spine != null)
			{
				Spine.AnimationState.SetAnimation(0, "animation", true);
			}
			animator.SetTrigger("IDLE");
			state.CURRENT_STATE = StateMachine.State.Idle;
			Stealing = false;
		}
		EndOfPath = (System.Action)Delegate.Remove(EndOfPath, new System.Action(ArriveAtPickUp));
	}

	private void WonderFreely()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			UsePathing = false;
			animator.speed = 1f;
			if ((Timer -= Time.deltaTime) < 0f)
			{
				if (Inventory.GetItemType() == InventoryItem.ITEM_TYPE.NONE)
				{
					foreach (PickUp pickUp in PickUp.PickUps)
					{
						if (pickUp.CanBeStolenByCritter)
						{
							TargetPickUp = pickUp;
							Stealing = true;
							TargetPickUp.TargetedByCritter();
							return;
						}
					}
				}
				animator.SetTrigger("WALK");
				if (Spine != null)
				{
					Spine.AnimationState.SetAnimation(0, "run", true);
				}
				Timer = UnityEngine.Random.Range(1, 5);
				TargetAngle = UnityEngine.Random.Range(0, 360);
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Moving:
			animator.speed = 1f;
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			if ((Timer -= Time.deltaTime) < 0f)
			{
				if (Spine != null)
				{
					Spine.AnimationState.SetAnimation(0, "animation", true);
				}
				animator.SetTrigger("IDLE");
				Timer = UnityEngine.Random.Range(1, 5);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Fleeing:
			animator.speed = 1.5f;
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			if (TargetEnemy == null || Vector3.Distance(base.transform.position, TargetEnemy.transform.position) > 5f)
			{
				if (Spine != null)
				{
					Spine.AnimationState.SetAnimation(0, "animation", true);
				}
				animator.SetTrigger("IDLE");
				maxSpeed = WalkSpeed;
				Timer = UnityEngine.Random.Range(1, 5);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else if (Vector3.Distance(base.transform.position, TargetEnemy.transform.position) <= 3f && (IgnorePlayer -= Time.deltaTime) < 0f)
			{
				TargetAngle = Utils.GetAngle(TargetEnemy.transform.position, base.transform.position);
			}
			break;
		}
	}

	private void FollowerHost()
	{
		if (Vector3.Distance(base.transform.position, TargetHost.transform.position) > 5f)
		{
			base.transform.position = TargetHost.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			UsePathing = true;
			animator.speed = 1f;
			state.CURRENT_STATE = StateMachine.State.Moving;
			pathToFollow = new List<Vector3>();
			pathToFollow.Add(TargetHost.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f);
			currentWaypoint = 0;
			EndOfPath = (System.Action)Delegate.Combine(EndOfPath, new System.Action(ArriveAtPickUp));
			if (Spine != null)
			{
				Spine.AnimationState.SetAnimation(0, "run", true);
			}
			animator.SetTrigger("WALK");
			break;
		case StateMachine.State.Moving:
			animator.speed = 1f;
			maxSpeed = RunSpeed;
			if ((Timer += Time.deltaTime) > 2.5f)
			{
				Timer = 0f;
				state.CURRENT_STATE = StateMachine.State.Moving;
				pathToFollow = new List<Vector3>();
				pathToFollow.Add(TargetHost.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f);
				currentWaypoint = 0;
			}
			break;
		case StateMachine.State.Fleeing:
			animator.speed = 1.7f;
			maxSpeed = RunSpeed;
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			break;
		}
	}

	private void LookForDanger()
	{
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != Health.Team.PlayerTeam || !(Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < 2.5f) || allUnit.team == Health.Team.Neutral || allUnit.untouchable)
			{
				continue;
			}
			TargetEnemy = allUnit;
			TargetAngle = Utils.GetAngle(allUnit.transform.position, base.transform.position);
			maxSpeed = RunSpeed;
			if (state.CURRENT_STATE == StateMachine.State.Idle)
			{
				if (Spine != null)
				{
					Spine.AnimationState.SetAnimation(0, "run", true);
				}
				animator.SetTrigger("WALK");
			}
			state.CURRENT_STATE = StateMachine.State.Fleeing;
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		TargetAngle = state.facingAngle + 90f;
		IgnorePlayer = 2f;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		Inventory.DropItem();
		EndOfPath = (System.Action)Delegate.Remove(EndOfPath, new System.Action(ArriveAtPickUp));
		base.gameObject.Recycle();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		EndOfPath = (System.Action)Delegate.Remove(EndOfPath, new System.Action(ArriveAtPickUp));
	}
}
