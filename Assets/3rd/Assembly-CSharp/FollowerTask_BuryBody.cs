using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FollowerTask_BuryBody : FollowerTask
{
	private int _corpseID;

	private Structures_DeadWorshipper _corpse;

	private int _graveID;

	private Structures_Grave _grave;

	private bool _haveCorpse;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.BuryBody;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _grave.Data.Location;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _graveID;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
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
			return 25f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		switch (FollowerRole)
		{
		case FollowerRole.Worker:
			return PriorityCategory.Low;
		case FollowerRole.Worshipper:
		case FollowerRole.Lumberjack:
		case FollowerRole.Farmer:
		case FollowerRole.Monk:
			return PriorityCategory.Low;
		default:
			return PriorityCategory.Low;
		}
	}

	public FollowerTask_BuryBody(int corpseID, int graveID)
	{
		_corpseID = corpseID;
		_corpse = StructureManager.GetStructureByID<Structures_DeadWorshipper>(_corpseID);
		_graveID = graveID;
		_grave = StructureManager.GetStructureByID<Structures_Grave>(_graveID);
	}

	protected override int GetSubTaskCode()
	{
		return _corpseID * 1000 + _graveID;
	}

	public override void ClaimReservations()
	{
		if (_corpse != null)
		{
			_corpse.ReservedForTask = true;
		}
		_grave.ReservedForTask = true;
	}

	public override void ReleaseReservations()
	{
		if (_corpse != null)
		{
			_corpse.ReservedForTask = false;
		}
		_grave.ReservedForTask = false;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	public override void ProgressTask()
	{
		if (!_haveCorpse)
		{
			_haveCorpse = true;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			_grave.Data.FollowerID = _corpse.Data.FollowerID;
			StructureManager.RemoveStructure(_corpse);
			_corpseID = 0;
			_corpse = null;
			End();
		}
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Moving);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (!_haveCorpse)
		{
			return StructureManager.GetStructureByID<Structures_DeadWorshipper>(_corpseID).Data.Position;
		}
		return StructureManager.GetStructureByID<Structures_Grave>(_graveID).Data.Position;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.GoingTo && _haveCorpse)
		{
			DeadWorshipper deadWorshipper = FindCorpse();
			deadWorshipper.WrapBody();
			deadWorshipper.HideBody();
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-corpse");
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		UndoStateAnimationChanges(follower);
		if (!_haveCorpse)
		{
			follower.StartCoroutine(PickUpBodyRoutine(follower));
		}
		else
		{
			follower.StartCoroutine(BuryBodyRoutine(follower));
		}
	}

	private IEnumerator PickUpBodyRoutine(Follower follower)
	{
		DeadWorshipper corpse = FindCorpse();
		if (corpse == null)
		{
			follower.State.CURRENT_STATE = StateMachine.State.CustomAction0;
			yield return new WaitForSeconds(1f);
			_003C_003En__0(follower);
			follower.State.CURRENT_STATE = StateMachine.State.Idle;
			End();
		}
		else
		{
			corpse.WrapBody();
			follower.State.CURRENT_STATE = StateMachine.State.Idle;
			yield return new WaitForSeconds(0.5f);
			corpse.HideBody();
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-corpse");
			ProgressTask();
		}
	}

	private IEnumerator BuryBodyRoutine(Follower follower)
	{
		DeadWorshipper deadWorshipper = FindCorpse();
		deadWorshipper.StructureInfo.Position = follower.transform.position;
		deadWorshipper.transform.position = follower.transform.position;
		deadWorshipper.ShowBody();
		yield return new WaitForSeconds(0.5f);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("dig", true);
		yield return new WaitForSeconds(5f);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAction0;
		yield return new WaitForSeconds(0.5f);
		ProgressTask();
		FindGrave().SetGameObjects();
		follower.State.CURRENT_STATE = StateMachine.State.Idle;
	}

	public override void Cleanup(Follower follower)
	{
		DeadWorshipper deadWorshipper = FindCorpse();
		if (deadWorshipper != null)
		{
			deadWorshipper.StructureInfo.Position = follower.transform.position;
			deadWorshipper.transform.position = follower.transform.position;
			deadWorshipper.ShowBody();
		}
		UndoStateAnimationChanges(follower);
		base.Cleanup(follower);
	}

	private DeadWorshipper FindCorpse()
	{
		DeadWorshipper result = null;
		foreach (DeadWorshipper deadWorshipper in DeadWorshipper.DeadWorshippers)
		{
			if (deadWorshipper.StructureInfo.ID == _corpseID)
			{
				result = deadWorshipper;
				break;
			}
		}
		return result;
	}

	private Grave FindGrave()
	{
		Grave result = null;
		foreach (Grave grafe in Grave.Graves)
		{
			if (grafe.StructureInfo.ID == _graveID)
			{
				result = grafe;
				break;
			}
		}
		return result;
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0(Follower follower)
	{
		base.Cleanup(follower);
	}
}
