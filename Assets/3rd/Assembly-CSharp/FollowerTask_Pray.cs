using System.Collections;
using UnityEngine;

public class FollowerTask_Pray : FollowerTask
{
	protected int _shrineID;

	public Structures_Shrine _shrine;

	private Follower follower;

	private bool recalculatedPosition = true;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Pray;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _shrineID;
		}
	}

	public int PrayerID
	{
		get
		{
			return _brain.Info.ID;
		}
	}

	public int PreferredCircleIndex { get; set; } = -1;


	public new FollowerBrain Brain
	{
		get
		{
			return _brain;
		}
	}

	public override float Priorty
	{
		get
		{
			return 1f;
		}
	}

	private float _progress
	{
		get
		{
			return _brain.Info.PrayProgress;
		}
		set
		{
			_brain.Info.PrayProgress = value;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.Worshipper)
		{
			return PriorityCategory.WorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_Pray(int shrineID)
	{
		_shrineID = shrineID;
		_shrine = StructureManager.GetStructureByID<Structures_Shrine>(_shrineID);
	}

	protected override int GetSubTaskCode()
	{
		return _shrineID;
	}

	public override void ClaimReservations()
	{
		if (_shrine != null)
		{
			_shrine.AddPrayer(this);
		}
	}

	public override void ReleaseReservations()
	{
		if (_shrine != null)
		{
			_shrine.RemovePrayer(this);
		}
	}

	protected override void OnStart()
	{
		ClearDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		SetState(FollowerTaskState.Idle);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_state == FollowerTaskState.Idle)
		{
			_progress += deltaGameTime;
			if (_progress >= GetDurationPerDevotion(_brain))
			{
				SetState(FollowerTaskState.Doing);
				_progress = 0f;
			}
			if (follower != null && _shrine != null && _shrine.GetPrayerPosition(follower.Brain) == _shrine.Data.Position)
			{
				SetState(FollowerTaskState.GoingTo);
			}
		}
		if (follower != null)
		{
			follower.FacePosition(_shrine.Data.Position);
		}
		if (_shrine.SoulCount >= _shrine.SoulMax)
		{
			Complete();
		}
	}

	public float GetDurationPerDevotion(Follower forFollower)
	{
		return GetDurationPerDevotion(forFollower.Brain);
	}

	public float GetDurationPerDevotion(FollowerBrain brain)
	{
		return 60f / _shrine.DevotionSpeedMultiplier / brain.DevotionToGive;
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return _shrine.GetPrayerPosition(follower.Brain);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, _shrine.Data.FullyFueled ? "pray-flame" : "pray");
		this.follower = follower;
		if (_brain != null && _shrine != null && _brain.LastPosition == _shrine.Data.Position)
		{
			follower.Seeker.CancelCurrentPathRequest();
			ClearDestination();
			SetState(FollowerTaskState.Doing);
			follower.transform.position = UpdateDestination(follower);
		}
	}

	public override void OnIdleBegin(Follower follower)
	{
		base.OnIdleBegin(follower);
		this.follower = follower;
		follower.UIFollowerPrayingProgress.Show();
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, _shrine.Data.Position);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, _shrine.Data.FullyFueled ? "pray-flame" : "pray");
	}

	public override void OnDoingBegin(Follower follower)
	{
		base.OnDoingBegin(follower);
		this.follower = follower;
		follower.UIFollowerPrayingProgress.Flash();
		_brain.GetXP(1f);
		BuildingShrine buildingShrine = FindShrine();
		if (buildingShrine == null)
		{
			int SoulsToDeposit = 0;
			SoulsToDeposit += _brain.Info.XPLevel;
			while (--SoulsToDeposit >= 0)
			{
				SoulCustomTarget.Create(buildingShrine.ReceiveSoulPosition, follower.transform.position, Color.white, delegate
				{
					DepositSoul(1, SoulsToDeposit == 0);
				}, 0.2f);
			}
		}
		else
		{
			follower.StartCoroutine(GiveDevotionRoutine(buildingShrine, follower));
		}
		follower.TimedAnimation(_shrine.Data.FullyFueled ? "devotion/devotion-collect-flame" : "devotion/devotion-collect", 1f, delegate
		{
			SetState(FollowerTaskState.Idle);
			follower.State.CURRENT_STATE = StateMachine.State.Idle;
		});
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		follower.UIFollowerPrayingProgress.Hide();
		base.OnFinaliseBegin(follower);
	}

	public override void Cleanup(Follower follower)
	{
		follower.UIFollowerPrayingProgress.Hide();
		UndoStateAnimationChanges(follower);
		base.Cleanup(follower);
	}

	protected IEnumerator GiveDevotionRoutine(BuildingShrine shrine, Follower follower)
	{
		yield return new WaitForSeconds(0.2f);
		int SoulsToDeposit = 0;
		SoulsToDeposit += _brain.Info.XPLevel;
		while (--SoulsToDeposit >= 0)
		{
			SoulCustomTarget.Create(shrine.ReceiveSoulPosition, follower.transform.position, Color.white, delegate
			{
				DepositSoul(1, SoulsToDeposit == 0);
			}, 0.2f);
			yield return new WaitForSeconds(0.1f);
		}
	}

	protected virtual void DepositSoul(int DevotionToGive, bool SetIdle)
	{
		_shrine.SoulCount += DevotionToGive;
		if (SetIdle)
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	protected BuildingShrine FindShrine()
	{
		BuildingShrine result = null;
		foreach (BuildingShrine shrine in BuildingShrine.Shrines)
		{
			if (shrine.Structure.Structure_Info.ID == _shrineID)
			{
				result = shrine;
				break;
			}
		}
		return result;
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		int num = 0;
		num += _brain.Info.XPLevel;
		while (--num >= 0)
		{
			DepositSoul(1, num == 0);
		}
	}
}
