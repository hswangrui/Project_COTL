using System;
using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemySpiderShooterMiniboss : EnemySpider
{
	[Serializable]
	private struct Shot
	{
		public int Amount;

		public float ShootAnticipation;

		public float ShootDuration;

		public float ShootCooldown;

		public float TimeBetweenShooting;

		[SpineAnimation("", "", true, false, dataField = "Spine")]
		public string shootAnticipationAnimation;

		[SpineAnimation("", "", true, false, dataField = "Spine")]
		public string shootAnimation;

		public ProjectilePatternBase ProjectilePattern;
	}

	[Space]
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string spawnAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string spawnAnimation;

	[Space]
	[SerializeField]
	private Shot[] shots;

	[SerializeField]
	private float timeBetweenShots;

	[SerializeField]
	private float minShootDistance;

	[Space]
	[SerializeField]
	private bool canSpawnEnemies;

	[SerializeField]
	private Vector2 spawnAmount;

	[SerializeField]
	private int maxActiveEnemies;

	[SerializeField]
	private float spawnAnticipation;

	[SerializeField]
	private Vector2 timeBetweenSpawns;

	[SerializeField]
	private AssetReferenceGameObject[] enemiesList;

	private float shootTimestamp;

	private float spawnTimestamp;

	private int shootIndex;

	private LayerMask islandMask;

	public override void Awake()
	{
		base.Awake();
		islandMask = (int)islandMask | (1 << LayerMask.NameToLayer("Island"));
		spawnTimestamp = ((GameManager.GetInstance() != null) ? GameManager.GetInstance().CurrentTime : Time.time) + UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
	}

	public override void Update()
	{
		base.Update();
		if (!base.Attacking)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > shootTimestamp && (bool)PlayerFarming.Instance && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > minShootDistance)
			{
				StartCoroutine(ShootIE(shots[UnityEngine.Random.Range(0, shots.Length)]));
				return;
			}
		}
		if (canSpawnEnemies && !base.Attacking)
		{
			GameManager instance2 = GameManager.GetInstance();
			if ((((object)instance2 != null) ? new float?(instance2.CurrentTime) : null) > spawnTimestamp && Health.team2.Count - 1 < maxActiveEnemies)
			{
				StartCoroutine(SpawnIE());
			}
		}
	}

	private IEnumerator ShootIE(Shot shot)
	{
		shootIndex++;
		updateDirection = false;
		base.Attacking = true;
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		ClearPaths();
		AudioManager.Instance.PlayOneShot(warningSfx, base.transform.position);
		float time;
		for (int i = 0; i < shot.Amount; i++)
		{
			SetAnimation(shot.shootAnticipationAnimation);
			TargetEnemy = GetClosestTarget();
			LookAtTarget();
			yield return new WaitForEndOfFrame();
			float t = 0f;
			while (t < shot.ShootAnticipation)
			{
				float amt = t / shot.ShootAnticipation;
				SimpleSpineFlash.FlashWhite(amt);
				t += Time.deltaTime * Spine.timeScale;
				yield return null;
			}
			SimpleSpineFlash.FlashWhite(false);
			SetAnimation(shot.shootAnimation);
			state.CURRENT_STATE = StateMachine.State.Attacking;
			StartCoroutine(shot.ProjectilePattern.ShootIE());
			AudioManager.Instance.PlayOneShot(attackSfx, base.transform.position);
			TargetEnemy = GetClosestTarget();
			LookAtTarget();
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * Spine.timeScale);
				if (!(num < shot.TimeBetweenShooting))
				{
					break;
				}
				yield return null;
			}
		}
		AddAnimation(IdleAnimation, true);
		time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < shot.ShootCooldown))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		shootTimestamp = GameManager.GetInstance().CurrentTime + timeBetweenShots;
		updateDirection = true;
		base.Attacking = false;
	}

	private IEnumerator SpawnIE()
	{
		updateDirection = false;
		base.Attacking = true;
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		ClearPaths();
		SetAnimation(spawnAnticipationAnimation);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < spawnAnticipation))
			{
				break;
			}
			yield return null;
		}
		SetAnimation(spawnAnimation);
		AddAnimation(IdleAnimation);
		int num2 = UnityEngine.Random.Range((int)spawnAmount.x, (int)spawnAmount.y + 1);
		UnityEngine.Random.Range(0, 360);
		for (int i = 0; i < num2; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(enemiesList[UnityEngine.Random.Range(0, enemiesList.Length)], base.transform.position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				UnitObject component = obj.Result.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				component.RemoveModifier();
				Interaction_Chest instance = Interaction_Chest.Instance;
				if ((object)instance != null)
				{
					instance.AddEnemy(component.health);
				}
				EnemyRoundsBase instance2 = EnemyRoundsBase.Instance;
				if ((object)instance2 != null)
				{
					instance2.AddEnemyToRound(component.GetComponent<Health>());
				}
				component.RemoveModifier();
				SpawnEnemyOnDeath component2 = obj.Result.GetComponent<SpawnEnemyOnDeath>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
				Vector3 localScale = component.transform.localScale;
				component.transform.localScale = Vector3.zero;
				component.transform.DOScale(localScale, 0.5f).SetEase(Ease.Linear);
				if (!string.IsNullOrEmpty(spawnAnimation))
				{
					SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>();
					foreach (SkeletonAnimation obj2 in componentsInChildren)
					{
						obj2.AnimationState.SetAnimation(0, spawnAnimation, false);
						obj2.AnimationState.AddAnimation(0, "idle", true, 0f);
					}
				}
				component.DoKnockBack(UnityEngine.Random.Range(0f, 360f), 2f, 1f);
				component.StartCoroutine(DelayedEnemyHealthEnable(component));
			};
		}
		time = 0f;
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
		state.CURRENT_STATE = StateMachine.State.Idle;
		updateDirection = true;
		spawnTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
		base.Attacking = false;
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
		Collider2D[] array = Physics2D.OverlapCircleAll(enemy.transform.position, 0.5f);
		foreach (Collider2D collider2D in array)
		{
			Health component = collider2D.GetComponent<Health>();
			if (component != null && component.team == Health.Team.Neutral)
			{
				collider2D.GetComponent<Health>().DealDamage(2.1474836E+09f, enemy.gameObject, Vector3.Lerp(component.transform.position, enemy.transform.position, 0.7f));
			}
		}
	}

	protected override bool ShouldAttack()
	{
		if ((AttackDelay -= Time.deltaTime) < 0f && !base.Attacking && (bool)TargetEnemy && Vector3.Distance(base.transform.position, TargetEnemy.transform.position) < (float)VisionRange && GameManager.GetInstance().CurrentTime > initialAttackDelayTimer)
		{
			return UnityEngine.Random.Range(0f, 1f) > 0.66f;
		}
		return false;
	}
}
