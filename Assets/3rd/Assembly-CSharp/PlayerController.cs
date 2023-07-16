using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using MMBiomeGeneration;
using MMTools;
using Spine.Unity.Examples;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class PlayerController : BaseMonoBehaviour
{
	private struct DroppedItem
	{
		public int Type;

		public int Quantity;
	}

	public AudioClip GameOverMusic;

	private bool GameOver;

	[HideInInspector]
	public UnitObject unitObject;

	private StateMachine state;

	public float forceDir;

	public float speed;

	public float xDir;

	public float yDir;

	private Vector3 SpinePosition;

	private PlayerFarming playerFarming;

	private float DodgeTimer;

	public float DodgeSpeed = 12f;

	public float DodgeDuration = 0.3f;

	public float DodgeMaxDuration = 0.5f;

	public float DodgeDelay = 0.3f;

	private float DodgeCollisionDelay;

	public float HitDuration = 1f;

	public float HitTimer;

	public float HitRecoverTimer;

	private float ConversionTimer;

	private float DeathTimer;

	private float RespawnTimer;

	private float LungeDuration;

	private float LungeTimer;

	private float LungeSpeed;

	public LineRenderer GrappleChain;

	public Transform BoneTool;

	public GameObject playerDieColorVolume;

	private UIDeathScreenOverlayController _deathScreenInstance;

	public SimpleSpineFlash SimpleSpineFlash;

	[SerializeField]
	private DamageCollider enlargedCollider;

	[SerializeField]
	private SkeletonGhost _skeletonGhost;

	public bool CarryingBody;

	private bool _ShowRunSmoke = true;

	public Action TimedActionCallback;

	public SpineEventListener FootStepSoundsObject;

	private Inventory inventory;

	private PlayerWeapon playerWeapon;

	private PlayerSpells playerSpells;

	public float RunSpeed = 5.5f;

	[HideInInspector]
	public float DefaultRunSpeed = 5.5f;

	public static float MinInputForMovement = 0.3f;

	public static bool CanParryAttacks = false;

	public static bool CanRespawn = true;

	private bool showGhostEffect;

	private string carryUp = "corpse/corpse-run-up";

	private string carryDown = "corpse/corpse-run-down";

	private string carryDownDiagonal = "corpse/corpse-run";

	private string carryUpDiagonal = "corpse/corpse-run-up-diagonal";

	private string carryHorizontal = "corpse/corpse-run-horizontal";

	[SerializeField]
	private UIDeathScreenOverlayController _deathScreenTemplate;

	private Health health;

	private float untouchableTimer;

	private float invincibleTimer;

	private int untouchableTimerFlash;

	private float KnockBackAngle;

	public Coroutine HitEffectsCoroutine;

	private float KnockbackVelocity = 0.2f;

	public float KnockbackGravity = 0.03f;

	public float KnockbackBounce = -0.8f;

	private float VZ;

	private float Z;

	public Transform SpineTransform;

	public SimpleSFX sfx;

	private float FootPrints;

	private Color FootStepColor;

	private float FootPrintsNum = 10f;

	private float FootPrintModifier = 5f;

	private List<MeshRenderer> EnemiesTurnedOff = new List<MeshRenderer>();

	private List<SpriteRenderer> SpritesTurnedOff = new List<SpriteRenderer>();

	private List<DroppedItem> droppedItems = new List<DroppedItem>();

	public Interaction_Grapple TargetGrapple;

	private float GrappleProgress;

	private float ElevatorProgress;

	private float ElevatorProgressSpeed;

	private CircleCollider2D circleCollider2D;

	private Interaction_Elevator TargetElevator;

	private float CurretElevatorZ;

	private float TargetElevatorZ;

	private Vector3 ElevatorPosition;

	private bool ElevatorChangedFloor;

	private Vector3 TargetPosition;

	public bool SpawnInShowHUD = true;

	private EventInstance _loopSound;

	private bool playedLoop;

	public float LungeDampener = 0.5f;

	private float SeperationRadius;

	private float SeperationDistance;

	private float SeperationAngle;

	private Camera currentMain;

	private float previousClipPlane;

	private static readonly int GlobalDitherIntensity = Shader.PropertyToID("_GlobalDitherIntensity");

	public bool ShowRunSmoke
	{
		get
		{
			return _ShowRunSmoke;
		}
		set
		{
			_ShowRunSmoke = value;
			FootStepSoundsObject.enabled = _ShowRunSmoke;
		}
	}

	private void Start()
	{
		unitObject = base.gameObject.GetComponent<UnitObject>();
		state = base.gameObject.GetComponent<StateMachine>();
		inventory = base.gameObject.GetComponent<Inventory>();
		playerFarming = base.gameObject.GetComponent<PlayerFarming>();
		circleCollider2D = GetComponent<CircleCollider2D>();
		GrappleChain.gameObject.SetActive(false);
		DefaultRunSpeed = RunSpeed;
		playerWeapon = base.gameObject.GetComponent<PlayerWeapon>();
		_skeletonGhost.ghostingEnabled = false;
		enlargedCollider = GetComponentInChildren<DamageCollider>(true);
		DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance, 1f).SetEase(Ease.OutQuart);
	}

	private void OnEnable()
	{
		health = base.gameObject.GetComponent<Health>();
		if (health != null)
		{
			health.OnHit += OnHit;
			health.OnDie += OnDie;
		}
	}

	private void OnDestroy()
	{
		if (health != null)
		{
			health.OnHit -= OnHit;
			health.OnDie -= OnDie;
		}
		EnemiesTurnedOff.Clear();
		SpritesTurnedOff.Clear();
	}

	private void SetDither(float value)
	{
		Shader.SetGlobalFloat(GlobalDitherIntensity, value);
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (health.untouchable)
		{
			return;
		}
		playerWeapon.DoingHeavyAttack = false;
		playerFarming.simpleSpineAnimator.FlashRedTint();
		if (Attacker == null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, AttackLocation);
		}
		else
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, Attacker.transform.position);
		}
		forceDir = state.facingAngle + 180f;
		CameraManager.shakeCamera(0.5f, 0f - state.facingAngle);
		if (state.CURRENT_STATE != StateMachine.State.Grabbed)
		{
			if (HitEffectsCoroutine != null)
			{
				StopCoroutine(HitEffectsCoroutine);
			}
			HitEffectsCoroutine = StartCoroutine(HitEffects(StateMachine.State.HitThrown, StateMachine.State.HitRecover));
			state.CURRENT_STATE = StateMachine.State.HitThrown;
			MakeUntouchable(1f * DifficultyManager.GetInvincibleTimeMultiplier(), false);
		}
		BiomeConstants.Instance.EmitHitVFX(AttackLocation, Quaternion.identity.z, "HitFX_Blocked");
		AudioManager.Instance.ToggleFilter(SoundParams.HitFilter, true);
		AudioManager.Instance.ToggleFilter(SoundParams.HitFilter, false, 0.2f);
		GameManager.GetInstance().HitStop();
		AudioManager.Instance.PlayOneShot("event:/player/gethit", base.transform.position);
	}

	public void MakeUntouchable(float duration, bool ghostEffect = true)
	{
		if (duration <= 0f)
		{
			return;
		}
		if (untouchableTimer < duration)
		{
			untouchableTimer = duration;
		}
		showGhostEffect = ghostEffect;
		health.untouchable = true;
		DeviceLightingManager.TransitionLighting(Color.red, Color.red, 0f, DeviceLightingManager.F_KEYS);
		StartCoroutine(Delay(0.1f, delegate
		{
			DeviceLightingManager.TransitionLighting(Color.white, Color.white, 0f, DeviceLightingManager.F_KEYS);
			StartCoroutine(Delay(0.1f, delegate
			{
				DeviceLightingManager.TransitionLighting(Color.red, Color.red, 0f, DeviceLightingManager.F_KEYS);
				StartCoroutine(Delay(0.1f, delegate
				{
					DeviceLightingManager.PulseAllLighting(Color.white, Color.black, 0.1f, DeviceLightingManager.F_KEYS);
				}));
			}));
		}));
	}

	private IEnumerator Delay(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	public void MakeInvincible(float duration)
	{
		invincibleTimer = duration;
		health.invincible = true;
	}

	public IEnumerator HitEffects(StateMachine.State EntryState, StateMachine.State NextState)
	{
		HitTimer = 0f;
		speed = 0f;
		HitDuration = 1f;
		health.untouchable = true;
		untouchableTimer = 1f * DifficultyManager.GetInvincibleTimeMultiplier();
		Z = 0f;
		while ((HitTimer += Time.unscaledDeltaTime) < 0.1f)
		{
			yield return null;
		}
		VZ = KnockbackVelocity;
		speed = 10f;
		while ((speed -= 0.9f * GameManager.DeltaTime) > 0f)
		{
			HitTimer += Time.deltaTime;
			yield return null;
		}
		if (NextState != StateMachine.State.GameOver && NextState != StateMachine.State.Resurrecting)
		{
			untouchableTimer = 1f * DifficultyManager.GetInvincibleTimeMultiplier();
		}
		state.CURRENT_STATE = NextState;
		HitEffectsCoroutine = null;
	}

	public void StopHitEffects()
	{
		if (HitEffectsCoroutine != null)
		{
			health.untouchable = true;
			untouchableTimer = 1f * DifficultyManager.GetInvincibleTimeMultiplier();
			StopCoroutine(HitEffectsCoroutine);
			HitEffectsCoroutine = null;
		}
	}

	public void BlockAttacker(GameObject attacker)
	{
		if (health.IsAttackerInDamageEventQueue(attacker))
		{
			health.ForgiveRecentDamage();
		}
	}

	private void KillPlayer()
	{
		DataManager.Instance.Followers.Clear();
		OnDie(base.gameObject, Vector3.zero, null, Health.AttackTypes.Melee, (Health.AttackFlags)0);
	}

	public void SetFootSteps(Color FootStepColor)
	{
		FootPrints = FootPrintsNum;
		this.FootStepColor = FootStepColor;
	}

	public void EmitFootprints()
	{
		AudioManager.Instance.PlayFootstepPlayer(base.transform.position);
		if (DataManager.Instance.PlayerScaleModifier > 1)
		{
			CameraManager.instance.ShakeCameraForDuration(0.15f, 0.2f, 0.1f);
		}
		if ((FootPrints -= 1f) > 0f)
		{
			BiomeConstants.Instance.EmitFootprintsParticles(base.transform.position, FootStepColor, Mathf.Min(1f, FootPrints / FootPrintModifier));
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.StopActiveLoops();
		AudioManager.Instance.PlayOneShot("event:/player/death_hit", base.gameObject);
		SimpleSpineFlash.FlashWhite(false);
		health.invincible = true;
		health.untouchable = true;
		GetComponent<Interactor>().HideIndicator();
		GameManager.GetInstance().HitStop(0.3f);
		state.CURRENT_STATE = StateMachine.State.Dieing;
		if (Attacker != null)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, Attacker.transform.position);
		}
		forceDir = state.facingAngle + 180f;
		DataManager.Instance.PlayerDamageReceived += 50f;
		Debug.Log("ResurrectOnHud.HasRessurection " + ResurrectOnHud.HasRessurection);
		if (MMConversation.isPlaying)
		{
			MMConversation mmConversation = MMConversation.mmConversation;
			if ((object)mmConversation != null)
			{
				mmConversation.Close();
			}
		}
		PlayerFarming.Instance.Spine.UseDeltaTime = false;
		PlayerFarming.Instance.Spine.timeScale = 1f;
		state.CURRENT_STATE = StateMachine.State.GameOver;
		state.LockStateChanges = true;
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 4f, 2f).SetEase(Ease.OutSine).SetUpdate(true);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		HUD_Manager.Instance.ShowBW(1f, 0f, 1f);
		MeshRenderer[] array = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		EnemiesTurnedOff.Clear();
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if (meshRenderer.gameObject.activeSelf && meshRenderer.gameObject.layer != 14 && meshRenderer.gameObject.layer != 15 && Vector3.Distance(meshRenderer.transform.position, base.transform.position) < 1f && meshRenderer.transform.position.y < base.transform.position.y)
			{
				meshRenderer.enabled = false;
				EnemiesTurnedOff.Add(meshRenderer);
			}
		}
		SpriteRenderer[] array3 = UnityEngine.Object.FindObjectsOfType<SpriteRenderer>();
		SpritesTurnedOff.Clear();
		SpriteRenderer[] array4 = array3;
		foreach (SpriteRenderer spriteRenderer in array4)
		{
			if (spriteRenderer.enabled && spriteRenderer.gameObject.activeSelf && Vector3.Distance(spriteRenderer.transform.position, base.transform.position) < 2f && spriteRenderer.transform.position.y < base.transform.position.y)
			{
				spriteRenderer.enabled = false;
				SpritesTurnedOff.Add(spriteRenderer);
			}
		}
		DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance * 2f, 1f).SetEase(Ease.OutQuart);
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(GameObject.FindWithTag("Player Camera Bone"), 4f);
		DataManager.Instance.playerDeaths++;
		DataManager.Instance.playerDeathsInARow++;
		AudioManager.Instance.PlayMusic("event:/music/game_over/game_over");
		if (BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentX == BiomeGenerator.BossCoords.x && BiomeGenerator.Instance.CurrentY == BiomeGenerator.BossCoords.y)
		{
			DataManager.Instance.playerDeathsInARowFightingLeader++;
		}
		if (!TrinketManager.HasTrinket(TarotCards.Card.TheDeal) || TrinketManager.IsOnCooldown(TarotCards.Card.TheDeal))
		{
			UIBossHUD.Hide();
		}
		CameraManager.shakeCamera(0.5f, 0f - state.facingAngle);
	}

	public void OnFinalGameOver()
	{
		AudioManager.Instance.PlayOneShot("event:/player/death_hit", base.gameObject);
		health.invincible = true;
		health.untouchable = true;
		GetComponent<Interactor>().HideIndicator();
		GameManager.GetInstance().HitStop(0.3f);
		state.CURRENT_STATE = StateMachine.State.FinalGameOver;
		if (MMConversation.isPlaying)
		{
			MMConversation mmConversation = MMConversation.mmConversation;
			if ((object)mmConversation != null)
			{
				mmConversation.Close();
			}
		}
		PlayerFarming.Instance.Spine.UseDeltaTime = false;
		PlayerFarming.Instance.Spine.timeScale = 1f;
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 4f, 2f).SetEase(Ease.OutSine).SetUpdate(true);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		HUD_Manager.Instance.ShowBW(1f, 0f, 1f);
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(GameObject.FindWithTag("Player Camera Bone"), 4f);
		DataManager.Instance.playerDeaths++;
		AudioManager.Instance.PlayMusic("event:/music/game_over/game_over");
		UIBossHUD.Hide();
		CameraManager.shakeCamera(0.5f, 0f - state.facingAngle);
	}

	public void DoGrapple(Interaction_Grapple GrappleTarget)
	{
		GameManager.GetInstance().HitStop();
		TargetGrapple = GrappleTarget;
		speed = 0f;
		state.facingAngle = Utils.GetAngle(base.transform.position, GrappleTarget.transform.position);
		state.CURRENT_STATE = StateMachine.State.Grapple;
		circleCollider2D.enabled = false;
		playerFarming.simpleSpineAnimator.Animate("grapple", 0, false);
		playerFarming.simpleSpineAnimator.AddAnimate("grapple-fly", 0, true, 0f);
		GrappleChain.gameObject.SetActive(true);
		GrappleChain.textureMode = LineTextureMode.Tile;
		ElevatorProgress = (GrappleProgress = 0f);
		GrappleChain.SetPosition(0, BoneTool.position);
		GrappleChain.SetPosition(1, BoneTool.position);
	}

	public void DoElevator(Vector3 ElevatorPosition, Interaction_Elevator TargetElevator, float CurretElevatorZ, float TargetElevatorZ)
	{
		GameManager.GetInstance().HitStop();
		this.TargetElevator = TargetElevator;
		speed = 0f;
		state.facingAngle = Utils.GetAngle(base.transform.position, TargetElevator.transform.position);
		state.CURRENT_STATE = StateMachine.State.Elevator;
		circleCollider2D.enabled = false;
		playerFarming.simpleSpineAnimator.Animate("grapple", 0, false);
		playerFarming.simpleSpineAnimator.AddAnimate("grapple-fly", 0, true, 0f);
		GrappleChain.gameObject.SetActive(true);
		GrappleChain.textureMode = LineTextureMode.Tile;
		GrappleProgress = 0f;
		ElevatorProgress = 0f;
		ElevatorProgressSpeed = 0f;
		GrappleChain.SetPosition(0, BoneTool.position);
		GrappleChain.SetPosition(1, BoneTool.position);
		this.CurretElevatorZ = CurretElevatorZ;
		this.TargetElevatorZ = TargetElevatorZ;
		this.ElevatorPosition = ElevatorPosition;
		ElevatorChangedFloor = false;
	}

	public void DoIslandDash(Vector3 TargetPosition)
	{
		DodgeTimer = 0f;
		this.TargetPosition = TargetPosition;
		speed = DodgeSpeed * 1.2f;
		state.CURRENT_STATE = StateMachine.State.DashAcrossIsland;
		circleCollider2D.enabled = false;
	}

	public void ToggleGhost(bool toggle)
	{
		_skeletonGhost.ghostingEnabled = toggle;
	}

	private void DoIdle()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private void Update()
	{
		if (Time.timeScale <= 0f && state.CURRENT_STATE != StateMachine.State.Resurrecting && state.CURRENT_STATE != StateMachine.State.GameOver && state.CURRENT_STATE != StateMachine.State.FinalGameOver)
		{
			return;
		}
		if (untouchableTimer > 0f)
		{
			if (showGhostEffect)
			{
				if (PlayerRelic.InvincibleFromRelic)
				{
					Time.timeScale = 1.25f;
					AudioManager.Instance.SetMusicPitch(1.25f);
				}
				if (!playedLoop)
				{
					_loopSound = AudioManager.Instance.CreateLoop("event:/relics/invincible", base.gameObject, true);
					playedLoop = true;
				}
				_skeletonGhost.ghostingEnabled = true;
			}
			SimpleSpineFlash.FlashMeWhite(1f, 7);
			untouchableTimer -= Time.deltaTime;
			if (untouchableTimer <= 0f)
			{
				if (showGhostEffect)
				{
					if (PlayerRelic.InvincibleFromRelic)
					{
						Time.timeScale = 1f;
						AudioManager.Instance.SetMusicPitch(1f);
						PlayerRelic.InvincibleFromRelic = false;
					}
					AudioManager.Instance.StopLoop(_loopSound);
					playedLoop = false;
					_skeletonGhost.ghostingEnabled = false;
				}
				health.untouchable = false;
				DeviceLightingManager.StopAll();
				DeviceLightingManager.UpdateLocation();
				if (health.HP <= 1f)
				{
					DeviceLightingManager.PulseAllLighting(Color.white, Color.red, 0.35f, new KeyCode[0]);
				}
			}
		}
		else if (SimpleSpineFlash.isFillWhite)
		{
			SimpleSpineFlash.FlashWhite(false);
		}
		if (invincibleTimer > 0f)
		{
			invincibleTimer -= Time.deltaTime;
			if (invincibleTimer <= 0f)
			{
				health.untouchable = false;
			}
		}
		if (!playerFarming.GoToAndStopping)
		{
			xDir = InputManager.Gameplay.GetHorizontalAxis();
			yDir = InputManager.Gameplay.GetVerticalAxis();
			if (state.CURRENT_STATE == StateMachine.State.Moving)
			{
				speed *= Mathf.Clamp01(new Vector2(xDir, yDir).magnitude);
			}
			speed = Mathf.Max(speed, 0f);
			unitObject.vx = speed * Mathf.Cos(forceDir * ((float)Math.PI / 180f));
			unitObject.vy = speed * Mathf.Sin(forceDir * ((float)Math.PI / 180f));
		}
		else
		{
			xDir = (yDir = 0f);
			playerFarming.Spine.AnimationState.TimeScale = 1f;
		}
		enlargedCollider.gameObject.SetActive(DataManager.Instance.PlayerScaleModifier > 1);
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			Z = 0f;
			SpineTransform.localPosition = Vector3.zero;
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if (Mathf.Abs(xDir) > MinInputForMovement || Mathf.Abs(yDir) > MinInputForMovement)
			{
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			if (DataManager.Instance.GameOver)
			{
				OnFinalGameOver();
			}
			else
			{
				if (!DataManager.Instance.DisplayGameOverWarning)
				{
					break;
				}
				DataManager.Instance.DisplayGameOverWarning = false;
				DataManager.Instance.InGameOver = true;
				if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.GameOver))
				{
					UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.GameOver);
					uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
					{
						ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/GameOver", Objectives.CustomQuestTypes.GameOver, -1, 4800f));
					});
				}
			}
			break;
		case StateMachine.State.Moving:
			if (playerFarming.GoToAndStopping || Time.timeScale == 0f)
			{
				break;
			}
			if (Mathf.Abs(xDir) <= MinInputForMovement && Mathf.Abs(yDir) <= MinInputForMovement)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				break;
			}
			forceDir = Utils.GetAngle(Vector3.zero, new Vector3(xDir, yDir));
			if (unitObject.vx != 0f || unitObject.vy != 0f)
			{
				state.facingAngle = Utils.GetAngle(base.transform.position, base.transform.position + new Vector3(unitObject.vx, unitObject.vy));
			}
			state.LookAngle = state.facingAngle;
			speed += (GetPlayerMaxSpeed() - speed) / 3f * GameManager.DeltaTime;
			break;
		case StateMachine.State.Aiming:
			if (!playerFarming.GoToAndStopping)
			{
				if (new Vector2(xDir, yDir).sqrMagnitude > 0f)
				{
					state.facingAngle = Utils.GetAngle(Vector3.zero, new Vector3(xDir, yDir));
				}
				state.LookAngle = state.facingAngle;
				speed += (0f - speed) / 7f * GameManager.DeltaTime;
			}
			break;
		case StateMachine.State.ChargingHeavyAttack:
		{
			if (playerFarming.GoToAndStopping)
			{
				break;
			}
			if (Mathf.Abs(xDir) <= MinInputForMovement && Mathf.Abs(yDir) <= MinInputForMovement)
			{
				speed += (0f - speed) / 3f * GameManager.DeltaTime;
				break;
			}
			forceDir = Utils.GetAngle(Vector3.zero, new Vector3(xDir, yDir));
			Vector3 vector = new Vector3(unitObject.vx, unitObject.vy);
			if (vector != Vector3.zero)
			{
				state.facingAngle = Utils.GetAngle(base.transform.position, base.transform.position + vector);
			}
			speed += (RunSpeed * 0.7f - speed) / 3f * GameManager.DeltaTime;
			break;
		}
		case StateMachine.State.Attacking:
			if (!playerFarming.GoToAndStopping)
			{
				LungeTimer -= Time.deltaTime;
				forceDir = state.facingAngle;
				speed += (LungeSpeed * (LungeTimer / LungeDuration) - speed) / 3f * GameManager.DeltaTime;
			}
			break;
		case StateMachine.State.Dodging:
			Z = 0f;
			SpineTransform.localPosition = Vector3.zero;
			forceDir = state.facingAngle;
			if (DodgeCollisionDelay < 0f)
			{
				speed = Mathf.Lerp(speed, DodgeSpeed, 2f * Time.deltaTime);
			}
			DodgeCollisionDelay -= Time.deltaTime;
			DodgeTimer += Time.deltaTime;
			if (DodgeTimer < 0.1f && (Mathf.Abs(xDir) > MinInputForMovement || Mathf.Abs(yDir) > MinInputForMovement))
			{
				MMVibrate.Rumble(0.2f, 0.2f, 0.25f, this);
				state.facingAngle = (forceDir = Utils.GetAngle(Vector3.zero, new Vector3(xDir, yDir)));
			}
			if ((!InputManager.Gameplay.GetDodgeButtonHeld() && DodgeTimer > DodgeDuration) || DodgeTimer > DodgeMaxDuration)
			{
				DodgeTimer = 0f;
				DodgeCollisionDelay = 0f;
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.DashAcrossIsland:
			speed = Mathf.Lerp(speed, DodgeSpeed, 2f * Time.deltaTime);
			DodgeTimer += Time.deltaTime;
			if (DodgeTimer > DodgeDuration)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				circleCollider2D.enabled = true;
			}
			break;
		case StateMachine.State.Elevator:
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetElevator.transform.position);
			forceDir = state.facingAngle;
			GrappleChain.SetPosition(0, BoneTool.position);
			speed = 0f;
			GrappleChain.SetPosition(1, Vector3.Lerp(BoneTool.position, TargetElevator.transform.position, GrappleProgress));
			if (GrappleProgress < 1f)
			{
				GrappleProgress += 5f * Time.deltaTime;
				if (GrappleProgress >= 1f)
				{
					ElevatorProgressSpeed = 0f;
					CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, TargetElevator.transform.position));
				}
			}
			else if (GrappleProgress >= 1f)
			{
				if (ElevatorProgressSpeed < 6f)
				{
					ElevatorProgressSpeed += 0.15f;
				}
				ElevatorProgress += ElevatorProgressSpeed * Time.deltaTime;
				base.transform.position = Vector3.Lerp(ElevatorPosition, TargetElevator.transform.position, ElevatorProgress);
				if (ElevatorProgress >= 0.5f && !ElevatorChangedFloor)
				{
					ElevatorChangedFloor = true;
				}
				if (ElevatorProgress >= 1f)
				{
					GameManager.GetInstance().HitStop();
					CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, TargetElevator.transform.position));
					base.transform.position = TargetElevator.transform.position;
					state.CURRENT_STATE = StateMachine.State.Idle;
					circleCollider2D.enabled = true;
					GrappleChain.gameObject.SetActive(false);
				}
			}
			break;
		case StateMachine.State.Grapple:
			state.facingAngle = Utils.GetAngle(base.transform.position, TargetGrapple.transform.position);
			forceDir = state.facingAngle;
			GrappleChain.SetPosition(0, BoneTool.position);
			if (GrappleProgress < 1f)
			{
				GrappleProgress += 0.1f;
				if (GrappleProgress >= 1f)
				{
					TargetGrapple.BecomeTarget();
					speed = -10f;
					CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, TargetGrapple.transform.position));
				}
			}
			else if (speed < 20f)
			{
				speed += 1f * GameManager.DeltaTime;
			}
			GrappleChain.SetPosition(1, Vector3.Lerp(BoneTool.position, TargetGrapple.BoneTarget.position, GrappleProgress));
			if (Vector2.Distance(base.transform.position, TargetGrapple.transform.position) < speed * Time.deltaTime)
			{
				base.transform.position = TargetGrapple.transform.position;
				speed = 0f;
				state.CURRENT_STATE = StateMachine.State.Idle;
				circleCollider2D.enabled = true;
				TargetGrapple.BecomeOrigin();
				GrappleChain.gameObject.SetActive(false);
				CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, TargetGrapple.transform.position));
				GameManager.GetInstance().HitStop();
			}
			break;
		case StateMachine.State.HitThrown:
			if (HitTimer > 0.1f)
			{
				VZ -= KnockbackGravity;
				Z += VZ;
				if (Z < 0f)
				{
					VZ *= KnockbackBounce;
					Z = 0f;
				}
				SpinePosition = SpineTransform.localPosition;
				SpineTransform.localPosition = SpinePosition;
			}
			break;
		case StateMachine.State.HitRecover:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if ((HitRecoverTimer += Time.deltaTime) > 0.3f)
			{
				HitRecoverTimer = 0f;
				state.CURRENT_STATE = StateMachine.State.Idle;
				SpinePosition = SpineTransform.localPosition;
				SpinePosition.z = 0f;
				SpineTransform.localPosition = SpinePosition;
			}
			break;
		case StateMachine.State.Teleporting:
			speed = 0f;
			break;
		case StateMachine.State.Converting:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if ((ConversionTimer += Time.deltaTime) > 7.3f)
			{
				ConversionTimer = 0f;
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.Dieing:
			if (HitTimer > 0.3f)
			{
				VZ -= KnockbackGravity;
				Z += VZ;
				if (Z < 0f)
				{
					VZ *= KnockbackBounce;
					Z = 0f;
				}
				SpinePosition = SpineTransform.localPosition;
				SpineTransform.localPosition = SpinePosition;
			}
			break;
		case StateMachine.State.Resurrecting:
		{
			int num = 0;
			int count = droppedItems.Count;
			for (int num2 = droppedItems.Count - 1; num2 >= 0; num2--)
			{
				Vector3 vector2 = Utils.DegreeToVector2((float)num * (360f / (float)count));
				ResourceCustomTarget.Create(base.gameObject, base.transform.position + vector2, (InventoryItem.ITEM_TYPE)droppedItems[num2].Type, null);
				Inventory.AddItem(droppedItems[num2].Type, droppedItems[num2].Quantity);
				droppedItems.Remove(droppedItems[num2]);
				num++;
			}
			Time.timeScale = 0f;
			playerFarming.Spine.UseDeltaTime = false;
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if (!((state.Timer += Time.unscaledDeltaTime) > 3f))
			{
				break;
			}
			Time.timeScale = 1f;
			playerFarming.Spine.UseDeltaTime = true;
			health.enabled = true;
			health.invincible = false;
			untouchableTimer = 1f * DifficultyManager.GetInvincibleTimeMultiplier();
			health.untouchable = true;
			HUD_Manager.Instance.ShowBW(1f, 0f, 0f);
			GameManager.GetInstance().OnConversationEnd();
			GameManager.GetInstance().CameraResetTargetZoom();
			droppedItems.Clear();
			if (BiomeGenerator.Instance != null && (bool)RespawnRoomManager.Instance && !RespawnRoomManager.Instance.Respawning)
			{
				AudioManager.Instance.PlayMusic(BiomeGenerator.Instance.biomeMusicPath);
				if (BiomeGenerator.Instance.CurrentRoom != null && BiomeGenerator.Instance.CurrentRoom.generateRoom != null)
				{
					AudioManager.Instance.SetMusicRoomID(BiomeGenerator.Instance.CurrentRoom.generateRoom.roomMusicID);
				}
			}
			HealthPlayer component = playerFarming.GetComponent<HealthPlayer>();
			ResurrectionType resurrectionType = ResurrectOnHud.ResurrectionType;
			if (resurrectionType != ResurrectionType.Pyre && resurrectionType == ResurrectionType.DealTarot)
			{
				TrinketManager.TriggerCooldown(TarotCards.Card.TheDeal);
				component.HP = 2f;
				PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
			}
			else
			{
				component.HP = RespawnRoomManager.HP;
				component.SpiritHearts = RespawnRoomManager.SpiritHearts;
				component.BlueHearts = RespawnRoomManager.BlueHearts;
				component.BlackHearts = RespawnRoomManager.BlackHearts;
			}
			if (ResurrectOnHud.OverridenResurrectionType != 0)
			{
				ResurrectOnHud.ResurrectionType = ResurrectOnHud.OverridenResurrectionType;
				ResurrectOnHud.OverridenResurrectionType = ResurrectionType.None;
			}
			else
			{
				ResurrectOnHud.ResurrectionType = ResurrectionType.None;
			}
			break;
		}
		case StateMachine.State.Dead:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if ((DeathTimer += Time.deltaTime) > 2f && !GameOver)
			{
				GameOver = true;
				DeathTimer = 0f;
				GameManager.ToShip();
			}
			break;
		case StateMachine.State.GameOver:
		{
			if (CanRespawn)
			{
				Time.timeScale = 0f;
			}
			PlayerFarming.Instance.Spine.UseDeltaTime = false;
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			state.Timer += Time.unscaledDeltaTime;
			bool flag = PlayerFleeceManager.FleecePreventsRespawn();
			if (!flag && TrinketManager.HasTrinket(TarotCards.Card.TheDeal) && !TrinketManager.IsOnCooldown(TarotCards.Card.TheDeal))
			{
				state.LockStateChanges = false;
				state.CURRENT_STATE = StateMachine.State.Resurrecting;
				ReenableTurnedOffEnemies();
				ReenableTurnedOffSpriteRenderers();
			}
			else if (!flag && ResurrectOnHud.HasRessurection && PlayerFarming.Location != FollowerLocation.Boss_5 && !DungeonSandboxManager.Active)
			{
				if (CanRespawn && state.Timer > 2f && !MMTransition.IsPlaying)
				{
					HUD_Manager.Instance.ShowBW(0.5f, 1f, 0f);
					PlayerFarming.Instance.Spine.UseDeltaTime = true;
					RespawnRoomManager.Play();
					state.LockStateChanges = false;
					ReenableTurnedOffEnemies();
					ReenableTurnedOffSpriteRenderers();
				}
			}
			else if (CanRespawn && state.Timer > 2f && _deathScreenInstance == null && UIDeathScreenOverlayController.Instance == null)
			{
				_deathScreenInstance = MonoSingleton<UIManager>.Instance.ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results.Killed);
			}
			break;
		}
		case StateMachine.State.FinalGameOver:
			Time.timeScale = 0f;
			PlayerFarming.Instance.Spine.UseDeltaTime = false;
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			state.Timer += Time.unscaledDeltaTime;
			if (state.Timer > 2f && _deathScreenInstance == null)
			{
				_deathScreenInstance = MonoSingleton<UIManager>.Instance.ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results.GameOver, 0);
			}
			break;
		case StateMachine.State.SignPostAttack:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if (Mathf.Abs(xDir) > MinInputForMovement || Mathf.Abs(yDir) > MinInputForMovement)
			{
				state.facingAngle = (forceDir = Utils.GetAngle(Vector3.zero, new Vector3(xDir, yDir)));
			}
			if ((state.Timer -= Time.deltaTime) < 0f)
			{
				state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
				AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
				if (TimedActionCallback != null)
				{
					TimedActionCallback();
				}
			}
			break;
		case StateMachine.State.RecoverFromAttack:
			speed += (0f - speed) / 7f * GameManager.DeltaTime;
			state.Timer -= Time.deltaTime;
			if (state.Timer < 0f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.Respawning:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if ((RespawnTimer += Time.deltaTime) > 6f)
			{
				GameManager.GetInstance().OnConversationEnd();
				RespawnTimer = 0f;
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.SpawnIn:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if ((state.Timer += Time.deltaTime) > 3f)
			{
				Debug.Log("SpawnInShowHUD: ".Colour(Color.green) + SpawnInShowHUD);
				GameManager.GetInstance().OnConversationEnd(true, SpawnInShowHUD);
				state.CURRENT_STATE = StateMachine.State.Idle;
				SpawnInShowHUD = true;
			}
			break;
		case StateMachine.State.TimedAction:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if ((state.Timer -= Time.deltaTime) < 0f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				if (TimedActionCallback != null)
				{
					TimedActionCallback();
				}
			}
			break;
		case StateMachine.State.Idle_CarryingBody:
			Z = 0f;
			SpineTransform.localPosition = Vector3.zero;
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			if (Mathf.Abs(xDir) > MinInputForMovement || Mathf.Abs(yDir) > MinInputForMovement)
			{
				state.CURRENT_STATE = StateMachine.State.Moving_CarryingBody;
			}
			break;
		case StateMachine.State.Moving_CarryingBody:
			if (playerFarming.GoToAndStopping)
			{
				break;
			}
			if (Mathf.Abs(xDir) <= MinInputForMovement && Mathf.Abs(yDir) <= MinInputForMovement)
			{
				state.CURRENT_STATE = StateMachine.State.Idle_CarryingBody;
				break;
			}
			forceDir = Utils.GetAngle(Vector3.zero, new Vector3(xDir, yDir));
			state.facingAngle = Utils.GetAngle(base.transform.position, base.transform.position + new Vector3(unitObject.vx, unitObject.vy));
			state.LookAngle = state.facingAngle;
			speed += (RunSpeed * 0.5f - speed) / 3f * GameManager.DeltaTime;
			switch (Utils.GetAngleDirectionFull(forceDir))
			{
			case Utils.DirectionFull.Up:
				if (playerFarming.Spine.AnimationState.GetCurrent(0).Animation.Name != carryUp)
				{
					playerFarming.Spine.AnimationState.SetAnimation(0, carryUp, true);
				}
				break;
			case Utils.DirectionFull.Down:
				if (playerFarming.Spine.AnimationState.GetCurrent(0).Animation.Name != carryDown)
				{
					playerFarming.Spine.AnimationState.SetAnimation(0, carryDown, true);
				}
				break;
			case Utils.DirectionFull.Up_Diagonal:
				if (playerFarming.Spine.AnimationState.GetCurrent(0).Animation.Name != carryUpDiagonal)
				{
					playerFarming.Spine.AnimationState.SetAnimation(0, carryUpDiagonal, true);
				}
				break;
			case Utils.DirectionFull.Down_Diagonal:
				if (playerFarming.Spine.AnimationState.GetCurrent(0).Animation.Name != carryDownDiagonal)
				{
					playerFarming.Spine.AnimationState.SetAnimation(0, carryDownDiagonal, true);
				}
				break;
			case Utils.DirectionFull.Left:
			case Utils.DirectionFull.Right:
				if (playerFarming.Spine.AnimationState.GetCurrent(0).Animation.Name != carryHorizontal)
				{
					playerFarming.Spine.AnimationState.SetAnimation(0, carryHorizontal, true);
				}
				break;
			}
			break;
		default:
			speed += (0f - speed) / 3f * GameManager.DeltaTime;
			break;
		case StateMachine.State.Stealth:
			break;
		}
		if (state.CURRENT_STATE != StateMachine.State.Dodging && state.CURRENT_STATE != StateMachine.State.HitThrown && state.CURRENT_STATE != StateMachine.State.DashAcrossIsland && state.CURRENT_STATE != StateMachine.State.Grapple)
		{
			SeperateFromEnemies();
		}
		else
		{
			unitObject.seperatorVX = 0f;
			unitObject.seperatorVY = 0f;
		}
		if (playerFarming.Spine != null && playerFarming.Spine.AnimationState != null)
		{
			if (state.CURRENT_STATE == StateMachine.State.Moving && !playerFarming.GoToAndStopping)
			{
				playerFarming.Spine.AnimationState.TimeScale = Mathf.Clamp01(speed / RunSpeed);
			}
			else
			{
				playerFarming.Spine.AnimationState.TimeScale = 1f;
			}
		}
	}

	public float GetPlayerMaxSpeed()
	{
		return RunSpeed * (playerWeapon.GetCurrentWeapon().MovementSpeedMultiplier * TrinketManager.GetMovementSpeedMultiplier()) * Mathf.Lerp(1.25f, 0.7f, (float)DataManager.Instance.PlayerScaleModifier / 2f) * (GameManager.IsDungeon(PlayerFarming.Location) ? PlayerFleeceManager.FleeceSpeedMultiplier() : 1f);
	}

	public void SetCarryingObjectAnimations(string idle, string carryUp, string carryDown, string carryDownDiagonal, string carryUpDiagonal, string carryHorizontal)
	{
		PlayerFarming.Instance.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Idle_CarryingBody, idle);
		this.carryUp = carryUp;
		this.carryDown = carryDown;
		this.carryDownDiagonal = carryDownDiagonal;
		this.carryUpDiagonal = carryUpDiagonal;
		this.carryHorizontal = carryHorizontal;
	}

	public void ResetCarryingObjectAnimations()
	{
		PlayerFarming.Instance.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Idle_CarryingBody, "corpse/corpse-idle");
		carryUp = "corpse/corpse-run-up";
		carryDown = "corpse/corpse-run-down";
		carryDownDiagonal = "corpse/corpse-run";
		carryUpDiagonal = "corpse/corpse-run-up-diagonal";
		carryHorizontal = "corpse/corpse-run-horizontal";
	}

	public void Lunge(float lungeDuration, float lungeSpeed)
	{
		LungeDuration = lungeDuration;
		LungeTimer = LungeDuration;
		LungeSpeed = lungeSpeed * LungeDampener;
	}

	public void CancelLunge(float hitKnockback)
	{
		LungeTimer = 0f;
		speed = 0f;
		if (hitKnockback != 0f)
		{
			float angle = Mathf.Repeat(state.facingAngle + 180f, 360f) * ((float)Math.PI / 180f);
			unitObject.DoKnockBack(angle, hitKnockback, 0.1f);
		}
	}

	private void SeperateFromEnemies()
	{
		unitObject.Seperate(1f);
	}

	private void ReenableTurnedOffEnemies()
	{
		foreach (MeshRenderer item in EnemiesTurnedOff)
		{
			if (item != null)
			{
				item.enabled = true;
			}
		}
		EnemiesTurnedOff.Clear();
	}

	private void ReenableTurnedOffSpriteRenderers()
	{
		foreach (SpriteRenderer item in SpritesTurnedOff)
		{
			if (item != null)
			{
				item.enabled = true;
			}
		}
		SpritesTurnedOff.Clear();
	}
}
