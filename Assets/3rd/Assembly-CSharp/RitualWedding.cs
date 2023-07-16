using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using Lamb.UI.Tarot;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class RitualWedding : Ritual
{
	private Follower contestant1;

	private FollowerTask_ManualControl Task1;

	private int followerID;

	private EventInstance loopedSound;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Wedding;
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
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		PlayerFarming.Instance.GoToAndStop(Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.left * 0.5f, null, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("idle", 0, true);
		});
		yield return StartCoroutine(WaitFollowersFormCircle());
		yield return new WaitForSeconds(1f);
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
	//	followerSelectInstance.VotingType = TwitchVoting.VotingType.RITUAL_MARRY;
		followerSelectInstance.Show(Ritual.GetFollowersAvailableToAttendSermon(), null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.bongos_singing);
			followerID = followerInfo.ID;
			contestant1 = FollowerManager.FindFollowerByID(followerInfo.ID);
			AudioManager.Instance.PlayOneShot("event:/rituals/wedding_select_follower", PlayerFarming.Instance.gameObject);
			Task1 = new FollowerTask_ManualControl();
			contestant1.Brain.HardSwapToTask(Task1);
			GameManager.GetInstance().StartCoroutine(ContinueRitual());
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnShow = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnShow, (Action)delegate
		{
			foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
			{
				if (followerInfoBox.followBrain.Info.MarriedToLeader)
				{
					followerInfoBox.ShowMarried();
				}
			}
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnCancel, (Action)delegate
		{
			Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
			GameManager.GetInstance().StartCoroutine(EndRitual());
			CancelFollowers();
			AudioManager.Instance.StopLoop(loopedSound);
			CompleteRitual(true);
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController4 = followerSelectInstance;
		uIFollowerSelectMenuController4.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController4.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
	}

	private IEnumerator ContinueRitual()
	{
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(base.transform.position, contestant1.transform.position);
		yield return StartCoroutine(SetUpCombatant1Routine());
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(base.transform.position, contestant1.transform.position);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("kiss-follower", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("dance", 0, true, 0f);
		contestant1.SetBodyAnimation("kiss", true);
		yield return new WaitForSeconds(5f / 6f);
		Vector3 position = Interaction_TempleAltar.Instance.PortalEffect.transform.position;
		BiomeConstants.Instance.EmitHeartPickUpVFX(position, 0f, "red", "burst_big");
		BiomeConstants.Instance.EmitConfettiVFX(position);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 1f, 0.5f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact);
		AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.transform.position);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position - Vector3.back);
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(item.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		contestant1.Brain.Info.MarriedToLeader = true;
		yield return new WaitForSeconds(3f);
		contestant1.SetBodyAnimation("dance", true);
		int targetFollowerID = contestant1.Brain.Info.ID;
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			if (item2.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(item2.CurrentTask as FollowerTask_AttendRitual).Dance();
			}
		}
		yield return new WaitForSeconds(4f);
		if (!TarotCards.IsUnlocked(TarotCards.Card.Lovers2))
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.TRINKET_CARD_UNLOCKED, 1, contestant1.transform.position).GetComponent<Interaction_TarotCardUnlock>().CardOverride = TarotCards.Card.Lovers2;
			yield return new WaitForSeconds(1f);
		}
		List<int> JealousSpouses = new List<int>();
		FollowerBrain.AddMarriageThoughts();
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			if (item3 != contestant1.Brain)
			{
				if (!item3.Info.MarriedToLeader)
				{
					item3.AddThought(Thought.AttendedWedding);
					continue;
				}
				item3.AddThought(Thought.AttendedWeddingSpouse);
				JealousSpouses.Add(item3.Info.ID);
				Debug.Log("ATTENDED WEDDING AS SPOUSE!");
			}
		}
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.Temple);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		float num = 0f;
		foreach (FollowerBrain item4 in Ritual.FollowerToAttendSermon)
		{
			float num2 = UnityEngine.Random.Range(0.1f, 0.5f);
			num += num2;
			StartCoroutine(DelayFollowerReaction(item4, num2));
		}
		AudioManager.Instance.StopLoop(loopedSound);
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		JudgementMeter.ShowModify(1);
		yield return new WaitForSeconds(0.5f);
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Wedding))
		{
			UITutorialOverlayController tutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Wedding);
			while (tutorialOverlay != null)
			{
				yield return null;
			}
		}
		ChurchFollowerManager.Instance.RemoveBrainFromAudience(contestant1.Brain);
		CompleteRitual(false, targetFollowerID);
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_Wedding, followerID, 1f);
		foreach (int item5 in JealousSpouses)
		{
			CultFaithManager.AddThought(Thought.Cult_Wedding_JealousSpouse, item5, 1f);
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
		bool isAtDestination = false;
		Task1.GoToAndStop(contestant1, Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.right * 0.5f, delegate
		{
			contestant1.FacePosition(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
			contestant1.SetBodyAnimation("idle", true);
			isAtDestination = true;
		});
		while (!isAtDestination)
		{
			yield return null;
		}
	}

	private IEnumerator EndRitual()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		yield return new WaitForSeconds(0.5f);
	}
}
