using System;
using UnityEngine;

public class SimFollower
{
	public float TravelTimeGameMinutes;

	public FollowerBrain Brain { get; private set; }

	public bool Retired { get; private set; }

	public SimFollower(FollowerBrain brain)
	{
		Brain = brain;
		FollowerBrain brain2 = Brain;
		brain2.OnTaskChanged = (Action<FollowerTask, FollowerTask>)Delegate.Combine(brain2.OnTaskChanged, new Action<FollowerTask, FollowerTask>(OnTaskChanged));
		if (Brain.CurrentTask != null)
		{
			OnTaskChanged(Brain.CurrentTask, null);
			OnFollowerTaskStateChanged(FollowerTaskState.None, Brain.CurrentTask.State);
		}
	}

	public void Retire()
	{
		if (!Retired)
		{
			Retired = true;
			FollowerBrain brain = Brain;
			brain.OnTaskChanged = (Action<FollowerTask, FollowerTask>)Delegate.Remove(brain.OnTaskChanged, new Action<FollowerTask, FollowerTask>(OnTaskChanged));
			if (Brain.CurrentTask != null)
			{
				FollowerTask currentTask = Brain.CurrentTask;
				currentTask.OnFollowerTaskStateChanged = (FollowerTask.FollowerTaskDelegate)Delegate.Remove(currentTask.OnFollowerTaskStateChanged, new FollowerTask.FollowerTaskDelegate(OnFollowerTaskStateChanged));
				Brain.CurrentTask.SimCleanup(this);
				OnTaskChanged(null, Brain.CurrentTask);
			}
		}
	}

	public void TransitionFromFollower(Follower follower)
	{
		if (Brain.CurrentTask != null)
		{
			FollowerTaskState state = Brain.CurrentTask.State;
			if (state == FollowerTaskState.GoingTo)
			{
				float distanceToDestination = Brain.CurrentTask.GetDistanceToDestination();
				TravelTimeGameMinutes *= distanceToDestination / UnityEngine.Random.Range(10f, 50f);
			}
		}
	}

	private void OnTaskChanged(FollowerTask newTask, FollowerTask oldTask)
	{
		if (oldTask != null)
		{
			oldTask.OnFollowerTaskStateChanged = (FollowerTask.FollowerTaskDelegate)Delegate.Remove(oldTask.OnFollowerTaskStateChanged, new FollowerTask.FollowerTaskDelegate(OnFollowerTaskStateChanged));
		}
		if (newTask != null)
		{
			newTask.SimSetup(this);
			if (oldTask == null || oldTask.Type != FollowerTaskType.ChangeLocation)
			{
				newTask.ClaimReservations();
			}
			newTask.OnFollowerTaskStateChanged = (FollowerTask.FollowerTaskDelegate)Delegate.Combine(newTask.OnFollowerTaskStateChanged, new FollowerTask.FollowerTaskDelegate(OnFollowerTaskStateChanged));
		}
	}

	private void OnFollowerTaskStateChanged(FollowerTaskState oldState, FollowerTaskState newState)
	{
		if (Brain.CurrentTask != null)
		{
			switch (oldState)
			{
			case FollowerTaskState.Idle:
				Brain.CurrentTask.SimIdleEnd(this);
				break;
			case FollowerTaskState.GoingTo:
				Brain.CurrentTask.SimGoingToEnd(this);
				break;
			case FollowerTaskState.Doing:
				Brain.CurrentTask.SimDoingEnd(this);
				break;
			case FollowerTaskState.Finalising:
				Brain.CurrentTask.SimFinaliseEnd(this);
				break;
			case FollowerTaskState.Done:
				Debug.LogError("Should never change a Task state once it's Done!");
				break;
			}
			switch (newState)
			{
			case FollowerTaskState.None:
				Debug.LogError("Should never change a Task state back to None!");
				break;
			case FollowerTaskState.Idle:
				Brain.CurrentTask.SimIdleBegin(this);
				break;
			case FollowerTaskState.GoingTo:
				Brain.CurrentTask.SimGoingToBegin(this);
				break;
			case FollowerTaskState.Doing:
				Brain.CurrentTask.SimDoingBegin(this);
				break;
			case FollowerTaskState.Finalising:
				Brain.CurrentTask.SimFinaliseBegin(this);
				break;
			case FollowerTaskState.Done:
				Brain.CurrentTask.SimCleanup(this);
				break;
			case FollowerTaskState.WaitingForLocation:
				break;
			}
		}
	}

	public void Tick(float deltaGameTime)
	{
		if (Retired)
		{
			return;
		}
		if (Brain._directInfoAccess.MissionaryFinished && DataManager.Instance.Followers_OnMissionary_IDs.Contains(Brain.Info.ID) && (Brain.CurrentTask == null || Brain.CurrentTaskType != FollowerTaskType.MissionaryComplete))
		{
			Brain.HardSwapToTask(new FollowerTask_MissionaryComplete());
		}
		if (Brain.Location == FollowerLocation.Missionary && DataManager.Instance.Followers_OnMissionary_IDs.Contains(Brain.Info.ID) && (Brain.CurrentTask == null || Brain.CurrentTaskType != FollowerTaskType.MissionaryInProgress))
		{
			Brain.HardSwapToTask(new FollowerTask_OnMissionary());
		}
		if (Brain.Location == FollowerLocation.Demon && (Brain.CurrentTask == null || Brain.CurrentTaskType != FollowerTaskType.IsDemon))
		{
			Brain.HardSwapToTask(new FollowerTask_IsDemon());
		}
		if (Brain.CurrentTask == null || (!Brain.CurrentTask.BlockTaskChanges && !Brain.CurrentTask.BlockReactTasks))
		{
			Brain.SpeakersInRange = 0;
			foreach (SimFollower item in FollowerManager.SimFollowersAtLocation(Brain.Location))
			{
				if (item != this && UnityEngine.Random.Range(0f, 1f) < 0.001f && !FollowerManager.FollowerLocked(item.Brain.Info.ID) && Brain.CheckForInteraction(item.Brain))
				{
					break;
				}
			}
			foreach (StructureBrain item2 in StructureManager.StructuresAtLocation(Brain.Location))
			{
				if (UnityEngine.Random.Range(0f, 1f) < 0.001f && Brain.CheckForSimInteraction(item2))
				{
					break;
				}
			}
			if (Brain.SpeakersInRange > 0)
			{
				if (!Brain.ThoughtExists(Thought.PropogandaSpeakers))
				{
					Brain.AddThought(Thought.PropogandaSpeakers, true);
				}
			}
			else
			{
				Brain.RemoveThought(Thought.PropogandaSpeakers, true);
			}
		}
		Brain.Tick(deltaGameTime);
		FollowerTask currentTask = Brain.CurrentTask;
		if (currentTask != null && currentTask.State == FollowerTaskState.GoingTo && TravelTimeGameMinutes > 0f)
		{
			TravelTimeGameMinutes -= deltaGameTime;
			if (TravelTimeGameMinutes <= 0f)
			{
				Brain.CurrentTask.Arrive();
			}
		}
	}

	public void Die(NotificationCentre.NotificationType Notification, Vector3 corpseLocation)
	{
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
		infoByType.FollowerID = Brain.Info.ID;
		StructureManager.BuildStructure(Brain.Location, infoByType, corpseLocation, Vector2Int.one, true, delegate(GameObject r)
		{
			if (PlacementRegion.Instance != null)
			{
				PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(corpseLocation);
				if (closestTileGridTileAtWorldPosition != null)
				{
					r.GetComponent<Structure>().Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
				}
			}
		});
		Brain.Die(Notification);
	}
}
