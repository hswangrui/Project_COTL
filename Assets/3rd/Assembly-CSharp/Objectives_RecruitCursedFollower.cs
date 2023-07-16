using System;
using I2.Loc;

[Serializable]
public class Objectives_RecruitCursedFollower : ObjectivesData
{
	[Serializable]
	public class FinalizedData_RecruitCursedFollower : ObjectivesDataFinalized
	{
		public Thought CursedState;

		public int Target;

		public int Count;

		public override string GetText()
		{
			return string.Format("{0} {1}/{2}", LocalizationManager.GetTranslation(string.Format("Objectives/RecruitCursedFollower/{0}", CursedState)), Count, Target);
		}
	}

	public Thought CursedState;

	public int Target;

	public int Count;

	public override string Text
	{
		get
		{
			return string.Format(LocalizationManager.GetTranslation(string.Format("Objectives/RecruitCursedFollower/{0}", CursedState)) + " ({0}/{1})", Count, Target);
		}
	}

	public Objectives_RecruitCursedFollower()
	{
	}

	public Objectives_RecruitCursedFollower(string groupId, Thought cursedState, int target = 1, float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.RECRUIT_CURSED_FOLLOWER;
		CursedState = cursedState;
		Target = target;
		Count = 0;
	}

	public override void Init(bool initialAssigning)
	{
		if (!initialised)
		{
			FollowerRecruit.OnFollowerRecruited = (FollowerRecruit.FollowerEventDelegate)Delegate.Combine(FollowerRecruit.OnFollowerRecruited, new FollowerRecruit.FollowerEventDelegate(OnFollowerRecruited));
		}
		base.Init(initialAssigning);
		if (initialAssigning)
		{
			Count = 0;
		}
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_RecruitCursedFollower
		{
			GroupId = GroupId,
			Index = Index,
			CursedState = CursedState,
			Target = Target,
			Count = Count,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void OnFollowerRecruited(FollowerInfo info)
	{
		if (info.CursedState == CursedState || info.StartingCursedState == CursedState)
		{
			Count++;
		}
		ObjectiveManager.CheckObjectives(Objectives.TYPES.RECRUIT_CURSED_FOLLOWER);
	}

	protected override bool CheckComplete()
	{
		return Count >= Target;
	}

	public override void Complete()
	{
		base.Complete();
		FollowerRecruit.OnFollowerRecruited = (FollowerRecruit.FollowerEventDelegate)Delegate.Remove(FollowerRecruit.OnFollowerRecruited, new FollowerRecruit.FollowerEventDelegate(OnFollowerRecruited));
	}

	public override void Failed()
	{
		base.Failed();
		FollowerRecruit.OnFollowerRecruited = (FollowerRecruit.FollowerEventDelegate)Delegate.Remove(FollowerRecruit.OnFollowerRecruited, new FollowerRecruit.FollowerEventDelegate(OnFollowerRecruited));
	}
}
