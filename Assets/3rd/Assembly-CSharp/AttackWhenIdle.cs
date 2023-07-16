using UnityEngine;

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Health))]
public class AttackWhenIdle : BaseMonoBehaviour
{
	public Bullet bullet;

	public float Damage = 1f;

	public float AttackRange = 5f;

	private StateMachine state;

	private Health target;

	private Health health;

	public bool ShootThroughWalls;

	public LayerMask layerToCheck;

	private int currentBurst;

	public int bursts = 1;

	public int burstsInterval = 1;

	public int attackAnticipation = 30;

	public int attackDuration = 60;

	public int attackInterval = 120;

	private int timer;

	private float testDistance;

	private float targetDistance;

	private void Start()
	{
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
		timer = attackInterval;
	}

	private void Update()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (++timer <= attackInterval)
			{
				break;
			}
			target = null;
			targetDistance = float.MaxValue;
			foreach (Health allUnit in Health.allUnits)
			{
				testDistance = Vector2.Distance(base.transform.position, allUnit.transform.position);
				if (allUnit.team != health.team && testDistance < AttackRange && testDistance < targetDistance && (ShootThroughWalls || (!ShootThroughWalls && CheckLineOfSight(allUnit.transform.position))))
				{
					target = allUnit;
					targetDistance = testDistance;
				}
			}
			if (target != null)
			{
				currentBurst = 0;
				state.facingAngle = Utils.GetAngle(base.transform.position, target.transform.position);
				state.CURRENT_STATE = StateMachine.State.Attacking;
				timer = 0;
			}
			break;
		case StateMachine.State.Attacking:
			if (timer == attackAnticipation + ((bursts != 1) ? (currentBurst * burstsInterval) : 0) && (bursts == 1 || (bursts != 1 && currentBurst < bursts)))
			{
				if (target != null)
				{
					Bullet obj = Object.Instantiate(bullet, base.transform.position, Quaternion.identity, base.transform.parent);
					obj.target = target;
					obj.Damage = Damage;
					SimpleAnimator componentInChildren = GetComponentInChildren<SimpleAnimator>();
					if (componentInChildren != null)
					{
						componentInChildren.SetScale(0.5f, 1.2f);
					}
				}
				currentBurst++;
			}
			if (++timer > attackDuration)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				timer = 0;
			}
			break;
		}
	}

	private bool CheckLineOfSight(Vector3 pointToCheck)
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, pointToCheck - base.transform.position, testDistance, layerToCheck);
		if (raycastHit2D.collider != null)
		{
			Debug.DrawRay(base.transform.position, (raycastHit2D.collider.transform.position - base.transform.position) * Vector2.Distance(base.transform.position, raycastHit2D.collider.transform.position), Color.yellow);
			return false;
		}
		Debug.DrawRay(base.transform.position, pointToCheck - base.transform.position, Color.green);
		return true;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, AttackRange, Color.red);
	}
}
