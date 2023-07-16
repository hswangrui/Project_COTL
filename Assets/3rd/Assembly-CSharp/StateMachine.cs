using System;
using UnityEngine;

public class StateMachine : BaseMonoBehaviour
{
	public enum State
	{
		Idle,
		Moving,
		Attacking,
		Defending,
		SignPostAttack,
		RecoverFromAttack,
		AimDodge,
		Dodging,
		Fleeing,
		Inventory,
		Map,
		WeaponSelect,
		CustomAction0,
		InActive,
		RaiseAlarm,
		Casting,
		TimedAction,
		Worshipping,
		Sleeping,
		BeingCarried,
		HitThrown,
		HitLeft,
		HitRight,
		HitRecover,
		Teleporting,
		SignPostCounterAttack,
		RecoverFromCounterAttack,
		Charging,
		Vulnerable,
		Converting,
		Unconverted,
		FoundItem,
		Dieing,
		Dead,
		Building,
		Respawning,
		AwaitRecruit,
		PickedUp,
		SacrificeRecruit,
		Recruited,
		Dancing,
		SpawnIn,
		SpawnOut,
		CrowdWorship,
		Grapple,
		DashAcrossIsland,
		ChargingHeavyAttack,
		Elevator,
		Grabbed,
		CustomAnimation,
		Preach,
		Stealth,
		GameOver,
		KnockBack,
		Aiming,
		Meditate,
		Resurrecting,
		Idle_CarryingBody,
		Moving_CarryingBody,
		Heal,
		Reeling,
		TiedToAltar,
		FinalGameOver
	}

	public delegate void StateChange(State NewState, State PrevState);

	public bool IsPlayer;

	public StateChange OnStateChange;

	[SerializeField]
	private State currentstate;

	public float facingAngle;

	public float LookAngle;

	[HideInInspector]
	public bool isDefending;

	[HideInInspector]
	public float Timer;

	public State CURRENT_STATE
	{
		get
		{
			return currentstate;
		}
		set
		{
			if (!LockStateChanges)
			{
				Timer = 0f;
				if (OnStateChange != null)
				{
					OnStateChange(value, currentstate);
				}
				currentstate = value;
				if (IsPlayer)
				{
					Debug.Log(value);
				}
			}
		}
	}

	public bool LockStateChanges { get; set; }

	public void SmoothFacingAngle(float Angle, float Easing)
	{
		facingAngle += Mathf.Atan2(Mathf.Sin((Angle - facingAngle) * ((float)Math.PI / 180f)), Mathf.Cos((Angle - facingAngle) * ((float)Math.PI / 180f))) * 57.29578f / Easing * GameManager.DeltaTime;
	}

	private void LateUpdate()
	{
		facingAngle = Mathf.Repeat(facingAngle, 360f);
		LookAngle = Mathf.Repeat(LookAngle, 360f);
	}

	private void OnDrawGizmos()
	{
		Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos(facingAngle * ((float)Math.PI / 180f)), 2f * Mathf.Sin(facingAngle * ((float)Math.PI / 180f))), Color.blue);
		Utils.DrawLine(base.transform.position, base.transform.position + new Vector3(2f * Mathf.Cos(LookAngle * ((float)Math.PI / 180f)), 2f * Mathf.Sin(LookAngle * ((float)Math.PI / 180f))), Color.green);
	}

	private void FacingToLook()
	{
		facingAngle = LookAngle;
	}

	private void LookToFacing()
	{
		LookAngle = facingAngle;
	}
}
