using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using UnityEngine;

public class RitualTaxEnforcer : Ritual
{
	private Follower contestant1;

	private FollowerTask_ManualControl Task1;

	private bool Waiting = true;

	private EventInstance loopedSound;

	private bool waiting = true;

	private int count;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_AssignTaxCollector;
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
		yield return StartCoroutine(WaitFollowersFormCircle());
		yield return new WaitForSeconds(1f);
		List<FollowerBrain> list = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Info.CursedState != 0 || list[num].Info.TaxEnforcer)
			{
				list.RemoveAt(num);
			}
		}
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
	//	followerSelectInstance.VotingType = TwitchVoting.VotingType.RITUAL_ENFORCER;
		followerSelectInstance.Show(list);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			contestant1 = FollowerManager.FindFollowerByID(followerInfo.ID);
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
			loopedSound = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
			Task1 = new FollowerTask_ManualControl();
			contestant1.Brain.HardSwapToTask(Task1);
			contestant1.Brain.Info.FaithEnforcer = false;
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain.Info.TaxEnforcer)
				{
					allBrain.Info.TaxEnforcer = false;
					Follower follower = FollowerManager.FindFollowerByID(allBrain.Info.ID);
					if ((bool)follower)
					{
						follower.SetHat(HatType.None);
					}
				}
			}
			GameManager.GetInstance().StartCoroutine(ContinueRitual());
			GameManager.GetInstance().StartCoroutine(SetUpCombatant1Routine());
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnShow = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnShow, (Action)delegate
		{
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnCancel, (Action)delegate
		{
			AudioManager.Instance.StopLoop(loopedSound);
			Interaction_TempleAltar.Instance.RitualCloseSetCamera.Reset();
			GameManager.GetInstance().StartCoroutine(EndRitual());
			CompleteRitual(true);
			CancelFollowers();
			Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController4 = followerSelectInstance;
		uIFollowerSelectMenuController4.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController4.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
	}

	private IEnumerator ContinueRitual()
	{
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(base.transform.position, contestant1.transform.position);
		yield return new WaitForSeconds(1.5f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		yield return new WaitForSeconds(5f / 6f);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position - Vector3.back);
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(item.CurrentTask as FollowerTask_AttendRitual).Pray(2);
			}
		}
		yield return new WaitForSeconds(0.5f);
		waiting = true;
		contestant1.GoTo(ChurchFollowerManager.Instance.RitualCenterPosition.position, delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		contestant1.SetBodyAnimation("Conversations/react-love3", false);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(contestant1.transform.position + Vector3.back);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short");
		contestant1.Brain.Info.TaxEnforcer = true;
		contestant1.SetOutfit(contestant1.Outfit.CurrentOutfit, false);
		yield return new WaitForSeconds(2f);
		for (int i = 0; i < 2; i++)
		{
			waiting = true;
			contestant1.GoTo(ChurchFollowerManager.Instance.RitualCenterPosition.position + (Vector3)UnityEngine.Random.insideUnitCircle * 1.85f, delegate
			{
				waiting = false;
			});
			while (waiting)
			{
				yield return null;
			}
			string animName = "Reactions/react-determined1";
			switch (UnityEngine.Random.Range(0, 3))
			{
			case 0:
				animName = "Reactions/react-determined2";
				break;
			case 1:
				animName = "Reactions/react-non-believers";
				break;
			}
			contestant1.SetBodyAnimation(animName, false);
			yield return new WaitForSeconds(1f);
		}
		waiting = true;
		contestant1.GoTo(ChurchFollowerManager.Instance.RitualCenterPosition.position, delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		contestant1.AddBodyAnimation("tax-enforcer", false, 0f);
		yield return new WaitForSeconds(1.5f);
		contestant1.AddBodyAnimation("Conversations/react-nice3", false, 0f);
		contestant1.AddBodyAnimation("idle", true, 0f);
		count = 0;
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			if (item2 != contestant1.Brain)
			{
				AudioManager.Instance.PlayOneShot("event:/rituals/coins", item2.LastPosition);
				ResourceCustomTarget.Create(contestant1.gameObject, item2.LastPosition, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
				(item2.CurrentTask as FollowerTask_AttendRitual).Pray(2);
				count++;
				yield return new WaitForSeconds(0.1f);
			}
			yield return null;
		}
		contestant1.Brain._directInfoAccess.TaxCollected += count;
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			item3.CompleteCurrentTask();
			float delay = UnityEngine.Random.Range(0.1f, 0.5f);
			StartCoroutine(DelayFollowerReaction(item3, delay));
			Follower follower = FollowerManager.FindFollowerByID(item3.Info.ID);
			if ((bool)follower)
			{
				follower.Spine.randomOffset = false;
			}
		}
		AudioManager.Instance.StopLoop(loopedSound);
		yield return new WaitForSeconds(0.5f);
		ChurchFollowerManager.Instance.AddBrainToAudience(contestant1.Brain);
		CompleteRitual(false, contestant1.Brain.Info.ID);
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_TaxEnforcer, -1, 1f);
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
		Task1.GoToAndStop(contestant1, ChurchFollowerManager.Instance.RitualCenterPosition.position, delegate
		{
			contestant1.FacePosition(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
			contestant1.SetBodyAnimation("idle", true);
			Waiting = false;
		});
	}

	private IEnumerator EndRitual()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		yield return new WaitForSeconds(1f);
		Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
