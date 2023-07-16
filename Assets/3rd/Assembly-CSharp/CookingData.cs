using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Unify;
using UnityEngine;
using UnityEngine.U2D;

public sealed class CookingData
{
	public enum IngredientType
	{
		NONE = 0,
		FRUIT_LOW = 1,
		VEGETABLE_LOW = 100,
		VEGETABLE_MEDIUM = 101,
		VEGETABLE_HIGH = 102,
		MEAT_LOW = 200,
		MEAT_MEDIUM = 201,
		MEAT_HIGH = 202,
		FISH_LOW = 300,
		FISH_MEDIUM = 301,
		FISH_HIGH = 302,
		MIXED_LOW = 400,
		MIXED_MEDIUM = 401,
		MIXED_HIGH = 402,
		SPECIAL_FOLLOWER_MEAT = 500,
		SPECIAL_GRASS = 501,
		SPECIAL_POOP = 502,
		SPECIAL_DEADLY = 503,
		MEAT = 1000,
		VEGETABLE = 1001,
		FISH = 1002,
		FRUIT = 1003,
		MIXED = 1004,
		SPECIAL = 1005
	}

	public struct MealEffect
	{
		public MealEffectType MealEffectType;

		public int Chance;
	}

	public enum MealEffectType
	{
		None,
		InstantlyPoop,
		InstantlyVomit,
		InstantlyDie,
		RemovesDissent,
		RemovesIllness,
		CausesIllness,
		CausesExhaustion,
		CausesDissent,
		CausesIllnessOrDissent,
		CausesIllPoopy,
		DropLoot,
		IncreasesLoyalty,
		HealsIllness
	}

	public const int MaxIngredientSlots = 3;

	public static Action<InventoryItem.ITEM_TYPE> OnRecipeDiscovered;

	private const float RecipeBalanceValue = 2.25f;

	public static bool REQUIRES_LOC;

	public static bool CanMakeMeal(InventoryItem.ITEM_TYPE mealType)
	{
		List<List<InventoryItem>> recipe = GetRecipe(mealType);
		for (int i = 0; i < recipe.Count; i++)
		{
			bool flag = true;
			for (int num = recipe[i].Count - 1; num >= 0; num--)
			{
				if (Inventory.GetItemQuantity(recipe[i][num].type) < recipe[i][num].quantity)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanMakeMealUsingRecipe(List<InventoryItem> recipe)
	{
		bool result = true;
		for (int num = recipe.Count - 1; num >= 0; num--)
		{
			if (Inventory.GetItemQuantity(recipe[num].type) < recipe[num].quantity)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public static int GetCookableRecipeAmount(InventoryItem.ITEM_TYPE mealType, List<InventoryItem> ingredients)
	{
		List<List<InventoryItem>> recipe = GetRecipe(mealType);
		int num = 0;
		int num2;
		using (List<List<InventoryItem>>.Enumerator enumerator = recipe.GetEnumerator())
		{
			if (!enumerator.MoveNext())
			{
				return num;
			}
			List<InventoryItem> current = enumerator.Current;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (InventoryItem ingredient in ingredients)
			{
				if (dictionary.ContainsKey(ingredient.type))
				{
					Dictionary<int, int> dictionary2 = dictionary;
					num2 = ingredient.type;
					dictionary2[num2] += ingredient.quantity;
				}
				else
				{
					dictionary.Add(ingredient.type, ingredient.quantity);
				}
			}
			while (true)
			{
				if (recipe.Count == 0)
				{
					num2 = 0;
					break;
				}
				foreach (InventoryItem item in current)
				{
					bool flag = false;
					int num3 = 0;
					while (true)
					{
						if (num3 < dictionary.Count)
						{
							if (item.type == dictionary.ElementAt(num3).Key)
							{
								flag = true;
								if (dictionary.ElementAt(num3).Value < item.quantity)
								{
									num2 = num;
									goto end_IL_009e;
								}
								dictionary[item.type] -= item.quantity;
							}
							num3++;
							continue;
						}
						if (flag)
						{
							break;
						}
						num2 = num;
						goto end_IL_009e;
					}
				}
				num++;
				continue;
				end_IL_009e:
				break;
			}
		}
		return num2;
	}

	private static int BalanceValue(int value)
	{
		return Mathf.CeilToInt((float)value * 2.25f);
	}

	public static List<List<InventoryItem>> GetRecipe(InventoryItem.ITEM_TYPE mealType)
	{
		switch (mealType)
		{
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.BERRY, 6)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.PUMPKIN, 4)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.CAULIFLOWER, 4)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.MEAT_MORSEL, 3)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.MEAT, 2)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_SMALL, 3)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH, 3)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_SQUID, 1),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_OCTOPUS, 1),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_BLOWFISH, 1),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_SWORDFISH, 1)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.MEAT, 4),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_CRAB, 1),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_LOBSTER, 1)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 3),
					new InventoryItem(InventoryItem.ITEM_TYPE.BONE, 5)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.POOP, 3)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 1),
					new InventoryItem(InventoryItem.ITEM_TYPE.POOP, 1),
					new InventoryItem(InventoryItem.ITEM_TYPE.GRASS, 1)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.GRASS, 5)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.BERRY, 4),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_SMALL, 2),
					new InventoryItem(InventoryItem.ITEM_TYPE.MEAT_MORSEL, 2)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.PUMPKIN, 4),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH, 2),
					new InventoryItem(InventoryItem.ITEM_TYPE.MEAT, 2)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.BEETROOT, 4),
					new InventoryItem(InventoryItem.ITEM_TYPE.FISH_BIG, 2),
					new InventoryItem(InventoryItem.ITEM_TYPE.MEAT, 2)
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			return new List<List<InventoryItem>>
			{
				new List<InventoryItem>
				{
					new InventoryItem(InventoryItem.ITEM_TYPE.BEETROOT, 6),
					new InventoryItem(InventoryItem.ITEM_TYPE.PUMPKIN, 2),
					new InventoryItem(InventoryItem.ITEM_TYPE.CAULIFLOWER, 2)
				}
			};
		default:
			return new List<List<InventoryItem>>();
		}
	}

	public static List<InventoryItem> GetRecipeSimplified(InventoryItem.ITEM_TYPE meal)
	{
		List<List<InventoryItem>> recipe = GetRecipe(meal);
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (InventoryItem item in recipe[0])
		{
			bool flag = false;
			foreach (InventoryItem item2 in list)
			{
				if (item2.type == item.type)
				{
					item2.quantity++;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(new InventoryItem((InventoryItem.ITEM_TYPE)item.type, item.quantity));
			}
		}
		return list;
	}

	public static bool HasRecipeDiscovered(InventoryItem.ITEM_TYPE meal)
	{
		return DataManager.Instance.RecipesDiscovered.Contains(meal);
	}

	public static bool TryDiscoverRecipe(InventoryItem.ITEM_TYPE meal)
	{
		if (!DataManager.Instance.RecipesDiscovered.Contains(meal))
		{
			DataManager.Instance.RecipesDiscovered.Add(meal);
			Action<InventoryItem.ITEM_TYPE> onRecipeDiscovered = OnRecipeDiscovered;
			if (onRecipeDiscovered != null)
			{
				onRecipeDiscovered(meal);
			}
			return true;
		}
		return false;
	}

	public static string GetLocalizedName(InventoryItem.ITEM_TYPE mealType)
	{
		return LocalizationManager.GetTranslation(string.Format("CookingData/{0}/Name", mealType));
	}

	public static string GetLocalizedDescription(InventoryItem.ITEM_TYPE mealType)
	{
		return LocalizationManager.GetTranslation(string.Format("CookingData/{0}/Description", mealType));
	}

	public static InventoryItem.ITEM_TYPE GetIngredientFromInventory(IngredientType ingredientType)
	{
		foreach (InventoryItem item in Inventory.items)
		{
			if (GetIngredientType((InventoryItem.ITEM_TYPE)item.type) == ingredientType)
			{
				return (InventoryItem.ITEM_TYPE)item.type;
			}
		}
		return InventoryItem.ITEM_TYPE.NONE;
	}

	public static InventoryItem.ITEM_TYPE RecipeTypeFromFollowerCommand(FollowerCommands followerCommand)
	{
		switch (followerCommand)
		{
		case FollowerCommands.MealGreatFish:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH;
		case FollowerCommands.MealBadFish:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_FISH;
		case FollowerCommands.MealBerries:
			return InventoryItem.ITEM_TYPE.MEAL_BERRIES;
		case FollowerCommands.MealMediumVeg:
			return InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG;
		case FollowerCommands.MealMixedLow:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED;
		case FollowerCommands.MealMixedMedium:
			return InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED;
		case FollowerCommands.MealMixedHigh:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED;
		case FollowerCommands.MealDeadly:
			return InventoryItem.ITEM_TYPE.MEAL_DEADLY;
		case FollowerCommands.MealMeatLow:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT;
		case FollowerCommands.Meal:
			return InventoryItem.ITEM_TYPE.MEAL;
		case FollowerCommands.MealGrass:
			return InventoryItem.ITEM_TYPE.MEAL_GRASS;
		case FollowerCommands.MealPoop:
			return InventoryItem.ITEM_TYPE.MEAL_POOP;
		case FollowerCommands.MealGoodFish:
			return InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH;
		case FollowerCommands.MealFollowerMeat:
			return InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT;
		case FollowerCommands.MealGreat:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT;
		case FollowerCommands.MealMeatHigh:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT;
		case FollowerCommands.MealMushrooms:
			return InventoryItem.ITEM_TYPE.MEAL_MUSHROOMS;
		case FollowerCommands.MealMeat:
			return InventoryItem.ITEM_TYPE.MEAL_MEAT;
		default:
			return InventoryItem.ITEM_TYPE.NONE;
		}
	}

	public static IngredientType GetIngredientType(InventoryItem.ITEM_TYPE ingredient)
	{
		switch (ingredient)
		{
		case InventoryItem.ITEM_TYPE.BERRY:
			return IngredientType.FRUIT_LOW;
		case InventoryItem.ITEM_TYPE.PUMPKIN:
			return IngredientType.VEGETABLE_MEDIUM;
		case InventoryItem.ITEM_TYPE.CAULIFLOWER:
			return IngredientType.VEGETABLE_MEDIUM;
		case InventoryItem.ITEM_TYPE.BEETROOT:
			return IngredientType.VEGETABLE_HIGH;
		case InventoryItem.ITEM_TYPE.MEAT_MORSEL:
			return IngredientType.MEAT_LOW;
		case InventoryItem.ITEM_TYPE.MEAT:
			return IngredientType.MEAT_HIGH;
		case InventoryItem.ITEM_TYPE.FOLLOWER_MEAT:
		case InventoryItem.ITEM_TYPE.FOLLOWER_MEAT_ROTTEN:
			return IngredientType.SPECIAL_FOLLOWER_MEAT;
		case InventoryItem.ITEM_TYPE.FISH_SMALL:
			return IngredientType.FISH_LOW;
		case InventoryItem.ITEM_TYPE.FISH:
		case InventoryItem.ITEM_TYPE.FISH_BIG:
		case InventoryItem.ITEM_TYPE.FISH_CRAB:
		case InventoryItem.ITEM_TYPE.FISH_BLOWFISH:
			return IngredientType.FISH_MEDIUM;
		case InventoryItem.ITEM_TYPE.FISH_LOBSTER:
		case InventoryItem.ITEM_TYPE.FISH_OCTOPUS:
		case InventoryItem.ITEM_TYPE.FISH_SQUID:
		case InventoryItem.ITEM_TYPE.FISH_SWORDFISH:
			return IngredientType.FISH_HIGH;
		case InventoryItem.ITEM_TYPE.POOP:
			return IngredientType.SPECIAL_POOP;
		case InventoryItem.ITEM_TYPE.GRASS:
			return IngredientType.SPECIAL_GRASS;
		default:
			return IngredientType.NONE;
		}
	}

	public static IngredientType GetIngredientCategory(IngredientType ingredientType)
	{
		switch (ingredientType)
		{
		case IngredientType.FRUIT_LOW:
			return IngredientType.FRUIT;
		case IngredientType.FISH_LOW:
		case IngredientType.FISH_MEDIUM:
		case IngredientType.FISH_HIGH:
			return IngredientType.FISH;
		case IngredientType.VEGETABLE_LOW:
		case IngredientType.VEGETABLE_MEDIUM:
		case IngredientType.VEGETABLE_HIGH:
			return IngredientType.VEGETABLE;
		case IngredientType.MEAT_LOW:
		case IngredientType.MEAT_MEDIUM:
		case IngredientType.MEAT_HIGH:
			return IngredientType.MEAT;
		case IngredientType.SPECIAL_FOLLOWER_MEAT:
		case IngredientType.SPECIAL_GRASS:
		case IngredientType.SPECIAL_POOP:
			return IngredientType.SPECIAL;
		default:
			return IngredientType.NONE;
		}
	}

	public static int GetIngredientTypeWeight(IngredientType ingredientType)
	{
		switch (ingredientType)
		{
		case IngredientType.FRUIT_LOW:
		case IngredientType.VEGETABLE_LOW:
		case IngredientType.MEAT_LOW:
		case IngredientType.FISH_LOW:
			return 4;
		case IngredientType.VEGETABLE_MEDIUM:
		case IngredientType.MEAT_MEDIUM:
		case IngredientType.FISH_MEDIUM:
			return 6;
		case IngredientType.VEGETABLE_HIGH:
		case IngredientType.MEAT_HIGH:
		case IngredientType.FISH_HIGH:
			return 10;
		default:
			return 0;
		}
	}

	public static IngredientType DetermineIngredientTypeLevel(IngredientType ingredientType, int value)
	{
		switch (ingredientType)
		{
		case IngredientType.MEAT:
			if ((float)value >= 7.5f)
			{
				return IngredientType.MEAT_HIGH;
			}
			if ((float)value > 4f)
			{
				return IngredientType.MEAT_MEDIUM;
			}
			return IngredientType.MEAT_LOW;
		case IngredientType.VEGETABLE:
			if ((float)value >= 7.5f)
			{
				return IngredientType.VEGETABLE_HIGH;
			}
			if ((float)value > 4f)
			{
				return IngredientType.VEGETABLE_MEDIUM;
			}
			return IngredientType.VEGETABLE_LOW;
		case IngredientType.FISH:
			if ((float)value >= 7.5f)
			{
				return IngredientType.FISH_HIGH;
			}
			if ((float)value > 4f)
			{
				return IngredientType.FISH_MEDIUM;
			}
			return IngredientType.FISH_LOW;
		case IngredientType.MIXED:
			if ((float)value >= 7.5f)
			{
				return IngredientType.MIXED_HIGH;
			}
			if ((float)value > 4f)
			{
				return IngredientType.MIXED_MEDIUM;
			}
			return IngredientType.MIXED_LOW;
		case IngredientType.FRUIT:
			return IngredientType.FRUIT_LOW;
		default:
			return IngredientType.NONE;
		}
	}

	public static int GetIllnessAmount(InventoryItem.ITEM_TYPE meal, FollowerBrain _brain)
	{
		switch (meal)
		{
		case InventoryItem.ITEM_TYPE.GRASS:
			if (!_brain.HasTrait(FollowerTrait.TraitType.GrassEater))
			{
				return 5;
			}
			break;
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			if (!_brain.HasTrait(FollowerTrait.TraitType.Cannibal))
			{
				return 10;
			}
			break;
		case InventoryItem.ITEM_TYPE.POOP:
			return 20;
		case InventoryItem.ITEM_TYPE.MEAL_ROTTEN:
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			return 10;
		}
		return 0;
	}

	public static int GetSatationAmount(InventoryItem.ITEM_TYPE meal)
	{
		switch (meal)
		{
		case InventoryItem.ITEM_TYPE.NONE:
			return 0;
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return 75;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return 100;
		default:
			return 60;
		}
	}

	public static float GetTummyRating(InventoryItem.ITEM_TYPE meal)
	{
		switch (meal)
		{
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return 0.5f;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return 1f;
		default:
			return 0.25f;
		}
	}

	public static int GetSatationLevel(InventoryItem.ITEM_TYPE meal)
	{
		switch (meal)
		{
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return 2;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return 3;
		default:
			return 1;
		}
	}

	public static int GetFaithAmount(InventoryItem.ITEM_TYPE meal)
	{
		switch (meal)
		{
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return 0;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return 3;
		default:
			return 0;
		}
	}

	public static ThoughtData ThoughtDataForMeal(InventoryItem.ITEM_TYPE meal)
	{
		switch (meal)
		{
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			return FollowerThoughts.GetData(Thought.AtePoopMeal);
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			if (!DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Cannibal))
			{
				return FollowerThoughts.GetData(Thought.Cult_AteFollowerMeat);
			}
			return FollowerThoughts.GetData(Thought.Cult_AteFollowerMeatTrait);
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.GrassEater))
			{
				return FollowerThoughts.GetData(Thought.Cult_AteGrassMealTrait);
			}
			return FollowerThoughts.GetData(Thought.Cult_AteGrassMeal);
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			return FollowerThoughts.GetData(Thought.Cult_AteGreatMeal);
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			return FollowerThoughts.GetData(Thought.Cult_AteGreatFishMeal);
		default:
			return new ThoughtData();
		}
	}

	public static InventoryItem.ITEM_TYPE[] GetAllFoods()
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		list.Add(InventoryItem.ITEM_TYPE.GRASS);
		list.Add(InventoryItem.ITEM_TYPE.POOP);
		InventoryItem.ITEM_TYPE[] lowQualityFoods = GetLowQualityFoods();
		foreach (InventoryItem.ITEM_TYPE item in lowQualityFoods)
		{
			list.Add(item);
		}
		lowQualityFoods = GetMediumQualityFoods();
		foreach (InventoryItem.ITEM_TYPE item2 in lowQualityFoods)
		{
			list.Add(item2);
		}
		lowQualityFoods = GetHighQualityFoods();
		foreach (InventoryItem.ITEM_TYPE item3 in lowQualityFoods)
		{
			list.Add(item3);
		}
		return list.ToArray();
	}

	public static InventoryItem.ITEM_TYPE[] GetLowQualityFoods()
	{
		return new InventoryItem.ITEM_TYPE[3]
		{
			InventoryItem.ITEM_TYPE.MEAT_MORSEL,
			InventoryItem.ITEM_TYPE.FISH_SMALL,
			InventoryItem.ITEM_TYPE.BERRY
		};
	}

	public static InventoryItem.ITEM_TYPE[] GetMediumQualityFoods()
	{
		return new InventoryItem.ITEM_TYPE[9]
		{
			InventoryItem.ITEM_TYPE.MEAT,
			InventoryItem.ITEM_TYPE.FOLLOWER_MEAT,
			InventoryItem.ITEM_TYPE.FOLLOWER_MEAT_ROTTEN,
			InventoryItem.ITEM_TYPE.FISH,
			InventoryItem.ITEM_TYPE.FISH_BIG,
			InventoryItem.ITEM_TYPE.FISH_CRAB,
			InventoryItem.ITEM_TYPE.FISH_BLOWFISH,
			InventoryItem.ITEM_TYPE.CAULIFLOWER,
			InventoryItem.ITEM_TYPE.PUMPKIN
		};
	}

	public static InventoryItem.ITEM_TYPE[] GetHighQualityFoods()
	{
		return new InventoryItem.ITEM_TYPE[5]
		{
			InventoryItem.ITEM_TYPE.FISH_SWORDFISH,
			InventoryItem.ITEM_TYPE.FISH_SQUID,
			InventoryItem.ITEM_TYPE.FISH_LOBSTER,
			InventoryItem.ITEM_TYPE.FISH_OCTOPUS,
			InventoryItem.ITEM_TYPE.BEETROOT
		};
	}

	private static IngredientType GetSpecialType(List<InventoryItem> ingredients)
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		foreach (InventoryItem ingredient in ingredients)
		{
			list.Add((InventoryItem.ITEM_TYPE)ingredient.type);
		}
		if (list.Contains(InventoryItem.ITEM_TYPE.POOP) && list.Contains(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT))
		{
			return IngredientType.SPECIAL_DEADLY;
		}
		if (list.Contains(InventoryItem.ITEM_TYPE.POOP))
		{
			return IngredientType.SPECIAL_POOP;
		}
		if (list.Contains(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT))
		{
			return IngredientType.SPECIAL_FOLLOWER_MEAT;
		}
		if (list.Contains(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT_ROTTEN))
		{
			return IngredientType.SPECIAL_DEADLY;
		}
		if (list.Contains(InventoryItem.ITEM_TYPE.GRASS))
		{
			int num = 0;
			foreach (InventoryItem ingredient2 in ingredients)
			{
				if (ingredient2.type == 35)
				{
					num += ingredient2.quantity;
				}
			}
			if (num >= 2)
			{
				return IngredientType.SPECIAL_GRASS;
			}
		}
		return IngredientType.NONE;
	}

	public static List<InventoryItem.ITEM_TYPE> GetDiscoveredMealTypes()
	{
		return DataManager.Instance.RecipesDiscovered;
	}

	public static Sprite GetIcon(InventoryItem.ITEM_TYPE mealType)
	{
		SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("Atlases/MealIcons");
		string text = mealType.ToString().ToLower();
		char c = text[0];
		text = text.Remove(0, 1);
		text = text.Insert(0, char.ToUpper(c).ToString());
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '_')
			{
				c = text[i + 1];
				text = text.Remove(i + 1, 1);
				text = text.Insert(i + 1, char.ToUpper(c).ToString());
			}
		}
		return spriteAtlas.GetSprite(text);
	}

	public static float GetMealCookDuration(StructureBrain.TYPES mealType)
	{
		return GetMealCookDuration(GetMealFromStructureType(mealType));
	}

	public static float GetMealCookDuration(InventoryItem.ITEM_TYPE mealType)
	{
		switch (mealType)
		{
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
		case InventoryItem.ITEM_TYPE.MEAL_ROTTEN:
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED:
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			return 30f;
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return 36f;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return 42f;
		default:
			return 42f;
		}
	}

	public static void CookedMeal(InventoryItem.ITEM_TYPE mealType)
	{
		if (mealType == InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FEED_FOLLOWER_MEAT"));
		}
		TryDiscoverRecipe(mealType);
		switch (mealType)
		{
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
			DataManager.Instance.FRUIT_LOW_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL:
			DataManager.Instance.VEGETABLE_LOW_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
			DataManager.Instance.VEGETABLE_MEDIUM_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			DataManager.Instance.VEGETABLE_HIGH_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			DataManager.Instance.MEAT_LOW_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
			DataManager.Instance.MEAT_MEDIUM_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			DataManager.Instance.MEAT_HIGH_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			DataManager.Instance.FISH_LOW_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
			DataManager.Instance.FISH_MEDIUM_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			DataManager.Instance.FISH_HIGH_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.NONE:
			DataManager.Instance.MealsCooked++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			DataManager.Instance.GRASS_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			DataManager.Instance.FOLLOWER_MEAT_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			DataManager.Instance.POOP_MEALS_COOKED++;
			break;
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
			DataManager.Instance.DEADLY_MEALS_COOKED++;
			break;
		}
	}

	public static int GetCookedMeal(InventoryItem.ITEM_TYPE mealType)
	{
		switch (mealType)
		{
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
			return DataManager.Instance.FRUIT_LOW_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL:
			return DataManager.Instance.VEGETABLE_LOW_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
			return DataManager.Instance.VEGETABLE_MEDIUM_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			return DataManager.Instance.VEGETABLE_HIGH_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			return DataManager.Instance.MEAT_LOW_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
			return DataManager.Instance.MEAT_MEDIUM_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return DataManager.Instance.MEAT_HIGH_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			return DataManager.Instance.FISH_LOW_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
			return DataManager.Instance.FISH_MEDIUM_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			return DataManager.Instance.FISH_HIGH_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.NONE:
			return DataManager.Instance.MealsCooked;
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			return DataManager.Instance.GRASS_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			return DataManager.Instance.FOLLOWER_MEAT_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			return DataManager.Instance.POOP_MEALS_COOKED;
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
			return DataManager.Instance.DEADLY_MEALS_COOKED;
		default:
			return 0;
		}
	}

	public static StructureBrain.TYPES GetStructureFromMealType(InventoryItem.ITEM_TYPE mealType)
	{
		switch (mealType)
		{
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
			return StructureBrain.TYPES.MEAL_BERRIES;
		case InventoryItem.ITEM_TYPE.MEAL:
			return StructureBrain.TYPES.MEAL;
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
			return StructureBrain.TYPES.MEAL_MEDIUM_VEG;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			return StructureBrain.TYPES.MEAL_GREAT;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			return StructureBrain.TYPES.MEAL_BAD_FISH;
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
			return StructureBrain.TYPES.MEAL_GOOD_FISH;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			return StructureBrain.TYPES.MEAL_GREAT_FISH;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			return StructureBrain.TYPES.MEAL_BAD_MEAT;
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
			return StructureBrain.TYPES.MEAL_MEAT;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return StructureBrain.TYPES.MEAL_GREAT_MEAT;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED:
			return StructureBrain.TYPES.MEAL_BAD_MIXED;
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return StructureBrain.TYPES.MEAL_MEDIUM_MIXED;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
			return StructureBrain.TYPES.MEAL_GREAT_MIXED;
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			return StructureBrain.TYPES.MEAL_GRASS;
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			return StructureBrain.TYPES.MEAL_FOLLOWER_MEAT;
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
			return StructureBrain.TYPES.MEAL_DEADLY;
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			return StructureBrain.TYPES.MEAL_POOP;
		case InventoryItem.ITEM_TYPE.MEAL_ROTTEN:
			return StructureBrain.TYPES.MEAL_ROTTEN;
		default:
			return StructureBrain.TYPES.NONE;
		}
	}

	public static InventoryItem.ITEM_TYPE GetMealFromStructureType(StructureBrain.TYPES structureType)
	{
		switch (structureType)
		{
		case StructureBrain.TYPES.MEAL_BERRIES:
			return InventoryItem.ITEM_TYPE.MEAL_BERRIES;
		case StructureBrain.TYPES.MEAL:
			return InventoryItem.ITEM_TYPE.MEAL;
		case StructureBrain.TYPES.MEAL_MEDIUM_VEG:
			return InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG;
		case StructureBrain.TYPES.MEAL_GREAT:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT;
		case StructureBrain.TYPES.MEAL_BAD_FISH:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_FISH;
		case StructureBrain.TYPES.MEAL_GOOD_FISH:
			return InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH;
		case StructureBrain.TYPES.MEAL_GREAT_FISH:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH;
		case StructureBrain.TYPES.MEAL_BAD_MEAT:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT;
		case StructureBrain.TYPES.MEAL_MEAT:
			return InventoryItem.ITEM_TYPE.MEAL_MEAT;
		case StructureBrain.TYPES.MEAL_GREAT_MEAT:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT;
		case StructureBrain.TYPES.MEAL_BAD_MIXED:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED;
		case StructureBrain.TYPES.MEAL_MEDIUM_MIXED:
			return InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED;
		case StructureBrain.TYPES.MEAL_GREAT_MIXED:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED;
		case StructureBrain.TYPES.MEAL_GRASS:
			return InventoryItem.ITEM_TYPE.MEAL_GRASS;
		case StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
			return InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT;
		case StructureBrain.TYPES.MEAL_DEADLY:
			return InventoryItem.ITEM_TYPE.MEAL_DEADLY;
		case StructureBrain.TYPES.MEAL_POOP:
			return InventoryItem.ITEM_TYPE.MEAL_POOP;
		case StructureBrain.TYPES.MEAL_ROTTEN:
			return InventoryItem.ITEM_TYPE.MEAL_ROTTEN;
		default:
			return InventoryItem.ITEM_TYPE.NONE;
		}
	}

	public static InventoryItem.ITEM_TYPE GetMealFromIngredientType(IngredientType ingredientType)
	{
		switch (ingredientType)
		{
		case IngredientType.FISH_LOW:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_FISH;
		case IngredientType.FISH_MEDIUM:
			return InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH;
		case IngredientType.FISH_HIGH:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH;
		case IngredientType.MEAT_LOW:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT;
		case IngredientType.MEAT_MEDIUM:
			return InventoryItem.ITEM_TYPE.MEAL_MEAT;
		case IngredientType.MEAT_HIGH:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT;
		case IngredientType.VEGETABLE_LOW:
			return InventoryItem.ITEM_TYPE.MEAL;
		case IngredientType.VEGETABLE_MEDIUM:
			return InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG;
		case IngredientType.VEGETABLE_HIGH:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT;
		case IngredientType.MIXED_LOW:
			return InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED;
		case IngredientType.MIXED_MEDIUM:
			return InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED;
		case IngredientType.MIXED_HIGH:
			return InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED;
		case IngredientType.FRUIT_LOW:
			return InventoryItem.ITEM_TYPE.MEAL_BERRIES;
		case IngredientType.SPECIAL_DEADLY:
			return InventoryItem.ITEM_TYPE.MEAL_DEADLY;
		case IngredientType.SPECIAL_FOLLOWER_MEAT:
			return InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT;
		case IngredientType.SPECIAL_GRASS:
			return InventoryItem.ITEM_TYPE.MEAL_GRASS;
		case IngredientType.SPECIAL_POOP:
			return InventoryItem.ITEM_TYPE.MEAL_POOP;
		default:
			return InventoryItem.ITEM_TYPE.NONE;
		}
	}

	public static IngredientType GetIngredientFromMealType(InventoryItem.ITEM_TYPE mealType)
	{
		switch (mealType)
		{
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			return IngredientType.FISH_LOW;
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
			return IngredientType.FISH_MEDIUM;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			return IngredientType.FISH_HIGH;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			return IngredientType.MEAT_LOW;
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
			return IngredientType.MEAT_MEDIUM;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return IngredientType.MEAT_HIGH;
		case InventoryItem.ITEM_TYPE.MEAL:
			return IngredientType.VEGETABLE_LOW;
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
			return IngredientType.VEGETABLE_MEDIUM;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			return IngredientType.VEGETABLE_HIGH;
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED:
			return IngredientType.MIXED_LOW;
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return IngredientType.MIXED_MEDIUM;
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
			return IngredientType.MIXED_HIGH;
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
			return IngredientType.FRUIT_LOW;
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
			return IngredientType.SPECIAL_DEADLY;
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			return IngredientType.SPECIAL_FOLLOWER_MEAT;
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			return IngredientType.SPECIAL_GRASS;
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			return IngredientType.SPECIAL_POOP;
		default:
			return IngredientType.NONE;
		}
	}

	public static InventoryItem.ITEM_TYPE[] GetAllMeals()
	{
		return new InventoryItem.ITEM_TYPE[17]
		{
			InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT,
			InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH,
			InventoryItem.ITEM_TYPE.MEAL_GREAT,
			InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED,
			InventoryItem.ITEM_TYPE.MEAL_MEAT,
			InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH,
			InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG,
			InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED,
			InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT,
			InventoryItem.ITEM_TYPE.MEAL_BAD_FISH,
			InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT,
			InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED,
			InventoryItem.ITEM_TYPE.MEAL,
			InventoryItem.ITEM_TYPE.MEAL_BERRIES,
			InventoryItem.ITEM_TYPE.MEAL_GRASS,
			InventoryItem.ITEM_TYPE.MEAL_POOP,
			InventoryItem.ITEM_TYPE.MEAL_DEADLY
		};
	}

	public static MealEffect[] GetMealEffects(InventoryItem.ITEM_TYPE mealType)
	{
		if (REQUIRES_LOC)
		{
			return new MealEffect[0];
		}
		switch (mealType)
		{
		case InventoryItem.ITEM_TYPE.MEAL:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.CausesIllPoopy,
					Chance = 5
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_MEAT:
			return new MealEffect[2]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.DropLoot,
					Chance = 25
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.InstantlyPoop,
					Chance = 15
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH:
			return new MealEffect[2]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.DropLoot,
					Chance = 25
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.InstantlyVomit,
					Chance = 15
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG:
			return new MealEffect[2]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.DropLoot,
					Chance = 25
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.CausesIllness,
					Chance = 5
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.IncreasesLoyalty,
					Chance = 20
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_BERRIES:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.InstantlyPoop,
					Chance = 15
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.IncreasesLoyalty,
					Chance = 50
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_BAD_FISH:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.CausesIllness,
					Chance = 10
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH:
			return new MealEffect[2]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.DropLoot,
					Chance = 25
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.RemovesIllness,
					Chance = 30
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.CausesExhaustion,
					Chance = 10
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.DropLoot,
					Chance = 75
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.IncreasesLoyalty,
					Chance = 10
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED:
			return new MealEffect[2]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.IncreasesLoyalty,
					Chance = 100
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.RemovesDissent,
					Chance = 100
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT:
			return new MealEffect[3]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.CausesIllness,
					Chance = 75
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.IncreasesLoyalty,
					Chance = 25
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.RemovesDissent,
					Chance = 40
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_GRASS:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.CausesIllness,
					Chance = 25
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_POOP:
			return new MealEffect[1]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.CausesIllPoopy,
					Chance = 50
				}
			};
		case InventoryItem.ITEM_TYPE.MEAL_DEADLY:
			return new MealEffect[2]
			{
				new MealEffect
				{
					MealEffectType = MealEffectType.InstantlyDie,
					Chance = 75
				},
				new MealEffect
				{
					MealEffectType = MealEffectType.DropLoot,
					Chance = 100
				}
			};
		default:
			return new MealEffect[0];
		}
	}

	public static void DoMealEffect(InventoryItem.ITEM_TYPE meal, FollowerBrain follower)
	{
		if (REQUIRES_LOC || (meal == InventoryItem.ITEM_TYPE.MEAL_GRASS && DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.GrassEater)))
		{
			return;
		}
		MealEffect[] mealEffects = GetMealEffects(meal);
		if (meal == InventoryItem.ITEM_TYPE.MEAL_DEADLY)
		{
			if (UnityEngine.Random.Range(0, 100) < 75)
			{
				InstantlyDie(follower);
			}
			else
			{
				DropLoot(follower);
			}
			return;
		}
		MealEffect[] array = mealEffects;
		FollowerTask task;
		for (int i = 0; i < array.Length; i++)
		{
			MealEffect mealEffect = array[i];
			int num = UnityEngine.Random.Range(0, 100);
			Debug.Log(string.Concat(mealEffect.MealEffectType, "  chance: ", num, " / ", mealEffect.Chance, "  ", (num < mealEffect.Chance).ToString()));
			if (num >= mealEffect.Chance)
			{
				continue;
			}
			switch (mealEffect.MealEffectType)
			{
			case MealEffectType.InstantlyPoop:
				if (IllnessBar.IllnessNormalized > 0.05f)
				{
					GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
					{
						task = new FollowerTask_InstantPoop();
						follower.HardSwapToTask(task);
					}));
				}
				break;
			case MealEffectType.InstantlyVomit:
				if (IllnessBar.IllnessNormalized > 0.05f)
				{
					GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
					{
						task = new FollowerTask_Vomit();
						follower.HardSwapToTask(task);
					}));
				}
				break;
			case MealEffectType.InstantlyDie:
				InstantlyDie(follower);
				return;
			case MealEffectType.CausesDissent:
				follower.MakeDissenter();
				break;
			case MealEffectType.CausesIllness:
				if ((meal != InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT || !DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Cannibal)) && (DataManager.Instance.OnboardedSickFollower || TimeManager.CurrentDay >= 10))
				{
					follower.MakeSick();
				}
				break;
			case MealEffectType.HealsIllness:
			{
				follower.Stats.Illness = 0f;
				FollowerBrainStats.StatStateChangedEvent onIllnessStateChanged2 = FollowerBrainStats.OnIllnessStateChanged;
				if (onIllnessStateChanged2 != null)
				{
					onIllnessStateChanged2(follower.Info.ID, FollowerStatState.Off, FollowerStatState.On);
				}
				break;
			}
			case MealEffectType.CausesExhaustion:
				GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
				{
					follower.MakeExhausted();
				}));
				break;
			case MealEffectType.CausesIllnessOrDissent:
				num = UnityEngine.Random.Range(0, 100);
				if (num > 50)
				{
					if (DataManager.Instance.OnboardedDissenter || TimeManager.CurrentDay >= 10)
					{
						follower.MakeDissenter();
					}
				}
				else if (DataManager.Instance.OnboardedSickFollower || TimeManager.CurrentDay >= 10)
				{
					follower.MakeSick();
				}
				break;
			case MealEffectType.CausesIllPoopy:
				if (DataManager.Instance.OnboardedSickFollower || TimeManager.CurrentDay >= 10)
				{
					follower.MakeSick();
					follower._directInfoAccess.CursedStateVariant = 1;
					GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
					{
						task = new FollowerTask_IllPoopy(true);
						follower.HardSwapToTask(task);
					}));
				}
				break;
			case MealEffectType.IncreasesLoyalty:
				follower.AddAdoration(FollowerBrain.AdorationActions.BigGift, null);
				break;
			case MealEffectType.RemovesDissent:
				if (follower.Info.CursedState == Thought.Dissenter)
				{
					follower.Stats.Reeducation = 0f;
				}
				break;
			case MealEffectType.RemovesIllness:
				if (follower.Info.CursedState == Thought.Ill)
				{
					follower.Stats.Illness = 0f;
					FollowerBrainStats.StatStateChangedEvent onIllnessStateChanged = FollowerBrainStats.OnIllnessStateChanged;
					if (onIllnessStateChanged != null)
					{
						onIllnessStateChanged(follower.Info.ID, FollowerStatState.Off, FollowerStatState.On);
					}
				}
				break;
			case MealEffectType.DropLoot:
				DropLoot(follower);
				break;
			}
		}
	}

	private static void InstantlyDie(FollowerBrain follower)
	{
		GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
		{
			if (FollowerManager.FindFollowerByID(follower.Info.ID) != null)
			{
				FollowerManager.FindFollowerByID(follower.Info.ID).Die(NotificationCentre.NotificationType.DiedFromDeadlyMeal);
			}
			else
			{
				follower.Die(NotificationCentre.NotificationType.DiedFromDeadlyMeal);
			}
		}));
	}

	private static void DropLoot(FollowerBrain follower)
	{
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		for (int i = 0; i < UnityEngine.Random.Range(7, 13); i++)
		{
			int num = UnityEngine.Random.Range(0, 100);
			if (num < 33)
			{
				list.Add(InventoryItem.ITEM_TYPE.BLACK_GOLD);
			}
			else if (num < 66)
			{
				list.Add(InventoryItem.ITEM_TYPE.LOG);
			}
			else
			{
				list.Add(InventoryItem.ITEM_TYPE.STONE);
			}
		}
		if (PlayerFarming.Location == FollowerLocation.Base && BaseLocationManager.Instance != null)
		{
			foreach (InventoryItem.ITEM_TYPE item in list)
			{
				PickUp pickUp = InventoryItem.Spawn(item, 1, follower.LastPosition);
				pickUp.transform.parent = BaseLocationManager.Instance.UnitLayer.transform;
				pickUp.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			return;
		}
		List<Structures_CollectedResourceChest> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_CollectedResourceChest>(FollowerLocation.Base);
		if (allStructuresOfType.Count <= 0)
		{
			return;
		}
		foreach (InventoryItem.ITEM_TYPE item2 in list)
		{
			allStructuresOfType[0].AddItem(item2, 1);
		}
	}

	private static IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	public static string GetEffectDescription(MealEffect mealEffect, InventoryItem.ITEM_TYPE mealType)
	{
		string text = string.Format(LocalizationManager.GetTranslation(string.Format("CookingData/{0}/Description", mealEffect.MealEffectType)), mealEffect.Chance);
		if (mealType == InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT && DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Cannibal) && mealEffect.MealEffectType == MealEffectType.CausesIllness)
		{
			Debug.Log("r: " + text.Colour(Color.yellow));
			text = text.Insert(31, "<s>") + "</s> \n \n<sprite name=\"icon_Trait_Cannibal\"> <color=#FFD201>" + FollowerTrait.GetLocalizedTitle(FollowerTrait.TraitType.Cannibal) + "</color> \n \n";
		}
		if (mealType == InventoryItem.ITEM_TYPE.MEAL_GRASS && DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.GrassEater))
		{
			text = text.Insert(31, "<s>") + "</s> \n \n<sprite name=\"icon_Trait_GrassEater\"> <color=#FFD201>" + FollowerTrait.GetLocalizedTitle(FollowerTrait.TraitType.GrassEater);
		}
		return text;
	}

	public static string GetMealSkin(StructureBrain.TYPES mealType)
	{
		switch (mealType)
		{
		case StructureBrain.TYPES.MEAL:
			return "Meals/Meal";
		case StructureBrain.TYPES.MEAL_MEAT:
			return "Meals/MealGood";
		case StructureBrain.TYPES.MEAL_BAD_FISH:
			return "Meals/MealBadFish";
		case StructureBrain.TYPES.MEAL_BAD_MEAT:
			return "Meals/MealBadMeat";
		case StructureBrain.TYPES.MEAL_BAD_MIXED:
			return "Meals/MealBadMixed";
		case StructureBrain.TYPES.MEAL_BERRIES:
			return "Meals/MealBerries";
		case StructureBrain.TYPES.MEAL_DEADLY:
			return "Meals/MealDeadly";
		case StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
			return "Meals/MealFollowerMeat";
		case StructureBrain.TYPES.MEAL_GOOD_FISH:
			return "Meals/MealGoodFish";
		case StructureBrain.TYPES.MEAL_GRASS:
			return "Meals/MealGrass";
		case StructureBrain.TYPES.MEAL_GREAT:
			return "Meals/MealGreat";
		case StructureBrain.TYPES.MEAL_GREAT_FISH:
			return "Meals/MealGreatFish";
		case StructureBrain.TYPES.MEAL_GREAT_MEAT:
			return "Meals/MealGreatMeat";
		case StructureBrain.TYPES.MEAL_GREAT_MIXED:
			return "Meals/MealGreatMixed";
		case StructureBrain.TYPES.MEAL_MEDIUM_MIXED:
			return "Meals/MealMediumMixed";
		case StructureBrain.TYPES.MEAL_MEDIUM_VEG:
			return "Meals/MealMediumVeg";
		case StructureBrain.TYPES.MEAL_MUSHROOMS:
			return "Meals/MealMushrooms";
		case StructureBrain.TYPES.MEAL_POOP:
			return "Meals/MealPoop";
		case StructureBrain.TYPES.MEAL_ROTTEN:
			return "Meals/MealRotten";
		default:
			return "Meals/Meal";
		}
	}

	public static float GetTotalHunger()
	{
		return HungerBar.Count + HungerBar.ReservedSatiation;
	}
}
