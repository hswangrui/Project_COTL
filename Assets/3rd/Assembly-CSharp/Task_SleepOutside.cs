using System;
using UnityEngine;

public class Task_SleepOutside : Task
{
	public Vector3 SleepingPosition;

	private WorshipperInfoManager wim;

	public bool DoYawn = true;

	private Worshipper w;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		t.GetComponent<SimpleInventory>().DropItem();
		Type = Task_Type.SLEEP;
		w = t.GetComponent<Worshipper>();
	}

	public void FindNewSleepingPosition()
	{
		wim = t.GetComponent<WorshipperInfoManager>();
		SleepOutsidePosition(wim.v_i);
		SleepingPosition = wim.v_i.SleptOutsidePosition;
	}

	public static void SleepOutsidePosition(Villager_Info v_i)
	{
		v_i.SleptOutside = true;
		v_i.SleptOutsidePosition = TownCentre.Instance.RandomPositionInTownCentre();
	}

	private void EndOfPath()
	{
		Timer = 0f;
		if (w != null && DoYawn)
		{
			w.bubble.Play(WorshipperBubble.SPEECH_TYPE.HOME);
			w.TimedAnimation("sleepy", 3.8f, DoSleep);
		}
		else
		{
			DoSleep();
		}
		DoYawn = true;
		AstarPath.active.GetNearest(t.transform.position).node.Walkable = false;
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(EndOfPath));
	}

	private void DoSleep()
	{
		state.CURRENT_STATE = StateMachine.State.Sleeping;
		w.SetAnimation("sleep", true);
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		if (state.CURRENT_STATE != StateMachine.State.Sleeping)
		{
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Idle:
				if (Vector3.Distance(SleepingPosition, t.transform.position) > Farm.FarmTileSize)
				{
					Timer = 0f;
					PathToPosition(SleepingPosition);
					TaskDoer taskDoer = t;
					taskDoer.EndOfPath = (Action)Delegate.Combine(taskDoer.EndOfPath, new Action(EndOfPath));
				}
				else
				{
					EndOfPath();
				}
				break;
			case StateMachine.State.Moving:
				if ((Timer += Time.deltaTime) > 1f)
				{
					Timer = 0f;
					PathToPosition(SleepingPosition);
				}
				break;
			}
		}
		else
		{
			w.wim.v_i.Sleep += Time.deltaTime * 5f;
			if (w.wim.v_i.Sleep >= 60f)
			{
				ClearTask();
				t.ClearTask();
			}
		}
	}

	public override void ClearTask()
	{
		if (state.CURRENT_STATE == StateMachine.State.Sleeping && AstarPath.active != null)
		{
			AstarPath.active.GetNearest(t.transform.position).node.Walkable = true;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(EndOfPath));
		base.ClearTask();
	}
}
