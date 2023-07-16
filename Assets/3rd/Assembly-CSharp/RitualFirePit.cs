using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class RitualFirePit : Ritual
{
	private EventInstance fireSFXLoop;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_FirePit;
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
		Interaction_FireDancePit firePit = ChurchFollowerManager.Instance.FirePit;
		firePit.gameObject.SetActive(true);
		firePit.transform.position = new Vector3(firePit.transform.position.x, firePit.transform.position.y, 2f);
		firePit.transform.DOMoveZ(0f, 1f).SetEase(Ease.OutBack);
		firePit.GetComponent<BoxCollider2D>().enabled = true;
		Bounds bounds = firePit.GetComponent<BoxCollider2D>().bounds;
		AstarPath.active.UpdateGraphs(bounds);
		PlayerFarming.Instance.GoToAndStop(firePit.PlayerPosition.transform.position, null, false, true, delegate
		{
			Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("idle", 0, true);
			PlayerFarming.Instance.state.transform.DOMove(firePit.PlayerPosition.transform.position, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
		});
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.bongos_singing);
		Debug.Log("A");
		while (firePit.StructureInfo == null)
		{
			yield return null;
		}
		Debug.Log("B");
		yield return StartCoroutine(WaitForFollowersToTakeSeat(0));
		Debug.Log("C");
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
		ChurchFollowerManager.Instance.PlayOverlay(ChurchFollowerManager.OverlayType.Ritual, "resurrect");
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6.5f, 1f).SetEase(Ease.OutSine);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/fire-ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/fire-ritual-loop", 0, true, 0f);
		yield return new WaitForSeconds(0.66f);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.RedOverlay.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.RedOverlay.alpha = 0f;
		ChurchFollowerManager.Instance.RedOverlay.DOFade(1f, 1f).SetDelay(1.2f);
		DeviceLightingManager.TransitionLighting(Color.black, Color.red, 1.2f);
		firePit.BonfireOn.SetActive(true);
		firePit.BonfireOff.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", firePit.gameObject);
		fireSFXLoop = AudioManager.Instance.CreateLoop("event:/fire/big_fire", firePit.gameObject, true);
		yield return new WaitForSeconds(1.2f);
		DeviceLightingManager.TransitionLighting(Color.red, Color.red, 4f);
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
				follower.SetBodyAnimation("dance", true);
			}
		}
		yield return new WaitForSeconds(4f);
		ChurchFollowerManager.Instance.RedOverlay.DOFade(0f, 1f);
		DeviceLightingManager.TransitionLighting(Color.red, Color.black, 2f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/fire-ritual-stop", 0, false, 0f);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		foreach (FollowerBrain item2 in followers)
		{
			item2.CompleteCurrentTask();
		}
		firePit.BonfireOn.SetActive(false);
		firePit.BonfireOff.SetActive(true);
		AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", firePit.gameObject);
		AudioManager.Instance.StopLoop(fireSFXLoop);
		firePit.transform.DOMoveZ(4f, 1f).SetEase(Ease.OutSine);
		yield return new WaitForSeconds(1f);
		ChurchFollowerManager.Instance.RedOverlay.gameObject.SetActive(false);
		firePit.gameObject.SetActive(false);
		firePit.GetComponent<BoxCollider2D>().enabled = false;
		AstarPath.active.UpdateGraphs(bounds);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.Temple);
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
		CultFaithManager.AddThought(Thought.DancePit, -1, 1f);
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

	private IEnumerator WaitForFollowersToTakeSeat(int firepitID)
	{
		Debug.Log("WaitForFollowersToTakeSeat");
		GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 12f);
		bool getFollowers = Ritual.FollowerToAttendSermon == null || Ritual.FollowerToAttendSermon.Count <= 0;
		if (getFollowers)
		{
			Ritual.FollowerToAttendSermon = new List<FollowerBrain>();
		}
		List<FollowerBrain> list = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
		foreach (FollowerBrain item in list)
		{
			if (getFollowers)
			{
				Ritual.FollowerToAttendSermon.Add(item);
			}
			if (item.CurrentTask != null)
			{
				item.CurrentTask.Abort();
			}
			item.HardSwapToTask(new FollowerTask_DanceFirePit(firepitID));
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
			FollowerTask currentTask = item2.CurrentTask;
			if (currentTask != null)
			{
				currentTask.RecalculateDestination();
			}
		}
		float timer = 0f;
		while (!FollowersInPosition(firepitID))
		{
			float num;
			timer = (num = timer + Time.deltaTime);
			if (!(num < 10f))
			{
				break;
			}
			Debug.Log("WAITING");
			yield return null;
		}
		SimulationManager.Pause();
		GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.PortalEffect.gameObject, 8f);
	}

	private bool FollowersInPosition(int firepitID)
	{
		foreach (FollowerBrain item in Ritual.FollowerToAttendSermon)
		{
			if (item.CurrentTaskType != FollowerTaskType.ChangeLocation && item.Location == PlayerFarming.Location && (item.CurrentTaskType != FollowerTaskType.DanceFirePit || item.CurrentTask.State == FollowerTaskState.Doing))
			{
				continue;
			}
			if (item.Location != PlayerFarming.Location)
			{
				item.HardSwapToTask(new FollowerTask_DanceFirePit(firepitID));
				item.ShouldReconsiderTask = false;
				item.DesiredLocation = FollowerLocation.Church;
				if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					item.CurrentTask.Arrive();
				}
			}
			else if (item.CurrentTaskType != FollowerTaskType.DanceFirePit)
			{
				if (item.CurrentTask != null)
				{
					item.CurrentTask.Abort();
				}
				item.HardSwapToTask(new FollowerTask_DanceFirePit(firepitID));
				if (item.CurrentTaskType == FollowerTaskType.ChangeLocation)
				{
					item.CurrentTask.Arrive();
				}
			}
			return false;
		}
		return true;
	}

	public Vector3 GetDancePosition(Vector3 position)
	{
		Vector3 vector = position + Vector3.up * 3f;
		Vector3 vector2 = Random.insideUnitCircle;
		float num = Random.Range(2f, 4f);
		return vector + vector2 * num;
	}
}
