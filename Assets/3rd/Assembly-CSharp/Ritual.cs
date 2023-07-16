using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Spine;
using UnityEngine;

public class Ritual : BaseMonoBehaviour
{
	public static Action<bool> OnEnd;

	public static List<FollowerBrain> FollowerToAttendSermon;

	[CompilerGenerated]
	private readonly UpgradeSystem.Type _003CRitualType_003Ek__BackingField;

	protected virtual UpgradeSystem.Type RitualType
	{
		[CompilerGenerated]
		get
		{
			return _003CRitualType_003Ek__BackingField;
		}
	}

	public static int FollowersAvailableToAttendSermon()
	{
		return GetFollowersAvailableToAttendSermon().Count;
	}

	public static List<FollowerBrain> GetFollowersAvailableToAttendSermon(bool ignoreDissenters = false)
	{
		List<FollowerBrain> list = new List<FollowerBrain>();
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if ((!ignoreDissenters || allBrain.Info.CursedState != Thought.Dissenter) && (allBrain.CurrentTask == null || !allBrain.CurrentTask.BlockSermon) && !FollowerManager.FollowerLocked(allBrain.Info.ID, true))
			{
				list.Add(allBrain);
			}
		}
		return list;
	}

	public virtual void Play()
	{
		foreach (FollowerBrain item in GetFollowersAvailableToAttendSermon())
		{
			if (item.CurrentTask != null && (item.CurrentTask is FollowerTask_AttendRitual || item.CurrentTask is FollowerTask_AttendTeaching))
			{
				item.CurrentTask.Abort();
			}
		}
		Interaction_TempleAltar.Instance.FrontWall.SetActive(false);
	}

	public void CompleteRitual(bool cancelled = false, int targetFollowerID_1 = -1, int targetFollowerID_2 = -1)
	{
		if (!cancelled && !FollowerBrainStats.BrainWashed)
		{
			FaithBarFake.Play(UpgradeSystem.GetRitualFaithChange(RitualType));
		}
		Interaction_TempleAltar.Instance.FrontWall.SetActive(true);
		Action<bool> onEnd = OnEnd;
		if (onEnd != null)
		{
			onEnd(cancelled);
		}
		if (!cancelled)
		{
			ObjectiveManager.CompleteRitualObjective(RitualType, targetFollowerID_1, targetFollowerID_2);
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PerformAnyRitual);
			DataManager.Instance.HasPerformedRitual = true;
		}
		if (DataManager.Instance.WokeUpEveryoneDay == TimeManager.CurrentDay && TimeManager.CurrentPhase == DayPhase.Night && !FollowerBrainStats.IsWorkThroughTheNight)
		{
			CultFaithManager.AddThought(Thought.Cult_WokeUpEveryone, -1, 1f);
		}
		GameManager.GetInstance().StartCoroutine(Interaction_TempleAltar.Instance.FollowersEnterForSermonRoutine(true));
		Interaction_TempleAltar.Instance.OnInteract(PlayerFarming.Instance.state);
		ChurchFollowerManager.Instance.DisableAllOverlays();
	}

	public IEnumerator CentrePlayer()
	{
		Interaction_TempleAltar.Instance.state.facingAngle = 270f;
		float Progress = 0f;
		Vector3 StartPosition = Interaction_TempleAltar.Instance.state.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < 0.5f)
			{
				Interaction_TempleAltar.Instance.state.transform.position = Vector3.Lerp(StartPosition, ChurchFollowerManager.Instance.AltarPosition.position, Mathf.SmoothStep(0f, 1f, Progress / 0.5f));
				yield return null;
				continue;
			}
			break;
		}
	}

	public IEnumerator CentreAndAnimatePlayer()
	{
		Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		StartCoroutine(CentrePlayer());
	}

	public IEnumerator WaitFollowersFormCircle(bool ignoreDissenters = false)
	{
		GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 12f);
		yield return StartCoroutine(FollowersEnterForRitualRoutine(ignoreDissenters));
		GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.PortalEffect.gameObject, 8f);
	}

	public IEnumerator FollowersEnterForRitualRoutine(bool ignoreDissenters = false)
	{
		bool getFollowers = FollowerToAttendSermon == null || FollowerToAttendSermon.Count <= 0;
		if (getFollowers)
		{
			FollowerToAttendSermon = new List<FollowerBrain>();
		}
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			DataManager.Instance.WokeUpEveryoneDay = TimeManager.CurrentDay;
		}
		foreach (FollowerBrain item in GetFollowersAvailableToAttendSermon(ignoreDissenters))
		{
			Follower follower = FollowerManager.FindFollowerByID(item.Info.ID);
			if ((bool)follower)
			{
				follower.Brain.ShouldReconsiderTask = false;
				follower.HideAllFollowerIcons();
				follower.Spine.UseDeltaTime = true;
				follower.UseUnscaledTime = false;
			}
			if (getFollowers)
			{
				FollowerToAttendSermon.Add(item);
			}
			if (item.CurrentTask != null)
			{
				item.CurrentTask.Abort();
			}
			item.HardSwapToTask(new FollowerTask_AttendRitual());
			item.ShouldReconsiderTask = false;
			if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
			{
				item.CurrentTask.Arrive();
			}
			Follower follower2 = FollowerManager.FindFollowerByID(item.Info.ID);
			if ((object)follower2 != null)
			{
				follower2.HideAllFollowerIcons();
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.15f));
		}
		yield return null;
		Debug.Log("RECALCULATE! " + FollowerToAttendSermon.Count);
		foreach (FollowerBrain item2 in FollowerToAttendSermon)
		{
			FollowerTask currentTask = item2.CurrentTask;
			if (currentTask != null)
			{
				currentTask.RecalculateDestination();
			}
		}
		float timer = 0f;
		while (!FollowersInPosition())
		{
			float num;
			timer = (num = timer + Time.deltaTime);
			if (!(num < 10f))
			{
				break;
			}
			yield return null;
		}
		SimulationManager.Pause();
	}

	public void PlaySacrificePortalEffect()
	{
		Interaction_TempleAltar.Instance.PortalEffect.gameObject.SetActive(true);
		Interaction_TempleAltar.Instance.PortalEffect.AnimationState.SetAnimation(0, "start", false);
		Interaction_TempleAltar.Instance.PortalEffect.AnimationState.AddAnimation(0, "animation", true, 0f);
	}

	public void StopSacrificePortalEffect()
	{
		Interaction_TempleAltar.Instance.PortalEffect.AnimationState.SetAnimation(0, "stop", false);
		Interaction_TempleAltar.Instance.PortalEffect.AnimationState.Complete += PortalEffectComplete;
	}

	private void PortalEffectComplete(TrackEntry trackEntry)
	{
		Interaction_TempleAltar.Instance.PortalEffect.AnimationState.Complete -= PortalEffectComplete;
		Interaction_TempleAltar.Instance.PortalEffect.gameObject.SetActive(false);
	}

	private bool FollowersInPosition()
	{
		foreach (FollowerBrain item in FollowerToAttendSermon)
		{
			if (item.CurrentTaskType != FollowerTaskType.ChangeLocation && item.Location == PlayerFarming.Location && (item.CurrentTaskType != FollowerTaskType.AttendTeaching || item.CurrentTask.State == FollowerTaskState.Doing))
			{
				continue;
			}
			if (item.Location != PlayerFarming.Location)
			{
				item.HardSwapToTask(new FollowerTask_AttendRitual());
				item.ShouldReconsiderTask = false;
				if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					item.CurrentTask.Arrive();
				}
			}
			return false;
		}
		return true;
	}

	public IEnumerator DelayFollowerReaction(FollowerBrain brain, float Delay)
	{
		Follower f = FollowerManager.FindFollowerByID(brain.Info.ID);
		yield return new WaitForSecondsRealtime(Delay);
		if (!(f != null))
		{
			yield break;
		}
		f.HideAllFollowerIcons();
		f.HoodOff("idle", false, delegate
		{
			f.UseUnscaledTime = true;
			f.TimedAnimation("Reactions/react-enlightened1", 1.5f, delegate
			{
				f.Brain.HardSwapToTask(new FollowerTask_AttendTeaching());
			}, false, false);
		});
	}

	public IEnumerator DelayFollowerReaction(FollowerBrain brain, string anim, float Delay)
	{
		Follower f = FollowerManager.FindFollowerByID(brain.Info.ID);
		yield return new WaitForSecondsRealtime(Delay);
		if (!(f != null))
		{
			yield break;
		}
		f.HideAllFollowerIcons();
		f.HoodOff("idle", false, delegate
		{
			f.UseUnscaledTime = true;
			f.TimedAnimation("Reactions/react-enlightened1", 1.5f, delegate
			{
				f.Brain.HardSwapToTask(new FollowerTask_AttendTeaching());
			}, false, false);
		});
	}

	public void CancelFollowers()
	{
		foreach (FollowerBrain item in GetFollowersAvailableToAttendSermon())
		{
			item.CompleteCurrentTask();
		}
	}
}
