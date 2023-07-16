using System.Collections;
using UnityEngine;

public class FollowerTask_PrayPassive : FollowerTask
{
	public const int PRAYER_DURATION_GAME_MINUTES = 150;

	protected int _shrineID;

	public Structures_Shrine_Passive _shrine;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.PassivePray;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _shrine.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _shrineID;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public int PrayerID
	{
		get
		{
			return _brain.Info.ID;
		}
	}

	public override float Priorty
	{
		get
		{
			return 100f;
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
		if (FollowerRole == FollowerRole.Worshipper || FollowerRole == FollowerRole.Monk)
		{
			return PriorityCategory.Low;
		}
		return PriorityCategory.OverrideWorkPriority;
	}

	public FollowerTask_PrayPassive(int shrineID)
	{
		_shrineID = shrineID;
		_shrine = StructureManager.GetStructureByID<Structures_Shrine_Passive>(_shrineID);
	}

	protected override int GetSubTaskCode()
	{
		return _shrineID;
	}

	public override void ClaimReservations()
	{
		if (_shrine != null)
		{
			_shrine.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		if (_shrine != null)
		{
			_shrine.ReservedForTask = false;
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
			_progress += deltaGameTime * _brain.Info.ProductivityMultiplier;
			if (_progress >= GetDurationPerDevotion(_brain))
			{
				SetState(FollowerTaskState.Doing);
			}
		}
	}

	public float GetDurationPerDevotion(Follower forFollower)
	{
		return GetDurationPerDevotion(forFollower.Brain);
	}

	public float GetDurationPerDevotion(FollowerBrain followerBrain)
	{
		float num = 150f;
		if (StructureEffectManager.GetEffectAvailability(_shrineID, StructureEffectManager.EffectType.Shrine_DevotionEffeciency) == StructureEffectManager.State.Active)
		{
			num *= 0.8f;
		}
		num /= _shrine.DevotionSpeedMultiplier;
		return num / followerBrain.DevotionToGive;
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return _shrine.Data.Position + Vector3.down;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, _shrine.Data.FullyFueled ? "pray-flame" : "pray");
		_progress = 0f;
	}

	public override void OnIdleBegin(Follower follower)
	{
		base.OnIdleBegin(follower);
		follower.UIFollowerPrayingProgress.Show();
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, _shrine.Data.Position);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, _shrine.Data.FullyFueled ? "pray-flame" : "pray");
	}

	public override void OnDoingBegin(Follower follower)
	{
		base.OnDoingBegin(follower);
		follower.UIFollowerPrayingProgress.Flash();
		_brain.GetXP(1f);
		BuildingShrinePassive buildingShrinePassive = FindShrine();
		if (buildingShrinePassive == null)
		{
			DepositSoul(1);
		}
		else
		{
			follower.StartCoroutine(GiveDevotionRoutine(buildingShrinePassive, follower));
		}
		follower.TimedAnimation(_shrine.Data.FullyFueled ? "devotion/devotion-collect-flame" : "devotion/devotion-collect", 2.3f, delegate
		{
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

	protected IEnumerator GiveDevotionRoutine(BuildingShrinePassive shrine, Follower follower)
	{
		yield return new WaitForSeconds(0.2f);
		SoulCustomTarget.Create(shrine.ReceiveSoulPosition, follower.transform.position, Color.white, delegate
		{
			DepositSoul(1);
		}, 0.2f);
	}

	protected virtual void DepositSoul(int DevotionToGive)
	{
		float num = 1f;
		num += (float)_brain.Info.XPLevel;
		_shrine.SoulCount += Mathf.RoundToInt((float)DevotionToGive * num);
		SetState(FollowerTaskState.Idle);
		Complete();
		_shrine.SetFollowerPrayed();
	}

	protected BuildingShrinePassive FindShrine()
	{
		BuildingShrinePassive result = null;
		foreach (BuildingShrinePassive shrine in BuildingShrinePassive.Shrines)
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
		DepositSoul(1);
	}
}
