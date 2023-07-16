using UnityEngine;

public class FollowerTask_OfferingShrine : FollowerTask
{
	private Structures_OfferingShrine OfferingShrineStructure;

	private int refineryID;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.PassivePray;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return OfferingShrineStructure.Data.Location;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (FollowerRole)
		{
		case FollowerRole.Worker:
			return PriorityCategory.WorkPriority;
		case FollowerRole.Worshipper:
		case FollowerRole.Lumberjack:
		case FollowerRole.Monk:
			return PriorityCategory.Low;
		default:
			return PriorityCategory.Low;
		}
	}

	public FollowerTask_OfferingShrine(int refineryID)
	{
		this.refineryID = refineryID;
		OfferingShrineStructure = StructureManager.GetStructureByID<Structures_OfferingShrine>(refineryID);
	}

	protected override int GetSubTaskCode()
	{
		return refineryID;
	}

	public override void ClaimReservations()
	{
		Structures_OfferingShrine structureByID = StructureManager.GetStructureByID<Structures_OfferingShrine>(refineryID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		Structures_OfferingShrine structureByID = StructureManager.GetStructureByID<Structures_OfferingShrine>(refineryID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State != FollowerTaskState.Doing)
		{
			return;
		}
		if (OfferingShrineStructure.Data.Inventory.Count <= 0)
		{
			OfferingShrineStructure.Data.Progress += deltaGameTime;
			if (OfferingShrineStructure.Data.Progress > 30f)
			{
				Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
				OfferingShrineStructure.Complete((follower == null) ? Vector3.zero : follower.transform.position);
				if (OfferingShrineStructure.Data.Inventory.Count > 0)
				{
					Complete();
				}
			}
		}
		else
		{
			Complete();
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		base.OnDoingBegin(follower);
		follower.FacePosition(OfferingShrineStructure.Data.Position);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("idle-ritual-up", true);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return OfferingShrineStructure.Data.Position + new Vector3(0f, 0.2f);
	}
}
