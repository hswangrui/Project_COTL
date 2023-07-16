using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ProjectileGhost : BaseMonoBehaviour
{
	public float AttackInterval;

	private float AttackDelay;

	public SimpleSpineAnimator simpleSpineAnimator;

	public TrailRenderer TrailRenderer;

	public Collider2D DamageCollider;

	private List<Collider2D> collider2DList;

	private Health CollisionHealth;

	private float DetectEnemyRange = 50f;

	private GameObject _Master;

	private Health MasterHealth;

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

	public SkeletonAnimation spine;

	public LayerMask layerToCheck;

	public List<Health> DoubleHit;

	private float TurnSpeed;

	private EventInstance sfxLoop;

	private float damageMultiplier = 1f;

	private float checkFrame;

	private Health cachedTarget;

	private Vector3 pointToCheck;

	private Vector3 Seperator;

	public float SeperationRadius = 0.5f;

	private GameObject Master
	{
		get
		{
			if (_Master == null)
			{
				_Master = GameObject.FindGameObjectWithTag("Player");
				if (_Master != null)
				{
					MasterState = _Master.GetComponent<StateMachine>();
					MasterHealth = _Master.GetComponent<Health>();
				}
			}
			return _Master;
		}
		set
		{
			_Master = value;
		}
	}

	public static void SpawnGhost(Vector3 position, float delay, float damageMultplier, float scale = 1f)
	{
		GameManager.GetInstance().StartCoroutine(DelayCallback(delay, delegate
		{
			SpawnGhost(position, damageMultplier, scale);
		}));
	}

	private static IEnumerator DelayCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(sfxLoop);
	}

	public static void SpawnGhost(Vector3 position, float damageMultiplier, float scale = 1f)
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Enemies/Weapons/ArrowGhost.prefab", position, Quaternion.identity, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			ProjectileGhost component = obj.Result.GetComponent<ProjectileGhost>();
			component.transform.localScale *= scale;
			component.damageMultiplier = damageMultiplier;
			scale = component.transform.localScale.x;
			component.transform.localScale = Vector3.zero;
			component.transform.DOScale(scale, 0.1f);
		};
	}

	private void Start()
	{
		state = GetComponent<StateMachine>();
		SpineVZ = -1.5f;
		SpineVY = 0.5f;
		state.CURRENT_STATE = StateMachine.State.SpawnIn;
		AudioManager.Instance.PlayOneShot("event:/weapon/necromancer_ghost/ghost_spawn", base.gameObject);
		sfxLoop = AudioManager.Instance.CreateLoop("event:/weapon/necromancer_ghost/ghost_loop", base.gameObject, true);
		TurnSpeed = UnityEngine.Random.Range(1f, 7f);
	}

	private void Health_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.StopLoop(sfxLoop);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		AudioManager.Instance.StopLoop(sfxLoop);
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
		if ((bool)PlayerFarming.Instance)
		{
			PlayerFarming.Instance.health.OnDie -= Health_OnDie;
		}
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		base.transform.position = Master.transform.position + Vector3.right;
	}

	private void Update()
	{
		if (Master == null || !Master.gameObject.activeSelf || !base.gameObject.activeSelf || GameManager.DeltaTime == 0f)
		{
			return;
		}
		if ((state.CURRENT_STATE == StateMachine.State.Idle || state.CURRENT_STATE == StateMachine.State.Moving) && (AttackDelay += Time.deltaTime) > AttackInterval)
		{
			AttackDelay = 0f;
			if (GetClosestTarget() != null)
			{
				AudioManager.Instance.PlayOneShot("event:/weapon/necromancer_ghost/ghost_attack", base.gameObject);
				state.CURRENT_STATE = StateMachine.State.SignPostAttack;
			}
		}
		if (state.CURRENT_STATE != StateMachine.State.SignPostAttack && state.Timer > 1f)
		{
			if (state.CURRENT_STATE != StateMachine.State.SpawnOut)
			{
				AudioManager.Instance.PlayOneShot("event:/weapon/necromancer_ghost/ghost_leave", base.transform.position);
			}
			state.CURRENT_STATE = StateMachine.State.SpawnOut;
		}
		TrailRenderer.emitting = state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.SignPostAttack || state.CURRENT_STATE == StateMachine.State.RecoverFromAttack;
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.SpawnIn:
			if ((state.Timer += Time.deltaTime) > 0.5f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.Idle:
			state.Timer += Time.deltaTime;
			break;
		case StateMachine.State.Moving:
			state.Timer += Time.deltaTime;
			break;
		case StateMachine.State.SignPostAttack:
			Speed += (0f - Speed) / (7f * GameManager.DeltaTime);
			if (Time.frameCount % 5 == 0)
			{
				simpleSpineAnimator.FlashWhite(!simpleSpineAnimator.isFillWhite);
			}
			if ((state.Timer += Time.deltaTime) > 0.2f)
			{
				simpleSpineAnimator.FlashWhite(false);
				if (GetClosestTarget() != null)
				{
					TargetAngle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
				}
				CameraManager.shakeCamera(0.5f, state.facingAngle);
				Speed = 30f;
				state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
				DoubleHit.Clear();
			}
			break;
		case StateMachine.State.RecoverFromAttack:
			if (state.Timer < 2f)
			{
				if (GetClosestTarget() != null)
				{
					TargetAngle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
				}
				collider2DList = new List<Collider2D>();
				DamageCollider.GetContacts(collider2DList);
				foreach (Collider2D collider2D in collider2DList)
				{
					CollisionHealth = collider2D.gameObject.GetComponent<Health>();
					if (!DoubleHit.Contains(CollisionHealth) && CollisionHealth != null && !CollisionHealth.invincible && !CollisionHealth.untouchable && CollisionHealth.team == Health.Team.Team2)
					{
						CollisionHealth.DealDamage(1f * damageMultiplier, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile);
						AudioManager.Instance.StopLoop(sfxLoop);
						UnityEngine.Object.Destroy(base.gameObject);
						return;
					}
				}
			}
			if (GetClosestTarget() != null && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < 2f)
			{
				GetClosestTarget().DealDamage(1f * damageMultiplier, base.gameObject, base.transform.position, false, Health.AttackTypes.Projectile);
				AudioManager.Instance.StopLoop(sfxLoop);
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (Speed > 0f)
			{
				if (DoubleHit.Count > 0)
				{
					Speed -= 2f * GameManager.DeltaTime;
				}
				else
				{
					Speed -= 1f * GameManager.DeltaTime;
				}
				if (Speed <= 0f)
				{
					Speed = 0f;
				}
			}
			if ((state.Timer += Time.deltaTime) > 2f)
			{
				state.CURRENT_STATE = StateMachine.State.SpawnOut;
				AudioManager.Instance.PlayOneShot("event:/weapon/necromancer_ghost/ghost_leave", base.transform.position);
			}
			break;
		case StateMachine.State.SpawnOut:
			if ((state.Timer += Time.deltaTime) > 0.95f)
			{
				AudioManager.Instance.StopLoop(sfxLoop);
				UnityEngine.Object.Destroy(base.gameObject);
			}
			break;
		}
		vx = Speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		vy = Speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		spine.skeleton.ScaleX = ((!(Master.transform.position.x > base.transform.position.x)) ? 1 : (-1));
		spine.transform.eulerAngles = new Vector3(-60f, 0f, vx * -5f / Time.deltaTime);
		SpineVZ = Mathf.Lerp(SpineVZ, -1f, 5f * Time.deltaTime);
		SpineVY = Mathf.Lerp(SpineVY, 0.5f, 5f * Time.deltaTime);
		state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / TurnSpeed * Time.deltaTime * 60f;
		base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime, Speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime);
		SeperateDemons();
	}

	protected Health GetClosestTarget()
	{
		if (Time.time == checkFrame)
		{
			return cachedTarget;
		}
		Health.Team team = Health.Team.Team2;
		List<Health> list = new List<Health>();
		UnitObject[] componentsInChildren = BiomeGenerator.Instance.CurrentRoom.generateRoom.GetComponentsInChildren<UnitObject>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Health componentInChildren = componentsInChildren[i].GetComponentInChildren<Health>();
			if (componentInChildren.enabled && !componentInChildren.invincible && !componentInChildren.untouchable && !componentInChildren.InanimateObject && !(componentInChildren.HP <= 0f) && (bool)componentInChildren && componentInChildren.team == team)
			{
				list.Add(componentInChildren);
			}
		}
		Health health = null;
		foreach (Health item in list)
		{
			if (!(Vector3.Distance(item.transform.position, base.transform.position) > DetectEnemyRange) && (health == null || Vector3.Distance(item.transform.position, base.transform.position) < Vector3.Distance(health.transform.position, base.transform.position)))
			{
				health = item;
			}
		}
		checkFrame = Time.time;
		cachedTarget = health;
		return health;
	}

	public bool CheckLineOfSight(Vector3 pointToCheck, float distance)
	{
		this.pointToCheck = pointToCheck;
		if (Physics2D.Raycast(base.transform.position, pointToCheck - base.transform.position, distance, layerToCheck).collider != null)
		{
			return false;
		}
		return true;
	}

	private void SeperateDemons()
	{
		Seperator = Vector3.zero;
		foreach (GameObject demon in Demon_Arrows.Demons)
		{
			if (demon != base.gameObject && demon != null && state.CURRENT_STATE != StateMachine.State.SignPostAttack && state.CURRENT_STATE != StateMachine.State.RecoverFromAttack)
			{
				float num = Vector2.Distance(demon.gameObject.transform.position, base.transform.position);
				float angle = Utils.GetAngle(demon.gameObject.transform.position, base.transform.position);
				if (num < SeperationRadius)
				{
					Seperator.x += (SeperationRadius - num) / 2f * Mathf.Cos(angle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
					Seperator.y += (SeperationRadius - num) / 2f * Mathf.Sin(angle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
				}
			}
		}
		base.transform.position = base.transform.position + Seperator;
	}
}
