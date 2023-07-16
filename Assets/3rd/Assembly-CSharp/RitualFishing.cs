using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class RitualFishing : Ritual
{
	public static Action OnFishingRitualBegan;

	private EventInstance loopedInstance;

	private EventInstance loopedInstance1;

	private Follower Follower1;

	private Follower Follower2;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_FishingRitual;
		}
	}

	public override void Play()
	{
		base.Play();
		GameManager.GetInstance().StartCoroutine(RitualRoutine());
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(loopedInstance);
		AudioManager.Instance.StopLoop(loopedInstance1);
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
		ChurchFollowerManager.Instance.Water.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.Water.transform.localScale = Vector3.zero;
		ChurchFollowerManager.Instance.Water.transform.DOScale(new Vector3(0.5f, 0.5f), 1.5f).SetEase(Ease.OutQuart).SetUpdate(true);
		loopedInstance1 = AudioManager.Instance.CreateLoop("event:/player/watering", ChurchFollowerManager.Instance.Water, true);
		loopedInstance = AudioManager.Instance.CreateLoop("event:/atmos/hub_shore/water_lapping", ChurchFollowerManager.Instance.Water, true);
		AudioManager.Instance.SetEventInstanceParameter(loopedInstance, "intensity", 1f);
		AudioManager.Instance.PlayLoop(loopedInstance);
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
				if (i == 0)
				{
					Follower1 = follower;
				}
				else
				{
					Follower2 = follower;
				}
				StartCoroutine(MoveFollower(follower, i));
			}
		}
		foreach (FollowerBrain item in list)
		{
			if (item.CurrentTask is FollowerTask_AttendRitual)
			{
				((FollowerTask_AttendRitual)item.CurrentTask).Pray2();
			}
		}
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.StopLoop(loopedInstance1);
		yield return new WaitForSeconds(7.5f);
		if ((bool)Follower1)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FISH_BIG, 1, Follower1.transform.position);
		}
		yield return new WaitForSeconds(0.2f);
		if ((bool)Follower2)
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FISH_BIG, 1, Follower2.transform.position);
		}
		AudioManager.Instance.StopLoop(loopedInstance);
		ChurchFollowerManager.Instance.Water.transform.DOScale(new Vector3(0f, 0f), 1f).SetEase(Ease.OutQuart).SetUpdate(true);
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
			item3.AddThought(Thought.FishingRitual);
		}
		yield return new WaitForSeconds(1.5f);
		DataManager.Instance.LastFishingDeclared = TimeManager.TotalElapsedGameTime;
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		Action onFishingRitualBegan = OnFishingRitualBegan;
		if (onFishingRitualBegan != null)
		{
			onFishingRitualBegan();
		}
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_FishingRitual, -1, 1f);
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
		yield return StartCoroutine(follower.GoToRoutine(PlayerFarming.Instance.transform.position + positions[index]));
		follower.State.facingAngle = Utils.GetAngle(follower.transform.position, PlayerFarming.Instance.transform.position);
		follower.SetBodyAnimation("Fishing/fishing-start", false);
		follower.AddBodyAnimation("Fishing/fishing-reel", false, 0f);
		follower.AddBodyAnimation("Fishing/fishing-reel", false, 0f);
		follower.AddBodyAnimation("Fishing/fishing-catch-big", false, 0f);
		follower.AddBodyAnimation("idle", true, 0f);
	}
}
