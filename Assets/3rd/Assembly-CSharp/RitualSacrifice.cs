using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MMTools;
using Spine;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

public class RitualSacrifice : Ritual
{
	private Follower sacrificeFollower;

	public Light RitualLight;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Sacrifice;
		}
	}

	public override void Play()
	{
		base.Play();
		StartCoroutine(SacrificeFollowerRoutine());
	}

	private IEnumerator SacrificeFollowerRoutine()
	{
		yield return StartCoroutine(CentreAndAnimatePlayer());
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		Debug.Log("Ritual sacrifice begin gather");
		yield return StartCoroutine(WaitFollowersFormCircle());
		Debug.Log("Ritual sacrifice end gather");
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		bool Cancelled = false;
		UIFollowerSelectMenuController followerSelectInstance = null;
		if (!GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
		{
			followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		}
		else
		{
			followerSelectInstance = MonoSingleton<UIManager>.Instance.SacrificeFollowerMenuTemplate.Instantiate();
		}
	//	followerSelectInstance.VotingType = TwitchVoting.VotingType.RITUAL_SACRIFICE;
		followerSelectInstance.Show(Ritual.GetFollowersAvailableToAttendSermon(), null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			sacrificeFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
			UIManager.PlayAudio("event:/ritual_sacrifice/ritual_begin");
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnShow = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnShow, (Action)delegate
		{
			foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
			{
				if (followerInfoBox.followBrain.Info.SacrificialType == InventoryItem.ITEM_TYPE.NONE)
				{
					followerInfoBox.followBrain.Info.SacrificialType = InventoryItem.ITEM_TYPE.BLACK_GOLD;
				}
				if (!GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
				{
					if (followerInfoBox.followBrain.Info.SacrificialValue > 0)
					{
						followerInfoBox.ShowRewardItem(followerInfoBox.followBrain.Info.SacrificialType, followerInfoBox.followBrain.Info.SacrificialValue);
					}
					else
					{
						followerInfoBox.ShowCostItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, followerInfoBox.followBrain.Info.SacrificialValue);
					}
				}
				else
				{
					followerInfoBox.ShowDevotionGain(GetDevotionGain(followerInfoBox.followBrain.Info.XPLevel));
				}
			}
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().StartCoroutine(EndRitual());
			Cancelled = true;
			CompleteRitual(true);
			CancelFollowers();
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController4 = followerSelectInstance;
		uIFollowerSelectMenuController4.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController4.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
		while (followerSelectInstance != null && !Cancelled)
		{
			yield return null;
		}
		if (Cancelled)
		{
			yield break;
		}
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
		sacrificeFollower.Brain.CompleteCurrentTask();
		FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
		sacrificeFollower.Brain.HardSwapToTask(nextTask);
		sacrificeFollower.Brain.InRitual = true;
		yield return null;
		sacrificeFollower.SetOutfit(sacrificeFollower.Brain.Info.Outfit, true);
		sacrificeFollower.HoodOff("idle", false, delegate
		{
			ChurchFollowerManager.Instance.RemoveBrainFromAudience(sacrificeFollower.Brain);
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
				{
					allBrain.CurrentTask.RecalculateDestination();
					allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
				}
			}
			sacrificeFollower.Spine.AnimationState.SetAnimation(1, "walk", true);
			sacrificeFollower.gameObject.transform.DOMove(Interaction_TempleAltar.Instance.PortalEffect.transform.position, 2.5f).OnComplete(delegate
			{
				Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
				GameManager.GetInstance().StartCoroutine(DoSacrificeRoutine());
			});
		});
		GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject);
	}

	public static int GetDevotionGain(int XPLevel)
	{
		return Mathf.Clamp(40 + (XPLevel - 1) * 20, 30, 100);
	}

	private IEnumerator DoSacrificeRoutine()
	{
		if (RitualLight != null)
		{
			RitualLight.gameObject.SetActive(true);
			RitualLight.color = Color.black;
			RitualLight.DOColor(Color.red, 1f);
			RitualLight.DOIntensity(1.5f, 1f);
		}
		bool isSecret = (sacrificeFollower.Brain.Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Light && !DataManager.Instance.HasBaalSkin) || (sacrificeFollower.Brain.Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Dark && !DataManager.Instance.HasAymSkin);
		GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject, 4f);
		ChurchFollowerManager.Instance.PlayOverlay(ChurchFollowerManager.OverlayType.Sacrifice, "1");
		sacrificeFollower.Spine.AnimationState.Event += HandleSacrificeAnimationStateEvent;
		sacrificeFollower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		if (!isSecret)
		{
			Ritual.FollowerToAttendSermon.Remove(sacrificeFollower.Brain);
		}
		if (isSecret)
		{
			sacrificeFollower.SetBodyAnimation("sacrifice-tentacles-secret", false);
			sacrificeFollower.AddBodyAnimation("Forneus/scared", true, 0f);
		}
		else if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SacrificeEnthusiast) && sacrificeFollower.Brain.Info.CursedState != Thought.Dissenter)
		{
			sacrificeFollower.SetBodyAnimation("sacrifice-tentacles", false);
		}
		else
		{
			sacrificeFollower.SetBodyAnimation("sacrifice-tentacles-scared", false);
		}
		int followerID = sacrificeFollower.Brain.Info.ID;
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
		yield return new WaitForSeconds(1.5f);
		foreach (FollowerBrain allBrain2 in FollowerBrain.AllBrains)
		{
			if (allBrain2.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(allBrain2.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		ChurchFollowerManager.Instance.StartRitualOverlay();
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/ritual_end", PlayerFarming.Instance.gameObject);
		yield return new WaitForSeconds(0.7f);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(sacrificeFollower.CameraBone.transform.position);
		yield return new WaitForSeconds(3.1666665f);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
		if (isSecret)
		{
			sacrificeFollower.Outfit.SetOutfit(sacrificeFollower.Spine, sacrificeFollower.Outfit.CurrentOutfit, InventoryItem.ITEM_TYPE.NONE, false);
		}
		yield return new WaitForSeconds(0.3f);
		GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 8f);
		yield return new WaitForSeconds(0.5f);
		if (isSecret)
		{
			yield return new WaitForSeconds(9f);
			GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject, 4f);
			foreach (FollowerBrain allBrain3 in FollowerBrain.AllBrains)
			{
				if (allBrain3.CurrentTaskType == FollowerTaskType.AttendTeaching)
				{
					(allBrain3.CurrentTask as FollowerTask_AttendRitual).Pray();
				}
			}
			yield return new WaitForSeconds(2f);
			List<ConversationEntry> list = new List<ConversationEntry>
			{
				new ConversationEntry(base.gameObject, ""),
				new ConversationEntry(base.gameObject, ""),
				new ConversationEntry(base.gameObject, "")
			};
			for (int i = 0; i < list.Count; i++)
			{
				string translation = LocalizationManager.GetTranslation(string.Format("Conversation_NPC/{0}/Intro/{1}", (sacrificeFollower.Brain.Info.ID == FollowerManager.BaalID) ? "Baal" : "Aym", i));
				list[i].SkeletonData = sacrificeFollower.Spine;
				list[i].TermToSpeak = translation;
				list[i].CharacterName = ((sacrificeFollower.Brain.Info.ID == FollowerManager.BaalID) ? ScriptLocalization.NAMES.Guardian1 : ScriptLocalization.NAMES.Guardian2);
				list[i].soundPath = ((sacrificeFollower.Brain.Info.ID == FollowerManager.BaalID) ? "event:/dialogue/followers/boss/fol_guardian_b" : "event:/dialogue/followers/boss/fol_guardian_a");
				list[i].Speaker = sacrificeFollower.gameObject;
				list[i].SetZoom = true;
				list[i].Zoom = 6f;
			}
			if (sacrificeFollower.Brain.Info.ID == FollowerManager.AymID)
			{
				list.RemoveAt(list.Count - 1);
			}
			bool waiting = true;
			MMConversation.Play(new ConversationObject(list, null, delegate
			{
				waiting = false;
			}), false);
			while (waiting)
			{
				yield return null;
			}
			GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 8f);
		}
		else
		{
			if (!GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
			{
				yield return StartCoroutine(GiveSoulsRoutines(Interaction_TempleAltar.Instance.PortalEffect.transform, sacrificeFollower.Brain.Info.SacrificialValue, followerID));
			}
			else
			{
				DoctrineUpgradeSystem.GetXPBySermon(SermonCategory.PlayerUpgrade);
				float num = DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory.PlayerUpgrade) * ((float)GetDevotionGain(sacrificeFollower.Brain.Info.XPLevel) / 100f);
				num = Mathf.Ceil(num * 10f);
				yield return StartCoroutine(UpgradePlayer(Mathf.RoundToInt(num)));
			}
			yield return new WaitForSeconds(1f);
			GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 6f);
			yield return StartCoroutine(GiveBonesAndMeat(Interaction_TempleAltar.Instance.PortalEffect.transform));
		}
		yield return new WaitForSeconds(0.5f);
		StopSacrificePortalEffect();
		sacrificeFollower.Spine.AnimationState.Event -= HandleSacrificeAnimationStateEvent;
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (sacrificeFollower.Brain.Info.CursedState == Thought.OldAge)
			{
				item.AddThought(Thought.SacrificedOldFollower);
			}
		}
		if (!isSecret)
		{
			FollowerManager.FollowerDie(sacrificeFollower.Brain.Info.ID, NotificationCentre.NotificationType.SacrificeFollower);
			UnityEngine.Object.Destroy(sacrificeFollower.gameObject);
		}
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		float num2 = 0.5f;
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			float num3 = UnityEngine.Random.Range(0.1f, 0.5f);
			num2 += num3;
			StartCoroutine(DelayFollowerReaction(item2, num3));
		}
		yield return new WaitForSeconds(1.5f);
		JudgementMeter.ShowModify(DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SacrificeEnthusiast) ? 1 : (-1));
		yield return new WaitForSeconds(2f);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		if (isSecret)
		{
			ChurchFollowerManager.Instance.AddBrainToAudience(sacrificeFollower.Brain);
		}
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		DataManager.Instance.STATS_Sacrifices++;
		CompleteRitual(false, followerID);
		yield return new WaitForSeconds(1f);
		if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SacrificeEnthusiast))
		{
			CultFaithManager.AddThought(Thought.Cult_Sacrifice_Trait, followerID, 1f);
		}
		else
		{
			CultFaithManager.AddThought(Thought.Cult_Sacrifice, followerID, 1f);
		}
	}

	private IEnumerator EndRitual()
	{
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(2f / 3f);
	}

	private void HandleSacrificeAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		switch (e.Data.Name)
		{
		case "SFX/tentacleSecret":
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/just_tentacles", sacrificeFollower.CameraBone.gameObject);
			break;
		case "SECRET_SKIN_SWAP":
			if (sacrificeFollower.Brain.Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Light && !DataManager.Instance.HasBaalSkin)
			{
				DataManager.Instance.Followers_Dead.Add(sacrificeFollower.Brain._directInfoAccess);
				DataManager.Instance.Followers_Dead_IDs.Add(sacrificeFollower.Brain._directInfoAccess.ID);
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Church);
				followerInfo.Traits = sacrificeFollower.Brain._directInfoAccess.Traits;
				followerInfo.TraitsSet = true;
				followerInfo.ID = FollowerManager.BaalID;
				followerInfo.Name = ScriptLocalization.NAMES.Guardian1;
				followerInfo.SkinName = "Boss Baal";
				followerInfo.Necklace = InventoryItem.ITEM_TYPE.NONE;
				followerInfo.SkinColour = 0;
				followerInfo.SkinVariation = 0;
				for (int i = 0; i < WorshipperData.Instance.Characters.Count; i++)
				{
					if (WorshipperData.Instance.Characters[i].Title == "Boss Baal")
					{
						followerInfo.SkinCharacter = i;
						break;
					}
				}
				sacrificeFollower.Brain._directInfoAccess = followerInfo;
				sacrificeFollower.Brain.Info = new FollowerBrainInfo(followerInfo, sacrificeFollower.Brain);
				DataManager.Instance.HasBaalSkin = true;
			}
			else if (sacrificeFollower.Brain.Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Dark && !DataManager.Instance.HasAymSkin)
			{
				DataManager.Instance.Followers_Dead.Add(sacrificeFollower.Brain._directInfoAccess);
				DataManager.Instance.Followers_Dead_IDs.Add(sacrificeFollower.Brain._directInfoAccess.ID);
				FollowerInfo followerInfo2 = FollowerInfo.NewCharacter(FollowerLocation.Church);
				followerInfo2.Traits = sacrificeFollower.Brain._directInfoAccess.Traits;
				followerInfo2.TraitsSet = true;
				followerInfo2.ID = FollowerManager.AymID;
				followerInfo2.Name = ScriptLocalization.NAMES.Guardian2;
				followerInfo2.SkinName = "Boss Aym";
				followerInfo2.Necklace = InventoryItem.ITEM_TYPE.NONE;
				followerInfo2.SkinColour = 0;
				followerInfo2.SkinVariation = 0;
				for (int j = 0; j < WorshipperData.Instance.Characters.Count; j++)
				{
					if (WorshipperData.Instance.Characters[j].Title == "Boss Aym")
					{
						followerInfo2.SkinCharacter = j;
						break;
					}
				}
				sacrificeFollower.Brain._directInfoAccess = followerInfo2;
				sacrificeFollower.Brain.Info = new FollowerBrainInfo(followerInfo2, sacrificeFollower.Brain);
				DataManager.Instance.HasAymSkin = true;
			}
			DataManager.Instance.Followers.Add(sacrificeFollower.Brain._directInfoAccess);
			sacrificeFollower.Spine.Skeleton.SetSkin(sacrificeFollower.Brain.Info.SkinName);
			foreach (WorshipperData.SlotAndColor slotAndColour in WorshipperData.Instance.GetColourData("Boss Aym").SlotAndColours[0].SlotAndColours)
			{
				Slot slot = sacrificeFollower.Spine.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
			sacrificeFollower.Outfit.SetInfo(sacrificeFollower.Brain._directInfoAccess);
			break;
		case "Shake-small":
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			GameManager.GetInstance().OnConversationNext(sacrificeFollower.CameraBone, 6f);
			Interaction_TempleAltar.Instance.PulseDisplacementObject(sacrificeFollower.CameraBone.transform.position);
			break;
		case "Shake-big":
			CameraManager.instance.ShakeCameraForDuration(0.6f, 0.7f, 0.6f);
			GameManager.GetInstance().OnConversationNext(sacrificeFollower.CameraBone, 8f);
			Interaction_TempleAltar.Instance.PulseDisplacementObject(sacrificeFollower.CameraBone.transform.position);
			BiomeConstants.Instance.ImpactFrameForDuration();
			break;
		case "CamOffset-Add":
			GameManager.GetInstance().CamFollowTarget.SetOffset(new Vector3(0f, 0f, 1f));
			BiomeConstants.Instance.DepthOfFieldTween(0.5f, 7f, 8f, 1f, 150f);
			BiomeConstants.Instance.chromaticAbberration.intensity.value = 1f;
			break;
		case "CamOffset-Remove":
			GameManager.GetInstance().CamFollowTarget.SetOffset(Vector3.zero);
			BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
			BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
			break;
		}
	}

	private IEnumerator GiveSoulsRoutines(Transform sacrificeTransform, int SacrificialValue, int followerID)
	{
		Interaction_TempleAltar.Instance.PortalEffect.AnimationState.SetAnimation(0, "pulse", true);
		float SoulsToGive = SacrificialValue;
		float increment = 2f / SoulsToGive;
		while (true)
		{
			float num = SoulsToGive - 1f;
			SoulsToGive = num;
			if (!(num >= 0f))
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", sacrificeTransform.position);
			InventoryItem.ITEM_TYPE sacrificialType = sacrificeFollower.Brain.Info.SacrificialType;
			if (sacrificialType == InventoryItem.ITEM_TYPE.SOUL)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.CameraBone, sacrificeTransform.position, Color.white, delegate
				{
					PlayerFarming.Instance.GetXP(1f);
				});
			}
			else
			{
				ResourceCustomTarget.Create(PlayerFarming.Instance.CameraBone, sacrificeTransform.position, sacrificeFollower.Brain.Info.SacrificialType, delegate
				{
					Inventory.AddItem((int)sacrificeFollower.Brain.Info.SacrificialType, 1);
				});
			}
			CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.2f);
			yield return new WaitForSeconds(increment);
		}
	}

	private IEnumerator GiveBonesAndMeat(Transform sacrificeTransform)
	{
		int BonesAndMeat = UnityEngine.Random.Range(3, 5);
		for (int i = 0; i < BonesAndMeat; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", sacrificeTransform.position);
			ResourceCustomTarget.Create(PlayerFarming.Instance.CameraBone, sacrificeTransform.position, InventoryItem.ITEM_TYPE.BONE, delegate
			{
				Inventory.AddItem(9, 1);
			});
			yield return new WaitForSeconds(0.1f);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", sacrificeTransform.position);
			ResourceCustomTarget.Create(PlayerFarming.Instance.CameraBone, sacrificeTransform.position, InventoryItem.ITEM_TYPE.FOLLOWER_MEAT, delegate
			{
				Inventory.AddItem(62, 1);
			});
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator UpgradePlayer(int PerCentGain)
	{
		SermonController sermonController = UnityEngine.Object.FindObjectOfType<SermonController>();
		yield return StartCoroutine(sermonController.SacrificeLevelUp(PerCentGain, StopCheering));
	}

	private void StopCheering()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
			{
				(allBrain.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
			}
		}
	}
}
