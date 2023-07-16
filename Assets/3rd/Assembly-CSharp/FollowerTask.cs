using System;
using UnityEngine;

public abstract class FollowerTask
{
	public delegate void FollowerTaskDelegate(FollowerTaskState oldState, FollowerTaskState newState);

	public float _priority;

	protected FollowerBrain _brain;

	protected FollowerTaskState _state;

	public FollowerTaskDelegate OnFollowerTaskStateChanged;

	protected Vector3? _currentDestination;

	public abstract FollowerTaskType Type { get; }

	public abstract FollowerLocation Location { get; }

	public virtual bool DisablePickUpInteraction
	{
		get
		{
			return false;
		}
	}

	public virtual bool BlockTaskChanges
	{
		get
		{
			return false;
		}
	}

	public virtual bool BlockSocial
	{
		get
		{
			return false;
		}
	}

	public virtual bool BlockSermon
	{
		get
		{
			return false;
		}
	}

	public virtual bool BlockReactTasks
	{
		get
		{
			return false;
		}
	}

	public virtual int UsingStructureID
	{
		get
		{
			return 0;
		}
	}

	public virtual bool ShouldSaveDestination
	{
		get
		{
			return true;
		}
	}

	public virtual bool BlockThoughts
	{
		get
		{
			return false;
		}
	}

	public virtual float Priorty
	{
		get
		{
			return 0f;
		}
	}

	public bool Initialized { get; private set; }

	public bool AnimateOutFromLocation { get; set; } = true;


	public FollowerTaskState State
	{
		get
		{
			return _state;
		}
	}

	public FollowerBrain Brain
	{
		get
		{
			return _brain;
		}
	}

	public FollowerTask_ChangeLocation ChangeLocationTask { get; private set; }

	public virtual PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		return PriorityCategory.Low;
	}

	public static bool RequiredFollowerLevel(FollowerRole FollowerRole, FollowerTaskType Type)
	{
		return true;
	}

	public FollowerTask()
	{
	}

	public int GetUniqueTaskCode()
	{
		int subTaskCode = GetSubTaskCode();
		return (int)Type * 1000000 + subTaskCode;
	}

	protected abstract int GetSubTaskCode();

	public virtual void ClaimReservations()
	{
	}

	public virtual void ReleaseReservations()
	{
	}

	public void Init(FollowerBrain brain)
	{
		_brain = brain;
		if (_brain.SavedFollowerTaskType == Type && _brain.SavedFollowerTaskLocation == Location)
		{
			Vector3 savedFollowerTaskDestination = _brain.SavedFollowerTaskDestination;
			ClearDestination();
			_currentDestination = savedFollowerTaskDestination;
		}
		Initialized = true;
	}

	public void Start()
	{
		if (_brain.Location == Location)
		{
			OnStart();
			return;
		}
		ChangeLocationTask = new FollowerTask_ChangeLocation(Location, Type, AnimateOutFromLocation);
		FollowerTask_ChangeLocation changeLocationTask = ChangeLocationTask;
		changeLocationTask.OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Combine(changeLocationTask.OnFollowerTaskStateChanged, new FollowerTaskDelegate(OnLocationTaskStateChanged));
		ChangeLocationTask.Init(_brain);
		Action<FollowerTask, FollowerTask> onTaskChanged = _brain.OnTaskChanged;
		if (onTaskChanged != null)
		{
			onTaskChanged(ChangeLocationTask, this);
		}
		ChangeLocationTask.Start();
	}

	protected virtual void OnStart()
	{
		SetState(FollowerTaskState.Idle);
	}

	public void StartAgain(Follower follower)
	{
		SetState(FollowerTaskState.Doing);
		OnDoingBegin(follower);
	}

	private void OnLocationTaskStateChanged(FollowerTaskState oldState, FollowerTaskState newState)
	{
		if (newState == FollowerTaskState.Done)
		{
			FollowerTask_ChangeLocation changeLocationTask = ChangeLocationTask;
			FollowerTask_ChangeLocation changeLocationTask2 = ChangeLocationTask;
			changeLocationTask2.OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Remove(changeLocationTask2.OnFollowerTaskStateChanged, new FollowerTaskDelegate(OnLocationTaskStateChanged));
			ChangeLocationTask = null;
			Action<FollowerTask, FollowerTask> onTaskChanged = _brain.OnTaskChanged;
			if (onTaskChanged != null)
			{
				onTaskChanged(this, changeLocationTask);
			}
			SetState(FollowerTaskState.WaitingForLocation);
		}
	}

	public void Arrive()
	{
		if (_currentDestination.HasValue)
		{
			_brain.LastPosition = _currentDestination.Value;
		}
		OnArrive();
	}

	protected virtual void OnArrive()
	{
		SetState(FollowerTaskState.Doing);
	}

	public void End()
	{
		if (_brain.CurrentOverrideTaskType == Type)
		{
			_brain.ClearPersonalOverrideTaskProvider();
		}
		OnEnd();
	}

	protected virtual void OnEnd()
	{
		SetState(FollowerTaskState.Finalising);
	}

	protected void Complete()
	{
		OnComplete();
		ClearDestination();
		SetState(FollowerTaskState.Done);
		_brain.ContinueToNextTask();
	}

	protected virtual void OnComplete()
	{
	}

	protected virtual void OnAbort()
	{
	}

	public void Abort()
	{
		OnAbort();
		ClearDestination();
		Complete();
	}

	public void Tick(float deltaGameTime)
	{
		if (State == FollowerTaskState.WaitingForLocation && _brain.Location == Location)
		{
			Start();
		}
		if (!TimeManager.PauseGameTime)
		{
			_brain.Stats.Rest += RestChange(deltaGameTime);
			_brain.Stats.Satiation += SatiationChange(deltaGameTime) * DifficultyManager.GetHungerDepletionMultiplier();
			_brain.Stats.Vomit += VomitChange(deltaGameTime);
			_brain.Stats.Social += SocialChange(deltaGameTime);
			if (_brain.Stats.Illness > 0f)
			{
				_brain.Stats.Illness += IllnessChange(deltaGameTime) * DifficultyManager.GetIllnessDepletionMultiplier();
			}
			if (_brain.Stats.Exhaustion > 0f)
			{
				_brain.Stats.Exhaustion += ExhaustionChange(deltaGameTime);
			}
			_brain.Stats.Reeducation += ReeducationChange(deltaGameTime) * DifficultyManager.GetDissenterDepletionMultiplier();
		}
		else if (SatiationChange(deltaGameTime) > 0f)
		{
			_brain.Stats.Satiation += SatiationChange(deltaGameTime) * DifficultyManager.GetHungerDepletionMultiplier();
		}
		TaskTick(deltaGameTime);
	}

	protected void SetState(FollowerTaskState state)
	{
		if (_state != state)
		{
			FollowerTaskState state2 = _state;
			_state = state;
			FollowerTaskDelegate onFollowerTaskStateChanged = OnFollowerTaskStateChanged;
			if (onFollowerTaskStateChanged != null)
			{
				onFollowerTaskStateChanged(state2, _state);
			}
		}
	}

	protected virtual float RestChange(float deltaGameTime)
	{
		return 0f - 100f * (deltaGameTime / 1200f);
	}

	protected virtual float VomitChange(float deltaGameTime)
	{
		if (_brain.Stats.Illness > 0f)
		{
			return 0f - 30f * (deltaGameTime / 320f);
		}
		return 0f;
	}

	protected virtual float SocialChange(float deltaGameTime)
	{
		return 0f - 100f * (deltaGameTime / 200f);
	}

	protected virtual float IllnessChange(float deltaGameTime)
	{
		return 100f * (deltaGameTime / 3600f);
	}

	protected virtual float ExhaustionChange(float deltaGameTime)
	{
		return 100f * (deltaGameTime / 900f);
	}

	protected virtual float ReeducationChange(float deltaGameTime)
	{
		float num = 100f * (deltaGameTime / 3600f);
		float num2 = 1f;
		num2 *= ((PlayerFarming.Instance != null && PlayerFarming.Location != FollowerLocation.Base) ? 0.7f : 1f);
		return num * num2;
	}

	protected virtual float SatiationChange(float deltaGameTime)
	{
		if (_brain.Info.CursedState != 0 && _brain.Info.CursedState != Thought.BecomeStarving)
		{
			return 0f;
		}
		if (FollowerBrainStats.Fasting)
		{
			return 0f;
		}
		float num = 100f * (deltaGameTime / 2400f);
		float num2 = 1f;
		num2 *= ((PlayerFarming.Instance != null && PlayerFarming.Location != FollowerLocation.Base) ? 0.7f : 1f);
		return 0f - num * num2;
	}

	protected abstract void TaskTick(float deltaGameTime);

	public virtual void ProgressTask()
	{
	}

	public void RecalculateDestination()
	{
		ClearDestination();
		if (State != 0 && State != FollowerTaskState.GoingTo)
		{
			SetState(FollowerTaskState.GoingTo);
			return;
		}
		FollowerTaskDelegate onFollowerTaskStateChanged = OnFollowerTaskStateChanged;
		if (onFollowerTaskStateChanged != null)
		{
			onFollowerTaskStateChanged(FollowerTaskState.None, FollowerTaskState.GoingTo);
		}
	}

	private bool IsNaN(Vector3 check)
	{
		if (!float.IsNaN(_currentDestination.Value.magnitude) && !float.IsNaN(_currentDestination.Value.x) && !float.IsNaN(_currentDestination.Value.y))
		{
			return float.IsNaN(_currentDestination.Value.z);
		}
		return true;
	}

	public Vector3 GetDestination(Follower follower)
	{
		if (_currentDestination.HasValue && IsNaN(_currentDestination.Value))
		{
			ClearDestination();
		}
		if (!_currentDestination.HasValue || _currentDestination == Vector3.zero)
		{
			_currentDestination = UpdateDestination(follower);
			if (IsNaN(_currentDestination.Value))
			{
				ClearDestination();
				if (!(follower == null))
				{
					return follower.transform.position;
				}
				return Vector3.zero;
			}
			if (ShouldSaveDestination)
			{
				_brain.SavedFollowerTaskType = Type;
				_brain.SavedFollowerTaskLocation = Location;
				_brain.SavedFollowerTaskDestination = _currentDestination.Value;
			}
		}
		return _currentDestination.Value;
	}

	public void ClearDestination()
	{
		_currentDestination = null;
		_brain.SavedFollowerTaskType = FollowerTaskType.None;
		_brain.SavedFollowerTaskLocation = FollowerLocation.None;
		_brain.SavedFollowerTaskDestination = Vector3.zero;
	}

	public float GetDistanceToDestination()
	{
		float result = 0f;
		if (_currentDestination.HasValue)
		{
			result = Vector3.Distance(_brain.LastPosition, _currentDestination.Value);
		}
		return result;
	}

	protected abstract Vector3 UpdateDestination(Follower follower);

	public virtual void Setup(Follower follower)
	{
	}

	public virtual void OnIdleBegin(Follower follower)
	{
		follower.ClearPath();
		follower.State.CURRENT_STATE = StateMachine.State.Idle;
	}

	public virtual void OnIdleEnd(Follower follower)
	{
	}

	public virtual void OnGoingToBegin(Follower follower)
	{
		Vector3 destination = GetDestination(follower);
		if (Vector3.Distance(follower.transform.position, destination) > 0.3f)
		{
			follower.GoTo(destination, delegate
			{
				Arrive();
			});
		}
		else
		{
			follower.ClearPath();
			Arrive();
		}
	}

	public virtual void OnGoingToEnd(Follower follower)
	{
	}

	public virtual void OnDoingBegin(Follower follower)
	{
	}

	public virtual void OnDoingEnd(Follower follower)
	{
	}

	public virtual void OnFinaliseBegin(Follower follower)
	{
		Complete();
	}

	public virtual void OnFinaliseEnd(Follower follower)
	{
	}

	public virtual void Cleanup(Follower follower)
	{
		ReleaseReservations();
	}

	public virtual void SimSetup(SimFollower simFollower)
	{
	}

	public virtual void SimIdleBegin(SimFollower simFollower)
	{
	}

	public virtual void SimIdleEnd(SimFollower simFollower)
	{
	}

	public virtual void SimGoingToBegin(SimFollower simFollower)
	{
		simFollower.TravelTimeGameMinutes = 10f;
	}

	public virtual void SimGoingToEnd(SimFollower simFollower)
	{
	}

	public virtual void SimDoingBegin(SimFollower simFollower)
	{
	}

	public virtual void SimDoingEnd(SimFollower simFollower)
	{
	}

	public virtual void SimFinaliseBegin(SimFollower simFollower)
	{
		Complete();
	}

	public virtual void SimFinaliseEnd(SimFollower simFollower)
	{
	}

	public virtual void SimCleanup(SimFollower simFollower)
	{
		ReleaseReservations();
	}

	public virtual string ToDebugString()
	{
		return string.Format("{0}, State.{1}", Type, _state);
	}

	public static FollowerTaskType GetFollowerTaskFromRole(FollowerRole role)
	{
		switch (role)
		{
		case FollowerRole.Builder:
			return FollowerTaskType.Build;
		case FollowerRole.Chef:
			return FollowerTaskType.Cook;
		case FollowerRole.Farmer:
			return FollowerTaskType.Farm;
		case FollowerRole.Forager:
			return FollowerTaskType.Forage;
		case FollowerRole.Janitor:
			return FollowerTaskType.Janitor;
		case FollowerRole.Lumberjack:
			return FollowerTaskType.ChopTrees;
		case FollowerRole.Refiner:
			return FollowerTaskType.Refinery;
		case FollowerRole.StoneMiner:
			return FollowerTaskType.ClearRubble;
		case FollowerRole.Worshipper:
			return FollowerTaskType.Pray;
		case FollowerRole.Undertaker:
			return FollowerTaskType.Undertaker;
		default:
			return FollowerTaskType.None;
		}
	}
}
