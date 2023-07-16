using System.Collections.Generic;
using UnityEngine;

public class Structures_SiloSeed : StructureBrain
{
	public float Capacity = 15f;

	public static Structures_SiloSeed GetClosestSeeder(Vector3 fromPosition, FollowerLocation location, InventoryItem.ITEM_TYPE prioritisedSeedType = InventoryItem.ITEM_TYPE.NONE)
	{
		List<Structures_SiloSeed> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_SiloSeed>(location);
		Structures_SiloSeed structures_SiloSeed = null;
		foreach (Structures_SiloSeed item in allStructuresOfType)
		{
			if (prioritisedSeedType != 0)
			{
				foreach (InventoryItem item2 in item.Data.Inventory)
				{
					if (item2.type == (int)prioritisedSeedType && item2.UnreservedQuantity > 0)
					{
						return item;
					}
				}
			}
			else if (item.Data.Inventory.Count > 0 && item.Data.Inventory[0].quantity > 0 && (structures_SiloSeed == null || Vector3.Distance(item.Data.Position, fromPosition) < Vector3.Distance(structures_SiloSeed.Data.Position, fromPosition)))
			{
				structures_SiloSeed = item;
			}
		}
		return structures_SiloSeed;
	}
}
