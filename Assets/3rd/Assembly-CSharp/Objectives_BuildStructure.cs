using System;
using I2.Loc;

[Serializable]
public class Objectives_BuildStructure : ObjectivesData
{
	[Serializable]
	public class FinalizedData_BuildStructure : ObjectivesDataFinalized
	{
		public StructureBrain.TYPES StructureType;

		public int Target;

		public int Count;

		public override string GetText()
		{
			StructuresData infoByType = StructuresData.GetInfoByType(StructureType, 0);
			if (Target > 1)
			{
				return string.Format(ScriptLocalization.Objectives_BuildStructure.Plural, infoByType.GetLocalizedName(), Count.ToString(), Target.ToString());
			}
			return string.Format(ScriptLocalization.Objectives.BuildStructure, infoByType.GetLocalizedName());
		}
	}

	public StructureBrain.TYPES StructureType;

	public int Target;

	public int Count;

	public override string Text
	{
		get
		{
			StructuresData infoByType = StructuresData.GetInfoByType(StructureType, 0);
			if (Target > 1)
			{
				return string.Format(ScriptLocalization.Objectives_BuildStructure.Plural, infoByType.GetLocalizedName(), Count.ToString(), Target.ToString());
			}
			return string.Format(ScriptLocalization.Objectives.BuildStructure, infoByType.GetLocalizedName());
		}
	}

	public Objectives_BuildStructure()
	{
	}

	public Objectives_BuildStructure(string groupId, StructureBrain.TYPES structureType, int target = 1, float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.BUILD_STRUCTURE;
		StructureType = structureType;
		Target = target;
		Count = 0;
	}

	public override void Init(bool initialAssigning)
	{
		if (!initialised)
		{
			StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		}
		if (initialAssigning)
		{
			Count = 0;
		}
		base.Init(initialAssigning);
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_BuildStructure
		{
			GroupId = GroupId,
			Index = Index,
			StructureType = StructureType,
			Target = Target,
			Count = Count,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (structure.Type == StructureType)
		{
			Count++;
			ObjectiveManager.CheckObjectives(Objectives.TYPES.BUILD_STRUCTURE);
		}
	}

	protected override bool CheckComplete()
	{
		return Count >= Target;
	}
}
