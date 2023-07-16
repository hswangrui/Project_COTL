using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RitualElightenment : Ritual
{
	public static Action OnEnlightenmentRitualBegan;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Enlightenment;
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
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position, null, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("idle", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		});
		yield return StartCoroutine(WaitFollowersFormCircle());
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.StartRitualOverlay();
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		yield return new WaitForSeconds(1.5f);
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTask is FollowerTask_AttendRitual)
			{
				((FollowerTask_AttendRitual)item.CurrentTask).Pray2();
			}
		}
		Vector3 t = GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition;
		Vector3 vector = t;
		Vector3 normalized = (ChurchFollowerManager.Instance.RitualCenterPosition.position - t).normalized;
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition, delegate(Vector3 x)
		{
			GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition = x;
		}, GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition + normalized, 2.5f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(2.5f);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition, delegate(Vector3 x)
		{
			GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition = x;
		}, t, 0.5f).SetEase(Ease.OutBack);
		AudioManager.Instance.PlayOneShot("event:/rituals/enlightenment_beam", PlayerFarming.Instance.gameObject);
		ChurchFollowerManager.Instance.Light.SetActive(true);
		ChurchFollowerManager.Instance.Light.transform.DOScaleX(1.75f, 0.25f).SetEase(Ease.InOutBack);
		ChurchFollowerManager.Instance.Light.transform.DOScaleY(1.75f, 0.25f).SetEase(Ease.InOutBack);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(PlayerFarming.Instance.CameraBone.transform.position);
		yield return new WaitForSeconds(4.25f);
		ChurchFollowerManager.Instance.Light.SetActive(true);
		ChurchFollowerManager.Instance.Light.transform.DOScaleX(0f, 0.25f).SetEase(Ease.InOutBack);
		ChurchFollowerManager.Instance.Light.transform.DOScaleY(0f, 0.25f).SetEase(Ease.InOutBack);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			if (item2.CurrentTask is FollowerTask_AttendRitual)
			{
				(item2.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		yield return new WaitForSeconds(1f);
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
			item3.AddThought(Thought.EnlightenmentRitual);
		}
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		yield return new WaitForSeconds(1.5f);
		DataManager.Instance.LastEnlightenment = TimeManager.TotalElapsedGameTime;
		Action onEnlightenmentRitualBegan = OnEnlightenmentRitualBegan;
		if (onEnlightenmentRitualBegan != null)
		{
			onEnlightenmentRitualBegan();
		}
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_Enlightenment, -1, 1f);
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
		follower.SetBodyAnimation("dance-hooded", true);
	}
}
