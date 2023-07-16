using System.Collections.Generic;
using Spine;
using UnityEngine;

public class FollowerTask_Undertaker : FollowerTask
{
	private int _morgueID;

	private bool carryingBody;

	private bool droppedBody;

	private Structures_Morgue _morgue;

	private Structures_DeadWorshipper _deadBody;

	private Interaction_HarvestMeat harvestMeat;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Undertaker;
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

	public override bool BlockSocial
	{
		get
		{
			return true;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.Undertaker)
		{
			return PriorityCategory.OverrideWorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_Undertaker(int morgueID)
	{
		_morgueID = morgueID;
		_morgue = StructureManager.GetStructureByID<Structures_Morgue>(_morgueID);
	}

	protected override int GetSubTaskCode()
	{
		return _morgueID;
	}

	public override void ClaimReservations()
	{
		if (_morgue != null)
		{
			_morgue.ReservedForTask = true;
		}
		if (_deadBody != null)
		{
			_deadBody.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		if (_morgue != null)
		{
			_morgue.ReservedForTask = false;
		}
		if (_deadBody != null)
		{
			_deadBody.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	private int GetNextStructure()
	{
		List<StructureBrain> list = new List<StructureBrain>();
		list.AddRange(StructureManager.GetAllStructuresOfType(Location, StructureBrain.TYPES.DEAD_WORSHIPPER));
		List<StructureBrain> list2 = new List<StructureBrain>();
		foreach (StructureBrain item in list)
		{
			if (!item.ReservedByPlayer && !item.ReservedForTask && !item.Data.BeenInMorgueAlready && !item.Data.BodyWrapped)
			{
				list2.Add(item);
			}
		}
		if (list2.Count > 0)
		{
			StructureBrain structureBrain = null;
			foreach (StructureBrain item2 in list2)
			{
				if (structureBrain == null || Vector3.Distance(item2.Data.Position, _brain.LastPosition) < Vector3.Distance(structureBrain.Data.Position, _brain.LastPosition))
				{
					structureBrain = item2;
				}
			}
			return structureBrain.Data.ID;
		}
		return -1;
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if (carryingBody && _deadBody != null && !droppedBody)
		{
			DropBody();
		}
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetOutfit(FollowerOutfitType.Follower, false);
		}
		if (harvestMeat != null)
		{
			harvestMeat.enabled = true;
			harvestMeat.Interactable = true;
		}
		harvestMeat = null;
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		if (carryingBody && _deadBody != null && !droppedBody)
		{
			DropBody();
		}
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.SetOutfit(FollowerOutfitType.Follower, false);
		}
		if (harvestMeat != null)
		{
			harvestMeat.enabled = true;
			harvestMeat.Interactable = true;
		}
		harvestMeat = null;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += AnimationState_Event;
	}

	private void AnimationState_Event(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "CorpseCollect")
		{
			PickUpBody();
		}
		else if (e.Data.Name == "CorpseDrop")
		{
			droppedBody = true;
			_morgue.DepositBody(_deadBody.Data.FollowerID);
		}
	}

	private void Loop()
	{
		int nextStructure = GetNextStructure();
		if (_deadBody != null && !carryingBody)
		{
			carryingBody = true;
			PickUpBody();
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
		else if (_deadBody != null && carryingBody)
		{
			ClearDestination();
			_morgue.DepositBody(_deadBody.Data.FollowerID);
			RemoveBodyFromExistence();
			SetState(FollowerTaskState.GoingTo);
		}
		else if (nextStructure != -1)
		{
			carryingBody = false;
			_deadBody = StructureManager.GetStructureByID<Structures_DeadWorshipper>(nextStructure);
			_deadBody.ReservedForTask = true;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
			harvestMeat = FindBody(_deadBody.Data.ID);
			if (harvestMeat != null)
			{
				harvestMeat.enabled = false;
				harvestMeat.Interactable = false;
				if (Interactor.CurrentInteraction == harvestMeat)
				{
					Interactor.CurrentInteraction = null;
				}
			}
		}
		else
		{
			End();
		}
	}

	public Structure GetStructure(int ID)
	{
		foreach (Structure structure in Structure.Structures)
		{
			if (structure != null && structure.Brain != null && structure.Brain.Data != null && structure.Brain.Data.ID == ID)
			{
				return structure;
			}
		}
		return null;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (_deadBody != null && !carryingBody)
		{
			return _deadBody.Data.Position;
		}
		Interaction_Morgue interaction_Morgue = FindMorgue();
		if (interaction_Morgue != null)
		{
			return interaction_Morgue.transform.position + Vector3.back;
		}
		End();
		return Vector3.zero;
	}

	private void PickUpBody()
	{
		DeadWorshipper deadWorshipper = null;
		foreach (DeadWorshipper deadWorshipper2 in DeadWorshipper.DeadWorshippers)
		{
			if (deadWorshipper2.StructureInfo != null && deadWorshipper2.StructureInfo.ID == _deadBody.Data.ID)
			{
				deadWorshipper = deadWorshipper2;
				break;
			}
		}
		if (deadWorshipper != null)
		{
			deadWorshipper.gameObject.SetActive(false);
		}
	}

	private void DropBody()
	{
		int followerID = _deadBody.Data.FollowerID;
		RemoveBodyFromExistence();
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
		infoByType.Position = _brain.LastPosition;
		infoByType.BodyWrapped = false;
		infoByType.FollowerID = followerID;
		StructureManager.BuildStructure(FollowerLocation.Base, infoByType, _brain.LastPosition, Vector2Int.one, false, delegate(GameObject g)
		{
			DeadWorshipper component = g.GetComponent<DeadWorshipper>();
			PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(component.transform.position);
			if (closestTileGridTileAtWorldPosition != null)
			{
				component.Structure.Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
			}
		});
		if (harvestMeat != null)
		{
			harvestMeat.enabled = true;
			harvestMeat.Interactable = true;
		}
		harvestMeat = null;
	}

	private void RemoveBodyFromExistence()
	{
		DeadWorshipper deadWorshipper = null;
		foreach (DeadWorshipper deadWorshipper2 in DeadWorshipper.DeadWorshippers)
		{
			if (deadWorshipper2.StructureInfo.ID == _deadBody.Data.ID)
			{
				deadWorshipper = deadWorshipper2;
				break;
			}
		}
		if (deadWorshipper != null)
		{
			Object.Destroy(deadWorshipper.gameObject);
		}
		if (PlacementRegion.Instance != null)
		{
			PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(_deadBody.Data.Position);
			if (closestTileGridTileAtWorldPosition != null)
			{
				_deadBody.RemoveFromGrid(closestTileGridTileAtWorldPosition.Position);
			}
		}
		StructureManager.RemoveStructure(_deadBody);
		carryingBody = false;
		_deadBody = null;
		harvestMeat = null;
	}

	protected override void OnArrive()
	{
		base.OnArrive();
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if ((bool)follower)
		{
			follower.Outfit.SetOutfit(follower.Spine, FollowerOutfitType.Undertaker, InventoryItem.ITEM_TYPE.NONE, false);
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Undertaker/run");
		}
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		if (_deadBody == null)
		{
			Loop();
		}
		else if (_deadBody != null && _deadBody.ReservedForTask)
		{
			Loop();
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (_deadBody == null)
		{
			Loop();
		}
		else
		{
			if (_deadBody == null)
			{
				return;
			}
			follower.FacePosition(_deadBody.Data.Position);
			if (!_deadBody.ReservedForTask)
			{
				return;
			}
			if (!carryingBody)
			{
				follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Undertaker/run-carrying");
				follower.TimedAnimation("Undertaker/collect-corpse", 4.33f, delegate
				{
					Loop();
				});
				return;
			}
			follower.Interaction_FollowerInteraction.enabled = false;
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Undertaker/run");
			follower.TimedAnimation("Undertaker/drop-corpse", 3.66f, delegate
			{
				RemoveBodyFromExistence();
				Loop();
				follower.Interaction_FollowerInteraction.enabled = true;
				droppedBody = false;
			});
		}
	}

	public override void Cleanup(Follower follower)
	{
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= AnimationState_Event;
		follower.SetOutfit(FollowerOutfitType.Follower, false);
		follower.Interaction_FollowerInteraction.enabled = true;
	}

	private Interaction_Morgue FindMorgue()
	{
		Interaction_Morgue result = null;
		foreach (Interaction_Morgue morgue in Interaction_Morgue.Morgues)
		{
			if (morgue != null && morgue.StructureInfo.ID == _morgueID)
			{
				result = morgue;
				break;
			}
		}
		return result;
	}

	private Interaction_HarvestMeat FindBody(int id)
	{
		Interaction_HarvestMeat result = null;
		foreach (Interaction_HarvestMeat interaction_HarvestMeat in Interaction_HarvestMeat.Interaction_HarvestMeats)
		{
			if (interaction_HarvestMeat != null && interaction_HarvestMeat.structure.Structure_Info.ID == id)
			{
				result = interaction_HarvestMeat;
				break;
			}
		}
		return result;
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}
}
