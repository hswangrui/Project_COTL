using System;
using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemySpiderJumpMiniboss : EnemySpider
{
	[Serializable]
	private struct SpawnSet
	{
		public AssetReferenceGameObject[] EnemiesList;

		public Vector2 Amount;
	}

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string swinAwayAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string swinInAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string StuckAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string UnstuckAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string shootAnticipationAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string shootAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string jumpAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string landAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string jumpQuickAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	[SerializeField]
	private string jumpAnticipateAnimation;

	[Space]
	[SerializeField]
	private int attacksBeforeSlam;

	[SerializeField]
	private float slamAttackMinRange;

	[SerializeField]
	private float timeBetweenSlams;

	[SerializeField]
	private float slamInAirDuration;

	[SerializeField]
	private float slamLandDuration;

	[SerializeField]
	private float slamCooldown;

	[SerializeField]
	private float randomOffsetRadius = 0.25f;

	[SerializeField]
	private float screenShake;

	[SerializeField]
	private ProjectilePatternBase slamProjectilePattern;

	[SerializeField]
	private ProjectilePatternBase projectilePattern;

	[SerializeField]
	private float projectileAnticipation;

	[SerializeField]
	private Vector2 timeBetweenProjectileAttacks;

	[SerializeField]
	private bool canTargetJump;

	[SerializeField]
	private float jumpMaxDistance;

	[SerializeField]
	private float jumpDuration;

	[SerializeField]
	private Vector2 targetJumpAmount;

	[SerializeField]
	private float delayBetweenJumps;

	[SerializeField]
	private SpawnSet[] spawnSets;

	[SerializeField]
	private float spawnForce;

	[SerializeField]
	private GameObject slamParticlePrefab;

	[SerializeField]
	private SpriteRenderer indicatorIcon;

	private Color indicatorColor = Color.white;

	private float SlamTimer;

	private int attackCounter;

	private float projectileTimer;

	private int UpdateEveryFrameNum = 5;

	private int curFrame;

	public override void OnEnable()
	{
		SlamTimer = timeBetweenSlams + UnityEngine.Random.Range(0f, 3f);
		indicatorIcon.gameObject.SetActive(false);
		projectileTimer = UnityEngine.Random.Range(timeBetweenProjectileAttacks.x, timeBetweenProjectileAttacks.y);
		Spine.transform.localPosition = Vector3.zero;
		base.OnEnable();
	}

	public override void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		base.AttackingTargetPosition = base.transform.position - Vector3.up;
		base.Update();
		if (indicatorIcon.gameObject.activeSelf)
		{
			if (curFrame == UpdateEveryFrameNum)
			{
				indicatorColor = ((indicatorColor == Color.white) ? Color.red : Color.white);
				indicatorIcon.material.SetColor("_Color", indicatorColor);
				curFrame = 0;
			}
			else
			{
				curFrame++;
			}
		}
	}

	protected override IEnumerator ActiveRoutine()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (!GameManager.RoomActive)
			{
				yield return null;
				continue;
			}
			if (state.CURRENT_STATE == StateMachine.State.Idle && (IdleWait -= Time.deltaTime * Spine.timeScale) <= 0f && GameManager.GetInstance().CurrentTime > initialMovementDelay)
			{
				if (wander && attackCounter < attacksBeforeSlam)
				{
					GetNewTargetPosition();
				}
				else
				{
					Flee();
				}
				speed = maxSpeed;
			}
			if (PlayerFarming.Instance != null && !base.Attacking && !IsStunned)
			{
				state.LookAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
			}
			else
			{
				state.LookAngle = state.facingAngle;
			}
			if (MovingAnimation != "" && GameManager.GetInstance().CurrentTime > initialMovementDelay)
			{
				if (state.CURRENT_STATE == StateMachine.State.Moving && Spine.AnimationName != MovingAnimation)
				{
					SetAnimation(MovingAnimation, true);
				}
				if (state.CURRENT_STATE == StateMachine.State.Idle && Spine.AnimationName != IdleAnimation)
				{
					SetAnimation(IdleAnimation, true);
				}
			}
			int num = UnityEngine.Random.Range(0, 10);
			if (ShouldProjectileAttack() && num < 2)
			{
				AudioManager.Instance.PlayOneShot(warningSfx, base.gameObject);
				StartCoroutine(ProjectileAttack());
			}
			else if (ShouldAttack() && num < 5)
			{
				AudioManager.Instance.PlayOneShot(warningSfx, base.gameObject);
				attackCounter = 1;
				StartCoroutine(AttackRoutine());
			}
			else if (ShouldSlam() && num < 7)
			{
				AudioManager.Instance.PlayOneShot(warningSfx, base.gameObject);
				attackCounter = 0;
				StartCoroutine(SlamRoutine());
			}
			else if (ShouldTargetJump())
			{
				AudioManager.Instance.PlayOneShot(warningSfx, base.gameObject);
				attackCounter = 0;
				StartCoroutine(TargetJumpRoutine());
			}
			yield return null;
		}
	}

	protected override bool ShouldAttack()
	{
		return base.ShouldAttack();
	}

	private bool ShouldTargetJump()
	{
		if (canTargetJump && (SlamTimer -= Time.deltaTime * Spine.timeScale) < 0f && !base.Attacking)
		{
			return GameManager.GetInstance().CurrentTime > initialAttackDelayTimer;
		}
		return false;
	}

	private bool ShouldSlam()
	{
		if ((SlamTimer -= Time.deltaTime * Spine.timeScale) < 0f && !base.Attacking && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > slamAttackMinRange)
		{
			return GameManager.GetInstance().CurrentTime > initialAttackDelayTimer;
		}
		return false;
	}

	private bool ShouldProjectileAttack()
	{
		if ((projectileTimer -= Time.deltaTime * Spine.timeScale) < 0f && !base.Attacking)
		{
			return GameManager.GetInstance().CurrentTime > initialAttackDelayTimer;
		}
		return false;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		if (!GetComponentInParent<MiniBossController>())
		{
			return;
		}
		for (int num = EnemySpider.EnemySpiders.Count - 1; num >= 0; num--)
		{
			if ((bool)EnemySpider.EnemySpiders[num] && EnemySpider.EnemySpiders[num] != this)
			{
				SpawnEnemyOnDeath component = EnemySpider.EnemySpiders[num].GetComponent<SpawnEnemyOnDeath>();
				if ((bool)component)
				{
					component.Amount = 0;
				}
				EnemySpider.EnemySpiders[num].health.enabled = true;
				EnemySpider.EnemySpiders[num].health.DealDamage(EnemySpider.EnemySpiders[num].health.totalHP, base.gameObject, EnemySpider.EnemySpiders[num].transform.position);
			}
		}
	}

	private void Slam()
	{
		StartCoroutine(SlamRoutine());
	}

	private IEnumerator SlamRoutine()
	{
		Spine.ForceVisible = true;
		base.Attacking = true;
		updateDirection = false;
		ClearPaths();
		state.CURRENT_STATE = StateMachine.State.Attacking;
		SetAnimation(swinAwayAnimation);
		Vector3 targetShadowScale = ShadowSpriteRenderer.transform.localScale;
		ShadowSpriteRenderer.transform.DOScale(0f, 1f);
		AudioManager.Instance.PlayOneShot(warningSfx, base.transform.position);
		yield return new WaitForEndOfFrame();
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		if (EnemySpider.EnemySpiders.Count < 3)
		{
			SpawnEnemies();
		}
		health.enabled = false;
		AudioManager.Instance.PlayOneShot(jumpSfx, base.transform.position);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < (slamInAirDuration - 1f) / 2f))
			{
				break;
			}
			yield return null;
		}
		base.transform.position = PlayerFarming.Instance.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * randomOffsetRadius;
		base.transform.position = new Vector3(Mathf.Clamp(base.transform.position.x, -6.5f, 6.5f), Mathf.Clamp(base.transform.position.y, -4f, 4f), base.transform.position.z);
		indicatorIcon.gameObject.SetActive(true);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < (slamInAirDuration - 1f) / 2f))
			{
				break;
			}
			yield return null;
		}
		ShadowSpriteRenderer.transform.DOScale(targetShadowScale, slamLandDuration);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < slamLandDuration * 0.5f))
			{
				break;
			}
			yield return null;
		}
		SetAnimation(swinInAnimation);
		AddAnimation(StuckAnimation);
		AddAnimation(UnstuckAnimation);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < slamLandDuration * 0.5f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(stuckSfx, base.transform.position);
		CameraManager.instance.ShakeCameraForDuration(screenShake, screenShake, 0.1f);
		UnityEngine.Object.Instantiate(slamParticlePrefab, base.transform.position, Quaternion.identity);
		damageColliderEvents.gameObject.SetActive(true);
		indicatorIcon.gameObject.SetActive(false);
		StartCoroutine(slamProjectilePattern.ShootIE());
		health.enabled = true;
		health.DontCombo = true;
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.1f))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.gameObject.SetActive(false);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num < slamCooldown - 0.5f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot(breakFreeSfx, base.transform.position);
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
		health.DontCombo = false;
		IdleWait = 0f;
		SlamTimer = timeBetweenSlams;
		base.Attacking = false;
		updateDirection = true;
		state.CURRENT_STATE = StateMachine.State.Idle;
		Spine.ForceVisible = false;
	}

	private IEnumerator TargetJumpRoutine()
	{
		base.Attacking = true;
		ClearPaths();
		AnimationCurve curve = new AnimationCurve();
		curve.AddKey(0f, 0f);
		curve.AddKey(0.5f, 1f);
		curve.AddKey(1f, 0f);
		state.CURRENT_STATE = StateMachine.State.Attacking;
		int jumpAmount = (int)UnityEngine.Random.Range(targetJumpAmount.x, targetJumpAmount.y + 1f);
		for (int i = 0; i < jumpAmount; i++)
		{
			SetAnimation(jumpAnticipateAnimation);
			yield return new WaitForSeconds(0.2f);
			AudioManager.Instance.PlayOneShot("event:/enemy/jump_large", base.transform.position);
			SetAnimation(jumpQuickAnimation);
			LookAtTarget();
			Vector3 vector = Vector3.ClampMagnitude(GetClosestTarget().transform.position - base.transform.position, jumpMaxDistance);
			base.transform.DOMove(base.transform.position + vector, jumpDuration).SetEase(Ease.Linear);
			float t = 0f;
			float dur = jumpDuration - 0.2f;
			while (t < dur)
			{
				t += Time.deltaTime * Spine.timeScale;
				Spine.transform.localPosition = new Vector3(Spine.transform.localPosition.x, Spine.transform.localPosition.y, curve.Evaluate(t / dur) * -2f);
				yield return null;
			}
			Spine.transform.localPosition = Vector3.zero;
			SetAnimation(landAnimation);
			AddAnimation(IdleAnimation, true);
			float time3 = 0f;
			while (true)
			{
				float num;
				time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
				if (!(num < 0.1f))
				{
					break;
				}
				yield return null;
			}
			CameraManager.instance.ShakeCameraForDuration(screenShake, screenShake, 0.1f);
			damageColliderEvents.gameObject.SetActive(true);
			AudioManager.Instance.PlayOneShot("event:/enemy/land_large", base.transform.position);
			UnityEngine.Object.Instantiate(slamParticlePrefab, base.transform.position, Quaternion.identity);
			time3 = 0f;
			while (true)
			{
				float num;
				time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
				if (!(num < 0.1f))
				{
					break;
				}
				yield return null;
			}
			damageColliderEvents.gameObject.SetActive(false);
			time3 = 0f;
			while (true)
			{
				float num;
				time3 = (num = time3 + Time.deltaTime * Spine.timeScale);
				if (!(num < delayBetweenJumps - 0.1f))
				{
					break;
				}
				yield return null;
			}
		}
		IdleWait = 0f;
		SlamTimer = timeBetweenSlams;
		base.Attacking = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private void SpawnEnemies()
	{
		SpawnSet spawnSet = spawnSets[UnityEngine.Random.Range(0, spawnSets.Length)];
		float randomStartAngle = UnityEngine.Random.Range(0, 360);
		int randomAmount = (int)UnityEngine.Random.Range(spawnSet.Amount.x, spawnSet.Amount.y + 1f);
		for (int i = 0; i < randomAmount; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnSet.EnemiesList[UnityEngine.Random.Range(0, spawnSet.EnemiesList.Length)], base.transform.position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				UnitObject component = obj.Result.GetComponent<UnitObject>();
				Interaction_Chest instance = Interaction_Chest.Instance;
				if ((object)instance != null)
				{
					instance.AddEnemy(component.health);
				}
				DropLootOnDeath component2 = component.GetComponent<DropLootOnDeath>();
				if ((bool)component2)
				{
					component2.GiveXP = false;
				}
				SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>();
				foreach (SkeletonAnimation obj2 in componentsInChildren)
				{
					obj2.AnimationState.SetAnimation(0, "spawn", false);
					obj2.AnimationState.AddAnimation(0, "idle", true, 0f);
				}
				component.DoKnockBack(randomStartAngle, spawnForce, 0.5f);
				randomStartAngle = Mathf.Repeat(randomStartAngle + (float)(360 / randomAmount), 360f);
				component.StartCoroutine(DelayedEnemyHealthEnable(component));
			};
		}
	}

	private IEnumerator ProjectileAttack()
	{
		base.Attacking = true;
		state.CURRENT_STATE = StateMachine.State.Attacking;
		SetAnimation(shootAnticipationAnimation);
		AddAnimation(shootAnimation);
		AddAnimation(IdleAnimation, true);
		SimpleSpineFlash.FlashWhite(false);
		float t = 0f;
		while (t < projectileAnticipation)
		{
			t += Time.deltaTime * Spine.timeScale;
			SimpleSpineFlash.FlashWhite(t / projectileAnticipation * 0.75f);
			LookAtTarget();
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		AudioManager.Instance.PlayOneShot(attackSfx, base.gameObject);
		yield return StartCoroutine(projectilePattern.ShootIE());
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		projectileTimer = UnityEngine.Random.Range(timeBetweenProjectileAttacks.x, timeBetweenProjectileAttacks.y);
		base.Attacking = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private IEnumerator DelayedEnemyHealthEnable(UnitObject enemy)
	{
		enemy.health.invincible = true;
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		enemy.health.invincible = false;
	}
}
