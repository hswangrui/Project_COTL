using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnEnemyOnDeath : BaseMonoBehaviour
{
	public delegate void SpawnEvent(UnitObject spawnedEnemies);

	[SerializeField]
	private AssetReferenceGameObject[] enemiesList;

	[SerializeField]
	private int amount = 2;

	[SerializeField]
	private Vector2 randomSpawnAmountOffset = new Vector2(0f, 0f);

	[SerializeField]
	[Range(0f, 1f)]
	private float spawnChance = 1f;

	[SerializeField]
	private float spawnForce;

	[SerializeField]
	private float growDuration;

	[SerializeField]
	private string spawnAnimation = "";

	private Health health;

	private LayerMask islandMask;

	public int Amount
	{
		get
		{
			return amount;
		}
		set
		{
			amount = value;
		}
	}

	public UnitObject[] SpawnedEnemies { get; private set; }

	public bool SpawnEnemies { get; set; } = true;


	public event SpawnEvent OnEnemySpawned;

	public event SpawnEvent OnEnemyDespawned;

	private void Awake()
	{
		health = GetComponent<Health>();
		health.OnDie += OnDie;
		islandMask = (int)islandMask | (1 << LayerMask.NameToLayer("Island"));
	}

	private void OnDestroy()
	{
		if ((bool)health)
		{
			health.OnDie -= OnDie;
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!SpawnEnemies)
		{
			return;
		}
		SpawnEnemies = false;
		if (!(Random.value <= spawnChance))
		{
			return;
		}
		int num = amount + Random.Range((int)randomSpawnAmountOffset.x, (int)randomSpawnAmountOffset.y);
		SpawnedEnemies = new UnitObject[num + 1];
		float randomStartAngle = Random.Range(0, 360);
		Health.team2.Add(null);
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null)
		{
			instance.Enemies.Add(null);
		}
		SpawnEvent onEnemySpawned = this.OnEnemySpawned;
		if (onEnemySpawned != null)
		{
			onEnemySpawned(null);
		}
		int i;
		for (i = 0; i < num; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(enemiesList[Random.Range(0, enemiesList.Length)], base.transform.position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				if (Health.team2.Contains(null))
				{
					Health.team2.Remove(null);
					SpawnEvent onEnemyDespawned = this.OnEnemyDespawned;
					if (onEnemyDespawned != null)
					{
						onEnemyDespawned(null);
					}
					Interaction_Chest instance2 = Interaction_Chest.Instance;
					if ((object)instance2 != null)
					{
						instance2.Enemies.Remove(null);
					}
				}
				UnitObject component = obj.Result.GetComponent<UnitObject>();
				component.CanHaveModifier = false;
				component.RemoveModifier();
				Interaction_Chest instance3 = Interaction_Chest.Instance;
				if ((object)instance3 != null)
				{
					instance3.AddEnemy(component.health);
				}
				EnemyRoundsBase instance4 = EnemyRoundsBase.Instance;
				if ((object)instance4 != null)
				{
					instance4.AddEnemyToRound(component.GetComponent<Health>());
				}
				SpawnedEnemies[i] = component;
				component.RemoveModifier();
				SpawnEnemyOnDeath component2 = obj.Result.GetComponent<SpawnEnemyOnDeath>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
				if (growDuration != 0f)
				{
					Vector3 localScale = component.transform.localScale;
					component.transform.localScale = Vector3.zero;
					component.transform.DOScale(localScale, growDuration).SetEase(Ease.Linear);
				}
				if (!string.IsNullOrEmpty(spawnAnimation))
				{
					SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>();
					foreach (SkeletonAnimation obj2 in componentsInChildren)
					{
						obj2.AnimationState.SetAnimation(0, spawnAnimation, false);
						obj2.AnimationState.AddAnimation(0, "idle", true, 0f);
					}
				}
				component.DoKnockBack(randomStartAngle, spawnForce, 0.5f);
				component.StartCoroutine(DelayedEnemyHealthEnable(component));
				SpawnEvent onEnemySpawned2 = this.OnEnemySpawned;
				if (onEnemySpawned2 != null)
				{
					onEnemySpawned2(component);
				}
			};
			if ((bool)Physics2D.Raycast(base.transform.position, Utils.DegreeToVector2(randomStartAngle), 0.5f, islandMask))
			{
				randomStartAngle = Mathf.Repeat(randomStartAngle + 180f, 360f);
			}
			randomStartAngle = Mathf.Repeat(randomStartAngle + (float)(360 / num), 360f);
		}
	}

	private IEnumerator DelayedEnemyHealthEnable(UnitObject enemy)
	{
		enemy.health.invincible = true;
		yield return new WaitForSeconds(0.5f);
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
}
