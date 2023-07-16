using Spine.Unity;
using UnityEngine;

public class PlayerStealth : BaseMonoBehaviour
{
	private PlayerFarming playerFarming;

	private PlayerController playerController;

	private UnitObject unitObject;

	private StateMachine state;

	private EnemyStealth ClosestStealthCover;

	private Health health;

	public SkeletonAnimation Spine;

	private float CheckDist;

	private float Distance;

	public float StealthSpeed = 3.5f;

	private void Start()
	{
		playerFarming = GetComponent<PlayerFarming>();
		playerController = GetComponent<PlayerController>();
		unitObject = GetComponent<UnitObject>();
		state = GetComponent<StateMachine>();
		health = GetComponent<Health>();
	}

	private void Update()
	{
		if (Time.timeScale <= 0f || playerFarming.GoToAndStopping)
		{
			return;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
		case StateMachine.State.Moving:
		{
			foreach (EnemyStealth enemyStealth in EnemyStealth.EnemyStealths)
			{
				if (Vector3.Distance(enemyStealth.transform.position, base.transform.position) < enemyStealth.EnterRadius && enemyStealth.health.Unaware)
				{
					state.CURRENT_STATE = StateMachine.State.Stealth;
				}
			}
			break;
		}
		case StateMachine.State.Stealth:
			if (Mathf.Abs(playerController.xDir) > 0.3f || Mathf.Abs(playerController.yDir) > 0.3f)
			{
				if (Spine.AnimationName != "run-crouched")
				{
					Spine.AnimationState.SetAnimation(0, "run-crouched", true);
				}
				playerController.forceDir = Utils.GetAngle(Vector3.zero, new Vector3(playerController.xDir, playerController.yDir));
				state.facingAngle = Utils.GetAngle(base.transform.position, base.transform.position + new Vector3(unitObject.vx, unitObject.vy));
				playerController.speed += (StealthSpeed - playerController.speed) / 3f * GameManager.DeltaTime;
			}
			else
			{
				if (Spine.AnimationName != "idle-crouched")
				{
					Spine.AnimationState.SetAnimation(0, "idle-crouched", true);
				}
				playerController.speed += (0f - playerController.speed) / 3f * GameManager.DeltaTime;
			}
			ClosestStealthCover = null;
			Distance = 2.1474836E+09f;
			foreach (EnemyStealth enemyStealth2 in EnemyStealth.EnemyStealths)
			{
				CheckDist = Vector3.Distance(enemyStealth2.transform.position, base.transform.position);
				if (!enemyStealth2.health.Unaware)
				{
					ClosestStealthCover = null;
					break;
				}
				if (CheckDist < Distance)
				{
					ClosestStealthCover = enemyStealth2;
					Distance = CheckDist;
				}
			}
			if (!health.InStealthCover && (ClosestStealthCover == null || Distance > ClosestStealthCover.ExitRadius))
			{
				ClosestStealthCover = null;
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
	}
}
