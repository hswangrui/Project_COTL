using UnityEngine;

public class FollowerTask_InHealing : FollowerTask
{
	private float CacheRest;

	private int _prisonID;

	private StructureBrain _prison;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.InHealing;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _prison.Data.Location;
		}
	}

	public override bool DisablePickUpInteraction
	{
		get
		{
			return true;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _prisonID;
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

	public FollowerTask_InHealing(int prisonID)
	{
		_prisonID = prisonID;
		_prison = StructureManager.GetStructureByID<StructureBrain>(_prisonID);
	}

	protected override int GetSubTaskCode()
	{
		return _prisonID;
	}

	protected override void OnStart()
	{
		StructureManager.GetStructureByID<StructureBrain>(_prisonID);
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnEnd()
	{
		StructureManager.GetStructureByID<StructureBrain>(_prisonID).Data.FollowerID = -1;
		base.OnEnd();
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		HealingBay healingBay = FindPrison();
		if (!(healingBay == null))
		{
			return healingBay.HealingBayLocation.position;
		}
		return follower.transform.position;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Sick/idle-sick");
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Sick/idle-sick");
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		HealingBay healingBay = FindPrison();
		if (healingBay != null)
		{
			follower.GoTo(healingBay.HealingBayExitLocation.transform.position, base.Complete);
		}
		else
		{
			Complete();
		}
	}

	private HealingBay FindPrison()
	{
		HealingBay result = null;
		foreach (HealingBay healingBay in HealingBay.HealingBays)
		{
			if (healingBay.StructureInfo.ID == _prisonID)
			{
				result = healingBay;
				break;
			}
		}
		return result;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 100f * 0.7f * (deltaGameTime / 240f);
	}
}
