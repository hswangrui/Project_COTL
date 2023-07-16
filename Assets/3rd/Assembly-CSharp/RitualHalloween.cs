using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMTools;
using UnityEngine;

public class RitualHalloween : Ritual
{
	public static Action OnHalloweenRitualBegan;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Halloween;
		}
	}

	public override void Play()
	{
		base.Play();
		GameManager.GetInstance().StartCoroutine(RitualRoutine());
	}

	private IEnumerator RitualRoutine()
	{
		DataManager.Instance.LastHalloween = TimeManager.TotalElapsedGameTime;
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
		Vector3 currentTargetCameraPosition = GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition;
		Vector3 normalized = (ChurchFollowerManager.Instance.RitualCenterPosition.position - currentTargetCameraPosition).normalized;
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition, delegate(Vector3 x)
		{
			GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition = x;
		}, GameManager.GetInstance().CamFollowTarget.CurrentTargetCameraPosition + normalized, 2.5f).SetEase(Ease.OutSine);
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
				StartCoroutine(DanceFolower(follower, i));
			}
		}
		yield return new WaitForSeconds(2.5f);
		AudioManager.Instance.PlayOneShot("event:/rituals/consume_follower");
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.PlayOneShot("event:/rituals/funeral_ghost");
		yield return new WaitForSeconds(1.5f);
		bool waiting = true;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.4f, "", delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		ChurchFollowerManager.Instance.DisableAllOverlays();
		PlayerFarming.Instance.Spine.gameObject.SetActive(false);
		DoorRoomLocationManager.Instance.Activatable = false;
		BaseLocationManager.Instance.Activatable = false;
		ChurchLocationManager.Instance.Activatable = false;
		BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 0f, 0f);
		BiomeBaseManager.Instance.ActivateDoorRoom();
		DoorRoomLocationManager.Instance.SkyAnimator.SetTrigger("Trigger");
		Vector3 camPosition = GameManager.GetInstance().CamFollowTarget.transform.position;
		GameManager.GetInstance().CamFollowTarget.ResetTargetCamera(0f);
		yield return new WaitForEndOfFrame();
		DoorRoomLocationManager.Instance.SkyAnimator.SetBool("BloodMoon", true);
		GameManager.GetInstance().CamFollowTarget.ClearAllTargets();
		GameManager.GetInstance().CamFollowTarget.ForceSnapTo(new Vector3(-0.1999771f, 36.77f, -16.67f));
		GameManager.GetInstance().CamFollowTarget.transform.localRotation = Quaternion.Euler(-69.138f, 0f, 0f);
		yield return new WaitForSeconds(5.5f);
		waiting = true;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.4f, "", delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		GameManager.GetInstance().CamFollowTarget.ClearAllTargets();
		GameManager.GetInstance().CamFollowTarget.ForceSnapTo(camPosition);
		BiomeBaseManager.Instance.ActivateChurch();
		PlayerFarming.Instance.Spine.gameObject.SetActive(true);
		BaseLocationManager.Instance.Activatable = true;
		ChurchLocationManager.Instance.Activatable = true;
		DoorRoomLocationManager.Instance.Activatable = true;
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		yield return new WaitForSeconds(1f);
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
		BaseLocationManager.Instance.EnableBloodMoon();
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		float num = 0f;
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			float num2 = UnityEngine.Random.Range(0.1f, 0.5f);
			num += num2;
			StartCoroutine(DelayFollowerReaction(item3, num2));
		}
		yield return new WaitForSeconds(1.5f);
		CompleteRitual();
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		Action onHalloweenRitualBegan = OnHalloweenRitualBegan;
		if (onHalloweenRitualBegan != null)
		{
			onHalloweenRitualBegan();
		}
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator MoveFollower(Follower follower, int index, List<Vector3> positions)
	{
		for (int i = 0; i < 4; i++)
		{
			index++;
			if (index > positions.Count - 1)
			{
				index = 0;
			}
			bool waiting = true;
			follower.transform.DOMoveX((PlayerFarming.Instance.transform.position + positions[index]).x, 2f).SetEase(Ease.Linear).OnComplete(delegate
			{
				waiting = false;
			});
			follower.transform.DOMoveY((PlayerFarming.Instance.transform.position + positions[index]).y, 2f).SetEase(Ease.Linear);
			while (waiting)
			{
				yield return null;
			}
		}
	}

	private IEnumerator DanceFolower(Follower follower, int index)
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
		follower.SetBodyAnimation("Sermons/bloodmoon", true);
	}
}
