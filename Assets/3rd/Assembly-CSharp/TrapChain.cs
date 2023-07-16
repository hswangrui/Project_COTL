using System;
using UnityEngine;

public class TrapChain : BaseMonoBehaviour
{
	public Transform[] Chomp;

	private float Orbit;

	public float OrbitSpeed = 1f;

	public float Distance = 2f;

	private Health EnemyHealth;

	private void Start()
	{
		Orbit = UnityEngine.Random.Range(0, 360);
	}

	private void Update()
	{
		Orbit += OrbitSpeed * GameManager.DeltaTime;
		int num = -1;
		while (++num < Chomp.Length)
		{
			Chomp[num].localPosition = new Vector3(Distance * Mathf.Cos((Orbit + (float)(360 / Chomp.Length * num)) * ((float)Math.PI / 180f)), Distance * Mathf.Sin((Orbit + (float)(360 / Chomp.Length * num)) * ((float)Math.PI / 180f)));
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, Distance, Color.red);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		EnemyHealth = collision.gameObject.GetComponent<Health>();
		if (EnemyHealth != null)
		{
			Debug.Log("COLLISION!");
			EnemyHealth.DealDamage(2f, base.gameObject, base.transform.position);
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, EnemyHealth.transform.position));
			if (EnemyHealth.team == Health.Team.PlayerTeam)
			{
				GameManager.GetInstance().HitStop();
			}
		}
	}
}
