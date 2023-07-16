public class Interaction_ForagingShrine : Interaction
{
	private Structures_ForagingShrine _StructureInfo;

	public Structure Structure { get; private set; }

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_ForagingShrine structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_ForagingShrine;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}
}
