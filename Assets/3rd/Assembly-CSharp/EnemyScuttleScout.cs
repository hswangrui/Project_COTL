using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyScuttleScout : EnemyScuttleSwiper
{
	public AssetReferenceGameObject[] EnemyList;

	public SimpleSpineFlash SimpleSpineFlash;

	public float MortarDelay = 1f;

	private float MortarTimer;

	private float Distance = 2f;

	private float SpawnCircleCastRadius = 1f;

	public int SummonedCount;

	private List<AsyncOperationHandle<GameObject>> loadedAddressableAssets = new List<AsyncOperationHandle<GameObject>>();

	[EventRef]
	public string attackSoundPath = string.Empty;

	private bool canBeParried;

	private static float signPostParryWindow = 0.2f;

	private static float attackParryWindow = 0.15f;

	public float SignPostCloseCombatDelay = 1f;

	private float CloseCombatCooldown;

	public GameObject projectilePrefab;

	protected const float minBombRange = 2.5f;

	protected const float maxBombRange = 8f;

	public float timeBetweenShots = 0.5f;

	public float bombDuration = 0.75f;

	public int MortarShotsToFire = 2;

	private List<Vector3> TeleportPositions;

	private Vector3 Direction;

	private RaycastHit2D Results;

	public override void Awake()
	{
		base.Awake();
		if (BiomeGenerator.Instance != null)
		{
			GetComponent<Health>().totalHP *= BiomeGenerator.Instance.HumanoidHealthMultiplier;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (loadedAddressableAssets == null)
		{
			return;
		}
		foreach (AsyncOperationHandle<GameObject> loadedAddressableAsset in loadedAddressableAssets)
		{
			Addressables.Release((AsyncOperationHandle)loadedAddressableAsset);
		}
		loadedAddressableAssets.Clear();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		DoKnockBack(Attacker, KnockbackModifier, 0.5f);
		if (!string.IsNullOrEmpty(GetHitVO))
		{
			AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!string.IsNullOrEmpty(DeathVO))
		{
			AudioManager.Instance.PlayOneShot(DeathVO, base.transform.position);
		}
	}

	protected override IEnumerator ActiveRoutine()
	{
		MortarTimer = UnityEngine.Random.Range(3, 5);
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (state.CURRENT_STATE == StateMachine.State.Idle && (IdleWait -= Time.deltaTime * Spine.timeScale) <= 0f)
			{
				GetNewTargetPosition();
			}
			if (TargetObject != null && !Attacking && !IsStunned && GameManager.RoomActive)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			}
			else
			{
				state.LookAngle = state.facingAngle;
			}
			if (MovingAnimation != "")
			{
				if (state.CURRENT_STATE == StateMachine.State.Moving && Spine.AnimationName != MovingAnimation)
				{
					Spine.AnimationState.SetAnimation(0, MovingAnimation, true);
				}
				if (state.CURRENT_STATE == StateMachine.State.Idle && Spine.AnimationName != IdleAnimation)
				{
					Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
				}
			}
			if (TargetObject == null)
			{
				GetNewTarget();
				if (TargetObject != null)
				{
					ShowWarningIcon();
				}
			}
			else
			{
				if (!Attacking && (CloseCombatCooldown -= Time.deltaTime * Spine.timeScale) < 0f && Vector3.Distance(base.transform.position, TargetObject.transform.position) < 2f)
				{
					StartCoroutine(CloseCombatAttack());
				}
				if (ShouldMortar())
				{
					StartCoroutine(DoThrowMortar());
				}
			}
			yield return null;
		}
	}

	private IEnumerator CloseCombatAttack()
	{
		Attacking = true;
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		float Progress = 0f;
		AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
		Spine.AnimationState.SetAnimation(0, "grunt-attack-charge2", false);
		state.facingAngle = (state.LookAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position));
		Spine.skeleton.ScaleX = ((state.LookAngle > 90f && state.LookAngle < 270f) ? 1 : (-1));
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < SignPostCloseCombatDelay))
			{
				break;
			}
			if (Progress >= SignPostCloseCombatDelay - signPostParryWindow)
			{
				canBeParried = true;
			}
			SimpleSpineFlash.FlashWhite(Progress / SignPostCloseCombatDelay);
			yield return null;
		}
		speed = 0.2f;
		SimpleSpineFlash.FlashWhite(false);
		Spine.AnimationState.SetAnimation(0, "grunt-attack-impact2", false);
		if (!string.IsNullOrEmpty(AttackVO))
		{
			AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
		}
		if (!string.IsNullOrEmpty(attackSoundPath))
		{
			AudioManager.Instance.PlayOneShot(attackSoundPath, base.transform.position);
		}
		Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration))
			{
				break;
			}
			if (damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(true);
			}
			canBeParried = Progress <= attackParryWindow;
			yield return null;
		}
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		canBeParried = false;
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.8f))
			{
				break;
			}
			yield return null;
		}
		CloseCombatCooldown = 1f;
		state.CURRENT_STATE = StateMachine.State.Idle;
		Attacking = false;
	}

	private IEnumerator DoThrowMortar()
	{
		state.CURRENT_STATE = StateMachine.State.Attacking;
		Attacking = true;
		Vector3 targetPosition = TargetObject.transform.position;
		for (int i = 0; i < MortarShotsToFire; i++)
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.gameObject);
			Spine.AnimationState.SetAnimation(0, "throw-bomb", false);
			Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			float Progress = 0f;
			float Duration = 1f;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
				if (!(num < Duration))
				{
					break;
				}
				SimpleSpineFlash.FlashWhite(Progress / Duration);
				yield return null;
			}
			if (TargetObject == null)
			{
				MortarTimer = MortarDelay;
				yield break;
			}
			if (MortarShotsToFire > 1)
			{
				targetPosition = TargetObject.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f;
			}
			MortarBomb component = UnityEngine.Object.Instantiate(projectilePrefab, TargetObject.transform.position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>();
			float num2 = Vector2.Distance(base.transform.position, targetPosition);
			if (num2 < 2.5f)
			{
				component.transform.position = base.transform.position + (targetPosition - base.transform.position).normalized * 2.5f;
			}
			else if (num2 > 8f)
			{
				component.transform.position = base.transform.position + (targetPosition - base.transform.position).normalized * 8f;
			}
			component.Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.KillAll);
			SimpleSpineFlash.FlashWhite(false);
			if (!string.IsNullOrEmpty(AttackVO))
			{
				AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
			}
			if (!string.IsNullOrEmpty(attackSoundPath))
			{
				AudioManager.Instance.PlayOneShot(attackSoundPath, base.transform.position);
			}
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < timeBetweenShots))
				{
					break;
				}
				yield return null;
			}
		}
		SimpleSpineFlash.FlashWhite(false);
		state.CURRENT_STATE = StateMachine.State.Idle;
		Attacking = false;
		MortarTimer = MortarDelay;
	}

	protected virtual bool ShouldMortar()
	{
		if ((MortarTimer -= Time.deltaTime * Spine.timeScale) < 0f && !Attacking && Vector3.Distance(base.transform.position, TargetObject.transform.position) < 8f)
		{
			return GameManager.RoomActive;
		}
		return false;
	}

	protected override IEnumerator AttackRoutine()
	{
		Debug.Log("ATTACK!!!");
		if (GetAvailableSpawnPositions().Count <= 0 || SummonedCount >= 2)
		{
			AttackDelay = AttackDelayTime;
			yield break;
		}
		state.CURRENT_STATE = StateMachine.State.Attacking;
		Attacking = true;
		if (!string.IsNullOrEmpty(AttackVO))
		{
			AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
		}
		Spine.AnimationState.SetAnimation(0, "alarm", false);
		Spine.AnimationState.AddAnimation(0, "dance", true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 5f / 6f))
			{
				break;
			}
			yield return null;
		}
		Health.team2.Add(null);
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null)
		{
			instance.Enemies.Add(null);
		}
		foreach (Vector3 teleportPosition in TeleportPositions)
		{
			Vector3 vector = teleportPosition;
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(EnemyList[UnityEngine.Random.Range(0, EnemyList.Length)]);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				loadedAddressableAssets.Add(obj);
				if (Health.team2.Contains(null))
				{
					Health.team2.Remove(null);
					Interaction_Chest instance2 = Interaction_Chest.Instance;
					if ((object)instance2 != null)
					{
						instance2.Enemies.Remove(null);
					}
				}
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(obj.Result.transform.position, obj.Result.transform.parent, obj.Result);
				obj.Result.SetActive(false);
				Health component = obj.Result.GetComponent<Health>();
				component.OnDie += RemoveSpawned;
				Interaction_Chest instance3 = Interaction_Chest.Instance;
				if ((object)instance3 != null)
				{
					instance3.AddEnemy(component);
				}
			};
			SummonedCount++;
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1.2666667f))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		Attacking = false;
		AttackDelay = AttackDelayTime;
	}

	private List<Vector3> GetAvailableSpawnPositions()
	{
		TeleportPositions = new List<Vector3>();
		int num = -3;
		while ((num += 2) <= 1)
		{
			float f = (state.LookAngle + (float)(45 * num)) * ((float)Math.PI / 180f);
			Direction = new Vector3(Mathf.Cos(f), Mathf.Sin(f));
			Results = Physics2D.CircleCast(base.transform.position, SpawnCircleCastRadius, Direction, Distance, layerToCheck);
			if (Results.collider == null)
			{
				TeleportPositions.Add(base.transform.position + Direction * Distance);
			}
		}
		if (TeleportPositions.Count <= 0)
		{
			Direction = new Vector3(Mathf.Cos(state.LookAngle), Mathf.Sin(state.LookAngle));
			Results = Physics2D.CircleCast(base.transform.position, SpawnCircleCastRadius, Direction, Distance, layerToCheck);
			if (Results.collider == null)
			{
				TeleportPositions.Add(base.transform.position + Direction * Distance);
			}
		}
		return TeleportPositions;
	}

	private void RemoveSpawned(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AttackDelay = AttackDelayTime;
		SummonedCount--;
		Victim.OnDie -= RemoveSpawned;
	}
}
