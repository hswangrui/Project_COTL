using System;
using I2.Loc;

[Serializable]
public class Objectives_UseRelic : ObjectivesData
{
	[Serializable]
	public class FinalizedData_UseRelic : ObjectivesDataFinalized
	{
		public int Target;

		public override string GetText()
		{
			return ScriptLocalization.Objectives_Custom.UseRelic;
		}
	}

	public override bool AutoTrack
	{
		get
		{
			return true;
		}
	}

	public override string Text
	{
		get
		{
			return ScriptLocalization.Objectives_Custom.UseRelic;
		}
	}

	public Objectives_UseRelic()
	{
	}

	public Objectives_UseRelic(string groupId)
		: base(groupId)
	{
		Type = Objectives.TYPES.USE_RELIC;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_UseRelic
		{
			GroupId = GroupId,
			Index = Index,
			Target = ((!(RelicRoomManager.Instance != null)) ? 1 : RelicRoomManager.Instance.RelicTargetCount),
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		if (RelicRoomManager.Instance != null)
		{
			return RelicRoomManager.Instance.RelicUsedCount >= RelicRoomManager.Instance.RelicTargetCount;
		}
		return true;
	}
}
