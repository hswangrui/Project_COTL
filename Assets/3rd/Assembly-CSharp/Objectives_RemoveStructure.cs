using System;
using I2.Loc;

[Serializable]
public class Objectives_RemoveStructure : ObjectivesData
{
	[Serializable]
	public class FinalizedData_RemoveStructure : ObjectivesDataFinalized
	{
		public StructureBrain.TYPES StructureType;

		public int Target;

		public int Count;

		public override string GetText()
		{
			StructuresData infoByType = StructuresData.GetInfoByType(StructureType, 0);
			return ScriptLocalization.Interactions.Clean + " " + infoByType.GetLocalizedName() + " (" + Count + "/" + Target + ")";
		}
	}

	public StructureBrain.TYPES StructureType;

	public int Target = -1;

	public int Count;

	public override string Text
	{
		get
		{
			StructuresData infoByType = StructuresData.GetInfoByType(StructureType, 0);
			return ScriptLocalization.Interactions.Clean + " " + infoByType.GetLocalizedName() + " (" + Count + "/" + Target + ")";
		}
	}

	public Objectives_RemoveStructure()
	{
	}

	public Objectives_RemoveStructure(string groupId, StructureBrain.TYPES structureType)
		: base(groupId)
	{
		Type = Objectives.TYPES.REMOVE_STRUCTURES;
		StructureType = structureType;
	}

	public override void Init(bool initialAssigning)
	{
		if (!initialised)
		{
			StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(StructureRemoved));
		}
		base.Init(initialAssigning);
		if (Target == -1)
		{
			Target = StructureManager.GetAllStructuresOfType(StructureType).Count;
			Count = 0;
		}
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_RemoveStructure
		{
			GroupId = GroupId,
			Index = Index,
			StructureType = StructureType,
			Target = Target,
			Count = Count,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void StructureRemoved(StructuresData structure)
	{
		if (structure.Type == StructureType)
		{
			Count++;
		}
	}

	protected override bool CheckComplete()
	{
		return Count >= Target;
	}

	public override void Complete()
	{
		base.Complete();
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(StructureRemoved));
	}
}
