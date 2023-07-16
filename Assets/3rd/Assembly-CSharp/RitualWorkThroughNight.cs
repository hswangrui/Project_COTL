using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine;
using UnityEngine;

public class RitualWorkThroughNight : Ritual
{
	public static Action OnWorkThroughNightBegan;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_WorkThroughNight;
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
		for (int i = 0; i < 4; i++)
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
				(item.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		yield return new WaitForSeconds(7f);
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
			item3.Stats.Rest = 100f;
			item3.AddThought(Thought.WorkThroughNight);
			Follower follower2 = FollowerManager.FindFollowerByID(item3.Info.ID);
			if (follower2 != null)
			{
				follower2.Spine.AnimationState.Event -= HandleAnimationStateEvent;
			}
		}
		yield return new WaitForSeconds(1.5f);
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		DataManager.Instance.LastWorkThroughTheNight = TimeManager.TotalElapsedGameTime;
		Action onWorkThroughNightBegan = OnWorkThroughNightBegan;
		if (onWorkThroughNightBegan != null)
		{
			onWorkThroughNightBegan();
		}
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_WorkThroughNight, -1, 1f);
	}

	private IEnumerator MoveFollower(Follower follower, int index)
	{
		List<Vector3> positions = new List<Vector3>
		{
			Vector3.left,
			Vector3.up,
			Vector3.right,
			Vector3.down
		};
		string[] anims = new string[3] { "build", "dig", "mining" };
		bool waiting = true;
		follower.HoodOff("idle", false, delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		yield return StartCoroutine(follower.GoToRoutine(ChurchFollowerManager.Instance.RitualCenterPosition.position + positions[index]));
		int num = (int)Mathf.Repeat(index, 3f);
		follower.SetBodyAnimation(anims[num], true);
		follower.Spine.AnimationState.Event += HandleAnimationStateEvent;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string text = e.Data.Name;
		if (!(text == "Chop"))
		{
			bool flag = text == "Build";
		}
		else
		{
			AudioManager.Instance.PlayOneShot(SoundConstants.GetImpactSoundPathForMaterial(SoundConstants.SoundMaterial.Stone), PlayerFarming.Instance.transform.position);
		}
	}
}
