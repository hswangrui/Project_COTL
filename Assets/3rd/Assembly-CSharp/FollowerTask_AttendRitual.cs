using System.Collections;
using UnityEngine;

public class FollowerTask_AttendRitual : FollowerTask
{
	private Coroutine hoodOnRoutine;

	private Follower follower;

	public FollowerOutfitType SpecialOutfit = FollowerOutfitType.Custom;

	public bool DestinationIsCircle = true;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.AttendTeaching;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Church;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public override bool BlockSermon
	{
		get
		{
			return false;
		}
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (PlayerFarming.Location != 0)
		{
			End();
		}
		else if (_brain.Location != Location)
		{
			Start();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (DestinationIsCircle)
		{
			return ChurchFollowerManager.Instance.GetCirclePosition(_brain);
		}
		return ChurchFollowerManager.Instance.GetAudienceMemberPosition(_brain);
	}

	public void FormCircle()
	{
		DestinationIsCircle = true;
		RecalculateDestination();
	}

	public void HoodOff()
	{
		follower.HoodOff();
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		SetState(FollowerTaskState.GoingTo);
		_brain.InRitual = true;
		this.follower = follower;
	}

	public override void OnDoingBegin(Follower follower)
	{
		if ((bool)follower)
		{
			follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			follower.FacePosition(ChurchFollowerManager.Instance.RitualCenterPosition.transform.position);
			hoodOnRoutine = GameManager.GetInstance().StartCoroutine(DelayedHood(follower));
		}
	}

	public override void OnGoingToBegin(Follower follower)
	{
		base.OnGoingToBegin(follower);
	}

	private IEnumerator DelayedHood(Follower follower)
	{
		yield return new WaitForSeconds(0.35f);
		if ((bool)follower)
		{
			follower.HoodOn("pray", false);
		}
	}

	public void WorshipTentacle()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetBodyAnimation("worship", true);
		}
	}

	public void Cheer()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetBodyAnimation("cheer", true);
		}
	}

	public void Boo()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetBodyAnimation("boo", true);
		}
	}

	public void Dance()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetBodyAnimation("dance", true);
		}
	}

	public void DanceBrainwashed()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetBodyAnimation("dance-mushroom", true);
		}
	}

	public void Pray(int loopCount = 1)
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.Spine.randomOffset = true;
			for (int i = 0; i < loopCount; i++)
			{
				follower.AddBodyAnimation("devotion/devotion-start", false, 0f);
				follower.AddBodyAnimation((Random.Range(0, 2) == 0) ? "devotion/devotion-collect" : "devotion/devotion-collect2", false, 0f);
				follower.AddBodyAnimation("pray", true, 0f);
			}
		}
	}

	public void Pray2()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetBodyAnimation("devotion/devotion-start", false);
			follower.AddBodyAnimation("devotion/devotion-waiting", false, 0f);
			follower.AddBodyAnimation((Random.Range(0, 2) == 0) ? "devotion/devotion-collect" : "devotion/devotion-collect2", false, 0f);
			follower.AddBodyAnimation("pray", true, 0f);
		}
	}

	public void Pray3()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.AddBodyAnimation("pray", true, 0f);
		}
	}

	public void Idle()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetBodyAnimation("idle", true);
		}
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		base.OnFinaliseBegin(follower);
	}

	protected override void OnComplete()
	{
		base.OnComplete();
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		_brain.InRitual = false;
		if ((bool)follower)
		{
			follower.State.CURRENT_STATE = StateMachine.State.Idle;
			follower.SetOutfit(follower.Brain.Info.Outfit, false);
			follower.SetBodyAnimation("idle", true);
			if (SpecialOutfit != FollowerOutfitType.Custom)
			{
				follower.Outfit.SetOutfit(follower.Spine, SpecialOutfit, follower.Brain.Info.Necklace, false);
			}
		}
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		_brain.InRitual = false;
		base.SimCleanup(simFollower);
	}
}
