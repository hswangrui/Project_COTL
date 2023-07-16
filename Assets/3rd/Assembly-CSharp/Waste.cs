using System.Collections.Generic;

public class Waste : BaseMonoBehaviour
{
	public static List<Waste> Wastes = new List<Waste>();

	public Structure structure;

	public StructuresData StructureInfo
	{
		get
		{
			return structure.Structure_Info;
		}
	}

	public StructureBrain StructureBrain
	{
		get
		{
			return structure.Brain;
		}
	}

	private void OnEnable()
	{
		Wastes.Add(this);
	}

	private void OnDisable()
	{
		Wastes.Remove(this);
	}
}
