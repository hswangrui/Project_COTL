using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FollowerTask_ClaimDwelling : FollowerTask
{
	public const int CLAIM_DURATION_GAME_MINUTES = 2;

	private Dwelling.DwellingAndSlot _dwellingAndSlot;

	private Structures_Bed _dwelling;

	private float _progress;

	public static Action<int> OnClaimHome;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ClaimDwelling;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _dwelling.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _dwellingAndSlot.ID;
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

	public FollowerTask_ClaimDwelling(Dwelling.DwellingAndSlot dwellingAndSlot)
	{
		_dwellingAndSlot = dwellingAndSlot;
		_dwelling = StructureManager.GetStructureByID<Structures_Bed>(_dwellingAndSlot.ID);
	}

	protected override int GetSubTaskCode()
	{
		return _dwellingAndSlot.ID * 100 + _dwellingAndSlot.dwellingslot;
	}

	public override void ClaimReservations()
	{
		Structures_Bed structureByID = StructureManager.GetStructureByID<Structures_Bed>(_dwellingAndSlot.ID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		Structures_Bed structureByID = StructureManager.GetStructureByID<Structures_Bed>(_dwellingAndSlot.ID);
		if (structureByID != null)
		{
			structureByID.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnEnd()
	{
		_brain.AssignDwelling(_dwellingAndSlot, _brain.Info.ID, true);
		Action<int> onClaimHome = OnClaimHome;
		if (onClaimHome != null)
		{
			onClaimHome(_brain.Info.ID);
		}
		base.OnEnd();
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			_progress += deltaGameTime;
			if (_progress >= 2f)
			{
				End();
			}
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Vector3 result = StructureManager.GetStructureByID<Structures_Bed>(_dwellingAndSlot.ID).Data.Position;
		Dwelling dwelling = FindDwelling();
		if (dwelling != null)
		{
			result = dwelling.GetDwellingSlotPosition(_dwellingAndSlot.dwellingslot);
		}
		return result;
	}

	public override void OnDoingBegin(Follower follower)
	{
		follower.State.CURRENT_STATE = StateMachine.State.CustomAction0;
	}

	public override void OnFinaliseBegin(Follower follower)
	{
		follower.TimedAnimation("Reactions/react-happy1", 2.1f, delegate
		{
			_003C_003En__0(follower);
		});
	}

	private Dwelling FindDwelling()
	{
		return Dwelling.GetDwellingByID(_dwellingAndSlot.ID);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0(Follower follower)
	{
		base.OnFinaliseBegin(follower);
	}
}
