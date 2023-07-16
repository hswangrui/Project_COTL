using System;
using UnityEngine;

public class FollowerTask_ReactGrave : FollowerTask
{
	private int _graveID;

	private StructureBrain _grave;

	private int state;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ReactGrave;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _grave.Data.Location;
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
			return _graveID;
		}
	}

	public FollowerTask_ReactGrave(int graveID)
	{
		_graveID = graveID;
		_grave = StructureManager.GetStructureByID<StructureBrain>(_graveID);
	}

	protected override int GetSubTaskCode()
	{
		return _graveID;
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
		if (state == 0)
		{
			_brain.AddThought(Thought.ReactGrave);
		}
		else if (state == 1)
		{
			_brain.AddThought(Thought.ReactGraveAfterlife);
		}
		else if (state == 2)
		{
			_brain.AddThought(Thought.ReactGraveLover);
		}
		else if (state == 3)
		{
			_brain.AddThought(Thought.ReactGraveEnemy);
		}
		CultFaithManager.AddThought(Thought.Cult_BuildingGoodGraves, _brain.Info.ID, 1f);
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		StructureBrain structureByID = StructureManager.GetStructureByID<StructureBrain>(_graveID);
		float num = UnityEngine.Random.Range(2, 3);
		float f = Utils.GetAngle(structureByID.Data.Position, follower.transform.position) * ((float)Math.PI / 180f);
		return structureByID.Data.Position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	public override void OnDoingBegin(Follower follower)
	{
		string animation = "Reactions/react-grieve-sad";
		state = 0;
		if (follower.Brain.HasTrait(FollowerTrait.TraitType.DesensitisedToDeath))
		{
			animation = "Reactions/react-grieve-happy";
			state = 1;
		}
		foreach (IDAndRelationship relationship in follower.Brain.Info.Relationships)
		{
			if (relationship.ID == _grave.Data.FollowerID)
			{
				if (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Lovers)
				{
					animation = "Reactions/react-cry";
					state = 2;
					break;
				}
				if (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Enemies)
				{
					animation = "Reactions/react-grieve-happy";
					state = 3;
					break;
				}
			}
		}
		follower.TimedAnimation(animation, 3.33f, delegate
		{
			ProgressTask();
		});
	}
}
