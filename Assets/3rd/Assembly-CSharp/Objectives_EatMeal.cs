using System;
using I2.Loc;

[Serializable]
public class Objectives_EatMeal : ObjectivesData
{
	[Serializable]
	public class FinalizedData_EatMeal : ObjectivesDataFinalized
	{
		public StructureBrain.TYPES MealType;

		public string TargetFollowerName;

		public override string GetText()
		{
			string localizedName = CookingData.GetLocalizedName(CookingData.GetMealFromStructureType(MealType));
			return string.Format(ScriptLocalization.Objectives.EatMeal, TargetFollowerName, localizedName);
		}
	}

	public StructureBrain.TYPES MealType;

	public int TargetFollower;

	private bool complete;

	public override string Text
	{
		get
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(TargetFollower, true);
			string arg = ((infoByID != null) ? infoByID.Name : null);
			string localizedName = CookingData.GetLocalizedName(CookingData.GetMealFromStructureType(MealType));
			return string.Format(ScriptLocalization.Objectives.EatMeal, arg, localizedName);
		}
	}

	public Objectives_EatMeal()
	{
	}

	public Objectives_EatMeal(string groupId, StructureBrain.TYPES mealType, float questExpireDuration = -1f)
		: base(groupId, questExpireDuration)
	{
		Type = Objectives.TYPES.EAT_MEAL;
		MealType = mealType;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		complete = false;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		FinalizedData_EatMeal obj = new FinalizedData_EatMeal
		{
			GroupId = GroupId,
			Index = Index,
			MealType = MealType
		};
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(TargetFollower, true);
		obj.TargetFollowerName = ((infoByID != null) ? infoByID.Name : null);
		obj.UniqueGroupID = UniqueGroupID;
		return obj;
	}

	protected override bool CheckComplete()
	{
		base.CheckComplete();
		return complete;
	}

	public override void Update()
	{
		base.Update();
		if (!IsFailed && TargetFollower != -1 && FollowerInfo.GetInfoByID(TargetFollower) == null)
		{
			Failed();
		}
	}

	public void CheckComplete(StructureBrain.TYPES mealType, int targetFollowerID_1)
	{
		if (mealType == MealType && TargetFollower == targetFollowerID_1)
		{
			complete = true;
		}
		else if (FollowerInfo.GetInfoByID(TargetFollower) == null)
		{
			Failed();
		}
		CheckComplete();
	}
}
