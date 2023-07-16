using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine;
using UnityEngine;

public class FollowerTask_Bathroom : FollowerTask
{
	private int _toiletID;

	private Structures_Outhouse _toilet;

	private Coroutine _doorCoroutine;

	private int tryUseOuthouseCounter;

	private const int maxOuthouseTries = 3;

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
			if (_hasToilet)
			{
				return _toilet.Data.Location;
			}
			return _brain.HomeLocation;
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

	public override int UsingStructureID
	{
		get
		{
			int result = 0;
			if (_hasToilet)
			{
				result = _toiletID;
			}
			return result;
		}
	}

	private bool _hasToilet
	{
		get
		{
			return _toiletID != 0;
		}
	}

	public FollowerTask_Bathroom()
	{
	}

	public FollowerTask_Bathroom(int toiletID)
	{
		_toiletID = toiletID;
		_toilet = StructureManager.GetStructureByID<Structures_Outhouse>(_toiletID);
		tryUseOuthouseCounter = 0;
	}

	protected override int GetSubTaskCode()
	{
		return UsingStructureID;
	}

	public override void ClaimReservations()
	{
		if (_hasToilet && _toilet != null && !_toilet.ReservedForTask)
		{
			_toilet.ReservedForTask = true;
		}
		if (ClosestWasteTile != null)
		{
			ClosestWasteTile.ReservedForWaste = true;
		}
	}

	public override void ReleaseReservations()
	{
		if (_hasToilet)
		{
			_toilet.ReservedForTask = false;
		}
		if (ClosestWasteTile != null)
		{
			ClosestWasteTile.ReservedForWaste = false;
		}
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
		if (!_hasToilet)
		{
			StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.POOP, 0);
			infoByType.FollowerID = _brain.Info.ID;
			_brain.AddThought(Thought.BathroomOutside);
			if (ClosestWasteTile != null)
			{
				infoByType.GridTilePosition = ClosestWasteTile.Position;
				StructureManager.BuildStructure(_brain.Location, infoByType, ClosestWasteTile.WorldPosition, Vector2Int.one, false, LerpPoop);
			}
			else
			{
				StructureManager.BuildStructure(_brain.Location, infoByType, _currentDestination.Value, Vector2Int.one, false, LerpPoop);
			}
		}
		else
		{
			_toilet.Data.TotalPoops++;
			_toilet.DepositItem(InventoryItem.ITEM_TYPE.POOP);
			switch (_toilet.Data.Type)
			{
			case StructureBrain.TYPES.OUTHOUSE:
				_brain.AddThought(Thought.BathroomOuthouse);
				break;
			case StructureBrain.TYPES.OUTHOUSE_2:
				_brain.AddThought(Thought.BathroomOuthouse2);
				break;
			}
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
		Interaction_Outhouse interaction_Outhouse = FindOuthouse();
		if (_hasToilet && follower != null && (bool)interaction_Outhouse && !interaction_Outhouse.IsFull)
		{
			if (interaction_Outhouse.StructureInfo.FollowerID == _brain.Info.ID)
			{
				return interaction_Outhouse.InsideFollowerPosition.position;
			}
			return interaction_Outhouse.WaitingFollowerPosition.position + (Vector3)Random.insideUnitCircle * 0.5f;
		}
		ClosestWasteTile = StructureManager.GetBestWasteTile(_brain.Location);
		if (ClosestWasteTile != null)
		{
			ClosestWasteTile.ReservedForWaste = true;
			List<PlacementRegion.TileGridTile> availableFollowerPositions = GetAvailableFollowerPositions(ClosestWasteTile);
			if (availableFollowerPositions.Count > 0)
			{
				return availableFollowerPositions[Random.Range(0, availableFollowerPositions.Count)].WorldPosition;
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

	public override void OnGoingToBegin(Follower follower)
	{
		base.OnGoingToBegin(follower);
		if (_hasToilet)
		{
			Interaction_Outhouse interaction_Outhouse = FindOuthouse();
			if (interaction_Outhouse != null && interaction_Outhouse.StructureInfo.FollowerID == _brain.Info.ID)
			{
				_doorCoroutine = follower.StartCoroutine(DoorCoroutine(follower, interaction_Outhouse));
			}
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (!_hasToilet && follower != null)
		{
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
			return;
		}
		Interaction_Outhouse interaction_Outhouse = FindOuthouse();
		if (interaction_Outhouse != null && interaction_Outhouse.StructureInfo != null && interaction_Outhouse.StructureInfo.FollowerID == _brain.Info.ID)
		{
			follower.TimedAnimation("poop-outhouse", 3f, delegate
			{
				ProgressTask();
				End();
			});
		}
		else if (interaction_Outhouse != null)
		{
			if (_toilet != null && _toilet.Data != null && _toilet.Data.FollowerID == -1)
			{
				_toilet.Data.FollowerID = _brain.Info.ID;
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
			}
			else if (tryUseOuthouseCounter >= 3 || interaction_Outhouse.IsFull)
			{
				_toiletID = 0;
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
			}
			else
			{
				follower.TimedAnimation("waiting", 3.2f, delegate
				{
					tryUseOuthouseCounter++;
					SetState(FollowerTaskState.GoingTo);
				});
			}
		}
		else
		{
			End();
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		Interaction_Outhouse interaction_Outhouse = FindOuthouse();
		if (interaction_Outhouse != null)
		{
			interaction_Outhouse.StructureInfo.FollowerID = -1;
		}
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run");
		if (_doorCoroutine != null)
		{
			follower.StopCoroutine(_doorCoroutine);
			_doorCoroutine = null;
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

	private Interaction_Outhouse FindOuthouse()
	{
		Interaction_Outhouse result = null;
		foreach (Interaction_Outhouse outhouse in Interaction_Outhouse.Outhouses)
		{
			if (outhouse.StructureInfo.ID == _toiletID)
			{
				result = outhouse;
				break;
			}
		}
		return result;
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		Interaction_Outhouse interaction_Outhouse = FindOuthouse();
		if ((bool)interaction_Outhouse)
		{
			interaction_Outhouse.DoorClosed.SetActive(true);
			interaction_Outhouse.DoorOpen.SetActive(false);
		}
		if (_toilet != null && _toilet.Data != null && _toilet.Data.FollowerID == _brain.Info.ID)
		{
			_toilet.Data.FollowerID = -1;
		}
	}

	private IEnumerator DoorCoroutine(Follower follower, Interaction_Outhouse outhouse)
	{
		while (base.State == FollowerTaskState.GoingTo && Vector3.Distance(follower.transform.position, outhouse.InsideFollowerPosition.position) > 2f)
		{
			yield return null;
		}
		outhouse.DoorClosed.SetActive(false);
		outhouse.DoorOpen.SetActive(true);
		while (base.State == FollowerTaskState.GoingTo)
		{
			yield return null;
		}
		outhouse.DoorClosed.SetActive(true);
		outhouse.DoorOpen.SetActive(false);
		while (base.State == FollowerTaskState.Doing)
		{
			yield return null;
		}
		outhouse.DoorClosed.SetActive(false);
		outhouse.DoorOpen.SetActive(true);
		yield return new WaitForSeconds(0.5f);
		outhouse.DoorClosed.SetActive(true);
		outhouse.DoorOpen.SetActive(false);
		_doorCoroutine = null;
	}

	public override void SimDoingBegin(SimFollower simFollower)
	{
		base.SimDoingBegin(simFollower);
		GetDestination(null);
		End();
	}
}
