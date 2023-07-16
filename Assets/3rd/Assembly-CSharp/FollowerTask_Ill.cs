using System;
using Spine;
using UnityEngine;

public class FollowerTask_Ill : FollowerTask
{
	private float _gameTimeToNextStateUpdate;

	private bool GoToVomit;

	public bool ForceFirstVomit = true;

	private PlacementRegion.TileGridTile ClosestTile;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Ill;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override bool BlockSermon
	{
		get
		{
			return true;
		}
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	public override void ClaimReservations()
	{
		if (ClosestTile != null)
		{
			ClosestTile.ReservedForWaste = true;
		}
	}

	public override void ReleaseReservations()
	{
		if (ClosestTile != null)
		{
			ClosestTile.ReservedForWaste = false;
		}
	}

	protected override void OnArrive()
	{
		if (GoToVomit)
		{
			SetState(FollowerTaskState.Doing);
		}
		else
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_brain.Stats.Illness <= 0f)
		{
			End();
			return;
		}
		if (ForceFirstVomit && IllnessBar.IllnessNormalized > 0.05f)
		{
			ForceFirstVomit = false;
			GoToVomit = true;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		if (_state != FollowerTaskState.Idle)
		{
			return;
		}
		_gameTimeToNextStateUpdate -= deltaGameTime;
		if (_gameTimeToNextStateUpdate <= 0f)
		{
			GoToVomit = false;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
			_gameTimeToNextStateUpdate = UnityEngine.Random.Range(3f, 5f);
		}
		else if (TimeManager.TotalElapsedGameTime - _brain.Stats.LastVomit > 360f || UnityEngine.Random.Range(0f, 1f) <= 0.002f)
		{
			if (IllnessBar.IllnessNormalized > 0.05f)
			{
				GoToVomit = true;
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
			}
			else
			{
				GoToVomit = false;
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
				_gameTimeToNextStateUpdate = UnityEngine.Random.Range(3f, 5f);
			}
		}
	}

	public override void ProgressTask()
	{
		_brain.Stats.LastVomit = TimeManager.TotalElapsedGameTime;
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.VOMIT, 0);
		infoByType.FollowerID = _brain.Info.ID;
		if (!_currentDestination.HasValue)
		{
			StructureManager.BuildStructure(_brain.Location, infoByType, _brain.LastPosition, Vector2Int.one, false);
			return;
		}
		PlacementRegion.TileGridTile tileGridTile = null;
		if ((bool)PlacementRegion.Instance)
		{
			tileGridTile = StructureManager.GetClosestTileGridTileAtWorldPosition(_currentDestination.Value, PlacementRegion.Instance.StructureInfo.Grid);
		}
		if (tileGridTile != null)
		{
			infoByType.GridTilePosition = tileGridTile.Position;
			StructureManager.BuildStructure(_brain.Location, infoByType, tileGridTile.WorldPosition, Vector2Int.one, false);
		}
		else
		{
			StructureManager.BuildStructure(_brain.Location, infoByType, _currentDestination.Value, Vector2Int.one, false);
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (GoToVomit)
		{
			ClosestTile = StructureManager.GetBestWasteTile(_brain.Location);
			if (ClosestTile != null)
			{
				ClosestTile.ReservedForWaste = true;
				return ClosestTile.WorldPosition;
			}
			if (follower == null)
			{
				return _brain.LastPosition;
			}
			return follower.transform.position;
		}
		return TownCentre.RandomCircleFromTownCentre(10f);
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 100f;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Sick/idle-sick");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/walk-sick");
	}

	public override void OnDoingBegin(Follower follower)
	{
		GoToVomit = false;
		follower.TimedAnimation("Sick/chunder", 3.5f, delegate
		{
			SetState(FollowerTaskState.Idle);
		});
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Vomit")
		{
			ProgressTask();
		}
	}

	protected override float VomitChange(float deltaGameTime)
	{
		return 0f;
	}

	public override void SimSetup(SimFollower simFollower)
	{
		base.SimSetup(simFollower);
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (structure.Type == StructureBrain.TYPES.VOMIT && structure.FollowerID == _brain.Info.ID)
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		base.SimDoingBegin(simFollower);
		GetDestination(null);
		ProgressTask();
	}
}
