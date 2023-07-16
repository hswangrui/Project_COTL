using System;
using I2.Loc;

[Serializable]
public class Objectives_BedRest : ObjectivesData
{
	[Serializable]
	public class FinalizedData_BedRest : ObjectivesDataFinalized
	{
		public string FollowerName;

		public override string GetText()
		{
			return string.Format(LocalizationManager.GetTranslation("Objectives/SendFollowerToBedRest"), FollowerName);
		}
	}

	public string FollowerName;

	public override string Text
	{
		get
		{
			return string.Format(LocalizationManager.GetTranslation("Objectives/SendFollowerToBedRest"), FollowerName);
		}
	}

	public Objectives_BedRest()
	{
	}

	public Objectives_BedRest(string groupId, string followerName)
		: base(groupId)
	{
		Type = Objectives.TYPES.SEND_FOLLOWER_BED_REST;
		FollowerName = followerName;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_BedRest
		{
			GroupId = GroupId,
			Index = Index,
			FollowerName = FollowerName,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		return true;
	}
}
