using System.Collections.Generic;
using UnityEngine;

public class SpawnParticlesOnStart : BaseMonoBehaviour
{
	[Range(0f, 1f)]
	public int maxParticles = 10;

	public List<Sprite> ParticleChunks;

	public GameObject particleSpawn;

	public float zSpawn;

	private SpriteRenderer spriteRenderer;

	private void Start()
	{
	}

	private void doParticles()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		int num = -1;
		if (ParticleChunks.Count > 0)
		{
			while (++num < maxParticles)
			{
				Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle(base.transform.position, base.transform.position) + (float)Random.Range(-20, 20), ParticleChunks);
			}
		}
		if (particleSpawn != null)
		{
			Object.Instantiate(particleSpawn, new Vector3(base.transform.position.x, base.transform.position.y, zSpawn), Quaternion.identity);
		}
	}
}
