using DG.Tweening;
using UnityEngine;

public class Fishable : BaseMonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRender;

	[SerializeField]
	private float fishMoveSpeed = 0.15f;

	[SerializeField]
	private float fishRotationSpeed = 0.35f;

	[SerializeField]
	private float targetHookMoveSpeed;

	[Space]
	[SerializeField]
	[Range(0f, 1f)]
	private float hookTargetAngle;

	[SerializeField]
	private float hookTargetingDistance;

	[SerializeField]
	private float hookSplashDistance = 3f;

	[SerializeField]
	private float biteHookDistance = 0.3f;

	[SerializeField]
	private float fishInterestMaxDistance;

	public StateMachine.State state;

	private Interaction_Fishing fishing;

	private Vector3 originalCenterPoint;

	private float moveSpeed;

	private float rotationSpeed;

	private const float moveRadius = 2.5f;

	private float moveTimer;

	private Vector2 timeInterval = new Vector2(1f, 3f);

	private Vector3 targetPosition;

	private Vector3 targetLookAt;

	private Vector3 playerPosition;

	private bool chasing;

	public Interaction_Fishing.FishType FishType { get; private set; }

	public InventoryItem.ITEM_TYPE ItemType
	{
		get
		{
			return FishType.Type;
		}
	}

	public int Quantity
	{
		get
		{
			return FishType.Quantity;
		}
	}

	public void Configure(Interaction_Fishing.FishType fishType, Interaction_Fishing fishingParent)
	{
		FishType = fishType;
		fishing = fishingParent;
		fishing.OnCasted += HookLanded;
		base.transform.localScale = Vector3.one * Random.Range(fishType.Scale.x, fishType.Scale.y);
		originalCenterPoint = new Vector3(fishingParent.transform.position.x, base.transform.position.y, 0f);
		state = GetComponent<StateMachine>().CURRENT_STATE;
		moveSpeed = fishMoveSpeed;
		rotationSpeed = fishRotationSpeed;
	}

	private void OnDestroy()
	{
		if (fishing != null)
		{
			fishing.OnCasted -= HookLanded;
		}
	}

	private void OnDisable()
	{
		if (fishing != null)
		{
			fishing.OnCasted -= HookLanded;
		}
	}

	private void HookLanded()
	{
		if (Vector3.Distance(fishing.FishingHook.position, base.transform.position) < hookSplashDistance && fishing.IsClosestFish(this))
		{
			TargetedHook();
		}
	}

	public void FadeIn()
	{
		spriteRender.material.color = new Color(spriteRender.material.color.r, spriteRender.material.color.g, spriteRender.material.color.b, 0f);
		spriteRender.material.DOFade(1f, 1f);
	}

	private void Update()
	{
		if (state == StateMachine.State.Idle)
		{
			GameManager instance = GameManager.GetInstance();
			if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > moveTimer)
			{
				SetRandomTargetPosition(2.5f);
			}
		}
		if (state == StateMachine.State.Attacking)
		{
			base.transform.position = fishing.FishingHook.position;
			float angle = Utils.GetAngle(playerPosition, base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, 0f, angle), rotationSpeed * Time.deltaTime);
		}
		else
		{
			float num = fishInterestMaxDistance - Vector3.Distance(base.transform.position, targetPosition);
			base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, moveSpeed * num * Time.deltaTime);
			float angle2 = Utils.GetAngle(targetLookAt, base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, 0f, angle2), rotationSpeed * Time.deltaTime);
		}
		if (fishing != null && fishing.state.CURRENT_STATE == StateMachine.State.Reeling)
		{
			float num2 = Vector3.Distance(base.transform.position, fishing.FishingHook.position);
			if (num2 < hookTargetingDistance)
			{
				TargetedHook();
			}
			else if (chasing)
			{
				targetPosition = fishing.FishingHook.position;
				targetLookAt = fishing.FishingHook.position;
			}
			if (num2 < biteHookDistance)
			{
				Hooked();
			}
			else if (num2 > fishInterestMaxDistance && fishing.FishChasing)
			{
				fishing.FishChasing = false;
				chasing = false;
				SetRandomTargetPosition(2.5f);
				moveSpeed = fishMoveSpeed;
			}
		}
	}

	private void TargetedHook()
	{
		targetLookAt = fishing.FishingHook.position;
		targetPosition = fishing.FishingHook.position;
		fishing.FishChasing = true;
		chasing = true;
		moveSpeed = targetHookMoveSpeed;
	}

	private void Hooked()
	{
		fishing.FishOn(this);
		state = StateMachine.State.Attacking;
		playerPosition = fishing.PlayerPosition;
	}

	public void Spooked()
	{
		if (chasing)
		{
			fishing.FishChasing = false;
			chasing = false;
		}
		SetRandomTargetPosition(10f);
		moveSpeed = 1f;
		rotationSpeed = 1f;
		base.transform.DOScale(0f, 2f);
		state = StateMachine.State.Fleeing;
	}

	private void SetRandomTargetPosition(float r)
	{
		targetPosition = originalCenterPoint + (Vector3)(Random.insideUnitCircle * r);
		targetLookAt = targetPosition;
		moveTimer = GameManager.GetInstance().CurrentTime + Random.Range(timeInterval.x, timeInterval.y);
	}
}
