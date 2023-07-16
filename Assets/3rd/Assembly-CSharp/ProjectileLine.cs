using System.Collections;
using UnityEngine;

public class ProjectileLine : BaseMonoBehaviour
{
	[SerializeField]
	private float durationTillMaxLine;

	[SerializeField]
	private float stepSize = 0.5f;

	[SerializeField]
	private Vector2 direction = Vector2.left;

	[SerializeField]
	private int projectilesCount = 37;

	[SerializeField]
	private Projectile projectilePrefab;

	private Projectile[] projectiles;

	private Projectile projectile;

	private void Awake()
	{
		projectile = GetComponent<Projectile>();
		InitializeProjectiles();
	}

	private void InitializeProjectiles()
	{
		if (projectilePrefab == null)
		{
			Debug.LogError("Missing prefab", this);
			return;
		}
		projectiles = new Projectile[projectilesCount];
		if (ObjectPool.CountPooled(projectilePrefab) == 0)
		{
			ObjectPool.CreatePool(projectilePrefab, projectilesCount);
		}
	}

	public void InitDelayed(GameObject target, float shootDelay, float angle)
	{
		float num = ((projectiles.Length % 2 == 0) ? (stepSize / 2f) : 0f);
		int num2 = projectiles.Length % 2;
		for (int i = 0; i < projectiles.Length; i++)
		{
			int num3 = ((i % 2 <= 0) ? 1 : (-1));
			Vector2 vector = ((float)((i + num2) / 2) * stepSize + num) * (float)num3 * direction;
			Projectile projectile = ObjectPool.Spawn(projectilePrefab, base.transform);
			projectile.transform.localPosition = Vector3.zero;
			projectile.transform.localPosition = vector;
			projectile.health = this.projectile.health;
			projectile.team = Health.Team.Team2;
			projectile.gameObject.SetActive(false);
			projectiles[i] = projectile;
		}
		StartCoroutine(EnableProjectiles(target, shootDelay, angle));
	}

	private IEnumerator EnableProjectiles(GameObject target, float delay, float angle)
	{
		projectile.SpeedMultiplier = 0f;
		Projectile[] array = projectiles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(true);
			yield return new WaitForSeconds(0.02f);
		}
		yield return new WaitForSeconds(0.25f + delay);
		projectile.Angle = angle;
		projectile.SpeedMultiplier = 1f;
	}
}
