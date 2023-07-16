using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RitualDonation : Ritual
{
	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_DonationRitual;
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
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		int num4 = 10;
		float num5 = 5f;
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon())
		{
			int num6 = item.Info.XPLevel * num4;
			int num7 = 2;
			num3 += item.Info.XPLevel;
			num2 += num6 * num7;
			StartCoroutine(GiveCoins(FollowerManager.FindFollowerByID(item.Info.ID), num5, num, num6));
			num += 0.1f;
		}
		Inventory.AddItem(20, num2);
		float deathtimer = BiomeConstants.Instance.HitFX_Blocked.GetComponent<destroyMe>().deathtimer;
		BiomeConstants.Instance.HitFX_Blocked.CreatePool((int)((float)(num3 * num4) / (num5 / deathtimer)), true);
		yield return null;
		ResourceCustomTarget.CreatePool((int)((float)Ritual.FollowerToAttendSermon.Count * 1.5f));
		yield return new WaitForSeconds(6.2f);
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
		float num8 = 0f;
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			float num9 = Random.Range(0.1f, 0.5f);
			num8 += num9;
			StartCoroutine(DelayFollowerReaction(item3, num9));
		}
		yield return new WaitForSeconds(1.5f);
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_DonationRitual, -1, 1f);
	}

	private IEnumerator GiveCoins(Follower follower, float totalTime, float delay, int randomCoins)
	{
		if (!(follower == null))
		{
			yield return new WaitForSeconds(delay);
			float increment = (totalTime - delay) / (float)randomCoins;
			for (int i = 0; i < randomCoins; i++)
			{
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", follower.transform.position);
				ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, follower.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, delegate
				{
				});
				yield return new WaitForSeconds(increment);
			}
		}
	}
}
