using System;
using UnityEngine;

public class FollowerTask_ReactOuthouse : FollowerTask
{
	private int _poopStructureID;

	private StructureBrain _poop;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ReactOuthouse;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _poop.Data.Location;
		}
	}

	public override bool BlockTaskChanges
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
			return _poopStructureID;
		}
	}

	public FollowerTask_ReactOuthouse(int poopID)
	{
		_poopStructureID = poopID;
		_poop = StructureManager.GetStructureByID<StructureBrain>(_poopStructureID);
	}

	protected override int GetSubTaskCode()
	{
		return _poopStructureID;
	}

	protected override void OnStart()
	{
		SetState(FollowerTaskState.GoingTo);
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_brain.Location != PlayerFarming.Location)
		{
			End();
		}
	}

	public override void ProgressTask()
	{
		End();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if (_brain.HasTrait(FollowerTrait.TraitType.Germophobe))
		{
			_brain.AddThought(Thought.ReactToFullOuthouseGermophobeTrait);
		}
		else if (_brain.HasTrait(FollowerTrait.TraitType.Coprophiliac))
		{
			_brain.AddThought(Thought.ReactToFullOuthouseCoprophiliacTrait);
		}
		else
		{
			_brain.AddThought(Thought.ReactToFullOuthouse);
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		float num = UnityEngine.Random.Range(2, 3);
		float f = Utils.GetAngle(_poop.Data.Position, follower.transform.position) * ((float)Math.PI / 180f);
		return _poop.Data.Position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	public override void OnDoingBegin(Follower follower)
	{
		if (follower.Brain.HasTrait(FollowerTrait.TraitType.Coprophiliac))
		{
			follower.TimedAnimation("Reactions/react-laugh", 3.3f, delegate
			{
				ProgressTask();
			});
		}
		else
		{
			follower.TimedAnimation("Reactions/react-sick", 2.9666667f, delegate
			{
				ProgressTask();
			});
		}
	}
}
