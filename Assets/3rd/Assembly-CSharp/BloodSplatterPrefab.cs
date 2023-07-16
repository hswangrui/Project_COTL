using UnityEngine;

public class BloodSplatterPrefab : BaseMonoBehaviour
{
	public static BloodSplatterPrefab Instance;

	public ParticleSystem Ps;

	public ParticleSystem.Particle[] particles;

	private bool usedRoom;

	private void InitializeIfNeeded()
	{
		if (Ps == null)
		{
			Ps = GetComponent<ParticleSystem>();
		}
		if (particles == null || particles.Length < Ps.main.maxParticles)
		{
			particles = new ParticleSystem.Particle[Ps.main.maxParticles];
		}
	}

	private void OnEnable()
	{
		InitializeIfNeeded();
		Instance = this;
		if (particles != null)
		{
			Ps.SetParticles(particles);
		}
	}

	public void NewParticle()
	{
		usedRoom = true;
		Ps.GetParticles(particles);
	}

	private void OnDisable()
	{
		if (usedRoom && Ps != null)
		{
			ParticleSystem.MainModule main = Ps.main;
			main.playOnAwake = false;
		}
		if (particles != null)
		{
			Ps.GetParticles(particles);
		}
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnDestroy()
	{
		Ps.Clear();
		particles = null;
	}
}
