using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_ReactCorpse : FollowerTask
{
	private int _corpseStructureID;

	private Structures_DeadWorshipper _corpse;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.ReactCorpse;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _corpse.Data.Location;
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
			return _corpseStructureID;
		}
	}

	public FollowerTask_ReactCorpse(int corpseID)
	{
		_corpseStructureID = corpseID;
		_corpse = StructureManager.GetStructureByID<Structures_DeadWorshipper>(_corpseStructureID);
	}

	protected override int GetSubTaskCode()
	{
		return _corpseStructureID;
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
		if (_brain.HasTrait(FollowerTrait.TraitType.DesensitisedToDeath))
		{
			_brain.AddThought(Thought.HappyToSeeDeadBody);
			return;
		}
		if (_brain.HasTrait(FollowerTrait.TraitType.FearOfDeath))
		{
			if (_corpse.Data.Rotten)
			{
				_brain.AddThought(Thought.GrievedAtRottenUnburiedBodyFearOfDeathTrait);
				if (!(UnityEngine.Random.value < 0.75f))
				{
					return;
				}
				GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
				{
					if (IllnessBar.IllnessNormalized > 0.05f)
					{
						_brain.HardSwapToTask(new FollowerTask_Vomit());
					}
				}));
				return;
			}
			_brain.AddThought(Thought.GrievedAtUnburiedBodyFearOfDeathTrait);
			if (!(UnityEngine.Random.value < 0.33f))
			{
				return;
			}
			GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
			{
				if (IllnessBar.IllnessNormalized > 0.05f)
				{
					_brain.HardSwapToTask(new FollowerTask_Vomit());
				}
			}));
			return;
		}
		if (_corpse.Data.Rotten)
		{
			if (!(UnityEngine.Random.value < 0.5f))
			{
				return;
			}
			_brain.AddThought(Thought.GrievedAtRottenUnburiedBody);
			GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
			{
				if (IllnessBar.IllnessNormalized > 0.05f)
				{
					_brain.HardSwapToTask(new FollowerTask_Vomit());
				}
			}));
			return;
		}
		_brain.AddThought(Thought.GrievedAtUnburiedBody);
		if (!(UnityEngine.Random.value < 0.25f))
		{
			return;
		}
		GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
		{
			if (IllnessBar.IllnessNormalized > 0.05f)
			{
				_brain.HardSwapToTask(new FollowerTask_Vomit());
			}
		}));
	}

	private static IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		Structures_DeadWorshipper structureByID = StructureManager.GetStructureByID<Structures_DeadWorshipper>(_corpseStructureID);
		float num = UnityEngine.Random.Range(0.5f, 1.5f);
		float f = (Utils.GetAngle(structureByID.Data.Position, follower.transform.position) + (float)UnityEngine.Random.Range(-90, 90)) * ((float)Math.PI / 180f);
		return structureByID.Data.Position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	public override void OnDoingBegin(Follower follower)
	{
		bool rotten = _corpse.Data.Rotten;
		Structures_DeadWorshipper structureByID = StructureManager.GetStructureByID<Structures_DeadWorshipper>(_corpseStructureID);
		follower.FacePosition(structureByID.Data.Position);
		if (_brain.HasTrait(FollowerTrait.TraitType.DesensitisedToDeath))
		{
			follower.TimedAnimation("Reactions/react-grieve-happy", 3.3f, delegate
			{
				ProgressTask();
			});
			return;
		}
		IDAndRelationship orCreateRelationship = _brain.Info.GetOrCreateRelationship(_corpse.Data.FollowerID);
		if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Enemies)
		{
			follower.TimedAnimation("Reactions/react-laugh", 3.3f, delegate
			{
				ProgressTask();
			});
		}
		else if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Strangers)
		{
			follower.TimedAnimation("Reactions/react-sad", 3f, delegate
			{
				ProgressTask();
			});
		}
		else if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Friends)
		{
			follower.TimedAnimation("Reactions/react-cry", 3f, delegate
			{
				ProgressTask();
			});
		}
		else if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Lovers)
		{
			follower.TimedAnimation("Reactions/react-cry", 3f, delegate
			{
				ProgressTask();
			});
		}
	}
}
