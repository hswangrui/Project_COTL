using System;
using System.Collections;
using MMBiomeGeneration;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class Demon_Collector : Demon
{
	private float AttackProgress;

	private float AttackAngle;

	private float AttackDelay;

	private float AttackDurationCount = 60f;

	public SimpleSpineAnimator simpleSpineAnimator;

	private GameObject _Master;

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

	public LayerMask layerToCheck;

	public GameObject Container;

	private float collectTimer;

	private const float collectMin = 20f;

	private const float collectMax = 30f;

	private Vector3 collectPosition;

	private InventoryItem.ITEM_TYPE itemToDrop;

	private float DetectEnemyRange = 5f;

	private Health CurrentTarget;

	private Vector3 pointToCheck;

	private Vector3 Seperator;

	public float SeperationRadius = 0.5f;

	private GameObject Master
	{
		get
		{
			if (_Master == null)
			{
				_Master = GameObject.FindGameObjectWithTag("Player");
				if (_Master != null)
				{
					MasterState = _Master.GetComponent<StateMachine>();
					MasterHealth = _Master.GetComponent<Health>();
				}
			}
			return _Master;
		}
		set
		{
			_Master = value;
		}
	}

	public bool CanCollect { get; set; } = true;


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
			spine.skeleton.SetSkin("Heart+");
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
		if (state.CURRENT_STATE == StateMachine.State.SignPostAttack)
		{
			base.transform.position = PlayerFarming.Instance.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 50f;
		}
		else
		{
			base.transform.position = Master.transform.position + Vector3.right;
		}
	}

	private void Update()
	{
		if (Master == null || GameManager.DeltaTime == 0f || MMConversation.isPlaying)
		{
			return;
		}
		if ((state.CURRENT_STATE == StateMachine.State.Idle || state.CURRENT_STATE == StateMachine.State.Moving) && (AttackDelay += Time.deltaTime) > AttackDurationCount && CanCollect)
		{
			AttackDurationCount = UnityEngine.Random.Range(60, 120);
			AttackDelay = 0f;
			collectPosition = base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 50f;
			collectTimer = UnityEngine.Random.Range(20f, 30f);
			state.CURRENT_STATE = StateMachine.State.SignPostAttack;
		}
		if (!CanCollect && (state.CURRENT_STATE == StateMachine.State.SignPostAttack || state.CURRENT_STATE == StateMachine.State.RecoverFromAttack))
		{
			base.transform.position = Master.transform.position + Vector3.right;
			state.CURRENT_STATE = StateMachine.State.Idle;
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
			if (Vector2.Distance(base.transform.position, Master.transform.position) > 2f)
			{
				TargetAngle = Utils.GetAngle(base.transform.position, Master.transform.position);
				state.facingAngle = TargetAngle;
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			break;
		case StateMachine.State.Moving:
			TargetAngle = Utils.GetAngle(base.transform.position, Master.transform.position);
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			Speed += (6f - Speed) / (15f * GameManager.DeltaTime);
			if (Vector2.Distance(base.transform.position, Master.transform.position) < 1.5f)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.SignPostAttack:
			Speed += (6f - Speed) / (15f * GameManager.DeltaTime);
			state.facingAngle = Utils.GetAngle(base.transform.position, collectPosition);
			if (Health.team2.Count > 0 && (state.Timer += Time.deltaTime) > collectTimer)
			{
				base.transform.position = PlayerFarming.Instance.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 50f;
				state.CURRENT_STATE = StateMachine.State.RecoverFromAttack;
				float num = 0.5f;
				num += 0.05f * (float)base.Level;
				if (base.Level > 1)
				{
					if (UnityEngine.Random.value < num)
					{
						itemToDrop = ((UnityEngine.Random.value > 0.25f) ? InventoryItem.ITEM_TYPE.HALF_BLUE_HEART : InventoryItem.ITEM_TYPE.BLUE_HEART);
					}
					else
					{
						itemToDrop = ((UnityEngine.Random.value > 0.25f) ? InventoryItem.ITEM_TYPE.HALF_HEART : InventoryItem.ITEM_TYPE.RED_HEART);
					}
				}
				else
				{
					itemToDrop = ((UnityEngine.Random.value > 0.25f) ? InventoryItem.ITEM_TYPE.HALF_HEART : InventoryItem.ITEM_TYPE.RED_HEART);
				}
			}
			if (state.Timer > 10f)
			{
				Container.SetActive(false);
			}
			break;
		case StateMachine.State.RecoverFromAttack:
			Container.SetActive(true);
			TargetAngle = Utils.GetAngle(base.transform.position, Master.transform.position);
			state.facingAngle += Mathf.Atan2(Mathf.Sin((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - state.facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / (15f * GameManager.DeltaTime);
			Speed += (6f - Speed) / (15f * GameManager.DeltaTime);
			if (Vector2.Distance(base.transform.position, Master.transform.position) < 1.5f && PlayerFarming.Instance.gameObject != null && !PlayerFarming.Instance.GoToAndStopping)
			{
				InventoryItem.Spawn(itemToDrop, 1, base.transform.position);
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
		vx = Speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		vy = Speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
		base.transform.position = base.transform.position + new Vector3(vx, vy);
		spine.skeleton.ScaleX = ((!(Master.transform.position.x > base.transform.position.x)) ? 1 : (-1));
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
			if (allUnit.team != MasterHealth.team && allUnit.team != 0 && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DetectEnemyRange && CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, base.transform.position)))
			{
				float num2 = Vector3.Distance(base.transform.position, allUnit.gameObject.transform.position);
				if (num2 < num)
				{
					CurrentTarget = allUnit;
					num = num2;
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
}
