using Spine;
using Spine.Unity;
using UnityEngine;

public class CritterSquirrel : UnitObject
{
	private const int AmountForSkin = 5;

	public float DangerDistance = 3.5f;

	private float Timer;

	private float TargetAngle;

	public float WalkSpeed = 0.02f;

	public float RunSpeed = 0.07f;

	private float IgnorePlayer;

	public bool FleeNearEnemies = true;

	public bool EatGrass;

	public bool WonderAround = true;

	public bool FleeIntoGround = true;

	public SkeletonAnimation spine;

	private StateMachine.State _prevState;

	public float EscapeTimer;

	public float FleeTimer = 5f;

	private void Start()
	{
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Timer = 0f;
		if (spine != null && spine.AnimationState != null)
		{
			spine.AnimationState.Event += HandleEvent;
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (spine != null && spine.AnimationState != null)
		{
			spine.AnimationState.Event -= HandleEvent;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (spine != null && spine.AnimationState != null)
		{
			spine.AnimationState.Event -= HandleEvent;
		}
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "dig")
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_disappear_underground", base.gameObject);
		}
		else if (e.Data.Name == "step")
		{
			AudioManager.Instance.PlayOneShot("event:/material/footstep_woodland", base.gameObject);
		}
	}

	public override void Update()
	{
		if (!PlayerRelic.TimeFrozen)
		{
			base.Update();
			WonderFreely();
		}
	}

	private void WonderFreely()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (_prevState != state.CURRENT_STATE)
			{
				spine.AnimationState.SetAnimation(0, "animation", true);
				_prevState = state.CURRENT_STATE;
			}
			UsePathing = false;
			if (WonderAround)
			{
				if ((Timer -= Time.deltaTime) < 0f)
				{
					Timer = Random.Range(1, 5);
					TargetAngle = Random.Range(0, 360);
					state.CURRENT_STATE = StateMachine.State.Moving;
				}
				else
				{
					LookForDanger();
				}
			}
			break;
		case StateMachine.State.CustomAction0:
			if (_prevState != state.CURRENT_STATE)
			{
				AudioManager.Instance.PlayOneShot("event:/squirrel/squirell_tssk", base.gameObject);
				spine.AnimationState.SetAnimation(0, "eat", true);
				_prevState = state.CURRENT_STATE;
			}
			if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = Random.Range(1, 3);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Moving:
			if (_prevState != state.CURRENT_STATE)
			{
				if (spine != null)
				{
					spine.AnimationState.SetAnimation(0, "walk", true);
				}
				_prevState = state.CURRENT_STATE;
			}
			state.SmoothFacingAngle(TargetAngle, 10f);
			if ((Timer -= Time.deltaTime) < 0f)
			{
				if (EatGrass && Random.value < 0.8f)
				{
					Timer = Random.Range(2, 4);
					state.CURRENT_STATE = StateMachine.State.CustomAction0;
				}
				else
				{
					Timer = Random.Range(1, 5);
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
			}
			else
			{
				LookForDanger();
			}
			break;
		case StateMachine.State.Fleeing:
			if (_prevState != state.CURRENT_STATE)
			{
				AudioManager.Instance.PlayOneShot("event:/squirrel/squirrel_vocal", base.gameObject);
				spine.AnimationState.SetAnimation(0, "run", true);
				_prevState = state.CURRENT_STATE;
			}
			IgnorePlayer -= Time.deltaTime;
			state.SmoothFacingAngle(TargetAngle, 12f);
			if ((FleeTimer -= Time.deltaTime) < 0f)
			{
				if (FleeIntoGround)
				{
					state.CURRENT_STATE = StateMachine.State.CustomAnimation;
				}
			}
			else if (TargetEnemy == null)
			{
				maxSpeed = WalkSpeed;
				Timer = Random.Range(1, 5);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			else if (Vector3.Distance(base.transform.position, TargetEnemy.transform.position) <= 3f && IgnorePlayer < 0f)
			{
				state.facingAngle = (TargetAngle = Utils.GetAngle(TargetEnemy.transform.position, base.transform.position));
			}
			break;
		case StateMachine.State.CustomAnimation:
			if (_prevState != state.CURRENT_STATE)
			{
				AudioManager.Instance.PlayOneShot("event:/squirrel/squirrel_vocal", base.gameObject);
				spine.AnimationState.SetAnimation(0, "dig", false);
				_prevState = state.CURRENT_STATE;
				GetComponent<ShowHPBar>().DestroyHPBar();
			}
			if ((EscapeTimer += Time.deltaTime) > 1.1f)
			{
				base.gameObject.Recycle();
			}
			break;
		}
	}

	private void LookForDanger()
	{
		if (!FleeNearEnemies)
		{
			return;
		}
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team == Health.Team.PlayerTeam && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DangerDistance && allUnit.team != 0 && !allUnit.untouchable)
			{
				TargetEnemy = allUnit;
				TargetAngle = Utils.GetAngle(allUnit.transform.position, base.transform.position);
				maxSpeed = RunSpeed;
				state.CURRENT_STATE = StateMachine.State.Fleeing;
				FleeTimer = Random.Range(5, 10);
			}
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		CameraManager.instance.ShakeCameraForDuration(0.6f, 0.8f, 0.3f, false);
		AudioManager.Instance.PlayOneShot("event:/squirrel/squirrel_vocal", base.gameObject);
		spine.GetComponent<SimpleSpineFlash>().FlashFillRed();
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		AudioManager.Instance.PlayOneShot("event:/squirrel/squirrel_vocal", base.gameObject);
		base.gameObject.Recycle();
		DataManager.Instance.TotalSquirrelsCaught++;
		if (DataManager.Instance.TotalSquirrelsCaught < 5 || DataManager.Instance.FollowerSkinsUnlocked.Contains("Squirrel"))
		{
			return;
		}
		bool flag = false;
		foreach (FoundItemPickUp foundItemPickUp in FoundItemPickUp.FoundItemPickUps)
		{
			if (foundItemPickUp.SkinToForce == "Squirrel")
			{
				flag = true;
			}
		}
		if (!flag)
		{
			PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN, 1, base.transform.position);
			if (pickUp != null)
			{
				FoundItemPickUp component = pickUp.GetComponent<FoundItemPickUp>();
				component.FollowerSkinForceSelection = true;
				component.SkinToForce = "Squirrel";
			}
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		state.facingAngle = (TargetAngle = Utils.GetAngle(collision.contacts[0].point, base.transform.position));
		IgnorePlayer = 0.5f;
	}
}
