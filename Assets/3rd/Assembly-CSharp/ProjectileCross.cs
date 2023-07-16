using System.Collections;
using UnityEngine;

public class ProjectileCross : BaseMonoBehaviour
{
	[SerializeField]
	private float torque;

	[SerializeField]
	private Projectile[] projectiles;

	private Projectile projectile;

	private bool active;

	public Projectile Projectile
	{
		get
		{
			return projectile;
		}
	}

	public void InitDelayed()
	{
		projectile = GetComponent<Projectile>();
		Projectile[] array = projectiles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
		for (int j = 0; j < projectiles.Length; j++)
		{
			projectiles[j].health = projectile.health;
			projectiles[j].team = Health.Team.Team2;
		}
		StartCoroutine(EnableProjectiles());
	}

	private void Update()
	{
		if (active && !PlayerRelic.TimeFrozen)
		{
			base.transform.Rotate(new Vector3(0f, 0f, torque * Time.deltaTime));
		}
		Projectile[] array = projectiles;
		foreach (Projectile projectile in array)
		{
			if ((bool)projectile)
			{
				projectile.transform.eulerAngles = new Vector3(-60f, 0f, 0f);
			}
		}
		if (this.projectile != null && this.projectile.health == null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator EnableProjectiles()
	{
		yield return new WaitForSeconds(0.1f);
		int counter = 0;
		for (int i = 0; i < projectiles.Length; i++)
		{
			projectiles[i].gameObject.SetActive(true);
			projectiles[i].SpeedMultiplier = 0f;
			counter++;
			if (counter >= 4)
			{
				counter = 0;
				yield return new WaitForSeconds(0.08f);
			}
		}
		active = true;
	}

	public IEnumerator DisableProjectiles()
	{
		if (!active)
		{
			yield break;
		}
		active = false;
		int counter = 0;
		for (int i = projectiles.Length - 1; i >= 0; i--)
		{
			if (projectiles != null && projectiles[i] != null)
			{
				projectiles[i].DestroyProjectile(true);
			}
			counter++;
			if (counter >= 4)
			{
				counter = 0;
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}
