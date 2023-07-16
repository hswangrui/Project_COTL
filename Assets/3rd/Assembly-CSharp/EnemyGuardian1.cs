using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using I2.Loc;
using UnityEngine;

public class EnemyGuardian1 : UnitObject
{
	private GameObject TargetObject;

	private Health EnemyHealth;

	private bool Active = true;

	public ColliderEvents damageColliderEvents;

	private SimpleSpineAnimator simpleSpineAnimator;

	private SimpleSpineEventListener simpleSpineEventListener;

	private List<Collider2D> collider2DList;

	public Transform CameraBone;

	public ParticleSystem Particles;

	public ParticleSystem DashParticles;

	public EnemyGuardian2 Guardian2;

	private Collider2D HealthCollider;

	private Vector3 CentreOfLevel;

	public GameObject CenterOfLevelObject;

	[TermsPopup("")]
	public string DisplayName;

	[EventRef]
	public string attackSoundPath = string.Empty;

	[EventRef]
	public string onHitSoundPath = string.Empty;

	private bool active;

	private int PlayCoughing;

	public DeadBodySliding deadBodySliding;

	public GameObject Trap;

	private int TrapPattern;

	public GameObject[] Enemies;

	public LineRenderer lineRenderer;

	private int TargetBats = 2;

	private List<GameObject> spawnedEnemies = new List<GameObject>();

	private Vector3 TargetPosition
	{
		get
		{
			return TargetObject.transform.position;
		}
	}

	public override void Awake()
	{
		base.Awake();
		InitializeTraps();
		health.BlackSoulOnHit = true;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		PlayerFarming instance = PlayerFarming.Instance;
		TargetObject = (((object)instance != null) ? instance.gameObject : null);
		if (!GameManager.RoomActive)
		{
			return;
		}
		if (!active)
		{
			simpleSpineAnimator = GetComponentInChildren<SimpleSpineAnimator>();
			simpleSpineEventListener = GetComponent<SimpleSpineEventListener>();
			simpleSpineEventListener.OnSpineEvent += OnSpineEvent;
			Particles.Stop();
			state.facingAngle = 180f;
			HealthCollider = GetComponent<Collider2D>();
			HealthCollider.enabled = false;
			health.invincible = true;
			if (damageColliderEvents != null)
			{
				damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
				damageColliderEvents.SetActive(false);
			}
			active = true;
		}
		else
		{
			Particles.Stop();
			health.invincible = false;
			HealthCollider.enabled = true;
			StartCoroutine(FightPlayer());
			UIBossHUD.Play(health, LocalizationManager.GetTranslation(DisplayName));
		}
		AudioManager.Instance.SetMusicRoomID(1, "deathcat_room_id");
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (!string.IsNullOrEmpty(onHitSoundPath))
		{
			AudioManager.Instance.PlayOneShot(onHitSoundPath, base.transform.position);
		}
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		simpleSpineAnimator.FlashFillRed();
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.SetMusicRoomID(0, "deathcat_room_id");
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		simpleSpineAnimator.FlashWhite(false);
		lineRenderer.gameObject.SetActive(false);
		StopAllCoroutines();
		StartCoroutine(Die());
		GetComponent<Collider2D>().enabled = false;
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		UIBossHUD.Hide();
		foreach (GameObject spawnedEnemy in spawnedEnemies)
		{
			if ((bool)spawnedEnemy)
			{
				Health component = spawnedEnemy.GetComponent<Health>();
				component.enabled = true;
				component.invincible = false;
				component.DealDamage(float.MaxValue, component.gameObject, component.transform.position);
			}
		}
	}

	private IEnumerator Die()
	{
		state.CURRENT_STATE = StateMachine.State.Dead;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraBone.gameObject, 7f);
		yield return new WaitForEndOfFrame();
		simpleSpineAnimator.Animate("dead", 0, false);
		yield return new WaitForSeconds(4f);
		if (UnityEngine.Random.value < 0.33f)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 1, base.transform.position + Vector3.back);
		}
		else
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.RED_HEART, 1, base.transform.position + Vector3.back);
		}
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
		GameManager.GetInstance().OnConversationEnd();
		UnityEngine.Object.Destroy(base.gameObject);
		Guardian2.Activate();
	}

	private IEnumerator Coughing()
	{
		simpleSpineAnimator.Animate("Coughing", 0, true);
		yield return new WaitForSeconds(1f);
		simpleSpineAnimator.Animate("idle", 0, true);
		StartCoroutine(SpawnTraps());
	}

	private void Start()
	{
		CentreOfLevel = CenterOfLevelObject.transform.position;
	}

	private void OnSpineEvent(string EventName)
	{
		switch (EventName)
		{
		case "Intro Complete":
			GameManager.GetInstance().OnConversationEnd();
			health.invincible = false;
			HealthCollider.enabled = true;
			StartCoroutine(FightPlayer());
			AudioManager.Instance.SetMusicRoomID(1, "deathcat_room_id");
			UIBossHUD.Play(health, LocalizationManager.GetTranslation(DisplayName));
			GameManager.GetInstance().AddToCamera(base.gameObject);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			break;
		case "Start Particles":
			CameraManager.instance.ShakeCameraForDuration(0.1f, 0.4f, 2.5f);
			Particles.Play();
			break;
		case "Stop Particles":
			Particles.Stop();
			break;
		case "Invincible Off":
			break;
		}
	}

	public void Play()
	{
		Debug.Log("PLAY!");
		StartCoroutine(Activate());
	}

	private IEnumerator Activate()
	{
		Debug.Log("Activate!");
		GameManager.GetInstance().OnConversationNext(CameraBone.gameObject);
		yield return new WaitForSeconds(0.5f);
		HUD_DisplayName.Play(LocalizationManager.GetTranslation(DisplayName), 2, HUD_DisplayName.Positions.Centre, HUD_DisplayName.textBlendMode.DungeonFinal);
		simpleSpineAnimator.Animate("intro", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
	}

	public override void Update()
	{
		base.Update();
		foreach (GameObject spawnedEnemy in spawnedEnemies)
		{
			if (spawnedEnemy == null)
			{
				spawnedEnemies.Remove(spawnedEnemy);
				break;
			}
		}
	}

	private IEnumerator FightPlayer()
	{
		while (TargetObject == null)
		{
			EnemyGuardian1 enemyGuardian = this;
			PlayerFarming instance = PlayerFarming.Instance;
			enemyGuardian.TargetObject = (((object)instance != null) ? instance.gameObject : null);
			yield return null;
		}
		givePath(TargetObject.transform.position);
		float RepathTimer = 0f;
		int NumAttacks = 3;
		float AttackSpeed = 25f;
		bool Loop = true;
		while (Loop)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Moving:
				if (Vector2.Distance(base.transform.position, TargetPosition) < 3f)
				{
					state.CURRENT_STATE = StateMachine.State.SignPostAttack;
					simpleSpineAnimator.Animate("attack" + (4 - NumAttacks), 0, false);
					simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
				}
				else
				{
					float num2;
					RepathTimer = (num2 = RepathTimer + Time.deltaTime);
					if (num2 > 0.2f)
					{
						RepathTimer = 0f;
						givePath(TargetObject.transform.position);
					}
				}
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				break;
			case StateMachine.State.SignPostAttack:
				state.facingAngle = Utils.GetAngle(base.transform.position, TargetPosition);
				if ((state.Timer += Time.deltaTime) >= 0.5f)
				{
					StartCoroutine(EnableCollider());
					CameraManager.shakeCamera(0.4f, state.facingAngle);
					state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
					speed = AttackSpeed * Time.deltaTime;
					if (!string.IsNullOrEmpty(attackSoundPath))
					{
						AudioManager.Instance.PlayOneShot(attackSoundPath, base.transform.position);
					}
				}
				if (damageColliderEvents != null)
				{
					damageColliderEvents.SetActive(false);
				}
				break;
			case StateMachine.State.RecoverFromAttack:
				if (AttackSpeed > 0f)
				{
					AttackSpeed -= 1f * GameManager.DeltaTime;
				}
				speed = AttackSpeed * Time.deltaTime;
				if ((state.Timer += Time.deltaTime) >= 0.5f)
				{
					int num = NumAttacks - 1;
					NumAttacks = num;
					if (num > 0)
					{
						AttackSpeed = 25 + (3 - NumAttacks) * 2;
						state.CURRENT_STATE = StateMachine.State.SignPostAttack;
						simpleSpineAnimator.Animate("attack" + (4 - NumAttacks), 0, false);
						simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
					}
					else
					{
						Loop = false;
						state.CURRENT_STATE = StateMachine.State.Idle;
					}
				}
				break;
			}
			yield return null;
		}
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		if (UnityEngine.Random.Range(0, 4) < 3)
		{
			StartCoroutine(SpawnTraps());
		}
		else
		{
			StartCoroutine(FightPlayer());
		}
	}

	private IEnumerator EnableCollider()
	{
		yield return new WaitForSeconds(0.1f);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(true);
		}
		yield return new WaitForSeconds(0.1f);
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
	}

	private IEnumerator SpawnTraps()
	{
		if (active)
		{
			simpleSpineAnimator.Animate("summon", 0, false);
			simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			yield return new WaitForSeconds(1.7f);
			TrapPattern = UnityEngine.Random.Range(0, 3);
			switch (TrapPattern)
			{
			case 0:
				yield return StartCoroutine(TrapPattern0());
				break;
			case 1:
				yield return StartCoroutine(TrapPatternChasePlayer());
				break;
			case 2:
				yield return StartCoroutine(TrapPattern1());
				break;
			}
			yield return new WaitForSeconds(0.5f);
			if (spawnedEnemies.Count <= 1)
			{
				StartCoroutine(SpawnEnemies());
			}
			else
			{
				StartCoroutine(FightPlayer());
			}
		}
	}

	private void InitializeTraps()
	{
		ObjectPool.CreatePool(Trap, 40);
	}

	private IEnumerator TrapPattern0()
	{
		if (!active)
		{
			yield break;
		}
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < 10)
			{
				int num2 = -1;
				while (++num2 < 4)
				{
					GameObject obj = ObjectPool.Spawn(Trap);
					float f = (state.facingAngle + (float)(90 * num2)) * ((float)Math.PI / 180f);
					float num3 = i * 2;
					Vector3 vector = new Vector3(num3 * Mathf.Cos(f), num3 * Mathf.Sin(f));
					obj.transform.position = base.transform.position + vector;
				}
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private IEnumerator TrapPattern1()
	{
		if (!active)
		{
			yield break;
		}
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < 10)
			{
				int num2 = -1;
				while (++num2 < 1)
				{
					state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
					GameObject obj = ObjectPool.Spawn(Trap);
					float f = (state.facingAngle + (float)(90 * num2)) * ((float)Math.PI / 180f);
					float num3 = 2 + i * 2;
					Vector3 vector = new Vector3(num3 * Mathf.Cos(f), num3 * Mathf.Sin(f));
					obj.transform.position = base.transform.position + vector;
				}
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private IEnumerator TrapPattern2()
	{
		if (!active)
		{
			yield break;
		}
		Vector3 centreOfLevel = CentreOfLevel;
		int i = -1;
		float Dist = 1f;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		float Angle = state.facingAngle;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < 20)
			{
				GameObject obj = ObjectPool.Spawn(Trap);
				Angle += 10f;
				Dist += 0.5f;
				Vector3 vector = new Vector3(Dist * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Dist * Mathf.Sin(Angle * ((float)Math.PI / 180f)));
				obj.transform.position = base.transform.position + vector;
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private IEnumerator TrapPatternChasePlayer()
	{
		if (!active)
		{
			yield break;
		}
		Vector3 Position = CentreOfLevel;
		int i = -1;
		float Dist = 1f;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		float facingAngle = state.facingAngle;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < 20)
			{
				GameObject obj = ObjectPool.Spawn(Trap);
				float angle = Utils.GetAngle(base.transform.position + Position, TargetObject.transform.position);
				Position += new Vector3(Dist * Mathf.Cos(angle * ((float)Math.PI / 180f)), Dist * Mathf.Sin(angle * ((float)Math.PI / 180f)));
				obj.transform.position = base.transform.position + Position;
				CameraManager.shakeCamera(0.4f, UnityEngine.Random.Range(0, 360));
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private IEnumerator ShowLineRenderer()
	{
		lineRenderer.gameObject.SetActive(true);
		lineRenderer.SetPosition(0, base.transform.position + Vector3.back * 1f);
		lineRenderer.SetPosition(1, CentreOfLevel + Vector3.back * 1f);
		float Progress = 1f;
		Color c = Color.white;
		Gradient gradient = new Gradient();
		while (true)
		{
			float num;
			Progress = (num = Progress - Time.deltaTime);
			if (!(num >= 0f))
			{
				break;
			}
			if (Progress < 0f)
			{
				Progress = 0f;
			}
			gradient.SetKeys(new GradientColorKey[2]
			{
				new GradientColorKey(c, 0f),
				new GradientColorKey(c, 1f)
			}, new GradientAlphaKey[2]
			{
				new GradientAlphaKey(Progress, 0f),
				new GradientAlphaKey(Progress, 1f)
			});
			lineRenderer.colorGradient = gradient;
			yield return null;
		}
		lineRenderer.gameObject.SetActive(false);
	}

	private IEnumerator SpawnEnemies()
	{
		yield return new WaitForSeconds(1f);
		if (!active)
		{
			yield break;
		}
		simpleSpineAnimator.Animate("dash", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(0.15f);
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, CentreOfLevel));
		BiomeConstants.Instance.SpawnInWhite.Spawn().transform.position = base.transform.position + Vector3.down * 2f;
		base.transform.position = CentreOfLevel;
		BiomeConstants.Instance.SpawnInWhite.Spawn().transform.position = base.transform.position + Vector3.down * 2f;
		yield return new WaitForSeconds(1f);
		simpleSpineAnimator.Animate("floating-start", 0, true);
		simpleSpineAnimator.AddAnimate("floating-spin", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(1.5f);
		int Count = -1;
		while (true)
		{
			int num = Count + 1;
			Count = num;
			if (num >= TargetBats)
			{
				break;
			}
			GameObject gameObject = EnemySpawner.Create(new Vector3(2f * Mathf.Cos((float)(Count * (360 / TargetBats)) * ((float)Math.PI / 180f)), 2f * Mathf.Sin((float)(Count * (360 / TargetBats)) * ((float)Math.PI / 180f))), base.transform.parent, Enemies[UnityEngine.Random.Range(0, Enemies.Length)]);
			gameObject.GetComponent<UnitObject>().CanHaveModifier = false;
			gameObject.GetComponent<UnitObject>().RemoveModifier();
			spawnedEnemies.Add(gameObject);
			yield return new WaitForSeconds(0.1f);
		}
		if (TargetBats < 3)
		{
			TargetBats++;
		}
		yield return new WaitForSeconds(0.5f);
		simpleSpineAnimator.AddAnimate("floating", 0, true, 0f);
		simpleSpineAnimator.Animate("floating-stop", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(1f);
		StartCoroutine(FightPlayer());
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != health.team)
		{
			Debug.Log("OnDamageTriggerEnter".Colour(Color.red));
			component.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(CentreOfLevel, 0.4f, Color.blue);
	}
}
