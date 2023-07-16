using System;
using I2.Loc;
using MMBiomeGeneration;

[Serializable]
public class Objectives_NoCurses : Objectives_RoomChallenge
{
	[Serializable]
	public class FinalizedData_NoCurses : ObjectivesDataFinalized
	{
		public int RoomsRequired;

		public int RoomsCompleted;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives.NoCurses, RoomsCompleted, RoomsRequired);
		}
	}

	public override string Text
	{
		get
		{
			return string.Format(ScriptLocalization.Objectives.NoCurses, base.RoomsCompleted, base.RoomsRequired);
		}
	}

	public Objectives_NoCurses()
	{
	}

	public Objectives_NoCurses(string groupId, int roomsRequired)
		: base(groupId, roomsRequired)
	{
		Type = Objectives.TYPES.NO_CURSES;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		PlayerSpells.OnSpellCast += OnSpellCast;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_NoCurses
		{
			GroupId = GroupId,
			Index = Index,
			RoomsRequired = base.RoomsRequired,
			RoomsCompleted = base.RoomsCompleted,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void OnSpellCast(EquipmentType curse)
	{
		if (Health.team2.Count > 0 && !RoomLockController.DoorsOpen && BiomeGenerator.Instance.RoomEntrance != BiomeGenerator.Instance.CurrentRoom)
		{
			Failed();
			ObjectiveManager.CheckObjectives(Type);
			PlayerSpells.OnSpellCast -= OnSpellCast;
		}
	}
}
