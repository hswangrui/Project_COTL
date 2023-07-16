using System;

[Serializable]
public class FollowerInteractionAlerts : AlertCategory<FollowerCommands>
{
	public FollowerInteractionAlerts()
	{
		Inventory.OnItemAddedToInventory += OnInventoryItemAdded;
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		DoctrineUpgradeSystem.OnDoctrineUnlocked = (Action<DoctrineUpgradeSystem.DoctrineType>)Delegate.Combine(DoctrineUpgradeSystem.OnDoctrineUnlocked, new Action<DoctrineUpgradeSystem.DoctrineType>(OnDoctrineUnlocked));
	}

	~FollowerInteractionAlerts()
	{
		Inventory.OnItemAddedToInventory -= OnInventoryItemAdded;
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
		DoctrineUpgradeSystem.OnDoctrineUnlocked = (Action<DoctrineUpgradeSystem.DoctrineType>)Delegate.Remove(DoctrineUpgradeSystem.OnDoctrineUnlocked, new Action<DoctrineUpgradeSystem.DoctrineType>(OnDoctrineUnlocked));
	}

	private void OnInventoryItemAdded(InventoryItem.ITEM_TYPE itemType, int Delta)
	{
		if (InventoryItem.IsGift(itemType))
		{
			AddOnce(FollowerCommands.Gift);
		}
	}

	private void OnStructureAdded(StructuresData structuresData)
	{
		if (structuresData.Type == StructureBrain.TYPES.PRISON)
		{
			AddOnce(FollowerCommands.NoAvailablePrisons);
			AddOnce(FollowerCommands.Imprison);
		}
		if (structuresData.Type == StructureBrain.TYPES.SURVEILLANCE)
		{
			AddOnce(FollowerCommands.Surveillance);
		}
		if (structuresData.Type == StructureBrain.TYPES.KITCHEN || structuresData.Type == StructureBrain.TYPES.KITCHEN_II)
		{
			AddOnce(FollowerCommands.Cook_2);
		}
	}

	private void OnDoctrineUnlocked(DoctrineUpgradeSystem.DoctrineType doctrineType)
	{
		switch (doctrineType)
		{
		case DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower:
			AddOnce(FollowerCommands.Murder);
			break;
		case DoctrineUpgradeSystem.DoctrineType.Possessions_ExtortTithes:
			AddOnce(FollowerCommands.ExtortMoney);
			break;
		case DoctrineUpgradeSystem.DoctrineType.Possessions_Bribe:
			AddOnce(FollowerCommands.Bribe);
			break;
		case DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire:
			AddOnce(FollowerCommands.Dance);
			break;
		case DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate:
			AddOnce(FollowerCommands.Intimidate);
			break;
		}
	}
}
