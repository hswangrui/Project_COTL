using Spine;
using UnityEngine;

public class FollowerTask_Vomit : FollowerTask
{
	private PlacementRegion.TileGridTile ClosestTile;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Vomit;
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

	public override bool BlockTaskChanges
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
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State != FollowerTaskState.Doing)
		{
			SetState(FollowerTaskState.Doing);
		}
	}

	public override void ProgressTask()
	{
		if (_brain != null)
		{
			_brain.Stats.LastVomit = TimeManager.TotalElapsedGameTime;
			StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.VOMIT, 0);
			infoByType.FollowerID = _brain.Info.ID;
			PlacementRegion.TileGridTile tileGridTile = null;
			if ((bool)PlacementRegion.Instance)
			{
				tileGridTile = StructureManager.GetClosestTileGridTileAtWorldPosition(_brain.LastPosition, PlacementRegion.Instance.StructureInfo.Grid, 1f);
			}
			if (tileGridTile != null)
			{
				infoByType.GridTilePosition = tileGridTile.Position;
				StructureManager.BuildStructure(_brain.Location, infoByType, tileGridTile.WorldPosition, Vector2Int.one, false);
			}
			else
			{
				StructureManager.BuildStructure(_brain.Location, infoByType, _brain.LastPosition, Vector2Int.one, false);
			}
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (follower == null)
		{
			return _brain.LastPosition;
		}
		return follower.transform.position;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
	}

	public override void OnGoingToBegin(Follower follower)
	{
		base.OnGoingToBegin(follower);
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.TimedAnimation("Sick/chunder", 3.5f, base.End);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Vomit")
		{
			ProgressTask();
		}
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		base.SimDoingBegin(simFollower);
		GetDestination(null);
		End();
	}
}
