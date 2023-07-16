using System.Collections.Generic;

public class Structures_Kitchen : Structures_CookingFire
{
	public delegate void CookEvent();

	public Structures_FoodStorage FoodStorage;

	public bool IsContainingFoodStorage
	{
		get
		{
			return FoodStorage != null;
		}
	}

	public event CookEvent OnMealFinishedCooking;

	public int GetAllPossibleMeals()
	{
		return Data.QueuedMeals.Count;
	}

	public Interaction_Kitchen.QueuedMeal GetBestPossibleMeal()
	{
		if (Data.QueuedMeals.Count > 0)
		{
			return Data.QueuedMeals[0];
		}
		return null;
	}

	public override void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> tasks)
	{
		if (activity == ScheduledActivity.Work && !ReservedForTask && GetAllPossibleMeals() > 0 && Data.QueuedResources.Count < ((FoodStorage != null) ? FoodStorage.Capacity : 10))
		{
			FollowerTask_Cook followerTask_Cook = new FollowerTask_Cook(Data.ID);
			tasks.Add(followerTask_Cook.Priorty, followerTask_Cook);
		}
	}

	public void MealCooked()
	{
		CookEvent onMealFinishedCooking = this.OnMealFinishedCooking;
		if (onMealFinishedCooking != null)
		{
			onMealFinishedCooking();
		}
	}
}
