using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSplatterParticleCollision : BaseMonoBehaviour
{
	private ParticleSystem particles;

	private GameObject Player;

	private PlayerController playerController;

	private int numEnter;

	private List<ParticleSystem.Particle> enter;

	private void Start()
	{
		particles = GetComponent<ParticleSystem>();
		StartCoroutine(WaitForPlayer());
	}

	private IEnumerator WaitForPlayer()
	{
		while ((Player = GameObject.FindWithTag("Player")) == null)
		{
			yield return null;
		}
		playerController = Player.GetComponent<PlayerController>();
		particles.trigger.SetCollider(0, Player.GetComponent<CircleCollider2D>());
	}

	private void OnParticleTrigger()
	{
		if (!(playerController == null) && base.enabled)
		{
			enter = new List<ParticleSystem.Particle>();
			numEnter = particles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
			if (numEnter > 0)
			{
				playerController.SetFootSteps(enter[0].GetCurrentColor(particles));
			}
		}
	}
}
