using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMBiomeGeneration;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CircleCollider2D))]
public class UnitObject : BaseMonoBehaviour
{
	public delegate void EnemyKilled(Enemy enemy);

	public delegate void Action();

	[SerializeField]
	private Enemy enemyType;

	public bool SeperateObject;

	public bool SeparateObjectFromPlayer;

	public int VisionRange = 15;

	public bool CheckSightBeforePath = true;

	public bool UsePathing = true;

	public LayerMask layerToCheck;

	public float maxSpeed = 0.05f;

	public float StoppingDistance = 0.1f;

	public float SpeedMultiplier = 1f;

	public bool emitDustClouds = true;

	public float distanceBetweenDustClouds = 0.5f;

	private Seeker seeker;

	public static NNConstraint constraint = new NNConstraint();

	[HideInInspector]
	public StateMachine state;

	private Vector2 targetLocation;

	[HideInInspector]
	public float vx;

	[HideInInspector]
	public float vy;

	[HideInInspector]
	public float seperatorVX;

	[HideInInspector]
	public float seperatorVY;

	[HideInInspector]
	public float moveVX;

	[HideInInspector]
	public float moveVY;

	[HideInInspector]
	public float knockBackVX;

	[HideInInspector]
	public float knockBackVY;

	protected int currentWaypoint;

	[HideInInspector]
	public List<Vector3> pathToFollow;

	[HideInInspector]
	public float speed;

	public bool isFlyingEnemy;

	private Vector2 positionLastFrame;

	[HideInInspector]
	public Health health;

	[HideInInspector]
	public Health TargetEnemy;

	public System.Action EndOfPath;

	private Coroutine knockRoutine;

	private EnemyModifier modifier;

	private float modifierTimer;

	public float EnemyModifierIconOffset = 2.25f;

	[HideInInspector]
	public EnemyOrderGroupIndicator orderIndicator;

	[SerializeField]
	private bool isBoss;

	private float distanceTravelledSinceLastDustCloud;

	public bool CanHaveModifier = true;

	public static List<UnitObject> Seperaters = new List<UnitObject>();

	private MeshRenderer[] childRenderers = new MeshRenderer[0];

	private Vector3 previousPosition = Vector3.zero;

	public bool UseFixedDirectionalPathing;

	private ModifierIcon modifierIcon;

	private Vector3 goToNoPathfinding;

	private Vector3 pointToCheck;

	protected CircleCollider2D ColliderRadius;

	[HideInInspector]
	public Rigidbody2D rb;

	public bool DisableForces;

	private Vector3 PrevPosition;

	public bool LockToGround = true;

	private RaycastHit LockToGroundHit;

	private Vector3 LockToGroundPosition;

	private Vector3 LockToGroundNewPosition;

	private bool dead;

	private float checkFrame;

	private Health cachedTarget;

	public static readonly int LeaderEncounterColorBoost = Shader.PropertyToID("_LeaderEncounterColorBoost");

	public Enemy EnemyType
	{
		get
		{
			return enemyType;
		}
	}

	public bool UseDeltaTime { get; set; } = true;


	public bool IsBoss
	{
		get
		{
			return isBoss;
		}
	}

	public bool HasModifier
	{
		get
		{
			return modifier != null;
		}
	}

	public MeshRenderer[] ChildRenderers
	{
		get
		{
			return childRenderers;
		}
	}

	protected virtual float timeStopMultiplier
	{
		get
		{
			return 1f;
		}
	}

	public static event EnemyKilled OnEnemyKilled;

	public event Action NewPath;

	public void Seperate(float SeperationRadius, bool IgnorePlayer = false)
	{
		seperatorVX = 0f;
		seperatorVY = 0f;
		foreach (UnitObject seperater in Seperaters)
		{
			if ((!IgnorePlayer || seperater.health.team != Health.Team.PlayerTeam) && (!(seperater != null) || !(seperater != this) || health.team != Health.Team.PlayerTeam || seperater.SeparateObjectFromPlayer) && seperater != this && seperater != null && SeperateObject && seperater.SeperateObject && state.CURRENT_STATE != StateMachine.State.Dodging && state.CURRENT_STATE != StateMachine.State.Defending)
			{
				float num = Vector2.Distance(seperater.gameObject.transform.position, base.transform.position);
				float angle = Utils.GetAngle(seperater.gameObject.transform.position, base.transform.position);
				if (num < SeperationRadius)
				{
					seperatorVX += (SeperationRadius - num) / 2f * Mathf.Cos(angle * ((float)Math.PI / 180f)) * GameManager.FixedDeltaTime;
					seperatorVY += (SeperationRadius - num) / 2f * Mathf.Sin(angle * ((float)Math.PI / 180f)) * GameManager.FixedDeltaTime;
				}
			}
		}
	}

	public virtual void Awake()
	{
		seeker = GetComponent<Seeker>();
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
		rb = base.gameObject.GetComponent<Rigidbody2D>();
		childRenderers = GetComponentsInChildren<MeshRenderer>(true);
		if (health.team == Health.Team.Team2 && CanHaveModifier && modifier == null)
		{
			modifier = EnemyModifier.GetModifier(DataManager.Instance.EnemiesInNextRoomHaveModifiers ? 1 : 0);
			if ((bool)modifier)
			{
				ForceSetModifier(modifier);
			}
		}
	}

	public void ForceSetModifier(EnemyModifier modifier)
	{
		this.modifier = modifier;
		modifierIcon = UnityEngine.Object.Instantiate(modifier.ModifierIcon, base.transform.position, Quaternion.identity).GetComponent<ModifierIcon>();
		GameObject obj = modifierIcon.gameObject;
		obj.transform.parent = base.transform;
		obj.transform.localPosition = Vector3.back * EnemyModifierIconOffset;
		modifierIcon.Init(modifier);
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetColor("_Color", modifier.ColorTint);
		MeshRenderer[] array = childRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.sortingLayerID != 15 && meshRenderer.sortingLayerID != 20)
			{
				meshRenderer.SetPropertyBlock(materialPropertyBlock);
				meshRenderer.transform.localScale *= modifier.Scale;
			}
		}
		SimpleSpineFlash[] componentsInChildren = GetComponentsInChildren<SimpleSpineFlash>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OverrideBaseColor(modifier.ColorTint);
		}
		ShowHPBar component = GetComponent<ShowHPBar>();
		if ((bool)component)
		{
			component.zOffset *= modifier.Scale;
		}
		GetComponent<Health>().totalHP *= modifier.HealthMultiplier;
	}

	public void RemoveModifier()
	{
		if (!modifier)
		{
			return;
		}
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetColor("_Color", Color.white);
		MeshRenderer[] array = childRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.sortingLayerID != 15 && meshRenderer.sortingLayerID != 20)
			{
				meshRenderer.SetPropertyBlock(materialPropertyBlock);
				meshRenderer.transform.localScale /= modifier.Scale;
			}
		}
		UnityEngine.Object.Destroy(modifierIcon.gameObject);
		health.totalHP /= modifier.HealthMultiplier;
		health.HP = health.totalHP;
		modifier = null;
	}

	public virtual void OnEnable()
	{
		if ((bool)seeker)
		{
			Seeker obj = seeker;
			obj.pathCallback = (OnPathDelegate)Delegate.Combine(obj.pathCallback, new OnPathDelegate(startPath));
		}
		Seperaters.Add(this);
		health.OnDie += OnDie;
		health.OnHit += OnHit;
		Color value = ((!(LightingManager.Instance != null) || !LightingManager.Instance.inLeaderEncounter) ? new Color(0f, 0f, 0f, 0f) : new Color(-0.25f, 0f, 0.25f, 0f));
		if (SceneManager.GetActiveScene().name == "Base Biome 1")
		{
			value = new Color(0f, 0f, 0f, 0f);
		}
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer != null && meshRenderer.sharedMaterial != null && meshRenderer.sortingLayerID != 15 && meshRenderer.sortingLayerID != 20)
			{
				meshRenderer.sharedMaterial.SetColor(LeaderEncounterColorBoost, value);
			}
		}
		MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
		if (component != null && component.sharedMaterial != null)
		{
			component.sharedMaterial.SetColor(LeaderEncounterColorBoost, value);
		}
	}

	public virtual void OnDisable()
	{
		seeker.CancelCurrentPathRequest();
		Seperaters.Remove(this);
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Remove(obj.pathCallback, new OnPathDelegate(startPath));
		health.OnDie -= OnDie;
		health.OnHit -= OnHit;
	}

	public virtual void DoKnockBack(GameObject Attacker, float KnockbackModifier, float Duration, bool appendForce = true)
	{
		if (!(rb == null))
		{
			if (knockRoutine != null)
			{
				StopCoroutine(knockRoutine);
			}
			if (!appendForce)
			{
				rb.velocity = Vector3.zero;
			}
			float angle = Utils.GetAngle(Attacker.transform.position, base.transform.position) * ((float)Math.PI / 180f);
			knockRoutine = StartCoroutine(ApplyForceRoutine(angle, KnockbackModifier, Duration));
		}
	}

	public virtual void DoKnockBack(float angle, float KnockbackModifier, float Duration, bool appendForce = true)
	{
		if (knockRoutine != null)
		{
			StopCoroutine(knockRoutine);
		}
		if (!appendForce)
		{
			rb.velocity = Vector3.zero;
		}
		knockRoutine = StartCoroutine(ApplyForceRoutine(angle, KnockbackModifier, Duration));
	}

	private IEnumerator ApplyForceRoutine(float angle, float KnockbackModifier, float Duration)
	{
		DisableForces = true;
		Vector3 vector = new Vector2(25f * Mathf.Cos(angle), 25f * Mathf.Sin(angle));
		rb.velocity = vector * KnockbackModifier;
		yield return new WaitForSeconds(Duration);
		DisableForces = false;
		knockRoutine = null;
	}

	public virtual void BeAlarmed(GameObject TargetObject)
	{
	}

	public void givePath(Vector3 targetLocation)
	{
		ClearPaths();
		if (AstarPath.active != null)
		{
			if (CheckSightBeforePath && CheckLineOfSight(targetLocation, Vector2.Distance(base.transform.position, targetLocation)))
			{
				state.CURRENT_STATE = StateMachine.State.Moving;
				pathToFollow = new List<Vector3>();
				if (AstarPath.active.GetNearest(targetLocation).node != null)
				{
					goToNoPathfinding = (Vector3)AstarPath.active.GetNearest(targetLocation).node.position;
				}
				else
				{
					goToNoPathfinding = targetLocation;
				}
				pathToFollow.Add(goToNoPathfinding);
				currentWaypoint = 0;
			}
			else
			{
				state.CURRENT_STATE = StateMachine.State.Moving;
				GraphNode node = AstarPath.active.GetNearest(targetLocation).node;
				if (node != null)
				{
					goToNoPathfinding = (Vector3)node.position;
					seeker.StartPath(base.transform.position, goToNoPathfinding);
				}
			}
		}
		else
		{
			Debug.Log("No need pathfinding " + targetLocation);
			state.CURRENT_STATE = StateMachine.State.Moving;
			pathToFollow = new List<Vector3>();
			pathToFollow.Add(targetLocation);
			currentWaypoint = 0;
		}
		if (this.NewPath != null)
		{
			this.NewPath();
		}
	}

	public bool OnGround(Vector3 Position)
	{
		LayerMask layerMask = LayerMask.GetMask("Island");
		StartCoroutine(DrawRay(Position));
		RaycastHit hitInfo;
		if (Physics.Raycast(Position, Vector3.forward, out hitInfo, float.PositiveInfinity, layerMask))
		{
			return true;
		}
		return false;
	}

	private IEnumerator DrawRay(Vector3 Position)
	{
		float Timer = 3f;
		while (true)
		{
			float num;
			Timer = (num = Timer - Time.deltaTime);
			if (num > 0f)
			{
				Debug.DrawRay(Position, Vector3.forward, Color.blue);
				yield return null;
				continue;
			}
			break;
		}
	}

	public bool IsPathPossible(Vector3 PathStart, Vector3 PathEnd)
	{
		GraphNode node = AstarPath.active.GetNearest(PathStart, NNConstraint.Default).node;
		GraphNode node2 = AstarPath.active.GetNearest(PathEnd, NNConstraint.Default).node;
		return PathUtilities.IsPathPossible(node, node2);
	}

	public bool CheckLineOfSight(Vector3 pointToCheck, float distance)
	{
		if (PlayerFarming.Instance == null)
		{
			return false;
		}
		this.pointToCheck = pointToCheck;
		if (ColliderRadius == null)
		{
			ColliderRadius = GetComponent<CircleCollider2D>();
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, pointToCheck - base.transform.position, distance, layerToCheck);
		if (raycastHit2D.collider != null && raycastHit2D.collider != PlayerFarming.Instance.circleCollider2D)
		{
			return false;
		}
		float angle = Utils.GetAngle(base.transform.position, pointToCheck);
		raycastHit2D = Physics2D.Raycast(base.transform.position + new Vector3(ColliderRadius.radius * Mathf.Cos((angle + 90f) * ((float)Math.PI / 180f)), ColliderRadius.radius * Mathf.Sin((angle + 90f) * ((float)Math.PI / 180f))), pointToCheck - base.transform.position, distance, layerToCheck);
		if (raycastHit2D.collider != null && raycastHit2D.collider != PlayerFarming.Instance.circleCollider2D)
		{
			return false;
		}
		raycastHit2D = Physics2D.Raycast(base.transform.position + new Vector3(ColliderRadius.radius * Mathf.Cos((angle - 90f) * ((float)Math.PI / 180f)), ColliderRadius.radius * Mathf.Sin((angle - 90f) * ((float)Math.PI / 180f))), pointToCheck - base.transform.position, distance, layerToCheck);
		if (raycastHit2D.collider != null && raycastHit2D.collider != PlayerFarming.Instance.circleCollider2D)
		{
			return false;
		}
		return true;
	}

	public bool CheckLineOfSight(float distance)
	{
		return !Physics2D.Raycast(base.transform.position, (PlayerFarming.Instance.transform.position - base.transform.position).normalized, distance, layerToCheck);
	}

	public void CreateFleePath(Vector3 FleeFromPosition)
	{
		int searchLength = 50000;
		FleePath fleePath = FleePath.Construct(base.transform.position, FleeFromPosition, searchLength);
		fleePath.aimStrength = 0.5f;
		fleePath.spread = 4000;
		seeker.StartPath(fleePath);
		state.CURRENT_STATE = StateMachine.State.Fleeing;
	}

	public void startPath(Path p)
	{
		if (!p.error)
		{
			pathToFollow = new List<Vector3>();
			for (int i = 0; i < p.vectorPath.Count; i++)
			{
				pathToFollow.Add(p.vectorPath[i]);
			}
			currentWaypoint = 1;
		}
	}

	public static Quaternion FaceObject(Vector2 startingPosition, Vector2 targetPosition)
	{
		Vector2 vector = targetPosition - startingPosition;
		return Quaternion.AngleAxis(Mathf.Atan2(vector.y, vector.x) * 57.29578f, Vector3.forward);
	}

	public void ClearPaths()
	{
		pathToFollow = null;
		move();
	}

	public virtual void Update()
	{
		if (modifier.HasModifier(EnemyModifier.ModifierType.DropProjectiles))
		{
			float num = 5f;
			modifierTimer += Time.deltaTime;
			float progress = modifierTimer / num;
			if (modifierIcon != null)
			{
				modifierIcon.UpdateTimer(progress);
			}
			if (modifierTimer >= num)
			{
				Projectile.CreateProjectiles(5, health, new Vector3(base.transform.position.x, base.transform.position.y, 0f), false);
				AudioManager.Instance.PlayOneShot("event:/enemy/shoot_magicenergy", base.gameObject);
				modifierTimer = 0f;
			}
		}
		float num2 = (UseDeltaTime ? GameManager.DeltaTime : GameManager.UnscaledDeltaTime);
		if (UsePathing)
		{
			if (pathToFollow == null)
			{
				speed += (0f - speed) / 4f * num2;
				move();
				return;
			}
			if (currentWaypoint >= pathToFollow.Count)
			{
				speed += (0f - speed) / 4f * num2;
				move();
				return;
			}
		}
		if (state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.Fleeing)
		{
			speed += (maxSpeed * SpeedMultiplier - speed) / 7f * num2;
			if (UsePathing && pathToFollow != null)
			{
				if (UseFixedDirectionalPathing)
				{
					int num3 = Mathf.CeilToInt(((previousPosition == Vector3.zero) ? (StoppingDistance * 2f) : Vector3.Distance(previousPosition, base.transform.position)) / StoppingDistance);
					for (int i = 0; i < num3; i++)
					{
						float t = (float)i / (float)num3;
						Vector3 vector = Vector3.Lerp(previousPosition, base.transform.position, t);
						state.facingAngle = Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]);
						if (!(Vector2.Distance(base.transform.position, pathToFollow[currentWaypoint]) <= StoppingDistance) && !(Vector2.Distance(vector, pathToFollow[currentWaypoint]) <= StoppingDistance))
						{
							continue;
						}
						currentWaypoint++;
						if (currentWaypoint == pathToFollow.Count)
						{
							state.CURRENT_STATE = StateMachine.State.Idle;
							System.Action endOfPath = EndOfPath;
							if (endOfPath != null)
							{
								endOfPath();
							}
							pathToFollow = null;
							speed = 0f;
							break;
						}
					}
				}
				else
				{
					state.facingAngle = Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]);
					if (Vector2.Distance(base.transform.position, pathToFollow[currentWaypoint]) <= StoppingDistance)
					{
						currentWaypoint++;
						if (currentWaypoint == pathToFollow.Count)
						{
							state.CURRENT_STATE = StateMachine.State.Idle;
							System.Action endOfPath2 = EndOfPath;
							if (endOfPath2 != null)
							{
								endOfPath2();
							}
							pathToFollow = null;
							speed = 0f;
						}
					}
				}
			}
		}
		else
		{
			speed += (0f - speed) / 4f * num2;
		}
		move();
	}

	protected void move()
	{
		if (!float.IsNaN(state.facingAngle))
		{
			if (float.IsNaN(speed) || float.IsInfinity(speed))
			{
				speed = 0f;
			}
			speed = Mathf.Clamp(speed, 0f, maxSpeed);
			moveVX = speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f));
			moveVY = speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f));
			previousPosition = base.transform.position;
		}
	}

	protected virtual void FixedUpdate()
	{
		if (!(rb == null) && !DisableForces)
		{
			float num = (UseDeltaTime ? GameManager.DeltaTime : GameManager.UnscaledDeltaTime);
			knockBackVX += (0f - knockBackVX) / 4f * num;
			knockBackVY += (0f - knockBackVY) / 4f * num;
			if (float.IsNaN(moveVX) || float.IsInfinity(moveVX))
			{
				moveVX = 0f;
			}
			if (float.IsNaN(moveVY) || float.IsInfinity(moveVY))
			{
				moveVY = 0f;
			}
			Vector2 position = rb.position + new Vector2(vx, vy) * Time.deltaTime + new Vector2(moveVX, moveVY) * num + new Vector2(seperatorVX, seperatorVY) * num + new Vector2(knockBackVX, knockBackVY) * num;
			rb.MovePosition(position);
			positionLastFrame = position;
		}
	}

	private void LateUpdate()
	{
		if ((state.CURRENT_STATE == StateMachine.State.Moving || state.CURRENT_STATE == StateMachine.State.Fleeing || state.CURRENT_STATE == StateMachine.State.DashAcrossIsland || state.CURRENT_STATE == StateMachine.State.Dodging) && emitDustClouds)
		{
			distanceTravelledSinceLastDustCloud += (base.transform.position - PrevPosition).magnitude;
			if (distanceTravelledSinceLastDustCloud >= distanceBetweenDustClouds)
			{
				distanceTravelledSinceLastDustCloud = 0f;
				if (base.transform != null && BiomeConstants.Instance != null)
				{
					BiomeConstants.Instance.EmitDustCloudParticles(base.transform.position);
				}
			}
		}
		PrevPosition = base.transform.position;
		if (!LockToGround)
		{
			return;
		}
		LockToGroundPosition = base.transform.position + Vector3.back * 3f;
		if (Physics.Raycast(LockToGroundPosition, Vector3.forward, out LockToGroundHit, float.PositiveInfinity))
		{
			if (LockToGroundHit.collider.gameObject.GetComponent<MeshCollider>() != null)
			{
				LockToGroundNewPosition = base.transform.position;
				LockToGroundNewPosition.z = LockToGroundHit.point.z;
				base.transform.position = LockToGroundNewPosition;
			}
		}
		else
		{
			LockToGroundNewPosition = base.transform.position;
			LockToGroundNewPosition.z = 0f;
			base.transform.position = LockToGroundNewPosition;
		}
	}

	public virtual void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!dead)
		{
			if (modifier.HasModifier(EnemyModifier.ModifierType.DropPoison))
			{
				TrapPoison.CreatePoison(base.transform.position, 10, 0.5f, base.transform.parent);
				AudioManager.Instance.PlayOneShot("event:/player/poison_damage", base.transform.position);
			}
			if (modifier.HasModifier(EnemyModifier.ModifierType.DropBomb))
			{
				Bomb.CreateBomb(base.transform.position, health, base.transform.parent);
				AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_shoot", base.transform.position);
			}
			InventoryItem[] itemsToDrop = TrinketManager.GetItemsToDrop();
			foreach (InventoryItem inventoryItem in itemsToDrop)
			{
				InventoryItem.Spawn((InventoryItem.ITEM_TYPE)inventoryItem.type, inventoryItem.quantity, base.transform.position);
			}
			if (AttackType != Health.AttackTypes.Projectile && AttackType != Health.AttackTypes.Poison)
			{
				GameManager.GetInstance().HitStop(0.1f * timeStopMultiplier);
			}
			DataManager.Instance.AddEnemyKilled(enemyType);
			EnemyKilled onEnemyKilled = UnitObject.OnEnemyKilled;
			if (onEnemyKilled != null)
			{
				onEnemyKilled(enemyType);
			}
			dead = true;
		}
	}

	public virtual void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		CameraManager.instance.ShakeCameraForDuration(0.6f, 0.8f, 0.3f, false);
		if (modifier.HasModifier(EnemyModifier.ModifierType.DropPoison))
		{
			TrapPoison.CreatePoison(base.transform.position, 5, 0.1f, base.transform.parent);
			AudioManager.Instance.PlayOneShot("event:/player/poison_damage", base.transform.position);
		}
		if (AttackType != Health.AttackTypes.Projectile && AttackType != Health.AttackTypes.Poison && AttackType != Health.AttackTypes.NoHitStop)
		{
			GameManager.GetInstance().HitStop(0.05f * timeStopMultiplier);
		}
	}

	public virtual void OnDestroy()
	{
	}

	public void EmitFootstep()
	{
		AudioManager.Instance.PlayFootstep(base.transform.position);
	}

	public static string GetLocalisedEnemyName(Enemy enemy)
	{
		return LocalizationManager.GetTranslation("Enemies/" + enemy);
	}

	protected Health GetClosestTarget(bool ignoreBreakables = false)
	{
		if (BiomeGenerator.Instance == null || BiomeGenerator.Instance.CurrentRoom == null || BiomeGenerator.Instance.CurrentRoom.generateRoom == null)
		{
			if (!(PlayerFarming.Instance != null) || PlayerFarming.Instance.GoToAndStopping)
			{
				return null;
			}
			return PlayerFarming.Instance.health;
		}
		if (Time.time == checkFrame)
		{
			return cachedTarget;
		}
		Health.Team team = ((this.health.team != Health.Team.PlayerTeam) ? Health.Team.PlayerTeam : Health.Team.Team2);
		List<Health> list = new List<Health>(Health.team2);
		List<Health> list2 = new List<Health>();
		if (team == Health.Team.PlayerTeam)
		{
			if ((bool)PlayerFarming.Instance && Health.playerTeam.Count <= 1)
			{
				if (PlayerFarming.Instance.GoToAndStopping)
				{
					return null;
				}
				return PlayerFarming.Instance.health;
			}
			list.Clear();
			for (int i = 0; i < Health.playerTeam.Count; i++)
			{
				if (Health.playerTeam[i] != null)
				{
					list.Add(Health.playerTeam[i]);
				}
			}
		}
		foreach (Health item in list)
		{
			if (!(item == null) && item.enabled && !item.invincible && !item.untouchable && !item.InanimateObject && !(item.HP <= 0f) && (!ignoreBreakables || item.team != Health.Team.Team2 || !item.CompareTag("BreakableDecoration")) && (bool)item && item.team == team)
			{
				list2.Add(item);
			}
		}
		if (list2.Count == 0 && team == Health.Team.PlayerTeam && (bool)PlayerFarming.Instance)
		{
			return PlayerFarming.Instance.health;
		}
		Health health = null;
		foreach (Health item2 in list2)
		{
			if (!(Vector3.Distance(item2.transform.position, base.transform.position) > (float)VisionRange) && (health == null || Vector3.Distance(item2.transform.position, base.transform.position) < Vector3.Distance(health.transform.position, base.transform.position)))
			{
				health = item2;
			}
		}
		checkFrame = Time.time;
		cachedTarget = health;
		return health;
	}

	public static Health GetClosestTarget(Transform obj, Health.Team targetTeam)
	{
		if (BiomeGenerator.Instance == null || BiomeGenerator.Instance.CurrentRoom == null || BiomeGenerator.Instance.CurrentRoom.generateRoom == null)
		{
			if (!(PlayerFarming.Instance != null) || PlayerFarming.Instance.GoToAndStopping)
			{
				return null;
			}
			return PlayerFarming.Instance.health;
		}
		List<Health> list = new List<Health>(Health.team2);
		List<Health> list2 = new List<Health>();
		if (targetTeam == Health.Team.PlayerTeam)
		{
			if ((bool)PlayerFarming.Instance && Health.playerTeam.Count <= 1)
			{
				if (PlayerFarming.Instance.GoToAndStopping)
				{
					return null;
				}
				return PlayerFarming.Instance.health;
			}
			list.Clear();
			for (int i = 0; i < Health.playerTeam.Count; i++)
			{
				if (Health.playerTeam[i] != null)
				{
					list.Add(Health.playerTeam[i]);
				}
			}
		}
		foreach (Health item in list)
		{
			if (!(item == null) && item.enabled && !item.invincible && !item.untouchable && !item.InanimateObject && !(item.HP <= 0f) && (bool)item && item.team == targetTeam)
			{
				list2.Add(item);
			}
		}
		Health health = null;
		foreach (Health item2 in list2)
		{
			if (health == null || Vector3.Distance(item2.transform.position, obj.position) < Vector3.Distance(health.transform.position, obj.position))
			{
				health = item2;
			}
		}
		return health;
	}
}
