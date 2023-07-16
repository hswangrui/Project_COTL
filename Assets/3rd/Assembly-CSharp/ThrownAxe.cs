using System;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ThrownAxe : BaseMonoBehaviour
{
	private GameObject Master;

	[SerializeField]
	private GameObject HeavyContainer;

	[SerializeField]
	private GameObject StandardContainer;

	private Health MasterHealth;

	private StateMachine MasterState;

	public GameObject AxeSprite;

	public SpriteRenderer AxeSpriteRenderer;

	public SpriteRenderer AxeShadowSpriteRenderer;

	public float MaxSpeed = 10f;

	private float Speed;

	private float TargetAngle;

	public float ReturnTime = 1f;

	private float ReturnTimer;

	public float ReturnSpeedDelta = 0.1f;

	public float StartingReturnSpeed = 180f;

	private float ReturnSpeed;

	public float RotationSpeedMultiplier = 1f;

	public static ThrownAxe Instance;

	private EventInstance loopedSound;

	private bool HasResetList;

	private StateMachine state;

	public float TurnSpeed = 5f;

	private float Damage = 1f;

	private Health target;

	public float DamageToNeutral = 10f;

	public float NeutralSplashRadius = 2f;

	private LayerMask unitMask;

	public float ScreenShakeMultiplier = 1f;

	private List<Health> DamagedEnemies = new List<Health>();

	public static void SpawnThrowingAxe(Vector3 position, float damageMultiplier, Sprite AxeImage, float Angle)
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Thrown Axe.prefab", position, Quaternion.identity, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			ThrownAxe component = obj.Result.GetComponent<ThrownAxe>();
			component.Damage = damageMultiplier;
			component.BeginThrow(AxeImage, Angle);
		};
	}

	private void OnEnable()
	{
		Instance = this;
		Master = PlayerFarming.Instance.gameObject;
		MasterState = Master.GetComponent<StateMachine>();
		MasterHealth = Master.GetComponent<Health>();
		MasterHealth.OnDie += Health_OnDie;
	}

	public void BeginThrow(Sprite AxeImage, float Angle)
	{
		loopedSound = AudioManager.Instance.CreateLoop("event:/weapon/axe_heavy/spinning_axe", base.gameObject, true);
		state = GetComponent<StateMachine>();
		unitMask = (int)unitMask | (1 << LayerMask.NameToLayer("Obstacles"));
		unitMask = (int)unitMask | (1 << LayerMask.NameToLayer("Units"));
		float x = AxeSprite.transform.localScale.x;
		AxeSprite.transform.localScale = new Vector3(2f, 2f, 1f);
		AxeSprite.transform.DOScale(x, 0.2f);
		if (AxeImage != null)
		{
			StandardContainer.SetActive(true);
			HeavyContainer.SetActive(false);
			AxeSpriteRenderer.sprite = AxeImage;
			AxeShadowSpriteRenderer.sprite = AxeImage;
		}
		else
		{
			StandardContainer.SetActive(false);
			HeavyContainer.SetActive(true);
		}
		base.transform.position = Master.transform.position;
		state.facingAngle = (TargetAngle = Angle);
		MaxSpeed *= (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Axe) ? 1f : 0.75f);
		ReturnSpeedDelta *= (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HA_Axe) ? 1f : 2f);
		Speed = MaxSpeed;
		ReturnTimer = ReturnTime;
		ReturnSpeed = StartingReturnSpeed;
		state.CURRENT_STATE = StateMachine.State.Moving;
	}

	private void Update()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Moving:
			if (!((ReturnTimer -= Time.deltaTime) < 0f))
			{
				break;
			}
			if (Speed > 0f - MaxSpeed)
			{
				ReturnSpeed += ReturnSpeedDelta * Time.deltaTime;
				float speed = Speed;
				Speed = MaxSpeed * Mathf.Sin(ReturnSpeed);
				if (speed > 0f && Speed <= 0f)
				{
					DamagedEnemies.Clear();
					HasResetList = true;
				}
				if (57.29578f * Mathf.Sin(ReturnSpeed) <= -45f)
				{
					if (!HasResetList)
					{
						DamagedEnemies.Clear();
					}
					state.CURRENT_STATE = StateMachine.State.Fleeing;
				}
			}
			if (Speed < 0f)
			{
				TargetAngle = Utils.GetAngle(Master.transform.position, base.transform.position);
			}
			break;
		case StateMachine.State.Fleeing:
			Speed -= ReturnSpeedDelta * Time.deltaTime;
			TargetAngle = Utils.GetAngle(Master.transform.position, base.transform.position);
			if (Vector2.Distance(base.transform.position, Master.transform.position) <= 0.5f)
			{
				Action onCrownReturn = PlayerFarming.OnCrownReturn;
				if (onCrownReturn != null)
				{
					onCrownReturn();
				}
				if (PlayerFarming.Instance.playerController.speed <= 0f)
				{
					PlayerFarming.Instance.playerController.unitObject.DoKnockBack((TargetAngle + 180f) % 360f * ((float)Math.PI / 180f), 0.5f, 0.1f);
				}
				else
				{
					CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.2f);
				}
				CloseAxe();
			}
			break;
		}
		AxeSprite.transform.eulerAngles += new Vector3(0f, 0f, Mathf.Abs(RotationSpeedMultiplier) * Time.deltaTime);
		state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / TurnSpeed * Time.deltaTime * 60f;
		base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime, Speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime);
	}

	private void CloseAxe()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		AudioManager.Instance.PlayOneShot("event:/weapon/axe_heavy/catch_axe", base.transform.position);
		if (Instance == this)
		{
			Instance = null;
		}
		DamagedEnemies.Clear();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Health_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		CloseAxe();
	}

	private void OnDisable()
	{
		CloseAxe();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
		if ((bool)PlayerFarming.Instance)
		{
			PlayerFarming.Instance.health.OnDie -= Health_OnDie;
		}
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		CloseAxe();
	}

	protected void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider == null || !base.gameObject.activeSelf)
		{
			return;
		}
		target = collider.gameObject.GetComponent<Health>();
		if (target == null || DamagedEnemies.Contains(target))
		{
			return;
		}
		DamagedEnemies.Add(target);
		if (!(target != null) || !target.enabled || (target.team == MasterHealth.team && !target.IsCharmedEnemy) || target.untouchable || target.invincible || (!(target.state == null) && target.state.CURRENT_STATE == StateMachine.State.Dodging) || ((target.team == Health.Team.Neutral || !AxeSprite.gameObject.activeSelf || target.invincible) && (target.team != 0 || !(DamageToNeutral > 0f))))
		{
			return;
		}
		Health.AttackTypes attackType = Health.AttackTypes.Melee;
		if (target.HasShield)
		{
			attackType = Health.AttackTypes.Heavy;
		}
		if (!target.DealDamage((target.team == Health.Team.Neutral) ? DamageToNeutral : Damage, base.gameObject, base.transform.position, false, attackType, false, Health.AttackFlags.Penetration))
		{
			CameraManager.shakeCamera(0.2f * ScreenShakeMultiplier, Utils.GetAngle(base.transform.position, target.transform.position));
		}
		else
		{
			CameraManager.shakeCamera(0.5f * ScreenShakeMultiplier, Utils.GetAngle(base.transform.position, target.transform.position));
		}
		Collider2D[] array;
		if (target.team == Health.Team.Neutral && NeutralSplashRadius > 0f)
		{
			array = Physics2D.OverlapCircleAll(base.transform.position, NeutralSplashRadius, unitMask);
			for (int i = 0; i < array.Length; i++)
			{
				Health component = array[i].GetComponent<Health>();
				if ((bool)component && component.team == Health.Team.Neutral)
				{
					component.DealDamage(DamageToNeutral, base.gameObject, base.transform.position);
				}
			}
		}
		AudioManager.Instance.PlayOneShot("event:/player/Curses/arrow_hit", base.transform.position);
		if (!MasterHealth || MasterHealth.team != Health.Team.PlayerTeam || !collider || !(collider.GetComponentInParent<Projectile>() != null))
		{
			return;
		}
		array = Physics2D.OverlapCircleAll(base.transform.position, NeutralSplashRadius);
		for (int i = 0; i < array.Length; i++)
		{
			Projectile componentInParent = array[i].GetComponentInParent<Projectile>();
			if ((bool)componentInParent)
			{
				componentInParent.DestroyProjectile();
			}
		}
	}
}
