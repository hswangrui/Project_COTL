using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemySpiderSpawner : EnemySpider
{
	[SerializeField]
	private float spawnAnticipation;

	[SerializeField]
	private Vector2 timeBetweenSpawn;

	[SerializeField]
	private GameObject spawnable;

	[SerializeField]
	private float spawnMinDistance;

	[SerializeField]
	private bool isSpawnablesIncreasingDamageMultiplier = true;

	private float spawnTimestamp = -1f;

	private bool spawning;

	public override void Update()
	{
		base.Update();
		if (ShouldSpawn())
		{
			StartCoroutine(SpawnIE());
		}
		if (GameManager.RoomActive && spawnTimestamp == -1f)
		{
			spawnTimestamp = GameManager.GetInstance().CurrentTime + Random.Range(timeBetweenSpawn.x, timeBetweenSpawn.y);
		}
	}

	protected override bool ShouldAttack()
	{
		if (base.ShouldAttack() && !spawning && (bool)TargetEnemy)
		{
			return Vector3.Distance(base.transform.position, TargetEnemy.transform.position) < spawnMinDistance;
		}
		return false;
	}

	private bool ShouldSpawn()
	{
		if (GameManager.RoomActive)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > spawnTimestamp && (bool)TargetEnemy && !spawning && Vector3.Distance(base.transform.position, TargetEnemy.transform.position) > spawnMinDistance)
			{
				return Health.team2.Count <= 8;
			}
		}
		return false;
	}

	private IEnumerator SpawnIE()
	{
		spawning = true;
		float t = 0f;
		while (t < spawnAnticipation)
		{
			SimpleSpineFlash.FlashWhite(t / spawnAnticipation);
			t += Time.deltaTime * Spine.timeScale;
			yield return null;
		}
		SimpleSpineFlash.FlashWhite(false);
		GameObject obj = Object.Instantiate(spawnable, base.transform.position, Quaternion.identity, base.transform.parent);
		obj.transform.localScale = Vector3.zero;
		obj.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);
		Health component = obj.GetComponent<Health>();
		if (component != null)
		{
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.AddEnemy(component);
			}
			component.CanIncreaseDamageMultiplier = isSpawnablesIncreasingDamageMultiplier;
		}
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
		spawning = false;
		spawnTimestamp = GameManager.GetInstance().CurrentTime + Random.Range(timeBetweenSpawn.x, timeBetweenSpawn.y);
	}
}
