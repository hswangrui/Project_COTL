using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class EnemyReviver : UnitObject
{
	public SkeletonAnimation Spine;

	[SerializeField]
	private float reviveDuration = 2f;

	[SerializeField]
	private LayerMask deadBodyMask;

	[SerializeField]
	private LayerMask enemyMask;

	private GameManager gm;

	private const float timeBetweenChecks = 0.2f;

	private float checkBodyTimestamp;

	private bool reviving;

	private List<Health> spawnedEnemies = new List<Health>();

	private GameObject currentDeadBody;

	private Coroutine spawnRoutine;

	private void Start()
	{
		gm = GameManager.GetInstance();
		Spine.transform.localPosition = Spine.transform.up * -2f;
		Spine.transform.gameObject.SetActive(false);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (currentDeadBody != null)
		{
			Spine.transform.localPosition = Spine.transform.up * -2f;
			StartCoroutine(SpawnEnemyIE(currentDeadBody.GetComponentInChildren<Health>()));
		}
	}

	public override void Update()
	{
		base.Update();
		if (spawnRoutine != null && currentDeadBody == null)
		{
			StopCoroutine(spawnRoutine);
			spawnRoutine = null;
			StartCoroutine(PopOut());
		}
	}

	private void LateUpdate()
	{
		base.Update();
		if (!reviving && gm.CurrentTime > checkBodyTimestamp)
		{
			Health health = FindDeadBody();
			if ((bool)health)
			{
				spawnRoutine = StartCoroutine(SpawnEnemyIE(health));
			}
			checkBodyTimestamp = gm.CurrentTime + 0.2f;
		}
		List<Health> team = Health.team2;
		team.Remove(base.health);
		List<Health> neutralTeam = Health.neutralTeam;
		for (int num = neutralTeam.Count - 1; num >= 0; num--)
		{
			if (!neutralTeam[num].GetComponentInParent<DeadBodySliding>())
			{
				neutralTeam.RemoveAt(num);
			}
		}
		if (GameManager.RoomActive && neutralTeam.Count == 0 && team.Count == 0 && spawnedEnemies.Count == 0)
		{
			base.health.MeleeAttackVulnerability = 1f;
			base.health.DamageModifier = 1f;
			base.health.untouchable = false;
			base.health.DestroyOnDeath = false;
			base.health.DealDamage(base.health.totalHP, base.gameObject, base.transform.position);
			base.health.DestroyNextFrame();
		}
	}

	private IEnumerator SpawnEnemyIE(Health deadBody)
	{
		reviving = true;
		currentDeadBody = deadBody.gameObject;
		yield return new WaitForSeconds(1f);
		base.transform.position = deadBody.transform.position + (Vector3)Random.insideUnitCircle * 2f;
		yield return StartCoroutine(PopIn());
		Vector3 startingPosition = deadBody.transform.position;
		float t = 0f;
		while (t < reviveDuration)
		{
			float num = t / reviveDuration;
			float x = startingPosition.x + Mathf.Sin(Time.time * (10f * num)) * (0.05f * num);
			deadBody.transform.position = new Vector3(x, deadBody.transform.position.y, deadBody.transform.position.z);
			t += Time.deltaTime;
			yield return null;
		}
		SpawnEnemy(deadBody.GetComponentInParent<EnemyRevivable>());
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(PopOut());
	}

	private IEnumerator PopIn()
	{
		Spine.transform.localPosition = Spine.transform.up * -2f;
		Spine.transform.gameObject.SetActive(true);
		Spine.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator PopOut()
	{
		Spine.transform.DOLocalMove(Spine.transform.up * -2f, 0.5f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(0.5f);
		Spine.transform.gameObject.SetActive(false);
		reviving = false;
	}

	private void SpawnEnemy(EnemyRevivable revivable)
	{
		Health component = EnemySpawner.Create(revivable.transform.position, base.transform.parent, revivable.Enemy).GetComponent<Health>();
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null)
		{
			instance.AddEnemy(component);
		}
		Health.team2.Remove(revivable.GetComponentInChildren<Health>());
		Object.Destroy(revivable.gameObject);
		component.OnDie += EnemyDied;
		spawnedEnemies.Add(component);
	}

	private void EnemyDied(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		spawnedEnemies.Remove(Victim);
	}

	private Health FindDeadBody()
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, 50f, deadBodyMask);
		Collider2D collider2D = null;
		if (array.Length != 0)
		{
			Collider2D[] array2 = array;
			foreach (Collider2D collider2D2 in array2)
			{
				if ((bool)collider2D2.GetComponentInParent<DeadBodySliding>() && (collider2D == null || Vector3.Distance(base.transform.position, collider2D2.transform.position) < Vector3.Distance(base.transform.position, collider2D.transform.position)))
				{
					collider2D = collider2D2;
				}
			}
		}
		if ((object)collider2D == null)
		{
			return null;
		}
		return collider2D.GetComponentInChildren<Health>();
	}
}
