using System;

[Serializable]
public class Objectives_NoHealing : Objectives_RoomChallenge
{
	[Serializable]
	public class FinalizedData_NoHealing : ObjectivesDataFinalized
	{
		public override string GetText()
		{
			return "";
		}
	}

	public override string Text
	{
		get
		{
			return string.Format("REMOVED", base.RoomsCompleted, base.RoomsRequired);
		}
	}

	public Objectives_NoHealing()
	{
	}

	public Objectives_NoHealing(string groupId, int roomsRequired)
		: base(groupId, roomsRequired)
	{
		Type = Objectives.TYPES.NO_HEALING;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		HealthPlayer.OnHeal += Health_OnHeal;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_NoHealing
		{
			GroupId = GroupId,
			Index = Index,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void Health_OnHeal(HealthPlayer player)
	{
		Failed();
		ObjectiveManager.CheckObjectives(Type);
		HealthPlayer.OnHeal -= Health_OnHeal;
	}
}
