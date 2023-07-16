using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RitualUnlockWeapon : Ritual
{
	private int NumGivingDevotion;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_UnlockWeapon;
		}
	}

	public override void Play()
	{
		base.Play();
		StartCoroutine(HeartsOfTheFaithfulRitual());
	}

	private IEnumerator HeartsOfTheFaithfulRitual()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
		PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position, null, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		});
		yield return StartCoroutine(WaitFollowersFormCircle());
		yield return new WaitForSeconds(1.25f);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 10f);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.StartRitualOverlay();
		yield return new WaitForSeconds(0.5f);
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);
		NumGivingDevotion = 0;
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item != null && item.CurrentTask != null && item.CurrentTask is FollowerTask_AttendRitual)
			{
				NumGivingDevotion++;
				(item.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
				Follower follower = FollowerManager.FindFollowerByID(item.Info.ID);
				StartCoroutine(SpawnSouls(follower.transform.position));
				yield return new WaitForSeconds(0.075f);
			}
		}
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6f, 3.75f).SetEase(Ease.InOutSine);
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
		yield return StartCoroutine(EmitParticles());
		yield return new WaitForSeconds(0.5f);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().CameraResetTargetZoom();
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		float num = 0f;
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			float num2 = Random.Range(0.1f, 0.5f);
			num += num2;
			StartCoroutine(DelayFollowerReaction(item3, num2));
		}
		yield return new WaitForSeconds(2f);
		CompleteRitual();
	}

	private IEnumerator EmitParticles()
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
