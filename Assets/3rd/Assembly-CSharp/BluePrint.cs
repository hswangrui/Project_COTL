using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BluePrint
{
	public enum BluePrintType
	{
		TREE,
		STONE,
		PATH_DIRT
	}

	public BluePrintType type;

	public static StructuresData GetStructure(BluePrintType type)
	{
		switch (type)
		{
		case BluePrintType.TREE:
			return StructuresData.GetInfoByType(StructureBrain.TYPES.DECORATION_TREE, 0);
		case BluePrintType.STONE:
			return StructuresData.GetInfoByType(StructureBrain.TYPES.DECORATION_STONE, 0);
		case BluePrintType.PATH_DIRT:
			return StructuresData.GetInfoByType(StructureBrain.TYPES.PLANK_PATH, 0);
		default:
			return null;
		}
	}

	public static BluePrint Create(BluePrintType type)
	{
		Debug.Log("CREATE CARD! " + type);
		return new BluePrint
		{
			type = type
		};
	}

	public static BluePrintType GiveNewBluePrint()
	{
		List<BluePrintType> list = new List<BluePrintType>();
		foreach (BluePrintType allBluePrint in DataManager.AllBluePrints)
		{
			bool flag = false;
			foreach (BluePrint playerBluePrint in DataManager.Instance.PlayerBluePrints)
			{
				if (playerBluePrint.type == allBluePrint)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(allBluePrint);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}
}
