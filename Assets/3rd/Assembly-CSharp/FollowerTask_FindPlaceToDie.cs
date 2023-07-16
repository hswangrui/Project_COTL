using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_FindPlaceToDie : FollowerTask
{
	private NotificationCentre.NotificationType _deathNotificationType;

	private PlacementRegion.TileGridTile ClosestTile;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.FindPlaceToDie;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override bool DisablePickUpInteraction
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

	public FollowerTask_FindPlaceToDie(NotificationCentre.NotificationType deathNotificationType)
	{
		_deathNotificationType = deathNotificationType;
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

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		End();
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Vector3 result = Vector3.zero;
		List<StructureBrain> structuresFromRole = StructureManager.GetStructuresFromRole(FollowerRole.Worshipper);
		if (structuresFromRole.Count > 0)
		{
			result = (structuresFromRole[0] as Structures_Shrine).Data.Position + (Vector3)Random.insideUnitCircle.normalized * 3f;
		}
		else
		{
			ClosestTile = StructureManager.GetBestWasteTile(_brain.Location);
			if (ClosestTile != null)
			{
				ClosestTile.ReservedForWaste = true;
				result = ClosestTile.WorldPosition;
			}
		}
		return result;
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		follower.DieWithAnimation("tantrum-hungry", 3.2f, "dead", true, 1, _deathNotificationType);
	}

	public override void SimFinaliseEnd(SimFollower simFollower)
	{
		GetDestination(null);
		simFollower.Die(_deathNotificationType, _currentDestination.Value);
	}
}
