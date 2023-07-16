using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemyStealth : BaseMonoBehaviour
{
	public enum Activity
	{
		Animation,
		Patrol,
		Sleep
	}

	public Activity StartingActivity;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string StartingAnimation = "idle";

	public bool LoopAnimation = true;

	public List<Vector3> PatrolRoute = new List<Vector3>();

	public float PatrolSpeed = 0.02f;

	public float WaitBetweenPatrolPoints = 0.5f;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string PatrolWalk = "run";

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string PatrolIdle = "idle";

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string SleepingAnimation = "sleeping";

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string WakeUpAnimation = "wake-up";

	public LayerMask DetectionLayer;

	private Vector3 StartPosition;

	private int Patrol;

	private float RepathTimer;

	private UnitObject unitObject;

	public UnitObject AIScriptToDisable;

	private StateMachine state;

	[HideInInspector]
	public Health health;

	[SerializeField]
	private float alarmTime = 1.8f;

	[SerializeField]
	private float warnTime = 1f;

	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string RaiseAlarm;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string DetectPlayer;

	public SkeletonAnimation warningIcon;

	public GameObject EnemyStealthUIPrefab;

	private EnemyStealthUI EnemyStealthUI;

	public static List<EnemyStealth> EnemyStealths = new List<EnemyStealth>();

	public float EnterRadius = 5f;

	public float ExitRadius = 6f;

	private float _AlertLevel;

	public float AlertLimit = 3f;

	private Coroutine cPatrolRoutine;

	private float Distance;

	public float VisionRage = 8f;

	private float VisionConeAngle = 40f;

	private float CloseConeAngle = 120f;

	private float DetectEvenIfStealth = 3f;

	private int VisibleEnemies;

	private Health EnemyHealth;

	private float AlertLevel
	{
		get
		{
			return _AlertLevel;
		}
		set
		{
			_AlertLevel = Mathf.Clamp(value, 0f, AlertLimit);
		}
	}

	private void Start()
	{
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
		health.OnHit += Health_OnHit;
		AIScriptToDisable.enabled = false;
		EnemyStealthUI = UnityEngine.Object.Instantiate(EnemyStealthUIPrefab, base.transform).GetComponent<EnemyStealthUI>();
		EnemyStealthUI.UpdateProgress(0f);
		health.Unaware = true;
		StartPosition = base.transform.position;
		PatrolRoute.Insert(0, Vector3.zero);
		Spine.Initialize(false);
		switch (StartingActivity)
		{
		case Activity.Animation:
			Spine.AnimationState.SetAnimation(0, StartingAnimation, LoopAnimation);
			Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
			break;
		case Activity.Patrol:
			if (cPatrolRoutine != null)
			{
				StopCoroutine(cPatrolRoutine);
			}
			cPatrolRoutine = StartCoroutine(PatrolRoutine());
			break;
		case Activity.Sleep:
			Spine.AnimationState.SetAnimation(0, SleepingAnimation, true);
			DetectPlayer = WakeUpAnimation;
			break;
		}
	}

	private void Health_OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (health.Unaware)
		{
			ClearPatrol();
			if (Health.team2.Count > 1)
			{
				StartCoroutine(RaiseAlarmRoutine(EnemyHealth));
			}
			else
			{
				StartCoroutine(BeWarnedRoutine(EnemyHealth, 0f));
			}
		}
	}

	private void OnEnable()
	{
		EnemyStealths.Add(this);
	}

	private void OnDisable()
	{
		EnemyStealths.Remove(this);
		ClearPatrol();
		if (state != null)
		{
			StateMachine stateMachine = state;
			stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Remove(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
		}
	}

	private void OnDestroy()
	{
		if (EnemyStealthUI != null)
		{
			UnityEngine.Object.Destroy(EnemyStealthUI.gameObject);
		}
	}

	private void ClearPatrol()
	{
		if (cPatrolRoutine != null)
		{
			StopCoroutine(cPatrolRoutine);
		}
		if (unitObject != null)
		{
			UnityEngine.Object.Destroy(unitObject);
		}
		unitObject = null;
	}

	private IEnumerator PatrolRoutine()
	{
		if (unitObject == null)
		{
			unitObject = base.gameObject.AddComponent<UnitObject>();
		}
		unitObject.maxSpeed = PatrolSpeed;
		unitObject.distanceBetweenDustClouds = 1f;
		StateMachine stateMachine = state;
		stateMachine.OnStateChange = (StateMachine.StateChange)Delegate.Combine(stateMachine.OnStateChange, new StateMachine.StateChange(OnStateChange));
		while (true)
		{
			state.LookAngle = state.facingAngle;
			if (unitObject.pathToFollow == null)
			{
				yield return new WaitForSeconds(WaitBetweenPatrolPoints);
				Patrol = ++Patrol % PatrolRoute.Count;
				unitObject.givePath(StartPosition + PatrolRoute[Patrol]);
			}
			else if ((RepathTimer += Time.deltaTime) > 0.5f)
			{
				unitObject.givePath(StartPosition + PatrolRoute[Patrol]);
				RepathTimer = 0f;
			}
			yield return null;
		}
	}

	private void OnStateChange(StateMachine.State NewState, StateMachine.State PrevState)
	{
		switch (NewState)
		{
		case StateMachine.State.Idle:
			Spine.AnimationState.SetAnimation(0, PatrolIdle, true);
			break;
		case StateMachine.State.Moving:
			Spine.AnimationState.SetAnimation(0, PatrolWalk, true);
			break;
		}
	}

	private void Update()
	{
		Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		if (!health.Unaware || StartingActivity == Activity.Sleep || !GameManager.RoomActive)
		{
			return;
		}
		state.facingAngle = state.LookAngle;
		VisibleEnemies = 0;
		foreach (Health item in Health.playerTeam)
		{
			Distance = Vector3.Distance(base.transform.position, item.transform.position);
			if (!(item != null) || item.InStealthCover || !(Distance < VisionRage))
			{
				continue;
			}
			if (Distance < DetectEvenIfStealth)
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, item.transform.position - base.transform.position, VisionRage, DetectionLayer);
				if (raycastHit2D.collider != null && raycastHit2D.collider.gameObject == item.gameObject)
				{
					VisibleEnemies++;
					AlertLevel += Time.deltaTime * 10f;
					EnemyHealth = item;
				}
				continue;
			}
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(base.transform.position, item.transform.position - base.transform.position, VisionRage, DetectionLayer);
			if (raycastHit2D2.collider != null && raycastHit2D2.collider.gameObject == item.gameObject)
			{
				VisibleEnemies++;
				EnemyHealth = item;
				if (item.state.CURRENT_STATE == StateMachine.State.Stealth && Distance > DetectEvenIfStealth)
				{
					AlertLevel += Time.deltaTime * 2f;
				}
				else
				{
					AlertLevel += Time.deltaTime * 10f;
				}
			}
		}
		if (VisibleEnemies <= 0 && AlertLevel < AlertLimit)
		{
			AlertLevel -= Time.deltaTime * 2f;
		}
		EnemyStealthUI.UpdateProgress(AlertLevel / AlertLimit);
		if (!(AlertLevel > 0f))
		{
			return;
		}
		ClearPatrol();
		foreach (EnemyStealth enemyStealth in EnemyStealths)
		{
			if (enemyStealth.health.Unaware && Vector3.Distance(base.transform.position, enemyStealth.transform.position) < 15f)
			{
				enemyStealth.BeWarned(EnemyHealth, 0f);
			}
		}
	}

	private IEnumerator RaiseAlarmRoutine(Health TargetObject)
	{
		if (EnemyStealthUI != null)
		{
			UnityEngine.Object.Destroy(EnemyStealthUI.gameObject);
		}
		if (TargetObject != null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		}
		health.Unaware = false;
		state.CURRENT_STATE = StateMachine.State.RaiseAlarm;
		Spine.AnimationState.SetAnimation(0, RaiseAlarm, false);
		foreach (EnemyStealth enemyStealth in EnemyStealths)
		{
			if (enemyStealth != null && enemyStealth.health != null && enemyStealth.health.Unaware && Vector3.Distance(base.transform.position, enemyStealth.transform.position) < 15f)
			{
				enemyStealth.BeWarned(TargetObject, 1.3f);
			}
		}
		yield return new WaitForSeconds(alarmTime);
		if (AIScriptToDisable != null)
		{
			AIScriptToDisable.enabled = true;
		}
		base.enabled = false;
	}

	private void BeWarned(Health TargetObject, float Delay)
	{
		StartCoroutine(BeWarnedRoutine(TargetObject, Delay));
	}

	private IEnumerator BeWarnedRoutine(Health TargetObject, float Delay)
	{
		health.Unaware = false;
		if (EnemyStealthUI != null)
		{
			UnityEngine.Object.Destroy(EnemyStealthUI.gameObject);
		}
		if (AlertLevel <= AlertLimit * 0.3f)
		{
			yield return new WaitForSeconds(Delay);
		}
		if (TargetObject != null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		}
		state.CURRENT_STATE = StateMachine.State.RaiseAlarm;
		Spine.AnimationState.SetAnimation(0, DetectPlayer, false);
		yield return new WaitForSeconds(0.17f);
		if (warningIcon != null)
		{
			warningIcon.AnimationState.SetAnimation(0, "warn-start", false);
			warningIcon.AnimationState.AddAnimation(0, "warn-stop", false, 2f);
		}
		yield return new WaitForSeconds(warnTime);
		if (AIScriptToDisable != null)
		{
			AIScriptToDisable.enabled = true;
			if (TargetObject != null)
			{
				AIScriptToDisable.BeAlarmed(TargetObject.gameObject);
			}
		}
		base.enabled = false;
	}

	private void OnDrawGizmos()
	{
		if (StartingActivity != Activity.Patrol)
		{
			return;
		}
		if (!Application.isPlaying)
		{
			int num = -1;
			while (++num < PatrolRoute.Count)
			{
				if (num == PatrolRoute.Count - 1 || num == 0)
				{
					Utils.DrawLine(base.transform.position, base.transform.position + PatrolRoute[num], Color.yellow);
				}
				if (num > 0)
				{
					Utils.DrawLine(base.transform.position + PatrolRoute[num - 1], base.transform.position + PatrolRoute[num], Color.yellow);
				}
				Utils.DrawCircleXY(base.transform.position + PatrolRoute[num], 0.2f, Color.yellow);
			}
			return;
		}
		int num2 = -1;
		while (++num2 < PatrolRoute.Count)
		{
			if (num2 == PatrolRoute.Count - 1 || num2 == 0)
			{
				Utils.DrawLine(StartPosition, StartPosition + PatrolRoute[num2], Color.yellow);
			}
			if (num2 > 0)
			{
				Utils.DrawLine(StartPosition + PatrolRoute[num2 - 1], StartPosition + PatrolRoute[num2], Color.yellow);
			}
			Utils.DrawCircleXY(StartPosition + PatrolRoute[num2], 0.2f, Color.yellow);
		}
	}
}
