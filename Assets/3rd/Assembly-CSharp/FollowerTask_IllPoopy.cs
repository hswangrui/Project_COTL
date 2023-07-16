using System;
using System.Collections.Generic;
using DG.Tweening;
using Spine;
using UnityEngine;

public class FollowerTask_IllPoopy : FollowerTask
{
	private float _gameTimeToNextStateUpdate;

	private Follower follower;

	private bool GoToVomit;

	public bool ForceFirstVomit = true;

	private PlacementRegion.TileGridTile ClosestWasteTile;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.IllPoopy;
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

	public override bool BlockSermon
	{
		get
		{
			return true;
		}
	}

	public FollowerTask_IllPoopy(bool forceFirstVomit = false)
	{
		ForceFirstVomit = forceFirstVomit;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
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

	protected override void OnArrive()
	{
		if (GoToVomit)
		{
			SetState(FollowerTaskState.Doing);
		}
		else
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_brain.Stats.Illness <= 0f)
		{
			Debug.Log("CCC");
			End();
			return;
		}
		if (ForceFirstVomit && IllnessBar.IllnessNormalized > 0.05f)
		{
			ForceFirstVomit = false;
			GoToVomit = true;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
			if ((bool)follower)
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/run-poopy");
				((FollowerState_Ill)follower.Brain.CurrentState).SpeedMultiplier = 5f;
			}
		}
		if (_state != FollowerTaskState.Idle)
		{
			return;
		}
		_gameTimeToNextStateUpdate -= deltaGameTime;
		if (_gameTimeToNextStateUpdate <= 0f)
		{
			GoToVomit = false;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
			_gameTimeToNextStateUpdate = UnityEngine.Random.Range(4f, 6f);
			if ((bool)follower)
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/walk-sick");
				((FollowerState_Ill)follower.Brain.CurrentState).SpeedMultiplier = 1f;
			}
		}
		else
		{
			if (!(TimeManager.TotalElapsedGameTime - _brain.Stats.LastVomit > 360f) && !(UnityEngine.Random.Range(0f, 1f) <= 0.002f))
			{
				return;
			}
			if (IllnessBar.IllnessNormalized > 0.05f)
			{
				GoToVomit = true;
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
				if ((bool)follower)
				{
					follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/run-poopy");
					((FollowerState_Ill)follower.Brain.CurrentState).SpeedMultiplier = 5f;
				}
				return;
			}
			GoToVomit = false;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
			_gameTimeToNextStateUpdate = UnityEngine.Random.Range(4f, 6f);
			if ((bool)follower)
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/walk-sick");
				((FollowerState_Ill)follower.Brain.CurrentState).SpeedMultiplier = 1f;
			}
		}
	}

	public override void ProgressTask()
	{
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.POOP, 0);
		infoByType.FollowerID = _brain.Info.ID;
		_brain.Stats.LastVomit = TimeManager.TotalElapsedGameTime;
		if (ClosestWasteTile != null)
		{
			infoByType.GridTilePosition = ClosestWasteTile.Position;
			StructureManager.BuildStructure(_brain.Location, infoByType, ClosestWasteTile.WorldPosition, Vector2Int.one, false, LerpPoop);
		}
		else
		{
			StructureManager.BuildStructure(_brain.Location, infoByType, _brain.LastPosition, Vector2Int.one, false, LerpPoop);
		}
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
		if (GoToVomit)
		{
			return GetPoopPosition();
		}
		return TownCentre.RandomCircleFromTownCentre(10f);
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 100f;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Sick/idle-sick");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Sick/walk-sick");
		this.follower = follower;
	}

	public override void OnDoingBegin(Follower follower)
	{
		GoToVomit = false;
		follower.TimedAnimation("poop", 1.5333333f, delegate
		{
			follower.FacePosition(_brain.LastPosition);
			if (follower.Brain.HasTrait(FollowerTrait.TraitType.Coprophiliac))
			{
				follower.TimedAnimation("Reactions/react-laugh", 3.3333333f, ContinueAfterReaction, false);
			}
			else
			{
				follower.TimedAnimation("Reactions/react-embarrassed", 3f, ContinueAfterReaction, false);
			}
		});
	}

	private void ContinueAfterReaction()
	{
		follower.FacePosition(_brain.LastPosition);
		SetState(FollowerTaskState.Idle);
		follower.Brain.Stats.Bathroom = 0f;
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
		if (follower.Brain.CurrentState.Type == FollowerStateType.Ill)
		{
			((FollowerState_Ill)follower.Brain.CurrentState).SpeedMultiplier = 1f;
		}
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Poop")
		{
			ProgressTask();
		}
	}

	protected override float VomitChange(float deltaGameTime)
	{
		return 0f;
	}

	public override void SimSetup(SimFollower simFollower)
	{
		base.SimSetup(simFollower);
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	public override void SimCleanup(SimFollower simFollower)
	{
		base.SimCleanup(simFollower);
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(OnStructureAdded));
	}

	private void OnStructureAdded(StructuresData structure)
	{
		if (structure.Type == StructureBrain.TYPES.POOP && structure.FollowerID == _brain.Info.ID)
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		base.SimDoingBegin(simFollower);
		GetDestination(null);
		ProgressTask();
	}

	private Vector3 GetPoopPosition()
	{
		Structures_Outhouse structures_Outhouse = null;
		foreach (Structures_Outhouse item in StructureManager.GetAllStructuresOfType<Structures_Outhouse>())
		{
			if (structures_Outhouse == null || Vector3.Distance(_brain.LastPosition, item.Data.Position) < Vector3.Distance(_brain.LastPosition, structures_Outhouse.Data.Position))
			{
				structures_Outhouse = item;
			}
		}
		if (structures_Outhouse != null)
		{
			Vector3 normalized = (_brain.LastPosition - structures_Outhouse.Data.Position).normalized;
			Vector3 position = structures_Outhouse.Data.Position + normalized;
			ClosestWasteTile = StructureManager.GetCloseTile(position, FollowerLocation.Base);
			if (ClosestWasteTile != null)
			{
				ClosestWasteTile.ReservedForWaste = true;
				List<PlacementRegion.TileGridTile> availableFollowerPositions = GetAvailableFollowerPositions(ClosestWasteTile);
				if (availableFollowerPositions.Count > 0)
				{
					return availableFollowerPositions[UnityEngine.Random.Range(0, availableFollowerPositions.Count)].WorldPosition;
				}
				return ClosestWasteTile.WorldPosition;
			}
		}
		ClosestWasteTile = StructureManager.GetBestWasteTile(_brain.Location);
		if (ClosestWasteTile != null)
		{
			ClosestWasteTile.ReservedForWaste = true;
			List<PlacementRegion.TileGridTile> availableFollowerPositions2 = GetAvailableFollowerPositions(ClosestWasteTile);
			if (availableFollowerPositions2.Count > 0)
			{
				return availableFollowerPositions2[UnityEngine.Random.Range(0, availableFollowerPositions2.Count)].WorldPosition;
			}
			return ClosestWasteTile.WorldPosition;
		}
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
}
