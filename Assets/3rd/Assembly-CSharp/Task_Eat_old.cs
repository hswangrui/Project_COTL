using System;
using UnityEngine;

public class Task_Eat_old : Task
{
	private Worshipper worshipper;

	private SimpleInventory Inventory;

	private Structure structure;

	private Restaurant restaurant;

	private WorshipperBubble bubble;

	private bool MoveForTantrum;

	public Task_Eat_old()
	{
		Type = Task_Type.EAT;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		worshipper = t.GetComponent<Worshipper>();
		Inventory = t.GetComponent<SimpleInventory>();
		structure = TargetObject.transform.parent.gameObject.GetComponent<Structure>();
		restaurant = TargetObject.transform.parent.gameObject.GetComponent<Restaurant>();
		bubble = t.GetComponentInChildren<WorshipperBubble>();
		ClearOnComplete = true;
	}

	public override void TaskUpdate()
	{
		if (Inventory.GetItemType() != InventoryItem.ITEM_TYPE.MEAT)
		{
			if (Inventory.GetItemType() != 0)
			{
				Inventory.DropItem();
			}
			GetFoodFromStation();
		}
		else
		{
			SitAndEatFood();
		}
		base.TaskUpdate();
	}

	private void GetFoodFromStation()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
		{
			Timer = 0f;
			PathToPosition(structure.transform.position);
			TaskDoer taskDoer = t;
			taskDoer.EndOfPath = (Action)Delegate.Combine(taskDoer.EndOfPath, new Action(CollectFood));
			break;
		}
		case StateMachine.State.Moving:
			if (!MoveForTantrum)
			{
				if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					PathToPosition(structure.transform.position);
				}
			}
			else if ((Timer -= Time.deltaTime) <= 0f)
			{
				bubble.Play(WorshipperBubble.SPEECH_TYPE.FOOD);
				t.UsePathing = true;
				worshipper.EATEN_DINNNER = true;
				ClearTask();
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 0.5f)
			{
				if (structure.HasInventoryType(InventoryItem.ITEM_TYPE.MEAT))
				{
					structure.RemoveInventoryByType(InventoryItem.ITEM_TYPE.MEAT);
					Inventory.GiveItem(InventoryItem.ITEM_TYPE.MEAT);
					state.CURRENT_STATE = StateMachine.State.Idle;
					break;
				}
				Timer = UnityEngine.Random.Range(0.8f, 1.3f);
				t.UsePathing = false;
				state.CURRENT_STATE = StateMachine.State.Moving;
				t.state.facingAngle = UnityEngine.Random.Range(180, 360);
				t.speed = t.maxSpeed;
				MoveForTantrum = true;
				restaurant.RemoveFromPositions(t.gameObject);
			}
			break;
		}
	}

	private void CollectFood()
	{
		Timer = 0f;
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(CollectFood));
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
	}

	private void SitAndEatFood()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
		{
			Timer = 0f;
			PathToPosition(TargetObject.transform.position);
			TaskDoer taskDoer = t;
			taskDoer.EndOfPath = (Action)Delegate.Combine(taskDoer.EndOfPath, new Action(CollectFood));
			break;
		}
		case StateMachine.State.Moving:
			if ((Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(TargetObject.gameObject.transform.position);
			}
			break;
		case StateMachine.State.CustomAction0:
			if ((Timer += Time.deltaTime) > 5f)
			{
				Inventory.RemoveItem();
				worshipper.EATEN_DINNNER = true;
				restaurant.RemoveFromPositions(t.gameObject);
				ClearTask();
			}
			break;
		}
	}

	private void EndOfPath()
	{
		Timer = 0f;
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(EndOfPath));
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
	}

	public override void ClearTask()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(EndOfPath));
		TaskDoer taskDoer2 = t;
		taskDoer2.EndOfPath = (Action)Delegate.Remove(taskDoer2.EndOfPath, new Action(CollectFood));
		base.ClearTask();
	}
}
