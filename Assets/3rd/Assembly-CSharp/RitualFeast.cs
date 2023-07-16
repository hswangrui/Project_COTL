using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RitualFeast : Ritual
{
	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Feast;
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
		Interaction_FeastTable feastTable = ChurchFollowerManager.Instance.FeastTable;
		feastTable.gameObject.SetActive(true);
		feastTable.transform.position = new Vector3(feastTable.transform.position.x, feastTable.transform.position.y, 2f);
		feastTable.transform.DOMoveZ(0f, 1f).SetEase(Ease.OutBack);
		AudioManager.Instance.PlayOneShot("event:/ui/map_location_pan", feastTable.gameObject);
		AudioManager.Instance.PlayOneShot("event:/player/harvest_meat", PlayerFarming.Instance.gameObject);
		feastTable.GetComponent<BoxCollider2D>().enabled = true;
		Bounds bounds = feastTable.GetComponent<BoxCollider2D>().bounds;
		AstarPath.active.UpdateGraphs(bounds);
		PlayerFarming.Instance.GoToAndStop(feastTable.PlayerPosition.transform.position, null, false, false, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("idle", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(feastTable.PlayerPosition.transform.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		});
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		while (feastTable.StructureInfo == null)
		{
			yield return null;
		}
		yield return StartCoroutine(WaitForFollowersToTakeSeat(feastTable.StructureInfo.ID));
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/feast-start", 0, false);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower_noBookPage", PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/feast-eat", 0, true, 0f);
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.PlayOverlay(ChurchFollowerManager.OverlayType.Ritual, "feast");
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6.5f, 1f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(1.2f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		yield return new WaitForSeconds(1.2f);
		List<FollowerBrain> followers = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
		foreach (FollowerBrain item in followers)
		{
			if (item.CurrentTask is FollowerTask_ChangeLocation)
			{
				item.CurrentTask.Arrive();
			}
			Follower follower = FollowerManager.FindFollowerByID(item.Info.ID);
			if ((bool)follower)
			{
				follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
				follower.SetBodyAnimation("Food/feast-eat", true);
			}
		}
		feastTable.IsFeastActive = true;
		yield return new WaitForSeconds(7f);
		feastTable.IsFeastActive = false;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/feast-end", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		foreach (FollowerBrain item2 in followers)
		{
			item2.CompleteCurrentTask();
		}
		feastTable.transform.DOMoveZ(2f, 0.25f).SetEase(Ease.InBack);
		AudioManager.Instance.PlayOneShot("event:/ui/map_location_pan", feastTable.gameObject);
		AudioManager.Instance.PlayOneShot("event:/player/harvest_meat_done", PlayerFarming.Instance.gameObject);
		yield return new WaitForSeconds(1f);
		feastTable.gameObject.SetActive(false);
		feastTable.GetComponent<BoxCollider2D>().enabled = false;
		AstarPath.active.UpdateGraphs(bounds);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		float num = 0f;
		foreach (FollowerBrain item3 in Ritual.FollowerToAttendSermon)
		{
			float num2 = Random.Range(0.1f, 0.5f);
			num += num2;
			StartCoroutine(DelayFollowerReaction(item3, num2));
		}
		yield return new WaitForSeconds(1.5f);
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		DataManager.Instance.LastFeastDeclared = TimeManager.TotalElapsedGameTime;
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_Feast, -1, 1f);
		foreach (FollowerBrain item4 in followers)
		{
			item4.Stats.Starvation = 0f;
			item4.Stats.Satiation = 100f;
			item4.AddThought(Thought.FeastTable);
		}
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

	private IEnumerator WaitForFollowersToTakeSeat(int feastTableID)
	{
		GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 12f);
		bool getFollowers = Ritual.FollowerToAttendSermon == null || Ritual.FollowerToAttendSermon.Count <= 0;
		if (getFollowers)
		{
			Ritual.FollowerToAttendSermon = new List<FollowerBrain>();
		}
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon())
		{
			if (getFollowers)
			{
				Ritual.FollowerToAttendSermon.Add(item);
			}
			if (item.CurrentTask != null)
			{
				item.CurrentTask.Abort();
			}
			item.HardSwapToTask(new FollowerTask_EatFeastTable(feastTableID));
			if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
			{
				item.CurrentTask.Arrive();
			}
			Follower follower = FollowerManager.FindFollowerByID(item.Info.ID);
			if ((object)follower != null)
			{
				follower.HideAllFollowerIcons();
			}
			yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
		}
		yield return null;
		Debug.Log("RECALCULATE! " + Ritual.FollowerToAttendSermon.Count);
		foreach (FollowerBrain item2 in Ritual.FollowerToAttendSermon)
		{
			item2.CurrentTask.RecalculateDestination();
		}
		float timer = 0f;
		while (!FollowersInPosition())
		{
			float num;
			timer = (num = timer + Time.deltaTime);
			if (!(num < 10f))
			{
				break;
			}
			yield return null;
		}
		SimulationManager.Pause();
		GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.PortalEffect.gameObject, 8f);
	}

	private bool FollowersInPosition()
	{
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTaskType != FollowerTaskType.ChangeLocation && item.Location == PlayerFarming.Location && (item.CurrentTaskType != FollowerTaskType.EatFeastTable || item.CurrentTask.State == FollowerTaskState.Doing))
			{
				continue;
			}
			if (item.CurrentTaskType != FollowerTaskType.EatFeastTable)
			{
				if (item.CurrentTask != null)
				{
					item.CurrentTask.Abort();
				}
				item.HardSwapToTask(new FollowerTask_EatFeastTable(0));
				if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					item.CurrentTask.Arrive();
				}
			}
			if (item.Location != PlayerFarming.Location)
			{
				item.HardSwapToTask(new FollowerTask_EatFeastTable(0));
				item.ShouldReconsiderTask = false;
				if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					item.CurrentTask.Arrive();
				}
			}
			return false;
		}
		return true;
	}
}
