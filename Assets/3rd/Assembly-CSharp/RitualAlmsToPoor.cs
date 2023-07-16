using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class RitualAlmsToPoor : Ritual
{
	private EventInstance loopedSound;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_AlmsToPoor;
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
			PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		});
		yield return StartCoroutine(WaitFollowersFormCircle());
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
		loopedSound = AudioManager.Instance.CreateLoop("event:/rituals/coin_loop", PlayerFarming.Instance.gameObject, true);
		float num = 0f;
		float num2 = 5f;
		int maxCoins = 7;
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon())
		{
			StartCoroutine(GiveCoins(FollowerManager.FindFollowerByID(item.Info.ID), num2, num));
			num += 0.1f;
		}
		float deathtimer = BiomeConstants.Instance.HitFX_Blocked.GetComponent<destroyMe>().deathtimer;
		BiomeConstants.Instance.HitFX_Blocked.CreatePool((int)((float)(Ritual.FollowerToAttendSermon.Count * maxCoins) / (num2 / deathtimer)), true);
		yield return null;
		ResourceCustomTarget.CreatePool(Ritual.FollowerToAttendSermon.Count * maxCoins);
		yield return new WaitForSeconds(5f);
		AudioManager.Instance.StopLoop(loopedSound);
		yield return new WaitForSeconds(1.2f);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(PlayerFarming.Instance.CameraBone.transform.position);
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
		float EndingDelay = 0f;
		foreach (FollowerBrain brain in Ritual.FollowerToAttendSermon)
		{
			brain.AddThought(Thought.AlmsToThePoorRitual);
			brain.AddAdoration(FollowerBrain.AdorationActions.Ritual_AlmsToPoor, delegate
			{
				float num3 = Random.Range(0.1f, 0.5f);
				EndingDelay += num3;
				GameManager.GetInstance().StartCoroutine(DelayFollowerReaction(brain, num3));
			});
		}
		yield return new WaitForSeconds(3f + EndingDelay);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_AlmsToPoor, -1, 1f);
	}

	private IEnumerator GiveCoins(Follower follower, float totalTime, float delay)
	{
		if (!(follower == null))
		{
			yield return new WaitForSeconds(delay);
			int randomCoins = Random.Range(3, 7);
			float increment = (totalTime - delay) / (float)randomCoins;
			for (int i = 0; i < randomCoins; i++)
			{
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
				ResourceCustomTarget.Create(follower.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, PlayCoinSound);
				yield return new WaitForSeconds(increment);
			}
		}
	}

	private void PlayCoinSound()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/coins", PlayerFarming.Instance.transform.position);
	}
}
