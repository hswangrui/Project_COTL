using System.Collections.Generic;
using DG.Tweening;
using Spine;
using UnityEngine;

public class FollowerTask_InstantPoop : FollowerTask
{
	private PlacementRegion.TileGridTile ClosestWasteTile;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Bathroom;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
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

	protected override int GetSubTaskCode()
	{
		return UsingStructureID;
	}

	public override void ClaimReservations()
	{
		if (ClosestWasteTile != null)
		{
			ClosestWasteTile.ReservedForWaste = true;
		}
	}

	public override void ReleaseReservations()
	{
		if (ClosestWasteTile != null)
		{
			ClosestWasteTile.ReservedForWaste = false;
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	public override void ProgressTask()
	{
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.POOP, 0);
		infoByType.FollowerID = _brain.Info.ID;
		_brain.AddThought(Thought.BathroomOutside);
		ClosestWasteTile = StructureManager.GetCloseTile(_brain.LastPosition, FollowerLocation.Base);
		if (ClosestWasteTile != null)
		{
			infoByType.GridTilePosition = ClosestWasteTile.Position;
			StructureManager.BuildStructure(_brain.Location, infoByType, ClosestWasteTile.WorldPosition, Vector2Int.one, false, LerpPoop);
		}
		else
		{
			StructureManager.BuildStructure(_brain.Location, infoByType, _brain.LastPosition, Vector2Int.one, false, LerpPoop);
		}
		_brain.Stats.TargetBathroom = 0f;
	}

	private void LerpPoop(GameObject poop)
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (!(follower == null))
		{
			Vector3 position = poop.transform.position;
			poop.transform.position = follower.transform.position;
			poop.transform.localScale = Vector3.zero;
			poop.transform.DOMove(position, 0.25f).SetEase(Ease.OutSine);
			poop.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
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

	private List<PlacementRegion.TileGridTile> GetAvailableFollowerPositions(PlacementRegion.TileGridTile poopPosition)
	{
		List<PlacementRegion.TileGridTile> list = new List<PlacementRegion.TileGridTile>();
		PlacementRegion.TileGridTile closeTile = StructureManager.GetCloseTile(poopPosition.WorldPosition + Vector3.up * 0.5f, FollowerLocation.Base);
		if (closeTile != null)
		{
			list.Add(closeTile);
		}
		closeTile = StructureManager.GetCloseTile(poopPosition.WorldPosition - Vector3.up * 0.5f, FollowerLocation.Base);
		if (closeTile != null)
		{
			list.Add(closeTile);
		}
		closeTile = StructureManager.GetCloseTile(poopPosition.WorldPosition + Vector3.right * 0.5f, FollowerLocation.Base);
		if (closeTile != null)
		{
			list.Add(closeTile);
		}
		closeTile = StructureManager.GetCloseTile(poopPosition.WorldPosition - Vector3.right * 0.5f, FollowerLocation.Base);
		if (closeTile != null)
		{
			list.Add(closeTile);
		}
		return list;
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (!(follower != null))
		{
			return;
		}
		GetDestination(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
		follower.TimedAnimation("poop", 1.5333333f, delegate
		{
			follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
			follower.FacePosition(_currentDestination.Value);
			if (_brain.HasTrait(FollowerTrait.TraitType.Coprophiliac))
			{
				follower.TimedAnimation("Reactions/react-laugh", 3.3333333f, base.End, false);
			}
			else
			{
				follower.TimedAnimation("Reactions/react-embarrassed", 3f, base.End, false);
			}
		});
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Poop")
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

	protected override void OnEnd()
	{
		base.OnEnd();
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State != FollowerTaskState.Doing)
		{
			SetState(FollowerTaskState.Doing);
		}
	}
}
