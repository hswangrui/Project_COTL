using System.Collections.Generic;
using Spine;
using UnityEngine;

public class FollowerTask_ChopTrees : FollowerTask_AssistPlayerBase
{
	public const float REMOVAL_DURATION_GAME_MINUTES = 4f;

	private Structures_Tree _tree;

	public int _treeID;

	private FollowerLocation _location;

	private float _removalProgress;

	private float _gameTimeSinceLastProgress;

	private float WaitTimer;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ChopTrees;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _location;
		}
	}

	public override float Priorty
	{
		get
		{
			return (_tree == null) ? 3 : ((!BigTreeVariant) ? 3 : 0);
		}
	}

	private bool BigTreeVariant
	{
		get
		{
			if (_tree == null)
			{
				return false;
			}
			return _tree.Data.VariantIndex == 2;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		if (FollowerRole == FollowerRole.Lumberjack)
		{
			if (_tree == null || _tree.Data == null || !BigTreeVariant)
			{
				return PriorityCategory.WorkPriority;
			}
			return PriorityCategory.Medium;
		}
		return PriorityCategory.Low;
	}

	public FollowerTask_ChopTrees(int TreeID)
	{
		_helpingPlayer = false;
		_treeID = TreeID;
		_tree = StructureManager.GetStructureByID<Structures_Tree>(TreeID);
		_location = _tree.Data.Location;
	}

	public FollowerTask_ChopTrees()
	{
		_helpingPlayer = true;
	}

	public override void ClaimReservations()
	{
		base.ClaimReservations();
		_tree = StructureManager.GetStructureByID<Structures_Tree>(_treeID);
		if (_tree != null)
		{
			_tree.ReservedForTask = true;
		}
	}

	public override void ReleaseReservations()
	{
		base.ReleaseReservations();
		_tree = StructureManager.GetStructureByID<Structures_Tree>(_treeID);
		if (_tree != null)
		{
			_tree.ReservedForTask = false;
		}
	}

	protected override void OnStart()
	{
		ReleaseReservations();
		Loop(true);
		ClearDestination();
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void AssistPlayerTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Wait)
		{
			if (PlayerFarming.Location == _brain.Location)
			{
				Loop();
			}
			else if ((WaitTimer += deltaGameTime) > 60f)
			{
				Loop();
			}
			return;
		}
		if (LocationManager.GetLocationState(_location) == LocationState.Active)
		{
			TreeBase treeBase = FindTree();
			if (treeBase == null || treeBase.StructureBrain.TreeChopped)
			{
				_tree = null;
				_treeID = -1;
				SetState(FollowerTaskState.Idle);
				Loop();
			}
		}
		else if (_tree == null)
		{
			SetState(FollowerTaskState.Idle);
			Loop();
		}
		if (base.State == FollowerTaskState.Doing)
		{
			_gameTimeSinceLastProgress += deltaGameTime;
			if (_brain.Location != PlayerFarming.Location && _gameTimeSinceLastProgress > ConvertAnimTimeToGameTime(4.06f) / 5f)
			{
				ProgressTask();
			}
		}
	}

	private float ConvertAnimTimeToGameTime(float duration)
	{
		return duration * 2f;
	}

	public override void ProgressTask()
	{
		_tree = StructureManager.GetStructureByID<Structures_Tree>(_treeID);
		if (_tree == null || _tree.TreeChopped)
		{
			Debug.Log("Tree is null so END");
			End();
			return;
		}
		float num = 0.075f;
		_gameTimeSinceLastProgress = 0f;
		_tree.TreeHit(num * _brain.Info.ProductivityMultiplier, true, _brain.Info.ID);
		_brain.GetXP(0.1f);
		if (!_tree.TreeChopped)
		{
			return;
		}
		if (_brain.Location != PlayerFarming.Location)
		{
			List<Structures_CollectedResourceChest> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_CollectedResourceChest>(_brain.Location);
			if (allStructuresOfType.Count > 0)
			{
				allStructuresOfType[0].AddItem(InventoryItem.ITEM_TYPE.LOG, Mathf.RoundToInt((float)_tree.Data.LootCountToDrop * _brain.ResourceHarvestingMultiplier));
			}
		}
		if (_brain.Location != PlayerFarming.Location)
		{
			WaitTimer = 0f;
			SetState(FollowerTaskState.Wait);
		}
		else
		{
			Loop();
		}
	}

	private void Loop(bool force = false)
	{
		if (force || !_helpingPlayer || !EndIfPlayerIsDistant())
		{
			Structures_Tree nextTree = GetNextTree();
			if (nextTree == null)
			{
				End();
				return;
			}
			ReleaseReservations();
			ClearDestination();
			_treeID = nextTree.Data.ID;
			_tree = nextTree;
			_tree.ReservedForTask = true;
			SetState(FollowerTaskState.GoingTo);
		}
	}

	private Structures_Tree GetNextTree()
	{
		Structures_Tree structures_Tree = null;
		float num = float.MaxValue;
		float num2 = (_helpingPlayer ? AssistRange : float.MaxValue);
		PlayerFarming instance = PlayerFarming.Instance;
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		List<Structures_Tree> allAvailableTrees = StructureManager.GetAllAvailableTrees(Location);
		foreach (Structures_Tree item in allAvailableTrees)
		{
			if (follower == null)
			{
				structures_Tree = item;
				break;
			}
			float num3 = Vector3.Distance(_helpingPlayer ? instance.transform.position : follower.transform.position, item.Data.Position);
			if (!BigTreeVariant && num3 < num2 && !item.Data.IsSapling)
			{
				float num4 = num3 + (item.Data.Prioritised ? 0f : 1000f);
				if (num4 < num)
				{
					structures_Tree = item;
					num = num4;
				}
			}
		}
		if (structures_Tree == null)
		{
			foreach (Structures_Tree item2 in allAvailableTrees)
			{
				if (follower == null)
				{
					structures_Tree = item2;
					break;
				}
				float num5 = Vector3.Distance(_helpingPlayer ? instance.transform.position : follower.transform.position, item2.Data.Position);
				if (BigTreeVariant && num5 < num2 && !item2.Data.IsSapling)
				{
					float num6 = num5 + (item2.Data.Prioritised ? 0f : 1000f);
					if (num6 < num)
					{
						structures_Tree = item2;
						num = num6;
					}
				}
			}
		}
		return structures_Tree;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (_tree == null)
		{
			return follower.transform.position;
		}
		return _tree.Data.Position + (((double)Random.value < 0.5) ? new Vector3(1f, 0f, -0.5f) : new Vector3(-1f, 0f, -0.5f));
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (_treeID == 0)
		{
			ProgressTask();
			return;
		}
		if (FindTree() != null)
		{
			follower.FacePosition(_tree.Data.Position);
		}
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("chop-wood", true);
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		follower.Spine.AnimationState.Event -= HandleAnimationStateEvent;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string name = e.Data.Name;
		if (name == "Chop")
		{
			ProgressTask();
		}
	}

	private TreeBase FindTree()
	{
		TreeBase result = null;
		foreach (TreeBase tree in TreeBase.Trees)
		{
			if (tree.StructureInfo.ID == _treeID)
			{
				result = tree;
				break;
			}
		}
		return result;
	}
}
