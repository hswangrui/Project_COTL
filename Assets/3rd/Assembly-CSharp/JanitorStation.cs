using System.Collections.Generic;

public class JanitorStation : BaseMonoBehaviour
{
	public static List<JanitorStation> JanitorStations = new List<JanitorStation>();

	public Structure Structure;

	private Structures_JanitorStation _StructureInfo;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_JanitorStation StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_JanitorStation;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void OnEnable()
	{
		JanitorStations.Add(this);
	}

	private void OnDisable()
	{
		JanitorStations.Remove(this);
	}
}
