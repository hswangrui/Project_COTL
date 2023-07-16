using System;
using Spine;
using UnityEngine;

public class FollowerTask_Build : FollowerTask
{
	private bool _helpingPlayer;

	private int _buildSiteID;

	private Structures_BuildSite _buildSite;

	private float _gameTimeSinceLastProgress;

	private Follower follower;

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
			return false;
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
			return _priority;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if ((brain == null || brain.CurrentOverrideTaskType == FollowerTaskType.None || Interaction_Temple.Instance == null) && (FollowerRole == FollowerRole.Lumberjack || (uint)(FollowerRole - 5) <= 1u))
		{
			return PriorityCategory.WorkPriority;
		}
		return PriorityCategory.Medium;
	}

	public FollowerTask_Build(int buildSiteID)
	{
		_helpingPlayer = false;
		_buildSiteID = buildSiteID;
		_buildSite = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
		_priority = ((StructuresData.GetCategory(_buildSite.Data.ToBuildType) == StructureBrain.Categories.AESTHETIC) ? 80 : 100);
	}

	public FollowerTask_Build(BuildSitePlot buildSite)
	{
		_helpingPlayer = true;
		_buildSiteID = buildSite.StructureInfo.ID;
		_buildSite = buildSite.StructureBrain;
		Interaction_PlayerBuild.PlayerActivatingEnd = (Action<BuildSitePlot>)Delegate.Combine(Interaction_PlayerBuild.PlayerActivatingEnd, new Action<BuildSitePlot>(OnPlayerActivatingEnd));
		_priority = ((StructuresData.GetCategory(_buildSite.Data.ToBuildType) == StructureBrain.Categories.AESTHETIC) ? 80 : 100);
	}

	protected override int GetSubTaskCode()
	{
		return _buildSiteID;
	}

	public override void ClaimReservations()
	{
		if (!_helpingPlayer)
		{
			Structures_BuildSite structureByID = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
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
			Structures_BuildSite structureByID = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
			if (structureByID != null)
			{
				structureByID.UsedSlotCount--;
			}
		}
	}

	protected override void OnStart()
	{
		Structures_BuildSite structureByID = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
		if (structureByID != null)
		{
			structureByID.OnBuildComplete = (Action)Delegate.Combine(structureByID.OnBuildComplete, new Action(OnBuildComplete));
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			End();
		}
	}

	protected override void OnComplete()
	{
		Structures_BuildSite structureByID = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
		if (structureByID != null)
		{
			structureByID.OnBuildComplete = (Action)Delegate.Remove(structureByID.OnBuildComplete, new Action(OnBuildComplete));
		}
		Interaction_PlayerBuild.PlayerActivatingEnd = (Action<BuildSitePlot>)Delegate.Remove(Interaction_PlayerBuild.PlayerActivatingEnd, new Action<BuildSitePlot>(OnPlayerActivatingEnd));
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Doing)
		{
			_gameTimeSinceLastProgress += deltaGameTime;
			if (follower == null)
			{
				ProgressTask();
			}
		}
	}

	public override void ProgressTask()
	{
		Structures_BuildSite structureByID = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
		if (structureByID != null)
		{
			structureByID.BuildProgress += _gameTimeSinceLastProgress * _brain.Info.ProductivityMultiplier;
		}
		_gameTimeSinceLastProgress = 0f;
	}

	private void OnBuildComplete()
	{
		_brain.GetXP(1f);
		End();
	}

	private void OnPlayerActivatingEnd(BuildSitePlot buildSite)
	{
		if (buildSite.StructureInfo.ID == _buildSiteID)
		{
			End();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Structures_BuildSite structureByID = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
		return structureByID.Data.Position + new Vector3(0f, (float)structureByID.Data.Bounds.y / 2f) + (Vector3)(UnityEngine.Random.insideUnitCircle * ((float)structureByID.Data.Bounds.x * 0.5f));
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
		this.follower = follower;
	}

	public override void OnDoingBegin(Follower follower)
	{
		Structures_BuildSite structureByID = StructureManager.GetStructureByID<Structures_BuildSite>(_buildSiteID);
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, structureByID.Data.Position);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation((follower.Brain.CurrentState != null && follower.Brain.HasThought(Thought.Intimidated)) ? "build-fast-scared" : "build", true);
		this.follower = follower;
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

	private BuildSitePlot FindPlot()
	{
		BuildSitePlot result = null;
		foreach (BuildSitePlot buildSitePlot in BuildSitePlot.BuildSitePlots)
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
