using System;
using I2.Loc;

[Serializable]
public class Objective_FindRelic : ObjectivesData
{
	[Serializable]
	public class FinalizedData_FindRelic : ObjectivesDataFinalized
	{
		public FollowerLocation TargetLocation;

		public RelicType RelicType;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives.FindRelic, RelicData.GetTitleLocalisation(RelicType), LocalizationManager.GetTranslation(string.Format("NAMES/Places/{0}", TargetLocation)));
		}
	}

	public FollowerLocation TargetLocation;

	public RelicType RelicType;

	public override string Text
	{
		get
		{
			return string.Format(ScriptLocalization.Objectives.FindRelic, RelicData.GetTitleLocalisation(RelicType), LocalizationManager.GetTranslation(string.Format("NAMES/Places/{0}", TargetLocation)));
		}
	}

	public Objective_FindRelic()
	{
	}

	public Objective_FindRelic(string groupId, FollowerLocation targetLocation, RelicType relicType, float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.FIND_RELIC;
		TargetLocation = targetLocation;
		RelicType = relicType;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_FindRelic
		{
			GroupId = GroupId,
			Index = Index,
			TargetLocation = TargetLocation,
			RelicType = RelicType,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		base.CheckComplete();
		return IsComplete;
	}
}
