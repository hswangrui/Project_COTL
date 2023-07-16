using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using Spine;
using src.Extensions;
using UnityEngine;

public class RitualHeartsOfTheFaithful : Ritual
{
	public Light ritualLight;

	private Follower sacrificeFollower;

	private int NumGivingDevotion;

	private UIHeartsOfTheFaithfulChoiceMenuController _heartsOfTheFaithfulMenuTemplate;

	public void Play(UIHeartsOfTheFaithfulChoiceMenuController heartsOfTheFaithfulMenuTemplate)
	{
		_heartsOfTheFaithfulMenuTemplate = heartsOfTheFaithfulMenuTemplate;
		Play();
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
		UIHeartsOfTheFaithfulChoiceMenuController.Types chosenUpgrade = UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts;
		UIHeartsOfTheFaithfulChoiceMenuController heartsOfTheFaithfulMenuInstance = _heartsOfTheFaithfulMenuTemplate.Instantiate();
		heartsOfTheFaithfulMenuInstance.Show();
		UIHeartsOfTheFaithfulChoiceMenuController uIHeartsOfTheFaithfulChoiceMenuController = heartsOfTheFaithfulMenuInstance;
		uIHeartsOfTheFaithfulChoiceMenuController.OnChoiceMade = (Action<UIHeartsOfTheFaithfulChoiceMenuController.Types>)Delegate.Combine(uIHeartsOfTheFaithfulChoiceMenuController.OnChoiceMade, (Action<UIHeartsOfTheFaithfulChoiceMenuController.Types>)delegate(UIHeartsOfTheFaithfulChoiceMenuController.Types type)
		{
			chosenUpgrade = type;
		});
		UIHeartsOfTheFaithfulChoiceMenuController uIHeartsOfTheFaithfulChoiceMenuController2 = heartsOfTheFaithfulMenuInstance;
		uIHeartsOfTheFaithfulChoiceMenuController2.OnHidden = (Action)Delegate.Combine(uIHeartsOfTheFaithfulChoiceMenuController2.OnHidden, (Action)delegate
		{
			heartsOfTheFaithfulMenuInstance = null;
		});
		while (heartsOfTheFaithfulMenuInstance != null)
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
		yield return StartCoroutine(EmitParticles(chosenUpgrade));
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
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PerformRitual);
		yield return new WaitForSeconds(2f);
		yield return new WaitForSeconds(0.5f);
		switch (chosenUpgrade)
		{
		case UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts:
		{
			HealthPlayer healthPlayer = UnityEngine.Object.FindObjectOfType<HealthPlayer>();
			healthPlayer.totalHP += 1f;
			healthPlayer.HP = healthPlayer.totalHP;
			DataManager.Instance.PLAYER_HEARTS_LEVEL++;
			break;
		}
		case UIHeartsOfTheFaithfulChoiceMenuController.Types.Strength:
			DataManager.Instance.PLAYER_DAMAGE_LEVEL++;
			break;
		}
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
