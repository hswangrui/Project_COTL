using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyMillipedePoisonerMiniboss : EnemyMillipedeSpiker
{
	[SerializeField]
	private EnemyBomb bombPrefab;

	[SerializeField]
	private float shootAnticipation;

	[SerializeField]
	private float bombDuration;

	[SerializeField]
	private Vector2 amountToShoot;

	[SerializeField]
	private float timeBetweenShots;

	[SerializeField]
	private float shootingCooldown;

	[Space]
	[SerializeField]
	private float aggressionSpeedMultiplier;

	[SerializeField]
	private Vector2 aggressionDuration;

	[SerializeField]
	private Vector2 timeBetweenAggression;

	[SerializeField]
	private AssetReferenceGameObject[] spawnable;

	[Space]
	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnticipationAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	private string shootAnimation;

	private float shootTimestamp;

	private float aggressionTimestamp;

	private bool aggressive;

	private bool spawnedSecondWave;

	private bool spawnedThirdWave;

	private List<Vector3> spawnPositions = new List<Vector3>(2)
	{
		new Vector3(-3f, 0f, 0f),
		new Vector3(3f, 0f, 0f)
	};

	protected override void Start()
	{
		base.Start();
		SpawnEnemies();
		aggressionTimestamp = GameManager.GetInstance().CurrentTime + Random.Range(aggressionDuration.x, aggressionDuration.y);
	}

	private void SpawnEnemies()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/attack", base.gameObject);
		for (int i = 0; i < 2; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(spawnable[Random.Range(0, spawnable.Length)], spawnPositions[i], Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				obj.Result.gameObject.SetActive(false);
				EnemySpawner.CreateWithAndInitInstantiatedEnemy(obj.Result.transform.position, base.transform.parent, obj.Result);
			};
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		shootTimestamp = (GameManager.GetInstance() ? (GameManager.GetInstance().CurrentTime + shootingCooldown) : (Time.time + shootingCooldown));
	}

	public override void Update()
	{
		base.Update();
		GameManager instance = GameManager.GetInstance();
		if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > shootTimestamp / Spine.timeScale && !attacking && !aggressive)
		{
			StartCoroutine(ShootPoison());
		}
		GameManager instance2 = GameManager.GetInstance();
		if ((((object)instance2 != null) ? new float?(instance2.CurrentTime) : null) > aggressionTimestamp / Spine.timeScale && !attacking && !aggressive)
		{
			aggressive = true;
			aggressionTimestamp = GameManager.GetInstance().CurrentTime + Random.Range(aggressionDuration.x, aggressionDuration.y);
			SpeedMultiplier = aggressionSpeedMultiplier;
			focusOnTarget = true;
		}
		else
		{
			GameManager instance3 = GameManager.GetInstance();
			if ((((object)instance3 != null) ? new float?(instance3.CurrentTime) : null) > aggressionTimestamp / Spine.timeScale && aggressive)
			{
				aggressive = false;
				aggressionTimestamp = GameManager.GetInstance().CurrentTime + Random.Range(timeBetweenAggression.x, timeBetweenAggression.y);
				SpeedMultiplier = 1f;
				focusOnTarget = false;
			}
		}
		if (!spawnedSecondWave && health.HP < health.totalHP * 0.6f)
		{
			SpawnEnemies();
			spawnedSecondWave = true;
		}
		if (!spawnedThirdWave && health.HP < health.totalHP * 0.3f)
		{
			SpawnEnemies();
			spawnedThirdWave = true;
		}
	}

	private IEnumerator ShootPoison()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/warning", base.gameObject);
		attacking = true;
		SetAnimation(shootAnticipationAnimation, true);
		yield return new WaitForEndOfFrame();
		moveVX = 0f;
		moveVY = 0f;
		float t = 0f;
		while (t < shootAnticipation)
		{
			float amt = t / shootAnticipation;
			foreach (SimpleSpineFlash flash in flashes)
			{
				flash.FlashWhite(amt);
			}
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		foreach (SimpleSpineFlash flash2 in flashes)
		{
			flash2.FlashWhite(false);
		}
		SetAnimation(shootAnimation);
		AddAnimation(idleAnimation, true);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider/attack", base.gameObject);
		int amount = Random.Range((int)amountToShoot.x, (int)amountToShoot.y + 1);
		for (int i = 0; i < amount; i++)
		{
			Vector3 position = (Vector3)Random.insideUnitCircle * 5f;
			Object.Instantiate(bombPrefab, position, Quaternion.identity, base.transform.parent).Play(base.transform.position, bombDuration + Random.Range(-0.5f, 0.5f));
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.gameObject);
			if (timeBetweenShots == 0f)
			{
				continue;
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
		shootTimestamp = GameManager.GetInstance().CurrentTime + shootingCooldown;
		attacking = false;
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.StopActiveLoops();
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		for (int i = 0; i < Health.team2.Count; i++)
		{
			if (Health.team2[i] != null && Health.team2[i] != health)
			{
				Health.team2[i].invincible = false;
				Health.team2[i].enabled = true;
				Health.team2[i].DealDamage(Health.team2[i].totalHP, base.gameObject, base.transform.position);
			}
		}
	}
}
