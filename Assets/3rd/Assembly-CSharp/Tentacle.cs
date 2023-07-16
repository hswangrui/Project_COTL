using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;

public class Tentacle : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	public SpriteRenderer WarningCircle;

	private Health health;

	private StateMachine.State CurrentState;

	private CircleCollider2D CircleCollider2D;

	private const string SHADER_COLOR_NAME = "_Color";

	private bool DoWarning = true;

	private int Order;

	private bool PlaySound;

	private float damage;

	public Health.AttackFlags AttackFlags;

	private float shootTimestamp;

	public static List<Health> TotalDamagedEnemies = new List<Health>();

	public CircleCollider2D DamageCollider;

	public Collider2D UnitLayerCollider;

	private Health EnemyHealth;

	private EventInstance loop;

	private List<Health> DamagedEnemies = new List<Health>();

	public bool ShootsProjectiles { get; set; }

	public float TimeBetweenProjectiles { get; set; } = 8f;


	private void OnEnable()
	{
		CircleCollider2D = GetComponent<CircleCollider2D>();
		health = GetComponent<Health>();
		health.OnDie += OnDie;
		health.invincible = true;
		shootTimestamp = Time.time;
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(loop);
		health.OnDie -= OnDie;
		Object.Destroy(base.gameObject);
	}

	public void Play(float Delay, float Duration, float damage, Health.Team Team, bool DoWarning, int Order, bool PlaySound, bool continousDamage = false)
	{
		health.team = Team;
		this.DoWarning = DoWarning;
		this.Order = Order;
		this.PlaySound = PlaySound;
		this.damage = damage;
		if ((bool)Spine)
		{
			Spine.gameObject.SetActive(false);
		}
		StartCoroutine(Attack(Delay, Duration, continousDamage));
	}

	public void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (CurrentState != StateMachine.State.Dieing)
		{
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
			StopAllCoroutines();
			StartCoroutine(Die());
			CircleCollider2D.enabled = false;
		}
	}

	private void Update()
	{
		if (Time.time > shootTimestamp && ShootsProjectiles)
		{
			StartCoroutine(FireSpiralProjectiles());
			shootTimestamp = Time.time + TimeBetweenProjectiles;
			AudioManager.Instance.PlayOneShot("event:/enemy/shoot_magicenergy", Spine.gameObject);
			if ((bool)Spine)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/tentacle_slime", Spine.gameObject);
			}
			Spine.AnimationState.SetAnimation(0, "attack", false).Delay = 0f;
			Spine.AnimationState.AddAnimation(0, "idle" + ((Random.Range(0, 2) == 0) ? "2" : ""), true, 0f);
		}
	}

	private IEnumerator FireSpiralProjectiles()
	{
		int TotalProjectiles = 8;
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < TotalProjectiles)
			{
				Projectile.CreatePlayerProjectiles(1, health, base.transform.position, "Assets/Prefabs/Enemies/Weapons/ArrowPlayer.prefab", 16f, PlayerWeapon.GetDamage(0.5f, DataManager.Instance.CurrentWeaponLevel), 360f / (float)TotalProjectiles * (float)i, false, null, AttackFlags);
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private IEnumerator Die()
	{
		CurrentState = StateMachine.State.Dieing;
		if ((bool)Spine)
		{
			Spine.AnimationState.SetAnimation(0, "out", false);
			AudioManager.Instance.PlayOneShot("event:/relics/tentacle_exit", base.gameObject);
		}
		yield return new WaitForSeconds(0.6f);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator Attack(float Delay, float Duration, bool continousDamage)
	{
		UnitLayerCollider.enabled = false;
		DamageCollider.enabled = false;
		float Scale = 0f;
		WarningCircle.transform.localScale = Vector3.zero;
		if (DoWarning)
		{
			float WarningDelay = 0.5f;
			float flashTickTimer = 0f;
			while (true)
			{
				float num;
				WarningDelay = (num = WarningDelay - Time.deltaTime);
				if (num > 0f)
				{
					Scale += Time.deltaTime;
					WarningCircle.transform.localScale = Vector3.one * Scale;
					if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
					{
						WarningCircle.material.SetColor("_Color", (WarningCircle.material.GetColor("_Color") == Color.red) ? Color.white : Color.red);
						flashTickTimer = 0f;
					}
					flashTickTimer += Time.deltaTime;
					yield return null;
					continue;
				}
				break;
			}
			while (true)
			{
				float num;
				Delay = (num = Delay - Time.deltaTime);
				if (num > 0f)
				{
					if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
					{
						WarningCircle.material.SetColor("_Color", (WarningCircle.material.GetColor("_Color") == Color.red) ? Color.white : Color.red);
						flashTickTimer = 0f;
					}
					flashTickTimer += Time.deltaTime;
					yield return null;
					continue;
				}
				break;
			}
		}
		else
		{
			yield return new WaitForSeconds(Delay);
		}
		if ((bool)Spine)
		{
			loop = AudioManager.Instance.CreateLoop("event:/relics/cursed_fire", base.gameObject, true);
			Spine.gameObject.SetActive(true);
			Spine.AnimationState.SetAnimation(0, "intro", false);
			Spine.AnimationState.AddAnimation(0, "idle" + ((Random.Range(0, 2) == 0) ? "2" : ""), true, 0f);
			Spine.AnimationState.TimeScale = 2f;
		}
		health.invincible = false;
		if (Duration == -1f)
		{
			yield break;
		}
		CameraManager.shakeCamera(0.2f, Random.Range(0, 360));
		float IntroDuration = 0.2f;
		DamageCollider.enabled = true;
		DamagedEnemies = new List<Health>();
		while (true)
		{
			float num;
			IntroDuration = (num = IntroDuration - Time.deltaTime);
			if (!(num > 0f))
			{
				break;
			}
			Scale -= Time.deltaTime * 2f;
			if (Scale >= 0f)
			{
				WarningCircle.transform.localScale = Vector3.one * Scale;
			}
			else
			{
				WarningCircle.gameObject.SetActive(false);
			}
			yield return null;
		}
		if ((bool)Spine)
		{
			Spine.AnimationState.TimeScale = 1f;
		}
		UnitLayerCollider.enabled = true;
		DealDamage();
		float t = 0f;
		float resetEnemiesTimer = 0f;
		while (t < Duration)
		{
			t += Time.deltaTime;
			resetEnemiesTimer += Time.deltaTime;
			if (continousDamage)
			{
				if (resetEnemiesTimer > 1f)
				{
					foreach (Health damagedEnemy in DamagedEnemies)
					{
						TotalDamagedEnemies.Remove(damagedEnemy);
					}
					DamagedEnemies.Clear();
					resetEnemiesTimer = 0f;
				}
				DealDamage();
			}
			yield return null;
		}
		UnitLayerCollider.enabled = false;
		DamageCollider.enabled = false;
		if ((bool)Spine)
		{
			AudioManager.Instance.StopLoop(loop);
			AudioManager.Instance.PlayOneShot("event:/relics/tentacle_exit", base.gameObject);
			Spine.AnimationState.SetAnimation(0, "out", false);
		}
		yield return new WaitForSeconds(0.9f);
		foreach (Health damagedEnemy2 in DamagedEnemies)
		{
			TotalDamagedEnemies.Remove(damagedEnemy2);
		}
		Object.Destroy(base.gameObject);
	}

	private void DealDamage()
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, CircleCollider2D.radius);
		foreach (Collider2D collider2D in array)
		{
			EnemyHealth = collider2D.gameObject.GetComponent<Health>();
			if (EnemyHealth != null && EnemyHealth.team != health.team && !TotalDamagedEnemies.Contains(EnemyHealth))
			{
				if ((bool)Spine && EnemyHealth.team == Health.Team.Team2)
				{
					AudioManager.Instance.PlayOneShot("event:/relics/tentacle_slime", Spine.gameObject);
					Spine.AnimationState.SetAnimation(0, "attack", false).Delay = 0f;
					Spine.AnimationState.AddAnimation(0, "idle" + ((Random.Range(0, 2) == 0) ? "2" : ""), true, 0f);
				}
				EnemyHealth.DealDamage(damage, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f), false, Health.AttackTypes.Projectile, false, AttackFlags);
				DamagedEnemies.Add(EnemyHealth);
				TotalDamagedEnemies.Add(EnemyHealth);
			}
		}
	}
}
