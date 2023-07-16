using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskDoer : UnitObject
{
	[HideInInspector]
	public bool InConversation;

	public static List<TaskDoer> TaskDoers = new List<TaskDoer>();

	public Task _CurrentTask;

	public int Position;

	public List<Task> TaskList = new List<Task>();

	public System.Action OnCollision;

	public virtual Task CurrentTask
	{
		get
		{
			return _CurrentTask;
		}
		set
		{
			if (value != null)
			{
				value.StartTask(this, null);
			}
			_CurrentTask = value;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Position = TaskDoers.Count;
		TaskDoers.Add(this);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		TaskDoers.Remove(this);
	}

	public static void AddNewTaskToTeam(Task_Type TaskType, Health.Team Team, bool ClearOnComplete)
	{
		foreach (TaskDoer taskDoer in TaskDoers)
		{
			if (taskDoer.health.team == Team)
			{
				taskDoer.AddNewTask(TaskType, ClearOnComplete);
			}
		}
	}

	public void AddNewTask(Task_Type TaskType, bool ClearOnComplete)
	{
		if (CurrentTask != null)
		{
			CurrentTask.ClearTask();
		}
		CurrentTask = Task.GetTaskByType(TaskType);
		CurrentTask.ClearOnComplete = ClearOnComplete;
	}

	public void AddNewTask(Task task, bool ClearOnComplete)
	{
		if (CurrentTask != null)
		{
			CurrentTask.ClearTask();
		}
		CurrentTask = task;
		CurrentTask.ClearOnComplete = ClearOnComplete;
	}

	public void ClearTask()
	{
		CurrentTask = null;
	}

	public override void Update()
	{
		base.Update();
	}

	public void LookOutForEnemies()
	{
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team != health.team && allUnit.team != 0 && !allUnit.InanimateObject && (health.team != Health.Team.PlayerTeam || (health.team == Health.Team.PlayerTeam && allUnit.team != Health.Team.DangerousAnimals)) && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < 5f && CheckLineOfSight(allUnit.gameObject.transform.position, Vector2.Distance(allUnit.gameObject.transform.position, base.transform.position)))
			{
				AddNewTask(Task_Type.COMBAT, true);
				break;
			}
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (OnCollision != null)
		{
			OnCollision();
		}
	}
}
