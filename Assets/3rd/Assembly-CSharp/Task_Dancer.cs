using System;
using UnityEngine;

public class Task_Dancer : Task
{
	private WorshipperInfoManager wim;

	private WorkPlace workplace;

	private Structure structure;

	private Worshipper w;

	private float PowerTimer;

	public Task_Dancer()
	{
		Type = Task_Type.DANCER;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		wim = t.GetComponent<WorshipperInfoManager>();
		workplace = WorkPlace.GetWorkPlaceByID(wim.v_i.WorkPlace);
		structure = workplace.gameObject.GetComponent<Structure>();
		w = t.GetComponent<Worshipper>();
	}

	private void OnProgressCompleted()
	{
		w.TimedAnimation("cheer", 2f, ClearTask);
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
		if (structure.Structure_Info.Progress >= structure.Structure_Info.ProgressTarget)
		{
			OnProgressCompleted();
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (Vector2.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > t.StoppingDistance)
			{
				Timer = 0f;
				PathToPosition(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position);
				TaskDoer taskDoer = t;
				taskDoer.EndOfPath = (Action)Delegate.Combine(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
			}
			else
			{
				ArriveAtWorkPlace();
			}
			break;
		case StateMachine.State.Moving:
			if (Vector2.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > t.StoppingDistance && (Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position);
			}
			break;
		case StateMachine.State.Dancing:
			DoWork();
			if (Vector2.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > t.StoppingDistance)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
	}

	private void GeneratePower()
	{
		if ((PowerTimer += Time.deltaTime) > 1f)
		{
			PowerTimer = 0f;
		}
	}

	private void DoWork()
	{
		if (structure.Structure_Info.WorkIsRequiredForProgress)
		{
			structure.Structure_Info.Progress += 1f * Time.deltaTime;
		}
	}

	private void ArriveAtWorkPlace()
	{
		t.transform.position = workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position;
		state.CURRENT_STATE = StateMachine.State.Dancing;
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
		workplace.ArrivedAtJob();
	}

	public override void ClearTask()
	{
		workplace.EndJob(t, wim.v_i.WorkPlaceSlot);
		base.ClearTask();
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
	}
}
