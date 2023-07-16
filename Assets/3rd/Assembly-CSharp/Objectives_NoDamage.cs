using System;
using I2.Loc;
using MMBiomeGeneration;

[Serializable]
public class Objectives_NoDamage : Objectives_RoomChallenge
{
	[Serializable]
	public class FinalizedData_NoDamage : ObjectivesDataFinalized
	{
		public int RoomsRequired;

		public int RoomsCompleted;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives.NoDamage, RoomsCompleted, RoomsRequired);
		}
	}

	public override string Text
	{
		get
		{
			return string.Format(ScriptLocalization.Objectives.NoDamage, base.RoomsCompleted, base.RoomsRequired);
		}
	}

	public Objectives_NoDamage()
	{
	}

	public Objectives_NoDamage(string groupId, int roomsRequired)
		: base(groupId, roomsRequired)
	{
		Type = Objectives.TYPES.NO_DAMAGE;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		HealthPlayer.OnDamaged += HealthPlayer_OnDamaged;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_NoDamage
		{
			GroupId = GroupId,
			Index = Index,
			RoomsRequired = base.RoomsRequired,
			RoomsCompleted = base.RoomsCompleted,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void HealthPlayer_OnDamaged(HealthPlayer Target)
	{
		if (Health.team2.Count > 0 && !RoomLockController.DoorsOpen && BiomeGenerator.Instance.RoomEntrance != BiomeGenerator.Instance.CurrentRoom)
		{
			Failed();
			ObjectiveManager.CheckObjectives(Type);
			HealthPlayer.OnDamaged -= HealthPlayer_OnDamaged;
		}
	}
}
