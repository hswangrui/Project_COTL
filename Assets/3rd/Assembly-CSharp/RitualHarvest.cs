using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMTools;
using UnityEngine;

public class RitualHarvest : Ritual
{
	private Vector3 averagePosition;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_HarvestRitual;
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
		AudioManager.Instance.PlayOneShot("event:/Stings/harvest");
		ChurchFollowerManager.Instance.FarmMud.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.FarmMud.transform.DOMoveZ(1f, 0f);
		ChurchFollowerManager.Instance.FarmMud.transform.DOMoveZ(0f, 1.25f).SetEase(Ease.OutQuart);
		List<FollowerBrain> list = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
		for (int i = 0; i < 2; i++)
		{
			if (list.Count == 0)
			{
				break;
			}
			FollowerBrain followerBrain = list[Random.Range(0, list.Count)];
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
				((FollowerTask_AttendRitual)item.CurrentTask).Pray2();
			}
		}
		yield return new WaitForSeconds(6f);
		bool waiting = true;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.4f, "", delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		PlayerFarming.Instance.Spine.gameObject.SetActive(false);
		BaseLocationManager.Instance.Activatable = false;
		ChurchLocationManager.Instance.Activatable = false;
		BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 0f, 0f);
		BiomeBaseManager.Instance.ActivateRoom(false);
		Vector3 camPosition = GameManager.GetInstance().CamFollowTarget.transform.position;
		List<Structures_FarmerPlot> plots = StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>(FollowerLocation.Base);
		if (plots.Count > 0)
		{
			Structures_FarmerPlot structures_FarmerPlot = plots[Random.Range(0, plots.Count)];
			foreach (FarmPlot farmPlot in FarmPlot.FarmPlots)
			{
				if (farmPlot.StructureInfo.ID != structures_FarmerPlot.Data.ID)
				{
					continue;
				}
				averagePosition = Vector3.zero;
				int num = 0;
				foreach (FarmPlot farmPlot2 in FarmPlot.FarmPlots)
				{
					if (Vector3.Distance(farmPlot2.Position, farmPlot.Position) < 5f)
					{
						averagePosition += farmPlot2.Position;
						num++;
					}
				}
				averagePosition /= (float)num;
				GameManager.GetInstance().CamFollowTarget.ResetTargetCamera(0f);
				yield return new WaitForEndOfFrame();
				GameManager.GetInstance().CamFollowTarget.ClearAllTargets();
				GameManager.GetInstance().CamFollowTarget.SnapTo(averagePosition);
				GameManager.GetInstance().CamFollowTarget.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
				break;
			}
		}
		ChurchFollowerManager.Instance.Sparkles.gameObject.SetActive(true);
		Vector3 SparklesStartPos = ChurchFollowerManager.Instance.Sparkles.transform.position;
		ChurchFollowerManager.Instance.Sparkles.transform.parent = BiomeConstants.Instance.transform;
		ChurchFollowerManager.Instance.Sparkles.transform.position = averagePosition;
		ChurchFollowerManager.Instance.Sparkles.Play();
		AudioManager.Instance.PlayOneShot("event:/player/Curses/charm_curse", AudioManager.Instance.Listener);
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.PlayOneShot("event:/building/finished_farmplot", AudioManager.Instance.Listener);
		foreach (Structures_FarmerPlot item2 in plots)
		{
			item2.ForceFullyGrown();
			yield return null;
		}
		yield return new WaitForSeconds(2.5f);
		waiting = true;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.4f, "", delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		ChurchFollowerManager.Instance.Sparkles.transform.position = SparklesStartPos;
		ChurchFollowerManager.Instance.Sparkles.transform.parent = ChurchFollowerManager.Instance.transform;
		ChurchFollowerManager.Instance.Sparkles.gameObject.SetActive(false);
		GameManager.GetInstance().CamFollowTarget.ClearAllTargets();
		GameManager.GetInstance().CamFollowTarget.ForceSnapTo(camPosition);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
		BiomeBaseManager.Instance.ActivateChurch();
		PlayerFarming.Instance.Spine.gameObject.SetActive(true);
		BaseLocationManager.Instance.Activatable = true;
		ChurchLocationManager.Instance.Activatable = true;
		foreach (FollowerBrain item3 in Ritual.GetFollowersAvailableToAttendSermon())
		{
			if (item3 != null)
			{
				Follower follower2 = FollowerManager.FindFollowerByID(item3.Info.ID);
				if ((object)follower2 != null)
				{
					follower2.HoodOn("pray", true);
				}
			}
		}
		yield return new WaitForSeconds(2f);
		ChurchFollowerManager.Instance.FarmMud.transform.DOMoveZ(1f, 1f).SetEase(Ease.InQuart);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		foreach (FollowerBrain item4 in Ritual.FollowerToAttendSermon)
		{
			if (item4 != null && item4.CurrentTask is FollowerTask_AttendRitual)
			{
				(item4.CurrentTask as FollowerTask_AttendRitual).Cheer();
			}
		}
		yield return new WaitForSeconds(0.5f);
		ChurchFollowerManager.Instance.FarmMud.gameObject.SetActive(false);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		float num2 = 0f;
		foreach (FollowerBrain item5 in Ritual.FollowerToAttendSermon)
		{
			if (item5 != null)
			{
				float num3 = Random.Range(0.1f, 0.5f);
				num2 += num3;
				StartCoroutine(DelayFollowerReaction(item5, num3));
			}
		}
		yield return new WaitForSeconds(1.5f);
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		CompleteRitual();
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_HarvestRitual, -1, 1f);
	}

	private IEnumerator MoveFollower(Follower follower, int index)
	{
		List<Vector3> positions = new List<Vector3>
		{
			Vector3.left,
			Vector3.right
		};
		string[] anims = new string[4] { "Farming/add-seed-mushroom", "Farming/add-seed-pumpkin", "Farming/add-seed-redflower", "Farming/add-seed" };
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
		follower.SetBodyAnimation(anims[Random.Range(0, anims.Length)], false);
		follower.AddBodyAnimation(anims[Random.Range(0, anims.Length)], false, 0f);
		follower.AddBodyAnimation(anims[Random.Range(0, anims.Length)], false, 0f);
		follower.AddBodyAnimation("idle", true, 0f);
	}
}
