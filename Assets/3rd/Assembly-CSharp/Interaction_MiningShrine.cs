public class Interaction_MiningShrine : Interaction
{
	private Structures_MiningShrine _StructureInfo;

	public Structure Structure { get; private set; }

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_MiningShrine structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_MiningShrine;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}
}
