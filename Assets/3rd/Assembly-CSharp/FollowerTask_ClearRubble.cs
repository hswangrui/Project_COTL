using System;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class FollowerTask_ClearRubble : FollowerTask_AssistPlayerBase
{
	private FollowerLocation _location;

	private new bool _helpingPlayer;

	private int _rubbleID;

	private Structures_Rubble rubble;

	private float _gameTimeSinceLastProgress;

	private float WaitTimer;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ClearRubble;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return rubble.Data.Location;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return _helpingPlayer;
		}
	}

	public override int UsingStructureID
	{
		get
		{
			return _rubbleID;
		}
	}

	public override float Priorty
	{
		get
		{
			if (!rubble.Data.Prioritised)
			{
				return 1f;
			}
			return 5f;
		}
	}

	public int RubbleID
	{
		get
		{
			return _rubbleID;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.StoneMiner)
		{
			if (rubble.RockSize != 0)
			{
				return PriorityCategory.Medium;
			}
			return PriorityCategory.WorkPriority;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_ClearRubble(int rubbleID)
	{
		_helpingPlayer = false;
		_rubbleID = rubbleID;
		rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
		_location = rubble.Data.Location;
	}

	private Structures_Rubble GetNextRubble()
	{
		ReleaseReservations();
		Structures_Rubble structures_Rubble = null;
		float num = float.MaxValue;
		float num2 = (_helpingPlayer ? AssistRange : float.MaxValue);
		PlayerFarming instance = PlayerFarming.Instance;
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		List<Structures_Rubble> allAvailableRubble = StructureManager.GetAllAvailableRubble(Location);
		foreach (Structures_Rubble item in allAvailableRubble)
		{
			if (follower == null)
			{
				structures_Rubble = item;
				break;
			}
			float num3 = Vector3.Distance(_helpingPlayer ? instance.transform.position : follower.transform.position, item.Data.Position);
			if (item.RockSize == 0 && num3 < num2)
			{
				float num4 = num3 + (item.Data.Prioritised ? 0f : 1000f);
				if (num4 < num)
				{
					structures_Rubble = item;
					num = num4;
				}
			}
		}
		if (structures_Rubble == null)
		{
			foreach (Structures_Rubble item2 in allAvailableRubble)
			{
				if (follower == null)
				{
					structures_Rubble = item2;
					break;
				}
				float num5 = Vector3.Distance(_helpingPlayer ? instance.transform.position : follower.transform.position, item2.Data.Position);
				if (item2.RockSize == 1 && num5 < num2)
				{
					float num6 = num5 + (item2.Data.Prioritised ? 0f : 1000f);
					if (num6 < num)
					{
						structures_Rubble = item2;
						num = num6;
					}
				}
			}
		}
		return structures_Rubble;
	}

	public FollowerTask_ClearRubble(Rubble rubble)
	{
		_helpingPlayer = true;
		_rubbleID = rubble.StructureInfo.ID;
		Interaction_PlayerClearRubble.PlayerActivatingEnd = (Action<Rubble>)Delegate.Combine(Interaction_PlayerClearRubble.PlayerActivatingEnd, new Action<Rubble>(OnPlayerActivatingEnd));
	}

	public override void ClaimReservations()
	{
		if (!_helpingPlayer)
		{
			Structures_Rubble structureByID = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
			if (structureByID != null && structureByID.AvailableSlotCount > 0)
			{
				structureByID.AvailableSlotCount--;
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
			rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
			if (rubble != null)
			{
				rubble.AvailableSlotCount++;
			}
		}
	}

	protected override void OnStart()
	{
		rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
		if (rubble != null)
		{
			Structures_Rubble structures_Rubble = rubble;
			structures_Rubble.OnRemovalComplete = (Action)Delegate.Combine(structures_Rubble.OnRemovalComplete, new Action(OnRemovalComplete));
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
		rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
		if (rubble != null)
		{
			Structures_Rubble structures_Rubble = rubble;
			structures_Rubble.OnRemovalComplete = (Action)Delegate.Remove(structures_Rubble.OnRemovalComplete, new Action(OnRemovalComplete));
		}
		_brain.GetXP(1f);
		Interaction_PlayerClearRubble.PlayerActivatingEnd = (Action<Rubble>)Delegate.Remove(Interaction_PlayerClearRubble.PlayerActivatingEnd, new Action<Rubble>(OnPlayerActivatingEnd));
	}

	protected override void AssistPlayerTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Wait)
		{
			if ((WaitTimer += deltaGameTime) > 60f || PlayerFarming.Location == _brain.Location)
			{
				End();
			}
		}
		else if (base.State == FollowerTaskState.Doing)
		{
			_gameTimeSinceLastProgress += deltaGameTime;
			if (_brain.Location != PlayerFarming.Location && _gameTimeSinceLastProgress > ConvertAnimTimeToGameTime(1.9f) / 2f)
			{
				ProgressTask();
			}
		}
	}

	private float ConvertAnimTimeToGameTime(float duration)
	{
		return duration * 2f;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Chop")
		{
			ProgressTask();
		}
	}

	public override void ProgressTask()
	{
		rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
		if (rubble == null || rubble.ProgressFinished)
		{
			Debug.Log("rubble is null so END");
			End();
			return;
		}
		rubble.RemovalProgress += _gameTimeSinceLastProgress * 0.25f;
		if (_brain != null && _brain.Info != null && rubble != null)
		{
			rubble.UpdateProgress(_brain.Info.ID);
		}
		_gameTimeSinceLastProgress = 0f;
	}

	private void OnRemovalComplete()
	{
		if (_brain.Location != PlayerFarming.Location)
		{
			List<Structures_CollectedResourceChest> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_CollectedResourceChest>(_brain.Location);
			if (allStructuresOfType.Count > 0 && rubble != null && rubble.Data != null)
			{
				allStructuresOfType[0].AddItem(rubble.Data.LootToDrop, Mathf.RoundToInt((float)rubble.RubbleDropAmount * _brain.ResourceHarvestingMultiplier));
			}
			WaitTimer = 0f;
			SetState(FollowerTaskState.Wait);
		}
		else
		{
			if (rubble != null && rubble.Data != null)
			{
				rubble.Data.FollowerID = _brain.Info.ID;
			}
			End();
		}
	}

	private void OnPlayerActivatingEnd(Rubble rubble)
	{
		if (rubble.StructureInfo.ID == _rubbleID)
		{
			End();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
		return rubble.Data.Position + new Vector3(0f, rubble.Data.Bounds.y / 2) + (Vector3)(UnityEngine.Random.insideUnitCircle * rubble.Data.Bounds.x);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
		rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
		if (rubble == null || rubble.ProgressFinished)
		{
			Structures_Rubble nextRubble = GetNextRubble();
			if (nextRubble == null)
			{
				End();
				return;
			}
			ReleaseReservations();
			ClearDestination();
			_rubbleID = nextRubble.Data.ID;
			_location = nextRubble.Data.Location;
			nextRubble.ReservedForTask = true;
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		rubble = StructureManager.GetStructureByID<Structures_Rubble>(_rubbleID);
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, rubble.Data.Position);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("mining", true);
	}

	private Rubble FindRubble()
	{
		Rubble result = null;
		foreach (Rubble rubble in Rubble.Rubbles)
		{
			if (rubble.StructureInfo.ID == _rubbleID)
			{
				result = rubble;
				break;
			}
		}
		return result;
	}
}
