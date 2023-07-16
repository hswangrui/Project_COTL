using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class SpawnParticlesOnHit : BaseMonoBehaviour
{
	[Range(0f, 1f)]
	public float BreakCameraShake;

	private Health health;

	public int maxParticles = 10;

	public List<Sprite> ParticleChunks;

	public GameObject particleSpawn;

	public float zSpawn;

	private SpriteRenderer spriteRenderer;

	private void Start()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (spriteRenderer != null)
		{
			if (spriteRenderer.isVisible)
			{
				CameraManager.shakeCamera(BreakCameraShake, Utils.GetAngle(Attacker.transform.position, base.transform.position));
			}
		}
		else
		{
			CameraManager.shakeCamera(BreakCameraShake, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		}
		int num = -1;
		if (ParticleChunks.Count > 0)
		{
			while (++num < maxParticles)
			{
				Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle(Attacker.transform.position, base.transform.position) + (float)Random.Range(-20, 20), ParticleChunks);
			}
		}
		if (particleSpawn != null)
		{
			Object.Instantiate(particleSpawn, new Vector3(base.transform.position.x, base.transform.position.y, zSpawn), Quaternion.identity);
		}
	}
}
