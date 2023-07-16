using System;
using I2.Loc;

[Serializable]
public class Objectives_ShootDummy : ObjectivesData
{
	[Serializable]
	public class FinalizedData_ShootDummy : ObjectivesDataFinalized
	{
		public int Target;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives_Custom.ShootCurse, Target, Target, Target);
		}
	}

	private int Target = 5;

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
			return string.Format(ScriptLocalization.Objectives_Custom.ShootCurse, Target, (RatauGiveSpells.Instance != null) ? RatauGiveSpells.Instance.DummyCount : 0, Target);
		}
	}

	public Objectives_ShootDummy()
	{
	}

	public Objectives_ShootDummy(string groupId)
		: base(groupId)
	{
		Type = Objectives.TYPES.SHOOT_DUMMIES;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_ShootDummy
		{
			GroupId = GroupId,
			Index = Index,
			Target = Target,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		return RatauGiveSpells.Instance.DummyCount >= Target;
	}
}
