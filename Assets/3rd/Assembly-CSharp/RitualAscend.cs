using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using UnityEngine;

public class RitualAscend : Ritual
{
	private Follower contestant1;

	private FollowerTask_ManualControl Task1;

	private bool Waiting = true;

	private EventInstance loopedSound;

	private List<PickUp> Pickups = new List<PickUp>();

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Ascend;
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
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
	//	followerSelectInstance.VotingType = TwitchVoting.VotingType.RITUAL_ASCEND;
		List<FollowerBrain> followerBrains = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
		followerSelectInstance.Show(followerBrains, null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			contestant1 = FollowerManager.FindFollowerByID(followerInfo.ID);
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
			loopedSound = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
			Task1 = new FollowerTask_ManualControl();
			contestant1.Brain.HardSwapToTask(Task1);
			GameManager.GetInstance().StartCoroutine(ContinueRitual());
			StartCoroutine(SetUpCombatant1Routine());
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnShow = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnShow, (Action)delegate
		{
			foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
			{
				followerInfoBox.ShowFaithGain(GetAdorationRewardAmount(followerInfoBox.followBrain), followerInfoBox.followBrain.Stats.MAX_ADORATION);
			}
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnCancel, (Action)delegate
		{
			AudioManager.Instance.StopLoop(loopedSound);
			EndRitual();
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
		bool desensitisedToDeath = DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.DesensitisedToDeath);
		bool sacrificeEnthusiast = DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SacrificeEnthusiast);
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
		ChurchFollowerManager.Instance.GodRays.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.GodRays.GetComponent<ParticleSystem>().Play();
		ChurchFollowerManager.Instance.Goop.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.Goop.Play("Show");
		if (desensitisedToDeath && !sacrificeEnthusiast)
		{
			ChurchFollowerManager.Instance.Goop.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintCOlor", Color.white);
		}
		else
		{
			ChurchFollowerManager.Instance.Goop.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintCOlor", Color.red);
		}
		yield return new WaitForSeconds(0.5f);
		if (desensitisedToDeath && !sacrificeEnthusiast)
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/ritual_ascension_pg", PlayerFarming.Instance.gameObject);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/ritual_ascension", PlayerFarming.Instance.gameObject);
		}
		contestant1.SetBodyAnimation("ascend", false);
		Interaction_TempleAltar.Instance.CloseUpCamera.gameObject.SetActive(false);
		yield return new WaitForSeconds(5.6666665f);
		if (desensitisedToDeath)
		{
			Vector3 position2 = contestant1.transform.position;
			AudioManager.Instance.PlayOneShot("event:/dialogue/followers/loved", position2);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/enemy_charmed", position2);
			ChurchFollowerManager.Instance.Sparkles.gameObject.SetActive(true);
			ChurchFollowerManager.Instance.Sparkles.Play();
			for (int j = 0; j < 10; j++)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, contestant1.transform.position + Vector3.back * 5f, Color.white, null);
				yield return new WaitForSeconds(0.1f);
			}
			yield return new WaitForSeconds(1f);
		}
		if (sacrificeEnthusiast || (!desensitisedToDeath && !sacrificeEnthusiast))
		{
			Vector3 position = contestant1.transform.position;
			AudioManager.Instance.PlayOneShot("event:/dialogue/followers/scared_short", position);
			AudioManager.Instance.PlayOneShot("event:/player/harvest_meat", position);
			CameraManager.instance.ShakeCameraForDuration(0f, 1f, 2f);
			MMVibrate.RumbleContinuous(0f, 1f);
			yield return new WaitForSeconds(1f);
			for (int j = 0; j < 3; j++)
			{
				PickUp component = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 1, contestant1.transform.position + Vector3.back * 5f).GetComponent<PickUp>();
				Pickups.Add(component);
				component.enabled = false;
				component.child.transform.localScale = Vector3.one;
				Vector3 vector = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
				component.gameObject.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.transform.position + vector, 1f).SetEase(Ease.OutBounce).SetUpdate(true);
				yield return new WaitForSeconds(0.25f);
				component = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, 1, contestant1.transform.position + Vector3.back * 5f).GetComponent<PickUp>();
				Pickups.Add(component);
				vector = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
				component.enabled = false;
				component.child.transform.localScale = Vector3.one;
				component.gameObject.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.transform.position + vector, 1f).SetEase(Ease.OutBounce).SetUpdate(true);
				yield return new WaitForSeconds(0.25f);
			}
			AudioManager.Instance.PlayOneShot("event:/player/harvest_meat_done", position);
			PickUp component2 = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, 1, contestant1.transform.position + Vector3.back * 5f).GetComponent<PickUp>();
			Pickups.Add(component2);
			component2.enabled = false;
			component2.child.transform.localScale = Vector3.one;
			Vector3 vector2 = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
			component2.gameObject.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.transform.position + vector2, 1f).SetEase(Ease.OutBounce).SetUpdate(true);
			yield return new WaitForSeconds(0.2f);
			component2 = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, 1, contestant1.transform.position + Vector3.back * 5f).GetComponent<PickUp>();
			Pickups.Add(component2);
			component2.child.transform.localScale = Vector3.one;
			vector2 = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
			component2.enabled = false;
			component2.gameObject.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.transform.position + vector2, 1f).SetEase(Ease.OutBounce).SetUpdate(true);
		}
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		MMVibrate.StopRumble();
		ChurchFollowerManager.Instance.GodRays.GetComponent<ParticleSystem>().Stop();
		ChurchFollowerManager.Instance.Goop.Play("Hide");
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		int id = contestant1.Brain.Info.ID;
		Ritual.FollowerToAttendSermon.Remove(contestant1.Brain);
		contestant1.Die(NotificationCentre.NotificationType.Ascended, false, 1, "dead", null, true);
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			float delay = UnityEngine.Random.Range(0.1f, 0.5f);
			StartCoroutine(DelayFollowerReaction(item2, delay));
			Follower follower = FollowerManager.FindFollowerByID(item2.Info.ID);
			if ((bool)follower)
			{
				follower.Spine.randomOffset = false;
			}
		}
		AudioManager.Instance.StopLoop(loopedSound);
		FollowerBrain.AdorationActions adorationReward = GetAdorationReward(contestant1.Brain);
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			if (item3 != contestant1.Brain)
			{
				item3.AddAdoration(adorationReward, null);
			}
		}
		yield return new WaitForSeconds(1.5f);
		ChurchFollowerManager.Instance.Goop.gameObject.SetActive(false);
		ChurchFollowerManager.Instance.GodRays.SetActive(false);
		ChurchFollowerManager.Instance.Sparkles.Stop();
		foreach (PickUp pickup in Pickups)
		{
			pickup.enabled = true;
		}
		Pickups.Clear();
		JudgementMeter.ShowModify(1);
		yield return new WaitForSeconds(1.5f);
		ChurchFollowerManager.Instance.Sparkles.gameObject.SetActive(false);
		CompleteRitual(false, id);
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_Ascend, id, 1f);
	}

	private FollowerBrain.AdorationActions GetAdorationReward(FollowerBrain brain)
	{
		if (brain.Info.XPLevel == 2)
		{
			return FollowerBrain.AdorationActions.AscendedFollower_Lvl2;
		}
		if (brain.Info.XPLevel == 3)
		{
			return FollowerBrain.AdorationActions.AscendedFollower_Lvl3;
		}
		if (brain.Info.XPLevel == 4)
		{
			return FollowerBrain.AdorationActions.AscendedFollower_Lvl4;
		}
		if (brain.Info.XPLevel >= 5)
		{
			return FollowerBrain.AdorationActions.AscendedFollower_Lvl5;
		}
		return FollowerBrain.AdorationActions.Sermon;
	}

	private int GetAdorationRewardAmount(FollowerBrain brain)
	{
		return FollowerBrain.AdorationsAndActions[GetAdorationReward(brain)];
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
		contestant1.SetOutfit(FollowerOutfitType.HorseTown, false);
		Task1.GoToAndStop(contestant1, ChurchFollowerManager.Instance.RitualCenterPosition.position, delegate
		{
			contestant1.FacePosition(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
			contestant1.SetBodyAnimation("idle", true);
			Waiting = false;
		});
	}

	private IEnumerator EndRitualIE()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator SpawnSouls(GameObject target, Vector3 fromPosition, float delay)
	{
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < 1; i++)
		{
			SoulCustomTargetLerp.Create(target, fromPosition, 0.5f, Color.white).GetComponent<SoulCustomTargetLerp>().Offset = -Vector3.forward;
			yield return new WaitForSeconds(0.2f);
		}
	}

	private void EndRitual()
	{
		ChurchFollowerManager.Instance.EndRitualOverlay();
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			item.CompleteCurrentTask();
		}
	}
}
