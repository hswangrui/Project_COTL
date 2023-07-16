using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class SpawnParticlesOnDeath : BaseMonoBehaviour
{
	[Range(0f, 1f)]
	public float BreakCameraShake;

	private Health health;

	public int maxParticles = 10;

	public List<Sprite> ParticleChunks;

	public GameObject particleSpawn;

	public float zSpawn;

	public Material particleMaterial;

	public bool scaleParticlesOut = true;

	private void OnEnable()
	{
		if (health == null)
		{
			health = GetComponent<Health>();
		}
		if (health != null)
		{
			health.OnDie += OnDie;
		}
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (Attacker == null || base.transform == null)
		{
			return;
		}
		CameraManager.shakeCamera(BreakCameraShake, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		int num = -1;
		if (ParticleChunks.Count > 0)
		{
			while (++num < maxParticles)
			{
				if (Attacker == null || base.transform == null)
				{
					return;
				}
				if (particleMaterial == null)
				{
					Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle(Attacker.transform.position, base.transform.position) + (float)Random.Range(-20, 20), ParticleChunks, -1, scaleParticlesOut);
				}
				else
				{
					Particle_Chunk.AddNewMat(base.transform.position, Utils.GetAngle(Attacker.transform.position, base.transform.position) + (float)Random.Range(-20, 20), ParticleChunks, -1, particleMaterial, scaleParticlesOut);
				}
			}
		}
		if (particleSpawn != null)
		{
			Object.Instantiate(particleSpawn, new Vector3(base.transform.position.x, base.transform.position.y, zSpawn), Quaternion.identity);
		}
	}
}
