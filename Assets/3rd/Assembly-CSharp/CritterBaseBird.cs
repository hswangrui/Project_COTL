using System;
using System.Collections.Generic;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;

public class CritterBaseBird : BaseMonoBehaviour
{
	public enum State
	{
		WaitingToArrive,
		FlyingIn,
		Idle,
		FlyingOut
	}

	public static List<CritterBaseBird> Birds = new List<CritterBaseBird>();

	public State CurrentState;

	private float RandomDelay;

	public Vector3 FlyOutPosition;

	private float Progress;

	public Vector3 TargetPosition;

	private Health health;

	private Vector3 PrevPosition;

	private float Angle;

	private float Duration;

	private float FlipTimer;

	public SkeletonAnimation bird;

	[HideInInspector]
	public SkeletonAnimationLODManager skeletonAnimationLODManager;

	private EventInstance loopingSoundInstance;

	private bool createdLoop;

	public bool ManualControl { get; set; }

	public Vector2 FlipTimerIntervals { get; set; } = new Vector2(3f, 8f);


	public float EatChance { get; set; } = 0.5f;


	private void OnEnable()
	{
		Birds.Add(this);
		Start();
	}

	private void OnDisable()
	{
		Birds.Remove(this);
	}

	private void OnBecameInvisible()
	{
		if (!ManualControl)
		{
			skeletonAnimationLODManager.DoUpdate = false;
		}
	}

	public void OnBecameVisible()
	{
		if (!ManualControl)
		{
			skeletonAnimationLODManager.DoUpdate = true;
		}
	}

	private void Awake()
	{
		skeletonAnimationLODManager = base.gameObject.AddComponent<SkeletonAnimationLODManager>();
		skeletonAnimationLODManager.Initialise(bird, true, true);
	}

	private void Start()
	{
		TargetPosition = base.transform.position;
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
		health = GetComponent<Health>();
		NewFlyOutPosition(UnityEngine.Random.Range(0, 360));
		RandomDelay = UnityEngine.Random.Range(0f, 240f) / 2f;
		Duration = UnityEngine.Random.Range(120f, 360f) / 2f;
		base.transform.position = FlyOutPosition;
		bird.state.SetAnimation(0, "FLY", true);
		FlipTimer = UnityEngine.Random.Range(0.5f, 2f);
		skeletonAnimationLODManager.DoUpdate = false;
	}

	private void OnDestroy()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void OnNewPhaseStarted()
	{
		if (TimeManager.CurrentPhase != DayPhase.Night || ManualControl)
		{
			return;
		}
		switch (CurrentState)
		{
		case State.WaitingToArrive:
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
			if (this != null)
			{
				base.gameObject.Recycle();
			}
			break;
		case State.FlyingIn:
		case State.Idle:
			RandomDelay = UnityEngine.Random.Range(0f, 3f);
			NewFlyOutPosition(UnityEngine.Random.Range(0, 360));
			FlyOut();
			break;
		}
	}

	private void CheckForPlayer()
	{
		if (!(PlayerFarming.Instance == null) && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) <= 2f)
		{
			NewFlyOutPosition(Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position));
			FlyOut();
		}
	}

	private void NewFlyOutPosition(float Angle)
	{
		float num = UnityEngine.Random.Range(6f, 10f);
		Angle *= (float)Math.PI / 180f;
		FlyOutPosition = new Vector3(num * Mathf.Cos(Angle), num * Mathf.Sin(Angle), -12f);
	}

	public void FlyOut()
	{
		if (!(bird == null) && bird.state != null)
		{
			AudioManager.Instance.PlayOneShot("event:/birds/bird_fly_away", base.gameObject);
			bird.state.SetAnimation(0, "FLY", true);
			Progress = 0f;
			CurrentState = State.FlyingOut;
		}
	}

	private void Update()
	{
		if (bird == null || bird.state == null)
		{
			return;
		}
		PrevPosition = base.transform.position;
		switch (CurrentState)
		{
		case State.WaitingToArrive:
			health.enabled = false;
			if ((RandomDelay -= Time.deltaTime) < 0f && !ManualControl)
			{
				skeletonAnimationLODManager.DoUpdate = true;
				CurrentState = State.FlyingIn;
			}
			break;
		case State.FlyingIn:
			if (Progress < 1f)
			{
				base.transform.position = Vector3.Lerp(TargetPosition + FlyOutPosition, TargetPosition, Progress += Time.deltaTime * 0.5f);
				health.enabled = Progress > 0.8f;
			}
			else
			{
				bird.state.SetAnimation(0, "IDLE", true);
				CurrentState = State.Idle;
			}
			break;
		case State.Idle:
			if ((FlipTimer -= Time.deltaTime) < 0f)
			{
				FlipTimer = UnityEngine.Random.Range(FlipTimerIntervals.x, FlipTimerIntervals.y);
				if (UnityEngine.Random.value <= 0.5f)
				{
					base.transform.localScale = new Vector3(base.transform.localScale.x * -1f, 1f, 1f);
				}
				if (UnityEngine.Random.value >= EatChance)
				{
					bird.state.SetAnimation(0, "IDLE", true);
				}
				else
				{
					bird.state.SetAnimation(0, "EAT", true);
				}
			}
			if ((Duration -= Time.deltaTime) < 0f && !ManualControl)
			{
				FlyOut();
			}
			else
			{
				CheckForPlayer();
			}
			break;
		case State.FlyingOut:
			if (Progress < 1f)
			{
				base.transform.position = Vector3.Lerp(TargetPosition, TargetPosition + FlyOutPosition, Mathf.SmoothStep(0f, 1f, Progress += Time.deltaTime * 0.6f));
				health.enabled = Progress <= 0.2f;
			}
			else
			{
				TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
				base.gameObject.Recycle();
			}
			break;
		}
		if (base.transform.position != PrevPosition && CurrentState != State.Idle)
		{
			Angle = Mathf.Repeat(Utils.GetAngle(PrevPosition, base.transform.position), 360f);
			base.transform.localScale = new Vector3((Angle > 90f && Angle < 270f) ? 1 : (-1), 1f, 1f);
		}
	}
}
