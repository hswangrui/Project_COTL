using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using Lamb.UI.Tarot;
using src.Extensions;
using UnityEngine;

public class RitualFightpit : Ritual
{
	public static Follower contestant1;

	public static Follower contestant2;

	private FollowerTask_ManualControl Task1;

	private FollowerTask_ManualControl Task2;

	private bool Waiting = true;

	private EventInstance loopedSound;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Fightpit;
		}
	}

	public override void Play()
	{
		base.Play();
		StartCoroutine(RitualRoutine());
	}

	private IEnumerator RitualRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
		yield return StartCoroutine(CentreAndAnimatePlayer());
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		yield return StartCoroutine(WaitFollowersFormCircle());
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		UIFollowerSelectMenuController followerSelectInstance2 = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		//followerSelectInstance2.VotingType = TwitchVoting.VotingType.RITUAL_FIGHT_PIT;
		followerSelectInstance2.Show(Ritual.GetFollowersAvailableToAttendSermon(), null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance2;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			contestant1 = FollowerManager.FindFollowerByID(followerInfo.ID);
			AudioManager.Instance.PlayOneShot("event:/rituals/wedding_select_follower", PlayerFarming.Instance.gameObject);
			loopedSound = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
			Task1 = new FollowerTask_ManualControl();
			contestant1.Brain.HardSwapToTask(Task1);
			StartCoroutine(SetUpCombatant1Routine());
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance2;
		uIFollowerSelectMenuController2.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnCancel, (Action)delegate
		{
			AudioManager.Instance.StopLoop(loopedSound);
			GameManager.GetInstance().StartCoroutine(RitualFinished(true, -1, -1));
			CancelFollowers();
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance2;
		uIFollowerSelectMenuController3.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnHidden, (Action)delegate
		{
			followerSelectInstance2 = null;
		});
		while (followerSelectInstance2 != null || Waiting)
		{
			yield return null;
		}
		Waiting = true;
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		//followerSelectInstance.VotingType = TwitchVoting.VotingType.RITUAL_FIGHT_PIT;
		followerSelectInstance.Show(Ritual.GetFollowersAvailableToAttendSermon(), new List<FollowerBrain> { contestant1.Brain }, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController4 = followerSelectInstance;
		uIFollowerSelectMenuController4.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController4.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.fight_pit_drums);
			contestant2 = FollowerManager.FindFollowerByID(followerInfo.ID);
			AudioManager.Instance.PlayOneShot("event:/rituals/wedding_select_follower", PlayerFarming.Instance.gameObject);
			loopedSound = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
			Task2 = new FollowerTask_ManualControl();
			contestant2.Brain.HardSwapToTask(Task2);
			StartCoroutine(SetUpCombatant2Routine());
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController5 = followerSelectInstance;
		uIFollowerSelectMenuController5.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController5.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().StartCoroutine(RitualFinished(true, -1, -1));
			CancelFollowers();
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController6 = followerSelectInstance;
		uIFollowerSelectMenuController6.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController6.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
		while (followerSelectInstance != null || Waiting)
		{
			yield return null;
		}
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		yield return new WaitForSeconds(1f);
		contestant1.SetBodyAnimation("Reactions/react-bow", false);
		contestant2.SetBodyAnimation("Reactions/react-bow", false);
		yield return new WaitForSeconds(1.9333333f);
		int WaitCount = 0;
		Task1.GoToAndStop(contestant1, Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.right * 0.4f, delegate
		{
			int num2 = WaitCount + 1;
			WaitCount = num2;
		});
		Task2.GoToAndStop(contestant2, Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.left * 0.4f, delegate
		{
			int num = WaitCount + 1;
			WaitCount = num;
		});
		while (WaitCount < 2)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/rituals/fight_pit", contestant1.transform.position);
		contestant1.SetBodyAnimation("fight", true);
		contestant2.SetBodyAnimation("fight", true);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(Vector3.Lerp(contestant1.transform.position, contestant2.transform.position, 0.5f));
		CameraManager.instance.ShakeCameraForDuration(0.1f, 1f, 3f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact);
		yield return new WaitForSeconds(3f);
		if (UnityEngine.Random.value <= 0.5f)
		{
			Follower follower = contestant1;
			contestant1 = contestant2;
			contestant2 = follower;
		}
		foreach (StoryData storyObjective in DataManager.Instance.StoryObjectives)
		{
			foreach (StoryDataItem item in Quests.GetChildStoryDataItemsFromStoryDataItem(storyObjective.EntryStoryItem))
			{
				foreach (ObjectivesData objective in DataManager.Instance.Objectives)
				{
					if (item.Objective != null && objective.Index == item.Objective.Index && item.Objective is Objectives_PerformRitual && ((Objectives_PerformRitual)item.Objective).Ritual == UpgradeSystem.Type.Ritual_Fightpit && ((contestant1.Brain.Info.ID == ((Objectives_PerformRitual)item.Objective).TargetFollowerID_1 && contestant2.Brain.Info.ID == ((Objectives_PerformRitual)item.Objective).TargetFollowerID_2) || (contestant1.Brain.Info.ID == ((Objectives_PerformRitual)item.Objective).TargetFollowerID_2 && contestant2.Brain.Info.ID == ((Objectives_PerformRitual)item.Objective).TargetFollowerID_1)) && contestant1.Brain.Info.ID == ((Objectives_PerformRitual)item.Objective).TargetFollowerID_1)
					{
						Follower follower2 = contestant1;
						contestant1 = contestant2;
						contestant2 = follower2;
						break;
					}
				}
			}
		}
		contestant1.SetBodyAnimation("fight-lose", false);
		contestant1.AddBodyAnimation("unconverted", true, 0f);
		contestant2.SetBodyAnimation("fight-win", false);
		contestant2.AddBodyAnimation("cheer", true, 0f);
		yield return new WaitForSeconds(1.1f);
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.Temple);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position - Vector3.back);
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.2f);
		yield return new WaitForSeconds(0.9f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			if (item2.CurrentTask is FollowerTask_AttendRitual)
			{
				(item2.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		yield return new WaitForSeconds(1.5f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		yield return new WaitForSeconds(0.5f);
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			if (item3.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(item3.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
			}
		}
		GameObject g = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Choice Indicator"), GameObject.FindWithTag("Canvas").transform) as GameObject;
		ChoiceIndicator c = g.GetComponent<ChoiceIndicator>();
		c.Offset = new Vector3(0f, -350f);
		c.Show("<sprite name=\"icon_ThumbsUp\">", "<sprite name=\"icon_ThumbsDown\">", delegate
		{
			StartCoroutine(ShowMercy());
		}, delegate
		{
			StartCoroutine(Execute());
		}, PlayerFarming.Instance.transform.position);
		while (g != null)
		{
			c.UpdatePosition(PlayerFarming.Instance.transform.position);
			yield return null;
		}
	}

	private IEnumerator SetUpCombatant1Routine()
	{
		yield return null;
		ChurchFollowerManager.Instance.RemoveBrainFromAudience(contestant1.Brain);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				allBrain.CurrentTask.RecalculateDestination();
				allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
			}
		}
		Task1.GoToAndStop(contestant1, Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.right * 1f, delegate
		{
			contestant1.FacePosition(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
			contestant1.SetBodyAnimation("idle", true);
			Waiting = false;
		});
	}

	private IEnumerator SetUpCombatant2Routine()
	{
		yield return null;
		ChurchFollowerManager.Instance.RemoveBrainFromAudience(contestant2.Brain);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				allBrain.CurrentTask.RecalculateDestination();
				allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
			}
		}
		Task2.GoToAndStop(contestant2, Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.left * 1f, delegate
		{
			contestant2.FacePosition(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
			contestant2.SetBodyAnimation("idle", true);
			Waiting = false;
		});
	}

	private IEnumerator ShowMercy()
	{
		GameManager.GetInstance().OnConversationNext(contestant1.gameObject);
		GameManager.GetInstance().AddToCamera(contestant2.gameObject);
		yield return new WaitForSeconds(1f);
		contestant1.SetBodyAnimation("Conversations/greet-nice", false);
		contestant1.AddBodyAnimation("idle", true, 0f);
		contestant2.SetBodyAnimation("Conversations/greet-hate", false);
		contestant2.AddBodyAnimation("idle", true, 0f);
		if (!TarotCards.IsUnlocked(TarotCards.Card.Lovers2))
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.TRINKET_CARD_UNLOCKED, 1, contestant1.transform.position).GetComponent<Interaction_TarotCardUnlock>().CardOverride = TarotCards.Card.Lovers2;
			yield return new WaitForSeconds(1f);
		}
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			item.AddThought(Thought.FightPitMercy);
			if (item.CurrentTask is FollowerTask_AttendRitual)
			{
				(item.CurrentTask as FollowerTask_AttendRitual).Boo();
			}
		}
		yield return new WaitForSeconds(2f);
		contestant1.Brain.AddAdoration(FollowerBrain.AdorationActions.FightPitMercy, null);
		ChurchFollowerManager.Instance.AddBrainToAudience(contestant1.Brain);
		ChurchFollowerManager.Instance.AddBrainToAudience(contestant2.Brain);
		GameManager.GetInstance().StartCoroutine(EndRitual(contestant1.Brain.Info.ID, contestant2.Brain.Info.ID));
	}

	private IEnumerator Execute()
	{
		int followerID_1 = contestant1.Brain.Info.ID;
		int followerID_2 = contestant2.Brain.Info.ID;
		GameManager.GetInstance().OnConversationNext(contestant1.gameObject, 6f);
		GameManager.GetInstance().AddToCamera(contestant2.gameObject);
		yield return new WaitForSeconds(0.5f);
		Ritual.FollowerToAttendSermon.Remove(contestant1.Brain);
		contestant2.SetBodyAnimation("fight-kill", false);
		contestant2.AddBodyAnimation("cheer", true, 0f);
		yield return new WaitForSeconds(1.1f);
		AudioManager.Instance.PlayOneShot("event:/rituals/fight_pit_kill", contestant2.transform.position);
		float seconds = contestant1.SetBodyAnimation("fight-die", false);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(contestant1.CameraBone.transform.position - Vector3.back);
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.2f);
		yield return new WaitForSeconds(seconds);
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", contestant1.transform.position);
		AudioManager.Instance.PlayOneShot("event:/player/harvest_meat_done", contestant1.transform.position);
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 3, contestant1.transform.position);
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, 2, contestant1.transform.position);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(contestant1.transform.position, new Vector3(0.5f, 0.5f, 1f));
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		FollowerManager.FollowerDie(contestant1.Brain.Info.ID, NotificationCentre.NotificationType.KilledInAFightPit);
		UnityEngine.Object.Destroy(contestant1.gameObject);
		ChurchFollowerManager.Instance.AddBrainToAudience(contestant2.Brain);
		yield return null;
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(item.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		yield return new WaitForSeconds(2f);
		contestant2.Brain.AddAdoration(FollowerBrain.AdorationActions.FightPitDeath, null);
		GameManager.GetInstance().StartCoroutine(EndRitual(followerID_1, followerID_2));
	}

	private IEnumerator EndRitual(int follower1, int follower2)
	{
		JudgementMeter.ShowModify(-1);
		AudioManager.Instance.StopLoop(loopedSound);
		float EndingDelay = 0f;
		yield return null;
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon())
		{
			float num = UnityEngine.Random.Range(0.1f, 0.5f);
			EndingDelay += num;
			StartCoroutine(DelayFollowerReaction(item, num));
		}
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().StartCoroutine(RitualFinished(false, follower1, follower2));
	}

	private IEnumerator RitualFinished(bool cancelled, int follower1, int follower2)
	{
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.Temple);
		AudioManager.Instance.StopLoop(loopedSound);
		CompleteRitual(cancelled, follower1, follower2);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		yield return new WaitForSeconds(1f);
		if (!cancelled)
		{
			CultFaithManager.AddThought(Thought.Cult_FightPit, -1, 1f);
		}
	}
}
