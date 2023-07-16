using System.Collections;
using UnityEngine;

public class PoisonBomb : EnemyBomb
{
	[Space]
	[SerializeField]
	private GameObject poisonPrefab;

	[SerializeField]
	private GameObject splashPrefab;

	[SerializeField]
	private float poisonRadius;

	[SerializeField]
	private Vector2 posionScaleRange = new Vector2(1f, 1f);

	[SerializeField]
	private int poisonAmount;

	[SerializeField]
	private float impactDamageRadius;

	public float impactDamage = 1f;

	private TrailRenderer trailRenderer;

	public GameObject PoisonPrefab
	{
		get
		{
			return poisonPrefab;
		}
		set
		{
			poisonPrefab = value;
		}
	}

	public float PoisonRadius
	{
		get
		{
			return poisonRadius;
		}
		set
		{
			poisonRadius = value;
		}
	}

	public int PoisonAmount
	{
		get
		{
			return poisonAmount;
		}
		set
		{
			poisonAmount = value;
		}
	}

	public float DamageMultiplier { get; set; } = 1f;


	public float TickDurationMultiplier { get; set; } = 1f;


	private void OnDisable()
	{
		if (trailRenderer == null)
		{
			trailRenderer = GetComponentInChildren<TrailRenderer>();
		}
		if (trailRenderer != null)
		{
			trailRenderer.Clear();
		}
	}

	protected override void BombLanded()
	{
		AudioManager.Instance.PlayOneShot("event:/fishing/splash", base.gameObject);
		if (splashPrefab != null)
		{
			Object.Instantiate(splashPrefab, base.transform.position, Quaternion.identity, base.transform.parent).transform.Rotate(0f, 0f, Random.Range(0f, 360f));
		}
		for (int i = 0; i < poisonAmount; i++)
		{
			Vector2 vector = Random.insideUnitCircle * poisonRadius;
			float num = Random.Range(posionScaleRange.x, posionScaleRange.y);
			if (i == 0)
			{
				vector *= 0f;
				num = 1f;
			}
			GameObject gameObject = ObjectPool.Spawn(poisonPrefab, base.transform.parent, base.transform.position + (Vector3)vector, Quaternion.identity);
			gameObject.transform.localScale = new Vector3(num, num, 1f);
			if ((bool)gameObject.GetComponent<TrapGoop>())
			{
				gameObject.GetComponent<TrapGoop>().DamageMultiplier = DamageMultiplier;
				gameObject.GetComponent<TrapGoop>().TickDurationMultiplier = TickDurationMultiplier;
			}
			RaycastHit hitInfo;
			if (Physics.Raycast(gameObject.transform.position - Vector3.forward * 2f, Vector3.forward, out hitInfo, float.PositiveInfinity) && hitInfo.collider.gameObject.GetComponent<MeshCollider>() != null)
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, hitInfo.point.z);
			}
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.ProjectileAOE_ExplosiveImpact)
		{
			GameManager.GetInstance().StartCoroutine(CreateExplosions(base.transform.position, Random.Range(3, 6), 1.5f));
			return;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, impactDamageRadius);
		for (int j = 0; j < array.Length; j++)
		{
			Health component = array[j].GetComponent<Health>();
			if (!component)
			{
				continue;
			}
			if (component.team == Health.Team.Team2 || component.IsCharmedEnemy)
			{
				if (impactDamage > 0f)
				{
					component.DealDamage(impactDamage, base.gameObject, base.transform.position);
				}
				if (DataManager.Instance.CurrentCurse == EquipmentType.ProjectileAOE_Charm && Random.value <= EquipmentManager.GetCurseData(EquipmentType.ProjectileAOE_Charm).Chance)
				{
					component.AddCharm();
				}
			}
			else if (component.team == Health.Team.Neutral && impactDamage > 0f)
			{
				component.DealDamage(impactDamage, base.gameObject, base.transform.position);
			}
		}
	}

	private IEnumerator CreateExplosions(Vector3 p, int amount, float radius)
	{
		for (int i = 0; i < amount; i++)
		{
			Vector3 position = ((i != 0) ? (p + (Vector3)Random.insideUnitCircle * radius) : p);
			Explosion.CreateExplosion(position, Health.Team.PlayerTeam, PlayerFarming.Instance.health, 1f, impactDamage, impactDamage);
			yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
		}
	}
}
