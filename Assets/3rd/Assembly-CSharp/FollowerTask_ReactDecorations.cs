using System;
using UnityEngine;

public class FollowerTask_ReactDecorations : FollowerTask
{
	private int _poopStructureID;

	private StructureBrain _poop;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ReactDecoration;
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

	public FollowerTask_ReactDecorations(int poopID)
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
		if (_brain.HasTrait(FollowerTrait.TraitType.FalseIdols))
		{
			_brain.AddThought(Thought.ReactDecorationFalseIdols);
		}
		else
		{
			_brain.AddThought(Thought.ReactDecoration);
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		float num = UnityEngine.Random.Range(0.5f, 1.5f);
		float f = (Utils.GetAngle(_poop.Data.Position, follower.transform.position) + (float)UnityEngine.Random.Range(-45, 45)) * ((float)Math.PI / 180f);
		return _poop.Data.Position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	public override void OnDoingBegin(Follower follower)
	{
		int num = UnityEngine.Random.Range(1, 4);
		follower.TimedAnimation("Reactions/react-admire" + num, 2f, delegate
		{
			ProgressTask();
		});
	}
}
