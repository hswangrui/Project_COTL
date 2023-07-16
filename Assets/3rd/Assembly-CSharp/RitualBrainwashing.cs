using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RitualBrainwashing : Ritual
{
	public static Action OnBrainwashingRitualBegan;

	private Tween punchTween;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Brainwashing;
		}
	}

	public override void Play()
	{
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon(true))
		{
			if (item.CurrentTask != null && (item.CurrentTask is FollowerTask_AttendRitual || item.CurrentTask is FollowerTask_AttendTeaching))
			{
				item.CurrentTask.Abort();
			}
		}
		Interaction_TempleAltar.Instance.FrontWall.SetActive(false);
		GameManager.GetInstance().StartCoroutine(RitualRoutine());
	}

	private IEnumerator RitualRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position + Vector3.up, ChurchFollowerManager.Instance.RitualCenterPosition.gameObject, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("idle", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position + Vector3.up, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		});
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		yield return StartCoroutine(WaitFollowersFormCircle(true));
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6.5f, 1f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(1.2f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		List<FollowerBrain> followers2 = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon(true));
		yield return new WaitForSeconds(1f);
		ChurchFollowerManager.Instance.Mushrooms.SetActive(true);
		ChurchFollowerManager.Instance.Mushrooms.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f);
		foreach (FollowerBrain item in followers2)
		{
			if (item.CurrentTask is FollowerTask_AttendRitual)
			{
				((FollowerTask_AttendRitual)item.CurrentTask).Pray3();
			}
		}
		yield return new WaitForSeconds(1f);
		float num = 0f;
		foreach (FollowerBrain item2 in Ritual.GetFollowersAvailableToAttendSermon())
		{
			StartCoroutine(GiveShrooms(FollowerManager.FindFollowerByID(item2.Info.ID), 5f, num));
			num += 0.1f;
		}
		yield return new WaitForSeconds(1f);
		BiomeConstants.Instance.PsychedelicFadeIn(5f);
		AudioManager.Instance.SetMusicPsychedelic(1f);
		yield return new WaitForSeconds(4f);
		ChurchFollowerManager.Instance.Mushrooms.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(delegate
		{
			ChurchFollowerManager.Instance.Mushrooms.SetActive(false);
			ChurchFollowerManager.Instance.Mushrooms.transform.localScale = Vector3.one;
		});
		yield return new WaitForSeconds(1f);
		Transform obj = GameManager.GetInstance().CamFollowTarget.transform;
		GameManager.GetInstance().CamFollowTarget.enabled = false;
		Vector3 position = obj.position;
		Vector3 forward = obj.forward;
		obj.position = position;
		GameManager.GetInstance().CamFollowTarget.enabled = true;
		followers2 = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
		foreach (FollowerBrain item3 in followers2)
		{
			if (item3.CurrentTask is FollowerTask_AttendRitual)
			{
				Follower follower = FollowerManager.FindFollowerByID(item3.Info.ID);
				if ((bool)follower)
				{
					follower.SetFaceAnimation("Emotions/emotion-brainwashed", true);
					follower.SetOutfit(follower.Brain.Info.Outfit, true);
					((FollowerTask_AttendRitual)item3.CurrentTask).Idle();
				}
			}
		}
		yield return new WaitForSeconds(1f);
		foreach (FollowerBrain item4 in followers2)
		{
			if (item4.CurrentTask is FollowerTask_AttendRitual)
			{
				((FollowerTask_AttendRitual)item4.CurrentTask).DanceBrainwashed();
			}
		}
		yield return new WaitForSeconds(3f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		foreach (FollowerBrain item5 in Ritual.FollowerToAttendSermon)
		{
			if (item5.CurrentTask is FollowerTask_AttendRitual)
			{
				(item5.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		float num2 = 1.5f;
		foreach (FollowerBrain item6 in Ritual.FollowerToAttendSermon)
		{
			float num3 = UnityEngine.Random.Range(0.1f, 0.5f);
			num2 += num3;
			StartCoroutine(DelayFollowerReaction(item6, num3));
		}
		BiomeConstants.Instance.PsychedelicFadeOut(1.5f);
		AudioManager.Instance.SetMusicPsychedelic(0f);
		yield return new WaitForSeconds(1.5f);
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		foreach (FollowerBrain item7 in followers2)
		{
			item7.AddThought(Thought.Brainwashed);
			if (item7.HasTrait(FollowerTrait.TraitType.MushroomEncouraged))
			{
				item7.AddThought(Thought.FollowerBrainwashedSubstanceEncouraged);
			}
			else if (item7.HasTrait(FollowerTrait.TraitType.MushroomBanned))
			{
				item7.AddThought(Thought.FollowerBrainwashedSubstanceBanned);
				if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
				{
					item7.MakeSick();
				}
			}
			else
			{
				item7.AddThought(Thought.FollowerBrainwashed);
			}
		}
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.MushroomEncouraged))
		{
			CultFaithManager.AddThought(Thought.Cult_MushroomEncouraged_Trait, -1, 1f);
		}
		else
		{
			CultFaithManager.AddThought(Thought.Brainwashed, -1, 1f);
		}
		DataManager.Instance.LastBrainwashed = TimeManager.TotalElapsedGameTime;
		if (!DataManager.Instance.PerformedMushroomRitual)
		{
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitSozo", Objectives.CustomQuestTypes.SozoReturn));
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SozoPerformRitual);
		}
		DataManager.Instance.PerformedMushroomRitual = true;
		Action onBrainwashingRitualBegan = OnBrainwashingRitualBegan;
		if (onBrainwashingRitualBegan != null)
		{
			onBrainwashingRitualBegan();
		}
	}

	private IEnumerator GiveShrooms(Follower follower, float totalTime, float delay)
	{
		if (follower == null)
		{
			yield break;
		}
		yield return new WaitForSeconds(delay);
		int randomCoins = UnityEngine.Random.Range(3, 7);
		float increment = (totalTime - delay) / (float)randomCoins;
		for (int i = 0; i < randomCoins; i++)
		{
			if (punchTween == null || !punchTween.active)
			{
				punchTween = ChurchFollowerManager.Instance.Mushrooms.transform.DOPunchScale(Vector3.one * 0.1f, increment - 0.05f);
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", ChurchFollowerManager.Instance.Mushrooms.transform.position);
			ResourceCustomTarget.Create(follower.gameObject, ChurchFollowerManager.Instance.Mushrooms.transform.position, InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, null);
			yield return new WaitForSeconds(increment);
		}
	}
}
