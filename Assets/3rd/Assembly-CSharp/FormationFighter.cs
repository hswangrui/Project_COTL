using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationFighter : TaskDoer
{
	public float KnockBackAmount = 0.25f;

	public float PreAttackDuration = 1f;

	public float PostAttackDuration = 1f;

	public float DefendingDuration = 2f;

	[HideInInspector]
	public float Timer;

	private int AttackPosition;

	public float DetectEnemyRange = 5f;

	public float LoseEnemyRange = 8f;

	public float AttackRange = 0.5f;

	public float SeperationRadius = 1f;

	public List<Sprite> ChunksToSpawn;

	public int thisChecked;

	public bool BreakAttacksOnHit;

	private SimpleSpineAnimator _simpleSpineAnimator;

	private SimpleSpineFlash _simpleSpineFlash;

	public static List<FormationFighter> fighters = new List<FormationFighter>();

	public Task_Combat CombatTask;

	[HideInInspector]
	public float DefendTimer;

	[HideInInspector]
	public float HitTimer;

	private SimpleSpineAnimator simpleSpineAnimator
	{
		get
		{
			if (_simpleSpineAnimator == null)
			{
				_simpleSpineAnimator = GetComponentInChildren<SimpleSpineAnimator>();
			}
			return _simpleSpineAnimator;
		}
		set
		{
			_simpleSpineAnimator = value;
		}
	}

	private SimpleSpineFlash simpleSpineFlash
	{
		get
		{
			if (_simpleSpineFlash == null)
			{
				_simpleSpineFlash = GetComponentInChildren<SimpleSpineFlash>();
			}
			return _simpleSpineFlash;
		}
		set
		{
			_simpleSpineFlash = value;
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid/death", base.transform.position);
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		int num = -1;
		if (ChunksToSpawn.Count > 0)
		{
			while (++num < 10)
			{
				Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle(Attacker.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20), ChunksToSpawn);
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		SetTask();
		fighters.Add(this);
		if ((bool)health)
		{
			health.invincible = false;
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		fighters.Remove(this);
		if (CombatTask != null)
		{
			CombatTask.ClearTarget();
		}
		CombatTask = null;
	}

	public void GraveSpawn()
	{
		StopAllCoroutines();
		StartCoroutine(GraveSpawnRoutine());
	}

	private IEnumerator GraveSpawnRoutine()
	{
		CurrentTask.ClearTask();
		CurrentTask = null;
		health.invincible = true;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		simpleSpineAnimator.Animate("grave-spawn", 0, false, 0f);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(1.5f);
		health.invincible = false;
		SetTask();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid/gethit", base.transform.position);
		simpleSpineAnimator.FlashRedTint();
		if (AttackType != Health.AttackTypes.Heavy && !FromBehind)
		{
			knockBackVX = (0f - KnockBackAmount) * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			knockBackVY = (0f - KnockBackAmount) * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			if (AttackLocation.x > base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitRight)
			{
				state.CURRENT_STATE = StateMachine.State.HitRight;
			}
			if (AttackLocation.x < base.transform.position.x && state.CURRENT_STATE != StateMachine.State.HitLeft)
			{
				state.CURRENT_STATE = StateMachine.State.HitLeft;
			}
		}
		simpleSpineAnimator.Animate("hurt-eyes", 1, false);
		base.OnHit(Attacker, AttackLocation, AttackType);
	}

	public virtual void SetTask()
	{
		AddNewTask(Task_Type.COMBAT, false);
		CombatTask = CurrentTask as Task_Combat;
		CombatTask.Init(DetectEnemyRange, AttackRange, LoseEnemyRange, PreAttackDuration, PostAttackDuration, DefendingDuration, this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	public virtual void SpecialStates()
	{
		if (state.CURRENT_STATE == StateMachine.State.SignPostAttack && CurrentTask != null)
		{
			simpleSpineFlash.FlashWhite(CurrentTask.Timer / 0.5f);
		}
		else
		{
			simpleSpineFlash.FlashWhite(false);
		}
		if (state.CURRENT_STATE == StateMachine.State.HitLeft && (HitTimer += Time.deltaTime) >= 0.4f)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			HitTimer = 0f;
		}
		if (state.CURRENT_STATE == StateMachine.State.HitRight && (HitTimer += Time.deltaTime) >= 0.4f)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			HitTimer = 0f;
		}
	}

	public override void Update()
	{
		SpecialStates();
		base.Update();
		if (CurrentTask != null)
		{
			CurrentTask.TaskUpdate();
		}
		if (SeperateObject)
		{
			Seperate(SeperationRadius);
		}
	}
}
