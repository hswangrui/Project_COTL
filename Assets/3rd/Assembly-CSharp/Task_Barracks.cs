using System;
using UnityEngine;

public class Task_Barracks : Task
{
	private WorshipperInfoManager wim;

	private WorkPlace workplace;

	private Structure structure;

	private Worshipper w;

	private SimpleSpineEventListener SpineEventListener;

	private Barracks barracks;

	public Task_Barracks()
	{
		Type = Task_Type.BARRACKS;
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		wim = t.GetComponent<WorshipperInfoManager>();
		workplace = WorkPlace.GetWorkPlaceByID(wim.v_i.WorkPlace);
		structure = workplace.gameObject.GetComponent<Structure>();
		w = t.GetComponent<Worshipper>();
		SpineEventListener = w.GetComponentInChildren<SimpleSpineEventListener>();
		SpineEventListener.OnSpineEvent += OnSpineEvent;
		barracks = structure.GetComponent<Barracks>();
	}

	private void OnSpineEvent(string EventName)
	{
		if (EventName == "hit")
		{
			barracks.SlotTargets[wim.v_i.WorkPlaceSlot].DealDamage(0f, t.gameObject, t.transform.position);
		}
	}

	public override void TaskUpdate()
	{
		base.TaskUpdate();
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
			break;
		case StateMachine.State.Moving:
			if (Vector2.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > t.StoppingDistance && (Timer += Time.deltaTime) > 1f)
			{
				Timer = 0f;
				PathToPosition(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position);
			}
			break;
		case StateMachine.State.CustomAction0:
			DoWork();
			if (Vector2.Distance(workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position, t.transform.position) > t.StoppingDistance)
			{
				state.CURRENT_STATE = StateMachine.State.Idle;
			}
			break;
		}
	}

	private void DoWork()
	{
		structure.Structure_Info.Progress += 1f * Time.deltaTime;
	}

	private void ArriveAtWorkPlace()
	{
		t.transform.position = workplace.Positions[wim.v_i.WorkPlaceSlot].transform.position;
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(t.transform.position, barracks.SlotTargets[wim.v_i.WorkPlaceSlot].transform.position);
		w.SetAnimation("barracks-training", true);
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
	}

	public override void ClearTask()
	{
		SpineEventListener.OnSpineEvent -= OnSpineEvent;
		workplace.EndJob(t, wim.v_i.WorkPlaceSlot);
		base.ClearTask();
		TaskDoer taskDoer = t;
		taskDoer.EndOfPath = (Action)Delegate.Remove(taskDoer.EndOfPath, new Action(ArriveAtWorkPlace));
	}
}
