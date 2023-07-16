using System;
using System.Collections;
using MMBiomeGeneration;
using UnityEngine;

public class FollowerTask_ChangeLocation : FollowerTask
{
	private FollowerLocation _destLocation;

	private FollowerTaskType _parentType;

	private Follower _follower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ChangeLocation;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.Location;
		}
	}

	public override bool ShouldSaveDestination
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

	public override bool BlockTaskChanges
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
			return false;
		}
	}

	public FollowerTaskType ParentType
	{
		get
		{
			return _parentType;
		}
	}

	public FollowerLocation TargetLocation
	{
		get
		{
			return _destLocation;
		}
	}

	public FollowerTask_ChangeLocation(FollowerLocation destLocation, FollowerTaskType parentType, bool AnimateOutFromLocation)
	{
		_destLocation = destLocation;
		_parentType = parentType;
		base.AnimateOutFromLocation = AnimateOutFromLocation;
	}

	protected override int GetSubTaskCode()
	{
		return (int)_destLocation;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void OnArrive()
	{
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (follower != null && base.AnimateOutFromLocation && _brain.Location == PlayerFarming.Location && TargetLocation != 0 && TargetLocation != FollowerLocation.Demon && TargetLocation != FollowerLocation.Base && base.Brain.Location == FollowerLocation.Base)
		{
			follower.TimedAnimation("Conversations/greet-nice", 1.9333333f, delegate
			{
				if (follower != null)
				{
					follower.StartCoroutine(FrameDelay(delegate
					{
						if (follower != null)
						{
							follower.TimedAnimation("spawn-out", 5f / 6f, delegate
							{
								ChangedLocation();
							});
						}
						else
						{
							ChangedLocation();
						}
					}));
				}
				else
				{
					ChangedLocation();
				}
			});
		}
		else
		{
			ChangedLocation();
		}
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (follower != null)
		{
			follower.SetOutfit(FollowerOutfitType.Follower, false);
			follower.Interaction_FollowerInteraction.Interactable = true;
		}
	}

	private void ChangedLocation()
	{
		_brain.DesiredLocation = _destLocation;
		SetState(FollowerTaskState.Done);
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		return LocationManager.LocationManagers[Location].GetExitPosition(_destLocation);
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		_follower = follower;
		BiomeGenerator.OnBiomeChangeRoom += OnChangeRoom;
	}

	public override void Cleanup(Follower follower)
	{
		base.Cleanup(follower);
		_follower = null;
		BiomeGenerator.OnBiomeChangeRoom -= OnChangeRoom;
	}

	private void OnChangeRoom()
	{
		_follower.ClearPath();
		Arrive();
	}

	public override string ToDebugString()
	{
		return string.Format("{0} ({1}, {2})", base.ToDebugString(), _destLocation, _parentType);
	}
}
