using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Spine.Unity;
using UnityEngine;

public class TrapCharger : BaseMonoBehaviour
{
	public enum AttackDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	public StateMachine state;

	public ColliderEvents damageColliderEventsUp;

	public ColliderEvents damageColliderEventsDown;

	public ColliderEvents damageColliderEventsLeft;

	public ColliderEvents damageColliderEventsRight;

	public Collider2D BlockingCollider;

	public SkeletonAnimation Spine;

	public SimpleSpineFlash[] SimpleSpineFlashes;

	public SimpleSpriteFlash[] SimpleSpriteFlashes;

	public AnimationCurve shakeCurve;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string DirectionAnimationOff;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string DirectionAnimationUp;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string DirectionAnimationDown;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string DirectionAnimationLeft;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string DirectionAnimationRight;

	private AttackDirection currentDirection;

	[SerializeField]
	private GameObject TargetObject;

	[SerializeField]
	private GameObject ActiveToggle;

	public LayerMask layerToCheck;

	public float AttackDelayTime;

	[SerializeField]
	private int enemyDamage = 3;

	[HideInInspector]
	public float AttackDelay;

	public float MovementSpeed = 1f;

	public float SignPostAttackDuration = 0.5f;

	public Vector2 scanDimensions = new Vector2(1f, 1f);

	[Space]
	[SerializeField]
	private Vector3[] path = new Vector3[0];

	[SerializeField]
	private bool loop;

	private float shakeDuration = 0.25f;

	private float shakeDistance = 0.25f;

	public ParticleSystem movementParticles;

	public ParticleSystem impactParticles;

	private LineRenderer lineRenderer;

	private int pathIndex;

	private bool deactivated;

	private LayerMask playerMask;

	[HideInInspector]
	public Rigidbody2D rb;

	public bool DisableForces;

	private void Awake()
	{
		state = GetComponent<StateMachine>();
		rb = base.gameObject.GetComponent<Rigidbody2D>();
		Vector3[] array = new Vector3[loop ? (path.Length + 1) : path.Length];
		for (int i = 0; i < path.Length; i++)
		{
			array[i] = base.transform.TransformPoint(path[i]);
		}
		if (loop)
		{
			array[array.Length - 1] = base.transform.TransformPoint(path[0]);
		}
		path = array;
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = path.Length;
		lineRenderer.SetPositions(path);
		playerMask = (int)playerMask | (1 << LayerMask.NameToLayer("Player"));
	}

	private void Start()
	{
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private void OnDestroy()
	{
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
	}

	private void RoomLockController_OnRoomCleared()
	{
		if (base.gameObject.activeInHierarchy)
		{
			deactivated = true;
		}
		ActiveToggle.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutQuart).OnComplete(delegate
		{
			ActiveToggle.SetActive(false);
		});
	}

	private void OnEnable()
	{
		if (damageColliderEventsUp != null)
		{
			damageColliderEventsUp.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEventsUp.SetActive(false);
		}
		if (damageColliderEventsDown != null)
		{
			damageColliderEventsDown.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEventsDown.SetActive(false);
		}
		if (damageColliderEventsLeft != null)
		{
			damageColliderEventsLeft.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEventsLeft.SetActive(false);
		}
		if (damageColliderEventsRight != null)
		{
			damageColliderEventsRight.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEventsRight.SetActive(false);
		}
		SimpleSpineFlashes = GetComponentsInChildren<SimpleSpineFlash>();
		SimpleSpriteFlashes = GetComponentsInChildren<SimpleSpriteFlash>();
		state.CURRENT_STATE = StateMachine.State.Idle;
		AttackDelay = 0f;
		StartCoroutine(WaitForTargetRoutine());
	}

	private void OnDisable()
	{
		if (damageColliderEventsUp != null)
		{
			damageColliderEventsUp.SetActive(false);
			damageColliderEventsUp.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		if (damageColliderEventsDown != null)
		{
			damageColliderEventsDown.SetActive(false);
			damageColliderEventsDown.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		if (damageColliderEventsLeft != null)
		{
			damageColliderEventsLeft.SetActive(false);
			damageColliderEventsLeft.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		if (damageColliderEventsRight != null)
		{
			damageColliderEventsRight.SetActive(false);
			damageColliderEventsRight.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		}
		StopAllCoroutines();
		SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
		for (int i = 0; i < simpleSpineFlashes.Length; i++)
		{
			simpleSpineFlashes[i].FlashWhite(false);
		}
		SimpleSpriteFlash[] simpleSpriteFlashes = SimpleSpriteFlashes;
		for (int i = 0; i < simpleSpriteFlashes.Length; i++)
		{
			simpleSpriteFlashes[i].FlashWhite(false);
		}
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != Health.Team.PlayerTeam || !TrinketManager.HasTrinket(TarotCards.Card.ImmuneToTraps)) && !component.ImmuneToTraps)
		{
			component.DealDamage((component.team != Health.Team.Team2) ? 1 : enemyDamage, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	protected IEnumerator WaitForTargetRoutine()
	{
		yield return new WaitForEndOfFrame();
		if (Spine != null)
		{
			yield return new WaitForSeconds(AttackDelay);
		}
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		TargetObject = PlayerFarming.Instance.gameObject;
		while (true)
		{
			if (!deactivated)
			{
				if (state.CURRENT_STATE == StateMachine.State.Idle)
				{
					Vector3 normalized3 = (TargetObject.transform.position - base.transform.position).normalized;
					Vector3 vector = path[pathIndex];
					Vector3 zero = Vector3.zero;
					int num = (int)Mathf.Repeat(pathIndex + 1, path.Length);
					int num2 = (int)Mathf.Repeat(pathIndex - 1, path.Length);
					if (pathIndex + 1 > path.Length - 1 && loop)
					{
						num++;
					}
					if (pathIndex - 1 < 0 && loop)
					{
						num2--;
					}
					if (!loop && pathIndex - 1 < 0)
					{
						num2 = 0;
					}
					else if (!loop && pathIndex + 1 > path.Length - 1)
					{
						num = pathIndex;
					}
					Vector3 normalized = (path[num] - vector).normalized;
					Vector3 normalized2 = (path[num2] - vector).normalized;
					Vector3 targetPosition;
					if ((bool)Physics2D.BoxCast(base.transform.position, Vector3.one * 2f, 0f, normalized, Vector3.Distance(path[num], vector), playerMask))
					{
						targetPosition = path[num];
						pathIndex = (int)Mathf.Repeat(num, path.Length);
					}
					else
					{
						if (!Physics2D.BoxCast(base.transform.position, Vector3.one * 2f, 0f, normalized2, Vector3.Distance(path[num2], vector), playerMask))
						{
							yield return null;
							continue;
						}
						targetPosition = path[num2];
						pathIndex = (int)Mathf.Repeat(num2, path.Length);
					}
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					AudioManager.Instance.PlayOneShot("event:/enemy/moving_spike_trap/moving_spike_trap_start", base.gameObject);
					SimpleSpineFlash[] simpleSpineFlashes = SimpleSpineFlashes;
					for (int i = 0; i < simpleSpineFlashes.Length; i++)
					{
						simpleSpineFlashes[i].FlashWhite(0.5f);
					}
					SimpleSpriteFlash[] simpleSpriteFlashes = SimpleSpriteFlashes;
					for (int i = 0; i < simpleSpriteFlashes.Length; i++)
					{
						simpleSpriteFlashes[i].FlashWhite(0.5f);
					}
					yield return new WaitForSeconds(SignPostAttackDuration);
					simpleSpineFlashes = SimpleSpineFlashes;
					for (int i = 0; i < simpleSpineFlashes.Length; i++)
					{
						simpleSpineFlashes[i].FlashWhite(false);
					}
					simpleSpriteFlashes = SimpleSpriteFlashes;
					for (int i = 0; i < simpleSpriteFlashes.Length; i++)
					{
						simpleSpriteFlashes[i].FlashWhite(false);
					}
					state.CURRENT_STATE = StateMachine.State.Moving;
					movementParticles.Play();
					damageColliderEventsUp.SetActive(true);
					damageColliderEventsDown.SetActive(true);
					damageColliderEventsLeft.SetActive(true);
					damageColliderEventsRight.SetActive(true);
					state.LookAngle = state.facingAngle;
					BlockingCollider.enabled = false;
					bool moving = true;
					TweenerCore<Vector3, Vector3, VectorOptions> tween = base.transform.DOMove(targetPosition, MovementSpeed).SetSpeedBased().SetEase(Ease.InSine)
						.OnComplete(delegate
						{
							moving = false;
						});
					while (moving)
					{
						if (PlayerRelic.TimeFrozen && tween.IsPlaying())
						{
							tween.Pause();
						}
						else if (!PlayerRelic.TimeFrozen && !tween.IsPlaying())
						{
							tween.Play();
						}
						yield return null;
					}
					EndCharge();
					BlockingCollider.enabled = true;
					yield return new WaitForSeconds(1f);
					state.CURRENT_STATE = StateMachine.State.Idle;
				}
				else
				{
					yield return null;
				}
			}
			yield return null;
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			Vector3[] array = new Vector3[loop ? (path.Length + 1) : path.Length];
			for (int i = 0; i < path.Length; i++)
			{
				array[i] = base.transform.TransformPoint(path[i]);
			}
			if (loop)
			{
				array[array.Length - 1] = base.transform.TransformPoint(path[0]);
			}
			GetComponent<LineRenderer>().positionCount = array.Length;
			GetComponent<LineRenderer>().SetPositions(array);
			Vector3[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				Utils.DrawCircleXY(array2[j], 0.5f, Color.blue);
			}
			if (!(TargetObject == null))
			{
				Utils.DrawLine(base.transform.position, base.transform.position + (TargetObject.transform.position - base.transform.position).normalized * (TargetObject.transform.position - base.transform.position).magnitude, Color.magenta);
			}
		}
	}

	private void EndCharge()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/moving_spike_trap/moving_spike_trap_stop", base.gameObject);
		damageColliderEventsUp.SetActive(false);
		damageColliderEventsDown.SetActive(false);
		damageColliderEventsLeft.SetActive(false);
		damageColliderEventsRight.SetActive(false);
		CameraManager.shakeCamera(2f);
		StartCoroutine(ShakeRoutine(currentDirection));
		impactParticles.Play();
		AttackDelay = AttackDelayTime;
		movementParticles.Stop();
		StartCoroutine(WaitForTargetRoutine());
	}

	private IEnumerator ShakeRoutine(AttackDirection attackDirection)
	{
		float t = 0f;
		DisableForces = true;
		Vector3 targetPos = base.transform.position;
		Vector2 targetDir = Vector2.down;
		switch (attackDirection)
		{
		case AttackDirection.Down:
			targetDir = Vector2.up;
			break;
		case AttackDirection.Left:
			targetDir = Vector2.right;
			break;
		case AttackDirection.Right:
			targetDir = Vector2.left;
			break;
		}
		while (t < shakeDuration)
		{
			base.transform.position = targetPos + (Vector3)(targetDir * shakeCurve.Evaluate(t / shakeDuration) * shakeDistance);
			t += Time.deltaTime;
			yield return null;
		}
		base.transform.position = targetPos;
		DisableForces = false;
	}
}
