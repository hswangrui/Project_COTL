using System.Collections.Generic;

public class Structures_Demon_Summoner : StructureBrain
{
	public int DemonSlots
	{
		get
		{
			if (Data.Type == TYPES.DEMON_SUMMONER)
			{
				return 1;
			}
			if (Data.Type == TYPES.DEMON_SUMMONER_2)
			{
				return 2;
			}
			if (Data.Type == TYPES.DEMON_SUMMONER_3)
			{
				return 3;
			}
			return 1;
		}
	}

	public static void RemoveFollower(int ID)
	{
		List<Structures_Demon_Summoner> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Demon_Summoner>();
		for (int i = 0; i < allStructuresOfType.Count; i++)
		{
			if (allStructuresOfType[i].Data.MultipleFollowerIDs.Contains(ID))
			{
				allStructuresOfType[i].Data.MultipleFollowerIDs.Remove(ID);
			}
		}
		if (DataManager.Instance.Followers_Demons_IDs.Contains(ID))
		{
			int index = DataManager.Instance.Followers_Demons_IDs.IndexOf(ID);
			DataManager.Instance.Followers_Demons_IDs.Remove(ID);
			DataManager.Instance.Followers_Demons_Types.RemoveAt(index);
		}
	}
}
