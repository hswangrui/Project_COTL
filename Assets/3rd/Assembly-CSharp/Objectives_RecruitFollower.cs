using System;
using I2.Loc;

[Serializable]
public class Objectives_RecruitFollower : ObjectivesData
{
	[Serializable]
	public class FinalizedData_RecruitFollower : ObjectivesDataFinalized
	{
		public int Count;

		public int FollowerCount;

		public override string GetText()
		{
			if (Count == 1)
			{
				return ScriptLocalization.Objectives_Custom.GetNewFollowersFromDungeon;
			}
			return string.Format(ScriptLocalization.Objectives_RecruitFollower.Plural, Count, FollowerCount, Count);
		}
	}

	public int Count;

	public override string Text
	{
		get
		{
			if (Count == 1)
			{
				return ScriptLocalization.Objectives_Custom.GetNewFollowersFromDungeon;
			}
			int count = DataManager.Instance.Followers.Count;
			return string.Format(ScriptLocalization.Objectives_RecruitFollower.Plural, Count, count, Count);
		}
	}

	public Objectives_RecruitFollower()
	{
	}

	public Objectives_RecruitFollower(string groupId, int count = 1)
		: base(groupId)
	{
		Type = Objectives.TYPES.RECRUIT_FOLLOWER;
		Count = count;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_RecruitFollower
		{
			GroupId = GroupId,
			Index = Index,
			Count = Count,
			FollowerCount = DataManager.Instance.Followers.Count,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		return DataManager.Instance.Followers.Count >= Count;
	}
}
