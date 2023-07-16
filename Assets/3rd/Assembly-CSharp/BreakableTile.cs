using System.Collections;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BreakableTile : BaseMonoBehaviour
{
	private Health health;

	public Transform[] tToShake;

	public List<Vector2> tShakes;

	public SpriteRenderer[] spriteRenderers;

	public bool PlayHitSoundsInOrder;

	private Collider2D BoxCollider;

	public SoundConstants.SoundMaterial soundMaterial;

	private void OnEnable()
	{
		health = GetComponent<Health>();
		if (health != null)
		{
			health.OnDie += OnDie;
			health.OnHit += OnHit;
		}
	}

	private void Start()
	{
		tShakes = new List<Vector2>();
		Transform[] array = tToShake;
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform2 = array[i];
			tShakes.Add(Vector2.zero);
		}
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		BoxCollider = GetComponent<BoxCollider2D>();
	}

	public virtual void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (soundMaterial != 0)
		{
			AudioManager.Instance.PlayOneShot(SoundConstants.GetImpactSoundPathForMaterial(soundMaterial), base.transform.position);
		}
		FlashRed();
		int num = -1;
		while (++num < tShakes.Count)
		{
			tShakes[num] = new Vector2(Random.Range(-0.5f, 0.5f), 0f);
		}
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, Attacker.transform.position), false);
		HitFX();
	}

	public void HitFX()
	{
		BiomeConstants.Instance.EmitHitVFX(base.transform.position + Vector3.back, Quaternion.identity.z, "HitFX_Weak");
	}

	public void FlashRed()
	{
		StopCoroutine(DoFlashRed());
		StartCoroutine(DoFlashRed());
	}

	private IEnumerator DoFlashRed()
	{
		float Progress = 0f;
		SpriteRenderer[] array;
		while (true)
		{
			float num;
			Progress = (num = Progress + 0.1f);
			if (!(num <= 1f))
			{
				break;
			}
			array = spriteRenderers;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (spriteRenderer != null)
				{
					spriteRenderer.color = Color.Lerp(Color.red, Color.white, Progress);
				}
			}
			yield return null;
		}
		array = spriteRenderers;
		foreach (SpriteRenderer spriteRenderer2 in array)
		{
			if (spriteRenderer2 != null)
			{
				spriteRenderer2.color = Color.white;
			}
		}
	}

	private void Update()
	{
		int num = -1;
		while (++num < tShakes.Count)
		{
			Vector2 value = tShakes[num];
			value.y += (0f - value.x) * 0.3f;
			value.x += (value.y *= 0.7f);
			tShakes[num] = value;
		}
		num = -1;
		while (++num < tToShake.Length)
		{
			if (tToShake[num] != null)
			{
				tToShake[num].localPosition = new Vector3(tShakes[num].x, tToShake[num].localPosition.y, tToShake[num].localPosition.z);
			}
		}
	}

	public virtual void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (soundMaterial != 0)
		{
			AudioManager.Instance.PlayOneShot(SoundConstants.GetBreakSoundPathForMaterial(soundMaterial), base.transform.position);
		}
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(Attacker.transform.position, base.transform.position), false);
		if (BoxCollider != null)
		{
			Bounds bounds = BoxCollider.bounds;
			BoxCollider.enabled = false;
			AstarPath.active.UpdateGraphs(bounds);
		}
	}

	private void OnDestroy()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
			health.OnHit -= OnHit;
		}
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
			health.OnHit -= OnHit;
		}
	}

	public void SetParentToRoom(Transform transform)
	{
		transform.parent = BiomeGenerator.Instance.CurrentRoom.generateRoom.transform;
	}
}
