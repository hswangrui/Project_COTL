using System.Collections.Generic;

public class Structures_Missionary : StructureBrain
{
	public static void RemoveFollower(int ID)
	{
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>();
		for (int i = 0; i < allStructuresOfType.Count; i++)
		{
			if (allStructuresOfType[i].Data.MultipleFollowerIDs.Contains(ID))
			{
				allStructuresOfType[i].Data.MultipleFollowerIDs.Remove(ID);
			}
		}
		if (DataManager.Instance.Followers_OnMissionary_IDs.Contains(ID))
		{
			DataManager.Instance.Followers_OnMissionary_IDs.Remove(ID);
		}
	}
}
