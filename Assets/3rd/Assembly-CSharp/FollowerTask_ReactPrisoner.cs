using System;
using UnityEngine;

public class FollowerTask_ReactPrisoner : FollowerTask
{
	private int _prisonID;

	private StructureBrain _prison;

	private int state;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ReactPrisoner;
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

	public override int UsingStructureID
	{
		get
		{
			return _prisonID;
		}
	}

	public FollowerTask_ReactPrisoner(int prisonID)
	{
		_prisonID = prisonID;
		_prison = StructureManager.GetStructureByID<StructureBrain>(_prisonID);
	}

	protected override int GetSubTaskCode()
	{
		return _prisonID;
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
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		StructureBrain structureByID = StructureManager.GetStructureByID<StructureBrain>(_prisonID);
		float num = UnityEngine.Random.Range(2, 3);
		float f = Utils.GetAngle(structureByID.Data.Position, follower.transform.position) * ((float)Math.PI / 180f);
		return structureByID.Data.Position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	public override void OnDoingBegin(Follower follower)
	{
		string animation = "Reactions/react-laugh";
		state = 0;
		foreach (IDAndRelationship relationship in follower.Brain.Info.Relationships)
		{
			if (relationship.ID == _prison.Data.FollowerID && (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Lovers || relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Friends))
			{
				animation = "Reactions/react-cry";
				state = 1;
				break;
			}
		}
		follower.FacePosition(_brain.LastPosition);
		follower.TimedAnimation(animation, 3.33f, delegate
		{
			ProgressTask();
		});
	}
}
