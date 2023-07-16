using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RitualWorkHoliday : Ritual
{
	public static Action OnHolidayBegan;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Holiday;
		}
	}

	public override void Play()
	{
		base.Play();
		GameManager.GetInstance().StartCoroutine(RitualRoutine());
	}

	private IEnumerator RitualRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position, null, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("idle", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		});
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		yield return StartCoroutine(WaitFollowersFormCircle());
		AudioManager.Instance.SetFollowersDance(1f);
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.StartRitualOverlay();
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6.5f, 1f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(1.2f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		List<FollowerBrain> list = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
		for (int i = 0; i < 2; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			FollowerBrain followerBrain = list[UnityEngine.Random.Range(0, list.Count)];
			list.Remove(followerBrain);
			Follower follower = FollowerManager.FindFollowerByID(followerBrain.Info.ID);
			if ((bool)follower)
			{
				StartCoroutine(MoveFollower(follower, i));
			}
		}
		foreach (FollowerBrain item in list)
		{
			if (item.CurrentTask is FollowerTask_AttendRitual)
			{
				((FollowerTask_AttendRitual)item.CurrentTask).Dance();
				((FollowerTask_AttendRitual)item.CurrentTask).SpecialOutfit = FollowerOutfitType.Holiday;
			}
		}
		yield return new WaitForSeconds(6f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			if (item2.CurrentTask is FollowerTask_AttendRitual)
			{
				(item2.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.SetFollowersDance(0f);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		float num = 0f;
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			float num2 = UnityEngine.Random.Range(0.1f, 0.5f);
			num += num2;
			StartCoroutine(DelayFollowerReaction(item3, num2));
			item3.AddThought(Thought.Holiday);
		}
		yield return new WaitForSeconds(1.5f);
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		DataManager.Instance.LastHolidayDeclared = TimeManager.TotalElapsedGameTime;
		Action onHolidayBegan = OnHolidayBegan;
		if (onHolidayBegan != null)
		{
			onHolidayBegan();
		}
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_Holiday, -1, 1f);
	}

	private IEnumerator MoveFollower(Follower follower, int index)
	{
		List<Vector3> positions = new List<Vector3>
		{
			Vector3.left,
			Vector3.right
		};
		bool waiting = true;
		follower.HoodOff("idle", false, delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		yield return StartCoroutine(follower.GoToRoutine(PlayerFarming.Instance.transform.position + positions[index] * 0.75f));
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, PlayerFarming.Instance.transform.position);
		follower.SetBodyAnimation("dance", true);
		if (follower.Brain.Info.CursedState != Thought.OldAge)
		{
			follower.Outfit.SetOutfit(follower.Spine, FollowerOutfitType.Holiday, follower.Brain.Info.Necklace, false);
		}
	}
}
