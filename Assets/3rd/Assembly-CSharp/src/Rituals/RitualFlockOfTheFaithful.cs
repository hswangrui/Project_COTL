using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using Spine;
using UnityEngine;

namespace src.Rituals
{
	public class RitualFlockOfTheFaithful : Ritual
	{
		public Light ritualLight;

		private Follower sacrificeFollower;

		private int NumGivingDevotion;

		public override void Play()
		{
			base.Play();
			StartCoroutine(HeartsOfTheFaithfulRitual());
			PlayerFarming.Instance.Spine.AnimationState.Event += HandleEvent;
		}

		private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
		{
			if (e.Data.Name == "sfxTrigger")
			{
				AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/player_rise", PlayerFarming.Instance.gameObject);
				PlayerFarming.Instance.Spine.AnimationState.Event -= HandleEvent;
			}
		}

		private IEnumerator HeartsOfTheFaithfulRitual()
		{
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
			PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position, null, false, false, delegate
			{
				Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
				PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
				PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
			});
			yield return StartCoroutine(WaitFollowersFormCircle());
			yield return new WaitForSeconds(1f);
			UIHeartsOfTheFaithfulChoiceMenuController.Types upgradeType = UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts;
			UIUpgradePlayerTreeMenuController playerUpgradeMenuInstance = MonoSingleton<UIManager>.Instance.ShowPlayerUpgradeTree();
			UIUpgradePlayerTreeMenuController uIUpgradePlayerTreeMenuController = playerUpgradeMenuInstance;
			uIUpgradePlayerTreeMenuController.OnHide = (Action)Delegate.Combine(uIUpgradePlayerTreeMenuController.OnHide, (Action)delegate
			{
			});
			UIUpgradePlayerTreeMenuController uIUpgradePlayerTreeMenuController2 = playerUpgradeMenuInstance;
			uIUpgradePlayerTreeMenuController2.OnUpgradeUnlocked = (Action<UpgradeSystem.Type>)Delegate.Combine(uIUpgradePlayerTreeMenuController2.OnUpgradeUnlocked, (Action<UpgradeSystem.Type>)delegate(UpgradeSystem.Type type)
			{
				upgradeType = GetUpgradeType(type);
			});
			UIUpgradePlayerTreeMenuController uIUpgradePlayerTreeMenuController3 = playerUpgradeMenuInstance;
			uIUpgradePlayerTreeMenuController3.OnHidden = (Action)Delegate.Combine(uIUpgradePlayerTreeMenuController3.OnHidden, (Action)delegate
			{
				playerUpgradeMenuInstance = null;
			});
			while (playerUpgradeMenuInstance != null)
			{
				yield return null;
			}
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 10f);
			PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
			PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
			PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
			PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
			yield return new WaitForSeconds(0.5f);
			ritualLight.gameObject.SetActive(true);
			ChurchFollowerManager.Instance.StartRitualOverlay();
			yield return new WaitForSeconds(0.5f);
			BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
			BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);
			NumGivingDevotion = 0;
			foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
			{
				NumGivingDevotion++;
				if (item.CurrentTask is FollowerTask_AttendRitual)
				{
					(item.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
				}
				Follower follower = FollowerManager.FindFollowerByID(item.Info.ID);
				if (follower != null)
				{
					StartCoroutine(SpawnSouls(follower.transform.position));
				}
				yield return new WaitForSeconds(0.1f);
			}
			DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
			{
				GameManager.GetInstance().CamFollowTarget.targetDistance = x;
			}, 6f, 5f).SetEase(Ease.InOutSine);
			while (NumGivingDevotion > 0)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
			{
				if (item2.CurrentTask is FollowerTask_AttendRitual)
				{
					(item2.CurrentTask as FollowerTask_AttendRitual).Cheer();
				}
			}
			yield return StartCoroutine(EmitParticles(upgradeType));
			yield return new WaitForSeconds(0.5f);
			BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
			BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
			ChurchFollowerManager.Instance.EndRitualOverlay();
			GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			ritualLight.gameObject.SetActive(false);
			float num = 0f;
			foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
			{
				float num2 = UnityEngine.Random.Range(0.1f, 0.5f);
				num += num2;
				StartCoroutine(DelayFollowerReaction(item3, num2));
			}
			yield return new WaitForSeconds(2f);
			if (!DataManager.Instance.OnboardedLoyalty)
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
				DataManager.Instance.OnboardedLoyalty = true;
			}
			yield return new WaitForSeconds(0.5f);
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PerformRitual);
			CompleteRitual();
		}

		private IEnumerator EmitParticles(UIHeartsOfTheFaithfulChoiceMenuController.Types chosenUpgrade)
		{
			int Loops = 1;
			float Delay = 0f;
			while (true)
			{
				int num = Loops - 1;
				Loops = num;
				if (num >= 0)
				{
					Interaction_TempleAltar.Instance.PulseDisplacementObject(PlayerFarming.Instance.transform.position);
					CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.2f);
					GameManager.GetInstance().CamFollowTarget.targetDistance += 0.2f;
					switch (chosenUpgrade)
					{
					case UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts:
						AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", base.gameObject);
						AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
						BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "red", "burst_big");
						break;
					case UIHeartsOfTheFaithfulChoiceMenuController.Types.Strength:
						AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/swords_appear", base.gameObject);
						BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "strength", "strength");
						break;
					}
					Delay += 0.1f;
					yield return new WaitForSeconds(0.8f - Delay);
					continue;
				}
				break;
			}
		}

		public static UIHeartsOfTheFaithfulChoiceMenuController.Types GetUpgradeType(UpgradeSystem.Type type)
		{
			switch (type)
			{
			case UpgradeSystem.Type.PUpgrade_Heart_1:
			case UpgradeSystem.Type.PUpgrade_Heart_2:
			case UpgradeSystem.Type.PUpgrade_Heart_3:
			case UpgradeSystem.Type.PUpgrade_Heart_4:
			case UpgradeSystem.Type.PUpgrade_Heart_5:
			case UpgradeSystem.Type.PUpgrade_Heart_6:
				return UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts;
			case UpgradeSystem.Type.PUpgrade_Sword_0:
			case UpgradeSystem.Type.PUpgrade_Sword_1:
			case UpgradeSystem.Type.PUpgrade_Sword_2:
			case UpgradeSystem.Type.PUpgrade_Sword_3:
			case UpgradeSystem.Type.PUpgrade_Axe_0:
			case UpgradeSystem.Type.PUpgrade_Axe_1:
			case UpgradeSystem.Type.PUpgrade_Axe_2:
			case UpgradeSystem.Type.PUpgrade_Axe_3:
			case UpgradeSystem.Type.PUpgrade_Dagger_0:
			case UpgradeSystem.Type.PUpgrade_Dagger_1:
			case UpgradeSystem.Type.PUpgrade_Dagger_2:
			case UpgradeSystem.Type.PUpgrade_Dagger_3:
			case UpgradeSystem.Type.PUpgrade_Gauntlets_0:
			case UpgradeSystem.Type.PUpgrade_Gauntlets_1:
			case UpgradeSystem.Type.PUpgrade_Gauntlets_2:
			case UpgradeSystem.Type.PUpgrade_Gauntlets_3:
			case UpgradeSystem.Type.PUpgrade_Hammer_0:
			case UpgradeSystem.Type.PUpgrade_Hammer_1:
			case UpgradeSystem.Type.PUpgrade_Hammer_2:
			case UpgradeSystem.Type.PUpgrade_Hammer_3:
				return UIHeartsOfTheFaithfulChoiceMenuController.Types.Strength;
			case UpgradeSystem.Type.PUpgrade_Fireball_0:
			case UpgradeSystem.Type.PUpgrade_Fireball_1:
			case UpgradeSystem.Type.PUpgrade_Fireball_2:
			case UpgradeSystem.Type.PUpgrade_Fireball_3:
			case UpgradeSystem.Type.PUpgrade_EnemyBlast_0:
			case UpgradeSystem.Type.PUpgrade_EnemyBlast_1:
			case UpgradeSystem.Type.PUpgrade_EnemyBlast_2:
			case UpgradeSystem.Type.PUpgrade_EnemyBlast_3:
			case UpgradeSystem.Type.PUpgrade_ProjectileAOE_0:
			case UpgradeSystem.Type.PUpgrade_ProjectileAOE_1:
			case UpgradeSystem.Type.PUpgrade_ProjectileAOE_2:
			case UpgradeSystem.Type.PUpgrade_ProjectileAOE_3:
			case UpgradeSystem.Type.PUpgrade_Tentacles_0:
			case UpgradeSystem.Type.PUpgrade_Tentacles_1:
			case UpgradeSystem.Type.PUpgrade_Tentacles_2:
			case UpgradeSystem.Type.PUpgrade_Tentacles_3:
			case UpgradeSystem.Type.PUpgrade_Vortex_0:
			case UpgradeSystem.Type.PUpgrade_Vortex_1:
			case UpgradeSystem.Type.PUpgrade_Vortex_2:
			case UpgradeSystem.Type.PUpgrade_Vortex_3:
			case UpgradeSystem.Type.PUpgrade_MegaSlash_0:
			case UpgradeSystem.Type.PUpgrade_MegaSlash_1:
			case UpgradeSystem.Type.PUpgrade_MegaSlash_2:
			case UpgradeSystem.Type.PUpgrade_MegaSlash_3:
			case UpgradeSystem.Type.PUpgrade_Ammo_1:
			case UpgradeSystem.Type.PUpgrade_Ammo_2:
			case UpgradeSystem.Type.PUpgrade_Ammo_3:
				return UIHeartsOfTheFaithfulChoiceMenuController.Types.Strength;
			default:
				return UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts;
			}
		}

		private IEnumerator SpawnSouls(Vector3 Position)
		{
			float delay = 0.5f;
			float Count = 12f;
			for (int i = 0; (float)i < Count; i++)
			{
				float num = (float)i / Count;
				SoulCustomTargetLerp.Create(PlayerFarming.Instance.CrownBone.gameObject, Position, 0.5f, Color.red);
				yield return new WaitForSeconds(delay - 0.2f * num);
			}
			NumGivingDevotion--;
		}
	}
}
