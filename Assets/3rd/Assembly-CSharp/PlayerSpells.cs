using System;
using System.Collections;
using System.Collections.Generic;
using MMRoomGeneration;
using Rewired;
using Spine.Unity;
using UnityEngine;

public class PlayerSpells : BaseMonoBehaviour
{
	public delegate void CurseEvent(EquipmentType curse, int Level);

	public delegate void CastEvent(EquipmentType type);

	public const int AmmoCostBaseLine = 45;

	private PlayerFarming playerFarming;

	private StateMachine state;

	private Health health;

	private PlayerController playerController;

	private float castTimer;

	public SkeletonAnimation Spine;

	private float aimTimer;

	private static float minAimTime = 0.15f;

	private static float maxAimTime = 0.3f;

	[SerializeField]
	private GameObject projectileCorrectTimingParticle;

	public static Action CurseDamaged;

	public static Action CurseBroken;

	private const float projectileAOEShootDistance = 5f;

	[SerializeField]
	private LayerMask collisionMask;

	private float explosiveProjectileMaxChargeDuration = 0.75f;

	private float megaSlashMaxChargeDuration = 0.75f;

	private float chargeTimer;

	public static CurseEvent OnCurseChanged;

	private Vector3 targetPosition;

	private float acceleration;

	private bool aiming;

	private Plane plane = new Plane(Vector3.forward, Vector3.zero);

	private const float maxProjectileAOEDistance = 6f;

	private float TimeScaleDelay;

	public bool ForceSpells;

	private bool cantafford;

	public float AimAngle;

	private Health AimTarget;

	private float ArrowAttackDelay;

	private bool invokedDamage;

	public static Action OnCastSpell;

	public static int AmmoCost
	{
		get
		{
			return Mathf.RoundToInt(45f / TrinketManager.GetAmmoEfficiencyMultiplier() * DataManager.Instance.CurseFeverMultiplier * PlayerFleeceManager.GetCursesFeverMultiplier());
		}
	}

	public float SlowMotionSpeed
	{
		get
		{
			EquipmentType currentCurse = DataManager.Instance.CurrentCurse;
			if ((uint)(currentCurse - 700) <= 4u)
			{
				return 0.25f;
			}
			return 0.4f;
		}
	}

	private Transform spellParent
	{
		get
		{
			if (!GenerateRoom.Instance)
			{
				return base.transform.parent;
			}
			return GenerateRoom.Instance.transform;
		}
	}

	public static event CastEvent OnSpellCast;

	private void Start()
	{
		playerFarming = GetComponent<PlayerFarming>();
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
		playerController = GetComponent<PlayerController>();
		SetSpell(DataManager.Instance.CurrentCurse, DataManager.Instance.CurrentCurseLevel);
	}

	public void SetSpell(EquipmentType Spell, int CurseLevel)
	{
		DataManager.Instance.CurrentCurse = Spell;
		DataManager.Instance.CurrentCurseLevel = CurseLevel;
		CurseEvent onCurseChanged = OnCurseChanged;
		if (onCurseChanged != null)
		{
			onCurseChanged(DataManager.Instance.CurrentCurse, DataManager.Instance.CurrentCurseLevel);
		}
	}

	private void Update()
	{
		if (!DataManager.Instance.EnabledSpells || DataManager.Instance.CurrentCurse == EquipmentType.None)
		{
			return;
		}
		ArrowAttackDelay -= Time.deltaTime;
		if (playerFarming.GoToAndStopping || Time.timeScale == 0f)
		{
			return;
		}
		if (InputManager.Gameplay.GetCurseButtonDown())
		{
			aiming = true;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
		case StateMachine.State.Moving:
		case StateMachine.State.Aiming:
			if ((!LocationManager.LocationIsDungeon(PlayerFarming.Location) && !ForceSpells) || !aiming)
			{
				return;
			}
			if (state.CURRENT_STATE == StateMachine.State.Aiming && InputManager.General.MouseInputActive)
			{
				float angle = Utils.GetAngle(GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().WorldToScreenPoint(base.transform.position), InputManager.General.GetMousePosition());
				state.facingAngle = (state.LookAngle = (playerController.forceDir = angle));
			}
			if (!FaithAmmo.CanAfford(AmmoCost))
			{
				if (InputManager.Gameplay.GetCurseButtonDown())
				{
					FaithAmmo.UseAmmo(AmmoCost);
					AudioManager.Instance.PlayOneShot("event:/player/Curses/noarrows", base.gameObject);
				}
			}
			else if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).CanAim)
			{
				if (InputManager.Gameplay.GetCurseButtonUp())
				{
					CastCurrentSpell();
				}
			}
			else if (InputManager.Gameplay.GetCurseButtonDown())
			{
				CastCurrentSpell(false);
			}
			if (state.CURRENT_STATE == StateMachine.State.Aiming)
			{
				EquipmentType primaryEquipmentType = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PrimaryEquipmentType;
				switch (primaryEquipmentType)
				{
				case EquipmentType.Fireball:
					ChargeFireball();
					break;
				case EquipmentType.MegaSlash:
					ChargeMegaSlash();
					break;
				}
				if (primaryEquipmentType == EquipmentType.Fireball || primaryEquipmentType == EquipmentType.Tentacles || primaryEquipmentType == EquipmentType.MegaSlash)
				{
					GameManager.GetInstance().CamFollowTarget.SetOffset(Utils.DegreeToVector2(state.facingAngle) * 2f);
				}
			}
			else
			{
				GameManager.GetInstance().CamFollowTarget.SetOffset(Vector3.zero);
			}
			break;
		case StateMachine.State.Casting:
			if ((castTimer -= Time.deltaTime) <= 0f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				if (Interactor.CurrentInteraction != null)
				{
					Interactor.CurrentInteraction.HasChanged = true;
				}
			}
			break;
		case StateMachine.State.Dodging:
			if (aiming)
			{
				aiming = false;
				chargeTimer = 0f;
				aimTimer = 0f;
				GameManager.GetInstance().CamFollowTarget.SetOffset(Vector3.zero);
			}
			if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).CanAim && CanCastSpell() && InputManager.Gameplay.GetCurseButtonDown() && FaithAmmo.CanAfford(AmmoCost) && !LetterBox.IsPlaying && state.CURRENT_STATE != StateMachine.State.CustomAnimation)
			{
				aiming = true;
				state.CURRENT_STATE = StateMachine.State.Aiming;
				playerController.speed = 0f;
			}
			break;
		default:
			playerFarming.HideProjectileChargeBars();
			if (!LocationManager.LocationIsDungeon(PlayerFarming.Location) && !ForceSpells)
			{
				return;
			}
			break;
		}
		if (InputManager.Gameplay.GetCurseButtonDown())
		{
			AimAngle = state.LookAngle;
		}
		if (!FaithAmmo.CanAfford(AmmoCost))
		{
			cantafford = true;
		}
		if (cantafford && FaithAmmo.CanAfford(AmmoCost))
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/reload", base.transform.position);
			cantafford = false;
		}
		if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).CanAim && CanCastSpell() && InputManager.Gameplay.GetCurseButtonHeld() && FaithAmmo.CanAfford(AmmoCost) && !LetterBox.IsPlaying && state.CURRENT_STATE != StateMachine.State.Attacking && state.CURRENT_STATE != StateMachine.State.CustomAnimation && aiming)
		{
			if (!((aimTimer += Time.unscaledDeltaTime) > minAimTime))
			{
				return;
			}
			Time.timeScale = SlowMotionSpeed;
			if (state.CURRENT_STATE == StateMachine.State.Idle || state.CURRENT_STATE == StateMachine.State.Moving)
			{
				state.CURRENT_STATE = StateMachine.State.Aiming;
				if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PrimaryEquipmentType == EquipmentType.MegaSlash)
				{
					AudioManager.Instance.PlayOneShot("event:/player/Curses/mega_slash_charge", base.gameObject);
				}
				else if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PrimaryEquipmentType == EquipmentType.ProjectileAOE)
				{
					AudioManager.Instance.PlayOneShot("event:/player/Curses/goop_charge", base.gameObject);
				}
				else
				{
					AudioManager.Instance.PlayOneShot("event:/player/Curses/start_cast", base.gameObject);
				}
				if (!string.IsNullOrEmpty(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PerformActionAnimationLoop))
				{
					playerFarming.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Aiming, EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PerformActionAnimationLoop);
				}
				targetPosition = Vector3.zero;
				playerController.speed = 0f;
			}
			if (InputManager.General.GetLastActiveController().type == ControllerType.Keyboard && Mathf.DeltaAngle(AimAngle, state.LookAngle) < 100f)
			{
				AimAngle = Mathf.LerpAngle(AimAngle, state.LookAngle, 15f * Time.unscaledTime);
			}
			else
			{
				AimAngle = state.LookAngle;
			}
			if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PrimaryEquipmentType == EquipmentType.ProjectileAOE)
			{
				Vector3 vector = targetPosition + base.transform.position;
				if (state.CURRENT_STATE == StateMachine.State.Aiming && InputManager.General.MouseInputActive)
				{
					vector = GetProjectileAOETargetWithMouse();
				}
				for (int i = 0; i < 20; i++)
				{
					float num = (float)i / 20f;
					Vector3 position = Vector3.Lerp(base.transform.position + Vector3.forward, vector, num);
					position.z = base.transform.position.z + (0f - playerFarming.CurseAimingCurve.Evaluate(num)) * 3f;
					playerFarming.CurseAimLine.SetPosition(i, position);
				}
				playerFarming.CurseTarget.SetActive(true);
				playerFarming.CurseTarget.transform.position = vector;
				if (Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > 0.1f || Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > 0.1f)
				{
					acceleration = Mathf.Clamp01(acceleration + 8f * Time.unscaledDeltaTime);
				}
				else if (acceleration > 0f)
				{
					acceleration = Mathf.Clamp01(acceleration - 11f * Time.unscaledDeltaTime);
				}
				targetPosition += (Vector3)Utils.DegreeToVector2(AimAngle) * 10f * acceleration * Time.unscaledDeltaTime;
				targetPosition = Vector3.ClampMagnitude(targetPosition, 6f);
				return;
			}
			playerFarming.ShowProjectileChargeBars();
			float num2 = 0f;
			if (InputManager.General.GetLastActiveController().type == ControllerType.Keyboard)
			{
				Health autoAimTarget = GetAutoAimTarget(true);
				float num3 = AimAngle;
				if (autoAimTarget != null)
				{
					num3 = Utils.GetAngle(base.transform.position, autoAimTarget.transform.position);
				}
				num2 = num3;
			}
			else
			{
				num2 = AimAngle;
			}
			playerFarming.SetAimingRecticuleScaleAndRotation(0, new Vector3(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(aimTimer / maxAimTime)), 1f, 1f), new Vector3(0f, 0f, num2));
			playerFarming.SetAimingRecticuleScaleAndRotation(1, new Vector3(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(aimTimer / maxAimTime)), 1f, 1f), new Vector3(0f, 0f, Mathf.Repeat(num2 + 180f, 360f)));
			playerFarming.SetAimingRecticuleScaleAndRotation(2, new Vector3(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(aimTimer / maxAimTime)), 1f, 1f), new Vector3(0f, 0f, Mathf.Repeat(num2 + 90f, 360f)));
			playerFarming.SetAimingRecticuleScaleAndRotation(3, new Vector3(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(aimTimer / maxAimTime)), 1f, 1f), new Vector3(0f, 0f, Mathf.Repeat(num2 + 270f, 360f)));
			return;
		}
		Time.timeScale = 1f;
		aimTimer = 0f;
		chargeTimer = 0f;
		playerFarming.HideProjectileChargeBars();
		targetPosition = Vector3.zero;
		if (state.CURRENT_STATE == StateMachine.State.Aiming)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			if (Interactor.CurrentInteraction != null)
			{
				Interactor.CurrentInteraction.HasChanged = true;
			}
		}
		if (playerFarming.CurseTarget.activeSelf)
		{
			playerFarming.CurseAimLine.SetPositions(new Vector3[20]);
			playerFarming.CurseTarget.SetActive(false);
		}
	}

	private Vector3 GetProjectileAOETargetWithMouse()
	{
		Ray ray = GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().ScreenPointToRay(InputManager.General.GetMousePosition());
		float enter;
		plane.Raycast(ray, out enter);
		Vector3 vector = ray.GetPoint(enter) - base.transform.position;
		Vector3 normalized = vector.normalized;
		enter = Mathf.Min(6f, vector.magnitude);
		return base.transform.position + normalized * enter;
	}

	private void ChargeFireball()
	{
		playerFarming.ShowProjectileChargeBars();
		playerFarming.UpdateProjectileChargeBar(chargeTimer / explosiveProjectileMaxChargeDuration);
		chargeTimer += Time.unscaledDeltaTime;
	}

	private IEnumerator SlowMo()
	{
		GameManager.GetInstance().CameraSetZoom(6f);
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < 0.7f))
			{
				break;
			}
			GameManager.SetTimeScale(0.2f);
			yield return null;
		}
		GameManager.SetTimeScale(1f);
		GameManager.GetInstance().CameraResetTargetZoom();
	}

	private void ChargeMegaSlash()
	{
		playerFarming.ShowProjectileChargeBars();
		playerFarming.UpdateProjectileChargeBar(chargeTimer / megaSlashMaxChargeDuration);
		chargeTimer += Time.unscaledDeltaTime;
		if (playerFarming.Spine.AnimationState.GetCurrent(0).Animation.Name != EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PerformActionAnimationLoop)
		{
			playerFarming.Spine.AnimationState.SetAnimation(0, EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PerformActionAnimationLoop, true);
		}
	}

	private Health GetAutoAimTarget(bool prioritizeClosest = false, List<Health> blacklist = null)
	{
		float num = 180f;
		float num2 = float.MaxValue;
		StateMachine.State cURRENT_STATE = state.CURRENT_STATE;
		int num11 = 54;
		float num3 = 0f;
		float num4 = num / 2f;
		foreach (Health allUnit in Health.allUnits)
		{
			if (IsPotentialAutoAimTarget(allUnit, num))
			{
				float num5 = Vector2.Distance(base.transform.position, allUnit.transform.position);
				if (num5 > num3 && num5 < num2)
				{
					num3 = num5;
				}
			}
		}
		float num6 = 1f;
		AimTarget = null;
		foreach (Health allUnit2 in Health.allUnits)
		{
			if (allUnit2.team != Health.Team.Team2 || !IsPotentialAutoAimTarget(allUnit2, num) || (AimTarget != null && blacklist != null && blacklist.Contains(allUnit2)))
			{
				continue;
			}
			float num7 = Mathf.Sqrt(MagnitudeFindDistanceBetween(base.transform.position, allUnit2.transform.position)) / num2;
			if (!(Vector3.Dot(Utils.DegreeToVector2(AimAngle), (allUnit2.transform.position - base.transform.position).normalized) < (1f - num / 360f) * num7))
			{
				float num8 = Mathf.Abs(Mathf.DeltaAngle(AimAngle, Utils.GetAngle(base.transform.position, allUnit2.transform.position)));
				float num9 = Vector2.Distance(base.transform.position, allUnit2.transform.position);
				float num10 = 1f - Mathf.Cos(Mathf.Clamp01(num8 / num4) * (float)Math.PI / 2f);
				if (prioritizeClosest)
				{
					num10 += Mathf.Clamp01(num9 / num3) * 0.5f;
				}
				num10 /= allUnit2.autoAimAttractionFactor;
				if (num10 < num6)
				{
					AimTarget = allUnit2;
					num6 = num10;
				}
			}
		}
		return AimTarget;
	}

	private bool CanCastSpell()
	{
		if (ArrowAttackDelay > 0f)
		{
			return false;
		}
		return true;
	}

	private void CastCurrentSpell(bool autoAim = true, bool consumeAmmo = true, bool wasSpell = true)
	{
		if (ArrowAttackDelay > 0f)
		{
			return;
		}
		if (!FaithAmmo.UseAmmo(AmmoCost))
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/noarrows", base.gameObject);
			return;
		}
		if (InputManager.General.MouseInputActive)
		{
			float angle = Utils.GetAngle(GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().WorldToScreenPoint(base.transform.position), InputManager.General.GetMousePosition());
			state.facingAngle = (state.LookAngle = (playerController.forceDir = angle));
		}
		aiming = false;
		CastSpell(DataManager.Instance.CurrentCurse, autoAim, consumeAmmo, wasSpell);
	}

	private void CurseBreak()
	{
		Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, health, 1f, 1f);
		Action curseBroken = CurseBroken;
		if (curseBroken != null)
		{
			curseBroken();
		}
		DataManager.Instance.CurrentCurse = EquipmentType.None;
		DataManager.Instance.CurrentCurseLevel = 0;
		SetSpell(DataManager.Instance.CurrentCurse, DataManager.Instance.CurrentCurseLevel);
	}

	public void CastSpell(EquipmentType curseType, bool autoAim = true, bool consumeAmmo = true, bool wasSpell = true, bool smallScale = false, GameObject shooter = null)
	{
		if (smallScale)
		{
			AimTarget = UnitObject.GetClosestTarget(shooter.transform, Health.Team.Team2);
			if (AimTarget != null)
			{
				AimAngle = Utils.GetAngle(base.transform.position, AimTarget.transform.position);
			}
		}
		else if (AimTarget == null || AimTarget.invincible)
		{
			if (autoAim)
			{
				if (!InputManager.General.MouseInputActive && (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > 0.2f || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > 0.2f))
				{
					AimAngle = Utils.GetAngle(base.transform.position, base.transform.position + new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis()));
				}
				else
				{
					AimAngle = state.facingAngle;
				}
			}
			if (InputManager.General.GetLastActiveController().type == ControllerType.Keyboard)
			{
				AimTarget = GetAutoAimTarget(true);
			}
		}
		else
		{
			AimAngle = Utils.GetAngle(base.transform.position, AimTarget.transform.position);
		}
		AudioManager.Instance.PlayOneShot(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PerformActionSound, base.transform.position);
		CameraManager.shakeCamera(0.5f, state.facingAngle);
		switch (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PrimaryEquipmentType)
		{
		case EquipmentType.Tentacles:
			Spell_Tentacle(0f, smallScale, shooter);
			break;
		case EquipmentType.EnemyBlast:
			Spell_EnemyBlast(smallScale, shooter);
			break;
		case EquipmentType.ProjectileAOE:
			Spell_ProjectileAOE(smallScale, shooter);
			break;
		case EquipmentType.MegaSlash:
			Spell_MegaSlash(smallScale, shooter);
			break;
		case EquipmentType.Fireball:
			Spell_Fireball(DataManager.Instance.CurrentCurse, 0f, true, smallScale, shooter);
			break;
		}
		Action onCastSpell = OnCastSpell;
		if (onCastSpell != null)
		{
			onCastSpell();
		}
		if (wasSpell)
		{
			CastEvent onSpellCast = PlayerSpells.OnSpellCast;
			if (onSpellCast != null)
			{
				onSpellCast(curseType);
			}
		}
		bool num = IsShooterAPlayer(shooter);
		if (num || !smallScale)
		{
			ArrowAttackDelay = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Delay;
			state.CURRENT_STATE = StateMachine.State.Casting;
			if ((bool)Spine && Spine.AnimationState != null)
			{
				playerFarming.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Casting, EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PerformActionAnimation);
			}
		}
		if (num)
		{
			ArrowAttackDelay = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Delay;
			castTimer = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).CastingDuration;
		}
		StartCoroutine(FrameDelay(delegate
		{
			AimTarget = null;
		}));
	}

	private bool IsShooterAPlayer(GameObject shooter)
	{
		if (!(shooter == null))
		{
			return shooter == base.gameObject;
		}
		return true;
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private bool IsPotentialAutoAimTarget(Health h, float AutoAimArc)
	{
		if (h.team != health.team && h.HP > 0f && !h.invincible && !h.untouchable && (h.gameObject.layer == LayerMask.NameToLayer("Units") || h.gameObject.layer == LayerMask.NameToLayer("Obstacles Player Ignore") || h.gameObject.layer == LayerMask.NameToLayer("ObstaclesAndPlayer")))
		{
			Vector2 vector = Utils.DegreeToVector2(AimAngle);
			if (Mathf.Abs(Vector3.Angle(to: (Vector2)(base.transform.position - h.transform.position).normalized, from: vector) - 180f) < AutoAimArc)
			{
				return true;
			}
		}
		return false;
	}

	public static float GetCurseDamageMultiplier()
	{
		return PlayerFleeceManager.GetCursesDamageMultiplier() + TrinketManager.GetCurseDamageMultiplerIncrease() + (0.25f * (float)DataManager.Instance.PLAYER_DAMAGE_LEVEL + DataManager.Instance.PLAYER_RUN_DAMAGE_LEVEL) + (float)DataManager.Instance.CurrentCurseLevel * (1f / 6f);
	}

	public void Spell_Fireball(EquipmentType fireballType, float directionOffset = 0f, bool knockback = true, bool smallScale = false, GameObject shooter = null)
	{
		if (fireballType == EquipmentType.Fireball_Swarm)
		{
			StartCoroutine(Spell_Fireball_Swarm(smallScale, shooter));
			return;
		}
		bool flag = fireballType == EquipmentType.Fireball_Triple;
		int num = ((!flag) ? 1 : 2);
		int num2 = (flag ? (-1) : 0);
		if (directionOffset != 0f)
		{
			AimAngle = Mathf.Repeat(AimAngle + directionOffset, 360f);
		}
		List<Health> list = new List<Health>();
		Vector3 vector = ((shooter == null) ? base.transform.position : shooter.transform.position);
		vector.z = 0f;
		bool flag2 = IsShooterAPlayer(shooter);
		for (int i = num2; i < num; i++)
		{
			if (!flag)
			{
				AimAngle = state.LookAngle;
				if (AimTarget == null || InputManager.General.GetLastActiveController().type == ControllerType.Joystick)
				{
					AimTarget = GetAutoAimTarget(true, list);
					if (AimTarget != null)
					{
						AimAngle = Utils.GetAngle(vector, AimTarget.transform.position);
					}
				}
			}
			Projectile component = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(fireballType).Prefab, spellParent).GetComponent<Projectile>();
			float num3 = AimAngle + (float)(i * 15);
			component.transform.position = vector + new Vector3(0.5f * Mathf.Cos(num3 * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(num3 * ((float)Math.PI / 180f)), -0.5f);
			component.Angle = num3;
			component.team = health.team;
			component.Owner = health;
			component.transform.localScale *= (smallScale ? 0.5f : 1f);
			if (EquipmentManager.GetCurseData(fireballType).EquipmentType == EquipmentType.Fireball_Charm)
			{
				AudioManager.Instance.PlayOneShot("event:/player/Curses/charm_curse", component.gameObject);
			}
			if (EquipmentManager.GetCurseData(fireballType).EquipmentType == EquipmentType.Fireball_Charm && UnityEngine.Random.value <= EquipmentManager.GetCurseData(EquipmentType.Fireball_Charm).Chance)
			{
				component.AttackFlags = Health.AttackFlags.Charm;
			}
			component.Damage = EquipmentManager.GetCurseData(fireballType).Damage * GetCurseDamageMultiplier();
			component.Explosive = playerFarming.CorrectProjectileChargeRelease();
			if (smallScale)
			{
				component.Damage *= 0.5f;
			}
			if (playerFarming.CorrectProjectileChargeRelease())
			{
				AudioManager.Instance.PlayOneShot("event:/player/Curses/explosive_shot", base.gameObject);
				Explosion.CreateExplosion(vector + new Vector3(0.5f * Mathf.Cos(num3 * ((float)Math.PI / 180f)), 0.5f * Mathf.Sin(num3 * ((float)Math.PI / 180f))), Health.Team.PlayerTeam, health, 3f, 1f, 1f);
				GameManager.GetInstance().HitStop();
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/player/Curses/fireball", base.gameObject);
			}
			if (!flag)
			{
				component.homeInOnTarget = true;
				component.SetTarget(AimTarget);
			}
			list.Add(AimTarget);
			if (flag2 || !smallScale)
			{
				chargeTimer = 0f;
				playerFarming.HideProjectileChargeBars();
				if (directionOffset == 0f && knockback && !smallScale)
				{
					playerController.unitObject.DoKnockBack((AimAngle + 180f) % 360f * ((float)Math.PI / 180f), 1f, 0.3f);
				}
			}
		}
		if (flag2 || !smallScale)
		{
			playerFarming.UpdateProjectileChargeBar(0f);
		}
	}

	protected virtual IEnumerator Spell_Fireball_Swarm(bool smallScale = false, GameObject shooter = null)
	{
		int num = 5;
		List<float> shootAngles = new List<float>(10);
		for (int j = 0; j < num; j++)
		{
			shootAngles.Add(360f / (float)num * (float)j);
		}
		Vector3 p = ((shooter == null) ? base.transform.position : shooter.transform.position);
		shootAngles.Shuffle();
		float initAngle = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < shootAngles.Count; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/fireball", base.gameObject);
			Projectile component = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(EquipmentType.Fireball_Swarm).Prefab, base.transform.parent).GetComponent<Projectile>();
			component.transform.localScale /= 1.5f;
			component.transform.position = p;
			component.Angle = initAngle + shootAngles[i];
			component.team = health.team;
			component.Speed = UnityEngine.Random.Range(2f, 3f);
			component.turningSpeed = 4f;
			component.angleNoiseFrequency = 0.66f;
			component.angleNoiseAmplitude = 135f;
			component.LifeTime = 10f;
			component.Owner = health;
			component.homeInOnTarget = true;
			component.ScreenShakeMultiplier = 0.1f;
			component.Acceleration = 5f;
			component.Damage = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Damage * GetCurseDamageMultiplier();
			component.transform.localScale *= (smallScale ? 0.5f : 1f);
			component.Damage *= (smallScale ? 0.5f : 1f);
			if (AimTarget == null)
			{
				AimTarget = GetAutoAimTarget();
			}
			component.SetTarget(AimTarget);
			yield return new WaitForSeconds(0.03f);
		}
	}

	public void Spell_Tentacle(float directionOffset = 0f, bool smallScale = false, GameObject shooter = null)
	{
		Tentacle.TotalDamagedEnemies.Clear();
		AimAngle = Mathf.Repeat(AimAngle + directionOffset, 360f);
		float num = 1.5f;
		if (smallScale)
		{
			num *= 0.5f;
		}
		Vector3 vector = ((shooter == null) ? base.transform.position : shooter.transform.position);
		vector.z = 0f;
		Vector3 vector2 = vector + new Vector3(num * Mathf.Cos(AimAngle * ((float)Math.PI / 180f)), num * Mathf.Sin(AimAngle * ((float)Math.PI / 180f)) - 0.3f, 0.5f);
		int num2 = 0;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector2, Utils.DegreeToVector2(state.facingAngle), 30f, collisionMask);
		if (raycastHit2D.collider != null)
		{
			num2 = Mathf.CeilToInt(Mathf.Sqrt(MagnitudeFindDistanceBetween(base.transform.position, raycastHit2D.point)) / 5f);
		}
		if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).EquipmentType != EquipmentType.Tentacles_Ice)
		{
			num2 = 1;
		}
		int num3 = num2 * 5;
		float num4 = 0f;
		Vector3 vector3 = vector;
		float num5 = 7f;
		if (smallScale)
		{
			num5 *= 0.5f;
		}
		for (int i = 0; i < num2; i++)
		{
			vector3 = vector + new Vector3((float)i * num5 * Mathf.Cos(AimAngle * ((float)Math.PI / 180f)), (float)i * num5 * Mathf.Sin(AimAngle * ((float)Math.PI / 180f)), 0f);
			StartCoroutine(SpawnGroundCrack(num4, vector3, Mathf.Repeat(AimAngle, 360f), smallScale));
			num4 += 0.2f;
		}
		AudioManager.Instance.PlayOneShot("event:/player/Curses/tentacles", base.gameObject);
		if (DataManager.Instance.CurrentCurse == EquipmentType.Tentacles_Ice)
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/ice_curse", base.gameObject);
		}
		num5 = 1.25f;
		if (smallScale)
		{
			num5 *= 0.5f;
		}
		int num6 = 0;
		while (num6++ < num3)
		{
			float delay = 0.05f * (float)num6;
			Tentacle component = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Prefab, spellParent, true).GetComponent<Tentacle>();
			component.transform.position = vector2;
			float num7 = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Damage;
			if (smallScale)
			{
				num7 *= 0.5f;
			}
			if (DataManager.Instance.CurrentCurse == EquipmentType.Tentacles_Necromancy)
			{
				ProjectileGhost.SpawnGhost(component.transform.position, delay, num7 + DataManager.GetWeaponDamageMultiplier(DataManager.Instance.CurrentCurseLevel), smallScale ? 0.5f : 1f);
			}
			if (DataManager.Instance.CurrentCurse == EquipmentType.Tentacles_Ice && UnityEngine.Random.value <= EquipmentManager.GetCurseData(EquipmentType.Tentacles_Ice).Chance)
			{
				component.AttackFlags = Health.AttackFlags.Ice;
			}
			component.transform.localScale *= (smallScale ? 0.5f : 1f);
			component.Play(delay, 0.5f, num7 * GetCurseDamageMultiplier(), health.team, false, (int)Mathf.Repeat(num6, 7f), true);
			vector2 = vector + new Vector3(((float)num6 * num5 + num) * Mathf.Cos(AimAngle * ((float)Math.PI / 180f)), ((float)num6 * num5 + num) * Mathf.Sin(AimAngle * ((float)Math.PI / 180f)) - 0.3f, smallScale ? 0f : 0.5f);
		}
		if (directionOffset == 0f)
		{
			if (EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).EquipmentType == EquipmentType.Tentacles_Circular)
			{
				Spell_Tentacle(Mathf.Repeat(90f, 360f), smallScale, shooter);
			}
			else if (IsShooterAPlayer(shooter) && !smallScale)
			{
				playerController.unitObject.DoKnockBack((AimAngle + 180f) % 360f * ((float)Math.PI / 180f), 1f, 0.3f);
			}
		}
		else if (directionOffset != 270f && EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).EquipmentType == EquipmentType.Tentacles_Circular)
		{
			Spell_Tentacle(Mathf.Repeat(directionOffset + 90f, 360f), smallScale, shooter);
		}
	}

	private IEnumerator SpawnGroundCrack(float delay, Vector3 position, float aimAngle, bool smallScale = false)
	{
		yield return new WaitForSeconds(delay);
		GameObject obj = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).SecondaryPrefab, spellParent);
		obj.transform.localScale *= (smallScale ? 0.5f : 1f);
		obj.transform.position = position;
		obj.transform.eulerAngles = new Vector3(0f, 0f, aimAngle);
	}

	public void Spell_EnemyBlast(bool smallScale = false, GameObject shooter = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Prefab, spellParent, true);
		if (DataManager.Instance.CurrentCurse == EquipmentType.EnemyBlast_Ice)
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/ice_curse", base.gameObject);
		}
		AudioManager.Instance.PlayOneShot("event:/player/Curses/blast_push", base.gameObject);
		Vector3 position = ((shooter == null) ? base.transform.position : shooter.transform.position);
		position.z = 0f;
		gameObject.transform.position = position;
		gameObject.transform.localScale *= (smallScale ? 0.5f : 1f);
		if ((bool)gameObject.GetComponent<Vortex>())
		{
			gameObject.GetComponent<Vortex>().LifeTimeMultiplier = 0.5f;
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.EnemyBlast_DeflectsProjectiles)
		{
			float duration = (smallScale ? 1.5f : 3f);
			playerController.MakeUntouchable(duration);
		}
		else
		{
			float duration2 = (smallScale ? 0.33f : 0.66f);
			playerController.MakeInvincible(duration2);
		}
	}

	public void Spell_Vortex()
	{
		UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Prefab, spellParent, true).GetComponent<Vortex>().transform.position = base.transform.position;
	}

	public void Spell_ProjectileAOE(bool smallScale = false, GameObject shooter = null)
	{
		float damage = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Damage;
		Vector3 vector = ((shooter == null) ? base.transform.position : shooter.transform.position);
		vector.z = 0f;
		GoopBomb component = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Prefab, spellParent, true).GetComponent<GoopBomb>();
		component.DamageMultiplier = damage * GetCurseDamageMultiplier();
		component.TickDurationMultiplier = 0.75f;
		component.impactDamage = damage * GetCurseDamageMultiplier();
		component.transform.position = targetPosition + vector;
		component.transform.localScale *= (smallScale ? 0.5f : 1f);
		if (InputManager.General.MouseInputActive)
		{
			component.transform.position = GetProjectileAOETargetWithMouse();
		}
		component.Play(vector, 0.5f);
		component.RotateBomb = true;
		AudioManager.Instance.PlayOneShot("event:/player/Curses/goop_shot", base.gameObject);
		if (DataManager.Instance.CurrentCurse == EquipmentType.ProjectileAOE_Charm)
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/charm_curse", base.gameObject);
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.ProjectileAOE_GoopTrail)
		{
			PoisonTrail poisonTrail = component.BombVisual.gameObject.AddComponent<PoisonTrail>();
			poisonTrail.PoisonPrefab = component.PoisonPrefab;
			poisonTrail.Parent = component.transform.parent;
			poisonTrail.enabled = true;
		}
	}

	public void Spell_MegaSlash(bool smallScale = false, GameObject shooter = null)
	{
		bool flag = playerFarming.CorrectProjectileChargeRelease();
		chargeTimer = (flag ? 1f : chargeTimer);
		Vector3 position = ((shooter == null) ? base.transform.position : shooter.transform.position);
		position.z = 0f;
		MegaSlash component = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Prefab, spellParent, true).GetComponent<MegaSlash>();
		component.transform.position = position;
		component.transform.eulerAngles = new Vector3(0f, 0f, AimAngle);
		bool num = IsShooterAPlayer(shooter);
		if (num)
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "Curses/attack-slash-curse", false);
			PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", false, 0f);
			GameManager.GetInstance().WaitForSeconds(1.5f, delegate
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			});
		}
		float num2 = (num ? Mathf.Clamp(chargeTimer, 0f, megaSlashMaxChargeDuration) : 0f);
		component.Play(num2 / megaSlashMaxChargeDuration);
		if (num)
		{
			chargeTimer = num2;
			playerFarming.UpdateProjectileChargeBar(0f);
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.MegaSlash_Ice)
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/ice_curse", base.gameObject);
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.MegaSlash_Charm)
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/charm_curse", base.gameObject);
		}
		if (flag)
		{
			component = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Prefab, spellParent, true).GetComponent<MegaSlash>();
			component.transform.position = position;
			component.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Repeat(AimAngle + 180f, 360f));
			chargeTimer = Mathf.Clamp(chargeTimer, 0f, megaSlashMaxChargeDuration);
			component.Play(chargeTimer / megaSlashMaxChargeDuration);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/mega_slash", base.gameObject);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/player/Curses/mega_slash_double", base.gameObject);
		}
		if (num)
		{
			chargeTimer = 0f;
		}
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	private IEnumerator DelayCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}
}
