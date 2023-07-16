using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerTask_MissionaryComplete : FollowerTask
{
	private Coroutine _dissentBubbleCoroutine;

	private Follower follower;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.MissionaryComplete;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
		}
	}

	public override bool ShouldSaveDestination
	{
		get
		{
			return true;
		}
	}

	public override bool DisablePickUpInteraction
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

	public override bool BlockSermon
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

	public override bool BlockThoughts
	{
		get
		{
			return true;
		}
	}

	public override float Priorty
	{
		get
		{
			return 1000f;
		}
	}

	public override PriorityCategory GetPriorityCategory(FollowerRole FollowerRole, WorkerPriority WorkerPriority, FollowerBrain brain)
	{
		return PriorityCategory.ExtremelyUrgent;
	}

	protected override int GetSubTaskCode()
	{
		return 0;
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float RestChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override void TaskTick(float deltaGameTime)
	{
	}

	public override void OnGoingToBegin(Follower follower)
	{
		base.OnGoingToBegin(follower);
		if ((bool)follower)
		{
			follower.Seeker.CancelCurrentPathRequest();
			ClearDestination();
			SetState(FollowerTaskState.Doing);
			follower.transform.position = UpdateDestination(follower);
		}
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>(_brain.Location);
		foreach (Interaction_Missionaries missionary in Interaction_Missionaries.Missionaries)
		{
			if (missionary.StructureInfo.ID == allStructuresOfType[follower.Brain._directInfoAccess.MissionaryIndex].Data.ID && missionary.StructureInfo.MultipleFollowerIDs.Contains(follower.Brain.Info.ID))
			{
				return missionary.MissionarySlots[missionary.StructureInfo.MultipleFollowerIDs.IndexOf(follower.Brain.Info.ID)].Free.transform.position;
			}
		}
		if (allStructuresOfType.Count <= 0)
		{
			return _brain.LastPosition;
		}
		return allStructuresOfType[_brain._directInfoAccess.MissionaryIndex].Data.Position;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (follower.Brain._directInfoAccess.MissionaryRewards != null)
		{
			follower.SetBodyAnimation("attention", true);
			follower.HideStats();
			follower.GetComponent<Interaction_MissionaryComplete>().enabled = true;
			follower.Interaction_FollowerInteraction.Interactable = false;
			_dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
			follower.SetOutfit(FollowerOutfitType.Sherpa, false);
			if (follower.Brain._directInfoAccess.MissionaryRewards.Length == 0)
			{
				follower.Brain.Stats.Rest = 10f;
				follower.SetFaceAnimation("Emotions/emotion-tired", true);
			}
			this.follower = follower;
			ClearDestination();
			SetState(FollowerTaskState.GoingTo);
		}
	}

	protected override void OnAbort()
	{
		base.OnAbort();
		if ((bool)follower)
		{
			follower.SetOutfit(FollowerOutfitType.Follower, false);
			follower.Interaction_FollowerInteraction.Interactable = true;
			follower.GetComponent<Interaction_MissionaryComplete>().enabled = false;
			if (_brain._directInfoAccess.MissionaryFinished && _brain._directInfoAccess.MissionaryRewards.Length == 0)
			{
				SetState(FollowerTaskState.Done);
				follower.StartCoroutine(FrameDelayDeath());
			}
		}
	}

	private IEnumerator FrameDelayDeath()
	{
		yield return new WaitForEndOfFrame();
		follower.Die();
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if ((bool)follower)
		{
			if (_dissentBubbleCoroutine != null)
			{
				follower.StopCoroutine(_dissentBubbleCoroutine);
				_dissentBubbleCoroutine = null;
				follower.WorshipperBubble.Close();
			}
			follower.SetOutfit(FollowerOutfitType.Follower, false);
			follower.Interaction_FollowerInteraction.Interactable = true;
			follower.GetComponent<Interaction_MissionaryComplete>().enabled = false;
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (_dissentBubbleCoroutine != null && follower != null)
		{
			follower.StopCoroutine(_dissentBubbleCoroutine);
			_dissentBubbleCoroutine = null;
			follower.WorshipperBubble.Close();
		}
	}

	private IEnumerator DissentBubbleRoutine(Follower follower)
	{
		float bubbleTimer = 0.3f;
		while (true)
		{
			float num;
			bubbleTimer = (num = bubbleTimer - Time.deltaTime);
			if (num < 0f)
			{
				WorshipperBubble.SPEECH_TYPE type = WorshipperBubble.SPEECH_TYPE.HELP;
				follower.WorshipperBubble.Play(type);
				bubbleTimer = 4 + Random.Range(0, 2);
			}
			yield return null;
		}
	}
}
