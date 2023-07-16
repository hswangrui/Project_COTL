using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class EnemySpawner : BaseMonoBehaviour
{
	public static List<EnemySpawner> EnemySpawners = new List<EnemySpawner>();

	public SpriteRenderer sprite;

	private GameObject g;

	public float Duration = 1f;

	public ParticleSystem particleSystem;

	public SkeletonAnimation Spine;

	public SimpleVFX goop_0;

	public SimpleVFX goop_1;

	public SimpleVFX goop_2;

	public ParticleSystem groundGoop;

	public float waitDurationForGoop = 0.35f;

	public static GameObject enemySpawnerPrefab;

	private float Timer;

	public static GameObject LoadEnemySpawnerPrefab()
	{
		if (enemySpawnerPrefab == null)
		{
			enemySpawnerPrefab = Resources.Load("Prefabs/Enemy Spawner/EnemySpawner") as GameObject;
		}
		return enemySpawnerPrefab;
	}

	public static void Unload()
	{
		enemySpawnerPrefab = null;
	}

	public static GameObject Create(Vector3 Position, Transform Parent, GameObject Spawn)
	{
		EnemySpawner component = ObjectPool.Spawn(LoadEnemySpawnerPrefab(), Parent, Position, Quaternion.identity).GetComponent<EnemySpawner>();
		component.transform.position = Position;
		return component.InitAndInstantiate(Spawn);
	}

	public static void CreateWithAndInitInstantiatedEnemy(Vector3 Position, Transform Parent, GameObject Spawn)
	{
		EnemySpawner component = ObjectPool.Spawn(LoadEnemySpawnerPrefab(), Parent, Position, Quaternion.identity).GetComponent<EnemySpawner>();
		component.transform.position = Position;
		component.Init(Spawn);
	}

	public GameObject InitAndInstantiate(GameObject g)
	{
		this.g = Object.Instantiate(g, base.transform.position, Quaternion.identity, base.transform.parent);
		DropLootOnDeath component = g.GetComponent<DropLootOnDeath>();
		if ((bool)component && component.LootToDrop == InventoryItem.ITEM_TYPE.BLACK_SOUL)
		{
			component.GiveXP = false;
		}
		EnemyRoundsBase instance = EnemyRoundsBase.Instance;
		if ((object)instance != null)
		{
			instance.AddEnemyToRound(this.g.GetComponent<Health>());
		}
		this.g.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_appear", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/enemy/summoned", base.transform.position);
		StartCoroutine(SpawnEnemy());
		StartCoroutine(spawnVFX());
		return this.g;
	}

	public void Init(GameObject g)
	{
		this.g = g;
		DropLootOnDeath component = g.GetComponent<DropLootOnDeath>();
		if ((bool)component && component.LootToDrop == InventoryItem.ITEM_TYPE.BLACK_SOUL)
		{
			component.GiveXP = false;
		}
		Debug.Log("Init()");
		EnemyRoundsBase instance = EnemyRoundsBase.Instance;
		if ((object)instance != null)
		{
			instance.AddEnemyToRound(g.GetComponent<Health>());
		}
		g.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/enemy/summoned", base.transform.position);
		StartCoroutine(SpawnEnemy());
		StartCoroutine(spawnVFX());
	}

	private void OnDestroy()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/teleport_away", base.transform.position);
		StopAllCoroutines();
		if (g != null && g.activeSelf)
		{
			ObjectPool.Recycle(g);
		}
	}

	private void OnEnable()
	{
		EnemySpawners.Add(this);
		particleSystem.Play();
		Spine.state.SetAnimation(0, "animation", false);
	}

	private void OnDisable()
	{
		EnemySpawners.Remove(this);
	}

	private IEnumerator spawnVFX()
	{
		groundGoop.gameObject.SetActive(true);
		groundGoop.Play();
		Spine.transform.localScale = Vector3.zero;
		Spine.transform.DOScale(1f, 0.25f);
		yield return new WaitForSeconds(waitDurationForGoop);
		goop_0.gameObject.SetActive(true);
		goop_1.gameObject.SetActive(true);
		goop_2.gameObject.SetActive(true);
		goop_0.Play();
		goop_1.Play();
		goop_2.Play();
	}

	private IEnumerator SpawnEnemy()
	{
		Timer = 0f;
		sprite.transform.localScale = Vector3.zero;
		while ((Timer += Time.deltaTime) < Duration)
		{
			sprite.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Timer / Duration);
			yield return null;
		}
		particleSystem.Stop();
		yield return new WaitForSeconds(0.3f);
		CameraManager.shakeCamera(0.4f, Random.Range(0, 360));
		if ((bool)g)
		{
			g.SetActive(true);
			if (PlayerRelic.TimeFrozen)
			{
				PlayerFarming.Instance.playerRelic.AddFrozenEnemy(g.GetComponent<Health>());
			}
			g = BiomeConstants.Instance.SpawnInWhite.Spawn();
			g.transform.position = base.transform.position;
		}
		float ScaleSpeed = 0.2f;
		float Scale = 1f;
		while (sprite.transform.localScale.x > 0f)
		{
			ScaleSpeed -= 0.02f;
			Scale += ScaleSpeed;
			sprite.transform.localScale = new Vector3(Scale, Scale, Scale);
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		ObjectPool.Recycle(base.gameObject);
	}
}
