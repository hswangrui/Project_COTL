using System;
using I2.Loc;

[Serializable]
public class Objectives_FindFollower : ObjectivesData
{
	[Serializable]
	public class FinalizedData_FindFollower : ObjectivesDataFinalized
	{
		public FollowerLocation TargetLocation;

		public string TargetFollowerName;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives.FindFollower, TargetFollowerName, LocalizationManager.GetTranslation(string.Format("NAMES/Places/{0}", TargetLocation)));
		}
	}

	public FollowerLocation TargetLocation;

	public string FollowerSkin;

	public int FollowerColour;

	public int FollowerVariant;

	public string TargetFollowerName;

	public int ObjectiveVariant;

	public override string Text
	{
		get
		{
			return string.Format(ScriptLocalization.Objectives.FindFollower, TargetFollowerName, LocalizationManager.GetTranslation(string.Format("NAMES/Places/{0}", TargetLocation)));
		}
	}

	public Objectives_FindFollower()
	{
	}

	public Objectives_FindFollower(string groupId, FollowerLocation targetLocation, string followerSkin, int followerColour, int followerVariant, string targetFollowerName, int objectiveVariant, float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.FIND_FOLLOWER;
		TargetLocation = targetLocation;
		FollowerSkin = followerSkin;
		FollowerColour = followerColour;
		FollowerVariant = followerVariant;
		TargetFollowerName = targetFollowerName;
		ObjectiveVariant = objectiveVariant;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_FindFollower
		{
			GroupId = GroupId,
			Index = Index,
			TargetLocation = TargetLocation,
			TargetFollowerName = TargetFollowerName,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		base.CheckComplete();
		return IsComplete;
	}
}
