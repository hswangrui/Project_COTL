using System.Collections.Generic;

public class TreeBase : Interaction
{
	public static List<TreeBase> Trees = new List<TreeBase>();

	public Structure Structure;

	private Structures_Tree _StructureBrain;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Tree StructureBrain
	{
		get
		{
			if (_StructureBrain == null && Structure.Brain != null)
			{
				_StructureBrain = Structure.Brain as Structures_Tree;
			}
			return _StructureBrain;
		}
		set
		{
			_StructureBrain = value;
		}
	}
}
