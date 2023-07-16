using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BreakableDoor : BaseMonoBehaviour
{
	public BoxCollider2D _collider;

	public float CameraShake = 2f;

	public int maxParticles = 20;

	public float zSpawn = 0.5f;

	public List<GameObject> gameObjectsToDisable;

	public ParticleSystem particles;

	private Vector2 Velocity;

	public Animator doorAnimation;

	private void Start()
	{
		if (_collider == null)
		{
			_collider = base.gameObject.GetComponent<BoxCollider2D>();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Debug.Log("Collision");
		if (collision.gameObject.tag == "Player")
		{
			Velocity = collision.relativeVelocity;
			_collider.enabled = false;
			_collider.isTrigger = true;
			BreakDoor();
		}
	}

	private void BreakDoor()
	{
		if (doorAnimation != null)
		{
			doorAnimation.SetTrigger("Trigger");
		}
		for (int i = 0; i < gameObjectsToDisable.Count; i++)
		{
			gameObjectsToDisable[i].SetActive(false);
		}
		CameraManager.shakeCamera(CameraShake, Utils.GetAngle(base.transform.position, base.transform.position));
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * zSpawn);
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.velocity = new Vector3(Velocity.x / 4f, Velocity.y / 4f, -2f);
		particles = Object.Instantiate(particles, base.transform.position, Quaternion.identity, base.transform);
		particles.transform.parent = particles.transform.parent.parent;
		particles.Emit(emitParams, maxParticles);
		StartCoroutine(PauseParticles());
	}

	private IEnumerator PauseParticles()
	{
		yield return new WaitForSeconds(particles.main.duration);
		particles.Pause();
	}
}
