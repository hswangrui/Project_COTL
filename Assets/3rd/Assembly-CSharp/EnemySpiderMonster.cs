using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Unify;
using UnityEngine;

public class EnemySpiderMonster : UnitObject
{
	private enum BombType
	{
		Web,
		Egg,
		Count
	}

	public Interaction_MonsterHeart interaction_MonsterHeart;

	public bool Mutated;

	private Vector3 StartPosition;

	private SimpleSpineEventListener simpleSpineEventListener;

	public MeshRenderer SpineMonster;

	public SpriteRenderer Shadow;

	public ColliderEvents damageColliderEvents;

	private Coroutine damageColliderCoroutine;

	public CircleCollider2D Collider;

	public GameObject CameraTarget;

	public float SeperationRadius = 0.5f;

	private GameObject TargetObject;

	public Vector2 Range = new Vector2(6f, 3f);

	public float KnockbackSpeed = 0.1f;

	public SimpleSpineAnimator simpleSpineAnimator;

	public SimpleSpineFlash simpleSpineFlash;

	public GameObject poisonBomb;

	public Transform BombPoint;

	public GameObject SpawnWeb;

	public GameObject SpawnSlime;

	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	public float numberOfShotsToFire = 45f;

	[SerializeField]
	private Vector2 gravSpeed;

	[SerializeField]
	private float arc;

	[SerializeField]
	private Vector2 randomArcOffset = new Vector2(0f, 0f);

	[SerializeField]
	private Vector2 shootDistanceRange = new Vector2(2f, 3f);

	[SerializeField]
	private GameObject ShootBone;

	[Space]
	public ProjectilePattern LandProjectilePattern;

	[SerializeField]
	private GameObject followerToSpawn;

	private List<SpiderNest> spiderNests = new List<SpiderNest>();

	private bool active;

	private bool juicedForm;

	private Health EnemyHealth;

	private float FireWebsDelay;

	private BombType CurrentBombType;

	private int Ammo;

	private float ChargeDelay;

	private bool JumpAttacking;

	private bool JumpAroundAttacking;

	private float ZipAwayDelay;

	private Vector3 ShadowSize;

	private Coroutine cShrinkShadow;

	private float CloseRangeAttackDelay;

	private float AttackDashSpeed;

	private float AttackSpeedValue = 0.6f;

	private bool DontPlayHurtAnimation = true;

	private bool Roared;

	public override void Awake()
	{
		base.Awake();
		spiderNests = UnityEngine.Object.FindObjectsOfType<SpiderNest>().ToList();
		foreach (SpiderNest spiderNest in spiderNests)
		{
			spiderNest.Droppable = false;
		}
		juicedForm = GameManager.Layer2;
		if (juicedForm)
		{
			health.totalHP *= 1.5f;
			health.HP = health.totalHP;
			numberOfShotsToFire *= 1.5f;
			for (int i = 0; i < LandProjectilePattern.Waves.Length; i++)
			{
				LandProjectilePattern.Waves[i].Bullets = (int)((float)LandProjectilePattern.Waves[i].Bullets * 1.5f);
				LandProjectilePattern.Waves[i].AngleBetweenBullets = 360f / (float)LandProjectilePattern.Waves[i].Bullets;
				LandProjectilePattern.Waves[i].Speed *= 1.5f;
				LandProjectilePattern.Waves[i].FinishDelay /= 1.5f;
			}
		}
		health.SlowMoOnkill = false;
		InitializeMortarStrikes();
		InitializeGrenadeBullets();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (GameManager.RoomActive && active)
		{
			health.enabled = true;
			StartCoroutine(Roar());
			StartCoroutine(DelayAddCamera());
		}
	}

	private IEnumerator DelayAddCamera()
	{
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().AddToCamera(base.gameObject, 0.25f);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
	}

	private void InitializeMortarStrikes()
	{
		List<MortarBomb> list = new List<MortarBomb>();
		int num = 10;
		for (int i = 0; i < num; i++)
		{
			MortarBomb component = ObjectPool.Spawn(poisonBomb, base.transform.parent).GetComponent<MortarBomb>();
			component.destroyOnFinish = false;
			list.Add(component);
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].gameObject.Recycle();
		}
	}

	private void InitializeGrenadeBullets()
	{
		int initialPoolSize = (int)numberOfShotsToFire * 6;
		ObjectPool.CreatePool(bulletPrefab, initialPoolSize);
	}

	private void OnSpineEvent(string EventName)
	{
		switch (EventName)
		{
		case "start flash":
			break;
		case "stop flash":
			break;
		case "dash attack":
			CameraManager.shakeCamera(0.4f, state.facingAngle);
			AttackDashSpeed = AttackSpeedValue;
			break;
		case "roar shake":
			CameraManager.instance.ShakeCameraForDuration(0.1f, 0.3f, 1f);
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
			break;
		case "mutate zoom":
			GameManager.GetInstance().OnConversationNext(CameraTarget, 6f);
			break;
		case "show name":
			HUD_DisplayName.Play("Kumo");
			break;
		case "webshot":
			AudioManager.Instance.PlayOneShot("event:/enemy/shoot_acidslime", base.gameObject);
			break;
		case "battlecry":
			AudioManager.Instance.PlayOneShot("event:/enemy/shoot_acidslime", base.gameObject);
			break;
		case "die-explode":
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", base.gameObject);
			break;
		case "deal damage":
			DoDamageAttack(0f);
			break;
		case "turn off colliders":
			SeperateObject = false;
			Physics2D.IgnoreCollision(Collider, PlayerFarming.Instance.circleCollider2D, true);
			health.invincible = true;
			break;
		case "turn on colliders":
			SeperateObject = true;
			Physics2D.IgnoreCollision(Collider, PlayerFarming.Instance.circleCollider2D, false);
			health.invincible = false;
			break;
		case "shrink shadow":
			cShrinkShadow = StartCoroutine(ShrinkShadow());
			break;
		case "grow shadow":
			StartCoroutine(GrowShadow());
			break;
		case "jump attack":
			JumpAttacking = false;
			speed = 0f;
			DoDamageAttack(0f);
			AudioManager.Instance.PlayOneShot("event:/boss/spider/jump_attack_land", base.gameObject);
			if (simpleSpineAnimator.IsVisible)
			{
				CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.3f);
			}
			break;
		case "land":
			if (JumpAroundAttacking)
			{
				StartCoroutine(LandProjectilePattern.ShootIE());
				JumpAroundAttacking = false;
			}
			break;
		case "bomb":
			Debug.Log("BOMB!   " + CurrentBombType);
			AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_shoot", base.gameObject);
			switch (CurrentBombType)
			{
			case BombType.Web:
			{
				float angle = (float)(120 * Ammo) * ((float)Math.PI / 180f);
				Vector3 position2 = UnityEngine.Random.insideUnitCircle * 8f;
				MortarBomb component = ObjectPool.Spawn(poisonBomb, base.transform.parent, position2, Quaternion.identity).GetComponent<MortarBomb>();
				component.destroyOnFinish = false;
				component.Play(BombPoint.position, juicedForm ? 1.5f : 2f, Health.Team.Team2);
				Ammo++;
				break;
			}
			case BombType.Egg:
			{
				for (int i = 0; (float)i < numberOfShotsToFire; i++)
				{
					float angle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
					Vector3 position = ShootBone.transform.position;
					ObjectPool.Spawn(bulletPrefab, position, Quaternion.identity).GetComponent<GrenadeBullet>().Play(-6f, angle + UnityEngine.Random.Range(randomArcOffset.x, randomArcOffset.y), UnityEngine.Random.Range(shootDistanceRange.x, shootDistanceRange.y), UnityEngine.Random.Range(gravSpeed.x, gravSpeed.y), health.team);
				}
				break;
			}
			}
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
			break;
		}
	}

	private void DoDamageAttack(float startDelay)
	{
		if (damageColliderCoroutine != null)
		{
			StopCoroutine(damageColliderCoroutine);
		}
		damageColliderCoroutine = StartCoroutine(DoDamageAttackTimed(startDelay, 0.1f));
	}

	private IEnumerator DoDamageAttackTimed(float startDelay, float dur)
	{
		if (damageColliderEvents == null)
		{
			yield break;
		}
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num < startDelay))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.SetActive(true);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num < dur))
			{
				break;
			}
			yield return null;
		}
		damageColliderEvents.SetActive(false);
		damageColliderCoroutine = null;
	}

	private IEnumerator WaitForTarget()
	{
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 0.5f)
			{
				yield return null;
				continue;
			}
			break;
		}
		while (TargetObject == null)
		{
			TargetObject = PlayerFarming.Instance.gameObject;
			yield return null;
		}
	}

	private IEnumerator ChasePlayer()
	{
		if (!GameManager.GetInstance()._CamFollowTarget.Contains(base.gameObject))
		{
			GameManager.GetInstance().AddToCamera(base.gameObject, 0.25f);
			GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
			GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.SetActive(false);
		}
		float ActionDelay = (Roared ? UnityEngine.Random.Range(0.3f, 0.5f) : UnityEngine.Random.Range(0.5f, 1f));
		givePath(TargetObject.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(1f, 2f)));
		speed = 0f;
		float RepathTimer = 0f;
		while (true)
		{
			if (speed < maxSpeed)
			{
				speed += 0.01f * Time.deltaTime * simpleSpineAnimator.anim.timeScale;
			}
			state.facingAngle = Utils.SmoothAngle(state.facingAngle, Utils.GetAngle(base.transform.position, TargetObject.transform.position), 10f);
			float num;
			RepathTimer = (num = RepathTimer + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (num > 0.25f)
			{
				givePath(TargetObject.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(1f, 2f)));
				RepathTimer = 0f;
				AudioManager.Instance.PlayOneShot("event:/boss/spider/footstep", base.gameObject);
			}
			CloseRangeAttackDelay -= Time.deltaTime * simpleSpineAnimator.anim.timeScale;
			ZipAwayDelay -= Time.deltaTime * simpleSpineAnimator.anim.timeScale;
			ChargeDelay -= Time.deltaTime * simpleSpineAnimator.anim.timeScale;
			FireWebsDelay -= Time.deltaTime * simpleSpineAnimator.anim.timeScale;
			float num2 = Vector3.Distance(base.transform.position, TargetObject.transform.position);
			ActionDelay = (num = ActionDelay - Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (num <= 0f)
			{
				if (num2 < 5f && CloseRangeAttackDelay <= 0f)
				{
					StartCoroutine(CloseRangeAttack());
					yield break;
				}
				if (FireWebsDelay <= 0f)
				{
					StartCoroutine(FireWebs());
					yield break;
				}
				if (ZipAwayDelay <= 0f)
				{
					StartCoroutine(ZipAway());
					yield break;
				}
				if (ChargeDelay <= 0f)
				{
					break;
				}
			}
			yield return null;
		}
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			StartCoroutine(JumpAttack());
		}
		else
		{
			StartCoroutine(JumpAround());
		}
	}

	private IEnumerator FireWebs()
	{
		Ammo = 0;
		state.CURRENT_STATE = StateMachine.State.Attacking;
		if (CurrentBombType == BombType.Web)
		{
			simpleSpineAnimator.Animate("bomb-even-more", 0, false);
		}
		else
		{
			simpleSpineAnimator.Animate(Roared ? "bomb-more" : "bombs", 0, false);
		}
		simpleSpineAnimator.AddAnimate("idle-boss", 0, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_start", base.gameObject);
		float time = 0f;
		if (CurrentBombType == BombType.Web)
		{
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
				if (num < 4.3f)
				{
					yield return null;
					continue;
				}
				break;
			}
		}
		else
		{
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
				if (!(num < (Roared ? 3f : 2.3f)))
				{
					break;
				}
				yield return null;
			}
		}
		AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_end", base.gameObject);
		FireWebsDelay = 2f;
		CurrentBombType = (BombType)UnityEngine.Random.Range(0, 2);
		StartCoroutine(ChasePlayer());
	}

	private IEnumerator JumpAttack()
	{
		state.CURRENT_STATE = StateMachine.State.Attacking;
		simpleSpineAnimator.Animate("angry intro", 0, true);
		AudioManager.Instance.PlayOneShot("event:/boss/spider/angry", base.gameObject);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/boss/spider/jump_attack_jump", base.gameObject);
		int Loop = UnityEngine.Random.Range(2, 5);
		while (Loop > 0)
		{
			float num2 = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
			int num3 = 0;
			while (num3++ < 32 && (bool)Physics2D.Raycast(base.transform.position, Utils.DegreeToVector2(num2), 5f, layerToCheck))
			{
				num2 = Mathf.Repeat(num2 + (float)UnityEngine.Random.Range(0, 360), 360f);
			}
			state.facingAngle = num2;
			JumpAttacking = true;
			simpleSpineAnimator.Animate("jump-attack", 0, false);
			CameraManager.shakeCamera(0.3f, state.facingAngle);
			while (JumpAttacking)
			{
				speed = 15f * Time.deltaTime;
				yield return null;
			}
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
				if (!(num < 0.5f))
				{
					break;
				}
				yield return null;
			}
			int num4 = Loop - 1;
			Loop = num4;
			yield return null;
		}
		ChargeDelay = 1f;
		StartCoroutine(ChasePlayer());
	}

	private IEnumerator JumpAround()
	{
		state.CURRENT_STATE = StateMachine.State.Attacking;
		simpleSpineAnimator.Animate("angry intro", 0, true);
		AudioManager.Instance.PlayOneShot("event:/boss/spider/angry", base.gameObject);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num < 1f))
			{
				break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/boss/spider/jump_attack_jump", base.gameObject);
		int Loop = UnityEngine.Random.Range(2, 5);
		while (Loop > 0)
		{
			float num2 = Utils.GetAngle(base.transform.position, UnityEngine.Random.insideUnitCircle * 5f);
			int num3 = 0;
			while (num3++ < 32 && (bool)Physics2D.Raycast(base.transform.position, Utils.DegreeToVector2(num2), 5f, layerToCheck))
			{
				num2 = Mathf.Repeat(num2 + (float)UnityEngine.Random.Range(0, 360), 360f);
			}
			state.facingAngle = num2;
			JumpAroundAttacking = true;
			simpleSpineAnimator.Animate("jump", 0, false);
			CameraManager.shakeCamera(0.3f, state.facingAngle);
			while (JumpAroundAttacking)
			{
				speed = 15f * Time.deltaTime;
				yield return null;
			}
			time = 0f;
			while (true)
			{
				float num;
				time = (num = time + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
				if (!(num < 0.5f))
				{
					break;
				}
				yield return null;
			}
			int num4 = Loop - 1;
			Loop = num4;
			yield return null;
		}
		ChargeDelay = 1f;
		StartCoroutine(ChasePlayer());
	}

	private IEnumerator ZipAway()
	{
		state.CURRENT_STATE = StateMachine.State.Attacking;
		simpleSpineAnimator.Animate("swing-away", 0, false);
		AudioManager.Instance.PlayOneShot("event:/boss/spider/swing_away", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/dialogue/dun4_cult_leader_shamura/battle_cry_shamura", base.gameObject);
		float time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num < 2.5f))
			{
				break;
			}
			yield return null;
		}
		base.transform.position = TargetObject.transform.position;
		simpleSpineAnimator.Animate("swing-in", 0, false);
		AudioManager.Instance.PlayOneShot("event:/boss/spider/swing_in", base.gameObject);
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		CameraManager.shakeCamera(0.7f, Utils.GetAngle(base.transform.position, TargetObject.transform.position));
		int num2 = 1;
		for (int i = 0; i < num2; i++)
		{
			if (spiderNests.Count <= 0)
			{
				break;
			}
			SpiderNest spiderNest = spiderNests[UnityEngine.Random.Range(0, spiderNests.Count)];
			spiderNest.Droppable = true;
			spiderNests.Remove(spiderNest);
			spiderNest.DropEnemies();
		}
		foreach (SpiderNest spiderNest2 in spiderNests)
		{
			spiderNest2.Shake();
		}
		time2 = 0f;
		while (true)
		{
			float num;
			time2 = (num = time2 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num < 0.65f))
			{
				break;
			}
			yield return null;
		}
		ZipAwayDelay = 4f;
		StartCoroutine(ChasePlayer());
	}

	private IEnumerator ShrinkShadow()
	{
		float Scale = 1f;
		while (Scale > 0f)
		{
			Scale -= Time.deltaTime * 5f * simpleSpineAnimator.anim.timeScale;
			Shadow.transform.localScale = ShadowSize * Scale;
			yield return null;
		}
		Shadow.transform.localScale = Vector3.zero;
	}

	private IEnumerator GrowShadow()
	{
		if (cShrinkShadow != null)
		{
			StopCoroutine(cShrinkShadow);
		}
		cShrinkShadow = null;
		float Scale = 0f;
		while (Scale < 1f)
		{
			Scale += Time.deltaTime * 3f * simpleSpineAnimator.anim.timeScale;
			Shadow.transform.localScale = ShadowSize * Scale;
			yield return null;
		}
		Shadow.transform.localScale = ShadowSize;
	}

	private IEnumerator CloseRangeAttack()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/spider/attack", base.gameObject);
		state.CURRENT_STATE = StateMachine.State.Attacking;
		simpleSpineAnimator.Animate("attack", 0, false);
		simpleSpineAnimator.AddAnimate("idle-boss", 0, true, 0f);
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetObject.transform.position);
		AttackDashSpeed = 0f;
		float Duration = 2f;
		DoDamageAttack(0.5f);
		while (true)
		{
			float num;
			Duration = (num = Duration - Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num > 0f))
			{
				break;
			}
			if (AttackDashSpeed > 0f)
			{
				AttackDashSpeed -= 3f * Time.deltaTime * simpleSpineAnimator.anim.timeScale;
				speed = AttackDashSpeed;
			}
			yield return null;
		}
		CloseRangeAttackDelay = 1f;
		StartCoroutine(ChasePlayer());
	}

	public void Play()
	{
		simpleSpineEventListener = GetComponent<SimpleSpineEventListener>();
		simpleSpineEventListener.OnSpineEvent += OnSpineEvent;
		simpleSpineEventListener.skeletonAnimation.ForceVisible = true;
		StartCoroutine(WaitForTarget());
		SeperateObject = true;
		StartPosition = base.transform.position;
		ShadowSize = Shadow.transform.localScale;
		TargetObject = PlayerFarming.Instance.gameObject;
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		active = true;
		StartCoroutine(ChasePlayer());
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		simpleSpineFlash.FlashFillRed(0.25f);
		AudioManager.Instance.PlayOneShot("event:/boss/spider/gethit", base.gameObject);
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		BiomeConstants.Instance.EmitHitVFX(AttackLocation - Vector3.back * 0.5f, Quaternion.identity.z, "HitFX_Weak");
		if (!Roared && health.HP <= health.totalHP * 0.5f)
		{
			AudioManager.Instance.PlayOneShot("event:/dialogue/dun4_cult_leader_shamura/wounded_shamura", base.gameObject);
			StopAllCoroutines();
			DisableForces = false;
			if (damageColliderEvents != null)
			{
				damageColliderEvents.SetActive(false);
			}
			StartCoroutine(Roar());
		}
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		PlayerFarming.Instance.health.invincible = true;
		PlayerFarming.Instance.playerWeapon.DoSlowMo(false);
		RoomLockController.RoomCompleted(true, false);
		if (state.CURRENT_STATE != StateMachine.State.Dieing)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("KILL_BOSS_4"));
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
			knockBackVX = (0f - KnockbackSpeed) * 1f * Mathf.Cos(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			knockBackVY = (0f - KnockbackSpeed) * 1f * Mathf.Sin(Utils.GetAngle(base.transform.position, Attacker.transform.position) * ((float)Math.PI / 180f));
			GameObject obj = BiomeConstants.Instance.GroundSmash_Medium.Spawn();
			obj.transform.position = base.transform.position;
			obj.transform.rotation = Quaternion.identity;
			damageColliderEvents.gameObject.SetActive(false);
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
			ClearPaths();
			StopAllCoroutines();
			DisableForces = false;
			StartCoroutine(Die());
		}
	}

	private IEnumerator Roar()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/spider/roar", base.gameObject);
		int num = EnemySlime.Slimes.Count;
		while (--num > 0)
		{
			EnemySlime enemySlime = EnemySlime.Slimes[num];
			enemySlime.ExplodeOnDeath = false;
			enemySlime.health.DestroyNextFrame();
		}
		num = EnemySpawner.EnemySpawners.Count;
		while (--num > 0)
		{
			UnityEngine.Object.Destroy(EnemySpawner.EnemySpawners[num]);
		}
		ClearPaths();
		health.invincible = true;
		speed = 0f;
		simpleSpineAnimator.Animate("idle-boss", 0, false);
		float time2 = 0f;
		while (true)
		{
			float num2;
			time2 = (num2 = time2 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num2 < 0.5f))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Attacking;
		if (juicedForm != GameManager.Layer2)
		{
			simpleSpineAnimator.SetSkin("NoMask");
		}
		simpleSpineAnimator.Animate("roar", 0, false);
		simpleSpineAnimator.AddAnimate("idle-boss", 0, true, 0f);
		CameraManager.instance.ShakeCameraForDuration(2f, 2.5f, 2f);
		DontPlayHurtAnimation = true;
		time2 = 0f;
		while (true)
		{
			float num2;
			time2 = (num2 = time2 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num2 < 2.3f))
			{
				break;
			}
			yield return null;
		}
		Roared = true;
		DontPlayHurtAnimation = false;
		health.invincible = false;
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			StartCoroutine(JumpAttack());
		}
		else
		{
			StartCoroutine(JumpAround());
		}
		yield return null;
	}

	private IEnumerator Die()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/spider/death", base.gameObject);
		ClearPaths();
		speed = 0f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraTarget, 7f);
		simpleSpineAnimator.FlashWhite(false);
		state.CURRENT_STATE = StateMachine.State.Dieing;
		rb.velocity = Vector3.zero;
		rb.isKinematic = true;
		rb.simulated = false;
		rb.bodyType = RigidbodyType2D.Static;
		if (base.transform.position.x > 11f)
		{
			base.transform.position = new Vector3(11f, base.transform.position.y, 0f);
		}
		if (base.transform.position.x < -11f)
		{
			base.transform.position = new Vector3(-11f, base.transform.position.y, 0f);
		}
		if (base.transform.position.y > 7f)
		{
			base.transform.position = new Vector3(base.transform.position.x, 7f, 0f);
		}
		if (base.transform.position.y < -7f)
		{
			base.transform.position = new Vector3(base.transform.position.x, -7f, 0f);
		}
		yield return new WaitForEndOfFrame();
		simpleSpineFlash.StopAllCoroutines();
		DisableForces = false;
		simpleSpineFlash.SetColor(new Color(0f, 0f, 0f, 0f));
		TrapPoison.RemoveAllPoison();
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != health && Health.team2[num] != null && Health.team2[num].gameObject.activeInHierarchy)
			{
				Health.team2[num].invincible = false;
				Health.team2[num].untouchable = false;
				if (Health.team2[num].GetComponent<SpawnEnemyOnDeath>() != null)
				{
					Health.team2[num].GetComponent<SpawnEnemyOnDeath>().SpawnEnemies = false;
				}
				Health.team2[num].DealDamage(Health.team2[num].totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
			}
		}
		yield return new WaitForEndOfFrame();
		bool beatenLayer2 = DataManager.Instance.BeatenShamuraLayer2;
		float time3;
		if (!DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location) && !DungeonSandboxManager.Active)
		{
			simpleSpineAnimator.Animate("die", 0, false);
			simpleSpineAnimator.AddAnimate("dead", 0, true, 0f);
		}
		else
		{
			if (juicedForm && !DataManager.Instance.BeatenShamuraLayer2 && !DungeonSandboxManager.Active)
			{
				simpleSpineAnimator.Animate("die-follower", 0, false);
				time3 = 0f;
				while (true)
				{
					float num2;
					time3 = (num2 = time3 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
					if (!(num2 < 1.767f))
					{
						break;
					}
					yield return null;
				}
				simpleSpineAnimator.gameObject.SetActive(false);
				PlayerReturnToBase.Disabled = true;
				GameObject Follower = UnityEngine.Object.Instantiate(followerToSpawn, base.transform.position, Quaternion.identity, base.transform.parent);
				Follower.GetComponent<Interaction_FollowerSpawn>().Play("CultLeader 4", ScriptLocalization.NAMES_CultLeaders.Dungeon4, true, Thought.Dissenter);
				DataManager.SetFollowerSkinUnlocked("CultLeader 4");
				DataManager.Instance.BeatenShamuraLayer2 = true;
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.NewGamePlus4);
				while (Follower != null)
				{
					yield return null;
				}
				GameManager.GetInstance().OnConversationEnd();
				Interaction_Chest instance = Interaction_Chest.Instance;
				if ((object)instance != null)
				{
					instance.RevealBossReward(InventoryItem.ITEM_TYPE.GOD_TEAR);
				}
				yield break;
			}
			simpleSpineAnimator.Animate("die-noheart", 0, false);
			simpleSpineAnimator.AddAnimate("dead-noheart", 0, true, 0f);
			if (!DungeonSandboxManager.Active)
			{
				RoomLockController.RoomCompleted();
			}
		}
		time3 = 0f;
		while (true)
		{
			float num2;
			time3 = (num2 = time3 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num2 < 2.7f))
			{
				break;
			}
			yield return null;
		}
		time3 = 0f;
		while (true)
		{
			float num2;
			time3 = (num2 = time3 + Time.deltaTime * simpleSpineAnimator.anim.timeScale);
			if (!(num2 < 0.5f))
			{
				break;
			}
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 11f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 13f;
		if (!DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
		{
			interaction_MonsterHeart.ObjectiveToComplete = Objectives.CustomQuestTypes.BishopsOfTheOldFaith4;
		}
		interaction_MonsterHeart.Play((!beatenLayer2 && GameManager.Layer2) ? InventoryItem.ITEM_TYPE.GOD_TEAR : InventoryItem.ITEM_TYPE.NONE);
		base.enabled = false;
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && EnemyHealth.team != health.team)
		{
			EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
	}
}
