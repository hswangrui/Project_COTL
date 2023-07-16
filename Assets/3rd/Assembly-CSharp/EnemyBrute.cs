using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyBrute : UnitObject
{
	private enum AttackTypes
	{
		Area,
		Punch,
		Last
	}

	public SimpleSpineAnimator simpleSpineAnimator;

	private SimpleSpineEventListener simpleSpineEventListener;

	private GameObject TargetObject;

	private float GrappleCoolDown;

	public ColliderEvents damageColliderEvents;

	public ParticleSystem ParticleSystem;

	public TargetWarning TargetWarning;

	public LineRenderer lineRenderer;

	[EventRef]
	public string punchSoundPath = string.Empty;

	[EventRef]
	public string areaAttackSoundPath = string.Empty;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	private float LassoDistance = 4f;

	private float LassoMaxDistance = 9f;

	private float PostAttackDuration;

	private AttackTypes AttackType;

	private int AlternateAttack;

	public GameObject RockToThrow;

	public Transform ThrowBone;

	private float TargetDistance;

	private readonly float MaxThrowRange = 20f;

	private Vector3 TargetPosition
	{
		get
		{
			if (!(TargetObject == null))
			{
				return TargetObject.transform.position;
			}
			return Vector3.zero;
		}
	}

	private void Start()
	{
		state.facingAngle = UnityEngine.Random.Range(0, 360);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		simpleSpineEventListener = GetComponent<SimpleSpineEventListener>();
		simpleSpineEventListener.OnSpineEvent += OnSpineEvent;
		TargetWarning.gameObject.SetActive(false);
		ParticleSystem.Stop();
		StartCoroutine(WaitForEnemy());
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		simpleSpineAnimator.FlashFillRed();
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		TargetWarning.gameObject.SetActive(false);
		simpleSpineAnimator.FlashWhite(false);
		ClearTarget();
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		StopAllCoroutines();
		if (simpleSpineEventListener != null)
		{
			simpleSpineEventListener.OnSpineEvent -= OnSpineEvent;
		}
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		}
	}

	private void OnSpineEvent(string EventName)
	{
		switch (EventName)
		{
		case "throw":
			if (simpleSpineAnimator.IsVisible)
			{
				CameraManager.shakeCamera(0.4f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
			}
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			UnityEngine.Object.Instantiate(RockToThrow, TargetObject.transform.position, Quaternion.identity, base.transform.parent).GetComponent<BruteRock>().Play(ThrowBone.transform.position);
			break;
		case "shake":
			if (simpleSpineAnimator.IsVisible)
			{
				CameraManager.shakeCamera(0.4f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
			}
			break;
		}
	}

	private IEnumerator WaitForEnemy()
	{
		while ((TargetObject = GameObject.FindWithTag("Player")) == null)
		{
			yield return null;
		}
		TargetObject.GetComponent<Health>().attackers.Add(base.gameObject);
		StartCoroutine(ChasePlayer());
	}

	private IEnumerator LassoPlayer()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		simpleSpineAnimator.Animate("quickattack-charge", 0, false);
		yield return new WaitForSeconds(0.3f);
		simpleSpineAnimator.Animate("quickattack-impact", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		lineRenderer.gameObject.SetActive(true);
		float Timer = 0f;
		float Progress;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num <= 0.2f))
			{
				break;
			}
			Progress = Timer / 0.2f;
			lineRenderer.SetPosition(0, base.transform.position - Vector3.forward * 0.5f);
			lineRenderer.SetPosition(1, Vector3.Lerp(base.transform.position - Vector3.forward * 0.5f, TargetPosition - Vector3.forward * 0.5f, Progress));
			yield return null;
		}
		GameObject obj = BiomeConstants.Instance.HitFX_Blocked.Spawn();
		obj.transform.position = TargetPosition - Vector3.forward;
		obj.transform.rotation = Quaternion.identity;
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, TargetPosition));
		GameManager.GetInstance().HitStop();
		lineRenderer.SetPosition(0, base.transform.position - Vector3.forward * 0.5f);
		lineRenderer.SetPosition(1, TargetPosition - Vector3.forward);
		StateMachine PlayerState = TargetObject.GetComponent<StateMachine>();
		PlayerState.CURRENT_STATE = StateMachine.State.InActive;
		yield return new WaitForSeconds(0.2f);
		Progress = 0f;
		float ProgressSpeed = -2f;
		float Distance = Vector2.Distance(base.transform.position, TargetPosition);
		while (Progress <= 1f)
		{
			Progress += ProgressSpeed * Time.deltaTime;
			if (ProgressSpeed < 5f)
			{
				ProgressSpeed += 0.5f;
			}
			float num2 = Mathf.LerpUnclamped(Distance, 2f, Progress);
			float f = Utils.GetAngle(base.transform.position, TargetPosition) * ((float)Math.PI / 180f);
			TargetObject.transform.position = base.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
			lineRenderer.SetPosition(0, base.transform.position - Vector3.forward * 0.5f);
			lineRenderer.SetPosition(1, TargetPosition - Vector3.forward * 0.5f);
			yield return null;
		}
		PlayerState.CURRENT_STATE = StateMachine.State.Idle;
		lineRenderer.gameObject.SetActive(false);
		StartCoroutine(ChasePlayer());
		AttackType = AttackTypes.Area;
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		simpleSpineAnimator.Animate("attack-charge", 0, false);
		StartCoroutine(LassoTimerCountDown());
	}

	private IEnumerator LassoTimerCountDown()
	{
		GrappleCoolDown = 3f;
		while ((GrappleCoolDown -= Time.deltaTime) > 0f)
		{
			yield return null;
		}
	}

	private IEnumerator ThrowRock()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		simpleSpineAnimator.Animate("throw", 0, false);
		yield return new WaitForSeconds(3.2f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(ChasePlayer());
	}

	public virtual IEnumerator ChasePlayer()
	{
		givePath(TargetObject.transform.position);
		float RepathTimer = 0f;
		bool Loop = true;
		while (Loop)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Moving:
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				if (Vector2.Distance(base.transform.position, TargetPosition) < 2f)
				{
					AttackType = AttackTypes.Area;
					switch (AttackType)
					{
					case AttackTypes.Area:
						TargetWarning.gameObject.SetActive(true);
						simpleSpineAnimator.Animate("attack-charge", 0, false);
						break;
					case AttackTypes.Punch:
						simpleSpineAnimator.Animate("quickattack-charge", 0, false);
						break;
					}
					state.facingAngle = Utils.GetAngle(base.transform.position, TargetPosition);
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
				}
				else
				{
					float num;
					RepathTimer = (num = RepathTimer + Time.deltaTime);
					if (num > 0.2f)
					{
						RepathTimer = 0f;
						Vector3.Distance(base.transform.position, TargetPosition);
						givePath(PlayerFarming.Instance.transform.position);
					}
				}
				break;
			case StateMachine.State.SignPostAttack:
				simpleSpineAnimator.FlashMeWhite();
				switch (AttackType)
				{
				case AttackTypes.Area:
					if ((state.Timer += Time.deltaTime) >= 1f)
					{
						PostAttackDuration = 1.5f;
						simpleSpineAnimator.Animate("attack-impact", 0, false);
						simpleSpineAnimator.FlashWhite(false);
						CameraManager.instance.ShakeCameraForDuration(0.2f, 0.5f, 0.3f);
						state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
						if (!string.IsNullOrEmpty(areaAttackSoundPath))
						{
							AudioManager.Instance.PlayOneShot(areaAttackSoundPath, base.transform.position);
						}
						GameManager.GetInstance().HitStop();
						ParticleSystem.Play();
						TargetWarning.gameObject.SetActive(false);
						if (damageColliderEvents != null)
						{
							damageColliderEvents.SetActive(true);
						}
					}
					else if (damageColliderEvents != null)
					{
						damageColliderEvents.SetActive(false);
					}
					break;
				case AttackTypes.Punch:
					if ((state.Timer += Time.deltaTime) >= 0.5f)
					{
						PostAttackDuration = 1f;
						simpleSpineAnimator.Animate("quickattack-impact", 0, false);
						simpleSpineAnimator.FlashWhite(false);
						state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
						if (!string.IsNullOrEmpty(punchSoundPath))
						{
							AudioManager.Instance.PlayOneShot(punchSoundPath, base.transform.position);
						}
						float AttackRange = 2f;
						AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Enemies/Weapons/Swipe.prefab", base.transform.position + new Vector3(AttackRange * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), AttackRange * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f), Quaternion.identity, base.transform.parent);
						asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
						{
							Swipe component = obj.Result.GetComponent<Swipe>();
							Vector3 position = base.transform.position + new Vector3(AttackRange * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), AttackRange * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f);
							component.Init(position, state.facingAngle, health.team, health, null, AttackRange);
						};
					}
					if (damageColliderEvents != null)
					{
						damageColliderEvents.SetActive(false);
					}
					break;
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				state.Timer += Time.deltaTime;
				if (state.Timer >= PostAttackDuration)
				{
					AttackType = AttackTypes.Punch;
					if (TargetObject == null)
					{
						ClearTarget();
						Loop = false;
						StartCoroutine(WaitForEnemy());
					}
					else
					{
						Vector3.Distance(base.transform.position, TargetPosition);
						givePath(TargetObject.transform.position);
					}
				}
				if (state.Timer >= 0.1f && damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				break;
			}
			yield return null;
		}
	}

	private void ClearTarget()
	{
		if (TargetObject != null)
		{
			TargetObject.GetComponent<Health>().attackers.Remove(base.gameObject);
		}
		TargetObject = null;
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}
}
