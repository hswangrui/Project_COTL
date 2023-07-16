using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class EnemyJuicedMaggotMiniBoss : UnitObject
{
	public enum AttackPatterns
	{
		Dive,
		Shoot,
		Total
	}

	[Serializable]
	public class BulletPattern
	{
		public bool IsGrenade;

		public Vector2 RandomRange = new Vector2(3f, 5f);

		public float GravSpeed = -8f;

		public int BulletsToShoot = 10;

		public float WaveSize = 20f;

		public float WaveSpeed = 5f;

		public float Arc;

		public float DelayBetweenShots = 0.1f;

		public float Speed = 5f;

		public bool UpdateAngle;
	}

	public List<SimpleSpineFlash> SimpleSpineFlashes;

	public SkeletonAnimation Spine;

	public SkeletonAnimation Warning;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string IdleAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string RoarAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string ShootAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string ShootAnimationLong;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string LandAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AncitipationShootStraightAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AncitipationShootSpiralAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AncitipationJumpAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AnticipationRingAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AnticipationBounceyAnimation;

	private ShowHPBar ShowHPBar;

	private float Angle;

	private Vector3 Force;

	public float ArcHeight = 5f;

	private AttackPatterns _ActionPaterrn;

	private bool firstSeen;

	private bool phase2;

	public int NumberOfDives = 3;

	public float RevealMeshRenderer;

	public float HideMeshRenderer = 0.1f;

	public ColliderEvents damageColliderEvents;

	public ParticleSystem AoEParticles;

	public ProjectilePattern projectilePatternOnLand;

	public GameObject BulletPrefab;

	public GameObject GrenadeBulletPrefab;

	private int CurrentBulletPattern;

	public List<ProjectilePatternBase> BulletPatterns = new List<ProjectilePatternBase>();

	private ProjectilePatternBase b;

	private GameObject g;

	private GrenadeBullet GrenadeBullet;

	private bool EscapeIfHit;

	public FollowAsTail[] TailPieces;

	public float MoveSpeed = 5f;

	public Vector2 DistanceRange = new Vector2(2f, 4f);

	public Vector2 IdleWaitRange = new Vector2(3f, 5f);

	private float IdleWait;

	private float RandomDirection;

	private Vector3 TargetPosition;

	private bool PathingToPlayer;

	public float CircleCastRadius = 0.5f;

	public float CircleCastOffset = 1f;

	public bool ShowDebug;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> PointsLink = new List<Vector3>();

	public List<Vector3> EndPoints = new List<Vector3>();

	public List<Vector3> EndPointsLink = new List<Vector3>();

	private Health EnemyHealth;

	private AttackPatterns ActionPaterrn
	{
		get
		{
			return _ActionPaterrn;
		}
		set
		{
			_ActionPaterrn = (AttackPatterns)Mathf.Repeat((float)value, 2f);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
		DisableForces = true;
		ShowHPBar = GetComponent<ShowHPBar>();
		IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
		FollowAsTail[] tailPieces = TailPieces;
		foreach (FollowAsTail obj in tailPieces)
		{
			obj.transform.parent = base.transform.parent;
			obj.ForcePosition(Vector3.up);
		}
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		StartCoroutine(ActiveRoutine());
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (damageColliderEvents != null)
		{
			damageColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
			damageColliderEvents.SetActive(false);
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		EscapeIfHit = false;
		if (AttackType != Health.AttackTypes.NoKnockBack)
		{
			StartCoroutine(ApplyForceRoutine(Attacker));
		}
		foreach (SimpleSpineFlash simpleSpineFlash in SimpleSpineFlashes)
		{
			simpleSpineFlash.FlashFillRed();
		}
	}

	private IEnumerator DelayActiveRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(ActiveRoutine());
	}

	private IEnumerator ApplyForceRoutine(GameObject Attacker)
	{
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
		Force = new Vector2(50f * Mathf.Cos(Angle), 50f * Mathf.Sin(Angle));
		rb.AddForce(Force);
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		if (state.CURRENT_STATE == StateMachine.State.Idle)
		{
			IdleWait = 0f;
		}
	}

	private IEnumerator ActiveRoutine()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (!firstSeen)
			{
				firstSeen = true;
			}
			state.CURRENT_STATE = StateMachine.State.Idle;
			switch (ActionPaterrn)
			{
			case AttackPatterns.Dive:
				StartCoroutine(DiveMoveRoutine());
				break;
			case AttackPatterns.Shoot:
				StartCoroutine(ShootRoutine());
				break;
			}
			AttackPatterns actionPaterrn = ActionPaterrn + 1;
			ActionPaterrn = actionPaterrn;
		}
		yield break;
	}

	public override void Update()
	{
		base.Update();
		if (!(health.HP < health.totalHP / 2f) || phase2)
		{
			return;
		}
		NumberOfDives = 6;
		MoveSpeed *= 1.35f;
		phase2 = true;
		foreach (ProjectilePatternBase bulletPattern in BulletPatterns)
		{
			ProjectilePattern projectilePattern;
			ProjectilePatternBeam projectilePatternBeam;
			if ((object)(projectilePattern = bulletPattern as ProjectilePattern) != null)
			{
				for (int i = 0; i < projectilePattern.Waves.Length; i++)
				{
					projectilePattern.Waves[i].Speed *= 1.5f;
				}
			}
			else if ((object)(projectilePatternBeam = bulletPattern as ProjectilePatternBeam) != null)
			{
				for (int j = 0; j < projectilePatternBeam.BulletWaves.Length; j++)
				{
					projectilePatternBeam.BulletWaves[j].Speed *= 1.5f;
				}
			}
		}
	}

	private IEnumerator DiveMoveRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm_large/warning", base.transform.position);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= UnityEngine.Random.Range(NumberOfDives / 2, NumberOfDives))
			{
				break;
			}
			if (!GetNewTargetPosition())
			{
				continue;
			}
			while (Spine.timeScale == 0.0001f)
			{
				yield return null;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/patrol_worm/patrol_worm_jump", base.transform.position);
			health.untouchable = true;
			Spine.AnimationState.SetAnimation(0, "jump", false);
			Vector3 StartPosition = base.transform.position;
			float Progress = 0f;
			float Duration = Vector3.Distance(StartPosition, TargetPosition) / MoveSpeed;
			Vector3 Curve = StartPosition + (TargetPosition - StartPosition) / 2f + Vector3.back * ArcHeight;
			while (true)
			{
				float num2;
				Progress = (num2 = Progress + Time.deltaTime * Spine.timeScale);
				if (!(num2 < Duration))
				{
					break;
				}
				Vector3 a = Vector3.Lerp(StartPosition, Curve, Progress / Duration);
				Vector3 vector = Vector3.Lerp(Curve, TargetPosition, Progress / Duration);
				base.transform.position = Vector3.Lerp(a, vector, Progress / Duration);
				yield return null;
			}
			TargetPosition.z = 0f;
			base.transform.position = TargetPosition;
			Spine.transform.localPosition = Vector3.zero;
			if (projectilePatternOnLand != null)
			{
				projectilePatternOnLand.Shoot();
			}
			Spine.AnimationState.SetAnimation(0, LandAnimation, false);
			Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			AoEParticles.Play();
			AudioManager.Instance.PlayOneShot("event:/enemy/patrol_worm/patrol_worm_land", base.transform.position);
			AudioManager.Instance.PlayOneShot("event:/enemy/patrol_boss/patrol_boss_fire_aoe", base.transform.position);
			damageColliderEvents.SetActive(true);
			health.untouchable = false;
			if (phase2)
			{
				Projectile.CreateProjectiles(7, health, base.transform.position, 14f, 1f, UnityEngine.Random.Range(0, 360));
			}
			base.transform.DOMove(base.transform.position + Vector3.down * 0.5f, 0.2f);
			float time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.3f))
				{
					break;
				}
				yield return null;
			}
			damageColliderEvents.SetActive(false);
			if (i >= NumberOfDives - 1)
			{
				continue;
			}
			time2 = 0f;
			while (true)
			{
				float num2;
				time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
				if (!(num2 < 0.2f))
				{
					break;
				}
				yield return null;
			}
		}
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(ActiveRoutine());
	}

	private void ForceTailDirection(Vector3 Direction)
	{
		Debug.Log("TAILS : " + TailPieces.Length);
		FollowAsTail[] tailPieces = TailPieces;
		foreach (FollowAsTail obj in tailPieces)
		{
			Debug.Log(obj.name);
			obj.ForcePosition(Direction);
		}
	}

	private IEnumerator ShootRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/worm_large/warning", base.transform.position);
		int num = UnityEngine.Random.Range(0, BulletPatterns.Count);
		b = BulletPatterns[num];
		switch (num)
		{
		case 1:
			yield return Spine.YieldForAnimation(AnticipationRingAnimation);
			break;
		case 2:
			yield return Spine.YieldForAnimation(AnticipationBounceyAnimation);
			break;
		default:
			yield return Spine.YieldForAnimation(AncitipationShootStraightAnimation);
			break;
		}
		FollowAsTail[] tailPieces = TailPieces;
		for (int i = 0; i < tailPieces.Length; i++)
		{
			tailPieces[i].ForcePosition(Vector3.up);
		}
		bool doRoar = true;
		if (b is ProjectilePattern)
		{
			((ProjectilePattern)b).OnProjectileWaveShot += delegate(ProjectilePattern.BulletWave wave)
			{
				if (!string.IsNullOrEmpty(wave.AnimationToPlay))
				{
					CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.2f);
					Spine.AnimationState.SetAnimation(0, ShootAnimation, false);
				}
			};
		}
		else
		{
			CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 1.5f);
			Spine.AnimationState.SetAnimation(0, ShootAnimationLong, false);
			doRoar = false;
		}
		yield return StartCoroutine(b.ShootIE());
		state.CURRENT_STATE = StateMachine.State.Idle;
		EscapeIfHit = true;
		float time2 = 0f;
		while (true)
		{
			float num2;
			time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num2 < 0.5f))
			{
				break;
			}
			yield return null;
		}
		tailPieces = TailPieces;
		for (int i = 0; i < tailPieces.Length; i++)
		{
			tailPieces[i].ForcePosition(Vector3.up);
		}
		if (doRoar)
		{
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			Spine.AnimationState.SetAnimation(0, RoarAnimation, false);
		}
		AudioManager.Instance.PlayOneShot("event:/enemy/patrol_boss/patrol_boss_roar", base.transform.position);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		time2 = 0f;
		while (true)
		{
			float num2;
			time2 = (num2 = time2 + Time.deltaTime * Spine.timeScale);
			if (!(num2 < 2f))
			{
				break;
			}
			yield return null;
		}
		EscapeIfHit = false;
		StartCoroutine(ActiveRoutine());
	}

	private IEnumerator MoveRoutine()
	{
		ShowHPBar.Hide();
		state.CURRENT_STATE = StateMachine.State.Fleeing;
		health.enabled = false;
		Vector3 StartPosition = base.transform.position;
		float Progress = 0f;
		float Duration = Vector3.Distance(StartPosition, TargetPosition) / MoveSpeed;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num < Duration))
			{
				break;
			}
			base.transform.position = Vector3.Lerp(StartPosition, TargetPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		base.transform.position = TargetPosition;
		health.enabled = true;
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime * Spine.timeScale);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(ActiveRoutine());
	}

	public bool GetNewTargetPosition()
	{
		if (PlayerFarming.Instance != null)
		{
			float num = Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position);
			if (PlayerFarming.Instance != null && num < 10f && num >= 4f)
			{
				TargetPosition = PlayerFarming.Instance.transform.position;
				if (Mathf.Abs(TargetPosition.x) > 6.5f || Mathf.Abs(TargetPosition.y) > 4f)
				{
					TargetPosition = new Vector3(UnityEngine.Random.Range(-6.5f, 6.5f), UnityEngine.Random.Range(-4f, 4f), 0f);
				}
				return true;
			}
		}
		float num2 = 100f;
		if (PlayerFarming.Instance != null && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 8f && CheckLineOfSight(PlayerFarming.Instance.transform.position, 8f))
		{
			PathingToPlayer = true;
			RandomDirection = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position) * ((float)Math.PI / 180f);
		}
		else
		{
			RandomDirection = UnityEngine.Random.Range(0, 360);
		}
		while ((num2 -= 1f) > 0f)
		{
			float num3 = UnityEngine.Random.Range(DistanceRange.x, DistanceRange.y);
			if (!PathingToPlayer)
			{
				RandomDirection += (float)UnityEngine.Random.Range(-90, 90) * ((float)Math.PI / 180f);
			}
			float radius = 0.2f;
			Vector3 vector = base.transform.position + new Vector3(num3 * Mathf.Cos(RandomDirection), num3 * Mathf.Sin(RandomDirection));
			RaycastHit2D raycastHit2D = Physics2D.CircleCast(base.transform.position, radius, Vector3.Normalize(vector - base.transform.position), num3, layerToCheck);
			if (raycastHit2D.collider != null)
			{
				if (ShowDebug)
				{
					Points.Add(new Vector3(raycastHit2D.centroid.x, raycastHit2D.centroid.y) + Vector3.Normalize(base.transform.position - vector) * CircleCastOffset);
					PointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
				}
				RandomDirection = 180f - RandomDirection;
				continue;
			}
			if (ShowDebug)
			{
				EndPoints.Add(new Vector3(vector.x, vector.y));
				EndPointsLink.Add(new Vector3(base.transform.position.x, base.transform.position.y));
			}
			IdleWait = UnityEngine.Random.Range(IdleWaitRange.x, IdleWaitRange.y);
			TargetPosition = vector;
			if (Mathf.Abs(TargetPosition.x) > 6.5f || Mathf.Abs(TargetPosition.y) > 4f)
			{
				TargetPosition = new Vector3(UnityEngine.Random.Range(-6.5f, 6.5f), UnityEngine.Random.Range(-4f, 4f), 0f);
			}
			return true;
		}
		return false;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, VisionRange, Color.yellow);
		int num = -1;
		while (++num < Points.Count)
		{
			Utils.DrawCircleXY(PointsLink[num], 0.5f, Color.blue);
			Utils.DrawCircleXY(Points[num], CircleCastRadius, Color.blue);
			Utils.DrawLine(Points[num], PointsLink[num], Color.blue);
		}
		num = -1;
		while (++num < EndPoints.Count)
		{
			Utils.DrawCircleXY(EndPointsLink[num], 0.5f, Color.red);
			Utils.DrawCircleXY(EndPoints[num], CircleCastRadius, Color.red);
			Utils.DrawLine(EndPointsLink[num], EndPoints[num], Color.red);
		}
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
