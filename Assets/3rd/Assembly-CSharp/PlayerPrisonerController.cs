using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerPrisonerController : BaseMonoBehaviour
{
	public float Speed;

	public float MaxSpeed = 2f;

	public float Acceleration = 0.5f;

	public float TurnSpeed = 7f;

	public GameObject CameraBone;

	private StateMachine state;

	private UnitObject unitObject;

	private float forceDir;

	private float xDir;

	private float yDir;

	public SkeletonAnimation Spine;

	public static float MinInputForMovement = 0.3f;

	public static PlayerPrisonerController Instance;

	public bool GoToAndStopping;

	private StateMachine.State GoToAndStopCompleteState;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		state = GetComponent<StateMachine>();
		unitObject = GetComponent<UnitObject>();
		Spine.AnimationState.Event += HandleEvent;
	}

	private void HandleEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "footsteps")
		{
			AudioManager.Instance.PlayFootstepPlayer(base.transform.position);
		}
	}

	private void OnDisable()
	{
		Spine.AnimationState.Event -= HandleEvent;
	}

	private void OnEnable()
	{
		if (state == null)
		{
			state = GetComponent<StateMachine>();
		}
		if (unitObject == null)
		{
			unitObject = GetComponent<UnitObject>();
		}
	}

	public UnitObject GoToAndStop(Vector3 Position, StateMachine.State state = StateMachine.State.Idle, Action callback = null)
	{
		xDir = (yDir = (unitObject.vx = (unitObject.vy = 0f)));
		GoToAndStopping = true;
		unitObject.givePath(Position);
		UnitObject obj = unitObject;
		obj.EndOfPath = (Action)Delegate.Combine(obj.EndOfPath, new Action(EndOfPath));
		UnitObject obj2 = unitObject;
		obj2.EndOfPath = (Action)Delegate.Combine(obj2.EndOfPath, callback);
		unitObject.speed = unitObject.maxSpeed;
		GoToAndStopCompleteState = state;
		return unitObject;
	}

	public void EndOfPath()
	{
		Debug.Log("End go to and stopping");
		GoToAndStopping = false;
		UnitObject obj = unitObject;
		obj.EndOfPath = (Action)Delegate.Remove(obj.EndOfPath, new Action(EndOfPath));
		state.CURRENT_STATE = GoToAndStopCompleteState;
		Speed = 0f;
		unitObject.vx = (unitObject.vy = 0f);
	}

	private void Update()
	{
		Shader.SetGlobalVector("_PlayerPosition", base.gameObject.transform.position);
		if (!GoToAndStopping)
		{
			xDir = InputManager.Gameplay.GetHorizontalAxis();
			yDir = InputManager.Gameplay.GetVerticalAxis();
			if (state.CURRENT_STATE == StateMachine.State.Moving)
			{
				Speed *= Mathf.Clamp01(new Vector2(xDir, yDir).magnitude);
			}
			Speed = Mathf.Max(Speed, 0f);
			unitObject.vx = Speed * Mathf.Cos(forceDir * ((float)Math.PI / 180f));
			unitObject.vy = Speed * Mathf.Sin(forceDir * ((float)Math.PI / 180f));
		}
		else
		{
			xDir = (yDir = 0f);
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			Speed += (0f - Speed) / 3f * GameManager.DeltaTime;
			if (Mathf.Abs(xDir) > MinInputForMovement || Mathf.Abs(yDir) > MinInputForMovement)
			{
				state.CURRENT_STATE = StateMachine.State.Moving;
			}
			break;
		case StateMachine.State.CustomAnimation:
			Speed = 0f;
			xDir = (yDir = 0f);
			break;
		case StateMachine.State.Moving:
			if (GoToAndStopping || Time.timeScale == 0f)
			{
				break;
			}
			if (Mathf.Abs(xDir) <= MinInputForMovement && Mathf.Abs(yDir) <= MinInputForMovement)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
				break;
			}
			forceDir = Utils.GetAngle(Vector3.zero, new Vector3(xDir, yDir));
			if (unitObject.vx != 0f || unitObject.vy != 0f)
			{
				state.facingAngle = Utils.GetAngle(base.transform.position, base.transform.position + new Vector3(unitObject.vx, unitObject.vy));
			}
			state.LookAngle = state.facingAngle;
			Speed += (MaxSpeed - Speed) / 3f * GameManager.DeltaTime;
			break;
		}
	}
}
