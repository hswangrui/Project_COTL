using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class RitualRessurect : Ritual
{
	private FollowerBrain resurrectingFollower;

	public Light RitualLight;

	private float chanceForZombie = 0.1f;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Ressurect;
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
		Debug.Log("Ritual sacrifice begin gather");
		yield return StartCoroutine(WaitFollowersFormCircle());
		Debug.Log("Ritual sacrifice end gather");
		SimulationManager.Pause();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		//followerSelectInstance.VotingType = TwitchVoting.VotingType.RITUAL_RESURRECT;
		followerSelectInstance.Show(DataManager.Instance.Followers_Dead, null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			resurrectingFollower = FollowerBrain.GetOrCreateBrain(followerInfo);
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnCancel, (Action)delegate
		{
			EndRitual();
			CompleteRitual(true);
			CancelFollowers();
			Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
		while (followerSelectInstance != null)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
		yield return null;
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		GameManager.GetInstance().StartCoroutine(DoRessurectRoutine());
	}

	private IEnumerator DoRessurectRoutine()
	{
		if (RitualLight != null)
		{
			RitualLight.gameObject.SetActive(true);
			RitualLight.color = Color.black;
			RitualLight.DOColor(Color.red, 1f);
			RitualLight.DOIntensity(1.5f, 1f);
		}
		ChurchFollowerManager.Instance.PlayOverlay(ChurchFollowerManager.OverlayType.Ritual, "resurrect");
		yield return new WaitForSeconds(0.5f);
		PlaySacrificePortalEffect();
		Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 7f);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(allBrain.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
			}
		}
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/rituals/resurrect");
		if (DataManager.Instance.ResurrectRitualCount == 1 || DataManager.Instance.ResurrectRitualCount == 0 || UnityEngine.Random.Range(0f, 1f) < chanceForZombie)
		{
		}
		bool isZombie = false;
		DataManager.Instance.ResurrectRitualCount++;
		DataManager.Instance.Followers_Dead.Remove(resurrectingFollower._directInfoAccess);
		DataManager.Instance.Followers_Dead_IDs.Remove(resurrectingFollower._directInfoAccess.ID);
		foreach (Structures_Grave item in StructureManager.GetAllStructuresOfType<Structures_Grave>(FollowerLocation.Base))
		{
			if (item.Data.FollowerID == resurrectingFollower.Info.ID)
			{
				item.Data.FollowerID = -1;
			}
		}
		foreach (Structures_Crypt item2 in StructureManager.GetAllStructuresOfType<Structures_Crypt>(FollowerLocation.Base))
		{
			if (item2.Data.MultipleFollowerIDs.Contains(resurrectingFollower.Info.ID))
			{
				item2.WithdrawBody(resurrectingFollower.Info.ID);
			}
		}
		foreach (Structures_Morgue item3 in StructureManager.GetAllStructuresOfType<Structures_Morgue>(FollowerLocation.Base))
		{
			if (item3.Data.MultipleFollowerIDs.Contains(resurrectingFollower.Info.ID))
			{
				item3.WithdrawBody(resurrectingFollower.Info.ID);
			}
		}
		resurrectingFollower.ResetStats();
		if (resurrectingFollower.Info.Age > resurrectingFollower.Info.LifeExpectancy)
		{
			resurrectingFollower.Info.LifeExpectancy = resurrectingFollower.Info.Age + UnityEngine.Random.Range(20, 30);
		}
		else
		{
			resurrectingFollower.Info.LifeExpectancy += UnityEngine.Random.Range(20, 30);
		}
		FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
		resurrectingFollower.HardSwapToTask(nextTask);
		resurrectingFollower.Location = FollowerLocation.Church;
		resurrectingFollower.DesiredLocation = FollowerLocation.Church;
		resurrectingFollower.CurrentTask.Arrive();
		Follower revivedFollower = FollowerManager.CreateNewFollower(resurrectingFollower._directInfoAccess, ChurchFollowerManager.Instance.RitualCenterPosition.position);
		revivedFollower.SetOutfit(FollowerOutfitType.Worker, false);
		revivedFollower.Brain.CheckChangeState();
		Ritual.FollowerToAttendSermon.Add(revivedFollower.Brain);
		revivedFollower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().OnConversationNext(revivedFollower.gameObject, 5f);
		revivedFollower.Spine.gameObject.SetActive(false);
		yield return new WaitForSeconds(1f);
		revivedFollower.Spine.gameObject.SetActive(true);
		DeadWorshipper deadWorshipper = null;
		foreach (DeadWorshipper deadWorshipper2 in DeadWorshipper.DeadWorshippers)
		{
			if (deadWorshipper2.StructureInfo.FollowerID == resurrectingFollower.Info.ID)
			{
				deadWorshipper = deadWorshipper2;
				break;
			}
		}
		if (deadWorshipper != null)
		{
			StructureManager.RemoveStructure(deadWorshipper.Structure.Brain);
		}
		GameManager.GetInstance().OnConversationNext(revivedFollower.gameObject, 8f);
		revivedFollower.SetBodyAnimation("Sermons/resurrect", false);
		if (isZombie)
		{
			revivedFollower.AddBodyAnimation("Insane/be-weird", false, 0f);
			revivedFollower.AddBodyAnimation("Insane/idle-insane", true, 0f);
			revivedFollower.Brain.ApplyCurseState(Thought.Zombie);
			yield return new WaitForSeconds(4.5f);
		}
		else
		{
			revivedFollower.AddBodyAnimation("Reactions/react-enlightened1", false, 0f);
			revivedFollower.AddBodyAnimation("idle", true, 0f);
			yield return new WaitForSeconds(2f);
		}
		if (isZombie)
		{
			float num = 0f;
			foreach (FollowerBrain item4 in Ritual.FollowerToAttendSermon)
			{
				float num2 = UnityEngine.Random.Range(0.1f, 0.5f);
				num += num2;
				StartCoroutine(DelayFollowerReaction(item4, "Reactions/react-spooked", num2));
			}
			yield return new WaitForSeconds(2f);
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Zombies))
			{
				UITutorialOverlayController tutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Zombies);
				while (tutorialOverlay != null)
				{
					yield return null;
				}
			}
		}
		Interaction_TempleAltar.Instance.PulseDisplacementObject(revivedFollower.transform.position);
		yield return new WaitForSeconds(2f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(2f / 3f);
		float num3 = 0.5f;
		foreach (FollowerBrain item5 in Ritual.FollowerToAttendSermon)
		{
			float num4 = UnityEngine.Random.Range(0.1f, 0.5f);
			num3 += num4;
			StartCoroutine(DelayFollowerReaction(item5, num4));
		}
		yield return new WaitForSeconds(1f);
		BiomeConstants.Instance.ChromaticAbberationTween(0.25f, 7f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		EndRitual();
		yield return new WaitForSeconds(0.5f);
		CompleteRitual(false, revivedFollower.Brain.Info.ID);
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_Ressurection, revivedFollower.Brain.Info.ID, 1f);
	}

	private void EndRitual()
	{
		StopSacrificePortalEffect();
		ChurchFollowerManager.Instance.StopSacrificePortalEffect();
	}
}
