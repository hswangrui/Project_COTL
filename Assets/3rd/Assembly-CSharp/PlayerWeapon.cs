using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using MMBiomeGeneration;
using MMTools;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerWeapon : BaseMonoBehaviour
{
	public delegate void DoSpecialAction();

	public enum AttackSwipeDirections
	{
		Down,
		DownRight,
		Up
	}

	public delegate void WeaponEvent(EquipmentType Weapon, int Level);

	public enum AttackState
	{
		Begin,
		CanBreak,
		Finish
	}

	[Serializable]
	public class EquippedWeaponsInfo
	{
		public EquipmentType WeaponType;

		public WeaponData WeaponData;

		public float WeaponDamageMultiplier { get; set; } = 1f;


		public float CriticalChance { get; set; }

		public float RangeMultiplier { get; set; } = 1f;


		public float AttackRateMultiplier { get; set; } = 1f;


		public float MovementSpeedMultiplier { get; set; } = 1f;


		public float XPDropMultiplier { get; set; } = 1f;


		public float NegateDamageChance { get; set; }

		public float HealChance { get; set; }

		public float HealAmount { get; set; } = 1f;


		public float PoisonChance { get; set; }

		public void ResetMultipliers()
		{
			WeaponDamageMultiplier = 1f;
			RangeMultiplier = 1f;
			AttackRateMultiplier = 1f;
			MovementSpeedMultiplier = 1f;
			XPDropMultiplier = 1f;
			CriticalChance = 0f;
			NegateDamageChance = 0f;
			HealChance = 0f;
			HealAmount = 1f;
			PoisonChance = 0f;
		}
	}

	[Serializable]
	public class WeaponCombos
	{
		public float CameraShake;

		public float Damage;

		public float RangeRadius = 1f;

		public string Animation;

		public GameObject SwipeObject;

		public bool CanQueueNextAttack = true;

		public bool CanChangeDirectionDuringAttack = true;

		public bool CanFreelyChangeDirection;

		public bool ShowDirectionIndicator;

		public float LungeSpeed = 20f;

		public float LungeDuration = 0.15f;

		public float HitKnockback;

		public Health.AttackTypes AttackType;
	}

	[Serializable]
	public class SpecialsData
	{
		public string Name;

		[SpineAnimation("", "skeletonAnimation", true, false)]
		public string NorthAnimation;

		[SpineAnimation("", "skeletonAnimation", true, false)]
		public string HorizontalAnimation;

		[SpineAnimation("", "skeletonAnimation", true, false)]
		public string SouthAnimation;

		public float Target;
	}

	public EquippedWeaponsInfo CurrentWeapon;

	public int CurrentWeaponLevel;

	public SkeletonAnimation skeletonAnimation;

	private PlayerFarming playerFarming;

	private PlayerController playerController;

	private StateMachine state;

	private Health health;

	private float ResetCombo = 0.2f;

	private const float ResetComboDuration = 0.2f;

	private int StealthDamageMultiplierIncrease = 4;

	private PlayerArrows playerArrows;

	private Health.Team? overrideTeam;

	private static float minHeavyAttackChargeBeforeSecondAttack = 0.2f;

	public GameObject HeavyAttackTarget;

	[SerializeField]
	private CriticalTimer criticalTimer;

	[SerializeField]
	private DamageNegationTimer damageNegation;

	[SerializeField]
	private float damageNegationTime = 10f;

	private float damageNegationTimer;

	public static Action WeaponDamaged;

	public static Action WeaponBroken;

	private float criticalHitChargeDuration = 25f;

	private bool _critHitCharged;

	public static bool FirstTimeUsingWeapon = false;

	private Coroutine attackRoutine;

	public static AttackSwipeDirections AttackSwipeDirection;

	private const float GUANTLET_HEAVY_RANGE = 2f;

	private bool invokedDamage;

	public bool ForceWeapons;

	public static WeaponEvent OnWeaponChanged;

	public AttackState CurrentAttackState;

	private int CurrentCombo;

	private bool HoldingForHeavyAttack;

	private bool StealthSneakAttack;

	private bool CanChangeDirection = true;

	private float StoreFacing = -1f;

	private float aimTimer;

	public bool EnableHeavyAttack = true;

	private float MaxHold = 0.3f;

	public bool ChargedNegation;

	public float DaggerAngleOffset = 10f;

	public float DaggerDistance = 0.75f;

	private bool DoHeavyAttack;

	public bool DoingHeavyAttack;

	private bool ShowHeavyAim;

	private float HeavyAimSpeed;

	private EventInstance HeavyAttackSound;

	public PlayerVFXManager playerVFX;

	private EventInstance loopedSound;

	private SpecialsData CurrentSpecial;

	public List<SpecialsData> Specials;

	public static float HeavySwordDamage
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Sword))
			{
				return 2.5f * DataManager.Instance.SpecialAttackDamageMultiplier;
			}
			return 2f * DataManager.Instance.SpecialAttackDamageMultiplier;
		}
	}

	public static float HeavyAxeDamage
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Axe))
			{
				return 2.5f * DataManager.Instance.SpecialAttackDamageMultiplier;
			}
			return 2f * DataManager.Instance.SpecialAttackDamageMultiplier;
		}
	}

	public static float HeavyDaggerDamage
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger))
			{
				return 2f * DataManager.Instance.SpecialAttackDamageMultiplier;
			}
			return 1.5f * DataManager.Instance.SpecialAttackDamageMultiplier;
		}
	}

	public static float HeavyGauntletDamageFirst
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Gauntlets))
			{
				return 5f * DataManager.Instance.SpecialAttackDamageMultiplier;
			}
			return 4f * DataManager.Instance.SpecialAttackDamageMultiplier;
		}
	}

	public static float HeavyGauntletDamageSubsequent
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Gauntlets))
			{
				return 1.5f * DataManager.Instance.SpecialAttackDamageMultiplier;
			}
			return 1f * DataManager.Instance.SpecialAttackDamageMultiplier;
		}
	}

	public static float HeavyHammerDamage
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Sword))
			{
				return 5f * DataManager.Instance.SpecialAttackDamageMultiplier;
			}
			return 4f * DataManager.Instance.SpecialAttackDamageMultiplier;
		}
	}

	public static float CriticalHitTimer { get; set; }

	public bool CriticalHitCharged
	{
		get
		{
			bool flag = CriticalHitTimer >= criticalHitChargeDuration;
			if (!_critHitCharged && flag)
			{
				_critHitCharged = true;
				criticalHitChargeDuration = UnityEngine.Random.Range(20f, 30f);
			}
			return flag;
		}
	}

	private int HeavyAttackFervourCost
	{
		get
		{
			switch (CurrentWeapon.WeaponType)
			{
			case EquipmentType.Axe:
				return 20;
			case EquipmentType.Dagger:
				return 15;
			default:
				return 10;
			}
		}
	}

	public static event DoSpecialAction OnSpecial;

	private void Start()
	{
		health = GetComponent<Health>();
		state = GetComponent<StateMachine>();
		playerFarming = GetComponent<PlayerFarming>();
		playerController = GetComponent<PlayerController>();
		playerArrows = GetComponent<PlayerArrows>();
		criticalTimer.gameObject.SetActive(false);
		damageNegation.gameObject.SetActive(false);
		SetWeapon(DataManager.Instance.CurrentWeapon, DataManager.Instance.CurrentWeaponLevel);
		health.OnHit += OnHit;
		health.OnDamageNegated += OnDamageNegated;
		skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		SetSpecial(0);
		DataManager.Instance.PLAYER_SPECIAL_AMMO = DataManager.Instance.Followers.Count;
		HeavyAttackTarget.SetActive(false);
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		StopAllCoroutines();
		GameManager.SetTimeScale(1f);
		GameManager.GetInstance().CameraResetTargetZoom();
	}

	private void OnDamageNegated()
	{
		Debug.Log("OnDamageNegated".Colour(Color.red));
		damageNegation.transform.DOKill();
		damageNegation.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		health.OnHit -= OnHit;
		health.OnDamageNegated -= OnDamageNegated;
		skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		FirstTimeUsingWeapon = false;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		switch (e.Data.Name)
		{
		case "THROW_DAGGER":
		{
			ShowHeavyAim = false;
			playerFarming.HideHeavyChargeBars();
			CanChangeDirection = false;
			int num = -1;
			int num2 = (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger) ? 6 : 5);
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger))
			{
				AudioManager.Instance.PlayOneShot("event:/weapon/dagger_heavy/dagger_glint", base.gameObject);
			}
			while (++num <= num2)
			{
				float num3 = UnityEngine.Random.Range(0f - DaggerAngleOffset, DaggerAngleOffset);
				ThrownDagger.SpawnThrownDagger(base.transform.position + DaggerDistance * new Vector3((float)num * Mathf.Cos((state.facingAngle + num3) * ((float)Math.PI / 180f)), (float)num * Mathf.Sin((state.facingAngle + num3) * ((float)Math.PI / 180f))), GetDamage(HeavyDaggerDamage, CurrentWeaponLevel), (float)num * 0.1f, UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger) ? null : CurrentWeapon.WeaponData.WorldSprite);
			}
			FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
			playerController.unitObject.DoKnockBack((state.facingAngle + 180f) % 360f * ((float)Math.PI / 180f), 0.4f, 0.3f);
			break;
		}
		case "THROW_AXE":
			ShowHeavyAim = false;
			playerFarming.HideHeavyChargeBars();
			CanChangeDirection = false;
			playerController.unitObject.DoKnockBack((state.facingAngle + 180f) % 360f * ((float)Math.PI / 180f), 0.3f, 0.3f);
			ThrownAxe.SpawnThrowingAxe(base.transform.position, GetDamage(HeavyAxeDamage, CurrentWeaponLevel), UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Axe) ? null : CurrentWeapon.WeaponData.WorldSprite, state.facingAngle);
			FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
			break;
		case "Swipe-Down":
			AttackSwipeDirection = AttackSwipeDirections.Down;
			break;
		case "Swipe-DownRight":
			AttackSwipeDirection = AttackSwipeDirections.DownRight;
			break;
		case "Swipe-Up":
			AttackSwipeDirection = AttackSwipeDirections.Up;
			break;
		case "Attack Can Break":
			CurrentAttackState = AttackState.CanBreak;
			break;
		case "Attack Has Finished":
			CurrentAttackState = AttackState.Finish;
			break;
		case "Hammer Heavy":
			AudioManager.Instance.PlayOneShot(CurrentWeapon.WeaponData.PerformActionSound, base.gameObject);
			PlayerFarming.Instance.HideWeaponChargeBars();
			AttackDealDamage(HeavyHammerDamage, Health.AttackFlags.Penetration);
			break;
		case "Attack Deal Damage":
			AudioManager.Instance.PlayOneShot(CurrentWeapon.WeaponData.PerformActionSound, base.gameObject);
			switch (EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType)
			{
			case EquipmentType.Sword:
				AudioManager.Instance.PlayOneShot("event:/weapon/metal_medium", base.gameObject);
				MMVibrate.Rumble(1f / 6f, 1f / 30f, 0.33f, this);
				break;
			case EquipmentType.Axe:
				AudioManager.Instance.PlayOneShot("event:/weapon/metal_heavy", base.gameObject);
				MMVibrate.Rumble(1f / 6f, 0.05f, 0.5f, this);
				break;
			case EquipmentType.Dagger:
				if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger) && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks))
				{
					AudioManager.Instance.PlayOneShot("event:/weapon/metal_small", base.gameObject);
				}
				MMVibrate.Rumble(1f / 12f, 0.025f, 0.25f, this);
				break;
			case EquipmentType.Hammer:
				PlayerFarming.Instance.HideWeaponChargeBars();
				if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Hammer) && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks))
				{
					AudioManager.Instance.PlayOneShot("event:/weapon/metal_heavy", base.gameObject);
				}
				BiomeConstants.Instance.EmitHammerEffects(base.transform.position, state.facingAngle);
				MMVibrate.Rumble(1f / 6f, 0.05f, 0.5f, this);
				break;
			default:
				AudioManager.Instance.PlayOneShot("event:/weapon/metal_medium", base.gameObject);
				MMVibrate.Rumble(1f / 6f, 1f / 30f, 0.33f, this);
				break;
			}
			AttackDealDamage(CurrentWeapon.WeaponData.Combos[CurrentCombo].Damage);
			break;
		case "invincibility_ON":
			health.invincible = true;
			break;
		case "invincibility_OFF":
			health.invincible = false;
			break;
		case "S1 - Deal Damage":
		{
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
			Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position + new Vector3(1f * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), 1f * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f), 2f);
			for (int i = 0; i < array.Length; i++)
			{
				Health component = array[i].gameObject.GetComponent<Health>();
				if (component != null && component != health && component.team != (overrideTeam ?? health.team))
				{
					component.DealDamage(HeavyGauntletDamageFirst, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.8f));
					CameraManager.shakeCamera(0.6f, Utils.GetAngle(base.transform.position, component.transform.position));
				}
			}
			break;
		}
		case "S2 - Deal Damage 1":
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
			CreateSwipe(0f, 0, HeavyGauntletDamageSubsequent, CurrentWeapon.WeaponData.Combos[CurrentCombo].AttackType, 1f, Health.AttackFlags.Penetration);
			break;
		case "S2 - Deal Damage 2":
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
			CreateSwipe(90 + ((!(state.facingAngle > -90f) || !(state.facingAngle < 90f)) ? 180 : 0), 0, HeavyGauntletDamageSubsequent, CurrentWeapon.WeaponData.Combos[CurrentCombo].AttackType, 2f, Health.AttackFlags.Penetration);
			break;
		case "S2 - Deal Damage 3":
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
			CreateSwipe(180f, 0, HeavyGauntletDamageSubsequent, CurrentWeapon.WeaponData.Combos[CurrentCombo].AttackType, 2f, Health.AttackFlags.Penetration);
			break;
		case "S2 - Deal Damage 4":
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
			CreateSwipe(270 + ((!(state.facingAngle > -90f) || !(state.facingAngle < 90f)) ? 180 : 0), 0, HeavyGauntletDamageSubsequent, CurrentWeapon.WeaponData.Combos[CurrentCombo].AttackType, 2f, Health.AttackFlags.Penetration);
			break;
		case "S2 - Deal Damage 5":
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
			CreateSwipe(state.facingAngle, 0, HeavyGauntletDamageSubsequent, CurrentWeapon.WeaponData.Combos[CurrentCombo].AttackType, 2f, Health.AttackFlags.Penetration);
			break;
		case "attack-charge1-hit":
		{
			AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
			EquipmentType primaryEquipmentType = EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType;
			CreateSwipe(state.facingAngle, 0, HeavySwordDamage, Health.AttackTypes.Heavy, (primaryEquipmentType == EquipmentType.Sword) ? 2.5f : 2f, Health.AttackFlags.Penetration);
			PlayerFarming.Instance.HideWeaponChargeBars();
			if (primaryEquipmentType == EquipmentType.Hammer)
			{
				if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Hammer) && !UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks))
				{
					AudioManager.Instance.PlayOneShot("event:/weapon/metal_heavy", base.gameObject);
				}
				else
				{
					AudioManager.Instance.PlayOneShot("event:/weapon/metal_heavy", base.gameObject);
				}
			}
			BiomeConstants.Instance.EmitHammerEffects(base.transform.position, state.facingAngle);
			MMVibrate.Rumble(1f / 6f, 0.05f, 0.5f, this);
			CameraManager.shakeCamera(0.4f);
			if (DoingHeavyAttack)
			{
				FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
			}
			DoingHeavyAttack = false;
			CanChangeDirection = false;
			break;
		}
		case "Update Angle":
			if (CurrentWeapon.WeaponData.Combos[CurrentCombo].CanChangeDirectionDuringAttack)
			{
				state.facingAngle = (playerController.forceDir = StoreFacing);
			}
			CanChangeDirection = false;
			break;
		}
	}

	private void AttackDealDamage(float DamageToDeal, Health.AttackFlags additionalFlags = (Health.AttackFlags)0)
	{
		AudioManager.Instance.PlayOneShot("event:/player/attack", base.gameObject);
		CameraManager.shakeCamera(CurrentWeapon.WeaponData.Combos[CurrentCombo].CameraShake, state.facingAngle);
		Swipe component = UnityEngine.Object.Instantiate(CurrentWeapon.WeaponData.Combos[CurrentCombo].SwipeObject, base.transform, true).GetComponent<Swipe>();
		Vector3 position = base.transform.position + new Vector3(CurrentWeapon.WeaponData.Combos[CurrentCombo].RangeRadius * CurrentWeapon.RangeMultiplier * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), CurrentWeapon.WeaponData.Combos[CurrentCombo].RangeRadius * CurrentWeapon.RangeMultiplier * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f);
		Health.AttackFlags attackFlags = (Health.AttackFlags)0;
		attackFlags |= additionalFlags;
		if (TrinketManager.HasTrinket(TarotCards.Card.Skull))
		{
			attackFlags |= Health.AttackFlags.Skull;
		}
		if (TrinketManager.HasTrinket(TarotCards.Card.Spider))
		{
			attackFlags |= Health.AttackFlags.Poison;
		}
		component.Init(Damage: GetDamage(DamageToDeal, DataManager.Instance.CurrentWeaponLevel), Position: position, Angle: state.facingAngle, team: overrideTeam ?? health.team, Origin: health, CallBack: HitEnemyCallBack, Radius: CurrentWeapon.WeaponData.Combos[CurrentCombo].RangeRadius * CurrentWeapon.RangeMultiplier, CritChance: GetCritChance(), AttackType: CurrentWeapon.WeaponData.Combos[CurrentCombo].AttackType, AttackFlags: attackFlags);
	}

	public void DoSlowMo(bool setZoom = true)
	{
		StartCoroutine(SlowMo(setZoom));
	}

	private IEnumerator SlowMo(bool setZoom = true)
	{
		if (setZoom)
		{
			GameManager.GetInstance().CameraSetZoom(6f);
		}
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

	private void CreateSwipe(float Angle, int Combo, float BaseDamage, Health.AttackTypes AttackType, float Radius, Health.AttackFlags AttackFlags)
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(string.Format("Assets/Prefabs/Enemies/Weapons/PlayerSwipe{0}.prefab", Combo), base.transform.position + new Vector3(Radius * 0.5f * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Radius * 0.5f * Mathf.Sin(Angle * ((float)Math.PI / 180f)), -0.5f), Quaternion.identity, base.transform.parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			Swipe component = obj.Result.GetComponent<Swipe>();
			Vector3 position = base.transform.position + new Vector3(Radius * 0.5f * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Radius * 0.5f * Mathf.Sin(Angle * ((float)Math.PI / 180f)), -0.5f);
			component.Init(position, Angle, overrideTeam ?? health.team, health, HitEnemyCallBack, Radius, GetDamage(BaseDamage, DataManager.Instance.CurrentWeaponLevel), GetCritChance(), AttackType, AttackFlags);
		};
	}

	public static float GetDamage(float BaseDamage, int WeaponLevel)
	{
		float num = 1f;
		num += TrinketManager.GetWeaponDamageMultiplerIncrease();
		num += 0.2f * (float)DataManager.Instance.PLAYER_DAMAGE_LEVEL + DataManager.Instance.PLAYER_RUN_DAMAGE_LEVEL;
		num += PlayerFleeceManager.GetWeaponDamageMultiplier();
		num += DataManager.GetWeaponDamageMultiplier(WeaponLevel);
		num += ((DataManager.Instance.PlayerScaleModifier > 1) ? 0.2f : 0f);
		return BaseDamage * num * DifficultyManager.GetPlayerDamageMultiplier();
	}

	public Sprite GetCurrentIcon(EquipmentType weaponType)
	{
		return EquipmentManager.GetWeaponData(weaponType).UISprite;
	}

	public Sprite GetSprite(EquipmentType weaponType)
	{
		return EquipmentManager.GetWeaponData(weaponType).WorldSprite;
	}

	private float GetCritChance()
	{
		return 0f + TrinketManager.GetWeaponCritChanceIncrease();
	}

	private void HitEnemyCallBack(Health h, Health.AttackTypes AttackType)
	{
		if (h.team == Health.Team.Team2)
		{
			playerController.forceDir = (state.facingAngle = Utils.GetAngle(base.transform.position, h.transform.position));
			if (h.HP <= 0f)
			{
				if (h.SlowMoOnkill)
				{
					StartCoroutine(SlowMo());
				}
				if (DataManager.Instance.PLAYER_ARROW_AMMO <= 0)
				{
					playerArrows.RestockAllArrows();
				}
				CameraManager.shakeCamera(0.6f, Utils.GetAngle(base.transform.position, h.gameObject.transform.position));
			}
		}
		switch (EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType)
		{
		case EquipmentType.Sword:
			MMVibrate.Rumble(0.5f, 0.1f, 0.33f, this);
			break;
		case EquipmentType.Axe:
			MMVibrate.Rumble(0.5f, 0.15f, 0.5f, this);
			break;
		case EquipmentType.Dagger:
			MMVibrate.Rumble(0.25f, 0.075f, 0.25f, this);
			break;
		default:
			MMVibrate.Rumble(0.5f, 0.1f, 0.33f, this);
			break;
		}
		if (h.HasShield && AttackType != Health.AttackTypes.Projectile)
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, h.transform.position);
			playerController.CancelLunge(0f);
			playerController.unitObject.knockBackVX = (h.WasJustParried ? (-1f) : (-0.75f)) * Mathf.Cos(Utils.GetAngle(base.transform.position, h.transform.position) * ((float)Math.PI / 180f));
			playerController.unitObject.knockBackVY = (h.WasJustParried ? (-1f) : (-0.75f)) * Mathf.Sin(Utils.GetAngle(base.transform.position, h.transform.position) * ((float)Math.PI / 180f));
		}
		if (h.DontCombo)
		{
			playerController.CancelLunge((h.team == Health.Team.Team2) ? CurrentWeapon.WeaponData.Combos[CurrentCombo].HitKnockback : 0f);
		}
		if (h.OnHitBlockAttacker || h.WasJustParried)
		{
			if (h.AttackerToBlock == null)
			{
				Debug.LogWarning(string.Concat("Player tried to block an attack from ", h.gameObject, "but AttackerToBlock was null. Have you set the AttackerToBlock on the Health prefab in the Inspector?"), h.gameObject);
			}
			else
			{
				playerController.BlockAttacker(h.AttackerToBlock);
			}
		}
	}

	private void Update()
	{
		if ((!LocationManager.LocationIsDungeon(PlayerFarming.Location) && !ForceWeapons) || !DataManager.Instance.EnabledSword || DataManager.Instance.CurrentWeapon == EquipmentType.None || ThrownAxe.Instance != null)
		{
			return;
		}
		if ((state.CURRENT_STATE != StateMachine.State.Attacking || ((DoHeavyAttack || CurrentWeapon.WeaponData.PrimaryEquipmentType != EquipmentType.Hammer) && (!DoHeavyAttack || CurrentWeapon.WeaponData.PrimaryEquipmentType != 0)) || !(Time.timeScale > 0f) || LetterBox.IsPlaying || (MMConversation.CURRENT_CONVERSATION != null && !MMConversation.isBark)) && state.CURRENT_STATE != StateMachine.State.ChargingHeavyAttack)
		{
			AudioManager.Instance.StopLoop(HeavyAttackSound);
			PlayerFarming.Instance.HideWeaponChargeBars();
			PlayerFarming.Instance.HideHeavyChargeBars();
		}
		if (state.CURRENT_STATE != StateMachine.State.Attacking && ShowHeavyAim)
		{
			ShowHeavyAim = false;
			playerFarming.HideHeavyChargeBars();
			AudioManager.Instance.StopLoop(HeavyAttackSound);
		}
		if (Time.timeScale <= 0f || playerFarming.GoToAndStopping)
		{
			return;
		}
		if (state.CURRENT_STATE != StateMachine.State.Attacking)
		{
			if (CurrentCombo > 0 && (ResetCombo -= Time.deltaTime) < 0f)
			{
				ResetCombo = 0.2f;
				CurrentCombo = 0;
			}
		}
		else
		{
			ResetCombo = 0.2f;
		}
		if (Interactor.CurrentInteraction != null && Interactor.CurrentInteraction.HasSecondaryInteraction && Interactor.CurrentInteraction.SecondaryAction == 2)
		{
			return;
		}
		if (state.CURRENT_STATE == StateMachine.State.Dodging)
		{
			AudioManager.Instance.StopLoop(HeavyAttackSound);
		}
		StateMachine.State cURRENT_STATE = state.CURRENT_STATE;
		if ((uint)cURRENT_STATE <= 1u || cURRENT_STATE == StateMachine.State.Dodging || cURRENT_STATE == StateMachine.State.Stealth)
		{
			if (InputManager.Gameplay.GetAttackButtonDown())
			{
				if (attackRoutine != null)
				{
					StopCoroutine(attackRoutine);
				}
				if (CurrentWeapon.WeaponType == EquipmentType.Hammer)
				{
					attackRoutine = StartCoroutine(DoAttackRoutineButtonUp());
				}
				else
				{
					attackRoutine = StartCoroutine(DoAttackRoutine());
				}
			}
			if (InputManager.Gameplay.GetHeavyAttackButtonDown() && !DoingHeavyAttack && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) && !DataManager.Instance.SpecialAttacksDisabled && !LetterBox.IsPlaying)
			{
				if (FaithAmmo.CanAfford(HeavyAttackFervourCost))
				{
					if (attackRoutine != null)
					{
						StopCoroutine(attackRoutine);
					}
					attackRoutine = StartCoroutine(DoAttackRoutine(true));
				}
				else
				{
					FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
				}
			}
		}
		if (InputManager.Gameplay.GetHeavyAttackButtonDown() && !DoingHeavyAttack && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) && !DataManager.Instance.SpecialAttacksDisabled && !LetterBox.IsPlaying)
		{
			if (FaithAmmo.CanAfford(HeavyAttackFervourCost))
			{
				if (attackRoutine != null)
				{
					StopCoroutine(attackRoutine);
				}
				attackRoutine = StartCoroutine(DoAttackRoutine(true));
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/player/Curses/noarrows", base.gameObject);
				FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
			}
		}
		if (criticalTimer.gameObject.activeSelf)
		{
			criticalTimer.UpdateCharging(CriticalHitTimer / criticalHitChargeDuration);
			CriticalHitTimer += Time.deltaTime;
		}
		if (!(damageNegationTimer > 0f))
		{
			return;
		}
		damageNegationTimer -= Time.deltaTime;
		damageNegation.UpdateCharging(damageNegationTimer / damageNegationTime);
		if (damageNegationTimer <= 0f)
		{
			ChargedNegation = false;
			damageNegation.transform.DOKill();
			damageNegation.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(delegate
			{
				damageNegation.gameObject.SetActive(false);
			});
		}
	}

	public void DoAttack()
	{
		if (attackRoutine != null)
		{
			StopCoroutine(attackRoutine);
		}
		if (CurrentWeapon.WeaponType == EquipmentType.Hammer)
		{
			attackRoutine = StartCoroutine(DoAttackRoutineButtonUp());
		}
		else
		{
			attackRoutine = StartCoroutine(DoAttackRoutine());
		}
	}

	public void ManualHit(Health.Team? overrideTeam = null)
	{
		this.overrideTeam = overrideTeam;
		if (attackRoutine != null)
		{
			StopCoroutine(attackRoutine);
		}
		if (CurrentWeapon.WeaponType == EquipmentType.Hammer)
		{
			attackRoutine = StartCoroutine(DoAttackRoutineButtonUp());
		}
		else
		{
			attackRoutine = StartCoroutine(DoAttackRoutine());
		}
	}

	public void SetWeapon(EquipmentType weaponType, int WeaponLevel)
	{
		AudioManager.Instance.ToggleFilter("heavy_attack_lvl_2", false);
		if (weaponType == EquipmentType.Axe && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Axe))
		{
			AudioManager.Instance.ToggleFilter("heavy_attack_lvl_2", true);
		}
		if (weaponType == EquipmentType.Sword && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Sword))
		{
			AudioManager.Instance.ToggleFilter("heavy_attack_lvl_2", true);
		}
		if (weaponType == EquipmentType.Gauntlet && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Gauntlets))
		{
			AudioManager.Instance.ToggleFilter("heavy_attack_lvl_2", true);
		}
		if (weaponType == EquipmentType.Dagger && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger))
		{
			AudioManager.Instance.ToggleFilter("heavy_attack_lvl_2", true);
		}
		if (weaponType == EquipmentType.Hammer && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Hammer))
		{
			AudioManager.Instance.ToggleFilter("heavy_attack_lvl_2", true);
		}
		CurrentWeapon = new EquippedWeaponsInfo();
		CurrentWeapon.WeaponType = weaponType;
		CurrentWeapon.WeaponData = EquipmentManager.GetWeaponData(weaponType);
		if (criticalTimer != null)
		{
			criticalTimer.gameObject.SetActive(CurrentWeapon.WeaponData.GetAttachment(AttachmentEffect.Critical));
		}
		if (DataManager.Instance.CurrentWeapon != weaponType)
		{
			CriticalHitTimer = 0f;
		}
		ChargedNegation = false;
		if (damageNegation != null)
		{
			damageNegation.gameObject.SetActive(false);
		}
		DataManager.Instance.CurrentWeapon = weaponType;
		DataManager.Instance.CurrentWeaponLevel = WeaponLevel;
		DoAttachmentEffect(AttachmentState.Constant);
		CurrentWeaponLevel = WeaponLevel;
		CurrentCombo = 0;
		WeaponEvent onWeaponChanged = OnWeaponChanged;
		if (onWeaponChanged != null)
		{
			onWeaponChanged(DataManager.Instance.CurrentWeapon, DataManager.Instance.CurrentWeaponLevel);
		}
		PlayerFarming.Instance.SetSkin();
	}

	public EquippedWeaponsInfo GetCurrentWeapon()
	{
		return CurrentWeapon;
	}

	public void StopAttackRoutine()
	{
		if (attackRoutine != null)
		{
			StopCoroutine(attackRoutine);
		}
	}

	private IEnumerator DoAttackRoutineButtonUp(bool ForceHeavyAttack = false)
	{
		DoHeavyAttack = ForceHeavyAttack;
		if (!ForceHeavyAttack && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) && !DataManager.Instance.SpecialAttacksDisabled)
		{
			if (FaithAmmo.CanAfford(HeavyAttackFervourCost))
			{
				float HoldTimer = 0f;
				while (InputManager.Gameplay.GetHeavyAttackButtonHeld() && HoldTimer < MaxHold)
				{
					HoldTimer += Time.deltaTime;
					if (HoldTimer >= MaxHold)
					{
						DoHeavyAttack = true;
					}
					else
					{
						yield return null;
					}
				}
			}
			else
			{
				FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
			}
		}
		DoingHeavyAttack = DoHeavyAttack;
		playerVFX.stopEmitChargingParticles();
		CanChangeDirection = true;
		if (CurrentWeapon.WeaponData.Combos[CurrentCombo].CanChangeDirectionDuringAttack)
		{
			StoreFacing = (state.facingAngle = playerController.forceDir);
		}
		CurrentAttackState = AttackState.Begin;
		aimTimer = 0f;
		if ((!DoHeavyAttack && CurrentWeapon.WeaponData.Combos[CurrentCombo].ShowDirectionIndicator) || (DoHeavyAttack && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType == EquipmentType.Sword))
		{
			bool flag = EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType == EquipmentType.Sword;
			playerFarming.ShowWeaponChargeBars(flag ? 2.5f : 1.5f);
		}
		HoldingForHeavyAttack = true;
		if (DoHeavyAttack)
		{
			switch (EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType)
			{
			case EquipmentType.Sword:
				if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Sword))
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-sword2", false).MixDuration = 0f;
					HeavyAttackSound = AudioManager.Instance.CreateLoop("event:/weapon/sword_heavy/sword_heavy_lvl2", true);
				}
				else
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-sword", false).MixDuration = 0f;
					HeavyAttackSound = AudioManager.Instance.CreateLoop("event:/weapon/sword_heavy/sword_heavy_lvl1", true);
				}
				break;
			case EquipmentType.Axe:
				DoHeavyAttack = false;
				StopAllCoroutines();
				StartCoroutine(DoAxeHeavyAttack());
				yield break;
			case EquipmentType.Dagger:
				if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger))
				{
					HeavyAttackSound = AudioManager.Instance.CreateLoop("event:/weapon/dagger_heavy/dagger_heavy_lvl2", true);
				}
				else
				{
					HeavyAttackSound = AudioManager.Instance.CreateLoop("event:/weapon/dagger_heavy/dagger_heavy_lvl1", true);
				}
				HeavyAimSpeed = 0.1f;
				skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-dagger", false).MixDuration = 0f;
				ShowHeavyAim = true;
				playerFarming.ShowHeavyAttackProjectileChargeBars();
				break;
			case EquipmentType.Hammer:
				DoHeavyAttack = false;
				StopAllCoroutines();
				StartCoroutine(DoHammerHeavyAttack());
				yield break;
			case EquipmentType.Gauntlet:
				CanChangeDirection = false;
				FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
				if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Gauntlets))
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-gauntlet2", false).MixDuration = 0f;
					AudioManager.Instance.PlayOneShot("event:/weapon/gauntlet_heavy/gauntlet_lvl2", base.gameObject);
				}
				else
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-gauntlet", false).MixDuration = 0f;
				}
				break;
			default:
				DoHeavyAttack = false;
				break;
			}
		}
		if (!DoHeavyAttack)
		{
			skeletonAnimation.AnimationState.SetAnimation(0, CurrentWeapon.WeaponData.Combos[CurrentCombo].Animation, false).MixDuration = 0f;
		}
		StealthSneakAttack = state.CURRENT_STATE == StateMachine.State.Stealth;
		float num = ((state.CURRENT_STATE == StateMachine.State.Dodging) ? 1.25f : 1f);
		if (state.CURRENT_STATE == StateMachine.State.Dodging)
		{
			playerController.speed = playerController.DodgeSpeed * 1.2f;
		}
		if (!DoHeavyAttack)
		{
			playerController.Lunge(CurrentWeapon.WeaponData.Combos[CurrentCombo].LungeDuration * num, CurrentWeapon.WeaponData.Combos[CurrentCombo].LungeSpeed * num);
		}
		state.CURRENT_STATE = StateMachine.State.Attacking;
		if (DataManager.Instance.SpawnPoisonOnAttack)
		{
			TrapPoison.CreatePoison(base.transform.position, 2, 0.2f, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
		}
		if (TrinketManager.HasTrinket(TarotCards.Card.HandsOfRage) && !TrinketManager.IsOnCooldown(TarotCards.Card.HandsOfRage))
		{
			playerFarming.playerSpells.AimAngle = state.LookAngle;
			playerFarming.playerSpells.Spell_Fireball(EquipmentType.Fireball, 0f, false);
			TrinketManager.TriggerCooldown(TarotCards.Card.HandsOfRage);
		}
		bool QueueAttack = false;
		float QueueAttackTimer = 0f;
		float QueueHeavyAtackTimer = 0f;
		DoAttachmentEffect(AttachmentState.OnAttackStart);
		float attackRateMultiplier = CurrentWeapon.AttackRateMultiplier;
		attackRateMultiplier += TrinketManager.GetAttackRateMultiplier();
		skeletonAnimation.timeScale = attackRateMultiplier;
		if (CurrentWeapon.WeaponData.Combos[CurrentCombo].ShowDirectionIndicator)
		{
			AudioManager.Instance.PlayOneShot("event:/weapon/melee_charge", base.gameObject);
		}
		while (CurrentAttackState == AttackState.Begin)
		{
			if (!InputManager.Gameplay.GetHeavyAttackButtonHeld())
			{
				HoldingForHeavyAttack = false;
			}
			if (CanChangeDirection && CurrentWeapon.WeaponData.Combos[CurrentCombo].CanChangeDirectionDuringAttack && (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement))
			{
				StoreFacing = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis()));
			}
			if (CanChangeDirection && (CurrentWeapon.WeaponData.Combos[CurrentCombo].CanFreelyChangeDirection || DoHeavyAttack) && (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement))
			{
				state.facingAngle = (playerController.forceDir = (StoreFacing = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis()))));
			}
			if (CanChangeDirection && InputManager.General.MouseInputActive && state.CURRENT_STATE == StateMachine.State.Attacking)
			{
				float angle = Utils.GetAngle(GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().WorldToScreenPoint(base.transform.position), InputManager.General.GetMousePosition());
				state.facingAngle = (playerController.forceDir = (StoreFacing = angle));
			}
			if ((CurrentWeapon.WeaponData.Combos[CurrentCombo].ShowDirectionIndicator || (DoHeavyAttack && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType == EquipmentType.Sword)) && Time.timeScale > 0f)
			{
				aimTimer += Time.deltaTime;
				float facingAngle = state.facingAngle;
				playerFarming.SetWeaponAimingRecticuleScaleAndRotation(0, Vector3.one, new Vector3(0f, 0f, facingAngle));
			}
			if (ShowHeavyAim && Time.timeScale > 0f)
			{
				aimTimer += Time.deltaTime;
				playerFarming.SetHeavyAimingRecticuleScaleAndRotation(0, new Vector3(Mathf.SmoothStep(0f, 1f, aimTimer / HeavyAimSpeed), 1f, 1f), new Vector3(0f, 0f, state.facingAngle));
			}
			float num2;
			QueueAttackTimer = (num2 = QueueAttackTimer + Time.deltaTime);
			if (num2 > 0.1f && CurrentCombo < CurrentWeapon.WeaponData.Combos.Count && CurrentWeapon.WeaponData.Combos[CurrentCombo].CanQueueNextAttack && InputManager.Gameplay.GetAttackButtonDown())
			{
				QueueAttack = true;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) && !DataManager.Instance.SpecialAttacksDisabled)
			{
				if (!DoingHeavyAttack && InputManager.Gameplay.GetHeavyAttackButtonHeld())
				{
					QueueHeavyAtackTimer += Time.deltaTime;
					if (QueueHeavyAtackTimer > 0.3f)
					{
						QueueAttack = true;
					}
				}
				else
				{
					QueueHeavyAtackTimer = 0f;
				}
			}
			yield return null;
		}
		overrideTeam = null;
		CurrentCombo = (int)Mathf.Repeat(CurrentCombo + 1, CurrentWeapon.WeaponData.Combos.Count);
		while (CurrentAttackState == AttackState.CanBreak)
		{
			if (!InputManager.Gameplay.GetHeavyAttackButtonHeld())
			{
				AudioManager.Instance.StopLoop(HeavyAttackSound);
				HoldingForHeavyAttack = false;
			}
			skeletonAnimation.timeScale = 1f;
			if (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement)
			{
				if (state.CURRENT_STATE == StateMachine.State.Attacking)
				{
					if (!DoHeavyAttack || EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType != EquipmentType.Axe)
					{
						Action onCrownReturnSubtle = PlayerFarming.OnCrownReturnSubtle;
						if (onCrownReturnSubtle != null)
						{
							onCrownReturnSubtle();
						}
					}
					state.CURRENT_STATE = StateMachine.State.Moving;
				}
				DoAttachmentEffect(AttachmentState.OnAttackEnd);
				yield break;
			}
			if ((QueueAttack || InputManager.Gameplay.GetAttackButtonDown()) && (LocationManager.LocationIsDungeon(PlayerFarming.Location) || ForceWeapons) && ThrownAxe.Instance == null)
			{
				StopAllCoroutines();
				GameManager.SetTimeScale(1f);
				GameManager.GetInstance().CameraResetTargetZoom();
				DoAttachmentEffect(AttachmentState.OnAttackEnd);
				if (attackRoutine != null)
				{
					StopCoroutine(attackRoutine);
				}
				if (CurrentWeapon.WeaponType == EquipmentType.Hammer)
				{
					attackRoutine = StartCoroutine(DoAttackRoutineButtonUp());
				}
				else
				{
					attackRoutine = StartCoroutine(DoAttackRoutine());
				}
				yield break;
			}
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		DoingHeavyAttack = false;
	}

	private IEnumerator DoAttackRoutine(bool ForceHeavyAttack = false, bool followingHeavyAttack = false)
	{
		DoHeavyAttack = ForceHeavyAttack;
		DoingHeavyAttack = DoHeavyAttack;
		playerVFX.stopEmitChargingParticles();
		CanChangeDirection = true;
		if (CurrentWeapon.WeaponData.Combos[CurrentCombo].CanChangeDirectionDuringAttack)
		{
			StoreFacing = (state.facingAngle = playerController.forceDir);
		}
		CurrentAttackState = AttackState.Begin;
		aimTimer = 0f;
		if ((!DoHeavyAttack && CurrentWeapon.WeaponData.Combos[CurrentCombo].ShowDirectionIndicator) || (DoHeavyAttack && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType == EquipmentType.Sword))
		{
			bool flag = EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType == EquipmentType.Sword;
			playerFarming.ShowWeaponChargeBars(flag ? 2.5f : 1.5f);
		}
		HoldingForHeavyAttack = true;
		if (DoHeavyAttack)
		{
			switch (EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType)
			{
			case EquipmentType.Sword:
				if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Sword))
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-sword2", false).MixDuration = 0f;
					HeavyAttackSound = AudioManager.Instance.CreateLoop("event:/weapon/sword_heavy/sword_heavy_lvl2", true);
				}
				else
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-sword", false).MixDuration = 0f;
					HeavyAttackSound = AudioManager.Instance.CreateLoop("event:/weapon/sword_heavy/sword_heavy_lvl1", true);
				}
				break;
			case EquipmentType.Axe:
				DoHeavyAttack = false;
				StopAllCoroutines();
				StartCoroutine(DoAxeHeavyAttack());
				yield break;
			case EquipmentType.Dagger:
				DoHeavyAttack = false;
				StopAllCoroutines();
				StartCoroutine(DoDaggerHeavyAttack());
				yield break;
			case EquipmentType.Hammer:
				DoHeavyAttack = false;
				StopAllCoroutines();
				StartCoroutine(DoHammerHeavyAttack());
				yield break;
			case EquipmentType.Gauntlet:
				CanChangeDirection = false;
				AudioManager.Instance.PlayOneShot("event:/weapon/gauntlet_heavy/gauntlet_scream", base.gameObject);
				FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
				if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Gauntlets))
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-gauntlet2", false).MixDuration = 0f;
					AudioManager.Instance.PlayOneShot("event:/weapon/gauntlet_heavy/gauntlet_lvl2", base.gameObject);
				}
				else
				{
					skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-gauntlet", false).MixDuration = 0f;
				}
				break;
			default:
				DoHeavyAttack = false;
				break;
			}
		}
		if (!DoHeavyAttack)
		{
			skeletonAnimation.AnimationState.SetAnimation(0, CurrentWeapon.WeaponData.Combos[CurrentCombo].Animation, false).MixDuration = 0f;
		}
		StealthSneakAttack = state.CURRENT_STATE == StateMachine.State.Stealth;
		float num = ((state.CURRENT_STATE == StateMachine.State.Dodging) ? 1.25f : 1f);
		if (state.CURRENT_STATE == StateMachine.State.Dodging)
		{
			playerController.speed = playerController.DodgeSpeed * 1.2f;
		}
		if (!DoHeavyAttack)
		{
			playerController.Lunge(CurrentWeapon.WeaponData.Combos[CurrentCombo].LungeDuration * num, CurrentWeapon.WeaponData.Combos[CurrentCombo].LungeSpeed * num);
		}
		state.CURRENT_STATE = StateMachine.State.Attacking;
		if (DataManager.Instance.SpawnPoisonOnAttack)
		{
			TrapPoison.CreatePoison(base.transform.position, 2, 0.2f, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
		}
		if (TrinketManager.HasTrinket(TarotCards.Card.HandsOfRage) && !TrinketManager.IsOnCooldown(TarotCards.Card.HandsOfRage))
		{
			playerFarming.playerSpells.AimAngle = state.LookAngle;
			playerFarming.playerSpells.Spell_Fireball(EquipmentType.Fireball, 0f, false);
			TrinketManager.TriggerCooldown(TarotCards.Card.HandsOfRage);
		}
		bool QueueAttack = false;
		float QueueAttackTimer = 0f;
		bool DoForceHeavyAttack = false;
		float QueueHeavyAtackTimer = 0f;
		DoAttachmentEffect(AttachmentState.OnAttackStart);
		float attackRateMultiplier = CurrentWeapon.AttackRateMultiplier;
		attackRateMultiplier += TrinketManager.GetAttackRateMultiplier();
		skeletonAnimation.timeScale = attackRateMultiplier;
		if (CurrentWeapon.WeaponData.Combos[CurrentCombo].ShowDirectionIndicator)
		{
			AudioManager.Instance.PlayOneShot("event:/weapon/melee_charge", base.gameObject);
		}
		while (CurrentAttackState == AttackState.Begin)
		{
			if (!InputManager.Gameplay.GetHeavyAttackButtonHeld())
			{
				HoldingForHeavyAttack = false;
			}
			if (CanChangeDirection && CurrentWeapon.WeaponData.Combos[CurrentCombo].CanChangeDirectionDuringAttack && (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement))
			{
				StoreFacing = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis()));
			}
			if (CanChangeDirection && (CurrentWeapon.WeaponData.Combos[CurrentCombo].CanFreelyChangeDirection || DoHeavyAttack) && (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement))
			{
				state.facingAngle = (playerController.forceDir = (StoreFacing = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis()))));
			}
			if (CanChangeDirection && InputManager.General.MouseInputActive && state.CURRENT_STATE == StateMachine.State.Attacking)
			{
				float angle = Utils.GetAngle(GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().WorldToScreenPoint(base.transform.position), InputManager.General.GetMousePosition());
				state.facingAngle = (playerController.forceDir = (StoreFacing = angle));
			}
			if ((CurrentWeapon.WeaponData.Combos[CurrentCombo].ShowDirectionIndicator || (DoHeavyAttack && EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType == EquipmentType.Sword)) && Time.timeScale > 0f)
			{
				aimTimer += Time.deltaTime;
				float facingAngle = state.facingAngle;
				playerFarming.SetWeaponAimingRecticuleScaleAndRotation(0, Vector3.one, new Vector3(0f, 0f, facingAngle));
			}
			if (ShowHeavyAim && Time.timeScale > 0f)
			{
				aimTimer += Time.deltaTime;
				playerFarming.SetHeavyAimingRecticuleScaleAndRotation(0, new Vector3(Mathf.SmoothStep(0f, 1f, aimTimer / HeavyAimSpeed), 1f, 1f), new Vector3(0f, 0f, state.facingAngle));
			}
			float num2;
			QueueAttackTimer = (num2 = QueueAttackTimer + Time.deltaTime);
			if (num2 > 0.1f && CurrentCombo < CurrentWeapon.WeaponData.Combos.Count && CurrentWeapon.WeaponData.Combos[CurrentCombo].CanQueueNextAttack && InputManager.Gameplay.GetAttackButtonDown())
			{
				QueueAttack = true;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) && !DataManager.Instance.SpecialAttacksDisabled)
			{
				if (!DoingHeavyAttack && InputManager.Gameplay.GetHeavyAttackButtonHeld())
				{
					QueueHeavyAtackTimer += Time.deltaTime;
					if (QueueHeavyAtackTimer > 1f)
					{
						QueueAttack = true;
						DoForceHeavyAttack = true;
					}
				}
				else
				{
					QueueHeavyAtackTimer = 0f;
				}
			}
			yield return null;
		}
		overrideTeam = null;
		CurrentCombo = (int)Mathf.Repeat(CurrentCombo + 1, CurrentWeapon.WeaponData.Combos.Count);
		while (CurrentAttackState == AttackState.CanBreak)
		{
			skeletonAnimation.timeScale = 1f;
			if (MoveFromCanBreakState())
			{
				yield break;
			}
			if ((QueueAttack || InputManager.Gameplay.GetAttackButtonDown()) && (LocationManager.LocationIsDungeon(PlayerFarming.Location) || ForceWeapons) && ThrownAxe.Instance == null)
			{
				StopAllCoroutines();
				GameManager.SetTimeScale(1f);
				GameManager.GetInstance().CameraResetTargetZoom();
				DoAttachmentEffect(AttachmentState.OnAttackEnd);
				if (attackRoutine != null)
				{
					StopCoroutine(attackRoutine);
				}
				attackRoutine = StartCoroutine(DoAttackRoutine(DoForceHeavyAttack));
				yield break;
			}
			if (!followingHeavyAttack && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) && !DataManager.Instance.SpecialAttacksDisabled)
			{
				HoldingForHeavyAttack = true;
				PlayerFarming.Instance.GetComponent<PlayerWeapon>().GetCurrentWeapon();
				float delayTimeBeforeHeavyAttack = Time.realtimeSinceStartup + 0.4f;
				while (Time.realtimeSinceStartup < delayTimeBeforeHeavyAttack)
				{
					if (!InputManager.Gameplay.GetHeavyAttackButtonHeld() || CheckForDeath())
					{
						HoldingForHeavyAttack = false;
						break;
					}
					yield return null;
				}
				if (HoldingForHeavyAttack)
				{
					HoldingForHeavyAttack = false;
					if (!followingHeavyAttack && !CheckForDeath())
					{
						StopAllCoroutines();
						StartCoroutine(DoAttackRoutine(true, true));
						yield break;
					}
				}
			}
			yield return null;
		}
		if (!CheckForDeath())
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			DoingHeavyAttack = false;
		}
	}

	private bool CheckForDeath()
	{
		if (state.CURRENT_STATE != StateMachine.State.Dieing)
		{
			return state.CURRENT_STATE == StateMachine.State.Dieing;
		}
		return true;
	}

	private bool MoveFromCanBreakState()
	{
		if (InputManager.Gameplay.GetHeavyAttackButtonHeld())
		{
			return false;
		}
		if (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement)
		{
			if (state.CURRENT_STATE == StateMachine.State.Attacking)
			{
				if (!DoHeavyAttack || EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PrimaryEquipmentType != EquipmentType.Axe)
				{
					Action onCrownReturnSubtle = PlayerFarming.OnCrownReturnSubtle;
					if (onCrownReturnSubtle != null)
					{
						onCrownReturnSubtle();
					}
				}
				state.CURRENT_STATE = StateMachine.State.Moving;
				DoingHeavyAttack = false;
			}
			DoAttachmentEffect(AttachmentState.OnAttackEnd);
			return true;
		}
		return false;
	}

	private void DoAttachmentEffect(AttachmentState attachmentState)
	{
		if (attachmentState == AttachmentState.Constant)
		{
			CurrentWeapon.ResetMultipliers();
		}
		foreach (WeaponAttachmentData item in GetAttachmentsWithState(DataManager.Instance.CurrentWeapon, attachmentState))
		{
			if (item.IsAttachmentActive())
			{
				Vector3 position = base.transform.position + new Vector3(item.ExplosionOffset * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), item.ExplosionOffset * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), -0.5f);
				switch (item.Effect)
				{
				case AttachmentEffect.Explode:
					Explosion.CreateExplosion(position, Health.Team.PlayerTeam, health, item.ExplosionRadius, item.ExplosionDamage);
					break;
				case AttachmentEffect.Dash:
					playerController.forceDir = Utils.GetAngle(Vector3.zero, new Vector3(playerController.xDir, playerController.yDir));
					playerController.speed = item.DashSpeed;
					break;
				case AttachmentEffect.Damage:
					CurrentWeapon.WeaponDamageMultiplier += item.DamageMultiplierIncrement;
					break;
				case AttachmentEffect.Critical:
					CurrentWeapon.CriticalChance += item.CriticalMultiplierIncrement;
					break;
				case AttachmentEffect.Range:
					CurrentWeapon.RangeMultiplier += item.RangeIncrement;
					break;
				case AttachmentEffect.AttackRate:
					CurrentWeapon.AttackRateMultiplier += item.AttackRateIncrement;
					break;
				case AttachmentEffect.MovementSpeed:
					CurrentWeapon.MovementSpeedMultiplier += item.MovementSpeedIncrement;
					break;
				case AttachmentEffect.IncreasedXPDrop:
					CurrentWeapon.XPDropMultiplier += item.xpDropIncrement;
					break;
				case AttachmentEffect.HealChance:
					CurrentWeapon.HealChance += item.healChanceIncrement;
					CurrentWeapon.HealAmount += item.healAmount;
					break;
				case AttachmentEffect.NegateDamageChance:
					CurrentWeapon.NegateDamageChance += item.negateDamageChanceIncrement;
					break;
				case AttachmentEffect.Poison:
					CurrentWeapon.PoisonChance += item.poisonChance;
					break;
				}
			}
		}
	}

	public float GetAverageWeaponDamage(EquipmentType weaponType, int WeaponLevel)
	{
		if (weaponType == EquipmentType.None)
		{
			return 1f;
		}
		WeaponData weaponData = EquipmentManager.GetWeaponData(weaponType);
		if (weaponData == null || weaponData.EquipmentType == EquipmentType.None)
		{
			return 0f;
		}
		float num = 0f;
		foreach (WeaponCombos combo in weaponData.Combos)
		{
			num += GetDamage(combo.Damage, WeaponLevel);
		}
		return (float)Math.Round(num / (float)weaponData.Combos.Count * 100f / 100f, 1);
	}

	public float GetWeaponSpeed(EquipmentType weaponType)
	{
		if (weaponType == EquipmentType.None)
		{
			return 1f;
		}
		WeaponData weaponData = EquipmentManager.GetWeaponData(weaponType);
		if (!(weaponData != null))
		{
			return 1f;
		}
		return weaponData.Speed;
	}

	public List<WeaponAttachmentData> GetAttachmentsWithState(EquipmentType weaponType, AttachmentState state)
	{
		WeaponData weaponData = EquipmentManager.GetWeaponData(weaponType);
		List<WeaponAttachmentData> list = new List<WeaponAttachmentData>();
		if (weaponData != null && weaponData.Attachments != null)
		{
			foreach (WeaponAttachmentData attachment in weaponData.Attachments)
			{
				if (attachment.State == state)
				{
					list.Add(attachment);
				}
			}
		}
		return list;
	}

	private IEnumerator DoHammerHeavyAttack()
	{
		loopedSound = AudioManager.Instance.CreateLoop("event:/weapon/hammer_heavy/hammer_unsheath", true);
		state.CURRENT_STATE = StateMachine.State.ChargingHeavyAttack;
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Hammer))
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-hammer-charge2", false);
			skeletonAnimation.AnimationState.AddAnimation(0, "attack-heavy-hammer-wait2", true, 0f);
		}
		else
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-hammer-charge", false);
			skeletonAnimation.AnimationState.AddAnimation(0, "attack-heavy-hammer-wait", true, 0f);
		}
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < 0.5f))
			{
				break;
			}
			if (state.CURRENT_STATE == StateMachine.State.Dodging)
			{
				yield break;
			}
			playerController.speed = 0f;
			if (state.CURRENT_STATE != StateMachine.State.ChargingHeavyAttack)
			{
				PlayerFarming.Instance.HideWeaponChargeBars();
				StopAllCoroutines();
				yield break;
			}
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/weapon/hammer_heavy/hammer_charged", base.gameObject);
		PlayerFarming.Instance.ShowWeaponChargeBars();
		TrackEntry Track = skeletonAnimation.AnimationState.SetAnimation(1, "attack-charge-walk", true);
		health.OnHit += OnHitClearHeavyAttackTrack;
		while (InputManager.Gameplay.GetHeavyAttackButtonHeld())
		{
			Track.TimeScale = Mathf.Clamp01(playerController.speed / playerController.RunSpeed) * 2f;
			if (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement)
			{
				state.facingAngle = (playerController.forceDir = (StoreFacing = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis()))));
			}
			if (InputManager.General.MouseInputActive)
			{
				Vector3 fromPosition = GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().WorldToScreenPoint(base.transform.position);
				state.facingAngle = (StoreFacing = Utils.GetAngle(fromPosition, InputManager.General.GetMousePosition()));
			}
			playerFarming.SetWeaponAimingRecticuleScaleAndRotation(0, Vector3.one, new Vector3(0f, 0f, state.facingAngle));
			if (state.CURRENT_STATE != StateMachine.State.ChargingHeavyAttack)
			{
				skeletonAnimation.AnimationState.ClearTrack(1);
				health.OnHit -= OnHitClearHeavyAttackTrack;
				PlayerFarming.Instance.HideWeaponChargeBars();
				playerFarming.simpleSpineAnimator.FlashWhite(false);
				StopAllCoroutines();
				yield break;
			}
			playerFarming.simpleSpineAnimator.FlashMeWhite();
			yield return null;
		}
		AudioManager.Instance.StopLoop(loopedSound);
		skeletonAnimation.AnimationState.ClearTrack(1);
		health.OnHit -= OnHitClearHeavyAttackTrack;
		PlayerFarming.Instance.HideWeaponChargeBars();
		playerFarming.simpleSpineAnimator.FlashWhite(false);
		CurrentAttackState = AttackState.Begin;
		state.CURRENT_STATE = StateMachine.State.Attacking;
		CameraManager.shakeCamera(0.5f, state.facingAngle);
		AudioManager.Instance.PlayOneShot("event:/weapon/hammer_heavy/hammer_release_swing", base.gameObject);
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Hammer))
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-hammer2", false);
		}
		else
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-hammer", false);
		}
		FaithAmmo.UseAmmo(HeavyAttackFervourCost, false);
		while (CurrentAttackState == AttackState.Begin)
		{
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		DoingHeavyAttack = false;
	}

	private IEnumerator DoAxeHeavyAttack()
	{
		Debug.Log("DoAxeHeavyAttack()".Colour(Color.yellow));
		loopedSound = AudioManager.Instance.CreateLoop("event:/weapon/hammer_heavy/hammer_unsheath", true);
		state.CURRENT_STATE = StateMachine.State.ChargingHeavyAttack;
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Axe))
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-axe-start2", false);
			skeletonAnimation.AnimationState.AddAnimation(0, "attack-heavy-axe-holding2", true, 0f);
		}
		else
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-axe-start", false);
			skeletonAnimation.AnimationState.AddAnimation(0, "attack-heavy-axe-holding", true, 0f);
		}
		playerFarming.ShowHeavyAttackProjectileChargeBars();
		float ChargeBarScale = 0f;
		while (InputManager.Gameplay.GetHeavyAttackButtonHeld())
		{
			if (state.CURRENT_STATE == StateMachine.State.Dodging)
			{
				yield break;
			}
			playerController.speed = 0f;
			Debug.Log("A".Colour(Color.red));
			if (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement)
			{
				state.facingAngle = (StoreFacing = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis())));
			}
			if (InputManager.General.MouseInputActive)
			{
				Vector3 fromPosition = GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().WorldToScreenPoint(base.transform.position);
				state.facingAngle = (StoreFacing = Utils.GetAngle(fromPosition, InputManager.General.GetMousePosition()));
			}
			float a;
			ChargeBarScale = (a = ChargeBarScale + Time.deltaTime * 2f);
			float z = Mathf.Min(a, 1f);
			playerFarming.SetHeavyAimingRecticuleScaleAndRotation(0, new Vector3(1f, 1f, z), new Vector3(0f, 0f, state.facingAngle));
			if (state.CURRENT_STATE != StateMachine.State.ChargingHeavyAttack)
			{
				skeletonAnimation.AnimationState.ClearTrack(1);
				playerFarming.HideHeavyChargeBars();
				playerFarming.simpleSpineAnimator.FlashWhite(false);
				ShowHeavyAim = false;
				CanChangeDirection = false;
				StopAllCoroutines();
				yield break;
			}
			playerFarming.simpleSpineAnimator.FlashMeWhite();
			yield return null;
		}
		AudioManager.Instance.StopLoop(loopedSound);
		skeletonAnimation.AnimationState.ClearTrack(1);
		playerFarming.simpleSpineAnimator.FlashWhite(false);
		ShowHeavyAim = false;
		playerFarming.HideHeavyChargeBars();
		CanChangeDirection = false;
		playerController.unitObject.DoKnockBack((state.facingAngle + 180f) % 360f * ((float)Math.PI / 180f), 0.3f, 0.3f);
		CurrentAttackState = AttackState.Begin;
		state.CURRENT_STATE = StateMachine.State.Attacking;
		CameraManager.shakeCamera(0.5f, state.facingAngle);
		AudioManager.Instance.PlayOneShot("event:/weapon/axe_heavy/thorw_axe_release", base.gameObject);
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Hammer))
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-axe-throw2", false);
		}
		else
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-axe-throw", false);
		}
		while (CurrentAttackState == AttackState.Begin)
		{
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		DoingHeavyAttack = false;
	}

	private IEnumerator DoDaggerHeavyAttack()
	{
		Debug.Log("DoAxeHeavyAttack()".Colour(Color.yellow));
		loopedSound = AudioManager.Instance.CreateLoop("event:/weapon/hammer_heavy/hammer_unsheath", true);
		state.CURRENT_STATE = StateMachine.State.ChargingHeavyAttack;
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Dagger))
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-dagger-charge", false);
			skeletonAnimation.AnimationState.AddAnimation(0, "attack-heavy-dagger-holding", true, 0f);
		}
		else
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-dagger-charge", false);
			skeletonAnimation.AnimationState.AddAnimation(0, "attack-heavy-dagger-holding", true, 0f);
		}
		playerFarming.ShowHeavyAttackProjectileChargeBars();
		float ChargeBarScale = 0f;
		while (InputManager.Gameplay.GetHeavyAttackButtonHeld())
		{
			if (state.CURRENT_STATE == StateMachine.State.Dodging)
			{
				yield break;
			}
			playerController.speed = 0f;
			Debug.Log("A".Colour(Color.red));
			if (Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > PlayerController.MinInputForMovement || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > PlayerController.MinInputForMovement)
			{
				state.facingAngle = (StoreFacing = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.Gameplay.GetHorizontalAxis(), InputManager.Gameplay.GetVerticalAxis())));
			}
			if (InputManager.General.MouseInputActive)
			{
				Vector3 fromPosition = GameManager.GetInstance().CamFollowTarget.GetComponent<Camera>().WorldToScreenPoint(base.transform.position);
				state.facingAngle = (StoreFacing = Utils.GetAngle(fromPosition, InputManager.General.GetMousePosition()));
			}
			float a;
			ChargeBarScale = (a = ChargeBarScale + Time.deltaTime * 2f);
			float z = Mathf.Min(a, 1f);
			playerFarming.SetHeavyAimingRecticuleScaleAndRotation(0, new Vector3(1f, 1f, z), new Vector3(0f, 0f, state.facingAngle));
			if (state.CURRENT_STATE != StateMachine.State.ChargingHeavyAttack)
			{
				skeletonAnimation.AnimationState.ClearTrack(1);
				playerFarming.HideHeavyChargeBars();
				playerFarming.simpleSpineAnimator.FlashWhite(false);
				ShowHeavyAim = false;
				CanChangeDirection = false;
				StopAllCoroutines();
				yield break;
			}
			playerFarming.simpleSpineAnimator.FlashMeWhite();
			yield return null;
		}
		AudioManager.Instance.StopLoop(loopedSound);
		skeletonAnimation.AnimationState.ClearTrack(1);
		playerFarming.simpleSpineAnimator.FlashWhite(false);
		ShowHeavyAim = false;
		playerFarming.HideHeavyChargeBars();
		CanChangeDirection = false;
		playerController.unitObject.DoKnockBack((state.facingAngle + 180f) % 360f * ((float)Math.PI / 180f), 0.3f, 0.3f);
		CurrentAttackState = AttackState.Begin;
		state.CURRENT_STATE = StateMachine.State.Attacking;
		CameraManager.shakeCamera(0.5f, state.facingAngle);
		AudioManager.Instance.PlayOneShot("event:/weapon/hammer_heavy/hammer_release_swing", base.gameObject);
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Hammer))
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-dagger", false);
		}
		else
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "attack-heavy-dagger", false);
		}
		while (CurrentAttackState == AttackState.Begin)
		{
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		DoingHeavyAttack = false;
	}

	private void OnHitClearHeavyAttackTrack(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		health.OnHit -= OnHitClearHeavyAttackTrack;
		skeletonAnimation.AnimationState.ClearTrack(1);
	}

	private void SetSpecial(int Num)
	{
		CurrentSpecial = Specials[Num];
		DataManager.Instance.PLAYER_SPECIAL_CHARGE_TARGET = CurrentSpecial.Target;
	}
}
