using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class Cower : BaseMonoBehaviour
{
	public bool CanCower = true;

	private StateMachine state;

	private Health health;

	public UnitObject AIScriptToDisable;

	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string KnockBackStart;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string KnockBackLoop;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string KnockBackEnd;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string StaggerStart;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string StaggerLoop;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string StaggerEnd;

	private UnitObject unitObject;

	private bool ShieldDontCower;

	private bool CoweringActivated;

	private GameObject Player;

	private ShowHPBar ShowHPBar;

	private Rigidbody2D rb2d;

	private SpawnDeadBodyOnDeath SpawnDeadBodyOnDeath;

	public int HitsNeededToEndStagger = 1;

	public bool HeavyAttackEndsStagger = true;

	private int CurrentStaggerHits;

	private int StartingEnemies;

	public bool KnockBackOnMelee;

	public bool preventStandardStagger;

	private Coroutine cStaggeredRoutine;

	private bool Staggered;

	public UnityEvent StaggerBegun;

	public UnityEvent StaggerEnded;

	private float Speed = 1500f;

	private void OnEnable()
	{
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
		health.OnHit += Health_OnHit;
		health.OnHitEarly += Health_OnHitEarly;
		health.OnDie += Health_OnDie;
		ShowHPBar = GetComponent<ShowHPBar>();
		unitObject = GetComponent<UnitObject>();
		rb2d = GetComponent<Rigidbody2D>();
		SpawnDeadBodyOnDeath = GetComponent<SpawnDeadBodyOnDeath>();
		if (SpawnDeadBodyOnDeath != null)
		{
			SpawnDeadBodyOnDeath.enabled = false;
		}
		if (CoweringActivated)
		{
			StartCoroutine(CowerRoutine());
		}
	}

	private void Health_OnHitEarly(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		if (Staggered)
		{
			CurrentStaggerHits++;
			if (CurrentStaggerHits >= HitsNeededToEndStagger || (HeavyAttackEndsStagger && AttackType == Health.AttackTypes.Heavy) || health.HP <= 0f)
			{
				EndStaggered();
			}
			else if (CurrentStaggerHits < HitsNeededToEndStagger)
			{
				CameraManager.shakeCamera(2f);
			}
		}
	}

	private void Health_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (AttackType != Health.AttackTypes.Projectile && (AttackType == Health.AttackTypes.Heavy || (KnockBackOnMelee && AttackType == Health.AttackTypes.Melee)))
		{
			health.DestroyOnDeath = false;
			StartCoroutine(KnockbackRoutine(Attacker, AttackLocation));
			return;
		}
		SpawnDeadBodyOnDeath spawnDeadBodyOnDeath = SpawnDeadBodyOnDeath;
		if ((object)spawnDeadBodyOnDeath != null)
		{
			spawnDeadBodyOnDeath.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		}
	}

	private void Start()
	{
		ShieldDontCower = health.HasShield;
		StartingEnemies = Health.team2.Count;
	}

	private void OnDisable()
	{
		health.OnHitEarly -= Health_OnHitEarly;
		health.OnHit -= Health_OnHit;
		health.OnDie -= Health_OnDie;
	}

	public void Health_OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (preventStandardStagger)
		{
			return;
		}
		Player = Attacker;
		if (health.WasJustParried || AttackType == Health.AttackTypes.Projectile)
		{
			if (cStaggeredRoutine != null)
			{
				StopCoroutine(cStaggeredRoutine);
			}
			cStaggeredRoutine = StartCoroutine(StaggerRoutine(Attacker));
		}
		else if (AttackType == Health.AttackTypes.Heavy || (KnockBackOnMelee && AttackType == Health.AttackTypes.Melee))
		{
			StartCoroutine(KnockbackRoutine(Attacker, AttackLocation));
		}
	}

	private IEnumerator StaggerRoutine(GameObject Attacker)
	{
		Staggered = true;
		Speed = 1000f;
		rb2d.angularDrag = 10f;
		rb2d.drag = 10f;
		float f = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Vector2 force = new Vector2(Speed * Mathf.Cos(f), Speed * Mathf.Sin(f));
		rb2d.AddForce(force);
		Spine.AnimationState.SetAnimation(0, StaggerStart, false);
		Spine.AnimationState.AddAnimation(0, StaggerLoop, true, 0f);
		AIScriptToDisable.enabled = false;
		StaggerBegun.Invoke();
		float Progress = 0f;
		float Duration = 3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, StaggerEnd, false);
		yield return new WaitForSeconds(19f / 30f);
		EndStaggered();
	}

	private void EndStaggered()
	{
		CurrentStaggerHits = 0;
		if (cStaggeredRoutine != null)
		{
			StopCoroutine(cStaggeredRoutine);
		}
		StaggerEnded.Invoke();
		state.CURRENT_STATE = StateMachine.State.Idle;
		Staggered = false;
		AIScriptToDisable.enabled = true;
	}

	private IEnumerator KnockbackRoutine(GameObject Attacker, Vector3 AttackLocation)
	{
		AIScriptToDisable.enabled = false;
		Spine.AnimationState.SetAnimation(0, KnockBackStart, false);
		Spine.AnimationState.AddAnimation(0, KnockBackLoop, true, 0f);
		Speed = 1500f;
		rb2d.angularDrag = 10f;
		rb2d.drag = 10f;
		float f = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Vector2 force = new Vector2(Speed * Mathf.Cos(f), Speed * Mathf.Sin(f));
		rb2d.AddForce(force);
		while (rb2d.velocity.magnitude > 0.1f)
		{
			yield return null;
		}
		if (health.HP <= 0f)
		{
			yield return new WaitForSeconds(0.5f);
			if (SpawnDeadBodyOnDeath != null)
			{
				SpawnDeadBodyOnDeath.ReEnable(800f);
			}
			health.enabled = true;
			health.OnHit -= Health_OnHit;
			health.OnDie -= Health_OnDie;
			health.DestroyOnDeath = true;
			health.invincible = false;
			health.DealDamage(1f, Attacker, AttackLocation);
		}
		else
		{
			yield return new WaitForSeconds(1f);
			Spine.AnimationState.SetAnimation(0, KnockBackEnd, false);
			yield return new WaitForSeconds(14f / 15f);
			if (health.HP > 0f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				AIScriptToDisable.enabled = true;
				health.enabled = true;
			}
		}
	}

	private IEnumerator CowerRoutine()
	{
		if (Health.team2.Count <= 1 && StartingEnemies > 1)
		{
			health.SlowMoOnkill = true;
		}
		health.HP = 1f;
		AIScriptToDisable.enabled = false;
		Spine.AnimationState.SetAnimation(0, "scared", false);
		Spine.AnimationState.AddAnimation(0, "scared-loop", true, 0f);
		health.invincible = false;
		while (true)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, Player.transform.position);
			Spine.skeleton.ScaleX = ((!(state.facingAngle < 90f) || !(state.facingAngle > -90f)) ? 1 : (-1));
			yield return null;
		}
	}
}
