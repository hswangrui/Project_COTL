using System.Collections.Generic;
using UnityEngine;

public class Task_Dissenter : Task
{
	public Worshipper w;

	private float BubbleTimer;

	private float SpeechDuration;

	private List<Worshipper> ListeningWorshippers;

	private bool Spoken;

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Type = Task_Type.DISSENTER;
		w = t.GetComponent<Worshipper>();
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public override void TaskUpdate()
	{
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			if (!Spoken && Random.Range(0f, 1f) < 0.01f)
			{
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
				w.SetAnimation("Dissenters/dissenter", true);
				SpeechDuration = 8 + Random.Range(0, 4);
				BubbleTimer = 0.3f;
				ListeningWorshippers = new List<Worshipper>();
				Spoken = true;
			}
			else if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = Random.Range(4f, 6f);
				t.givePath(TownCentre.Instance.RandomPositionInTownCentre());
				Spoken = false;
			}
			break;
		case StateMachine.State.CustomAction0:
			foreach (Worshipper worshipper in Worshipper.worshippers)
			{
				if (Vector3.Distance(t.transform.position, worshipper.transform.position) < 4f && !ListeningWorshippers.Contains(worshipper) && worshipper.CurrentTask != null && worshipper.CurrentTask.Type == Task_Type.NONE && !worshipper.InConversation)
				{
					worshipper.CurrentTask = new Task_DessenterListener();
					worshipper.CurrentTask.StartTask(worshipper, t.gameObject);
					ListeningWorshippers.Add(worshipper);
				}
			}
			if ((BubbleTimer -= Time.deltaTime) < 0f)
			{
				WorshipperBubble.SPEECH_TYPE type = (WorshipperBubble.SPEECH_TYPE)(6 + Random.Range(0, 3));
				w.bubble.Play(type);
				BubbleTimer = 4 + Random.Range(0, 2);
				foreach (Worshipper listeningWorshipper in ListeningWorshippers)
				{
					listeningWorshipper.bubble.Play(type);
				}
			}
			if (!((SpeechDuration -= Time.deltaTime) < 0f))
			{
				break;
			}
			foreach (Worshipper listeningWorshipper2 in ListeningWorshippers)
			{
				listeningWorshipper2.ClearTask();
				listeningWorshipper2.CurrentTask = null;
			}
			state.CURRENT_STATE = StateMachine.State.Idle;
			break;
		}
	}
}
