using System.Collections.Generic;
using UnityEngine;

public class Structures_SiloFertiliser : StructureBrain
{
	public float Capacity = 15f;

	public static Structures_SiloFertiliser GetClosestFertiliser(Vector3 fromPosition, FollowerLocation location)
	{
		List<Structures_SiloFertiliser> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_SiloFertiliser>(location);
		Structures_SiloFertiliser structures_SiloFertiliser = null;
		foreach (Structures_SiloFertiliser item in allStructuresOfType)
		{
			if (item.Data.Inventory.Count > 0 && item.Data.Inventory[0].quantity > 0 && (structures_SiloFertiliser == null || Vector3.Distance(item.Data.Position, fromPosition) < Vector3.Distance(structures_SiloFertiliser.Data.Position, fromPosition)))
			{
				structures_SiloFertiliser = item;
			}
		}
		return structures_SiloFertiliser;
	}
}
