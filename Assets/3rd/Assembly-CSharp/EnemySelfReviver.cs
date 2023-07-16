using UnityEngine;

[RequireComponent(typeof(EnemyRevivable))]
public class EnemySelfReviver : BaseMonoBehaviour
{
	[SerializeField]
	private float spawnDelay = 2f;

	private Rigidbody2D rigidbody;

	private Health health;

	private EnemyRevivable revivable;

	private GameManager gm;

	private Vector3 startingPosition = Vector3.zero;

	private float spawnInitialDelay = 1f;

	private float spawnDelayTimestamp;

	private float spawnTimer;

	private float shakeSpeed = 10f;

	private float shakeAmount = 0.05f;

	private void Awake()
	{
		revivable = GetComponent<EnemyRevivable>();
		rigidbody = GetComponentInChildren<Rigidbody2D>();
		health = GetComponentInChildren<Health>();
	}

	private void OnEnable()
	{
		gm = GameManager.GetInstance();
		spawnDelayTimestamp = gm.CurrentTime + spawnInitialDelay;
		Health.team2.Add(health);
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null)
		{
			instance.AddEnemy(health);
		}
	}

	private void OnDisable()
	{
		Health.team2.Remove(health);
	}

	private void Update()
	{
		if ((bool)gm && gm.CurrentTime > spawnDelayTimestamp && rigidbody.velocity.magnitude <= 0.1f)
		{
			if (startingPosition == Vector3.zero)
			{
				startingPosition = base.transform.position;
			}
			spawnTimer += Time.deltaTime;
			float num = spawnTimer / spawnDelay;
			float x = startingPosition.x + Mathf.Sin(Time.time * (shakeSpeed * num)) * (shakeAmount * num);
			base.transform.position = new Vector3(x, base.transform.position.y, base.transform.position.z);
			if (num > 1f)
			{
				Spawn();
			}
		}
	}

	private void Spawn()
	{
		Health component = EnemySpawner.Create(base.transform.position, base.transform.parent, revivable.Enemy).GetComponent<Health>();
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null)
		{
			instance.AddEnemy(component);
		}
		Interaction_Chest instance2 = Interaction_Chest.Instance;
		if ((object)instance2 != null)
		{
			instance2.OnSpawnedDie(base.gameObject, base.transform.position, health, Health.AttackTypes.Heavy, Health.AttackFlags.Crit);
		}
		Object.Destroy(base.gameObject);
	}
}
