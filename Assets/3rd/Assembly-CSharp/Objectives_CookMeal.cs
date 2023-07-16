using System;
using I2.Loc;

[Serializable]
public class Objectives_CookMeal : ObjectivesData
{
	[Serializable]
	public class FinalizedData_CookMeal : ObjectivesDataFinalized
	{
		public InventoryItem.ITEM_TYPE MealType;

		public int Target;

		public int Count;

		public override string GetText()
		{
			string cookMeals = ScriptLocalization.Objectives.CookMeals;
			if (MealType == InventoryItem.ITEM_TYPE.NONE)
			{
				return string.Format(cookMeals, ScriptLocalization.Interactions.Meals, Count, Target);
			}
			return string.Format(cookMeals, CookingData.GetLocalizedName(MealType), Count, Target);
		}
	}

	public InventoryItem.ITEM_TYPE MealType;

	public int Count;

	public int StartingAmount = -1;

	public override string Text
	{
		get
		{
			int cookedMeal = CookingData.GetCookedMeal(MealType);
			cookedMeal -= StartingAmount;
			string cookMeals = ScriptLocalization.Objectives.CookMeals;
			if (MealType == InventoryItem.ITEM_TYPE.NONE)
			{
				return string.Format(cookMeals, ScriptLocalization.Interactions.Meals, cookedMeal, Count);
			}
			return string.Format(cookMeals, CookingData.GetLocalizedName(MealType), cookedMeal, Count);
		}
	}

	public Objectives_CookMeal()
	{
	}

	public Objectives_CookMeal(string groupId, InventoryItem.ITEM_TYPE mealType, int count, float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.COOK_MEALS;
		MealType = mealType;
		Count = count;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		if (initialAssigning)
		{
			StartingAmount = CookingData.GetCookedMeal(MealType);
		}
		ObjectiveManager.CheckObjectives(Type);
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		int cookedMeal = CookingData.GetCookedMeal(MealType);
		cookedMeal -= StartingAmount;
		return new FinalizedData_CookMeal
		{
			GroupId = GroupId,
			Index = Index,
			MealType = MealType,
			Count = cookedMeal,
			Target = Count,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		return CookingData.GetCookedMeal(MealType) - StartingAmount >= Count;
	}
}
