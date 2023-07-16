using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMBiomeGeneration;
using UnityEngine;

public class Familiar : MonoBehaviour
{
	public enum MovementType
	{
		Follow,
		Spin
	}

	public enum CombatType
	{
		DamageOnTouch,
		FreezeOnTouch,
		PoisonOnTouch,
		ShootCurrentCurse
	}

	public static List<Familiar> Familiars = new List<Familiar>();

	[SerializeField]
	private MovementType familiarType;

	[SerializeField]
	private GameObject container;

	[SerializeField]
	private GameObject shadow;

	[SerializeField]
	private float minDistance;

	[SerializeField]
	private CombatType combatType;

	[SerializeField]
	private float baseVariable;

	[SerializeField]
	private ColliderEvents colliderEvents;

	[Space]
	[SerializeField]
	private float lifetime;

	[SerializeField]
	private float timeBetween;

	[SerializeField]
	private SpriteRenderer sprite;

	private float spineVZ;

	private float spineVY;

	private float vx;

	private float vy;

	private float bobbing;

	private float targetAngle;

	private float speed;

	private float timestamp;

	private int direction = 1;

	private bool destroyingSelf;

	private UnitObject master;

	private Health health;

	private StateMachine state;

	private Renderer[] renderers;

	public GameObject Container
	{
		get
		{
			return container;
		}
	}

	private Health.Team team
	{
		get
		{
			if (health == null && master != null)
			{
				health = master.GetComponent<Health>();
			}
			if (health == null)
			{
				return Health.Team.PlayerTeam;
			}
			return health.team;
		}
	}

	private float maxSpeed
	{
		get
		{
			if (PlayerFarming.Instance != null && master == PlayerFarming.Instance.unitObject)
			{
				if (PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Dodging)
				{
					return PlayerFarming.Instance.playerController.DodgeSpeed;
				}
				return PlayerFarming.Instance.playerController.GetPlayerMaxSpeed();
			}
			if (master != null)
			{
				return master.maxSpeed;
			}
			return 1f;
		}
	}

	private void Awake()
	{
		state = GetComponent<StateMachine>();
		colliderEvents.OnTriggerEnterEvent += OnTriggerEnterEvent;
		renderers = GetComponentsInChildren<Renderer>();
		direction = ((UnityEngine.Random.value > 0.5f) ? 1 : (-1));
		state.facingAngle += ((direction == -1) ? 180 : 0);
		timestamp = Time.time + 2.5f;
		BiomeGenerator.OnBiomeChangeRoom += BiomeGenerator_OnBiomeChangeRoom;
	}

	private void OnDestroy()
	{
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
	}

	public void SetDirection(int dir)
	{
		direction = dir;
		state.facingAngle += ((direction == -1) ? 180 : 0);
	}

	private void ShootCurse(EquipmentType type)
	{
		if (combatType == CombatType.ShootCurrentCurse)
		{
			timestamp = Time.time + timeBetween;
			sprite.material.DOFloat(1f, "_FillAlpha", 0.5f);
			StartCoroutine(Delay(0.5f, delegate
			{
				sprite.material.DOFloat(0f, "_FillAlpha", 0f);
				PlayerFarming.Instance.playerSpells.CastSpell(type, true, true, false, true, base.gameObject);
			}));
		}
	}

	private void OnEnable()
	{
		Familiars.Add(this);
	}

	private void OnDisable()
	{
		Familiars.Remove(this);
	}

	public void SetMaster(UnitObject master, float startingHeight = -1.25f)
	{
		this.master = master;
		spineVZ = startingHeight;
		if (state == null)
		{
			state = GetComponent<StateMachine>();
		}
		if (familiarType == MovementType.Follow)
		{
			UpdateFollowState();
		}
		else if (familiarType == MovementType.Spin)
		{
			UpdateSpinState();
		}
	}

	protected virtual void Update()
	{
		if (master == null && (bool)PlayerFarming.Instance)
		{
			master = PlayerFarming.Instance.unitObject;
		}
		if (master == null || (RespawnRoomManager.Instance != null && RespawnRoomManager.Instance.gameObject.activeSelf) || (DeathCatRoomManager.Instance != null && DeathCatRoomManager.Instance.gameObject.activeSelf) || (MysticShopKeeperManager.Instance != null && MysticShopKeeperManager.Instance.gameObject.activeSelf))
		{
			container.gameObject.SetActive(false);
			shadow.gameObject.SetActive(false);
		}
		else
		{
			container.gameObject.SetActive(true);
			shadow.gameObject.SetActive(true);
		}
		if (master == null)
		{
			return;
		}
		if (familiarType == MovementType.Follow)
		{
			UpdateFollowState();
		}
		else if (familiarType == MovementType.Spin)
		{
			UpdateSpinState();
		}
		if (lifetime != -1f)
		{
			lifetime -= Time.deltaTime;
			if (lifetime <= 0f && !destroyingSelf)
			{
				destroyingSelf = true;
				Renderer[] array = renderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].material.DOFade(0f, 0.5f);
				}
			}
			else if (lifetime < -0.5f && destroyingSelf)
			{
				BiomeConstants.Instance.EmitSmokeInteractionVFX(container.transform.position, Vector3.one * 0.5f);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		if (Time.time > timestamp)
		{
			ShootCurse(DataManager.Instance.CurrentCurse);
		}
	}

	protected virtual void UpdateFollowState()
	{
		float num = Vector3.Distance(base.transform.position, master.transform.position);
		targetAngle = Utils.GetAngle(base.transform.position, master.transform.position);
		state.facingAngle += Mathf.Atan2(Mathf.Sin((targetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((targetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
		if (num < minDistance)
		{
			speed = Mathf.Clamp(speed - 0.35f * GameManager.DeltaTime, 0f, 100f);
		}
		else
		{
			speed += (maxSpeed - speed) / (15f * GameManager.DeltaTime);
		}
		if (float.IsNaN(speed))
		{
			speed = maxSpeed;
		}
		if (float.IsNaN(state.facingAngle))
		{
			state.facingAngle = 0f;
		}
		vx = speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		vy = speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		if (float.IsNaN(vx))
		{
			vx = 0f;
		}
		if (float.IsNaN(vy))
		{
			vy = 0f;
		}
		base.transform.position = base.transform.position + new Vector3(vx, vy);
		if (Time.deltaTime == 0f)
		{
			container.transform.eulerAngles = new Vector3(-60f, 0f, vx * -5f);
		}
		else
		{
			container.transform.eulerAngles = new Vector3(-60f, 0f, vx * -5f / Time.deltaTime);
		}
		spineVZ = Mathf.Lerp(spineVZ, -1f, 5f * Time.deltaTime);
		spineVY = Mathf.Lerp(spineVY, 0.5f, 5f * Time.deltaTime);
		container.transform.localPosition = new Vector3(0f, 0f, spineVZ + 0.1f * Mathf.Cos(bobbing += 5f * Time.deltaTime));
	}

	public virtual void UpdateSpinState()
	{
		vx = Mathf.Cos(state.facingAngle) * minDistance;
		vy = Mathf.Sin(state.facingAngle) * minDistance;
		base.transform.position = Vector3.Lerp(base.transform.position, master.transform.position + new Vector3(vx, vy), 9f * Time.deltaTime);
		state.facingAngle += Time.deltaTime * (float)(2 * direction);
		spineVZ = Mathf.Lerp(spineVZ, -1f, 5f * Time.deltaTime);
		spineVY = Mathf.Lerp(spineVY, 0.5f, 5f * Time.deltaTime);
		container.transform.localPosition = new Vector3(0f, 0f, spineVZ + 0.1f * Mathf.Cos(bobbing += 5f * Time.deltaTime));
	}

	private void OnTriggerEnterEvent(Collider2D collider)
	{
		Health component = collider.GetComponent<Health>();
		if (component != null && component.team != team && component.team != 0)
		{
			if (combatType == CombatType.DamageOnTouch)
			{
				float damage = ((team == Health.Team.PlayerTeam) ? PlayerWeapon.GetDamage(baseVariable, DataManager.Instance.CurrentWeaponLevel) : 1f);
				component.DealDamage(damage, base.gameObject, base.transform.position, false, Health.AttackTypes.NoHitStop);
			}
			else if (combatType == CombatType.PoisonOnTouch)
			{
				float damage2 = ((team == Health.Team.PlayerTeam) ? PlayerWeapon.GetDamage(0.1f, DataManager.Instance.CurrentWeaponLevel) : 1f);
				component.DealDamage(damage2, base.gameObject, base.transform.position, false, Health.AttackTypes.NoHitStop);
				component.AddPoison(base.gameObject, baseVariable);
			}
			else if (combatType == CombatType.FreezeOnTouch)
			{
				float damage3 = ((team == Health.Team.PlayerTeam) ? PlayerWeapon.GetDamage(0.1f, DataManager.Instance.CurrentWeaponLevel) : 1f);
				component.DealDamage(damage3, base.gameObject, base.transform.position, false, Health.AttackTypes.NoHitStop);
				component.AddIce(baseVariable);
			}
		}
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		Transform transform = ((!(master == null)) ? master.transform : PlayerFarming.Instance.transform);
		base.transform.position = transform.position + Vector3.right;
	}

	private IEnumerator Delay(float seconds, Action callback)
	{
		yield return new WaitForSeconds(seconds);
		if (callback != null)
		{
			callback();
		}
	}

	public RelicType GetRelicType()
	{
		switch (combatType)
		{
		case CombatType.DamageOnTouch:
			return RelicType.DamageOnTouch_Familiar;
		case CombatType.FreezeOnTouch:
			return RelicType.FreezeOnTouch_Familiar;
		case CombatType.PoisonOnTouch:
			return RelicType.PoisonOnTouch_Familiar;
		case CombatType.ShootCurrentCurse:
			return RelicType.ShootCurses_Familiar;
		default:
			throw new Exception();
		}
	}
}
