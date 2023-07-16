using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemyHordeMiniboss : UnitObject
{
	private enum SpawnType
	{
		Horizontal1,
		Horizontal2,
		Vertical,
		Circle
	}

	public SkeletonAnimation Spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string idleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string spawnAnimation;

	[SerializeField]
	private Vector2 timeBetweenMove;

	[SerializeField]
	private GameObject singleSpawnable;

	[SerializeField]
	private float spawnAnticipation;

	[SerializeField]
	private Vector2 timeBetweenSpawns;

	[SerializeField]
	private int maxSpawnAmount;

	[SerializeField]
	private float horizontalDistanceBetween;

	[SerializeField]
	private Vector3[] horizontalSpawnPositions = new Vector3[2];

	[SerializeField]
	private float horizontalTimeBetween;

	[SerializeField]
	private int horizontalAmount;

	[SerializeField]
	private float verticalDistanceBetween;

	[SerializeField]
	private Vector3[] verticalSpawnPositions = new Vector3[2];

	[SerializeField]
	private float verticalTimeBetween;

	[SerializeField]
	private int verticalAmount;

	[SerializeField]
	private float circleRadius;

	[SerializeField]
	private int circleAmount;

	[SerializeField]
	private float circleTimeBetween;

	private bool spawning;

	private float spawnTimestamp = -1f;

	private float moveTimestamp;

	private List<UnitObject> spawnedEnemies = new List<UnitObject>();

	private SpawnType previousSpawnType;

	private int currentEnemies;

	private int availableEnemyCount
	{
		get
		{
			return maxSpawnAmount - currentEnemies;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		spawning = false;
		simpleSpineFlash.FlashWhite(false);
	}

	public override void Update()
	{
		base.Update();
		if (GameManager.RoomActive && spawnTimestamp == -1f)
		{
			spawnTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
		}
		if (GameManager.RoomActive)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > moveTimestamp)
			{
				Flee();
			}
		}
		if (CanSpawn())
		{
			Spawn();
		}
	}

	private bool CanSpawn()
	{
		if (GameManager.RoomActive && !spawning)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > spawnTimestamp)
			{
				return spawnTimestamp != -1f;
			}
		}
		return false;
	}

	private void Spawn()
	{
		SpawnType spawnType;
		do
		{
			spawnType = (SpawnType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(SpawnType)).Length);
		}
		while (spawnType == previousSpawnType);
		switch (spawnType)
		{
		case SpawnType.Horizontal1:
			SpawnHorizontalLine1();
			break;
		case SpawnType.Horizontal2:
			SpawnHorizontalLine2();
			break;
		case SpawnType.Vertical:
			SpawnVerticalLine();
			break;
		case SpawnType.Circle:
			if (Vector3.Distance(PlayerFarming.Instance.transform.position, Vector3.zero) < 4.5f - circleRadius / 2f)
			{
				SpawnCircleAroundPlayer();
			}
			break;
		}
		previousSpawnType = spawnType;
	}

	private void SpawnHorizontalLine1()
	{
		StartCoroutine(SpawnHorizontalLineIE(horizontalSpawnPositions[0]));
	}

	private void SpawnHorizontalLine2()
	{
		StartCoroutine(SpawnHorizontalLineIE(horizontalSpawnPositions[1]));
	}

	private IEnumerator SpawnHorizontalLineIE(Vector3 spawnPosition)
	{
		if (availableEnemyCount < verticalAmount)
		{
			spawning = false;
			yield break;
		}
		spawning = true;
		Spine.AnimationState.SetAnimation(0, spawnAnimation, false);
		yield return new WaitForSeconds(spawnAnticipation);
		if (availableEnemyCount < horizontalAmount)
		{
			spawning = false;
			yield break;
		}
		float num = horizontalDistanceBetween * (float)horizontalAmount / 2f;
		float xStartPosition = 0f - num;
		for (int i = 0; i < horizontalAmount; i++)
		{
			StartCoroutine(SpawnEnemy(new Vector3(xStartPosition + spawnPosition.x, spawnPosition.y, spawnPosition.z)));
			xStartPosition += horizontalDistanceBetween;
			yield return new WaitForSeconds(horizontalTimeBetween);
		}
		spawnTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
		spawning = false;
	}

	private void SpawnVerticalLine()
	{
		StartCoroutine(SpawnVerticalLineIE());
	}

	private IEnumerator SpawnVerticalLineIE()
	{
		if (availableEnemyCount < verticalAmount)
		{
			spawning = false;
			yield break;
		}
		spawning = true;
		Spine.AnimationState.SetAnimation(0, spawnAnimation, false);
		int spawnType = UnityEngine.Random.Range(0, 4);
		yield return new WaitForSeconds(spawnAnticipation);
		if (availableEnemyCount < verticalAmount)
		{
			spawning = false;
			yield break;
		}
		float num = verticalDistanceBetween * (float)verticalAmount / 2f;
		float yStartPosition1 = 0f - num;
		float yStartPosition2 = 0f - num;
		int direction1 = 1;
		int direction2 = 1;
		switch (spawnType)
		{
		case 1:
			direction1 = -1;
			direction2 = -1;
			yStartPosition1 = num;
			yStartPosition2 = num;
			break;
		case 2:
			direction1 = -1;
			yStartPosition1 = num;
			break;
		case 3:
			direction2 = -1;
			yStartPosition2 = num;
			break;
		}
		for (int i = 0; i < verticalAmount * 2; i += 2)
		{
			Vector3 position = new Vector3(verticalSpawnPositions[0].x, verticalSpawnPositions[0].y + yStartPosition1, verticalSpawnPositions[0].z);
			Vector3 position2 = new Vector3(verticalSpawnPositions[1].x, verticalSpawnPositions[1].y + yStartPosition2, verticalSpawnPositions[1].z);
			StartCoroutine(SpawnEnemy(position));
			StartCoroutine(SpawnEnemy(position2));
			yStartPosition1 += verticalDistanceBetween * (float)direction1;
			yStartPosition2 += verticalDistanceBetween * (float)direction2;
			yield return new WaitForSeconds(verticalTimeBetween);
		}
		spawnTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
		spawning = false;
	}

	private void SpawnCircleAroundPlayer()
	{
		StartCoroutine(SpawnCircleAroundPlayerIE());
	}

	private IEnumerator SpawnCircleAroundPlayerIE()
	{
		if (availableEnemyCount < verticalAmount)
		{
			spawning = false;
			yield break;
		}
		spawning = true;
		Spine.AnimationState.SetAnimation(0, spawnAnimation, false);
		yield return new WaitForSeconds(spawnAnticipation);
		if (availableEnemyCount < circleAmount)
		{
			spawning = false;
			yield break;
		}
		Vector3 playerPosition = PlayerFarming.Instance.transform.position;
		float angle = 0f;
		float increment = 360 / circleAmount;
		int direction = ((UnityEngine.Random.Range(0, 2) != 0) ? 1 : (-1));
		for (int i = 0; i < circleAmount * 2; i += 2)
		{
			Vector3 position = playerPosition + (Vector3)(Utils.RadianToVector2(angle * ((float)Math.PI / 180f)) * circleRadius);
			StartCoroutine(SpawnEnemy(position));
			angle = Mathf.Repeat(angle + increment * (float)direction, 360f);
			yield return new WaitForSeconds(circleTimeBetween);
		}
		spawnTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y);
		spawning = false;
	}

	private IEnumerator SpawnEnemy(Vector3 position)
	{
		UnitObject enemy = UnityEngine.Object.Instantiate(singleSpawnable, position, Quaternion.identity, base.transform.parent).GetComponent<UnitObject>();
		SkeletonAnimation spine = enemy.GetComponent<EnemySwordsman>().Spine;
		spine.AnimationState.SetAnimation(0, "grave-spawn", false);
		spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		spawnedEnemies.Add(enemy);
		EnemyStealth stealth = enemy.GetComponent<EnemyStealth>();
		stealth.enabled = false;
		enemy.enabled = false;
		enemy.health.invincible = true;
		yield return new WaitForSeconds(1f);
		stealth.enabled = true;
		enemy.enabled = true;
		enemy.health.invincible = false;
		enemy.health.OnDie += OnEnemyKilled;
		currentEnemies++;
	}

	private new void OnEnemyKilled(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		currentEnemies--;
	}

	private void Flee()
	{
		if (!PlayerFarming.Instance)
		{
			return;
		}
		float num = 100f;
		while ((num -= 1f) > 0f)
		{
			float f = (float)UnityEngine.Random.Range(0, 360) * ((float)Math.PI / 180f);
			float num2 = UnityEngine.Random.Range(7, 10);
			Vector3 vector = PlayerFarming.Instance.transform.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
			Vector3 vector2 = Vector3.Normalize(vector - PlayerFarming.Instance.transform.position);
			if (Physics2D.CircleCast(PlayerFarming.Instance.transform.position, 0.5f, vector2, num2, layerToCheck).collider == null)
			{
				moveTimestamp = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(timeBetweenMove.x, timeBetweenMove.y);
				givePath(vector);
			}
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		simpleSpineFlash.FlashFillRed();
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
				Health.team2[num].DealDamage(Health.team2[num].totalHP, Attacker, AttackLocation);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Vector3[] array = horizontalSpawnPositions;
		for (int i = 0; i < array.Length; i++)
		{
			Utils.DrawCircleXY(array[i], 0.5f, Color.green);
		}
		array = verticalSpawnPositions;
		for (int i = 0; i < array.Length; i++)
		{
			Utils.DrawCircleXY(array[i], 0.5f, Color.green);
		}
	}
}
