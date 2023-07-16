using UnityEngine;

public class ParticleSystemAnimEvent : BaseMonoBehaviour
{
	public ParticleSystem ParticleSystem;

	public SpriteRenderer sprite;

	public void Play()
	{
		if (ParticleSystem != null)
		{
			ParticleSystem.Play();
		}
	}
}
