public class Interaction_ChoppingShrine : Interaction
{
	private Structures_ChoppingShrine _StructureInfo;

	public Structure Structure { get; private set; }

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_ChoppingShrine structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_ChoppingShrine;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}
}
