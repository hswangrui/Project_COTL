using System;
using I2.Loc;

[Serializable]
public class Objectives_PlaceStructure : ObjectivesData
{
	[Serializable]
	public class FinalizedData_PlaceStructure : ObjectivesDataFinalized
	{
		public int Target;

		public int Count;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives.PlaceDecorations, Count, Target);
		}
	}

	public StructureBrain.Categories category;

	public int Count;

	public int Target;

	public string Term;

	public override string Text
	{
		get
		{
			return string.Format(ScriptLocalization.Objectives.PlaceDecorations, Count, Target);
		}
	}

	public Objectives_PlaceStructure()
	{
	}

	public Objectives_PlaceStructure(string groupId, StructureBrain.Categories category, int target, float expireDuration)
		: base(groupId, expireDuration)
	{
		Type = Objectives.TYPES.PLACE_STRUCTURES;
		Target = target;
		this.category = category;
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
		return new FinalizedData_PlaceStructure
		{
			GroupId = GroupId,
			Index = Index,
			Target = Target,
			Count = Count,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (StructuresData.GetCategory(structure.Type) == category)
		{
			Count++;
		}
	}

	public override void Complete()
	{
		base.Complete();
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	public override void Failed()
	{
		base.Failed();
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	protected override bool CheckComplete()
	{
		return Count >= Target;
	}
}
