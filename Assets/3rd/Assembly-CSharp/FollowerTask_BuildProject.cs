using System;
using Spine;
using UnityEngine;

public class FollowerTask_BuildProject : FollowerTask
{
	private bool _helpingPlayer;

	private int _buildSiteID;

	private Structures_BuildSiteProject _buildSite;

	private float _gameTimeSinceLastProgress;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Build;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _buildSite.Data.Location;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return _helpingPlayer;
		}
	}

	public override bool BlockReactTasks
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
			return _buildSiteID;
		}
	}

	public override float Priorty
	{
		get
		{
			return 100f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		return PriorityCategory.OverrideWorkPriority;
	}

	public FollowerTask_BuildProject(int buildSiteID)
	{
		_buildSiteID = buildSiteID;
		_buildSite = StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID);
	}

	public FollowerTask_BuildProject(BuildSitePlotProject buildSite)
	{
		_helpingPlayer = true;
		_buildSiteID = buildSite.StructureInfo.ID;
		_buildSite = buildSite.StructureBrain;
		Interaction_PlayerBuildProject.PlayerActivatingEnd = (Action<BuildSitePlotProject>)Delegate.Combine(Interaction_PlayerBuildProject.PlayerActivatingEnd, new Action<BuildSitePlotProject>(OnPlayerActivatingEnd));
	}

	protected override int GetSubTaskCode()
	{
		return _buildSiteID;
	}

	public override void ClaimReservations()
	{
		if (!_helpingPlayer)
		{
			Structures_BuildSiteProject structureByID = StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID);
			if (structureByID != null && structureByID.AvailableSlotCount > 0)
			{
				structureByID.UsedSlotCount++;
			}
			else
			{
				End();
			}
		}
	}

	public override void ReleaseReservations()
	{
		if (!_helpingPlayer)
		{
			Structures_BuildSiteProject structureByID = StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID);
			if (structureByID != null)
			{
				structureByID.UsedSlotCount--;
			}
		}
	}

	protected override void OnStart()
	{
		Structures_BuildSiteProject structureByID = StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID);
		if (structureByID != null)
		{
			structureByID.OnBuildComplete = (Action)Delegate.Combine(structureByID.OnBuildComplete, new Action(OnBuildComplete));
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			End();
		}
	}

	protected override void OnComplete()
	{
		Structures_BuildSiteProject structureByID = StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID);
		if (structureByID != null)
		{
			structureByID.OnBuildComplete = (Action)Delegate.Remove(structureByID.OnBuildComplete, new Action(OnBuildComplete));
		}
		Interaction_PlayerBuildProject.PlayerActivatingEnd = (Action<BuildSitePlotProject>)Delegate.Remove(Interaction_PlayerBuildProject.PlayerActivatingEnd, new Action<BuildSitePlotProject>(OnPlayerActivatingEnd));
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			_gameTimeSinceLastProgress += deltaGameTime;
		}
	}

	public override void ProgressTask()
	{
		StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID).BuildProgress += _gameTimeSinceLastProgress * _brain.Info.ProductivityMultiplier;
		_gameTimeSinceLastProgress = 0f;
	}

	private void OnBuildComplete()
	{
		_brain.GetXP(1f);
		End();
	}

	private void OnPlayerActivatingEnd(BuildSitePlotProject buildSite)
	{
		if (buildSite.StructureInfo.ID == _buildSiteID)
		{
			End();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Structures_BuildSiteProject structureByID = StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID);
		return structureByID.Data.Position + new Vector3(0f, (float)structureByID.Data.Bounds.y / 2f) + (Vector3)(UnityEngine.Random.insideUnitCircle * ((float)structureByID.Data.Bounds.x * 0.5f));
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
	}

	public override void OnDoingBegin(Follower follower)
	{
		Structures_BuildSiteProject structureByID = StructureManager.GetStructureByID<Structures_BuildSiteProject>(_buildSiteID);
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, structureByID.Data.Position);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation((follower.Brain.CurrentState.Type == FollowerStateType.Motivated) ? "build-fast-scared" : "build", true);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Build")
		{
			ProgressTask();
		}
	}

	private BuildSitePlotProject FindPlot()
	{
		BuildSitePlotProject result = null;
		foreach (BuildSitePlotProject buildSitePlot in BuildSitePlotProject.BuildSitePlots)
		{
			if (buildSitePlot.StructureInfo.ID == _buildSiteID)
			{
				result = buildSitePlot;
				break;
			}
		}
		return result;
	}
}
