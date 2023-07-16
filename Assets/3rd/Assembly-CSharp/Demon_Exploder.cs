using System;
using System.Collections;
using FMOD.Studio;
using MMBiomeGeneration;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class Demon_Exploder : Demon
{
	private float AttackProgress;

	private float AttackAngle;

	private float AttackDelay;

	public SimpleSpineAnimator simpleSpineAnimator;

	private Health MasterHealth;

	private StateMachine MasterState;

	private StateMachine state;

	private float TargetAngle;

	private Vector3 MoveVector;

	private float Speed;

	private float vx;

	private float vy;

	private float Bobbing;

	private float SpineVZ;

	private float SpineVY;

	public SkeletonAnimation spine;

	public Transform ChainPoint;

	public LayerMask layerToCheck;

	public GameObject Container;

	private EventInstance loopedSound;

	private float DetectEnemyRange = 5f;

	private Health CurrentTarget;

	private Vector3 pointToCheck;

	private Vector3 Seperator;

	public float SeperationRadius = 0.5f;

	private void OnEnable()
	{
		Demon_Arrows.Demons.Add(base.gameObject);
	}

	private void OnDisable()
	{
		Demon_Arrows.Demons.Remove(base.gameObject);
	}

	private void Start()
	{
		state = GetComponent<StateMachine>();
		SpineVZ = -1.5f;
		SpineVY = 0.5f;
		spine.transform.localPosition = new Vector3(0f, SpineVY, SpineVZ + 0.1f * Mathf.Cos(Bobbing += 5f * Time.deltaTime));
		BiomeGenerator.OnBiomeChangeRoom += BiomeGenerator_OnBiomeChangeRoom;
		HealthPlayer.OnPlayerDied += Health_OnDie;
		StartCoroutine(SetSkin());
	}

	protected override IEnumerator SetSkin()
	{
		while (spine.AnimationState == null)
		{
			yield return null;
		}
		if (base.Level > 1)
		{
			spine.skeleton.SetSkin("Explode+");
			spine.skeleton.SetSlotsToSetupPose();
			spine.AnimationState.Apply(spine.skeleton);
		}
	}

	private void Health_OnDie(HealthPlayer player)
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
		HealthPlayer.OnPlayerDied -= Health_OnDie;
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		base.transform.position = PlayerFarming.Instance.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private void Update()
	{
		if (PlayerFarming.Instance == null || GameManager.DeltaTime == 0f || MMConversation.isPlaying || MMConversation.isPlaying)
		{
			return;
		}
		if (MasterHealth == null)
		{
			MasterHealth = PlayerFarming.Instance.health;
		}
		if ((state.CURRENT_STATE == StateMachine.State.Idle || state.CURRENT_STATE == StateMachine.State.Moving) && (AttackDelay += Time.deltaTime) > 3f)
		{
			AttackDelay = 0f;
			GetNewTarget();
			if (CurrentTarget != null)
			{
				Container.SetActive(true);
				state.CURRENT_STATE = StateMachine.State.SignPostAttack;
				AudioManager.Instance.PlayOneShot("event:/enemy/chaser/chaser_charge", base.gameObject);
			}
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.SpawnIn:
			if ((state.Timer += Time.deltaTime) > 0.6f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.Idle:
			Speed += (0f - Speed) / (7f * GameManager.DeltaTime);
			if (Vector2.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 2f)
			{
				TargetAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
				state.facingAngle = TargetAngle;
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			Container.SetActive(true);
			break;
		case StateMachine.State.Moving:
			TargetAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			Speed += (6f - Speed) / (15f * GameManager.DeltaTime);
			if (Vector2.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < 1.5f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.SignPostAttack:
			if (!Container.activeSelf)
			{
				return;
			}
			Speed += (6f - Speed) / (15f * GameManager.DeltaTime);
			if (Time.frameCount % 5 == 0)
			{
				simpleSpineAnimator.FlashWhite(!simpleSpineAnimator.isFillWhite);
			}
			if ((bool)CurrentTarget)
			{
				state.facingAngle = Utils.GetAngle(base.transform.position, CurrentTarget.transform.position);
			}
			if (CurrentTarget != null && MagnitudeFindDistanceBetween(base.transform.position, CurrentTarget.transform.position) < 1f)
			{
				float team2Damage = 5f * (float)base.Level;
				Explosion.CreateExplosion(base.transform.position, Health.Team.PlayerTeam, MasterHealth, 2f, -1f, team2Damage);
				state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
			}
			break;
		case StateMachine.State.RecoverFromAttack:
			Container.SetActive(false);
			break;
		}
		vx = Speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		vy = Speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		base.transform.position = base.transform.position + new Vector3(vx, vy);
		spine.skeleton.ScaleX = ((!(PlayerFarming.Instance.transform.position.x > base.transform.position.x)) ? 1 : (-1));
		spine.transform.eulerAngles = new Vector3(-60f, 0f, vx * -5f / Time.deltaTime);
		SpineVZ = Mathf.Lerp(SpineVZ, -1f, 5f * Time.deltaTime);
		SpineVY = Mathf.Lerp(SpineVY, 0.5f, 5f * Time.deltaTime);
		spine.transform.localPosition = new Vector3(0f, 0f, SpineVZ + 0.1f * Mathf.Cos(Bobbing += 5f * Time.deltaTime));
		SeperateDemons();
	}

	public void GetNewTarget()
	{
		CurrentTarget = null;
		float num = float.MaxValue;
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit != null && allUnit.team != MasterHealth.team && allUnit.team != 0 && allUnit.enabled && !allUnit.invincible && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange && CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, base.transform.position)))
			{
				float num2 = MagnitudeFindDistanceBetween(base.transform.position, allUnit.gameObject.transform.position);
				if (num2 < num * num)
				{
					CurrentTarget = allUnit;
					num = Mathf.Sqrt(num2);
				}
			}
		}
	}

	public bool CheckLineOfSight(Vector3 pointToCheck, float distance)
	{
		this.pointToCheck = pointToCheck;
		if (Physics2D.Raycast(base.transform.position, pointToCheck - base.transform.position, distance, layerToCheck).collider != null)
		{
			return false;
		}
		return true;
	}

	private void SeperateDemons()
	{
		Seperator = Vector3.zero;
		foreach (GameObject demon in Demon_Arrows.Demons)
		{
			if (demon != base.gameObject && demon != null && state.CURRENT_STATE != StateMachine.State.SignPostAttack && state.CURRENT_STATE != StateMachine.State.RecoverFromAttack)
			{
				float num = Vector2.Distance(demon.gameObject.transform.position, base.transform.position);
				float angle = Utils.GetAngle(demon.gameObject.transform.position, base.transform.position);
				if (num < SeperationRadius)
				{
					Seperator.x += (SeperationRadius - num) / 2f * Mathf.Cos(angle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
					Seperator.y += (SeperationRadius - num) / 2f * Mathf.Sin(angle * ((float)Math.PI / 180f)) * GameManager.DeltaTime;
				}
			}
		}
		base.transform.position = base.transform.position + Seperator;
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}
}
