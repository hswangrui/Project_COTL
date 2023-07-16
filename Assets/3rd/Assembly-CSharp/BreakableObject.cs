using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BreakableObject : BaseMonoBehaviour
{
	public bool destroyOnWalk = true;

	public bool destroyOnRoll;

	public bool destroyOnAttack = true;

	public bool shakeCamera = true;

	public bool emitSmokeExplosion = true;

	public bool emitGroundSmashDecal;

	public float Radius = 1f;

	public Vector3 RadiusOffset = Vector3.zero;

	public bool Seperate = true;

	public List<GameObject> gameObjectsToDisable = new List<GameObject>();

	public SoundConstants.SoundMaterial soundMaterial;

	public BiomeConstants.TypeOfParticle Type;

	public float zSpawn = 0.5f;

	public float CameraShake = 2f;

	public int maxParticles = 20;

	private float RandomVariation = 0.5f;

	public float VelocityMultiplyer = 1f;

	public float ParticleMultiplyer = 1f;

	public static List<BreakableObject> BreakableObjects = new List<BreakableObject>();

	private bool Activated;

	private float CheckDistance;

	private Vector3 SeperatorVelocity;

	public bool hasHealth;

	public int HP;

	public int TotalHP = 6;

	public bool ShowGizmos;

	public bool TESTDEBUG = true;

	private Vector2 Velocity;

	public bool ShakeObject = true;

	public float shakeDuration = 0.5f;

	public Vector3 shakeStrength = new Vector3(0.5f, 0.5f, 0.01f);

	[Range(0f, 30f)]
	public int vibrato = 10;

	[Range(0f, 180f)]
	public float randomness = 90f;

	private bool ActivatedHit;

	private void OnEnable()
	{
		BreakableObjects.Add(this);
	}

	private void OnDisable()
	{
		BreakableObjects.Remove(this);
	}

	private void OnDrawGizmos()
	{
		if (ShowGizmos)
		{
			Utils.DrawCircleXY(base.transform.position + RadiusOffset, Radius, Color.green);
		}
	}

	private void Start()
	{
		HP = TotalHP;
	}

	private void Update()
	{
		if (Activated || ActivatedHit)
		{
			return;
		}
		foreach (Health allUnit in Health.allUnits)
		{
			if ((allUnit.team == Health.Team.PlayerTeam || allUnit.team == Health.Team.Team2) && !(allUnit == null))
			{
				CheckDistance = Vector3.Distance(allUnit.transform.position, base.transform.position + RadiusOffset);
				if (CheckDistance <= Radius && destroyOnWalk)
				{
					Collide(allUnit);
				}
				else if (allUnit.state != null && CheckDistance <= Radius && allUnit.state.CURRENT_STATE == StateMachine.State.Dodging && destroyOnRoll)
				{
					Collide(allUnit);
				}
				else if (allUnit.state != null && (allUnit.state.CURRENT_STATE == StateMachine.State.RecoverFromAttack || allUnit.state.CURRENT_STATE == StateMachine.State.Attacking) && CheckDistance <= Radius + 1f && Mathf.Abs(Utils.GetAngle(allUnit.transform.position, base.transform.position + RadiusOffset) - allUnit.state.facingAngle) < 90f && destroyOnAttack)
				{
					Collide(allUnit);
				}
			}
		}
		if (Seperate)
		{
			SeperateObject();
		}
	}

	private void SeperateObject()
	{
		SeperatorVelocity = Vector3.zero;
		foreach (BreakableObject breakableObject in BreakableObjects)
		{
			if (breakableObject == null || breakableObject == this)
			{
				continue;
			}
			float num = Vector2.Distance(breakableObject.transform.position + breakableObject.RadiusOffset, base.transform.position + RadiusOffset);
			if (num < Radius + breakableObject.Radius)
			{
				float f = Utils.GetAngle(breakableObject.transform.position + breakableObject.RadiusOffset, base.transform.position + RadiusOffset) * ((float)Math.PI / 180f);
				if (TESTDEBUG)
				{
					SeperatorVelocity.x += (breakableObject.Radius + Radius - num) * Mathf.Cos(f);
					SeperatorVelocity.y += (breakableObject.Radius + Radius - num) * Mathf.Sin(f);
				}
				else
				{
					SeperatorVelocity.x += (breakableObject.Radius - num) * Mathf.Cos(f);
					SeperatorVelocity.y += (breakableObject.Radius - num) * Mathf.Sin(f);
				}
			}
		}
		base.transform.position = base.transform.position + SeperatorVelocity;
	}

	private void Collide(Health h)
	{
		StopAllCoroutines();
		if (!hasHealth)
		{
			StartCoroutine(CollideRoutine(h));
			return;
		}
		if (HP <= 0)
		{
			StartCoroutine(CollideRoutine(h));
			return;
		}
		HP--;
		StartCoroutine(HitRoutine(h));
	}

	private IEnumerator HitRoutine(Health h)
	{
		ActivatedHit = true;
		if (h != null)
		{
			Vector3 LastPos = h.transform.position;
			yield return null;
			Velocity = (h.transform.position - LastPos) * 50f;
		}
		if (ShakeObject)
		{
			foreach (GameObject item in gameObjectsToDisable)
			{
				item.transform.DORestart();
				item.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato, randomness);
			}
		}
		if (soundMaterial != 0)
		{
			AudioManager.Instance.PlayOneShot(SoundConstants.GetImpactSoundPathForMaterial(soundMaterial), base.transform.position);
		}
		if (shakeCamera)
		{
			CameraManager.shakeCamera(CameraShake / 1.5f, Utils.GetAngle(base.transform.position, base.transform.position), false);
		}
		BiomeConstants.Instance.EmitParticleChunk(Type, base.transform.position, Velocity, maxParticles / 4, ParticleMultiplyer);
		yield return new WaitForSeconds(shakeDuration);
		ActivatedHit = false;
	}

	private IEnumerator CollideRoutine(Health h)
	{
		Activated = true;
		Vector3 LastPos = h.transform.position;
		yield return null;
		Velocity = (h.transform.position - LastPos) * 50f;
		foreach (GameObject item in gameObjectsToDisable)
		{
			item.SetActive(false);
		}
		if (soundMaterial != 0)
		{
			AudioManager.Instance.PlayOneShot(SoundConstants.GetBreakSoundPathForMaterial(soundMaterial), base.transform.position);
		}
		if (shakeCamera)
		{
			CameraManager.shakeCamera(CameraShake, Utils.GetAngle(base.transform.position, base.transform.position), false);
		}
		if (emitSmokeExplosion)
		{
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * zSpawn);
		}
		if (emitGroundSmashDecal)
		{
			BiomeConstants.Instance.EmitGroundSmashVFXParticles(base.transform.position);
		}
		BiomeConstants.Instance.EmitParticleChunk(Type, base.transform.position, Velocity * VelocityMultiplyer, maxParticles, ParticleMultiplyer);
	}
}
