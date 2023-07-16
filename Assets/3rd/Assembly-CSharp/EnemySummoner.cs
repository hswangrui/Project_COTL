using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WebSocketSharp;

public class EnemySummoner : UnitObject
{
	public bool Summon = true;

	public bool FireBalls = true;

	public bool Mortar = true;

	public bool HealOthers = true;

	public SkeletonAnimation skeletonAnimation;

	public SimpleSpineFlash simpleSpineFlash;

	public GameObject Arrow;

	public SpriteRenderer Shadow;

	public ParticleSystem summonParticles;

	public ParticleSystem teleportEffect;

	public float SeperationRadius = 0.5f;

	private GameObject TargetObject;

	public float Range = 6f;

	public float KnockbackSpeed = 0.2f;

	private CircleCollider2D CircleCollider;

	public AssetReferenceGameObject[] EnemyList;

	[SerializeField]
	private bool isSpawnablesIncreasingDamageMultiplier = true;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string idleAnimation;

	[EventRef]
	public string AttackVO = string.Empty;

	[EventRef]
	public string DeathVO = string.Empty;

	[EventRef]
	public string GetHitVO = string.Empty;

	[EventRef]
	public string WarningVO = string.Empty;

	[EventRef]
	public string SummonSfx = "event:/enemy/summon";

	private float StartSpeed = 0.4f;

	public string LoopedSoundSFX;

	private EventInstance LoopedSound;

	private int SummonedCount;

	private float SummonDelay = 1f;

	private float FireBallDelay = 1f;

	private float MortarDelay = 1f;

	private float HealDelay = 1f;

	private float TeleportDelay = 3f;

	public float TeleportDelayMin = 3f;

	public float TeleportDelayMax = 5f;

	public float TeleportFleeRadius = 2.5f;

	public float TeleportFleeDelayMax = 2f;

	public float TeleportMinDistance = 4f;

	public float TeleportMaxDistance = 7f;

	private float FleeDelay = 1f;

	private GameObject EnemySpawnerGO;

	public int NumToSpawn;

	private bool Stunned;

	public int NumToShoot = 3;

	private bool Shooting;

	public GameObject projectilePrefab;

	protected const float minBombRange = 2.5f;

	protected const float maxBombRange = 5f;

	public float timeBetweenShots = 0.5f;

	public float bombDuration = 0.75f;

	public int MortarShotsToFire = 2;

	private bool Teleporting;

	private Coroutine cTeleporting;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	public bool shorterTeleportSearch;

	public int shorterTeleportSteps = 12;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	private void Start()
	{
		SeperateObject = true;
		skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		CircleCollider = GetComponent<CircleCollider2D>();
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string text = e.Data.Name;
		if (!(text == "Teleport"))
		{
			if (text == "Fireball")
			{
				if (!string.IsNullOrEmpty(AttackVO))
				{
					AudioManager.Instance.PlayOneShot(AttackVO, base.transform.position);
				}
				Projectile component = ObjectPool.Spawn(Arrow, base.transform.parent).GetComponent<Projectile>();
				component.transform.position = base.transform.position;
				if (TargetObject != null)
				{
					state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
				}
				component.Angle = Mathf.Round(state.facingAngle / 45f) * 45f;
				component.team = health.team;
				component.Owner = health;
			}
		}
		else
		{
			Teleport();
		}
	}

	public override void OnEnable()
	{
		StartCoroutine(WaitForTarget());
		base.OnEnable();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		ClearPaths();
		StopAllCoroutines();
		if (!LoopedSoundSFX.IsNullOrEmpty())
		{
			AudioManager.Instance.StopLoop(LoopedSound);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (!LoopedSoundSFX.IsNullOrEmpty())
		{
			AudioManager.Instance.StopLoop(LoopedSound);
		}
	}

	private IEnumerator WaitForTarget()
	{
		while (TargetObject == null)
		{
			if (PlayerFarming.Instance != null)
			{
				TargetObject = PlayerFarming.Instance.gameObject;
			}
			yield return null;
		}
		if (!LoopedSoundSFX.IsNullOrEmpty())
		{
			LoopedSound = AudioManager.Instance.CreateLoop(LoopedSoundSFX, skeletonAnimation.gameObject, true);
		}
		while (Vector3.Distance(TargetObject.transform.position, base.transform.position) > Range)
		{
			yield return null;
		}
		StopAllCoroutines();
		StartCoroutine(ChasePlayer());
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		simpleSpineFlash.FlashWhite(false);
		if (AttackType == Health.AttackTypes.Projectile)
		{
			if (!Stunned)
			{
				if (!string.IsNullOrEmpty(GetHitVO))
				{
					AudioManager.Instance.PlayOneShot(GetHitVO, base.transform.position);
				}
				CameraManager.shakeCamera(0.4f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
				GameManager.GetInstance().HitStop();
				BiomeConstants.Instance.EmitHitVFX(AttackLocation + Vector3.back * 1f, Quaternion.identity.z, "HitFX_Weak");
				knockBackVX = (0f - KnockbackSpeed) * Mathf.Cos(Utils.GetAngle(base.transform.position, AttackLocation) * ((float)Math.PI / 180f));
				knockBackVY = (0f - KnockbackSpeed) * Mathf.Sin(Utils.GetAngle(base.transform.position, AttackLocation) * ((float)Math.PI / 180f));
				simpleSpineFlash.FlashFillRed();
				StopAllCoroutines();
				StartCoroutine(DoStunned());
			}
			else
			{
				CameraManager.shakeCamera(0.1f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
				GameObject obj = BiomeConstants.Instance.HitFX_Blocked.Spawn();
				obj.transform.position = AttackLocation + Vector3.back * 0.5f;
				obj.transform.rotation = Quaternion.identity;
			}
		}
		else
		{
			if (!Stunned)
			{
				bool shooting = Shooting;
			}
			CameraManager.shakeCamera(0.1f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
			BiomeConstants.Instance.EmitHitVFX(AttackLocation + Vector3.back * 1f, Quaternion.identity.z, "HitFX_Weak");
			knockBackVX = (0f - KnockbackSpeed) * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			knockBackVY = (0f - KnockbackSpeed) * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			simpleSpineFlash.FlashFillRed();
			state.facingAngle = Utils.GetAngle(base.transform.position, Attacker.transform.position);
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!LoopedSoundSFX.IsNullOrEmpty())
		{
			AudioManager.Instance.StopLoop(LoopedSound);
		}
		if (state.CURRENT_STATE != StateMachine.State.Dieing)
		{
			if (!string.IsNullOrEmpty(DeathVO))
			{
				AudioManager.Instance.PlayOneShot(DeathVO, base.transform.position);
			}
			knockBackVX = (0f - KnockbackSpeed) * 1f * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			knockBackVY = (0f - KnockbackSpeed) * 1f * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		}
	}

	private IEnumerator ChasePlayer()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		bool Loop = true;
		while (Loop)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			float num = Vector3.Distance(TargetObject.transform.position, base.transform.position);
			FleeDelay -= Time.deltaTime * skeletonAnimation.timeScale;
			TeleportDelay -= Time.deltaTime * skeletonAnimation.timeScale;
			if ((FleeDelay < 0f && num < TeleportFleeRadius) || TeleportDelay < 0f)
			{
				if (FleeDelay < 0f && num < TeleportFleeRadius)
				{
					FireBallDelay = UnityEngine.Random.Range(0f, 1f);
					SummonDelay = UnityEngine.Random.Range(0f, 1f);
				}
				FleeDelay = TeleportFleeDelayMax;
				TeleportDelay = UnityEngine.Random.Range(TeleportDelayMin, TeleportDelayMax);
				yield return StartCoroutine(DoTeleport());
			}
			if (Summon && (SummonDelay -= Time.deltaTime * skeletonAnimation.timeScale) < 0f && SummonedCount < 2)
			{
				StopAllCoroutines();
				StartCoroutine(DoSummon());
				break;
			}
			if (FireBalls && (FireBallDelay -= Time.deltaTime * skeletonAnimation.timeScale) < 0f)
			{
				StopAllCoroutines();
				FireBallDelay = UnityEngine.Random.Range(2f, 3f);
				StartCoroutine(DoThrowFireBall());
				break;
			}
			if (Mortar && (MortarDelay -= Time.deltaTime * skeletonAnimation.timeScale) < 0f)
			{
				StopAllCoroutines();
				MortarDelay = UnityEngine.Random.Range(3f, 5f);
				StartCoroutine(DoThrowMortar());
				break;
			}
			if (HealOthers && (HealDelay -= Time.deltaTime * skeletonAnimation.timeScale) < 0f && Health.team2.Count > 1)
			{
				StopAllCoroutines();
				HealDelay = UnityEngine.Random.Range(3f, 4f);
				StartCoroutine(DoHealOthers());
				break;
			}
			yield return null;
		}
	}

	private IEnumerator DoSummon()
	{
		ClearPaths();
		int SpawnCount = NumToSpawn;
		while (SpawnCount > 0)
		{
			if (!string.IsNullOrEmpty(WarningVO))
			{
				AudioManager.Instance.PlayOneShot(WarningVO, base.transform.position);
			}
			summonParticles.startDelay = 1.5f;
			summonParticles.Play();
			skeletonAnimation.AnimationState.SetAnimation(0, "summon", false);
			skeletonAnimation.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			Vector3 normalized = (TargetObject.transform.position - base.transform.position).normalized;
			Vector3 position = base.transform.position + normalized * 2f;
			if ((bool)Physics2D.Raycast(base.transform.position, normalized, 2f, layerToCheck))
			{
				position = base.transform.position + normalized * -2f;
				if ((bool)Physics2D.Raycast(base.transform.position, normalized * -1f, 2f, layerToCheck))
				{
					position = base.transform.position;
				}
			}
			Health.team2.Add(null);
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.Enemies.Add(null);
			}
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(EnemyList[UnityEngine.Random.Range(0, EnemyList.Length)], position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
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
				component.CanIncreaseDamageMultiplier = false;
				Interaction_Chest instance3 = Interaction_Chest.Instance;
				if ((object)instance3 != null)
				{
					instance3.AddEnemy(obj.Result.GetComponent<Health>());
				}
			};
			SummonedCount++;
			SummonDelay = 4f;
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * skeletonAnimation.timeScale);
				if (!(num < 1.6f))
				{
					break;
				}
				yield return null;
			}
			EnemySpawnerGO = null;
			int num2 = SpawnCount - 1;
			SpawnCount = num2;
		}
		StopAllCoroutines();
		StartCoroutine(ChasePlayer());
	}

	private void RemoveSpawned(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		SummonedCount--;
		Victim.OnDie -= RemoveSpawned;
	}

	private IEnumerator DoStunned()
	{
		Stunned = true;
		health.ArrowAttackVulnerability = 1f;
		health.MeleeAttackVulnerability = 1f;
		state.CURRENT_STATE = StateMachine.State.Attacking;
		skeletonAnimation.AnimationState.SetAnimation(0, "stunned", true);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * skeletonAnimation.timeScale);
			if (!(num < 2f))
			{
				break;
			}
			yield return null;
		}
		Stunned = false;
		health.ArrowAttackVulnerability = 1f;
		if (!HealOthers)
		{
			health.MeleeAttackVulnerability = 0.1f;
		}
		StopAllCoroutines();
		StartCoroutine(DoTeleport());
	}

	private IEnumerator DoThrowFireBall()
	{
		Shooting = true;
		int NumToShootCount = NumToShoot;
		while (NumToShootCount > 0)
		{
			state.CURRENT_STATE = StateMachine.State.Attacking;
			skeletonAnimation.AnimationState.SetAnimation(0, "projectile", false);
			skeletonAnimation.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			float Timer = 0f;
			while (true)
			{
				float num;
				Timer = (num = Timer + Time.deltaTime * skeletonAnimation.timeScale);
				if (!(num < 1.35f))
				{
					break;
				}
				simpleSpineFlash.FlashWhite(Timer / 1.35f);
				yield return null;
			}
			simpleSpineFlash.FlashWhite(false);
			int num2 = NumToShootCount - 1;
			NumToShootCount = num2;
		}
		Shooting = false;
		StopAllCoroutines();
		StartCoroutine(ChasePlayer());
	}

	private void FireMortar()
	{
		StopAllCoroutines();
		StartCoroutine(DoThrowMortar());
	}

	private IEnumerator DoThrowMortar()
	{
		Vector3 targetPosition = TargetObject.transform.position;
		for (int i = 0; i < MortarShotsToFire; i++)
		{
			if (TargetObject == null)
			{
				StartCoroutine(ChasePlayer());
				yield break;
			}
			if (MortarShotsToFire > 1)
			{
				targetPosition = TargetObject.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 2f;
			}
			MortarBomb component = UnityEngine.Object.Instantiate(projectilePrefab, TargetObject.transform.position, Quaternion.identity, base.transform.parent).GetComponent<MortarBomb>();
			if (Vector2.Distance(base.transform.position, targetPosition) < 2.5f)
			{
				component.transform.position = base.transform.position + (targetPosition - base.transform.position).normalized * 2.5f;
			}
			else
			{
				component.transform.position = base.transform.position + (targetPosition - base.transform.position).normalized * 5f;
			}
			component.Play(base.transform.position + new Vector3(0f, 0f, -1.5f), bombDuration, Health.Team.Team2);
			simpleSpineFlash.FlashWhite(false);
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * skeletonAnimation.timeScale);
				if (!(num < timeBetweenShots))
				{
					break;
				}
				yield return null;
			}
		}
		StartCoroutine(ChasePlayer());
	}

	private IEnumerator DoTeleport()
	{
		state.CURRENT_STATE = StateMachine.State.Teleporting;
		Shadow.enabled = false;
		Teleporting = true;
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * skeletonAnimation.timeScale);
			if (!(num < 0.15f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", base.gameObject);
		summonParticles.startDelay = 0f;
		summonParticles.Play();
		teleportEffect.Play();
		skeletonAnimation.AnimationState.SetAnimation(0, "teleport", false);
		skeletonAnimation.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		CircleCollider.enabled = false;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * skeletonAnimation.timeScale);
			if (!(num < 0.8f))
			{
				break;
			}
			yield return null;
		}
		StopAllCoroutines();
		StartCoroutine(ChasePlayer());
	}

	private void Teleport()
	{
		if (shorterTeleportSearch)
		{
			float num = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			float num2 = UnityEngine.Random.Range(TeleportMinDistance, TeleportMaxDistance);
			for (int i = 0; i < shorterTeleportSteps; i++)
			{
				Vector3 vector = TargetObject.transform.position + new Vector3(num2 * Mathf.Cos(num), num2 * Mathf.Sin(num));
				RaycastHit2D raycastHit2D = Physics2D.CircleCast(TargetObject.transform.position, CircleCastRadius, Vector3.Normalize(vector - TargetObject.transform.position), num2, layerToCheck);
				if (raycastHit2D.collider != null)
				{
					if (Vector3.Distance(TargetObject.transform.position, raycastHit2D.centroid) > TeleportMinDistance)
					{
						if (ShowDebug)
						{
							Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y));
							PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
						}
						base.transform.position = (Vector3)raycastHit2D.centroid + Vector3.Normalize(TargetObject.transform.position - vector) * CircleCastOffset;
						break;
					}
					num += 360f / (float)shorterTeleportSteps * ((float)Math.PI / 180f);
					continue;
				}
				if (ShowDebug)
				{
					EndPoints.Add(new Vector3(vector.x, vector.y));
					EndPointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
				}
				base.transform.position = vector;
				break;
			}
		}
		else
		{
			float num3 = 100f;
			while ((num3 -= 1f) > 0f)
			{
				float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
				float num4 = UnityEngine.Random.Range(TeleportMinDistance, TeleportMaxDistance);
				Vector3 vector2 = TargetObject.transform.position + new Vector3(num4 * Mathf.Cos(f), num4 * Mathf.Sin(f));
				RaycastHit2D raycastHit2D2 = Physics2D.CircleCast(TargetObject.transform.position, CircleCastRadius, Vector3.Normalize(vector2 - TargetObject.transform.position), num4, layerToCheck);
				if (raycastHit2D2.collider != null)
				{
					if (Vector3.Distance(TargetObject.transform.position, raycastHit2D2.centroid) > TeleportMinDistance)
					{
						if (ShowDebug)
						{
							Points.Add(new Vector3(raycastHit2D2.centroid.x, raycastHit2D2.centroid.y));
							PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
						}
						base.transform.position = (Vector3)raycastHit2D2.centroid + Vector3.Normalize(TargetObject.transform.position - vector2) * CircleCastOffset;
						break;
					}
					continue;
				}
				if (ShowDebug)
				{
					EndPoints.Add(new Vector3(vector2.x, vector2.y));
					EndPointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
				}
				base.transform.position = vector2;
				break;
			}
		}
		if (TargetObject != null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		}
		CircleCollider.enabled = true;
		Shadow.enabled = true;
		summonParticles.startDelay = 0f;
		summonParticles.Play();
		teleportEffect.Play();
		Teleporting = false;
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_appear", base.gameObject);
	}

	private Health FindTargetToHeal()
	{
		Health result = null;
		float num = float.MaxValue;
		foreach (Health item in Health.team2)
		{
			if (item != null && item.HP < item.totalHP && item.HP < num && item != health)
			{
				result = item;
				num = item.HP;
			}
		}
		return result;
	}

	private IEnumerator DoHealOthers()
	{
		Health TargetToHeal = FindTargetToHeal();
		Debug.Log(" DoHealOthers() - TargetToHeal: " + TargetToHeal);
		if (TargetToHeal == null)
		{
			Debug.Log("no target!");
			StartCoroutine(ChasePlayer());
			yield break;
		}
		ClearPaths();
		if (!string.IsNullOrEmpty(WarningVO))
		{
			AudioManager.Instance.PlayOneShot(WarningVO, base.transform.position);
		}
		summonParticles.startDelay = 1.5f;
		summonParticles.Play();
		skeletonAnimation.AnimationState.SetAnimation(0, "summon", false);
		skeletonAnimation.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * skeletonAnimation.timeScale);
			if (!(num < 1.3333334f))
			{
				break;
			}
			yield return null;
		}
		TargetToHeal = FindTargetToHeal();
		if (TargetToHeal != null)
		{
			SoulCustomTarget.Create(TargetToHeal.gameObject, base.transform.position, Color.white, delegate
			{
				if (TargetToHeal != null)
				{
					if (TargetToHeal.HP > 0f)
					{
						TargetToHeal.HP = TargetToHeal.totalHP;
						ShowHPBar component = TargetToHeal.GetComponent<ShowHPBar>();
						if ((object)component != null)
						{
							component.OnHit(TargetToHeal.gameObject, Vector3.zero, Health.AttackTypes.Melee, false);
						}
					}
					AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", TargetToHeal.gameObject.transform.position);
					BiomeConstants.Instance.EmitHeartPickUpVFX(TargetToHeal.transform.position - Vector3.forward, 0f, "red", "burst_big");
				}
			}, 0.75f);
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * skeletonAnimation.timeScale);
			if (!(num < 0.46666658f))
			{
				break;
			}
			yield return null;
		}
		StopAllCoroutines();
		StartCoroutine(ChasePlayer());
	}

	private void OnDrawGizmos()
	{
		int num = -1;
		while (++num < Points.Count)
		{
			Utils.DrawCircleXY(PointsLink[num], 0.5f, Color.blue);
			Utils.DrawCircleXY(Points[num], CircleCastRadius, Color.blue);
			Utils.DrawLine(Points[num], PointsLink[num], Color.blue);
		}
		num = -1;
		while (++num < EndPoints.Count)
		{
			Utils.DrawCircleXY(EndPointsLink[num], 0.5f, Color.red);
			Utils.DrawCircleXY(EndPoints[num], CircleCastRadius, Color.red);
			Utils.DrawLine(EndPointsLink[num], EndPoints[num], Color.red);
		}
	}
}
