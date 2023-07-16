using System.Collections.Generic;
using I2.Loc;
using Lamb.UI.FollowerInteractionWheel;

public sealed class FollowerCommandItems
{
	private class FollowerRoleCommandItem : CommandItem
	{
		public FollowerTaskType FollowerTaskType;

		public override bool IsAvailable(Follower follower)
		{
			if (!FollowerBrain.IsTaskAvailable(FollowerTaskType))
			{
				return FollowerTaskType == FollowerTaskType.Pray;
			}
			return true;
		}

		public override string GetLockedDescription(Follower follower)
		{
			return LocalizationManager.GetTranslation("FollowerInteractions/TaskNotAvailable");
		}
	}

	private class CollectTaxCommandItem : CommandItem
	{
		public override string GetTitle(Follower follower)
		{
			if (follower.Brain._directInfoAccess.TaxCollected <= 0)
			{
				return LocalizationManager.GetTranslation("FollowerInteractions/NoTax");
			}
			return base.GetTitle(follower) + " <sprite name=\"icon_blackgold\"> x" + follower.Brain._directInfoAccess.TaxCollected;
		}
	}

	private class KissCommandItem : CommandItem
	{
		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.KissedAction;
		}
	}

	private class PetDogCommandItem : CommandItem
	{
		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.PetDog;
		}
	}

	private class BlessCommandItem : CommandItem
	{
		public override string GetDescription(Follower follower)
		{
			return base.GetDescription(follower) + "<br><sprite name=\"icon_Faith\">" + (" +" + FollowerThoughts.GetData(Thought.Cult_Bless).Modifier).Colour(StaticColors.GreenColor);
		}

		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.ReceivedBlessing;
		}
	}

	private class MurderCommandItem : CommandItem
	{
		public override string GetDescription(Follower follower)
		{
			return base.GetDescription(follower) + "<br><sprite name=\"icon_Faith\">" + FollowerThoughts.GetData(Thought.Cult_Murder).Modifier.ToString().Colour(StaticColors.RedColor);
		}
	}

	private class ExtortCommandItem : CommandItem
	{
		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.PaidTithes;
		}
	}

	private class BribeCommandItem : CommandItem
	{
		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.Bribed;
		}
	}

	private class DanceCommandItem : CommandItem
	{
		public override string GetDescription(Follower follower)
		{
			return base.GetDescription(follower) + "<br><sprite name=\"icon_Faith\">" + (" +" + FollowerThoughts.GetData(Thought.Cult_Inspire).Modifier).Colour(StaticColors.GreenColor);
		}

		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.Inspired;
		}
	}

	private class IntimidateCommandItem : CommandItem
	{
		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.Intimidated;
		}
	}

	public class GiftCommandItem : CommandItem
	{
		private InventoryItem.ITEM_TYPE _itemType;

		public GiftCommandItem(InventoryItem.ITEM_TYPE itemType)
		{
			_itemType = itemType;
		}

		public override string GetTitle(Follower follower)
		{
			return string.Format("{0} ({1})", LocalizationManager.GetTranslation(string.Format("Inventory/{0}", _itemType)), Inventory.GetItemQuantity(_itemType));
		}
	}

	private class ReeducateCommandItem : CommandItem
	{
		public override bool IsAvailable(Follower follower)
		{
			return !follower.Brain.Stats.ReeducatedAction;
		}
	}

	public class FoodCommandItem : CommandItem
	{
		public override string GetTitle(Follower follower)
		{
			return LocalizationManager.GetTranslation(string.Format("CookingData/{0}/Name", CookingData.RecipeTypeFromFollowerCommand(Command)));
		}

		public override string GetDescription(Follower follower)
		{
			return LocalizationManager.GetTranslation(string.Format("CookingData/{0}/Description", CookingData.RecipeTypeFromFollowerCommand(Command)));
		}
	}

	public class LeaderCommandItem : CommandItem
	{
		public FollowerLocation LeaderLocation;

		public override string GetTitle(Follower follower)
		{
			return LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}/SecretInteraction/Name", LeaderLocation));
		}

		public override string GetDescription(Follower follower)
		{
			return LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}/SecretInteraction/Description", LeaderLocation));
		}
	}

	public class ConfirmationItem : CommandItem
	{
		public override string GetDescription(Follower follower)
		{
			return "";
		}
	}

	private class GiverWorkerCommandItem : CommandItem
	{
		public override bool IsAvailable(Follower follower)
		{
			return !FollowerBrainStats.IsHoliday;
		}
	}

	public class NextPageCommandItem : CommandItem
	{
		public int PageNumber;

		public int TotalPageNumbers;

		public override string GetDescription(Follower follower)
		{
			return string.Format("{0}/{1}", PageNumber, TotalPageNumbers);
		}
	}

	public static CommandItem CutTrees()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.CutTrees,
			FollowerTaskType = FollowerTaskType.ChopTrees
		};
	}

	public static CommandItem ClearRubble()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.ClearRubble,
			FollowerTaskType = FollowerTaskType.ClearRubble
		};
	}

	public static CommandItem Farmer()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.Farmer_2,
			FollowerTaskType = FollowerTaskType.Farm
		};
	}

	public static CommandItem Builder()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.Build,
			FollowerTaskType = FollowerTaskType.Build
		};
	}

	public static CommandItem Refiner()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.Refiner_2,
			FollowerTaskType = FollowerTaskType.Refinery
		};
	}

	public static CommandItem CollectTax()
	{
		return new CollectTaxCommandItem
		{
			Command = FollowerCommands.CollectTax
		};
	}

	public static CommandItem Worship()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.WorshipAtShrine,
			FollowerTaskType = FollowerTaskType.Pray
		};
	}

	public static CommandItem Kiss()
	{
		return new KissCommandItem
		{
			Command = FollowerCommands.Romance
		};
	}

	public static CommandItem PetDog()
	{
		return new PetDogCommandItem
		{
			Command = FollowerCommands.PetDog
		};
	}

	public static CommandItem Janitor()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.Janitor_2,
			FollowerTaskType = FollowerTaskType.Janitor
		};
	}

	public static CommandItem Undertaker()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.Undertaker,
			FollowerTaskType = FollowerTaskType.Undertaker
		};
	}

	public static CommandItem BedRest()
	{
		return new CommandItem
		{
			Command = FollowerCommands.BedRest
		};
	}

	public static CommandItem Sleep()
	{
		return new CommandItem
		{
			Command = FollowerCommands.Sleep
		};
	}

	public static CommandItem Bless()
	{
		return new BlessCommandItem
		{
			Command = FollowerCommands.Bless
		};
	}

	public static CommandItem Murder()
	{
		return new MurderCommandItem
		{
			Command = FollowerCommands.Murder,
			SubCommands = FollowerCommandGroups.AreYouSureCommands()
		};
	}

	public static CommandItem Surveillance()
	{
		return new CommandItem
		{
			Command = FollowerCommands.Surveillance
		};
	}

	public static CommandItem Extort()
	{
		return new ExtortCommandItem
		{
			Command = FollowerCommands.ExtortMoney
		};
	}

	public static CommandItem Bribe()
	{
		return new BribeCommandItem
		{
			Command = FollowerCommands.Bribe
		};
	}

	public static CommandItem Dance()
	{
		return new DanceCommandItem
		{
			Command = FollowerCommands.Dance
		};
	}

	public static CommandItem Intimidate()
	{
		return new IntimidateCommandItem
		{
			Command = FollowerCommands.Intimidate
		};
	}

	public static CommandItem EatSomething(Follower follower)
	{
		CommandItem commandItem = new CommandItem
		{
			Command = FollowerCommands.EatSomething
		};
		if (!FollowerBrainStats.Fasting)
		{
			List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
			foreach (Structures_Meal item in StructureManager.GetAllStructuresOfType<Structures_Meal>(PlayerFarming.Location))
			{
				InventoryItem.ITEM_TYPE mealType = StructuresData.GetMealType(item.Data.Type);
				if (!list.Contains(mealType))
				{
					list.Add(mealType);
				}
			}
			foreach (Structures_Kitchen item2 in StructureManager.GetAllStructuresOfType<Structures_Kitchen>(PlayerFarming.Location))
			{
				foreach (InventoryItem item3 in item2.FoodStorage.Data.Inventory)
				{
					if (!list.Contains((InventoryItem.ITEM_TYPE)item3.type))
					{
						list.Add((InventoryItem.ITEM_TYPE)item3.type);
					}
				}
			}
			if (list.Count > 0)
			{
				commandItem.SubCommands = FollowerCommandGroups.MealCommands(list);
			}
		}
		return commandItem;
	}

	public static CommandItem Gift()
	{
		return new CommandItem
		{
			Command = FollowerCommands.Gift,
			SubCommands = FollowerCommandGroups.GiftCommands()
		};
	}

	public static CommandItem Gift_Small()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.GIFT_SMALL)
		{
			Command = FollowerCommands.Gift_Small
		};
	}

	public static CommandItem Gift_Medium()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.GIFT_MEDIUM)
		{
			Command = FollowerCommands.Gift_Medium
		};
	}

	public static CommandItem Gift_Necklace1()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_1)
		{
			Command = FollowerCommands.Gift_Necklace1
		};
	}

	public static CommandItem Gift_Necklace2()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_2)
		{
			Command = FollowerCommands.Gift_Necklace2
		};
	}

	public static CommandItem Gift_Necklace3()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_3)
		{
			Command = FollowerCommands.Gift_Necklace3
		};
	}

	public static CommandItem Gift_Necklace4()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_4)
		{
			Command = FollowerCommands.Gift_Necklace4
		};
	}

	public static CommandItem Gift_Necklace5()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_5)
		{
			Command = FollowerCommands.Gift_Necklace5
		};
	}

	public static CommandItem Gift_Necklace_Dark()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_Dark)
		{
			Command = FollowerCommands.Gift_Necklace_Dark
		};
	}

	public static CommandItem Gift_Necklace_Light()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_Light)
		{
			Command = FollowerCommands.Gift_Necklace_Light
		};
	}

	public static CommandItem Gift_Necklace_Missionary()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_Missionary)
		{
			Command = FollowerCommands.Gift_Necklace_Missionary
		};
	}

	public static CommandItem Gift_Necklace_Demonic()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_Demonic)
		{
			Command = FollowerCommands.Gift_Necklace_Demonic
		};
	}

	public static CommandItem Gift_Necklace_Loyalty()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_Loyalty)
		{
			Command = FollowerCommands.Gift_Necklace_Loyalty
		};
	}

	public static CommandItem Gift_Necklace_Gold_Skull()
	{
		return new GiftCommandItem(InventoryItem.ITEM_TYPE.Necklace_Gold_Skull)
		{
			Command = FollowerCommands.Gift_Necklace_Gold_Skull
		};
	}

	public static CommandItem RemoveNecklace()
	{
		return new CommandItem
		{
			Command = FollowerCommands.RemoveNecklace
		};
	}

	public static CommandItem Imprison()
	{
		return new CommandItem
		{
			Command = FollowerCommands.Imprison
		};
	}

	public static CommandItem Reeducate()
	{
		return new ReeducateCommandItem
		{
			Command = FollowerCommands.Reeducate
		};
	}

	public static CommandItem NoAvailablePrisons()
	{
		return new CommandItem
		{
			Command = FollowerCommands.NoAvailablePrisons
		};
	}

	public static CommandItem Cook()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.Cook_2,
			FollowerTaskType = FollowerTaskType.Cook
		};
	}

	public static LeaderCommandItem GiveLeaderSecretItem(FollowerLocation location)
	{
		return new LeaderCommandItem
		{
			LeaderLocation = location,
			Command = FollowerCommands.GiveLeaderItem
		};
	}

	public static CommandItem Meal()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.Meal
		};
	}

	public static CommandItem MealGrass()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealGrass
		};
	}

	public static CommandItem MealPoop()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealPoop
		};
	}

	public static CommandItem MealMushrooms()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMushrooms
		};
	}

	public static CommandItem MealFollowerMeat()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealFollowerMeat
		};
	}

	public static CommandItem ForageBerries()
	{
		return new FollowerRoleCommandItem
		{
			Command = FollowerCommands.ForageBerries,
			FollowerTaskType = FollowerTaskType.Forage
		};
	}

	public static CommandItem MealGoodFish()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealGoodFish
		};
	}

	public static CommandItem MealGreat()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealGreat
		};
	}

	public static CommandItem MealMeat()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMeat
		};
	}

	public static CommandItem MealGreatFish()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealGreatFish
		};
	}

	public static CommandItem MealBadFish()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealBadFish
		};
	}

	public static CommandItem MealBerries()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealBerries
		};
	}

	public static CommandItem MealDeadly()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealDeadly
		};
	}

	public static CommandItem MealMediumVeg()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMediumVeg
		};
	}

	public static CommandItem MealMeatLow()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMeatLow
		};
	}

	public static CommandItem MealMeatHigh()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMeatHigh
		};
	}

	public static CommandItem MealMixedLow()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMixedLow
		};
	}

	public static CommandItem MealMixedMedium()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMixedMedium
		};
	}

	public static CommandItem MealMixedHigh()
	{
		return new FoodCommandItem
		{
			Command = FollowerCommands.MealMixedHigh
		};
	}

	public static CommandItem AreYouSureYes()
	{
		return new ConfirmationItem
		{
			Command = FollowerCommands.AreYouSureYes
		};
	}

	public static CommandItem AreYouSureNo()
	{
		return new ConfirmationItem
		{
			Command = FollowerCommands.AreYouSureNo
		};
	}

	public static CommandItem WakeUp()
	{
		return new CommandItem
		{
			Command = FollowerCommands.WakeUp
		};
	}

	public static CommandItem MakeDemand(Follower follower)
	{
		return new CommandItem
		{
			Command = FollowerCommands.MakeDemand,
			SubCommands = FollowerCommandGroups.MakeDemandCommands(follower)
		};
	}

	public static CommandItem GiveWorkerCommand(Follower follower)
	{
		GiverWorkerCommandItem giverWorkerCommandItem = new GiverWorkerCommandItem
		{
			Command = FollowerCommands.GiveWorkerCommand_2
		};
		if (follower.Brain.Info.CursedState == Thought.None)
		{
			giverWorkerCommandItem.SubCommands = FollowerCommandGroups.GiveWorkerCommands(follower);
		}
		return giverWorkerCommandItem;
	}

	public static NextPageCommandItem NextPage()
	{
		return new NextPageCommandItem
		{
			Command = FollowerCommands.NextPage,
			SubCommands = new List<CommandItem>()
		};
	}
}
