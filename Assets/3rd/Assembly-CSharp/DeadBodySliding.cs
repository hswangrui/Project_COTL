using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBodySliding : BaseMonoBehaviour
{
	private Rigidbody2D rb2d;

	private float Speed = 1700f;

	private Vector2 ScaleX;

	private Vector2 ScaleY;

	private Vector2 StartScale = new Vector2(3f, 3f);

	public Renderer sprite;

	private float ColorProgress;

	private Vector2 Force;

	public CircleCollider2D circleCollider;

	private Health health;

	private Material material;

	private const string SHADER_COLOR_NAME = "_Color";

	public static List<DeadBodySliding> DeadBodies = new List<DeadBodySliding>();

	public LayerMask hitMask;

	private float _Z;

	private float Zv;

	private bool explode;

	public Health Health
	{
		get
		{
			return health;
		}
	}

	private float Z
	{
		get
		{
			return _Z;
		}
		set
		{
			_Z = value;
			if ((bool)sprite)
			{
				sprite.transform.localPosition = Vector3.forward * Z;
			}
		}
	}

	private void Start()
	{
		if (sprite != null)
		{
			sprite.receiveShadows = true;
		}
		hitMask = (int)hitMask | (1 << LayerMask.NameToLayer("Obstacles"));
		hitMask = (int)hitMask | (1 << LayerMask.NameToLayer("Island"));
	}

	private void OnEnable()
	{
		health = GetComponentInChildren<Health>();
		health.DestroyOnDeath = false;
		material = sprite.material;
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Combine(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(OnChestRevealed));
		DeadBodies.Add(this);
	}

	private void OnDisable()
	{
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(OnChestRevealed));
		DeadBodies.Remove(this);
	}

	private void OnChestRevealed()
	{
		if (!explode && DataManager.Instance.BonesEnabled && DataManager.Instance.CurrentRelic != RelicType.SpawnCombatFollowerFromBodies)
		{
			health.DealDamage(2.1474836E+09f, base.gameObject, base.transform.position);
		}
	}

	public void Init(GameObject enemy, float Angle, float Speed = 1700f, bool explode = false)
	{
		rb2d = GetComponent<Rigidbody2D>();
		Force = new Vector2(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f)));
		ScaleX = new Vector2(1f, 0f);
		ScaleY = new Vector2(1f, 0f);
		if (sprite != null)
		{
			sprite.transform.localScale = new Vector3(ScaleX.x, ScaleY.x, 1f);
		}
		Z = -0.5f;
		Zv = -3f;
		ColorProgress = 0f;
		SetFacing();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(DelayForce());
			StartCoroutine(DoScale());
			StartCoroutine(DoBounce());
			StartCoroutine(HealthRoutine());
		}
		if (GetComponentInChildren<Health>(true) != null)
		{
			GetComponentInChildren<Health>(true).OnEnable();
		}
		this.explode = explode;
		if (explode)
		{
			StartCoroutine(ExplodeRoutine());
		}
	}

	private IEnumerator ExplodeRoutine()
	{
		float flashTickTimer = 0f;
		float explodeDelay = 1f;
		Color color = Color.white;
		material.SetColor("_Color", Color.white);
		yield return new WaitForSeconds(1f);
		while (true)
		{
			float num;
			explodeDelay = (num = explodeDelay - Time.deltaTime);
			if (!(num > 0f))
			{
				break;
			}
			if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				Material obj = material;
				Color value;
				color = (value = ((color == Color.white) ? new Color(1f, 1f, 1f, 0f) : Color.white));
				obj.SetColor("_Color", value);
				flashTickTimer = 0f;
			}
			flashTickTimer += Time.deltaTime;
			yield return null;
		}
		material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
		Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, health, 4f);
		base.gameObject.Recycle();
	}

	private IEnumerator HealthRoutine()
	{
		if ((bool)health)
		{
			health.invincible = true;
			yield return new WaitForSeconds(0.3f);
			health.invincible = false;
		}
	}

	private IEnumerator DoBounce()
	{
		yield return new WaitForSeconds(0.2f);
		while (Z < 0f)
		{
			Zv += 0.5f * GameManager.DeltaTime;
			Z += Zv * Time.deltaTime;
			if (Z >= 0f && Zv > 4f)
			{
				Zv *= -0.4f;
				Z = -0.1f;
				ScaleX = new Vector2(1.5f, 0f);
				ScaleY = new Vector2(0.5f, 0f);
				if (sprite.isVisible)
				{
					CameraManager.shakeCamera(0.1f, UnityEngine.Random.Range(0, 360), false);
				}
			}
			yield return null;
		}
		Z = 0f;
		ScaleX = new Vector2(1.5f, 0f);
		ScaleY = new Vector2(0.5f, 0f);
		if (sprite.isVisible)
		{
			CameraManager.shakeCamera(0.3f, UnityEngine.Random.Range(0, 360), false);
		}
	}

	private IEnumerator DoScale()
	{
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 3f)
			{
				ScaleX.y += (1f - ScaleX.x) * 0.2f;
				ScaleX.x += (ScaleX.y *= 0.7f);
				ScaleY.y += (1f - ScaleY.x) * 0.2f;
				ScaleY.x += (ScaleY.y *= 0.7f);
				ScaleX.y += (1f - ScaleX.x) * 0.2f;
				ScaleX.x += (ScaleX.y *= 0.7f);
				ScaleY.y += (1f - ScaleY.x) * 0.2f;
				ScaleY.x += (ScaleY.y *= 0.7f);
				sprite.transform.localScale = new Vector3(ScaleX.x * -1f, ScaleY.x, 1f);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator DelayForce()
	{
		yield return new WaitForSeconds(0.2f);
		if (Physics2D.Raycast(base.transform.position, Force.normalized, 0.5f, hitMask).collider != null)
		{
			Force *= -1f;
		}
		rb2d.AddForce(Force);
		while ((ColorProgress += 0.1f) <= 1f)
		{
			if (sprite is SpriteRenderer)
			{
				((SpriteRenderer)sprite).color = Color.Lerp(Color.red, Color.white, ColorProgress);
			}
			yield return null;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		SetFacing();
		if (sprite.isVisible)
		{
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, collision.contacts[0].point), false);
		}
	}

	private void SetFacing()
	{
		if ((bool)rb2d)
		{
			if (rb2d.velocity.x < 0f)
			{
				base.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			else if (rb2d.velocity.x > 0f)
			{
				base.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}
	}

	public void OnDie()
	{
		AudioManager.Instance.PlayOneShot(SoundConstants.GetBreakSoundPathForMaterial(SoundConstants.SoundMaterial.Bone), base.transform.position);
		base.gameObject.SetActive(false);
	}
}
