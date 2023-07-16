using System;
using System.Collections;
using System.Collections.Generic;
using CotL.Projectiles;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;

public class EnemyDeathCatEye : UnitObject
{
	public SkeletonAnimation Spine;

	public SimpleSpineFlash SimpleSpineFlash;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string enterAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string exitAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string dieAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string attackAnticipateAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string attackLoopAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string attackEndAnimation;

	[SerializeField]
	private float shootAnticipation;

	[SerializeField]
	private GameObject deathParticle;

	public GameObject SplashParticle;

	public GameObject PopParticle;

	[SerializeField]
	private float projectilePatternRingsSpeed;

	[SerializeField]
	private float projectilePatternRingsAcceleration;

	[SerializeField]
	private float projectilePatternRingsRadius;

	[SerializeField]
	private ProjectileCircleBase projectilePatternRings;

	[SerializeField]
	private GameObject grenadeBullet;

	[SerializeField]
	private Vector2 grenadeNumberOfShots;

	[SerializeField]
	public float grenadeGravitySpeed;

	[SerializeField]
	private Vector2 grenadeShootDistanceRange;

	[SerializeField]
	private Vector2 grenadeDelayBetweenShots;

	[SerializeField]
	private GameObject chunkBullet;

	[SerializeField]
	private Vector2 chunkNumberOfShots;

	[SerializeField]
	public float chunkRadius;

	[SerializeField]
	public float chunkSpeed;

	[SerializeField]
	private ProjectilePatternBeam projectilePatternBeam;

	[SerializeField]
	private ProjectilePattern projectilePatternCircles;

	[SerializeField]
	private GameObject trailPrefab;

	private List<GameObject> Trails = new List<GameObject>();

	private Coroutine hidingRoutine;

	private Coroutine attackingRoutine;

	private Coroutine movementRoutine;

	private float damageRequiredToHide = 10f;

	private float currentHP;

	private float delayBetweenTrails = 0.1f;

	private float trailsTimer;

	private GameObject trail;

	private Vector3 previousSpawnPosition;

	private EventInstance loopingSoundInstance;

	private Vector2 DistanceRange = new Vector2(3f, 4f);

	private Vector2 IdleWaitRange = new Vector2(0.2f, 1.5f);

	private float RandomDirection;

	private float IdleWait;

	public bool Attacking { get; set; }

	public bool Active { get; set; }

	public bool spawnTrails { get; set; }

	public override void Awake()
	{
		base.Awake();
		GetComponent<Health>().OnHit += EnemyDeathCatEye_OnHit;
		ProjectilePatternBase.OnProjectileSpawned += ProjectilePatternBase_OnProjectileSpawned;
		InitializeProjectilePatternRings();
		InitializeGranadeBullets();
		InitializeChunkBullets();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		ProjectilePatternBase.OnProjectileSpawned -= ProjectilePatternBase_OnProjectileSpawned;
		AudioManager.Instance.StopLoop(loopingSoundInstance);
	}

	private void ProjectilePatternBase_OnProjectileSpawned()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_spawn", base.gameObject);
	}

	public void Attack(int index, int activeEyes, float delay)
	{
		StartCoroutine(AttackIE(index, activeEyes, delay));
	}

	private IEnumerator AttackIE(int index, int activeEyes, float delay)
	{
		Attacking = true;
		yield return new WaitForSeconds(delay);
		Spine.AnimationState.SetAnimation(0, attackAnticipateAnimation, false);
		Spine.AnimationState.AddAnimation(0, attackLoopAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/jellyfish_large/warning", base.gameObject);
		yield return new WaitForSeconds(1.33f);
		if (Active)
		{
			switch (index)
			{
			case 0:
				yield return attackingRoutine = StartCoroutine(ShootProjectileRingsIE(activeEyes));
				break;
			case 1:
				yield return attackingRoutine = StartCoroutine(ShootProjectileBeamIE(activeEyes));
				break;
			case 2:
				yield return attackingRoutine = StartCoroutine(ShootProjectileCircleIE(activeEyes));
				break;
			case 3:
				yield return attackingRoutine = StartCoroutine(ShootGrenadeBulletsIE(activeEyes));
				break;
			case 4:
				yield return attackingRoutine = StartCoroutine(ShootProjectileChunkIE(activeEyes));
				break;
			}
			Spine.AnimationState.SetAnimation(0, attackEndAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
			yield return new WaitForSeconds(1.33f);
			Attacking = false;
		}
	}

	private void InitializeProjectilePatternRings()
	{
		int num = 10;
		if (projectilePatternRings is ProjectileCirclePattern)
		{
			ProjectileCirclePattern projectileCirclePattern = (ProjectileCirclePattern)projectilePatternRings;
			if (projectileCirclePattern.ProjectilePrefab != null)
			{
				ObjectPool.CreatePool(projectileCirclePattern.ProjectilePrefab, projectileCirclePattern.BaseProjectilesCount * num);
			}
		}
		ObjectPool.CreatePool(projectilePatternRings, num);
	}

	public void ShootProjectileRings()
	{
		StartCoroutine(ShootProjectileRingsIE(3));
	}

	private IEnumerator ShootProjectileRingsIE(int activeEyes)
	{
		int amount = 1;
		if (activeEyes < 3)
		{
			amount = ((activeEyes == 2) ? UnityEngine.Random.Range(2, 4) : UnityEngine.Random.Range(3, 5));
		}
		for (int i = 0; i < amount; i++)
		{
			speed = 0f;
			Projectile component = ObjectPool.Spawn(projectilePatternRings, base.transform.parent).GetComponent<Projectile>();
			component.transform.position = base.transform.position;
			component.Angle = GetAngleToPlayer();
			component.health = health;
			component.team = Health.Team.Team2;
			component.Speed = projectilePatternRingsSpeed;
			component.Acceleration = projectilePatternRingsAcceleration;
			component.GetComponent<ProjectileCircleBase>().InitDelayed(PlayerFarming.Instance.gameObject, projectilePatternRingsRadius * 2f, 0f);
			AudioManager.Instance.PlayOneShot("event:/boss/frog/mortar_spawn", base.gameObject);
			yield return new WaitForSeconds(1f);
		}
	}

	public void ShootProjectileBeam()
	{
		StartCoroutine(ShootProjectileBeamIE(3));
	}

	private IEnumerator ShootProjectileBeamIE(int activeEyes)
	{
		float postDelay = 5f;
		if (activeEyes < 3)
		{
			for (int i = 0; i < projectilePatternBeam.BulletWaves.Length; i++)
			{
				projectilePatternBeam.BulletWaves[i].Bullets = ((activeEyes == 2) ? 12 : 14);
				projectilePatternBeam.BulletWaves[i].DelayBetweenBullets = ((activeEyes == 2) ? 0.35f : 0.2f);
			}
			postDelay = ((activeEyes == 2) ? 3.5f : 3f);
		}
		yield return projectilePatternBeam.ShootIE();
		yield return new WaitForSeconds(postDelay);
	}

	public void ShootProjectileCircle()
	{
		StartCoroutine(ShootProjectileCircleIE(3));
	}

	private IEnumerator ShootProjectileCircleIE(int activeEyes)
	{
		if (activeEyes < 3)
		{
			for (int i = 0; i < projectilePatternCircles.Waves.Length; i++)
			{
				projectilePatternCircles.Waves[i].Bullets = ((activeEyes == 2) ? 10 : 20);
				projectilePatternCircles.Waves[i].AngleBetweenBullets = ((activeEyes == 2) ? 36 : 18);
			}
		}
		yield return projectilePatternCircles.ShootIE();
		yield return new WaitForSeconds(1f);
	}

	private void ShootGrenadeBullets()
	{
		StartCoroutine(ShootGrenadeBulletsIE(3));
	}

	private void InitializeGranadeBullets()
	{
		ObjectPool.CreatePool(grenadeBullet, (int)grenadeNumberOfShots.y * 3);
	}

	private IEnumerator ShootGrenadeBulletsIE(int activeEyes)
	{
		int shots = (int)UnityEngine.Random.Range(grenadeNumberOfShots.x, grenadeNumberOfShots.y);
		if (activeEyes < 3)
		{
			shots *= (int)((activeEyes == 2) ? 1.5f : 2f);
		}
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= shots)
			{
				break;
			}
			float angle = UnityEngine.Random.Range(0, 360);
			ObjectPool.Spawn(grenadeBullet, base.transform.position, Quaternion.identity).GetComponent<GrenadeBullet>().Play(-1f, angle, UnityEngine.Random.Range(grenadeShootDistanceRange.x, grenadeShootDistanceRange.y), UnityEngine.Random.Range(grenadeGravitySpeed - 2f, grenadeGravitySpeed + 2f), health.team);
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.gameObject);
			yield return new WaitForSeconds(UnityEngine.Random.Range(grenadeDelayBetweenShots.x, grenadeDelayBetweenShots.y));
		}
		yield return new WaitForSeconds(1f);
	}

	private void InitializeChunkBullets()
	{
		ObjectPool.CreatePool(chunkBullet, (int)chunkNumberOfShots.y * 4);
	}

	[SerializeField]
	private void ShootProjectileChunk()
	{
		StartCoroutine(ShootProjectileChunkIE(3));
	}

	private IEnumerator ShootProjectileChunkIE(int activeEyes)
	{
		int shots = (int)UnityEngine.Random.Range(chunkNumberOfShots.x, chunkNumberOfShots.y);
		float radius = chunkRadius;
		int amount = 1;
		if (activeEyes < 3)
		{
			shots *= (int)((activeEyes == 2) ? 1.2f : 1.5f);
			radius *= ((activeEyes == 2) ? 1.2f : 1.5f);
			amount = ((activeEyes == 2) ? UnityEngine.Random.Range(1, 3) : UnityEngine.Random.Range(2, 4));
		}
		for (int t = 0; t < amount; t++)
		{
			for (int i = 0; i < shots; i++)
			{
				Vector3 vector = UnityEngine.Random.insideUnitCircle * radius;
				Projectile component = ObjectPool.Spawn(chunkBullet, base.transform.parent).GetComponent<Projectile>();
				component.transform.position = base.transform.position + vector;
				component.Angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position + vector);
				component.team = health.team;
				component.Speed = chunkSpeed;
				component.LifeTime = 4f + UnityEngine.Random.Range(0f, 0.3f);
				component.Owner = health;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/vocals/frog_large/attack", base.gameObject);
			yield return new WaitForSeconds(1f);
		}
	}

	private float GetAngleToPlayer()
	{
		return Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
	}

	public void Hide(float delay)
	{
		StopAllCoroutines();
		StartCoroutine(HideIE(delay));
	}

	private IEnumerator HideIE(float delay = 0f)
	{
		if (movementRoutine != null)
		{
			StopCoroutine(movementRoutine);
		}
		if (Active)
		{
			Active = false;
			if (attackingRoutine != null)
			{
				StopCoroutine(attackingRoutine);
			}
			GameManager.GetInstance().RemoveFromCamera(base.gameObject);
			yield return new WaitForSeconds(delay);
			speed = 0f;
			health.enabled = false;
			Attacking = false;
			projectilePatternCircles.StopAllCoroutines();
			projectilePatternBeam.StopAllCoroutines();
			if (attackingRoutine != null)
			{
				StopCoroutine(attackingRoutine);
			}
			Spine.AnimationState.SetAnimation(0, exitAnimation, false);
			AudioManager.Instance.PlayOneShot("event:/enemy/chaser_boss/chaser_boss_egg_spawn", base.gameObject);
			yield return new WaitForSeconds(1f);
			spawnTrails = true;
			Spine.gameObject.SetActive(false);
			hidingRoutine = null;
		}
	}

	public void Show()
	{
		StartCoroutine(ShowIE());
	}

	private IEnumerator ShowIE()
	{
		if (EnemyDeathCatEyesManager.Instance.Eyes.Count <= 1)
		{
			Spine.Skeleton.SetSkin("Hurt");
		}
		GameManager.GetInstance().AddToCamera(base.gameObject);
		speed = 0f;
		health.enabled = true;
		currentHP = health.HP;
		damageRequiredToHide = UnityEngine.Random.Range(20, 25);
		Active = true;
		Spine.gameObject.SetActive(true);
		Spine.AnimationState.SetAnimation(0, enterAnimation, false);
		Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser_boss/chaser_boss_egg_spawn", base.gameObject);
		yield return new WaitForSeconds(1f);
	}

	public void Reposition(Vector3 position, float delayBetween)
	{
		movementRoutine = StartCoroutine(RepositionIE(position, delayBetween));
	}

	private IEnumerator RepositionIE(Vector3 position, float delayBetween)
	{
		Vector3 startPosition = base.transform.position;
		spawnTrails = true;
		loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/fishing/caught_something_loop", base.gameObject, true);
		float t = 0f;
		while (t < delayBetween)
		{
			t += Time.deltaTime;
			float num = t / delayBetween;
			speed = 0f;
			base.transform.position = Vector3.Lerp(startPosition, position, Mathf.SmoothStep(0f, 1f, num));
			if (num > 0.925f)
			{
				spawnTrails = false;
			}
			yield return null;
		}
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		Show();
	}

	private void EnemyDeathCatEye_OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		int count = EnemyDeathCatEyesManager.Instance.Eyes.Count;
		float num = EnemyDeathCatBoss.Instance.health.HP / EnemyDeathCatBoss.Instance.health.totalHP;
		SimpleSpineFlash.FlashFillRed();
		if ((count == 3 && num <= 0.66f) || (count == 2 && num <= 0.33f) || (count == 1 && num <= 0.1f))
		{
			EnemyDeathCatEyesManager.Instance.Eyes.Remove(this);
			health.invincible = true;
			if (EnemyDeathCatEyesManager.Instance.Eyes.Count > 0)
			{
				if (UnityEngine.Random.value < 0.33f)
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 1, base.transform.position + Vector3.back);
				}
				else
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.RED_HEART, 1, base.transform.position + Vector3.back);
				}
			}
			projectilePatternCircles.StopAllCoroutines();
			projectilePatternBeam.StopAllCoroutines();
			if (attackingRoutine != null)
			{
				StopCoroutine(attackingRoutine);
			}
			EnemyDeathCatBoss.Instance.EyeDestroyed();
			StopAllCoroutines();
			GameManager.GetInstance().StartCoroutine(Die());
		}
		else if (currentHP - health.HP >= damageRequiredToHide && Attacking)
		{
			if (hidingRoutine == null)
			{
				StopAllCoroutines();
				hidingRoutine = StartCoroutine(HideIE());
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/enemy/fly_spawn", base.gameObject);
		}
		EnemyDeathCatBoss.Instance.health.invincible = false;
		EnemyDeathCatBoss.Instance.health.team = Health.Team.Neutral;
		if (AttackType == Health.AttackTypes.Melee)
		{
			EnemyDeathCatBoss.Instance.health.DealDamage(PlayerWeapon.GetDamage(1f, DataManager.Instance.CurrentWeaponLevel), Attacker, AttackLocation);
		}
		else
		{
			EnemyDeathCatBoss.Instance.health.DealDamage(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Damage * PlayerSpells.GetCurseDamageMultiplier(), Attacker, AttackLocation, false, Health.AttackTypes.Projectile);
		}
		EnemyDeathCatBoss.Instance.health.team = Health.Team.Team2;
		EnemyDeathCatBoss.Instance.health.invincible = true;
	}

	private IEnumerator Die()
	{
		if (attackingRoutine != null)
		{
			StopCoroutine(attackingRoutine);
		}
		speed = 0f;
		health.enabled = false;
		Attacking = false;
		Active = false;
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		projectilePatternCircles.StopAllCoroutines();
		projectilePatternBeam.StopAllCoroutines();
		if (attackingRoutine != null)
		{
			StopCoroutine(attackingRoutine);
		}
		deathParticle.SetActive(true);
		Spine.AnimationState.SetAnimation(0, dieAnimation, false);
		AudioManager.Instance.PlayOneShot("event:/explosion/explosion", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm_large/warning", base.gameObject);
		PlayerFarming.Instance.playerWeapon.DoSlowMo();
		yield return new WaitForSeconds(0.6f);
		Spine.gameObject.SetActive(false);
		hidingRoutine = null;
	}

	public override void Update()
	{
		IdleWait -= Time.deltaTime;
		float num = 0f;
		GetNewTargetPosition();
		if (spawnTrails)
		{
			SpawnTrails();
		}
		if (EnemyDeathCatEyesManager.Instance != null)
		{
			if (EnemyDeathCatEyesManager.Instance.Eyes.Count == 3)
			{
				maxSpeed = 0.02f;
			}
			else if (EnemyDeathCatEyesManager.Instance.Eyes.Count == 2)
			{
				maxSpeed = 0.04f;
			}
			else if (EnemyDeathCatEyesManager.Instance.Eyes.Count == 1)
			{
				maxSpeed = 0.06f;
			}
		}
		if (UsePathing)
		{
			if (pathToFollow == null)
			{
				speed += (0f - speed) / 20f * GameManager.DeltaTime;
				move();
				return;
			}
			if (currentWaypoint >= pathToFollow.Count)
			{
				speed += (0f - speed) / 20f * GameManager.DeltaTime;
				move();
				return;
			}
		}
		if (state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.Fleeing)
		{
			speed += (maxSpeed * SpeedMultiplier - speed) / 35f * GameManager.DeltaTime;
			if (UsePathing)
			{
				state.facingAngle = Mathf.LerpAngle(state.facingAngle, Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]), Time.deltaTime * 2f);
				if (Vector2.Distance(base.transform.position, pathToFollow[currentWaypoint]) <= StoppingDistance)
				{
					currentWaypoint++;
					if (currentWaypoint == pathToFollow.Count)
					{
						state.CURRENT_STATE = StateMachine.State.Idle;
						System.Action endOfPath = EndOfPath;
						if (endOfPath != null)
						{
							endOfPath();
						}
						pathToFollow = null;
					}
				}
			}
		}
		else
		{
			speed += (0f - speed) / 20f * GameManager.DeltaTime;
		}
		move();
	}

	public void GetNewTargetPosition()
	{
		if (AstarPath.active == null)
		{
			return;
		}
		float num = 100f;
		while ((num -= 1f) > 0f)
		{
			float num2 = UnityEngine.Random.Range(DistanceRange.x, DistanceRange.y);
			RandomDirection += (float)UnityEngine.Random.Range(-45, 45) * ((float)Math.PI / 180f);
			float radius = 0.1f;
			Vector3 vector = base.transform.position + new Vector3(num2 * Mathf.Cos(RandomDirection), num2 * Mathf.Sin(RandomDirection));
			if (Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num2 * 0.5f, layerToCheck).collider != null)
			{
				RandomDirection += 0.17453292f;
				continue;
			}
			IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
			givePath(vector);
			break;
		}
	}

	public void SpawnTrails()
	{
		if (!((trailsTimer += Time.deltaTime) > delayBetweenTrails) || !(Vector3.Distance(base.transform.position, previousSpawnPosition) > 0.1f))
		{
			return;
		}
		trailsTimer = 0f;
		trail = null;
		if (Trails.Count > 0)
		{
			foreach (GameObject trail in Trails)
			{
				if (!trail.activeSelf)
				{
					this.trail = trail;
					this.trail.transform.position = base.transform.position;
					this.trail.SetActive(true);
					break;
				}
			}
		}
		if (this.trail == null)
		{
			this.trail = UnityEngine.Object.Instantiate(trailPrefab, base.transform.position, Quaternion.identity, base.transform.parent);
			Trails.Add(this.trail);
		}
		previousSpawnPosition = this.trail.transform.position;
	}
}
