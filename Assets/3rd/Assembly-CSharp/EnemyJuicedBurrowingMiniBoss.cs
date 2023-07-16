using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyJuicedBurrowingMiniBoss : UnitObject
{
	public SkeletonAnimation Spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string idleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootAttackAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootAttackAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string spawnEnemiesAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string shootSpikesAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string appearAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string disapearAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string hiddenAnimation;

	[Space]
	[SerializeField]
	private Vector2 timeBetweenAttacks;

	[SerializeField]
	private JuicedTrail trailPrefab;

	[SerializeField]
	private float spikesAnticipation;

	[SerializeField]
	private float spikesPost;

	[SerializeField]
	private GameObject grenadeBullet;

	[SerializeField]
	private GameObject shootPosition;

	[SerializeField]
	private Vector2 amountOfShots;

	[SerializeField]
	private Vector2 delayBetweenShots;

	[SerializeField]
	private Vector2 shootDistance;

	[SerializeField]
	private float gravSpeed;

	[SerializeField]
	private Vector2 amountOfRounds;

	[SerializeField]
	private float targetedShootingAnticipation;

	[SerializeField]
	private float targetedShootingPost;

	[SerializeField]
	private Vector2 spawnAmount;

	[SerializeField]
	private int maxEnemies;

	[SerializeField]
	private float spawnEnemiesAnticipation;

	[SerializeField]
	private float spawnEnemiesPost;

	[SerializeField]
	private Vector2 delayBetweenSpawns;

	[SerializeField]
	private float spawnForce;

	[SerializeField]
	private Vector2 timeBetweenSpawns;

	[SerializeField]
	private AssetReferenceGameObject[] spawnables;

	[Header("Weights")]
	[SerializeField]
	private float targetedSpikesWeight;

	[SerializeField]
	private float targetedShootWeight;

	[SerializeField]
	private float patternSpikesWeight;

	[SerializeField]
	private float spawnEnemiesWeight;

	[SerializeField]
	private float circleSpikesWeight;

	[Space]
	[SerializeField]
	private GameObject lighting;

	private List<JuicedTrail> spawnedSpikes = new List<JuicedTrail>();

	private Coroutine currentAttack;

	private EventInstance loopingSoundInstance;

	private ShowHPBar showHPBar;

	private float idleTime;

	private float spawnTime;

	private float initialDelay;

	private Vector3 targetPosition;

	private Coroutine currentRoutine;

	private bool phase2;

	private GameObject t;

	private Vector3 previousSpawnPosition;

	private float TrailsTimer;

	private float DelayBetweenTrails = 0.1f;

	private List<GameObject> Trails = new List<GameObject>();

	[Range(0f, 1f)]
	public float ChanceToPathTowardsPlayer = 0.8f;

	public float TurningArc = 90f;

	public Vector2 DistanceRange = new Vector2(1f, 3f);

	private bool PathingToPlayer;

	private float RandomDirection;

	public float DistanceToPathTowardsPlayer = 6f;

	public float MinimumPlayerDistance = 3f;

	public float MoveSpeed = 5f;

	public float MoveDelay;

	public Vector2 MaxMoveDistance;

	private void Start()
	{
		initialDelay = UnityEngine.Random.Range(0f, 2f);
		showHPBar = GetComponent<ShowHPBar>();
	}

	public override void Update()
	{
		base.Update();
		if (phase2 && currentRoutine == null)
		{
			base.transform.position = Vector3.zero;
		}
		idleTime -= Time.deltaTime * Spine.timeScale;
		spawnTime -= Time.deltaTime * Spine.timeScale;
		if (currentAttack != null || currentRoutine != null || !(GetClosestTarget() != null) || !(idleTime <= 0f) || !phase2)
		{
			return;
		}
		if (UnityEngine.Random.value <= targetedSpikesWeight && !phase2)
		{
			ShootTripleSoftTargeted();
		}
		else if (UnityEngine.Random.value <= targetedShootWeight)
		{
			TargetedShootProjectiles();
		}
		else if (Health.team2.Count - 1 < maxEnemies && spawnTime <= 0f && (Health.team2.Count - 1 <= 2 || UnityEngine.Random.value <= spawnEnemiesWeight))
		{
			SpawnEnemies();
		}
		else if (UnityEngine.Random.value <= patternSpikesWeight && !phase2)
		{
			if (UnityEngine.Random.value > 0.5f)
			{
				ShootCrossAngled();
			}
			else
			{
				ShootCross();
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		currentAttack = null;
		if (currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
		}
		currentRoutine = null;
		if (!phase2)
		{
			currentRoutine = StartCoroutine(ActiveRoutine());
		}
	}

	private void ShootCross()
	{
		currentAttack = StartCoroutine(ShootCrossIE(4, 0f, false));
	}

	private void ShootCrossAngled()
	{
		currentAttack = StartCoroutine(ShootCrossIE(4, 45f, false));
	}

	private void ShootCrossAngledContinous()
	{
		currentAttack = StartCoroutine(ShootCrossIE(4, 45f, true));
	}

	private void ShootTripleSoftTargeted()
	{
		currentAttack = StartCoroutine(ShootTripleSoftTargetedIE());
	}

	private IEnumerator ShootTripleSoftTargetedIE()
	{
		float angle = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position);
		angle = Mathf.Round(angle / 90f) * 90f;
		Vector3 dir = Utils.DegreeToVector2(angle);
		Spine.AnimationState.SetAnimation(0, shootSpikesAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spikesAnticipation))
			{
				break;
			}
			yield return null;
		}
		StartCoroutine(ShootSpikesInDirectionIE(dir, 15, 0.75f, false));
		Vector3 direction = Quaternion.Euler(0f, 0f, -25f) * dir;
		Vector3 direction2 = Quaternion.Euler(0f, 0f, 25f) * dir;
		StartCoroutine(ShootSpikesInDirectionIE(direction, 15, 0.75f, false));
		StartCoroutine(ShootSpikesInDirectionIE(direction2, 15, 0.75f, false));
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 2f))
			{
				break;
			}
			yield return null;
		}
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private IEnumerator ShootCrossIE(int directions, float directionOffset, bool continous)
	{
		Spine.AnimationState.SetAnimation(0, shootSpikesAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spikesAnticipation))
			{
				break;
			}
			yield return null;
		}
		List<KeyValuePair<GameObject, float>> spikes = new List<KeyValuePair<GameObject, float>>();
		int count = 0;
		float num2 = 0f;
		float num3 = 360f / (float)directions;
		for (int i = 0; i < directions; i++)
		{
			Vector3 direction = Utils.DegreeToVector2(num2 + directionOffset);
			StartCoroutine(ShootSpikesInDirectionIE(direction, 15, 0.5f, continous, delegate(List<KeyValuePair<GameObject, float>> s)
			{
				spikes.AddRange(s);
				count++;
			}));
			num2 += num3;
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spikesPost))
			{
				break;
			}
			yield return null;
		}
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private IEnumerator ShootSpikesInDirectionIE(Vector3 direction, int distance, float spacing, bool continous, Action<List<KeyValuePair<GameObject, float>>> callback = null)
	{
		List<KeyValuePair<GameObject, float>> spikes = new List<KeyValuePair<GameObject, float>>();
		Vector3 position = base.transform.position;
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_burst_out_of_ground", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_screech", base.transform.position);
		for (int i = 0; i < distance; i++)
		{
			GameObject spawnSpike = GetSpawnSpike(continous);
			spawnSpike.transform.position = position;
			position += direction * spacing;
			spikes.Add(new KeyValuePair<GameObject, float>(spawnSpike, Utils.GetAngle(base.transform.position, spawnSpike.transform.position)));
			float time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < 0.1f))
				{
					break;
				}
				yield return null;
			}
		}
		if (callback != null)
		{
			callback(spikes);
		}
	}

	public void TargetedShootProjectiles()
	{
		currentAttack = StartCoroutine(TargetedShootProjectilesIE());
	}

	private IEnumerator TargetedShootProjectilesIE()
	{
		int randomAmount = (int)UnityEngine.Random.Range(amountOfRounds.x, amountOfRounds.y + 1f);
		float time;
		for (int i = 0; i < randomAmount; i++)
		{
			Spine.AnimationState.SetAnimation(0, shootAttackAnticipationAnimation, true);
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < targetedShootingAnticipation))
				{
					break;
				}
				yield return null;
			}
			Spine.AnimationState.SetAnimation(0, shootAttackAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			yield return StartCoroutine(ShootProjectilesIE(true, null));
		}
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < targetedShootingPost))
			{
				break;
			}
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, idleAnimation, true);
		currentAttack = null;
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	public void ShootProjectiles()
	{
		currentAttack = StartCoroutine(ShootProjectilesIE(false, null));
	}

	private IEnumerator ShootProjectilesIE(bool shootAtTarget, System.Action callback)
	{
		int shotsToFire = (int)UnityEngine.Random.Range(amountOfShots.x, amountOfShots.y + 1f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= shotsToFire)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.transform.position);
			Vector3 position = shootPosition.transform.position;
			position.z = 0f;
			float num2 = UnityEngine.Random.Range(shootDistance.x, shootDistance.y);
			float angle = UnityEngine.Random.Range(0f, 360f);
			if (shootAtTarget && GetClosestTarget() != null)
			{
				num2 = Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) / 1.5f + UnityEngine.Random.Range(-2f, 2f);
				angle = Mathf.Repeat(Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) + UnityEngine.Random.Range(-20f, 20f), 360f);
			}
			ObjectPool.Spawn(grenadeBullet, position).GetComponent<GrenadeBullet>().Play(-3f, angle, num2, gravSpeed);
			float dur = UnityEngine.Random.Range(delayBetweenShots.x, delayBetweenShots.y);
			float time = 0f;
			while (true)
			{
				float num3;
				time = (num3 = time + Time.deltaTime * Spine.timeScale);
				if (!(num3 < dur))
				{
					break;
				}
				yield return null;
			}
		}
		if (callback != null)
		{
			callback();
		}
	}

	public void SpawnEnemies()
	{
		currentAttack = StartCoroutine(SpawnEnemiesIE());
	}

	private IEnumerator SpawnEnemiesIE()
	{
		Spine.AnimationState.SetAnimation(0, spawnEnemiesAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnEnemiesAnticipation))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_screech", base.transform.position);
		int amount = (int)UnityEngine.Random.Range(spawnAmount.x, spawnAmount.y + 1f);
		for (int i = 0; i < amount; i++)
		{
			Vector3 position = base.transform.position;
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnables[UnityEngine.Random.Range(0, spawnables.Length)], position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				UnitObject component = obj.Result.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				component.RemoveModifier();
				component.DoKnockBack(UnityEngine.Random.Range(0f, 360f), spawnForce, 0.75f);
			};
			float dur = UnityEngine.Random.Range(delayBetweenSpawns.x, delayBetweenSpawns.y);
			time2 = 0f;
			while (true)
			{
				float num;
				time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num < dur))
				{
					break;
				}
				yield return null;
			}
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnEnemiesPost))
			{
				break;
			}
			yield return null;
		}
		currentAttack = null;
		spawnTime = UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
		idleTime = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);
	}

	private GameObject GetSpawnSpike(bool continous)
	{
		JuicedTrail juicedTrail = null;
		if (spawnedSpikes.Count > 0)
		{
			foreach (JuicedTrail spawnedSpike in spawnedSpikes)
			{
				if (!spawnedSpike.gameObject.activeSelf)
				{
					juicedTrail = spawnedSpike;
					juicedTrail.transform.position = base.transform.position;
					juicedTrail.gameObject.SetActive(true);
					break;
				}
			}
		}
		if (juicedTrail == null)
		{
			juicedTrail = UnityEngine.Object.Instantiate(trailPrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			juicedTrail.transform.localScale = Vector3.one * 0.85f;
			spawnedSpikes.Add(juicedTrail);
			juicedTrail.ColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		}
		juicedTrail.SetContinious(continous);
		return juicedTrail.gameObject;
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashFillRed();
	}

	protected virtual void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && (component.team != health.team || health.team == Health.Team.PlayerTeam))
		{
			component.DealDamage(1f, component.gameObject, component.transform.position);
		}
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != null && Health.team2[num] != health)
			{
				Health.team2[num].enabled = true;
				Health.team2[num].invincible = false;
				Health.team2[num].untouchable = false;
				Health.team2[num].DealDamage(Health.team2[num].totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
		for (int num2 = spawnedSpikes.Count - 1; num2 >= 0; num2--)
		{
			if (spawnedSpikes[num2] != null)
			{
				UnityEngine.Object.Destroy(spawnedSpikes[num2].gameObject);
			}
		}
	}

	private IEnumerator MoveRoutine()
	{
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < initialDelay))
			{
				break;
			}
			yield return null;
		}
		initialDelay = 0f;
		yield return new WaitForEndOfFrame();
		Spine.AnimationState.SetAnimation(0, disapearAnimation, false);
		Spine.AnimationState.AddAnimation(0, hiddenAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_disappear_underground", base.gameObject);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.25f))
			{
				break;
			}
			yield return null;
		}
		health.invincible = true;
		previousSpawnPosition = Vector3.positiveInfinity;
		float Progress = 0f;
		float Duration2 = 11f / 30f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration2))
			{
				break;
			}
			SpawnTrails();
			yield return null;
		}
		if ((bool)lighting)
		{
			lighting.SetActive(false);
		}
		ShowHPBar obj = showHPBar;
		if ((object)obj != null)
		{
			obj.Hide();
		}
		state.CURRENT_STATE = StateMachine.State.Fleeing;
		if (!loopingSoundInstance.isValid())
		{
			loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/enemy/tunnel_worm/tunnel_worm_underground_loop", base.gameObject, true);
		}
		Vector3 StartPosition = base.transform.position;
		StartPosition.z = 0f;
		targetPosition.z = 0f;
		Progress = 0f;
		Duration2 = Vector3.Distance(StartPosition, targetPosition) / MoveSpeed;
		previousSpawnPosition = Vector3.positiveInfinity;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration2))
			{
				break;
			}
			base.transform.position = Vector3.Lerp(StartPosition, targetPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration2));
			SpawnTrails();
			yield return null;
		}
		base.transform.position = targetPosition;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		health.invincible = false;
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		AudioManager.Instance.PlayOneShot("event:/enemy/tunnel_worm/tunnel_worm_burst_out_of_ground", base.gameObject);
		Spine.AnimationState.SetAnimation(0, appearAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.55f))
			{
				break;
			}
			yield return null;
		}
		if ((bool)lighting)
		{
			lighting.SetActive(true);
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		currentAttack = null;
		while (currentAttack == null)
		{
			if (phase2)
			{
				ShootCrossAngledContinous();
			}
			else if (UnityEngine.Random.value <= targetedSpikesWeight)
			{
				ShootTripleSoftTargeted();
			}
			else if (UnityEngine.Random.value <= targetedShootWeight)
			{
				TargetedShootProjectiles();
			}
			else if (Health.team2.Count - 1 < maxEnemies && spawnTime <= 0f && (Health.team2.Count - 1 <= 2 || UnityEngine.Random.value <= spawnEnemiesWeight))
			{
				SpawnEnemies();
			}
			else if (UnityEngine.Random.value <= circleSpikesWeight)
			{
				ShootCrossAngled();
			}
			else if (UnityEngine.Random.value <= patternSpikesWeight)
			{
				if (UnityEngine.Random.value > 0.5f)
				{
					ShootCrossAngled();
				}
				else
				{
					ShootCross();
				}
			}
		}
		if (currentAttack != null)
		{
			yield return currentAttack;
		}
		if (!phase2)
		{
			if (health.HP < health.totalHP / 2f)
			{
				phase2 = true;
				if (currentRoutine != null)
				{
					StopCoroutine(currentRoutine);
				}
				targetPosition = Vector3.zero;
				currentRoutine = StartCoroutine(MoveRoutine());
			}
			else if (GetNewTargetPosition())
			{
				if (currentRoutine != null)
				{
					StopCoroutine(currentRoutine);
				}
				currentRoutine = StartCoroutine(MoveRoutine());
			}
			else
			{
				if (currentRoutine != null)
				{
					StopCoroutine(currentRoutine);
				}
				currentRoutine = StartCoroutine(ActiveRoutine());
			}
		}
		else
		{
			currentRoutine = null;
		}
	}

	public void SpawnTrails()
	{
		if ((TrailsTimer += Time.deltaTime) > DelayBetweenTrails && Vector3.Distance(base.transform.position, previousSpawnPosition) > 0.1f)
		{
			TrailsTimer = 0f;
			t = GetSpawnSpike(false);
			t.transform.position = base.transform.position;
			previousSpawnPosition = t.transform.position;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		foreach (GameObject trail in Trails)
		{
			if (trail != null)
			{
				ColliderEvents componentInChildren = trail.GetComponentInChildren<ColliderEvents>();
				if ((bool)componentInChildren)
				{
					componentInChildren.OnTriggerEnterEvent -= OnDamageTriggerEnter;
				}
			}
		}
		AudioManager.Instance.StopLoop(loopingSoundInstance);
	}

	private bool GetNewTargetPosition()
	{
		float num = 100f;
		if (GetClosestTarget() != null && ChanceToPathTowardsPlayer > 0f && UnityEngine.Random.value < ChanceToPathTowardsPlayer && Vector3.Distance(base.transform.position, GetClosestTarget().transform.position) < DistanceToPathTowardsPlayer)
		{
			PathingToPlayer = true;
			RandomDirection = Utils.GetAngle(base.transform.position, GetClosestTarget().transform.position) * ((float)Math.PI / 180f);
		}
		if (ChanceToPathTowardsPlayer >= 1f && GameManager.RoomActive && GetClosestTarget() != null)
		{
			float num2 = Vector3.Distance(base.transform.position, GetClosestTarget().transform.position);
			if (num2 > MinimumPlayerDistance && num2 < DistanceToPathTowardsPlayer)
			{
				Vector3 normalized = (GetClosestTarget().transform.position - base.transform.position).normalized;
				float num3 = MaxMoveDistance.y;
				RaycastHit2D raycastHit2D;
				RaycastHit2D raycastHit2D2 = (raycastHit2D = Physics2D.Raycast(base.transform.position, normalized, 100f, layerToCheck));
				if (raycastHit2D2.collider != null)
				{
					num3 = Mathf.Min(num3, Vector3.Distance(base.transform.position, raycastHit2D.point));
				}
				targetPosition = base.transform.position + normalized * UnityEngine.Random.Range(MaxMoveDistance.x, num3);
				return true;
			}
			PathingToPlayer = false;
		}
		while ((num -= 1f) > 0f)
		{
			float num4 = UnityEngine.Random.Range(DistanceRange.x, DistanceRange.y);
			if (!PathingToPlayer)
			{
				RandomDirection += UnityEngine.Random.Range(0f - TurningArc, TurningArc) * ((float)Math.PI / 180f);
			}
			PathingToPlayer = false;
			float radius = 0.2f;
			Vector3 vector = base.transform.position + new Vector3(num4 * Mathf.Cos(RandomDirection), num4 * Mathf.Sin(RandomDirection));
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num4, layerToCheck).collider != null)
			{
				RandomDirection = 180f - RandomDirection;
				continue;
			}
			targetPosition = vector;
			return true;
		}
		return false;
	}

	private IEnumerator ActiveRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		while (state.CURRENT_STATE != 0 || !(idleTime <= 0f) || !GetNewTargetPosition())
		{
			yield return null;
		}
		if (currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
		}
		currentRoutine = StartCoroutine(MoveRoutine());
	}
}
