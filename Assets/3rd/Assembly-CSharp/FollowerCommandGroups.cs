using System.Collections.Generic;
using Lamb.UI.FollowerInteractionWheel;

public sealed class FollowerCommandGroups
{
	public static List<CommandItem> DefaultCommands(Follower follower)
	{
		List<CommandItem> list = new List<CommandItem>();
		if ((follower.Brain.CurrentTaskType == FollowerTaskType.Sleep || follower.Brain.CurrentTaskType == FollowerTaskType.SleepBedRest) && follower.Brain.CurrentTask.State == FollowerTaskState.Doing)
		{
			return WakeUpCommands();
		}
		if (follower.Brain.Info.CursedState == Thought.Dissenter)
		{
			return DissenterCommands(follower);
		}
		if (follower.Brain.Info.CursedState == Thought.Ill)
		{
			return MakeDemandCommands(follower);
		}
		if (follower.Brain.Info.CursedState == Thought.OldAge)
		{
			return OldAgeCommands(follower);
		}
		list.Add(FollowerCommandItems.GiveWorkerCommand(follower));
		if (DataManager.Instance.InTutorial)
		{
			list.Add(FollowerCommandItems.MakeDemand(follower));
		}
		if (follower.Brain.Info.TaxEnforcer)
		{
			list.Add(FollowerCommandItems.CollectTax());
		}
		if (DataManager.Instance.CanReadMinds)
		{
			list.Add(FollowerCommandItems.Surveillance());
		}
		return list;
	}

	public static List<CommandItem> GiveWorkerCommands(Follower follower)
	{
		List<CommandItem> list = new List<CommandItem>();
		switch (Onboarding.CurrentPhase)
		{
		case DataManager.OnboardingPhase.Indoctrinate:
			list.Add(FollowerCommandItems.CutTrees());
			list.Add(FollowerCommandItems.ClearRubble());
			break;
		case DataManager.OnboardingPhase.IndoctrinateBerriesAllowed:
			list.Add(FollowerCommandItems.CutTrees());
			list.Add(FollowerCommandItems.ClearRubble());
			if (FarmStation.FarmStations.Count > 0)
			{
				list.Add(FollowerCommandItems.Farmer());
			}
			break;
		case DataManager.OnboardingPhase.Devotion:
			list.Add(FollowerCommandItems.Worship());
			break;
		default:
			list.Add(FollowerCommandItems.Worship());
			list.Add(FollowerCommandItems.CutTrees());
			list.Add(FollowerCommandItems.ClearRubble());
			list.Add(FollowerCommandItems.Builder());
			if (FarmStation.FarmStations.Count > 0)
			{
				list.Add(FollowerCommandItems.Farmer());
			}
			if (Interaction_FollowerKitchen.FollowerKitchens.Count > 0)
			{
				list.Add(FollowerCommandItems.Cook());
			}
			if (JanitorStation.JanitorStations.Count > 0)
			{
				list.Add(FollowerCommandItems.Janitor());
			}
			if (Interaction_Refinery.Refineries.Count > 0)
			{
				list.Add(FollowerCommandItems.Refiner());
			}
			if (Interaction_Morgue.Morgues.Count > 0)
			{
				list.Add(FollowerCommandItems.Undertaker());
			}
			break;
		}
		return list;
	}

	public static List<CommandItem> MealCommands(List<InventoryItem.ITEM_TYPE> availableMeals)
	{
		List<CommandItem> list = new List<CommandItem>();
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL))
		{
			list.Add(FollowerCommandItems.Meal());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_GRASS))
		{
			list.Add(FollowerCommandItems.MealGrass());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_POOP))
		{
			list.Add(FollowerCommandItems.MealPoop());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_MUSHROOMS))
		{
			list.Add(FollowerCommandItems.MealMushrooms());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT))
		{
			list.Add(FollowerCommandItems.MealFollowerMeat());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH))
		{
			list.Add(FollowerCommandItems.MealGoodFish());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_GREAT))
		{
			list.Add(FollowerCommandItems.MealGreat());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_MEAT))
		{
			list.Add(FollowerCommandItems.MealMeat());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH))
		{
			list.Add(FollowerCommandItems.MealGreatFish());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_BAD_FISH))
		{
			list.Add(FollowerCommandItems.MealBadFish());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_BERRIES))
		{
			list.Add(FollowerCommandItems.MealBerries());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG))
		{
			list.Add(FollowerCommandItems.MealMediumVeg());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT))
		{
			list.Add(FollowerCommandItems.MealMeatLow());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT))
		{
			list.Add(FollowerCommandItems.MealMeatHigh());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_DEADLY))
		{
			list.Add(FollowerCommandItems.MealDeadly());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED))
		{
			list.Add(FollowerCommandItems.MealMixedLow());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED))
		{
			list.Add(FollowerCommandItems.MealMixedMedium());
		}
		if (availableMeals.Contains(InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED))
		{
			list.Add(FollowerCommandItems.MealMixedHigh());
		}
		return list;
	}

	public static List<CommandItem> AreYouSureCommands()
	{
		return new List<CommandItem>
		{
			FollowerCommandItems.AreYouSureYes(),
			FollowerCommandItems.AreYouSureNo()
		};
	}

	public static List<CommandItem> MakeDemandCommands(Follower follower)
	{
		List<CommandItem> list = new List<CommandItem>();
		if (follower.Brain.Info.ID == FollowerManager.LeshyID && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.FLOWER_RED) > 0 && !DataManager.Instance.SecretItemsGivenToFollower.Contains(FollowerLocation.Dungeon1_1))
		{
			list.Add(FollowerCommandItems.GiveLeaderSecretItem(FollowerLocation.Dungeon1_1));
		}
		else if (follower.Brain.Info.ID == FollowerManager.HeketID && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL) > 0 && !DataManager.Instance.SecretItemsGivenToFollower.Contains(FollowerLocation.Dungeon1_2))
		{
			list.Add(FollowerCommandItems.GiveLeaderSecretItem(FollowerLocation.Dungeon1_2));
		}
		else if (follower.Brain.Info.ID == FollowerManager.KallamarID && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL) > 0 && !DataManager.Instance.SecretItemsGivenToFollower.Contains(FollowerLocation.Dungeon1_3))
		{
			list.Add(FollowerCommandItems.GiveLeaderSecretItem(FollowerLocation.Dungeon1_3));
		}
		else if (follower.Brain.Info.ID == FollowerManager.ShamuraID && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.SPIDER_WEB) > 0 && !DataManager.Instance.SecretItemsGivenToFollower.Contains(FollowerLocation.Dungeon1_4))
		{
			list.Add(FollowerCommandItems.GiveLeaderSecretItem(FollowerLocation.Dungeon1_4));
		}
		if (follower.Brain.Info.CursedState == Thought.Ill)
		{
			list.Add(FollowerCommandItems.BedRest());
		}
		else
		{
			list.Add(FollowerCommandItems.Sleep());
		}
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower))
		{
			list.Add(FollowerCommandItems.Murder());
		}
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.Possessions_ExtortTithes))
		{
			list.Add(FollowerCommandItems.Extort());
		}
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.Possessions_Bribe))
		{
			list.Add(FollowerCommandItems.Bribe());
		}
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire))
		{
			list.Add(FollowerCommandItems.Dance());
		}
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate))
		{
			list.Add(FollowerCommandItems.Intimidate());
		}
		if ((!DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire) && !DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate) && DataManager.Instance.ShowLoyaltyBars) || CheatConsole.ForceBlessEnabled)
		{
			list.Add(FollowerCommandItems.Bless());
		}
		list.Add(FollowerCommandItems.EatSomething(follower));
		if (Inventory.HasGift())
		{
			list.Add(FollowerCommandItems.Gift());
		}
		if (Prison.Prisons.Count > 0)
		{
			if (Prison.HasAvailablePrisons())
			{
				list.Add(FollowerCommandItems.Imprison());
			}
			else
			{
				list.Add(FollowerCommandItems.NoAvailablePrisons());
			}
		}
		if (follower.Brain.Info.MarriedToLeader || CheatConsole.ForceSmoochEnabled)
		{
			list.Add(FollowerCommandItems.Kiss());
		}
		if (follower.Brain.Info.SkinName.Contains("Dog"))
		{
			list.Add(FollowerCommandItems.PetDog());
		}
		return list;
	}

	public static List<CommandItem> WakeUpCommands()
	{
		List<CommandItem> list = new List<CommandItem>();
		list.Add(FollowerCommandItems.WakeUp());
		if (DataManager.Instance.CanReadMinds)
		{
			list.Add(FollowerCommandItems.Surveillance());
		}
		return list;
	}

	public static List<CommandItem> OldAgeCommands(Follower follower)
	{
		List<CommandItem> list = new List<CommandItem>();
		list.Add(FollowerCommandItems.Sleep());
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower))
		{
			list.Add(FollowerCommandItems.Murder());
		}
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire))
		{
			list.Add(FollowerCommandItems.Dance());
		}
		else if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate))
		{
			list.Add(FollowerCommandItems.Intimidate());
		}
		else if (DataManager.Instance.ShowLoyaltyBars || CheatConsole.ForceBlessEnabled)
		{
			list.Add(FollowerCommandItems.Bless());
		}
		if (DataManager.Instance.CanReadMinds)
		{
			list.Add(FollowerCommandItems.Surveillance());
		}
		list.Add(FollowerCommandItems.EatSomething(follower));
		if (follower.Brain.Info.MarriedToLeader || CheatConsole.ForceSmoochEnabled)
		{
			list.Add(FollowerCommandItems.Kiss());
		}
		if (follower.Brain.Info.SkinName.Contains("Dog"))
		{
			list.Add(FollowerCommandItems.PetDog());
		}
		return list;
	}

	public static List<CommandItem> DissenterCommands(Follower follower)
	{
		List<CommandItem> list = new List<CommandItem>();
		list.Add(FollowerCommandItems.Reeducate());
		if (Prison.Prisons.Count > 0)
		{
			if (Prison.HasAvailablePrisons())
			{
				list.Add(FollowerCommandItems.Imprison());
			}
			else
			{
				list.Add(FollowerCommandItems.NoAvailablePrisons());
			}
		}
		if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.LawOrder_MurderFollower))
		{
			list.Add(FollowerCommandItems.Murder());
		}
		if (DataManager.Instance.CanReadMinds)
		{
			list.Add(FollowerCommandItems.Surveillance());
		}
		list.Add(FollowerCommandItems.EatSomething(follower));
		return list;
	}

	public static List<CommandItem> GiftCommands()
	{
		List<CommandItem> list = new List<CommandItem>();
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.GIFT_SMALL) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Small());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.GIFT_MEDIUM) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Medium());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_1) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace1());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_2) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace2());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_3) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace3());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_4) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace4());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_5) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace5());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_Dark) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace_Dark());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_Gold_Skull) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace_Gold_Skull());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_Demonic) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace_Demonic());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_Light) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace_Light());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_Loyalty) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace_Loyalty());
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.Necklace_Missionary) > 0)
		{
			list.Add(FollowerCommandItems.Gift_Necklace_Missionary());
		}
		return list;
	}
}
