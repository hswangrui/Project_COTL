using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMRoomGeneration;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerFarming : BaseMonoBehaviour
{
	public delegate void PlayerEvent();

	public delegate void GoToEvent(Vector3 targetPosition);

	[Serializable]
	public class Attack
	{
		public float PreTimer;

		public float PostTimer;

		public float Speed;

		public float AttackDelay;

		public string AnimationName;

		public string AnimationNameUp;

		public string AnimationNameDown;

		public float CamShake = 0.5f;

		public Attack(float PreTimer, float PostTimer, float AttackDelay, float Speed, string AnimationName, float CamShake)
		{
			this.PreTimer = PreTimer;
			this.PostTimer = PostTimer;
			this.Speed = Speed;
			this.AttackDelay = AttackDelay;
			this.AnimationName = AnimationName;
			this.CamShake = CamShake;
		}
	}

	public delegate void GetSoulAction(int DeltaValue);

	public delegate void GetBlackSoulAction(int DeltaValue);

	public static readonly HashSet<StateMachine.State> LongToPerformPlayerStates = new HashSet<StateMachine.State>
	{
		StateMachine.State.Idle,
		StateMachine.State.Moving,
		StateMachine.State.Meditate,
		StateMachine.State.Idle_CarryingBody,
		StateMachine.State.Moving_CarryingBody
	};

	public static Action OnCrownReturn;

	public static Action OnCrownReturnSubtle;

	public static Action OnHideCrown;

	public static PlayerFarming Instance;

	public static FollowerLocation Location = FollowerLocation.None;

	public static FollowerLocation LastLocation = FollowerLocation.None;

	public GrowAndFade growAndFade;

	public GameObject CameraBone;

	private GameObject _CameraBone;

	public GameObject FishingLineBone;

	public Transform CrownBone;

	[SerializeField]
	private MeshRenderer playerGlow;

	private PlayerController _playerController;

	private UnitObject _unitObject;

	private Farm farm;

	private float ArrowAttackDelay;

	private float DodgeDelay;

	private HUD_Inventory InventoryMenu;

	public Inventory inventory;

	public PlayerSimpleInventory simpleInventory;

	private WeaponSelectMenu WeaponSelect;

	public StateMachine _state;

	private float AimAngle;

	public List<CurseChargeBar> CurseChargeBars;

	public List<CurseChargeBar> WeaponChargeBars;

	public List<CurseChargeBar> HeavyChargeBars;

	private Health AimTarget;

	public GameObject WateringCanAim;

	public float WateringCanScale;

	public float WateringCanScaleSpeed;

	public float WateringCanBob;

	public SkeletonAnimation Spine;

	public int CarryingDeadFollowerID = -1;

	public bool NearGrave;

	public bool NearCompostBody;

	public StructureBrain NearStructure;

	private PlayerArrows playerArrows;

	public ParticleSystem HealingParticles;

	public SimpleSpineAnimator simpleSpineAnimator;

	public Chain chain;

	public Transform CarryBone;

	public ColliderEvents PlayerDamageCollider;

	public TrailPicker damageOnRollTrail;

	public AnimationCurve CurseAimingCurve;

	public LineRenderer CurseAimLine;

	public GameObject CurseTarget;

	public Material originalMaterial;

	public Material BW_Material;

	private bool _Healing;

	[HideInInspector]
	public CircleCollider2D circleCollider2D;

	private Skin PlayerSkin;

	private WorkPlace ClosestWorkPlace;

	private WorkPlaceSlot ClosestWorkPlaceSlot;

	private Dwelling ClosestDwelling;

	private DwellingSlot ClosestDwellingSlot;

	public bool GoToAndStopping;

	public bool IdleOnEnd;

	public GameObject LookToObject;

	private Action GoToCallback;

	private float maxDuration = -1f;

	private bool forcePositionOnTimeout;

	private Vector3 forcePositionOnTimeoutTarget;

	private float startMoveTimestamp;

	private float damageOnTouchTimer;

	private bool Meditating;

	public bool HoldingAttack;

	public bool BlockMeditation;

	public bool DodgeQueued;

	private PlayerWeapon _playerWeapon;

	private PlayerSpells _playerSpells;

	private PlayerRelic _playerRelic;

	public bool AllowDodging = true;

	public float HeavyAttackCharge;

	private bool isDashAttacking;

	private Attack CurrentAttack;

	public int CurrentCombo = -1;

	private float ResetCombo;

	public List<Attack> AttackList = new List<Attack>();

	public List<Attack> HeavyAttackList = new List<Attack>();

	public Attack DashAttack;

	public static float LeftoverSouls = 0f;

	public static Action OnGetXP;

	public static Action OnGetDiscipleXP;

	private static readonly int ScorchPos = Shader.PropertyToID("_ScorchPos");

	public PlayerController playerController
	{
		get
		{
			if (_playerController == null)
			{
				_playerController = base.gameObject.GetComponent<PlayerController>();
			}
			return _playerController;
		}
	}

	public UnitObject unitObject
	{
		get
		{
			if (_unitObject == null)
			{
				_unitObject = base.gameObject.GetComponent<UnitObject>();
				_unitObject.UseFixedDirectionalPathing = true;
			}
			return _unitObject;
		}
	}

	public StateMachine state
	{
		get
		{
			if (_state == null)
			{
				_state = base.gameObject.GetComponent<StateMachine>();
			}
			return _state;
		}
	}

	public Health health { get; private set; }

	public static Health Health
	{
		get
		{
			if (Instance == null)
			{
				return null;
			}
			return Instance.health;
		}
	}

	public Vector3 PreviousPosition { get; private set; }

	public bool Healing
	{
		get
		{
			return _Healing;
		}
		set
		{
			if (!_Healing && value)
			{
				HealingParticles.Play();
				StartCoroutine(DoHealing());
			}
			if (_Healing && !value)
			{
				StopCoroutine(DoHealing());
				HealingParticles.Stop();
			}
			_Healing = value;
		}
	}

	public Follower PickedUpFollower { get; private set; }

	public PlayerWeapon playerWeapon
	{
		get
		{
			if (_playerWeapon == null)
			{
				_playerWeapon = GetComponent<PlayerWeapon>();
			}
			return _playerWeapon;
		}
	}

	public PlayerSpells playerSpells
	{
		get
		{
			if (_playerSpells == null)
			{
				_playerSpells = GetComponent<PlayerSpells>();
			}
			return _playerSpells;
		}
	}

	public PlayerRelic playerRelic
	{
		get
		{
			if (_playerRelic == null)
			{
				_playerRelic = GetComponent<PlayerRelic>();
			}
			return _playerRelic;
		}
	}

	public static event PlayerEvent OnDodge;

	public static event GoToEvent OnGoToAndStopBegin;

	public static event GetSoulAction OnGetSoul;

	public static event GetBlackSoulAction OnGetBlackSoul;

	public static event GetBlackSoulAction OnResetBlackSouls;

	private IEnumerator DoHealing()
	{
		float HealTimer = 0f;
		while (DataManager.Instance.PLAYER_HEALTH < DataManager.Instance.PLAYER_TOTAL_HEALTH)
		{
			float num;
			HealTimer = (num = HealTimer + Time.deltaTime);
			if (num > 1f)
			{
				HealTimer = 0f;
				health.HP++;
			}
			yield return null;
		}
		HealingParticles.Stop();
	}

	public void SpineUseDeltaTime(bool Bool)
	{
		Spine.UseDeltaTime = Bool;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		if (playerGlow != null)
		{
			playerGlow.sharedMaterial.DOColor(Color.black, 0f);
		}
		circleCollider2D = GetComponent<CircleCollider2D>();
		playerArrows = GetComponent<PlayerArrows>();
		InitHUD();
		inventory = GetComponent<Inventory>();
		foreach (CurseChargeBar curseChargeBar in CurseChargeBars)
		{
			curseChargeBar.gameObject.SetActive(false);
		}
		foreach (CurseChargeBar weaponChargeBar in WeaponChargeBars)
		{
			weaponChargeBar.gameObject.SetActive(false);
		}
		foreach (CurseChargeBar heavyChargeBar in HeavyChargeBars)
		{
			heavyChargeBar.gameObject.SetActive(false);
		}
		simpleInventory = GetComponent<PlayerSimpleInventory>();
		simpleSpineAnimator = GetComponentInChildren<SimpleSpineAnimator>();
		PlayerDamageCollider.SetActive(false);
		PlayerDamageCollider.OnTriggerEnterEvent += OnTriggerEnterEvent;
		HealingParticles.Stop();
		DataManager.OnChangeTool += ChangeTool;
		Inventory.CURRENT_WEAPON = Inventory.CURRENT_WEAPON;
		SetSkin();
		if (GameManager.IsDungeon(Location))
		{
			Vector3 localScale = Vector3.one * Mathf.Lerp(0.66f, 1.4f, (float)DataManager.Instance.PlayerScaleModifier / 2f);
			base.transform.localScale = localScale;
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(NewPhase));
			NewPhase();
		}
	}

	private void NewPhase()
	{
		if (!(playerGlow == null))
		{
			if (TimeManager.IsNight)
			{
				playerGlow.sharedMaterial.DOColor(new Color(0.25f, 0.25f, 0.25f), 1f);
			}
			else
			{
				playerGlow.sharedMaterial.DOColor(Color.black, 1f);
			}
		}
	}

	private void SetSkinOnHit(HealthPlayer target)
	{
		SetSkin();
	}

	public Skin SetSkin()
	{
		return SetSkin(!FaithAmmo.CanAfford(PlayerSpells.AmmoCost));
	}

	public Skin SetSkin(bool BlackAndWhite)
	{
		PlayerSkin = new Skin("Player Skin");
		Skin skin = Spine.Skeleton.Data.FindSkin("Lamb_" + DataManager.Instance.PlayerFleece + (BlackAndWhite ? "_BW" : ""));
		PlayerSkin.AddSkin(skin);
		string text = WeaponData.Skins.Normal.ToString().ToString();
		if (DataManager.Instance.CurrentWeapon != EquipmentType.None)
		{
			text = EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).Skin.ToString();
		}
		Skin skin2 = Spine.Skeleton.Data.FindSkin("Weapons/" + text);
		PlayerSkin.AddSkin(skin2);
		if (health.HP + health.BlackHearts + health.BlueHearts + health.SpiritHearts <= 1f && DataManager.Instance.PLAYER_TOTAL_HEALTH != 2f)
		{
			Skin skin3 = Spine.Skeleton.Data.FindSkin("Hurt2");
			PlayerSkin.AddSkin(skin3);
		}
		else if ((health.HP + health.BlackHearts + health.BlueHearts + health.SpiritHearts <= 2f && DataManager.Instance.PLAYER_TOTAL_HEALTH != 2f) || (health.HP + health.BlackHearts + health.BlueHearts + health.SpiritHearts <= 1f && DataManager.Instance.PLAYER_TOTAL_HEALTH == 2f))
		{
			Skin skin4 = Spine.Skeleton.Data.FindSkin("Hurt1");
			PlayerSkin.AddSkin(skin4);
		}
		Spine.Skeleton.SetSkin(PlayerSkin);
		Spine.Skeleton.SetSlotsToSetupPose();
		return PlayerSkin;
	}

	private void InitHUD()
	{
		InventoryMenu = UnityEngine.Object.FindObjectOfType<HUD_Inventory>();
		if (InventoryMenu != null)
		{
			InventoryMenu.gameObject.SetActive(false);
		}
		WeaponSelect = UnityEngine.Object.FindObjectOfType<WeaponSelectMenu>();
		if (WeaponSelect != null)
		{
			WeaponSelect.gameObject.SetActive(false);
		}
	}

	public void DropDeadFollower()
	{
		if ((Instance.state.CURRENT_STATE == StateMachine.State.Idle_CarryingBody || Instance.state.CURRENT_STATE == StateMachine.State.Moving_CarryingBody) && Interaction_HarvestMeat.CurrentMovingBody != null)
		{
			Interaction_HarvestMeat.CurrentMovingBody.DropBody();
			if (!BaseGoopDoor.Instance.IsOpen)
			{
				BaseGoopDoor.Instance.DoorDown();
			}
		}
	}

	public void PickUpFollower(Follower f)
	{
		PickedUpFollower = f;
		chain.SetConnection(f.ChainConnection);
		PickedUpFollower.PickUp();
	}

	public void DropFollower()
	{
		PickedUpFollower.Drop();
		DetachFollower();
	}

	private void DetachFollower()
	{
		PickedUpFollower = null;
		ResetCarryStructureColours();
		chain.Disconnect();
	}

	private void ResetCarryStructureColours()
	{
		if (ClosestWorkPlaceSlot != null)
		{
			ClosestWorkPlaceSlot.transform.localScale = Vector3.one;
		}
		if (ClosestDwellingSlot != null)
		{
			ClosestDwellingSlot.transform.localScale = Vector3.one;
		}
	}

	public void CustomAnimation(string Animation, bool Loop)
	{
		StartCoroutine(CustomAnimationRoutine(Animation, Loop));
	}

	private IEnumerator CustomAnimationRoutine(string Animation, bool Loop)
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		simpleSpineAnimator.Animate(Animation, 0, Loop);
	}

	private IEnumerator BleatRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower_noBookPage", base.gameObject);
		simpleSpineAnimator.Animate("bleat", 0, false);
		yield return new WaitForSeconds(0.4f);
		if (state.CURRENT_STATE == StateMachine.State.CustomAnimation)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
	}

	private IEnumerator BlessFollower(Follower follower)
	{
		FollowerTaskType currentTaskType = follower.Brain.CurrentTaskType;
		follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
		follower.Brain.Stats.ReceivedBlessing = true;
		follower.FacePosition(Instance.transform.position);
		List<FollowerTask> allUnoccupiedTasksOfType = FollowerBrain.GetAllUnoccupiedTasksOfType(currentTaskType);
		FollowerTask task = null;
		if (allUnoccupiedTasksOfType.Count > 0)
		{
			task = allUnoccupiedTasksOfType[UnityEngine.Random.Range(0, allUnoccupiedTasksOfType.Count)];
		}
		if (task != null)
		{
			task.ClaimReservations();
		}
		follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		follower.SetBodyAnimation("devotion/devotion-start", false);
		follower.AddBodyAnimation("devotion/devotion-waiting", true, 0f);
		yield return new WaitForSeconds(0.5f);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BlessAFollower);
		follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Bless, delegate
		{
			CultFaithManager.AddThought(Thought.Cult_Bless, -1, 1f);
			follower.SetBodyAnimation("idle", true);
			if (task != null)
			{
				follower.Brain.HardSwapToTask(task);
			}
			else
			{
				follower.Brain.CompleteCurrentTask();
			}
		});
	}

	private IEnumerator IntimidateFollower(Follower follower)
	{
		FollowerTaskType currentTaskType = follower.Brain.CurrentTaskType;
		follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
		follower.Brain.Stats.Intimidated = true;
		follower.FacePosition(Instance.transform.position);
		List<FollowerTask> allUnoccupiedTasksOfType = FollowerBrain.GetAllUnoccupiedTasksOfType(currentTaskType);
		FollowerTask task = null;
		if (allUnoccupiedTasksOfType.Count > 0)
		{
			task = allUnoccupiedTasksOfType[UnityEngine.Random.Range(0, allUnoccupiedTasksOfType.Count)];
		}
		if (task != null)
		{
			task.ClaimReservations();
		}
		follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		follower.SetBodyAnimation("Reactions/react-intimidate", false);
		follower.AddBodyAnimation("idle", true, 0f);
		yield return new WaitForSeconds(0.75f);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BlessAFollower);
		follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Intimidate, delegate
		{
			follower.Brain.AddThought(Thought.Intimidated);
			if (task != null)
			{
				follower.Brain.HardSwapToTask(task);
			}
			else
			{
				follower.Brain.CompleteCurrentTask();
			}
		});
	}

	private IEnumerator InspireFollower(Follower follower)
	{
		FollowerTaskType currentTaskType = follower.Brain.CurrentTaskType;
		follower.Brain.HardSwapToTask(new FollowerTask_ManualControl());
		follower.Brain.Stats.Inspired = true;
		follower.FacePosition(Instance.transform.position);
		List<FollowerTask> allUnoccupiedTasksOfType = FollowerBrain.GetAllUnoccupiedTasksOfType(currentTaskType);
		FollowerTask task = null;
		if (allUnoccupiedTasksOfType.Count > 0)
		{
			task = allUnoccupiedTasksOfType[UnityEngine.Random.Range(0, allUnoccupiedTasksOfType.Count)];
		}
		if (task != null)
		{
			task.ClaimReservations();
		}
		follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		follower.SetBodyAnimation("dance", true);
		follower.AddBodyAnimation("idle", true, 0f);
		yield return new WaitForSeconds(0.5f);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BlessAFollower);
		follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Inspire, delegate
		{
			CultFaithManager.AddThought(Thought.Cult_Inspire, -1, 1f);
			follower.SetBodyAnimation("idle", true);
			if (task != null)
			{
				follower.Brain.HardSwapToTask(task);
			}
			else
			{
				follower.Brain.CompleteCurrentTask();
			}
		});
	}

	public void GoToAndStop(GameObject TargetPosition, GameObject LookToObject = null, bool IdleOnEnd = false, bool DisableCollider = false, Action GoToCallback = null, float maxDuration = 20f, bool forcePositionOnTimeout = false)
	{
		playerController.xDir = 0f;
		playerController.yDir = 0f;
		unitObject.vx = 0f;
		unitObject.vy = 0f;
		unitObject.ClearPaths();
		GoToAndStopping = true;
		this.LookToObject = LookToObject;
		this.IdleOnEnd = IdleOnEnd;
		this.GoToCallback = GoToCallback;
		this.maxDuration = maxDuration;
		this.forcePositionOnTimeout = forcePositionOnTimeout;
		forcePositionOnTimeoutTarget = TargetPosition.transform.position;
		startMoveTimestamp = Time.unscaledTime;
		if (DisableCollider)
		{
			GetComponent<CircleCollider2D>().enabled = false;
		}
		unitObject.givePath(TargetPosition.transform.position);
		UnitObject obj = unitObject;
		obj.EndOfPath = (Action)Delegate.Combine(obj.EndOfPath, new Action(EndGoToAndStop));
		GoToEvent onGoToAndStopBegin = PlayerFarming.OnGoToAndStopBegin;
		if (onGoToAndStopBegin != null)
		{
			onGoToAndStopBegin(TargetPosition.transform.position);
		}
	}

	public void GoToAndStop(Vector3 TargetPosition, GameObject LookToObject = null, bool IdleOnEnd = false, bool DisableCollider = false, Action GoToCallback = null, float maxDuration = 20f, bool forcePositionOnTimeout = false)
	{
		playerController.xDir = 0f;
		playerController.yDir = 0f;
		unitObject.vx = 0f;
		unitObject.vy = 0f;
		unitObject.ClearPaths();
		GoToAndStopping = true;
		this.LookToObject = LookToObject;
		this.IdleOnEnd = IdleOnEnd;
		this.GoToCallback = GoToCallback;
		this.maxDuration = maxDuration;
		this.forcePositionOnTimeout = forcePositionOnTimeout;
		forcePositionOnTimeoutTarget = TargetPosition;
		startMoveTimestamp = Time.unscaledTime;
		if (DisableCollider)
		{
			GetComponent<CircleCollider2D>().enabled = false;
		}
		unitObject.givePath(TargetPosition);
		UnitObject obj = unitObject;
		obj.EndOfPath = (Action)Delegate.Combine(obj.EndOfPath, new Action(EndGoToAndStop));
		GoToEvent onGoToAndStopBegin = PlayerFarming.OnGoToAndStopBegin;
		if (onGoToAndStopBegin != null)
		{
			onGoToAndStopBegin(TargetPosition);
		}
	}

	public void EndGoToAndStop()
	{
		GoToAndStopping = false;
		unitObject.ClearPaths();
		state.CURRENT_STATE = ((!IdleOnEnd) ? StateMachine.State.InActive : StateMachine.State.Idle);
		if (LookToObject != null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, LookToObject.transform.position);
		}
		Action goToCallback = GoToCallback;
		if (goToCallback != null)
		{
			goToCallback();
		}
		GetComponent<CircleCollider2D>().enabled = true;
		UnitObject obj = unitObject;
		obj.EndOfPath = (Action)Delegate.Remove(obj.EndOfPath, new Action(EndGoToAndStop));
	}

	public void EnableDamageOnTouchCollider(float duration)
	{
		damageOnTouchTimer = duration;
		PlayerDamageCollider.SetActive(true);
	}

	private void Update()
	{
		if (Time.timeScale <= 0f)
		{
			return;
		}
		damageOnTouchTimer -= Time.deltaTime;
		if (state.CURRENT_STATE == StateMachine.State.Dodging && TrinketManager.DamageEnemyOnRoll())
		{
			if (!PlayerDamageCollider.isActiveAndEnabled)
			{
				AudioManager.Instance.PlayOneShot("event:/player/damage_roll", base.transform.position);
			}
			PlayerDamageCollider.SetActive(true);
		}
		else
		{
			PlayerDamageCollider.SetActive(damageOnTouchTimer > 0f);
		}
		if (TrinketManager.HasTrinket(TarotCards.Card.BlackSoulAutoRecharge) && !TrinketManager.IsOnCooldown(TarotCards.Card.BlackSoulAutoRecharge) && FaithAmmo.Ammo < FaithAmmo.Total && !HUD_Manager.Instance.Hidden && FaithAmmo.Ammo < FaithAmmo.Total)
		{
			FaithAmmo.Instance.DoFlash = false;
			bool playSound = !PlayerFleeceManager.FleeceSwapsWeaponForCurse();
			GetBlackSoul(1, false, playSound);
			FaithAmmo.Instance.DoFlash = true;
			TrinketManager.TriggerCooldown(TarotCards.Card.BlackSoulAutoRecharge);
		}
		Spine.timeScale = ((state.CURRENT_STATE == StateMachine.State.Attacking) ? Spine.timeScale : 1f);
		if (Location == FollowerLocation.Base)
		{
			if (Meditating && (state.CURRENT_STATE != StateMachine.State.Meditate || Instance.GoToAndStopping))
			{
				Meditating = false;
				if (Time.timeScale == 3f)
				{
					Time.timeScale = 1f;
				}
				state.CURRENT_STATE = StateMachine.State.Idle;
				GameManager.GetInstance().CameraResetTargetZoom();
				Spine.UseDeltaTime = true;
				MonoSingleton<Indicator>.Instance.Activate();
			}
			if (state.CURRENT_STATE == StateMachine.State.Meditate)
			{
				if (!InputManager.Gameplay.GetMeditateButtonHeld() || Instance.GoToAndStopping)
				{
					Meditating = false;
					Time.timeScale = 1f;
					state.CURRENT_STATE = StateMachine.State.Idle;
					GameManager.GetInstance().CameraResetTargetZoom();
					Spine.UseDeltaTime = true;
					MonoSingleton<Indicator>.Instance.Activate();
				}
				return;
			}
			if (state != null && Location == FollowerLocation.Base && !BlockMeditation && !Instance.GoToAndStopping && InputManager.Gameplay.GetMeditateButtonDown() && state.CURRENT_STATE != StateMachine.State.Meditate && state.CURRENT_STATE != StateMachine.State.Idle_CarryingBody && state.CURRENT_STATE != StateMachine.State.Moving_CarryingBody && state.CURRENT_STATE != StateMachine.State.CustomAction0 && !LetterBox.IsPlaying)
			{
				Meditating = true;
				Time.timeScale = 3f;
				state.CURRENT_STATE = StateMachine.State.Meditate;
				GameManager.GetInstance().CameraSetTargetZoom(15f);
				Spine.UseDeltaTime = false;
			}
		}
		else if (state.CURRENT_STATE == StateMachine.State.Meditate && !GameManager.IsDungeon(Location))
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		if (GoToAndStopping)
		{
			if (PickedUpFollower != null && state.CURRENT_STATE != StateMachine.State.TimedAction)
			{
				UpdatePickedUpFollower();
			}
			if (state.CURRENT_STATE != StateMachine.State.TimedAction)
			{
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			float unscaledTime = Time.unscaledTime;
			if (maxDuration != -1f && unscaledTime > startMoveTimestamp + maxDuration)
			{
				EndGoToAndStop();
				if (forcePositionOnTimeout)
				{
					Debug.LogWarning("Player GotoAndStop Timed out, placing player on end point");
					base.transform.position = forcePositionOnTimeoutTarget;
				}
			}
		}
		else
		{
			if (state.CURRENT_STATE == StateMachine.State.Map || state.CURRENT_STATE == StateMachine.State.Heal || state.CURRENT_STATE == StateMachine.State.CustomAnimation || state.CURRENT_STATE == StateMachine.State.Grabbed || state.CURRENT_STATE == StateMachine.State.DashAcrossIsland || state.CURRENT_STATE == StateMachine.State.Grapple || state.CURRENT_STATE == StateMachine.State.SpawnIn || state.CURRENT_STATE == StateMachine.State.Respawning || state.CURRENT_STATE == StateMachine.State.Dieing || state.CURRENT_STATE == StateMachine.State.Dead || state.CURRENT_STATE == StateMachine.State.Converting || state.CURRENT_STATE == StateMachine.State.InActive || state.CURRENT_STATE == StateMachine.State.Building || state.CURRENT_STATE == StateMachine.State.Unconverted || state.CURRENT_STATE == StateMachine.State.FoundItem || state.CURRENT_STATE == StateMachine.State.GameOver || state.CURRENT_STATE == StateMachine.State.FinalGameOver)
			{
				return;
			}
			if (state.CURRENT_STATE != StateMachine.State.Dodging)
			{
				DodgeDelay -= Time.deltaTime;
			}
			if (state.CURRENT_STATE == StateMachine.State.HitThrown || state.CURRENT_STATE == StateMachine.State.Casting || state.CURRENT_STATE == StateMachine.State.HitRecover)
			{
				if (state.CURRENT_STATE == StateMachine.State.HitRecover || state.CURRENT_STATE == StateMachine.State.Casting || (state.CURRENT_STATE == StateMachine.State.HitThrown && playerController.HitTimer > 0.35f))
				{
					DodgeRoll();
				}
				return;
			}
			ArrowAttackDelay -= Time.deltaTime;
			if (state.CURRENT_STATE == StateMachine.State.InActive || state.CURRENT_STATE == StateMachine.State.TimedAction || state.CURRENT_STATE == StateMachine.State.SignPostAttack || state.CURRENT_STATE == StateMachine.State.RecoverFromAttack)
			{
				return;
			}
			if (PickedUpFollower != null)
			{
				if (state.CURRENT_STATE != StateMachine.State.TimedAction)
				{
					UpdatePickedUpFollower();
				}
				return;
			}
			if (simpleInventory.Item != 0)
			{
				if (InputManager.Gameplay.GetAttackButtonHeld())
				{
					simpleInventory.DropItem();
				}
				return;
			}
			switch (Inventory.CURRENT_WEAPON)
			{
			case 3:
				WaterPlants();
				WateringCanAim.transform.localScale = new Vector3(WateringCanScale, WateringCanScale);
				break;
			}
			DodgeRoll();
			if (state.CURRENT_STATE == StateMachine.State.Inventory && !InventoryMenu.gameObject.activeSelf)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			PreviousPosition = base.transform.position;
		}
	}

	public void ShowProjectileChargeBars()
	{
		int num = 1;
		bool requiresCharging = false;
		if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PrimaryEquipmentType == EquipmentType.Fireball || EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PrimaryEquipmentType == EquipmentType.MegaSlash)
		{
			requiresCharging = true;
		}
		if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).EquipmentType == EquipmentType.Fireball_Swarm)
		{
			requiresCharging = false;
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.Tentacles_Circular)
		{
			num = 4;
		}
		for (int i = 0; i < num; i++)
		{
			CurseChargeBars[i].ShowProjectileCharge(requiresCharging);
		}
	}

	public void HideProjectileChargeBars()
	{
		foreach (CurseChargeBar curseChargeBar in CurseChargeBars)
		{
			curseChargeBar.HideProjectileCharge();
		}
	}

	public void ShowHeavyAttackProjectileChargeBars(float size = 1.5f)
	{
		for (int i = 0; i < HeavyChargeBars.Count; i++)
		{
			HeavyChargeBars[i].ShowProjectileCharge(false, size);
		}
	}

	public void UpdateHeavyChargeBar(float fillAmount)
	{
		foreach (CurseChargeBar heavyChargeBar in HeavyChargeBars)
		{
			heavyChargeBar.UpdateProjectileChargeBar(fillAmount);
		}
	}

	public void HideHeavyChargeBars()
	{
		foreach (CurseChargeBar heavyChargeBar in HeavyChargeBars)
		{
			heavyChargeBar.HideProjectileCharge();
		}
	}

	public void SetAimingRecticuleScaleAndRotation(int chargeBarIndex, Vector3 scale, Vector3 euler)
	{
		CurseChargeBars[chargeBarIndex].SetAimingRecticuleScaleAndRotation(scale, euler);
	}

	public void SetHeavyAimingRecticuleScaleAndRotation(int chargeBarIndex, Vector3 scale, Vector3 euler)
	{
		HeavyChargeBars[chargeBarIndex].SetAimingRecticuleScaleAndRotation(scale, euler);
	}

	public void ShowWeaponChargeBars(float size = 1.5f)
	{
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			WeaponChargeBars[i].ShowProjectileCharge(false, size);
		}
	}

	public void HideWeaponChargeBars()
	{
		foreach (CurseChargeBar weaponChargeBar in WeaponChargeBars)
		{
			weaponChargeBar.HideProjectileCharge();
		}
	}

	public void SetWeaponAimingRecticuleScaleAndRotation(int chargeBarIndex, Vector3 scale, Vector3 euler)
	{
		WeaponChargeBars[chargeBarIndex].SetAimingRecticuleScaleAndRotation(scale, euler);
	}

	public void UpdateProjectileChargeBar(float fillAmount)
	{
		foreach (CurseChargeBar curseChargeBar in CurseChargeBars)
		{
			curseChargeBar.UpdateProjectileChargeBar(fillAmount);
		}
	}

	public bool CorrectProjectileChargeRelease()
	{
		return CurseChargeBars[0].CorrectProjectileChargeRelease();
	}

	private void UpdatePickedUpFollower()
	{
		Vector3 position = CarryBone.position;
		position.y = base.transform.position.y;
		position += new Vector3(1f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), 1f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), 0f);
		PickedUpFollower.transform.position = Vector3.Lerp(PickedUpFollower.transform.position, position, 15f * Time.deltaTime);
		PickedUpFollower.State.facingAngle = Utils.GetAngle(base.transform.position, PickedUpFollower.transform.position);
	}

	private void ChangeTool(int PrevTool, int NewTool)
	{
		if (PrevTool == 3)
		{
			Spine.AnimationState.ClearTrack(1);
			WateringCanScale = 0f;
			WateringCanAim.transform.position = base.transform.position;
			WateringCanAim.SetActive(false);
		}
		switch (NewTool)
		{
		case 1:
			Spine.skeleton.SetAttachment("TOOLS", "SPADE");
			break;
		case 2:
			Spine.skeleton.SetAttachment("TOOLS", "SEED_BAG");
			break;
		case 3:
			Spine.skeleton.SetAttachment("TOOLS", "WATERING_CAN");
			WateringCanAim.SetActive(true);
			break;
		case 0:
			break;
		}
	}

	public void Cook()
	{
		TimedAction(2f, CookFood);
	}

	private void CookFood()
	{
		simpleInventory.GiveItem(InventoryItem.ITEM_TYPE.MEAT);
	}

	private void EatFood()
	{
		TimedAction(1f, null);
		simpleInventory.RemoveItem();
	}

	private void WaterPlants()
	{
		Vector3 vector = new Vector3(0.8f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), 0.8f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)));
		if (InputManager.Gameplay.GetAttackButtonHeld())
		{
			Spine.AnimationState.SetAnimation(1, "Farming-water_track", true);
			WateringCanAim.transform.position = Vector3.Lerp(WateringCanAim.transform.position, base.transform.position + vector, 3.5f * Time.deltaTime);
			WateringCanScaleSpeed += (1f - WateringCanScale) * 0.2f;
			WateringCanScale += (WateringCanScaleSpeed *= 0.7f) + 0.01f * Mathf.Cos(WateringCanBob += 0.2f);
			foreach (Crop crop in Crop.Crops)
			{
				if (Vector3.Distance(WateringCanAim.transform.position, crop.transform.position) < 0.75f)
				{
					crop.DoWork(1f * Time.deltaTime);
				}
			}
			foreach (Fire fire in Fire.Fires)
			{
				if (Vector3.Distance(WateringCanAim.transform.position, fire.transform.position) < 1.25f)
				{
					fire.DoWork(1f * Time.deltaTime);
				}
			}
		}
		else
		{
			WateringCanAim.transform.position = Vector3.Lerp(WateringCanAim.transform.position, base.transform.position, 3f * Time.deltaTime);
			if (WateringCanScale > 0f)
			{
				WateringCanScale -= 0.1f * GameManager.DeltaTime;
				if (WateringCanScale <= 0f)
				{
					WateringCanScale = 0f;
				}
			}
			Spine.AnimationState.ClearTrack(1);
		}
		WateringCanAim.transform.localScale = new Vector3(WateringCanScale, WateringCanScale);
	}

	public void TimedAction(float Duration, Action Callback, string SpineAnimation = "action")
	{
		playerController.TimedActionCallback = Callback;
		state.CURRENT_STATE = StateMachine.State.TimedAction;
		state.Timer = Duration;
		Spine.AnimationState.SetAnimation(0, SpineAnimation, true);
	}

	public bool DodgeRoll()
	{
		if (!AllowDodging || state.CURRENT_STATE == StateMachine.State.Idle_CarryingBody || state.CURRENT_STATE == StateMachine.State.Moving_CarryingBody || state.CURRENT_STATE == StateMachine.State.WeaponSelect || state.CURRENT_STATE == StateMachine.State.Meditate || state.CURRENT_STATE == StateMachine.State.Dead)
		{
			return false;
		}
		StateMachine.State cURRENT_STATE = state.CURRENT_STATE;
		if ((uint)cURRENT_STATE <= 1u && InputManager.Gameplay.GetBleatButtonDown() && !GameManager.IsDungeon(Location))
		{
			if (!GameManager.IsDungeon(Location))
			{
				StartCoroutine(BleatRoutine());
			}
			return false;
		}
		if ((!(playerWeapon != null) || playerWeapon.CurrentWeapon == null || !(playerWeapon.CurrentWeapon.WeaponData != null) || !playerWeapon.CurrentWeapon.WeaponData.CanBreakDodge || playerWeapon.CurrentAttackState != 0) && ((state.CURRENT_STATE == StateMachine.State.Attacking && playerWeapon != null && playerWeapon.CurrentAttackState == PlayerWeapon.AttackState.Begin) || state.CURRENT_STATE == StateMachine.State.Casting))
		{
			if (DodgeDelay <= 0f && InputManager.Gameplay.GetDodgeButtonDown())
			{
				DodgeQueued = true;
			}
			return false;
		}
		if (state.CURRENT_STATE != StateMachine.State.Dodging && state.CURRENT_STATE != StateMachine.State.CustomAction0 && (DodgeQueued || (DodgeDelay <= 0f && InputManager.Gameplay.GetDodgeButtonDown())) && (!PlayerFleeceManager.FleecePreventsRoll() || !GameManager.IsDungeon(Location)))
		{
			DodgeQueued = false;
			playerController.StopHitEffects();
			HideProjectileChargeBars();
			float facingAngle = state.facingAngle;
			simpleSpineAnimator.FlashWhite(false);
			playerController.forceDir = ((playerController.xDir != 0f || playerController.yDir != 0f) ? Utils.GetAngle(Vector3.zero, new Vector3(playerController.xDir, playerController.yDir)) : state.facingAngle);
			playerController.speed = playerController.DodgeSpeed * 1.2f * DataManager.Instance.DodgeDistanceMultiplier;
			state.CURRENT_STATE = StateMachine.State.Dodging;
			playerWeapon.StopAttackRoutine();
			AudioManager.Instance.PlayOneShot("event:/player/dash_roll", base.gameObject);
			MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact, false, true, GameManager.GetInstance());
			DodgeDelay = playerController.DodgeDelay;
			health.ForgiveRecentDamage();
			if (health.IsPoisoned && PlayerFleeceManager.FleeceCausesPoisonOnHit())
			{
				TrapPoison.CreatePoison(base.transform.position, 1, 0f, GenerateRoom.Instance.transform);
				health.ClearPoison();
			}
			if (TrinketManager.DropBombOnRoll() && !TrinketManager.IsOnCooldown(TarotCards.Card.BombOnRoll))
			{
				Bomb.CreateBomb(base.transform.position, health, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent);
				TrinketManager.TriggerCooldown(TarotCards.Card.BombOnRoll);
			}
			if (TrinketManager.DropBlackGoopOnRoll() && !TrinketManager.IsOnCooldown(TarotCards.Card.GoopOnRoll))
			{
				TrapGoop.CreateGoop(base.transform.position, 5, 0.5f, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent);
				TrinketManager.TriggerCooldown(TarotCards.Card.GoopOnRoll);
			}
			playerWeapon.DoingHeavyAttack = false;
			PlayerEvent onDodge = PlayerFarming.OnDodge;
			if (onDodge != null)
			{
				onDodge();
			}
			damageOnRollTrail.ClearTrails();
			return true;
		}
		return false;
	}

	private void Sword()
	{
		if (!(ArrowAttackDelay <= 0f) || !InputManager.Gameplay.GetAttackButtonDown())
		{
			return;
		}
		isDashAttacking = state.CURRENT_STATE == StateMachine.State.Dodging;
		if (isDashAttacking)
		{
			CurrentCombo = 1;
			CurrentAttack = DashAttack;
		}
		else
		{
			if (++CurrentCombo > AttackList.Count - 1)
			{
				CurrentCombo = 0;
			}
			CurrentAttack = AttackList[CurrentCombo];
		}
		state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		state.Timer = CurrentAttack.PreTimer;
		playerController.TimedActionCallback = DoAttack;
		HoldingAttack = true;
	}

	public void DoHeavyAttack()
	{
	}

	private void StartHeavyAttack()
	{
	}

	private void DoAttack()
	{
		state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
		Spine.AnimationState.SetAnimation(0, CurrentAttack.AnimationName, false);
		state.Timer = CurrentAttack.PostTimer;
		ArrowAttackDelay = CurrentAttack.AttackDelay;
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(string.Format("Assets/Prefabs/Enemies/Weapons/PlayerSwipe{0}.prefab", CurrentCombo), base.transform.position + new Vector3(0.5f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f), Quaternion.identity, base.transform.parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			Swipe component = obj.Result.GetComponent<Swipe>();
			Vector3 position = base.transform.position + new Vector3(0.5f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f);
			component.Init(position, state.facingAngle, health.team, health, null, 1f);
			ResetCombo = 0.3f;
			CameraManager.shakeCamera(CurrentAttack.CamShake, state.facingAngle);
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
		};
		if (isDashAttacking)
		{
			playerController.speed = CurrentAttack.Speed;
		}
	}

	private void OnTriggerEnterEvent(Collider2D collider)
	{
		Health componentInParent = collider.GetComponentInParent<Health>();
		if ((bool)componentInParent && componentInParent.team == Health.Team.Team2)
		{
			componentInParent.DealDamage(PlayerWeapon.GetDamage(1f, DataManager.Instance.CurrentWeaponLevel / 2), base.gameObject, base.transform.position);
		}
	}

	public void GetSoul()
	{
		GetSoul(1);
	}

	public void GetSoul(int delta = 1)
	{
		GetSoulAction onGetSoul = PlayerFarming.OnGetSoul;
		if (onGetSoul != null)
		{
			onGetSoul(delta);
		}
		if (delta > 0 && (UpgradeSystem.UpgradeType == UpgradeSystem.UpgradeTypes.Devotion || UpgradeSystem.UpgradeType == UpgradeSystem.UpgradeTypes.Both))
		{
			GetXP(1f);
		}
	}

	public void GetBlackSoul(int delta = 1, bool giveXp = true, bool playSound = true)
	{
		if (delta != 0)
		{
			LeftoverSouls += Demon.GetDemonLeftovers();
			if (LeftoverSouls >= 1f)
			{
				LeftoverSouls -= 1f;
				delta++;
			}
			if (playSound)
			{
				AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", base.gameObject);
			}
			Inventory.BlackSouls += delta;
			DataManager.Instance.dungeonRunXPOrbs++;
			GetBlackSoulAction onGetBlackSoul = PlayerFarming.OnGetBlackSoul;
			if (onGetBlackSoul != null)
			{
				onGetBlackSoul(delta);
			}
			if (delta > 0 && giveXp && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).ContainsAttachmentType(AttachmentEffect.Fervour))
			{
				GetXP(1f * DungeonModifier.HasPositiveModifier(DungeonPositiveModifier.DoubleXP, 2f, 1f) * TrinketManager.GetBlackSoulsMultiplier());
			}
		}
	}

	public void GetXP(float Delta)
	{
		DataManager.Instance.XP += Mathf.RoundToInt(Delta);
		if (DataManager.Instance.XP >= DataManager.GetTargetXP(Mathf.Min(DataManager.Instance.Level, DataManager.TargetXP.Count - 1)))
		{
			if (DataManager.Instance.Level == 0)
			{
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.UnlockSacredKnowledge);
			}
			DataManager.Instance.XP = 0;
			DataManager.Instance.Level++;
			UpgradeSystem.AbilityPoints++;
		}
		Action onGetXP = OnGetXP;
		if (onGetXP != null)
		{
			onGetXP();
		}
	}

	public void GetDisciple(float Delta)
	{
		StartCoroutine(AddDisciple(Delta));
	}

	private IEnumerator AddDisciple(float Delta)
	{
		yield return null;
		DataManager.Instance.DiscipleXP += Delta;
		Action onGetDiscipleXP = OnGetDiscipleXP;
		if (onGetDiscipleXP != null)
		{
			onGetDiscipleXP();
		}
		if (DataManager.Instance.DiscipleXP >= DataManager.TargetDiscipleXP[Mathf.Min(DataManager.Instance.DiscipleLevel, DataManager.TargetDiscipleXP.Count - 1)])
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GetFollowerUpgradePoint);
			DataManager.Instance.DiscipleXP = 0f;
			DataManager.Instance.DiscipleLevel++;
			UpgradeSystem.DisciplePoints++;
			NotificationCentreScreen.Play(NotificationCentre.NotificationType.UpgradeRitualReady);
		}
	}

	public void SetActiveDustEffect(bool isActive)
	{
		unitObject.emitDustClouds = isActive;
	}

	private void OnDisable()
	{
		HealthPlayer.OnHPUpdated -= SetSkinOnHit;
		HealthPlayer.OnTotalHPUpdated -= SetSkinOnHit;
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnEnable()
	{
		Instance = this;
		HealthPlayer.OnHPUpdated += SetSkinOnHit;
		HealthPlayer.OnTotalHPUpdated += SetSkinOnHit;
		health = base.gameObject.GetComponent<Health>();
	}

	private void OnDestroy()
	{
		DataManager.OnChangeTool -= ChangeTool;
		LeftoverSouls = 0f;
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(NewPhase));
	}
}
