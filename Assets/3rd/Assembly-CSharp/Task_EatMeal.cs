using System.Collections;
using UnityEngine;

public class Task_EatMeal : Task
{
	public Meal Meal;

	public Worshipper Worshipper;

	private bool Arrived;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Type = Task_Type.EAT;
		Worshipper = t.GetComponent<Worshipper>();
		Worshipper.GoToAndStop(Meal.gameObject, EatMeal, Meal.gameObject, false);
	}

	public override void TaskUpdate()
	{
		if (!Arrived && (Meal.TakenByPlayer || Meal == null))
		{
			ClearTask();
		}
	}

	private void EatMeal()
	{
		t.StartCoroutine(EatMealRoutine());
	}

	private IEnumerator EatMealRoutine()
	{
		Arrived = true;
		if (Meal == null)
		{
			ClearTask();
			yield break;
		}
		bool MealIsRotten = Meal.StructureInfo.Rotten;
		Worshipper.SetAnimation("Food/food_eat", true);
		yield return new WaitForSeconds(5f);
		Worshipper.SetAnimation("Food/food-finish", true);
		yield return new WaitForSeconds(1.8f);
		Worshipper.Hunger = 100f;
		Worshipper.wim.v_i.Starve = 0f;
		Worshipper.wim.v_i.Complaint_Food = false;
		Worshipper.BeenFed = true;
		if (MealIsRotten)
		{
			Worshipper.wim.v_i.Illness = Villager_Info.IllnessThreshold;
		}
		ClearTask();
	}

	public override void ClearTask()
	{
		Worshipper.GoToAndStopping = false;
		base.ClearTask();
		t.ClearPaths();
		t.StopAllCoroutines();
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
