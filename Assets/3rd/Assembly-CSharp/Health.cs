using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using MMRoomGeneration;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class Health : BaseMonoBehaviour
{
	public enum CheatMode
	{
		None,
		Immortal,
		Demigod,
		God
	}

	public delegate void DieAction(GameObject Attacker, Vector3 AttackLocation, Health Victim, AttackTypes AttackType, AttackFlags AttackFlags);

	public delegate void DieAllAction(Health Victim);

	public delegate void HitAction(GameObject Attacker, Vector3 AttackLocation, AttackTypes AttackType, bool FromBehind = false);

	public delegate void HealthEvent(GameObject attacker, Vector3 attackLocation, float damage, AttackTypes attackType, AttackFlags attackFlag);

	public enum Team
	{
		Neutral,
		PlayerTeam,
		Team2,
		DangerousAnimals,
		KillAll
	}

	public class DealDamageEvent
	{
		public float UnscaledTimestamp;

		public float Damage;

		public GameObject Attacker;

		public Vector3 AttackLocation;

		public bool BreakBlocking;

		public AttackTypes AttackType;

		public DealDamageEvent(float unscaledTimestamp, float damage, GameObject attacker, Vector3 attackLocation, bool breakBlocking, AttackTypes attackType)
		{
			UnscaledTimestamp = unscaledTimestamp;
			Damage = damage;
			Attacker = attacker;
			AttackLocation = attackLocation;
			BreakBlocking = breakBlocking;
			AttackType = attackType;
		}
	}

	public enum IMPACT_SFX
	{
		NONE,
		IMPACT_BLUNT,
		IMPACT_NORMAL,
		IMPACT_SQUISHY,
		HIT_SMALL,
		HIT_MEDIUM,
		HIT_LARGE
	}

	public enum DEATH_SFX
	{
		NONE,
		DEATH_SMALL,
		DEATH_MEDIUM,
		DEATH_LARGE
	}

	public delegate void StasisEvent();

	public enum AttackTypes
	{
		Melee,
		Heavy,
		Projectile,
		Poison,
		NoKnockBack,
		Ice,
		Charm,
		NoHitStop,
		Electrified
	}

	[Flags]
	public enum AttackFlags
	{
		Crit = 1,
		Skull = 2,
		Poison = 4,
		Ice = 8,
		Charm = 0x10,
		DoesntChargeRelics = 0x20,
		Electrified = 0x40,
		Penetration = 0x80
	}

	public enum DamageAllEnemiesType
	{
		BlackHeart,
		DeathsDoor,
		Manipulation,
		DamagePerFollower
	}

	[HideInInspector]
	public StateMachine state;

	public bool Unaware;

	[HideInInspector]
	public bool InStealthCover;

	public bool SlowMoOnkill;

	public bool HasShield;

	public bool WasJustParried;

	public bool IgnoreProjectiles;

	public bool CanIncreaseDamageMultiplier = true;

	[HideInInspector]
	public MinionProtector protector;

	public CheatMode GodMode;

	public UnityEvent OnHitCallback;

	public UnityEvent OnDieCallback;

	public Team team;

	public static List<Health> allUnits = new List<Health>();

	public static List<Health> neutralTeam = new List<Health>();

	public static List<Health> playerTeam = new List<Health>();

	public static List<Health> team2 = new List<Health>();

	public static List<Health> dangerousAnimals = new List<Health>();

	public static List<Health> killAll = new List<Health>();

	[SerializeField]
	public float _totalHP = 1f;

	[SerializeField]
	public float _HP;

	[SerializeField]
	public float _BlueHearts;

	[SerializeField]
	public float _BlackHearts;

	[SerializeField]
	[FormerlySerializedAs("_SpiritHearts")]
	protected float _TotalSpiritHearts;

	[SerializeField]
	protected float _SpiritHearts;

	public bool ArmoredFront;

	public bool invincible;

	public bool isPlayer;

	public bool isPlayerAlly;

	public List<GameObject> attackers = new List<GameObject>();

	public List<GameObject> followers = new List<GameObject>();

	public Task_Combat[] AttackPositions = new Task_Combat[4];

	public bool InanimateObject;

	public bool InanimateObjectEffect = true;

	public float ArrowAttackVulnerability = 1f;

	public float MeleeAttackVulnerability = 1f;

	public float DamageModifier = 1f;

	public bool untouchable;

	public bool DestroyOnDeath = true;

	public bool DontCombo;

	public bool CanBePoisoned = true;

	public bool CanBeIced = true;

	public bool CanBeCharmed = true;

	public bool CanBeElectrified = true;

	public bool ImmuneToDiseasedHearts;

	public bool ImmuneToTraps;

	public bool ImmuneToPlayer;

	public bool ImpactOnHit = true;

	public Color ImpactOnHitColor = Color.red;

	public float ImpactOnHitScale = 1f;

	public bool BloodOnHit;

	public bool BloodOnDie;

	public Color bloodColor = new Color(0.47f, 0.11f, 0.11f, 1f);

	public int BloodOnDieParticlesAmount = 5;

	public float BloodOnDieParticlesMultiplyer = 1f;

	public bool EmitGroundSmashDecal;

	public bool ScreenshakeOnHit = true;

	public bool ScreenshakeOnDie = true;

	public float ScreenShakeOnDieIntensity = 2f;

	public bool spawnParticles;

	public BiomeConstants.TypeOfParticle typeOfParticle;

	public bool SmokeOnDie;

	public bool OnHitBlockAttacker;

	public GameObject AttackerToBlock;

	[HideInInspector]
	public DealDamageEvent damageEventQueue;

	public static float DealDamageForgivenessWindow = 0.04f;

	public float autoAimAttractionFactor = 1f;

	public IMPACT_SFX ImpactSoundToPlay;

	public DEATH_SFX DeathSoundToPlay;

	public bool IgnoreLocationHPBuff;

	public bool BlackSoulOnHit;

	private Vector3 Velocity;

	protected int poisonCounter = -1;

	protected float enemyPoisonDuration = 5f;

	protected float enemyPoisonTimestamp = -1f;

	private GameObject poisonAttacker;

	private EnemyStasisTicker enemyPoisonTicker;

	private EventInstance PoisonLoopInstance;

	private bool createdPoisonLoop;

	protected int charmCounter = -1;

	protected float enemyCharmDuration = 5f;

	protected float enemyCharmTimestamp = -1f;

	protected float enemyLastCharmTimestamp = -1f;

	private EnemyStasisTicker enemyCharmTicker;

	private EventInstance CharmLoopInstance;

	private bool createdCharmLoop;

	protected int iceCounter = -1;

	protected float enemyIceDuration = 3f;

	protected float enemyIceTimestamp = -1f;

	private EnemyStasisTicker enemyIceTicker;

	private EventInstance IceLoopInstance;

	private bool createdIceLoop;

	private bool timeFrozen;

	protected int electrifiedCounter = -1;

	protected float enemyElectrifiedDuration = 1.5f;

	protected float enemyElectrifiedTimestamp = -1f;

	protected float enemyLastElectrifiedTimestamp = -1f;

	private EnemyStasisTicker enemyElectrifiedTicker;

	private EventInstance ElectrifiedLoopInstance;

	private bool createdElectrifiedLoop;

	private GameObject electrifiedAttacker;

	public virtual float totalHP
	{
		get
		{
			return _totalHP;
		}
		set
		{
			_totalHP = value;
		}
	}

	public virtual float HP
	{
		get
		{
			return _HP;
		}
		set
		{
			_HP = value;
		}
	}

	public virtual float BlueHearts
	{
		get
		{
			return _BlueHearts;
		}
		set
		{
			_BlueHearts = value;
		}
	}

	public virtual float BlackHearts
	{
		get
		{
			return _BlackHearts;
		}
		set
		{
			_BlackHearts = value;
		}
	}

	public virtual float TotalSpiritHearts
	{
		get
		{
			return _TotalSpiritHearts;
		}
		set
		{
			_TotalSpiritHearts = value;
		}
	}

	public virtual float SpiritHearts
	{
		get
		{
			return _SpiritHearts;
		}
		set
		{
			_SpiritHearts = value;
		}
	}

	public bool IsImmuneToAllStasis { get; private set; }

	public bool IsAilemented
	{
		get
		{
			if (!IsPoisoned && !IsCharmed)
			{
				return IsIced;
			}
			return true;
		}
	}

	public virtual float poisonTickDuration { get; set; } = 1f;


	protected virtual float playerPoisonDamage
	{
		get
		{
			return 1f;
		}
	}

	public virtual float enemyPoisonDamage { get; set; } = 0.3f;


	public float poisonTimer { get; set; }

	public bool IsPoisoned
	{
		get
		{
			return poisonCounter > 0;
		}
	}

	public ParticleSystem poisonedParticles { get; set; }

	public bool PoisonImmune { get; set; }

	public float charmTimer { get; set; }

	public bool IsCharmed
	{
		get
		{
			return charmCounter >= 0;
		}
	}

	public bool IsCharmedEnemy
	{
		get
		{
			if (team == Team.PlayerTeam && IsCharmed)
			{
				return !isPlayerAlly;
			}
			return false;
		}
	}

	public bool IsCharmedBuffer
	{
		get
		{
			if (charmCounter < 0)
			{
				return Time.time - enemyLastCharmTimestamp < 2f;
			}
			return true;
		}
	}

	public ParticleSystem charmParticles { get; set; }

	public bool CharmImmune { get; set; }

	public float iceTimer { get; set; }

	public bool IsIced
	{
		get
		{
			return iceCounter > 0;
		}
	}

	public ParticleSystem iceParticles { get; set; }

	public bool IceImmune { get; set; }

	public float electrifiedTimer { get; set; }

	public bool IsElectrified
	{
		get
		{
			return electrifiedCounter > 0;
		}
	}

	public bool IsElectrifiedBuffer
	{
		get
		{
			if (electrifiedCounter <= 0)
			{
				return Time.time - enemyLastElectrifiedTimestamp < 2f;
			}
			return true;
		}
	}

	public ParticleSystem electrifiedParticles { get; set; }

	public bool ElectrifiedImmune { get; set; }

	public virtual float electrifiedTickDuration { get; set; } = 0.3f;


	protected virtual float playerElectrifiedDamage
	{
		get
		{
			return 1f;
		}
	}

	public virtual float enemyElectrifiedDamage { get; set; } = 0.3f;


	public event DieAction OnDie;

	public static event DieAllAction OnDieAny;

	public event HitAction OnHit;

	public event HitAction OnHitEarly;

	public event HitAction OnPoisonedHit;

	public event HitAction OnPenetrationHit;

	public event Action OnDamageNegated;

	public event HealthEvent OnDamaged;

	public event StasisEvent OnStasisCleared;

	public event StasisEvent OnPoisoned;

	public event StasisEvent OnIced;

	public event StasisEvent OnFreezeTime;

	public event StasisEvent OnCharmed;

	public event StasisEvent OnElectrified;

	private void Awake()
	{
		InitHP();
		ClearPoison();
		ClearIce();
		ClearCharm();
		ClearElectrified();
	}

	public virtual void OnEnable()
	{
		switch (team)
		{
		default:
			playerTeam.Add(this);
			break;
		case Team.Neutral:
			if (!neutralTeam.Contains(this))
			{
				neutralTeam.Add(this);
			}
			break;
		case Team.Team2:
			if (!team2.Contains(this))
			{
				team2.Add(this);
			}
			break;
		case Team.DangerousAnimals:
			dangerousAnimals.Add(this);
			break;
		case Team.KillAll:
			killAll.Add(this);
			break;
		}
		if (!allUnits.Contains(this))
		{
			allUnits.Add(this);
		}
		state = GetComponent<StateMachine>();
		if (AttackerToBlock == null)
		{
			AttackerToBlock = base.gameObject;
		}
		ClearPoison();
		ClearIce();
		ClearCharm();
		ClearElectrified();
	}

	public virtual void InitHP()
	{
		if (isPlayer)
		{
			return;
		}
		totalHP /= DungeonModifier.HasPositiveModifier(DungeonPositiveModifier.HalfHealth, 2f, 1f);
		totalHP *= DifficultyManager.GetEnemyHealthMultiplier();
		totalHP *= DataManager.Instance.EnemyHealthMultiplier;
		if (team == Team.Team2 && !IgnoreLocationHPBuff)
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				totalHP += 0f;
				if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
				{
					totalHP += 4f;
				}
				break;
			case FollowerLocation.Dungeon1_2:
				totalHP += 2f;
				if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2))
				{
					totalHP += 4f;
				}
				break;
			case FollowerLocation.Dungeon1_3:
				totalHP += 4f;
				if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3))
				{
					totalHP += 4f;
				}
				break;
			case FollowerLocation.Dungeon1_4:
				totalHP += 6f;
				if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
				{
					totalHP += 4f;
				}
				break;
			}
			if (GameManager.DungeonEndlessLevel > 0 && GameManager.SandboxDungeonEnabled)
			{
				Debug.Log("GameManager.DungeonEndlessLevel: " + GameManager.DungeonEndlessLevel);
				MiniBossController componentInParent = GetComponentInParent<MiniBossController>();
				if (componentInParent != null && componentInParent.EnemiesToTrack.Count > 0 && componentInParent.EnemiesToTrack[0] == this)
				{
					totalHP += Mathf.Min(15 * (GameManager.DungeonEndlessLevel - 2), 150);
				}
				else
				{
					totalHP += Mathf.Min(GameManager.DungeonEndlessLevel - 2, 8);
				}
			}
			UnitObject component = GetComponent<UnitObject>();
			if ((bool)component)
			{
				switch (component.EnemyType)
				{
				case Enemy.WormBoss:
				case Enemy.FrogBoss:
				case Enemy.JellyBoss:
				case Enemy.SpiderBoss:
					if (DataManager.Instance.playerDeathsInARowFightingLeader >= 2)
					{
						totalHP *= 0.85f;
					}
					break;
				}
			}
		}
		HP = totalHP;
	}

	protected virtual void OnDisable()
	{
		if (isPlayer)
		{
			AudioManager.Instance.StopLoop(PoisonLoopInstance);
		}
		if (playerTeam.Contains(this))
		{
			playerTeam.Remove(this);
		}
		if (neutralTeam.Contains(this))
		{
			neutralTeam.Remove(this);
		}
		if (team2.Contains(this))
		{
			team2.Remove(this);
		}
		if (dangerousAnimals.Contains(this))
		{
			dangerousAnimals.Remove(this);
		}
		if (killAll.Contains(this))
		{
			killAll.Remove(this);
		}
		allUnits.Remove(this);
	}

	public static void DamageAllEnemies(float damage, DamageAllEnemiesType damageType)
	{
		GameManager.GetInstance().StartCoroutine(PlayerFarming.Instance.health.DamageAllEnemiesIE(damage, damageType));
	}

	private IEnumerator DamageAllEnemiesIE(float damage, DamageAllEnemiesType damageType)
	{
		foreach (Health item in new List<Health>(team2))
		{
			if (!(item == null) && !item.GetComponentInParent<Projectile>() && !item.ImmuneToDiseasedHearts)
			{
				switch (damageType)
				{
				case DamageAllEnemiesType.BlackHeart:
					BiomeConstants.Instance.ShowBlackHeartDamage(item.transform, Vector3.up);
					break;
				case DamageAllEnemiesType.DeathsDoor:
					BiomeConstants.Instance.ShowTarotCardDamage(item.transform, Vector3.up);
					break;
				case DamageAllEnemiesType.DamagePerFollower:
					BiomeConstants.Instance.ShowDamageTextIcon(item.transform, Vector3.up, damage);
					break;
				}
			}
		}
		yield return new WaitForSeconds(1f);
		foreach (Health item2 in new List<Health>(team2))
		{
			if (item2 != null)
			{
				item2.DealDamage(damage, base.gameObject, base.transform.position, false, AttackTypes.NoKnockBack);
			}
		}
	}

	public virtual bool DealDamage(float Damage, GameObject Attacker, Vector3 AttackLocation, bool BreakBlocking = false, AttackTypes AttackType = AttackTypes.Melee, bool dealDamageImmediately = false, AttackFlags AttackFlags = (AttackFlags)0)
	{
		if (!base.enabled)
		{
			return false;
		}
		if (invincible)
		{
			return false;
		}
		if (untouchable)
		{
			return false;
		}
		if (GodMode == CheatMode.God)
		{
			return false;
		}
		if (state != null && !dealDamageImmediately && (state.CURRENT_STATE == StateMachine.State.Dodging || state.CURRENT_STATE == StateMachine.State.InActive))
		{
			return false;
		}
		if (Attacker == base.gameObject && !isPlayer && IsCharmedBuffer)
		{
			return false;
		}
		if (Attacker == PlayerFarming.Instance.gameObject && ImmuneToPlayer)
		{
			return false;
		}
		if (isPlayer && (state.CURRENT_STATE == StateMachine.State.CustomAnimation || PlayerFarming.Instance.GoToAndStopping))
		{
			return false;
		}
		if (isPlayer && !DataManager.Instance.ShownDodgeTutorial && DataManager.Instance.ShownDodgeTutorialCount < 3)
		{
			UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UI Control Prompt Dodge") as GameObject, GameObject.FindWithTag("Canvas").transform).GetComponent<UIDodgePromptTutorial>().Play(Attacker);
			return false;
		}
		if (Attacker != null)
		{
			Velocity = AttackLocation - Attacker.transform.position;
		}
		if (isPlayer)
		{
			if (dealDamageImmediately)
			{
				MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact, false, true, this);
				damageEventQueue = null;
			}
			if (!dealDamageImmediately)
			{
				if (damageEventQueue == null)
				{
					damageEventQueue = new DealDamageEvent(Time.unscaledTime, Damage, Attacker, AttackLocation, BreakBlocking, AttackType);
					return true;
				}
				return false;
			}
			PlayerWeapon.EquippedWeaponsInfo currentWeapon = PlayerFarming.Instance.GetComponent<PlayerWeapon>().GetCurrentWeapon();
			bool flag = false;
			flag = ChanceToNegateDamage(currentWeapon.NegateDamageChance + TrinketManager.GetNegateDamageChance()) || flag;
			flag = (TrinketManager.CanNegateDamage() && state.CURRENT_STATE == StateMachine.State.Heal) || flag;
			PlayerWeapon component = GetComponent<PlayerWeapon>();
			if (component != null && component.ChargedNegation)
			{
				flag = true;
				component.ChargedNegation = false;
			}
			if (flag)
			{
				Action onDamageNegated = this.OnDamageNegated;
				if (onDamageNegated != null)
				{
					onDamageNegated();
				}
				NegatedDamage(AttackLocation);
				return false;
			}
			if (HP == 1f)
			{
				float chanceOfNegatingDeath = DifficultyManager.GetChanceOfNegatingDeath();
				chanceOfNegatingDeath += (PlayerWeapon.FirstTimeUsingWeapon ? 0.2f : 0f);
				if (UnityEngine.Random.Range(0f, 1f) <= chanceOfNegatingDeath)
				{
					return false;
				}
			}
			PlayerFarming.Instance.GetBlackSoul(TrinketManager.GetBlackSoulsOnDamaged(), false);
			if (TrinketManager.DropBlackGoopOnDamaged())
			{
				TrapGoop.CreateGoop(base.transform.position, 5, 0.5f, GenerateRoom.Instance.transform);
			}
			if (TrinketManager.DropBombOnDamaged() && !TrinketManager.IsOnCooldown(TarotCards.Card.BombOnDamaged))
			{
				Bomb.CreateBomb(base.transform.position, this, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent);
				TrinketManager.TriggerCooldown(TarotCards.Card.BombOnDamaged);
			}
			if (TrinketManager.DropTentacleOnDamaged() && !TrinketManager.IsOnCooldown(TarotCards.Card.TentacleOnDamaged))
			{
				float num = 10f;
				Tentacle t = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(EquipmentType.TENTACLE_TAROT_REF).Prefab, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent, true).GetComponent<Tentacle>();
				t.transform.position = base.transform.position;
				t.GetComponent<Health>().enabled = false;
				GameObject obj = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(EquipmentType.TENTACLE_TAROT_REF).SecondaryPrefab, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent);
				obj.transform.position = base.transform.position - Vector3.right;
				obj.GetComponent<FX_CrackController>().duration = num + 0.5f;
				float damage = EquipmentManager.GetCurseData(EquipmentType.TENTACLE_TAROT_REF).Damage;
				t.Play(0f, num, damage * PlayerSpells.GetCurseDamageMultiplier(), team, false, 0, true, true);
				TrinketManager.TriggerCooldown(TarotCards.Card.TentacleOnDamaged);
				CameraManager.instance.ShakeCameraForDuration(0.6f, 0.8f, 0.25f);
				AudioManager.Instance.PlayOneShot("event:/material/stone_break", base.gameObject);
				AudioManager.Instance.PlayOneShot("event:/followers/break_free", base.gameObject);
				BiomeConstants.Instance.EmitParticleChunk(BiomeConstants.TypeOfParticle.stone, t.transform.position, Vector3.one, 5);
				GameManager.GetInstance().WaitForSeconds(num + 0.5f, delegate
				{
					AudioManager.Instance.PlayOneShot("event:/material/stone_break", base.gameObject);
					AudioManager.Instance.PlayOneShot("event:/followers/break_free", base.gameObject);
					CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.2f);
					BiomeConstants.Instance.EmitSmokeExplosionVFX(t.transform.position);
					BiomeConstants.Instance.EmitParticleChunk(BiomeConstants.TypeOfParticle.stone, t.transform.position, Vector3.one, 10);
				});
			}
			PlayerFleeceManager.ResetDamageModifier();
			if (PlayerFleeceManager.FleeceCausesPoisonOnHit())
			{
				AddPoison(null, Mathf.Max(2f, DifficultyManager.GetInvincibleTimeMultiplier()));
			}
		}
		if (this.OnHitEarly != null)
		{
			this.OnHitEarly(Attacker, AttackLocation, AttackType);
		}
		HealthEvent onDamaged = this.OnDamaged;
		if (onDamaged != null)
		{
			onDamaged(Attacker, AttackLocation, Damage, AttackType, AttackFlags);
		}
		Damage *= DamageModifier;
		switch (ImpactSoundToPlay)
		{
		case IMPACT_SFX.IMPACT_BLUNT:
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_blunt", base.transform.position);
			break;
		case IMPACT_SFX.IMPACT_NORMAL:
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_normal", base.transform.position);
			break;
		case IMPACT_SFX.IMPACT_SQUISHY:
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", base.transform.position);
			break;
		case IMPACT_SFX.HIT_SMALL:
			AudioManager.Instance.PlayOneShot("event:/enemy/gethit_small", base.transform.position);
			break;
		case IMPACT_SFX.HIT_MEDIUM:
			AudioManager.Instance.PlayOneShot("event:/enemy/gethit_medium", base.transform.position);
			break;
		case IMPACT_SFX.HIT_LARGE:
			AudioManager.Instance.PlayOneShot("event:/enemy/gethit_large", base.transform.position);
			break;
		}
		if (AttackType == AttackTypes.Projectile)
		{
			Damage *= ArrowAttackVulnerability;
		}
		if (AttackType == AttackTypes.Melee)
		{
			Damage *= MeleeAttackVulnerability;
		}
		float angle = Utils.GetAngle(base.transform.position, AttackLocation);
		if (HasShield)
		{
			if (AttackFlags.HasFlag(AttackFlags.Penetration))
			{
				BiomeConstants.Instance.EmitBlockImpact(base.transform.position, angle, base.transform, "Break");
			}
			else
			{
				BiomeConstants.Instance.EmitBlockImpact(base.transform.position, angle, base.transform);
			}
			Damage *= 0.1f;
		}
		if ((bool)protector)
		{
			Attacker.gameObject.GetComponent<UnitObject>();
			ShowHPBar component2 = base.gameObject.GetComponent<ShowHPBar>();
			if (component2 != null && (bool)component2.hpBar && (bool)component2.hpBar.groupIndicator)
			{
				component2.hpBar.groupIndicator.transform.localScale = Vector3.one;
				component2.hpBar.groupIndicator.transform.DOKill();
				component2.hpBar.groupIndicator.transform.DOPunchScale(Vector3.one * 1.25f, 0.25f);
			}
			Damage *= protector.damageMultiplier;
		}
		if (Damage > 0f)
		{
			if (isPlayer)
			{
				DataManager.Instance.PlayerDamageReceived += Damage;
				DataManager.Instance.PlayerDamageReceivedThisRun += Damage;
			}
			else if (team == Team.Team2)
			{
				DataManager.Instance.PlayerDamageDealtThisRun += Damage;
				DataManager.Instance.PlayerDamageDealt += Damage;
				if (PlayerFarming.Instance.playerRelic.CurrentRelic != null && !AttackFlags.HasFlag(AttackFlags.DoesntChargeRelics))
				{
					PlayerFarming.Instance.playerRelic.IncreaseChargedAmount(Damage);
				}
			}
			else if (team == Team.Neutral)
			{
				PlayerFarming.Instance.playerRelic.IncreaseChargedAmount(Mathf.Min(Damage / 10f, 1f));
			}
		}
		if (!BreakBlocking && state != null && state.CURRENT_STATE == StateMachine.State.Defending)
		{
			GameObject obj2 = BiomeConstants.Instance.HitFX_Blocked.Spawn();
			obj2.transform.position = AttackLocation;
			obj2.transform.rotation = Quaternion.identity;
			Damage = 0f;
		}
		if (team == Team.PlayerTeam && !IsCharmedEnemy)
		{
			float value = DungeonModifier.HasNegativeModifier(DungeonNegativeModifier.DoubleDamage, 2f, 1f) + PlayerFleeceManager.GetDamageReceivedMultiplier();
			Damage *= Mathf.Clamp(value, 1f, 2f);
		}
		if (BlackSoulOnHit && Attacker != null && (bool)Attacker.GetComponent<PlayerFarming>() && AttackType == AttackTypes.Melee && team == Team.Team2 && base.gameObject.tag != "Projectile")
		{
			BlackSoul blackSoul = InventoryItem.SpawnBlackSoul(Mathf.RoundToInt(UnityEngine.Random.Range(1f, 2f) * TrinketManager.GetBlackSoulsMultiplier()), base.transform.position, true, true);
			if ((bool)blackSoul)
			{
				blackSoul.SetAngle(UnityEngine.Random.Range(0, 360), new Vector2(2f, 4f));
			}
		}
		float num2 = 0f;
		if (BlackHearts > 0f && Damage > 0f)
		{
			num2 = BlackHearts;
			BlackHearts -= Damage;
			Damage -= num2;
			if (BlackHearts < 0f)
			{
				BlackHearts = 0f;
			}
			StartCoroutine(DamageAllEnemiesIE(1.25f + DataManager.GetWeaponDamageMultiplier(DataManager.Instance.CurrentWeaponLevel) * 3f, DamageAllEnemiesType.BlackHeart));
		}
		if (BlueHearts > 0f && Damage > 0f)
		{
			num2 = BlueHearts;
			BlueHearts -= Damage;
			Damage -= num2;
			if (BlueHearts < 0f)
			{
				BlueHearts = 0f;
			}
		}
		if (SpiritHearts > 0f && Damage > 0f)
		{
			num2 = SpiritHearts;
			SpiritHearts -= Damage;
			Damage -= num2;
			if (SpiritHearts < 0f)
			{
				SpiritHearts = 0f;
			}
		}
		if (GodMode == CheatMode.Demigod)
		{
			Damage = 0f;
		}
		if (Damage > 0f)
		{
			HP -= Damage;
		}
		if (team != 0 && !IsPoisoned)
		{
			bool flag2 = false;
			float num3 = 0f;
			float num4 = 0f;
			bool flag3 = false;
			if (AttackFlags.HasFlag(AttackFlags.Poison))
			{
				flag3 = true;
				flag2 = true;
				if (AttackType == AttackTypes.Projectile)
				{
					num3 = 0.2f * (float)DataManager.Instance.CurrentCurseLevel;
				}
			}
			if ((AttackType == AttackTypes.Melee || AttackType == AttackTypes.Heavy) && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon) != null && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).ContainsAttachmentType(AttachmentEffect.Poison) && !isPlayer && !isPlayerAlly && Attacker == PlayerFarming.Instance.gameObject)
			{
				float value2 = UnityEngine.Random.value;
				float poisonChance = EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).GetAttachment(AttachmentEffect.Poison).poisonChance;
				if (flag3 || value2 <= poisonChance)
				{
					flag2 = true;
					num4 = 0.2f * (float)DataManager.Instance.CurrentWeaponLevel;
				}
			}
			enemyPoisonDamage = Mathf.Max(enemyPoisonDamage, num4, num3);
			if (flag2)
			{
				AddPoison(Attacker);
			}
		}
		HP = Mathf.Clamp(HP, 0f, float.MaxValue);
		bool flag4 = HP + BlueHearts + SpiritHearts + BlackHearts <= 0f && GodMode != CheatMode.Immortal;
		if (!flag4)
		{
			bool fromBehind = false;
			if (state != null && Attacker != null && base.transform != null)
			{
				fromBehind = Mathf.Abs(state.facingAngle - Utils.GetAngle(base.transform.position, Attacker.transform.position) % 360f) >= 150f;
			}
			if (AttackType == AttackTypes.Poison || AttackType == AttackTypes.Electrified)
			{
				ShowHPBar component3 = GetComponent<ShowHPBar>();
				if ((object)component3 != null)
				{
					component3.OnHit(Attacker, AttackLocation, AttackType, fromBehind);
				}
				UIBossHUD instance = UIBossHUD.Instance;
				if ((object)instance != null)
				{
					instance.OnBossHit(Attacker, AttackLocation, AttackType, fromBehind);
				}
				HitAction onPoisonedHit = this.OnPoisonedHit;
				if (onPoisonedHit != null)
				{
					onPoisonedHit(Attacker, AttackLocation, AttackTypes.Poison, fromBehind);
				}
			}
			if ((AttackType != AttackTypes.Poison && AttackType != AttackTypes.Electrified) || isPlayer)
			{
				HitAction onHit = this.OnHit;
				if (onHit != null)
				{
					onHit(Attacker, AttackLocation, AttackType, fromBehind);
				}
				UnityEvent onHitCallback = OnHitCallback;
				if (onHitCallback != null)
				{
					onHitCallback.Invoke();
				}
			}
			if (AttackFlags.HasFlag(AttackFlags.Penetration))
			{
				HitAction onPenetrationHit = this.OnPenetrationHit;
				if (onPenetrationHit != null)
				{
					onPenetrationHit(Attacker, AttackLocation, AttackType, fromBehind);
				}
			}
		}
		Vector3 vector = base.transform.position - AttackLocation;
		angle = Utils.GetAngle(base.transform.position, AttackLocation);
		if (Attacker != null)
		{
			StateMachine component4 = Attacker.GetComponent<StateMachine>();
			if (base.gameObject.tag != "Player")
			{
				if (InanimateObject)
				{
					if (InanimateObjectEffect)
					{
						BiomeConstants.Instance.EmitHitVFX(base.transform.position + Vector3.back, Quaternion.identity.z, "HitFX_Weak");
					}
				}
				else if (ImpactOnHit && team != 0)
				{
					if (!AttackFlags.HasFlag(AttackFlags.Crit) || InanimateObject)
					{
						BiomeConstants.Instance.PlayerEmitHitImpactEffect(base.transform.position + Vector3.back * 0.5f, (component4 != null) ? component4.facingAngle : angle, false, ImpactOnHitColor, ImpactOnHitScale, AttackFlags.HasFlag(AttackFlags.Crit));
					}
					else
					{
						GameManager.GetInstance().HitStop(0.2f);
						BiomeConstants.Instance.PlayerEmitHitImpactEffect(base.transform.position + Vector3.back * 0.5f, (component4 != null) ? component4.facingAngle : angle, true, default(Color), 1f, AttackFlags.HasFlag(AttackFlags.Crit));
					}
				}
			}
			else if (AttackFlags.HasFlag(AttackFlags.Crit))
			{
				if (!flag4)
				{
					BiomeConstants.Instance.PlayerEmitHitImpactEffect(base.transform.position + Vector3.back * 0.5f, (component4 != null) ? component4.facingAngle : angle, true, default(Color), 1f, AttackFlags.HasFlag(AttackFlags.Crit));
				}
				else
				{
					BiomeConstants.Instance.PlayerEmitHitImpactEffect(base.transform.position + Vector3.back * 0.5f, (component4 != null) ? component4.facingAngle : angle, false, default(Color), 1f, AttackFlags.HasFlag(AttackFlags.Crit));
				}
			}
			if (team != 0)
			{
				if (AttackFlags.HasFlag(AttackFlags.Charm))
				{
					AddCharm();
				}
				else if (AttackFlags.HasFlag(AttackFlags.Ice))
				{
					AddIce();
				}
				else if (AttackFlags.HasFlag(AttackFlags.Electrified))
				{
					AddElectrified(Attacker);
				}
			}
		}
		if (BloodOnHit)
		{
			Vector3 vector2 = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
			if (base.gameObject.tag != "Player" && !flag4)
			{
				if (AttackType == AttackTypes.Heavy)
				{
					BiomeConstants.Instance.EmitBloodSplatter(vector2, vector.normalized, bloodColor);
					BiomeConstants.Instance.EmitBloodImpact(vector2 + Vector3.back * 0.5f, angle, "black");
					string[] array = new string[2] { "BloodImpact_Large_0", "BloodImpact_Large_1" };
					int num5 = UnityEngine.Random.Range(0, array.Length - 1);
					if (array[num5] != null)
					{
						BiomeConstants.Instance.EmitBloodImpact(vector2 + Vector3.back * 0.5f, angle, "black", array[num5]);
					}
				}
				else
				{
					BiomeConstants.Instance.EmitBloodSplatter(vector2, vector.normalized, bloodColor);
					string[] array2 = new string[3] { "BloodImpact_0", "BloodImpact_1", "BloodImpact_2" };
					int num6 = UnityEngine.Random.Range(0, array2.Length - 1);
					if (array2[num6] != null)
					{
						BiomeConstants.Instance.EmitBloodImpact(vector2 + Vector3.back * 0.5f, angle, "black", array2[num6]);
					}
				}
			}
			else if (AttackType == AttackTypes.Heavy)
			{
				BiomeConstants.Instance.EmitBloodSplatterGroundParticles(vector2, Velocity, bloodColor);
				BiomeConstants.Instance.EmitBloodImpact(vector2 + Vector3.back * 0.5f, angle, "black");
				string[] array3 = new string[2] { "BloodImpact_Large_0", "BloodImpact_Large_1" };
				int num7 = UnityEngine.Random.Range(0, array3.Length - 1);
				if (array3[num7] != null)
				{
					BiomeConstants.Instance.EmitBloodImpact(vector2 + Vector3.back * 0.5f, angle, "black", array3[num7], false);
				}
			}
			else
			{
				BiomeConstants.Instance.EmitBloodSplatterGroundParticles(vector2, Velocity, bloodColor);
				BiomeConstants.Instance.EmitBloodSplatter(vector2, vector.normalized, bloodColor);
				string[] array4 = new string[3] { "BloodImpact_0", "BloodImpact_1", "BloodImpact_2" };
				int num8 = UnityEngine.Random.Range(0, array4.Length - 1);
				if (array4[num8] != null)
				{
					BiomeConstants.Instance.EmitBloodImpact(vector2 + Vector3.back * 0.5f, angle, "black", array4[num8], false);
				}
			}
		}
		if (ScreenshakeOnHit && !(ScreenshakeOnDie && flag4))
		{
			CameraManager.shakeCamera(ScreenShakeOnDieIntensity / 3f);
		}
		if (isPlayer && PlayerFarming.Instance.health.HP + PlayerFarming.Instance.health.BlueHearts + PlayerFarming.Instance.health.BlackHearts + PlayerFarming.Instance.health.SpiritHearts <= 1f && TrinketManager.HasTrinket(TarotCards.Card.DeathsDoor))
		{
			StartCoroutine(DamageAllEnemiesIE((float)TrinketManager.GetDamageAllEnemiesAmount(TarotCards.Card.DeathsDoor) + DataManager.GetWeaponDamageMultiplier(DataManager.Instance.CurrentWeaponLevel) * 3f, DamageAllEnemiesType.DeathsDoor));
		}
		if (Attacker != null && team == Team.Team2 && AttackType == AttackTypes.Projectile && (bool)Attacker.GetComponentInParent<MegaSlash>())
		{
			if (DataManager.Instance.CurrentCurse == EquipmentType.MegaSlash_Ice && UnityEngine.Random.value <= EquipmentManager.GetCurseData(EquipmentType.MegaSlash_Ice).Chance)
			{
				AddIce();
			}
			else if (DataManager.Instance.CurrentCurse == EquipmentType.MegaSlash_Charm && UnityEngine.Random.value <= EquipmentManager.GetCurseData(EquipmentType.MegaSlash_Charm).Chance)
			{
				AddCharm();
			}
		}
		if (flag4)
		{
			if (team == Team.Team2)
			{
				DataManager.Instance.PlayerKillsOnRun++;
				DataManager.Instance.KillsInGame++;
				if (Attacker != null && ((bool)Attacker.GetComponentInParent<PlayerFarming>() || (bool)Attacker.GetComponentInParent<MegaSlash>()) && ((AttackType == AttackTypes.Melee && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).ContainsAttachmentType(AttachmentEffect.Necromancy) && UnityEngine.Random.value <= EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).GetAttachment(AttachmentEffect.Necromancy).necromancyChance) || (AttackType == AttackTypes.Projectile && DataManager.Instance.CurrentCurse == EquipmentType.MegaSlash_Necromancy)))
				{
					ProjectileGhost.SpawnGhost(base.transform.position, 1f + DataManager.GetWeaponDamageMultiplier(DataManager.Instance.CurrentWeaponLevel));
				}
			}
			switch (DeathSoundToPlay)
			{
			case DEATH_SFX.DEATH_SMALL:
				AudioManager.Instance.PlayOneShot("event:/enemy/enemy_death_small", base.transform.position);
				break;
			case DEATH_SFX.DEATH_MEDIUM:
				AudioManager.Instance.PlayOneShot("event:/enemy/enemy_death_medium", base.transform.position);
				break;
			case DEATH_SFX.DEATH_LARGE:
				AudioManager.Instance.PlayOneShot("event:/enemy/enemy_death_large", base.transform.position);
				break;
			}
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact, false, true, GameManager.GetInstance());
			if (DungeonModifier.HasNegativeModifier(DungeonNegativeModifier.DropPoison) && team == Team.Team2)
			{
				TrapPoison.CreatePoison(base.transform.position, 5, 0.5f, base.transform.parent);
			}
			if (SmokeOnDie && !isPlayer)
			{
				BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position + Vector3.back * 0.5f);
			}
			if (BloodOnDie)
			{
				if (!isPlayer)
				{
					BiomeConstants.Instance.EmitBloodSplatter(base.transform.position, vector.normalized, bloodColor);
				}
				BiomeConstants.Instance.EmitBloodSplatterGroundParticles(base.transform.position, Velocity, bloodColor);
				BiomeConstants.Instance.EmitBloodImpact(base.transform.position + Vector3.back * 0.5f, angle, "black", null, !isPlayer);
				string[] array5 = new string[2] { "BloodImpact_Large_0", "BloodImpact_Large_1" };
				int num9 = UnityEngine.Random.Range(0, 1);
				BiomeConstants.Instance.EmitBloodImpact(base.transform.position + Vector3.back * 0.5f, angle, "black", array5[num9], !isPlayer);
			}
			if (spawnParticles)
			{
				BiomeConstants.Instance.EmitParticleChunk(typeOfParticle, base.transform.position, Velocity, 6);
			}
			if (ScreenshakeOnDie)
			{
				CameraManager.shakeCamera(ScreenShakeOnDieIntensity);
			}
			if (EmitGroundSmashDecal)
			{
				BiomeConstants.Instance.EmitGroundSmashVFXParticles(new Vector3(base.transform.position.x, base.transform.position.y, 0f));
			}
			HP = 0f;
			Health health = null;
			bool flag5 = false;
			if (Attacker != null)
			{
				health = Attacker.GetComponent<Health>();
				if (health == null)
				{
					flag5 = Attacker.GetComponent<Demon>() != null;
				}
			}
			if ((bool)health && health.team == Team.PlayerTeam && team == Team.Team2)
			{
				PlayerWeapon.EquippedWeaponsInfo currentWeapon2 = PlayerFarming.Instance.GetComponent<PlayerWeapon>().GetCurrentWeapon();
				health.ChanceToHeal(currentWeapon2.HealChance + TrinketManager.GetHealChance(), 1f);
				if (PlayerFarming.Instance.health.BlueHearts < 4f)
				{
					health.ChanceToGainBlueHeart(TrinketManager.GetChanceOfGainingBlueHeart());
				}
				if (CanIncreaseDamageMultiplier)
				{
					PlayerFleeceManager.IncrementDamageModifier();
				}
				if (Attacker != null && AttackType == AttackTypes.Projectile && (bool)Attacker.GetComponentInParent<MegaSlash>() && DataManager.Instance.CurrentCurse == EquipmentType.MegaSlash_Ice && UnityEngine.Random.value <= EquipmentManager.GetCurseData(EquipmentType.MegaSlash_Ice).Chance)
				{
					AddIce();
				}
			}
			else if (Attacker != null && team == Team.Team2 && (IsAttackerACurseProduct(Attacker) || flag5) && CanIncreaseDamageMultiplier)
			{
				PlayerFleeceManager.IncrementDamageModifier();
			}
			ClearElectrified();
			if (this.OnDie != null)
			{
				this.OnDie(Attacker, AttackLocation, this, AttackType, AttackFlags);
			}
			if (Health.OnDieAny != null)
			{
				Health.OnDieAny(this);
			}
			if (OnDieCallback != null)
			{
				OnDieCallback.Invoke();
			}
			if (DestroyOnDeath)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				base.enabled = false;
				if ((bool)state && !(this is HealthPlayer))
				{
					state.CURRENT_STATE = StateMachine.State.Dead;
				}
			}
		}
		return true;
	}

	private void NegatedDamage(Vector3 attackPosition)
	{
		BiomeConstants.Instance.EmitBlockImpact(base.transform.position, Utils.GetAngle(base.transform.position, attackPosition));
	}

	private bool IsAttackerACurseProduct(GameObject attacker)
	{
		ICurseProduct curseProduct = attacker.GetComponent<ICurseProduct>();
		if (curseProduct == null)
		{
			curseProduct = attacker.GetComponentInParent<ICurseProduct>();
		}
		return curseProduct != null;
	}

	public void ForgiveRecentDamage()
	{
		DealDamageEvent damageEventQueue2 = damageEventQueue;
		damageEventQueue = null;
	}

	public bool IsAttackerInDamageEventQueue(GameObject attacker)
	{
		if (damageEventQueue == null)
		{
			return false;
		}
		if (damageEventQueue.Attacker == null)
		{
			return false;
		}
		if (damageEventQueue.Attacker == attacker)
		{
			return true;
		}
		return false;
	}

	protected virtual void Update()
	{
		if (damageEventQueue != null && GameManager.GetInstance().UnscaledTimeSince(damageEventQueue.UnscaledTimestamp) >= DealDamageForgivenessWindow)
		{
			if (damageEventQueue.Attacker != null)
			{
				DealDamage(damageEventQueue.Damage, damageEventQueue.Attacker, damageEventQueue.AttackLocation, damageEventQueue.BreakBlocking, damageEventQueue.AttackType, true);
			}
			else
			{
				damageEventQueue = null;
			}
		}
		WasJustParried = false;
		PoisonCalculate();
		CharmCalculate();
		IceCalculate();
		ElectrifiedCalculate();
	}

	private void PoisonCalculate()
	{
		if (HP + BlueHearts + SpiritHearts + BlackHearts > 0f && poisonCounter > 0)
		{
			poisonTimer += Time.deltaTime;
			float num = poisonTimer / poisonTickDuration;
			if (!createdPoisonLoop && isPlayer)
			{
				PoisonLoopInstance = AudioManager.Instance.CreateLoop("event:/player/poison_loop", base.gameObject, true);
				PoisonLoopInstance.setParameterByName("parameter:/poison", num);
				createdPoisonLoop = true;
			}
			if (num > 1f)
			{
				if (isPlayer)
				{
					AudioManager.Instance.PlayOneShot("event:/player/poisoned", base.gameObject);
					AudioManager.Instance.StopLoop(PoisonLoopInstance);
					createdPoisonLoop = false;
				}
				if (isPlayer && PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Dodging)
				{
					AudioManager.Instance.StopLoop(PoisonLoopInstance);
					createdPoisonLoop = false;
				}
				else
				{
					AudioManager.Instance.PlayOneShot("event:/player/poison_damage", base.gameObject);
					if (base.transform != null)
					{
						DealDamage(isPlayer ? playerPoisonDamage : enemyPoisonDamage, poisonAttacker, base.transform.position, false, AttackTypes.Poison, true);
					}
					SimpleSpineFlash[] componentsInChildren = base.gameObject.GetComponentsInChildren<SimpleSpineFlash>();
					if (componentsInChildren.Length != 0)
					{
						SimpleSpineFlash[] array = componentsInChildren;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].FlashFillGreen();
						}
					}
					else
					{
						SimpleSpineAnimator componentInChildren = GetComponentInChildren<SimpleSpineAnimator>();
						if ((object)componentInChildren != null)
						{
							componentInChildren.FlashFillGreen();
						}
					}
				}
				poisonTimer = 0f;
			}
			else if (num <= 0f && isPlayer)
			{
				AudioManager.Instance.StopLoop(PoisonLoopInstance);
				createdPoisonLoop = false;
			}
			if (enemyPoisonTimestamp != -1f && Time.time > enemyPoisonTimestamp && team != Team.PlayerTeam)
			{
				poisonCounter = 0;
			}
		}
		else if (HP + BlueHearts + BlackHearts + SpiritHearts > 0f && poisonCounter == 0)
		{
			if (isPlayer)
			{
				AudioManager.Instance.StopLoop(PoisonLoopInstance);
				createdPoisonLoop = false;
			}
			poisonTimer -= Time.deltaTime;
			if (poisonTimer <= 0f)
			{
				ClearPoison();
			}
		}
	}

	private void IceCalculate()
	{
		if (HP > 0f && iceCounter > 0)
		{
			if (!createdIceLoop && isPlayer)
			{
				createdIceLoop = true;
			}
			if (enemyIceTimestamp != -1f && Time.time > enemyIceTimestamp && team != Team.PlayerTeam)
			{
				iceCounter = 0;
			}
		}
		else if (HP > 0f && iceCounter == 0)
		{
			if (isPlayer)
			{
				AudioManager.Instance.StopLoop(IceLoopInstance);
				createdIceLoop = false;
			}
			ClearIce();
		}
	}

	private void CharmCalculate()
	{
		if (HP > 0f && charmCounter > 0)
		{
			charmTimer += Time.deltaTime;
			if (!createdCharmLoop && isPlayer)
			{
				createdCharmLoop = true;
			}
			if ((enemyCharmTimestamp != -1f && Time.time > enemyCharmTimestamp) || team2.Count <= 1)
			{
				charmCounter = 0;
				enemyLastCharmTimestamp = Time.time;
			}
		}
		else if (HP > 0f && charmCounter == 0)
		{
			if (isPlayer)
			{
				AudioManager.Instance.StopLoop(CharmLoopInstance);
				createdCharmLoop = false;
			}
			charmTimer -= Time.deltaTime;
			if (charmTimer <= 0f)
			{
				ClearCharm();
			}
		}
	}

	private void ElectrifiedCalculate()
	{
		if (HP + BlueHearts + SpiritHearts + BlackHearts > 0f && electrifiedCounter > 0)
		{
			electrifiedTimer += Time.deltaTime;
			float num = electrifiedTimer / electrifiedTickDuration;
			if (!createdElectrifiedLoop && isPlayer)
			{
				createdElectrifiedLoop = true;
			}
			if (num > 1f)
			{
				if (isPlayer)
				{
					createdElectrifiedLoop = false;
				}
				if (!isPlayer || PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Dodging)
				{
					if (base.transform != null)
					{
						DealDamage(isPlayer ? playerElectrifiedDamage : enemyElectrifiedDamage, electrifiedAttacker, base.transform.position, false, AttackTypes.Electrified, true);
					}
					SimpleSpineFlash[] componentsInChildren = base.gameObject.GetComponentsInChildren<SimpleSpineFlash>();
					if (componentsInChildren.Length != 0)
					{
						SimpleSpineFlash[] array = componentsInChildren;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].FlashFillBlack(true);
						}
					}
					else
					{
						SimpleSpineAnimator componentInChildren = GetComponentInChildren<SimpleSpineAnimator>();
						if ((object)componentInChildren != null)
						{
							componentInChildren.FlashFillBlack(true);
						}
					}
				}
				electrifiedTimer = 0f;
			}
			if (enemyElectrifiedTimestamp != -1f && Time.time > enemyElectrifiedTimestamp && team != Team.PlayerTeam)
			{
				electrifiedCounter = 0;
			}
		}
		else if (HP + BlueHearts + BlackHearts + SpiritHearts > 0f && electrifiedCounter == 0)
		{
			if (isPlayer)
			{
				AudioManager.Instance.StopLoop(ElectrifiedLoopInstance);
				createdElectrifiedLoop = false;
			}
			electrifiedTimer -= Time.deltaTime;
			if (electrifiedTimer <= 0f)
			{
				ClearElectrified();
			}
		}
	}

	private void OnDestroy()
	{
		ClearElectrified();
	}

	public virtual void Heal(float healing)
	{
		float num = 0f;
		if (HP < totalHP && healing > 0f)
		{
			num = totalHP - HP;
			HP += healing;
			healing -= num;
			if (HP > totalHP)
			{
				HP = totalHP;
			}
		}
		if (SpiritHearts < TotalSpiritHearts && healing > 0f)
		{
			num = TotalSpiritHearts - SpiritHearts;
			SpiritHearts += healing;
			healing -= num;
			if (SpiritHearts > TotalSpiritHearts)
			{
				SpiritHearts = TotalSpiritHearts;
			}
		}
	}

	public void DestroyNextFrame()
	{
		StartCoroutine(DestroyNextFrameRoutine());
	}

	public bool ChanceToNegateDamage(float chance)
	{
		return UnityEngine.Random.Range(0f, 1f) <= chance;
	}

	public void ChanceToHeal(float chance, float healAmount)
	{
		if (UnityEngine.Random.Range(0f, 1f) <= chance)
		{
			Heal(healAmount);
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_small");
		}
	}

	public void ChanceToGainBlueHeart(float chance)
	{
		if (UnityEngine.Random.Range(0f, 1f) <= chance)
		{
			BlueHearts += TrinketManager.GetHealthAmountMultiplier();
			BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "blue", "burst_big");
		}
	}

	private IEnumerator SpawnParticle()
	{
		Transform transform1 = base.transform;
		Vector3 LastPos = transform1.position;
		yield return null;
		Velocity = (transform1.position - LastPos) * 50f;
		BiomeConstants.Instance.EmitParticleChunk(typeOfParticle, transform1.position, Velocity, 6);
	}

	private IEnumerator emitGroundBloodParticles()
	{
		Vector3 LastPos = base.transform.position;
		yield return null;
		Velocity = (base.transform.position - LastPos) * 50f;
		BiomeConstants.Instance.EmitBloodSplatterGroundParticles(base.transform.position, Velocity, bloodColor);
	}

	private IEnumerator DestroyNextFrameRoutine()
	{
		yield return new WaitForEndOfFrame();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void AddPoison(GameObject attacker, float duration = -1f)
	{
		if (IsAilemented)
		{
			return;
		}
		if (PoisonImmune || (isPlayer && TrinketManager.IsPoisonImmune()) || !CanBePoisoned)
		{
			ClearPoison();
			return;
		}
		if (poisonCounter == -1)
		{
			poisonCounter = 0;
			poisonTimer = 0f;
			if (!isPlayer && (bool)GetComponent<UnitObject>())
			{
				UnitObject component = GetComponent<UnitObject>();
				GetComponent<UnitObject>().SpeedMultiplier = 0.65f;
				SimpleSpineFlash[] componentsInChildren = component.GetComponentsInChildren<SimpleSpineFlash>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Tint(Color.green);
				}
			}
			StasisEvent onPoisoned = this.OnPoisoned;
			if (onPoisoned != null)
			{
				onPoisoned();
			}
		}
		poisonCounter++;
		poisonAttacker = attacker;
		PlayPoisonedParticles();
		enemyPoisonTimestamp = Time.time + enemyPoisonDuration;
		if (duration != -1f)
		{
			if (isPlayer)
			{
				StartCoroutine(ClearPoisonAfterTime(duration));
			}
			else
			{
				enemyPoisonTimestamp = Time.time + duration;
			}
		}
		if (team != Team.Team2 || !(enemyPoisonTicker == null))
		{
			return;
		}
		ShowHPBar component2 = GetComponent<ShowHPBar>();
		if ((bool)component2)
		{
			Vector2 offset = new Vector2(component2.StasisXOffset, component2.zOffset);
			EnemyStasisTicker.Instantiate(this, offset, AttackTypes.Poison, delegate(EnemyStasisTicker r)
			{
				enemyPoisonTicker = r;
			});
		}
	}

	private IEnumerator ClearPoisonAfterTime(float t)
	{
		yield return new WaitForSeconds(t);
		ClearPoison();
	}

	public void RemovePoison()
	{
		poisonCounter = Mathf.Clamp(poisonCounter - 1, 0, int.MaxValue);
		if (!isPlayer && (bool)GetComponent<UnitObject>())
		{
			GetComponent<UnitObject>().SpeedMultiplier = 1f;
		}
	}

	public void ClearPoison()
	{
		if (poisonCounter == -1)
		{
			return;
		}
		AudioManager.Instance.StopLoop(PoisonLoopInstance);
		poisonTimer = 0f;
		poisonCounter = -1;
		poisonAttacker = null;
		StopPoisonedParticles();
		if (!isPlayer && (bool)GetComponent<UnitObject>())
		{
			SimpleSpineFlash[] componentsInChildren = GetComponent<UnitObject>().GetComponentsInChildren<SimpleSpineFlash>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Tint(Color.white);
			}
		}
		if (enemyPoisonTicker != null)
		{
			enemyPoisonTicker.Hide();
			enemyPoisonTicker = null;
		}
		StasisEvent onStasisCleared = this.OnStasisCleared;
		if (onStasisCleared != null)
		{
			onStasisCleared();
		}
	}

	public void PlayPoisonedParticles()
	{
		if (!(poisonedParticles == null))
		{
			poisonedParticles.Play();
		}
	}

	public void StopPoisonedParticles()
	{
		if (!(poisonedParticles == null))
		{
			poisonedParticles.Stop();
		}
	}

	public void AddCharm()
	{
		if (IsAilemented || IsImmuneToAllStasis)
		{
			return;
		}
		if (CharmImmune || !CanBeCharmed)
		{
			ClearCharm();
			return;
		}
		if (charmCounter == -1)
		{
			charmCounter = 0;
			charmTimer = 0f;
			if (!isPlayer && (bool)GetComponent<UnitObject>())
			{
				SimpleSpineFlash[] componentsInChildren = GetComponent<UnitObject>().GetComponentsInChildren<SimpleSpineFlash>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Tint(Color.yellow);
				}
			}
			AudioManager.Instance.PlayOneShot("event:/player/Curses/enemy_charmed", base.transform.position);
			StasisEvent onCharmed = this.OnCharmed;
			if (onCharmed != null)
			{
				onCharmed();
			}
		}
		charmCounter++;
		PlayCharmParticles();
		enemyCharmTimestamp = Time.time + enemyCharmDuration;
		if (team == Team.Team2 && enemyCharmTicker == null)
		{
			ShowHPBar component = GetComponent<ShowHPBar>();
			if ((bool)component)
			{
				Vector2 offset = new Vector2(component.StasisXOffset, component.zOffset);
				EnemyStasisTicker.Instantiate(this, offset, AttackTypes.Charm, delegate(EnemyStasisTicker r)
				{
					enemyCharmTicker = r;
				});
			}
		}
		team = Team.PlayerTeam;
	}

	public void ClearCharm()
	{
		if (charmCounter == -1)
		{
			return;
		}
		charmCounter = -1;
		StopCharmParticles();
		team = Team.Team2;
		if (!isPlayer && (bool)GetComponent<UnitObject>())
		{
			SimpleSpineFlash[] componentsInChildren = GetComponent<UnitObject>().GetComponentsInChildren<SimpleSpineFlash>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Tint(Color.white);
			}
		}
		if (enemyCharmTicker != null)
		{
			enemyCharmTicker.Hide();
			enemyCharmTicker = null;
		}
		StasisEvent onStasisCleared = this.OnStasisCleared;
		if (onStasisCleared != null)
		{
			onStasisCleared();
		}
	}

	public void PlayCharmParticles()
	{
		if (!(charmParticles == null))
		{
			charmParticles.Play();
		}
	}

	public void StopCharmParticles()
	{
		if (!(charmParticles == null))
		{
			charmParticles.Stop();
		}
	}

	public void AddIce(float duration = -1f)
	{
		if (IsAilemented || IsImmuneToAllStasis)
		{
			return;
		}
		if (IceImmune || !CanBeIced)
		{
			ClearIce();
			return;
		}
		if (iceCounter == -1)
		{
			iceCounter = 0;
			iceTimer = 0f;
			if (!isPlayer && (bool)GetComponent<UnitObject>())
			{
				UnitObject component = GetComponent<UnitObject>();
				component.SpeedMultiplier = 0.1f;
				SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].timeScale = 0.25f;
				}
				SimpleSpineFlash[] componentsInChildren2 = component.GetComponentsInChildren<SimpleSpineFlash>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].Tint(Color.cyan);
				}
				if ((bool)component.rb)
				{
					component.rb.drag *= 2f;
				}
			}
			StasisEvent onIced = this.OnIced;
			if (onIced != null)
			{
				onIced();
			}
		}
		iceCounter++;
		PlayIceParticles();
		enemyIceTimestamp = Time.time + enemyIceDuration;
		if (duration != -1f)
		{
			enemyIceTimestamp = Time.time + duration;
		}
		if (team != Team.Team2 || !(enemyIceTicker == null))
		{
			return;
		}
		ShowHPBar component2 = GetComponent<ShowHPBar>();
		if ((bool)component2)
		{
			Vector2 offset = new Vector2(component2.StasisXOffset, component2.zOffset);
			EnemyStasisTicker.Instantiate(this, offset, AttackTypes.Ice, delegate(EnemyStasisTicker r)
			{
				enemyIceTicker = r;
			});
		}
	}

	public void ClearIce()
	{
		if (iceCounter == -1)
		{
			return;
		}
		iceCounter = -1;
		StopIceParticles();
		if (!isPlayer && (bool)GetComponent<UnitObject>())
		{
			UnitObject component = GetComponent<UnitObject>();
			component.SpeedMultiplier = 1f;
			SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].timeScale = 1f;
			}
			SimpleSpineFlash[] componentsInChildren2 = component.GetComponentsInChildren<SimpleSpineFlash>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Tint(Color.white);
			}
			if ((bool)component.rb)
			{
				component.rb.drag /= 2f;
			}
		}
		if (enemyIceTicker != null)
		{
			enemyIceTicker.Hide();
			enemyIceTicker = null;
		}
		StasisEvent onStasisCleared = this.OnStasisCleared;
		if (onStasisCleared != null)
		{
			onStasisCleared();
		}
	}

	public void PlayIceParticles()
	{
		if (!(iceParticles == null))
		{
			iceParticles.Play();
		}
	}

	public void StopIceParticles()
	{
		if (!(iceParticles == null))
		{
			iceParticles.Stop();
		}
	}

	public void AddFreezeTime(float duration = -1f)
	{
		if (!isPlayer && (bool)GetComponent<UnitObject>() && !timeFrozen)
		{
			UnitObject component = GetComponent<UnitObject>();
			component.SpeedMultiplier = 0.0001f;
			SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].timeScale = 0.0001f;
			}
			if ((bool)component.rb)
			{
				component.rb.drag *= 100f;
			}
			component.health.invincible = false;
			timeFrozen = true;
		}
		StasisEvent onFreezeTime = this.OnFreezeTime;
		if (onFreezeTime != null)
		{
			onFreezeTime();
		}
	}

	public void ClearFreezeTime()
	{
		if (!isPlayer && (bool)GetComponent<UnitObject>() && timeFrozen)
		{
			UnitObject component = GetComponent<UnitObject>();
			component.SpeedMultiplier = 1f;
			SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].timeScale = 1f;
			}
			SimpleSpineFlash[] componentsInChildren2 = component.GetComponentsInChildren<SimpleSpineFlash>(true);
			foreach (SimpleSpineFlash simpleSpineFlash in componentsInChildren2)
			{
				if (simpleSpineFlash.gameObject.activeInHierarchy)
				{
					simpleSpineFlash.Tint(Color.white);
				}
			}
			if ((bool)component.rb)
			{
				component.rb.drag /= 100f;
			}
			timeFrozen = false;
		}
		StasisEvent onStasisCleared = this.OnStasisCleared;
		if (onStasisCleared != null)
		{
			onStasisCleared();
		}
	}

	public void HandleFrozenTime()
	{
		if (PlayerRelic.TimeFrozen)
		{
			if (!timeFrozen)
			{
				AddFreezeTime();
			}
		}
		else if (timeFrozen)
		{
			ClearFreezeTime();
		}
	}

	public void AddElectrified(GameObject attacker)
	{
		if (IsAilemented || IsImmuneToAllStasis)
		{
			return;
		}
		if (ElectrifiedImmune || !CanBeElectrified)
		{
			ClearElectrified();
			return;
		}
		if (electrifiedCounter == -1)
		{
			electrifiedCounter = 0;
			electrifiedTimer = 0f;
			electrifiedAttacker = attacker;
			if (!isPlayer && (bool)GetComponent<UnitObject>())
			{
				UnitObject component = GetComponent<UnitObject>();
				component.SpeedMultiplier = 0.1f;
				SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].timeScale = 0.25f;
				}
				if ((bool)component.rb)
				{
					component.rb.drag *= 1.75f;
				}
			}
			AudioManager.Instance.PlayOneShot("event:/player/Curses/enemy_charmed", base.transform.position);
			StasisEvent onElectrified = this.OnElectrified;
			if (onElectrified != null)
			{
				onElectrified();
			}
		}
		electrifiedCounter++;
		PlayElectrifiedParticles();
		enemyElectrifiedTimestamp = Time.time + enemyElectrifiedDuration;
	}

	public void ClearElectrified()
	{
		if (electrifiedCounter == -1)
		{
			return;
		}
		electrifiedCounter = -1;
		StopElectrifiedParticles();
		if (!isPlayer && (bool)GetComponent<UnitObject>())
		{
			UnitObject component = GetComponent<UnitObject>();
			SimpleSpineFlash[] componentsInChildren = component.GetComponentsInChildren<SimpleSpineFlash>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Tint(Color.white);
			}
			component.SpeedMultiplier = 1f;
			SkeletonAnimation[] componentsInChildren2 = component.GetComponentsInChildren<SkeletonAnimation>(true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].timeScale = 1f;
			}
			if ((bool)component.rb)
			{
				component.rb.drag /= 1.75f;
			}
		}
		if (enemyElectrifiedTicker != null)
		{
			enemyElectrifiedTicker.Hide();
			enemyElectrifiedTicker = null;
		}
		StasisEvent onStasisCleared = this.OnStasisCleared;
		if (onStasisCleared != null)
		{
			onStasisCleared();
		}
	}

	public void PlayElectrifiedParticles()
	{
		if (electrifiedParticles == null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Art/Prefabs/CircleAura_Electrified.prefab", base.transform);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				if (this != null)
				{
					electrifiedParticles = obj.Result.GetComponent<ParticleSystem>();
					CircleCollider2D component = GetComponent<CircleCollider2D>();
					electrifiedParticles.transform.localScale *= ((component != null) ? (1f + component.radius) : 1f);
					electrifiedParticles.Play();
				}
			};
		}
		else
		{
			electrifiedParticles.Play();
		}
	}

	public void StopElectrifiedParticles()
	{
		if (!(electrifiedParticles == null))
		{
			electrifiedParticles.Stop();
		}
	}

	public void ApplyStasisImmunity()
	{
		IsImmuneToAllStasis = true;
		ClearCharm();
		ClearIce();
		ClearElectrified();
	}

	public void ClearStasisImmunity()
	{
		IsImmuneToAllStasis = false;
	}
}
