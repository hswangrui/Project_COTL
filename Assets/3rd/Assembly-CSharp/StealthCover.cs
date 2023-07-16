using System.Collections.Generic;
using UnityEngine;

public class StealthCover : BaseMonoBehaviour
{
	public static List<StealthCover> StealthCovers = new List<StealthCover>();

	public float Radius = 1f;

	private Health health;

	public bool ShowGizmos;

	private void OnEnable()
	{
		StealthCovers.Add(this);
		health = GetComponent<Health>();
		if (health != null)
		{
			health.OnDie += Health_OnDie;
		}
	}

	private void Health_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		StealthCovers.Remove(this);
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDie -= Health_OnDie;
		}
		StealthCovers.Remove(this);
	}

	public void EndStealth()
	{
		StealthCovers.Remove(this);
	}

	private void OnDrawGizmos()
	{
		if (ShowGizmos)
		{
			Utils.DrawCircleXY(base.transform.position, Radius, Color.magenta);
		}
	}
}
