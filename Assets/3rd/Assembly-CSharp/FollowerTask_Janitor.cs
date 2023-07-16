using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_Janitor : FollowerTask
{
	public const float CLEANING_DURATION_GAME_MINUTES = 4.2f;

	private int _janitorID;

	private int _targetID = -1;

	private Structures_JanitorStation _janitorStation;

	private float _progress;

	private float _gameTimeSinceLastProgress;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Janitor;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _janitorStation.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _janitorID;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override float Priorty
	{
		get
		{
			return 22f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.Janitor)
		{
			return PriorityCategory.WorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_Janitor(int janitorID)
	{
		_janitorID = janitorID;
		_janitorStation = StructureManager.GetStructureByID<Structures_JanitorStation>(_janitorID);
	}

	protected override int GetSubTaskCode()
	{
		return _janitorID;
	}

	public override void ClaimReservations()
	{
		Structures_JanitorStation structureByID = StructureManager.GetStructureByID<Structures_JanitorStation>(_janitorID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = true;
		}
		StructureBrain structureByID2 = StructureManager.GetStructureByID<StructureBrain>(_targetID);
		if (structureByID2 != null)
		{
			structureByID2.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		Structures_JanitorStation structureByID = StructureManager.GetStructureByID<Structures_JanitorStation>(_janitorID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = false;
		}
		StructureBrain structureByID2 = StructureManager.GetStructureByID<StructureBrain>(_targetID);
		if (structureByID2 != null)
		{
			structureByID2.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			float num = 1f;
			_gameTimeSinceLastProgress += deltaGameTime * num;
			ProgressTask();
		}
	}

	public override void ProgressTask()
	{
		StructureBrain structureByID = StructureManager.GetStructureByID<StructureBrain>(_targetID);
		if (structureByID == null)
		{
			Loop();
		}
		else
		{
			if (!structureByID.ReservedForTask)
			{
				return;
			}
			_progress += _gameTimeSinceLastProgress;
			_gameTimeSinceLastProgress = 0f;
			if (_progress >= 4.2f)
			{
				_progress = 0f;
				StructureBrain structureByID2 = StructureManager.GetStructureByID<StructureBrain>(_targetID);
				if (structureByID2 != null)
				{
					structureByID2.Remove();
				}
				Loop();
			}
		}
	}

	private int GetNextStructure()
	{
		List<StructureBrain> list = new List<StructureBrain>();
		list.AddRange(StructureManager.GetAllStructuresOfType(Location, StructureBrain.TYPES.POOP));
		list.AddRange(StructureManager.GetAllStructuresOfType(Location, StructureBrain.TYPES.VOMIT));
		List<StructureBrain> list2 = new List<StructureBrain>();
		foreach (StructureBrain item in list)
		{
			if (!item.ReservedByPlayer && !item.ReservedForTask)
			{
				list2.Add(item);
			}
		}
		if (list2.Count > 0)
		{
			StructureBrain structureBrain = null;
			foreach (StructureBrain item2 in list2)
			{
				if (structureBrain == null || Vector3.Distance(item2.Data.Position, _brain.LastPosition) < Vector3.Distance(structureBrain.Data.Position, _brain.LastPosition))
				{
					structureBrain = item2;
				}
			}
			return structureBrain.Data.ID;
		}
		return -1;
	}

	private void Loop()
	{
		int nextStructure = GetNextStructure();
		if (nextStructure != -1)
		{
			StructureBrain structureByID = StructureManager.GetStructureByID<StructureBrain>(nextStructure);
			_targetID = structureByID.Data.ID;
			structureByID.ReservedForTask = true;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			End();
		}
	}

	public Structure GetStructure(int ID)
	{
		foreach (Structure structure in Structure.Structures)
		{
			if (structure != null && structure.Brain != null && structure.Brain.Data != null && structure.Brain.Data.ID == ID)
			{
				return structure;
			}
		}
		return null;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		StructureBrain structureByID = StructureManager.GetStructureByID<StructureBrain>(_targetID);
		if (structureByID != null)
		{
			return structureByID.Data.Position;
		}
		JanitorStation janitorStation = FindJanitaorStation();
		if (janitorStation != null)
		{
			return janitorStation.transform.position;
		}
		End();
		return Vector3.zero;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (_targetID != -1)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-janitor");
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
		if (_targetID == -1)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-janitor");
			ProgressTask();
			return;
		}
		Structure structure = GetStructure(_targetID);
		if (!(structure != null))
		{
			return;
		}
		follower.FacePosition(structure.transform.position);
		if (structure.Brain.ReservedForTask)
		{
			follower.TimedAnimation("sweep-floor", 4.2f, delegate
			{
				ProgressTask();
			});
		}
	}

	public override void Cleanup(Follower follower)
	{
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
		base.Cleanup(follower);
	}

	private JanitorStation FindJanitaorStation()
	{
		JanitorStation result = null;
		foreach (JanitorStation janitorStation in JanitorStation.JanitorStations)
		{
			if (janitorStation.StructureInfo.ID == _janitorID)
			{
				result = janitorStation;
				break;
			}
		}
		return result;
	}
}
