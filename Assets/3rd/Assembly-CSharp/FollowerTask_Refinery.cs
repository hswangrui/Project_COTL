using UnityEngine;

public class FollowerTask_Refinery : FollowerTask
{
	private Structures_Refinery refineryStructure;

	private int refineryID;

	private Follower follower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Refinery;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return refineryStructure.Data.Location;
		}
	}

	public override bool BlockSocial
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
			return 25f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.Refiner)
		{
			return PriorityCategory.WorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_Refinery(int refineryID)
	{
		this.refineryID = refineryID;
		refineryStructure = StructureManager.GetStructureByID<Structures_Refinery>(refineryID);
	}

	protected override int GetSubTaskCode()
	{
		return refineryID;
	}

	public override void ClaimReservations()
	{
		StructureManager.GetStructureByID<Structures_Refinery>(refineryID).ReservedForTask = true;
	}

	public override void ReleaseReservations()
	{
		StructureManager.GetStructureByID<Structures_Refinery>(refineryID).ReservedForTask = false;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
		refineryStructure.Data.FollowerID = _brain.Info.ID;
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if ((TimeManager.IsNight && !base.Brain._directInfoAccess.WorkThroughNight) || refineryStructure.Data.QueuedResources.Count <= 0)
		{
			End();
		}
		if (base.State != FollowerTaskState.Doing)
		{
			return;
		}
		if (refineryStructure.Data.QueuedResources.Count > 0)
		{
			refineryStructure.Data.Progress += deltaGameTime * _brain.Info.ProductivityMultiplier;
			if (refineryStructure.Data.Progress > refineryStructure.RefineryDuration(refineryStructure.Data.QueuedResources[0]))
			{
				refineryStructure.RefineryDeposit();
				if (refineryStructure.Data.QueuedResources.Count <= 0)
				{
					Complete();
				}
			}
		}
		if ((bool)follower)
		{
			follower.transform.position = UpdateDestination(follower);
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		base.OnDoingBegin(follower);
		follower.FacePosition(refineryStructure.Data.Position);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("Buildings/refine", true);
		this.follower = follower;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Interaction_Refinery interaction_Refinery = FindRefinery();
		if (!(interaction_Refinery != null))
		{
			return refineryStructure.Data.Position + new Vector3(0f, 1f);
		}
		return interaction_Refinery.FollowerPosition.transform.position;
	}

	private Interaction_Refinery FindRefinery()
	{
		foreach (Interaction_Refinery refinery in Interaction_Refinery.Refineries)
		{
			if (refinery != null && refinery.Structure != null && refinery.Structure.Structure_Info != null && refinery.Structure.Structure_Info.ID == refineryID)
			{
				return refinery;
			}
		}
		return null;
	}
}
