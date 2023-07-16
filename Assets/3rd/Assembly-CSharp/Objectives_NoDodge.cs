using System;
using I2.Loc;
using MMBiomeGeneration;

[Serializable]
public class Objectives_NoDodge : Objectives_RoomChallenge
{
	[Serializable]
	public class FinalizedData_NoDodge : ObjectivesDataFinalized
	{
		public int RoomsRequired;

		public int RoomsCompleted;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives.NoDodge, RoomsCompleted, RoomsRequired);
		}
	}

	public override string Text
	{
		get
		{
			return string.Format(ScriptLocalization.Objectives.NoDodge, base.RoomsCompleted, base.RoomsRequired);
		}
	}

	public Objectives_NoDodge()
	{
	}

	public Objectives_NoDodge(string groupId, int roomsRequired)
		: base(groupId, roomsRequired)
	{
		Type = Objectives.TYPES.NO_DODGE;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		PlayerFarming.OnDodge += PlayerFarming_OnDodge;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_NoDodge
		{
			GroupId = GroupId,
			Index = Index,
			RoomsRequired = base.RoomsRequired,
			RoomsCompleted = base.RoomsCompleted,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void PlayerFarming_OnDodge()
	{
		if (Health.team2.Count > 0 && !RoomLockController.DoorsOpen && BiomeGenerator.Instance.RoomEntrance != BiomeGenerator.Instance.CurrentRoom)
		{
			Failed();
			ObjectiveManager.CheckObjectives(Type);
			PlayerFarming.OnDodge -= PlayerFarming_OnDodge;
		}
	}
}
