using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_Zombie : FollowerTask
{
	private const float IDLE_DURATION_GAME_MINUTES_MIN = 10f;

	private const float IDLE_DURATION_GAME_MINUTES_MAX = 20f;

	private float _gameTimeToNextStateUpdate;

	private float _speechDurationRemaining;

	private Follower follower;

	private float delay = 5f;

	private float deadBodyCheck = 5f;

	private StructureBrain deadBody;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Zombie;
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

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (_state == FollowerTaskState.Idle)
		{
			_gameTimeToNextStateUpdate -= deltaGameTime;
			if (_gameTimeToNextStateUpdate <= 0f)
			{
				ClearDestination();
				SetState(FollowerTaskState.GoingTo);
				_gameTimeToNextStateUpdate = Random.Range(10f, 20f);
			}
		}
		delay -= Time.deltaTime;
		deadBodyCheck -= Time.deltaTime;
		if (deadBodyCheck <= 0f && deadBody == null)
		{
			List<StructureBrain> allStructuresOfType = StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.DEAD_WORSHIPPER);
			if (allStructuresOfType.Count > 0)
			{
				deadBody = allStructuresOfType[0];
				SetState(FollowerTaskState.GoingTo);
			}
			deadBodyCheck = 5f;
		}
		if (deadBody != null && _state == FollowerTaskState.Idle)
		{
			SetState(FollowerTaskState.GoingTo);
		}
		if (_brain.Stats.Satiation != 0f || !(delay < 0f) || deadBody != null)
		{
			return;
		}
		FollowerBrain targetFollower = null;
		float num = float.MaxValue;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			float num2 = Vector3.Distance(PlayerFarming.Instance.transform.position, allBrain.LastPosition);
			if (num2 < num && allBrain.Info.CursedState != Thought.Zombie && !DataManager.Instance.Followers_OnMissionary_IDs.Contains(allBrain._directInfoAccess.ID) && !DataManager.Instance.Followers_Imprisoned_IDs.Contains(allBrain._directInfoAccess.ID))
			{
				num = num2;
				targetFollower = allBrain;
			}
		}
		_brain.HardSwapToTask(new FollowerTask_ZombieKillFollower(targetFollower));
	}

	public override void OnIdleBegin(Follower follower)
	{
		base.OnIdleBegin(follower);
		if (follower != null && _brain.Stats.Starvation >= 37.5f && !follower.WorshipperBubble.Active)
		{
			WorshipperBubble.SPEECH_TYPE type = WorshipperBubble.SPEECH_TYPE.FOLLOWERMEAT;
			follower.WorshipperBubble.Play(type);
		}
	}

	protected override void OnArrive()
	{
		if (deadBody != null)
		{
			if (StructureManager.GetStructureByID<StructureBrain>(deadBody.Data.ID) != null)
			{
				if (Vector3.Distance(deadBody.Data.Position, _brain.LastPosition) < 2f)
				{
					if (follower != null)
					{
						follower.StartCoroutine(EatDeadBodyIE());
					}
					else
					{
						EatDeadBody();
					}
				}
				else
				{
					SetState(FollowerTaskState.Idle);
				}
			}
			else
			{
				deadBody = null;
				SetState(FollowerTaskState.Idle);
			}
		}
		else
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	private IEnumerator EatDeadBodyIE()
	{
		foreach (Interaction_HarvestMeat interaction_HarvestMeat in Interaction_HarvestMeat.Interaction_HarvestMeats)
		{
			if (interaction_HarvestMeat.structure.Structure_Info.ID == deadBody.Data.ID)
			{
				interaction_HarvestMeat.enabled = false;
				break;
			}
		}
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("Insane/eat-poop", false);
		follower.AddBodyAnimation("Insane/idle-insane", false, 0f);
		yield return new WaitForSeconds(2f);
		deadBody.Remove();
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", follower.gameObject);
		for (int i = 0; i < 5; i++)
		{
			ResourceCustomTarget.Create(follower.gameObject, deadBody.Data.Position + (Vector3)Random.insideUnitCircle, InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, null);
		}
		yield return new WaitForSeconds(1f);
		_brain.Stats.Satiation = 100f;
		deadBody = null;
		follower.State.CURRENT_STATE = StateMachine.State.Idle;
		SetState(FollowerTaskState.Idle);
		_brain.AddThought(Thought.ZombieAteMeal);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.CursedState != Thought.Zombie)
			{
				allBrain.AddThought(Thought.ZombieAteDeadFollower);
			}
		}
	}

	private void EatDeadBody()
	{
		_brain.Stats.Satiation = 100f;
		deadBody.Remove();
		deadBody = null;
		SetState(FollowerTaskState.Idle);
		_brain.AddThought(Thought.ZombieAteMeal);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.CursedState != Thought.Zombie)
			{
				allBrain.AddThought(Thought.ZombieAteDeadFollower);
			}
		}
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Insane/idle-insane");
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Insane/run-insane");
		follower.SetOutfit(FollowerOutfitType.Rags, false);
		this.follower = follower;
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (deadBody != null)
		{
			return deadBody.Data.Position + (Vector3)Random.insideUnitCircle * 0.5f;
		}
		return TownCentre.RandomCircleFromTownCentre(8f);
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float SocialChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float VomitChange(float deltaGameTime)
	{
		return 0f;
	}
}
