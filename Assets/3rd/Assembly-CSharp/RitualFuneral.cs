using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MMTools;
using src.Extensions;
using UnityEngine;

public class RitualFuneral : Ritual
{
	private EventInstance loopedSound;

	protected override UpgradeSystem.Type RitualType
	{
		get
		{
			return UpgradeSystem.Type.Ritual_Funeral;
		}
	}

	public override void Play()
	{
		base.Play();
		StartCoroutine(RitualRoutine());
	}

	private IEnumerator RitualRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		yield return StartCoroutine(WaitFollowersFormCircle());
		yield return new WaitForSeconds(1f);
		List<Structures_Grave> graves = StructureManager.GetAllStructuresOfType<Structures_Grave>(FollowerLocation.Base);
		List<Structures_Crypt> crypts = StructureManager.GetAllStructuresOfType<Structures_Crypt>(FollowerLocation.Base);
		List<FollowerInfo> list = new List<FollowerInfo>(DataManager.Instance.Followers_Dead);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].HadFuneral)
			{
				list.RemoveAt(num);
			}
			else
			{
				bool flag = false;
				foreach (Structures_Grave item in graves)
				{
					if (item.Data.FollowerID == list[num].ID)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					foreach (Structures_Crypt item2 in crypts)
					{
						if (item2.Data.MultipleFollowerIDs.Contains(list[num].ID))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					list.RemoveAt(num);
				}
			}
		}
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		//followerSelectInstance.VotingType = TwitchVoting.VotingType.RITUAL_FUNERAL;
		followerSelectInstance.Show(list, null, false, RitualType);
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
			loopedSound = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
			StructureBrain structureBrain = null;
			foreach (Structures_Grave item3 in graves)
			{
				if (item3.Data.FollowerID == followerInfo.ID)
				{
					structureBrain = item3;
					break;
				}
			}
			if (structureBrain == null)
			{
				foreach (Structures_Crypt item4 in crypts)
				{
					if (item4.Data.MultipleFollowerIDs.Contains(followerInfo.ID))
					{
						structureBrain = item4;
						break;
					}
				}
			}
			GameManager.GetInstance().StartCoroutine(ContinueRitual(followerInfo, structureBrain));
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnCancel, (Action)delegate
		{
			AudioManager.Instance.StopLoop(loopedSound);
			EndRitual();
			Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
			Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
			CancelFollowers();
			CompleteRitual(true);
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
	}

	private IEnumerator ContinueRitual(FollowerInfo deadFollowerInfo, StructureBrain grave)
	{
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0f);
		foreach (FollowerBrain item in Ritual.GetFollowersAvailableToAttendSermon())
		{
			Follower follower = FollowerManager.FindFollowerByID(item.Info.ID);
			StartCoroutine(FollowerMoveRoutine(follower));
		}
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
		ChurchFollowerManager.Instance.StartRitualOverlay();
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);
		yield return new WaitForSeconds(1.5f);
		FollowerBrain deadFollower = FollowerBrain.GetOrCreateBrain(deadFollowerInfo);
		DeadWorshipper deadWorshipper = null;
		StructuresData structure = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
		structure.FollowerID = deadFollower.Info.ID;
		StructureManager.BuildStructure(FollowerLocation.Church, structure, ChurchFollowerManager.Instance.RitualCenterPosition.position + Vector3.left * 0.4f, Vector2Int.one, false, delegate(GameObject g)
		{
			g.GetComponent<Interaction_HarvestMeat>().enabled = false;
			deadWorshipper = g.GetComponent<DeadWorshipper>();
			deadWorshipper.ItemIndicator.SetActive(false);
			deadWorshipper.SetOutfit(FollowerOutfitType.Worshipper);
			deadWorshipper.RottenParticles.gameObject.SetActive(false);
			deadWorshipper.Spine.AnimationState.SetAnimation(0, "dead-funeral", true);
			deadWorshipper.Spine.skeleton.A = 0f;
			DOTween.To(() => deadWorshipper.Spine.skeleton.A, delegate(float x)
			{
				deadWorshipper.Spine.skeleton.A = x;
			}, 1f, 0.5f);
		}, null, false);
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/player/body_drop", ChurchFollowerManager.Instance.RitualCenterPosition.position);
		yield return new WaitForSeconds(1.5f);
		Interaction_TempleAltar.Instance.CloseUpCamera.Reset();
		GameManager.GetInstance().OnConversationNext(ChurchFollowerManager.Instance.RitualCenterPosition.gameObject, 6f);
		GameManager.GetInstance().CamFollowTarget.targetDistance = 6.5f;
		yield return new WaitForSeconds(1f);
		List<FollowerBrain> prayingFollowers = new List<FollowerBrain>();
		foreach (FollowerBrain item2 in Ritual.GetFollowersAvailableToAttendSermon())
		{
			if (item2.Info.GetOrCreateRelationship(deadFollower.Info.ID).CurrentRelationshipState == IDAndRelationship.RelationshipState.Friends)
			{
				prayingFollowers.Add(item2);
			}
		}
		bool waiting = true;
		List<FollowerBrain> followers = new List<FollowerBrain>(prayingFollowers);
		for (int i = 0; i < UnityEngine.Random.Range(2, 4); i++)
		{
			if (followers.Count == 0)
			{
				followers = new List<FollowerBrain>(Ritual.GetFollowersAvailableToAttendSermon());
				for (int num = followers.Count - 1; num >= 0; num--)
				{
					if (prayingFollowers.Contains(followers[num]))
					{
						followers.Remove(followers[num]);
					}
				}
				continue;
			}
			FollowerBrain b = followers[UnityEngine.Random.Range(0, followers.Count)];
			followers.Remove(b);
			Follower f = FollowerManager.FindFollowerByID(b.Info.ID);
			Vector3 startingPosition = b.LastPosition;
			float angle = Utils.GetAngle(b.LastPosition, ChurchFollowerManager.Instance.RitualCenterPosition.position);
			if (!(f == null))
			{
				f.HoodOff();
				yield return new WaitForSeconds(0.5f);
				waiting = true;
				((FollowerTask_ManualControl)b.CurrentTask).GoToAndStop(f, deadWorshipper.transform.position - (Vector3)Utils.DegreeToVector2(angle) * UnityEngine.Random.Range(0.75f, 1f), delegate
				{
					waiting = false;
				});
				while (waiting)
				{
					yield return null;
				}
				f.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
				f.SetBodyAnimation("action", true);
				AudioManager.Instance.PlayOneShot("event:/player/layer_clothes", f.gameObject);
				yield return new WaitForSeconds(0.33f);
				AudioManager.Instance.PlayOneShot("event:/player/layer_clothes", f.gameObject);
				yield return new WaitForSeconds(0.33f);
				AudioManager.Instance.PlayOneShot("event:/player/layer_clothes", f.gameObject);
				yield return new WaitForSeconds(0.33f);
				deadWorshipper.Flowers[i].gameObject.SetActive(true);
				deadWorshipper.Flowers[i].transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
				AudioManager.Instance.PlayOneShot("event:/player/tall_grass_push", deadWorshipper.Flowers[i]);
				waiting = true;
				((FollowerTask_ManualControl)b.CurrentTask).GoToAndStop(f, startingPosition, delegate
				{
					waiting = false;
					f.State.CURRENT_STATE = StateMachine.State.Idle;
					f.State.facingAngle = Utils.GetAngle(f.transform.position, ChurchFollowerManager.Instance.RitualCenterPosition.position);
				});
				while (waiting)
				{
					yield return null;
				}
				f.HoodOn("pray", false);
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.5f));
			}
		}
		yield return new WaitForSeconds(5f / 6f);
		DeadWorshipper ghost = null;
		StructureManager.BuildStructure(FollowerLocation.Church, structure, ChurchFollowerManager.Instance.RitualCenterPosition.position, Vector2Int.one, false, delegate(GameObject g)
		{
			g.GetComponent<Interaction_HarvestMeat>().enabled = false;
			ghost = g.GetComponent<DeadWorshipper>();
			ghost.SetOutfit(FollowerOutfitType.Worshipper);
			ghost.RottenParticles.gameObject.SetActive(false);
			ghost.Spine.AnimationState.SetAnimation(0, "ascend", false);
			ghost.ItemIndicator.SetActive(false);
			AudioManager.Instance.PlayOneShot("event:/rituals/funeral_ghost", ghost.gameObject);
			ghost.Spine.skeleton.A = 0.5f;
		}, null, false);
		while (ghost == null)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(ghost.gameObject, 12f);
		yield return new WaitForSeconds(0.5f);
		Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position - Vector3.back);
		yield return new WaitForSeconds(4f);
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			float t2 = t / 1f;
			ghost.Spine.skeleton.A = Mathf.Lerp(0.5f, 0f, t2);
			yield return null;
		}
		Interaction_TempleAltar.Instance.CloseUpCamera.Play();
		UnityEngine.Object.Destroy(ghost.gameObject);
		waiting = true;
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
		GameManager.GetInstance().CamFollowTarget.ResetTargetCamera(0f);
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().CamFollowTarget.ClearAllTargets();
		Vector3 position = grave.Data.Position - GameManager.GetInstance().CamFollowTarget.transform.forward * 3f;
		if (grave is Structures_Crypt)
		{
			position -= Vector3.forward;
		}
		GameManager.GetInstance().CamFollowTarget.ForceSnapTo(position);
		GameManager.GetInstance().CamFollowTarget.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
		ChurchFollowerManager.Instance.Sparkles.gameObject.SetActive(true);
		Vector3 SparklesStartPos = ChurchFollowerManager.Instance.Sparkles.transform.position;
		ChurchFollowerManager.Instance.Sparkles.transform.parent = BiomeConstants.Instance.transform;
		ChurchFollowerManager.Instance.Sparkles.transform.position = grave.Data.Position;
		ChurchFollowerManager.Instance.Sparkles.Play();
		AudioManager.Instance.PlayOneShot("event:/player/Curses/charm_curse", AudioManager.Instance.Listener);
		yield return new WaitForSeconds(1.5f);
		deadFollower._directInfoAccess.HadFuneral = true;
		foreach (Grave grafe in Grave.Graves)
		{
			if (grafe.StructureInfo.ID == grave.Data.ID)
			{
				grafe.SetGameObjects();
				AudioManager.Instance.PlayOneShot("event:/player/tall_grass_push", AudioManager.Instance.Listener);
				break;
			}
		}
		foreach (Interaction_Crypt crypt in Interaction_Crypt.Crypts)
		{
			if (crypt.StructureInfo.ID == grave.Data.ID)
			{
				crypt.SetGameObjects();
				AudioManager.Instance.PlayOneShot("event:/player/tall_grass_push", AudioManager.Instance.Listener);
				break;
			}
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
		BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
		Transform obj = ChurchFollowerManager.Instance.Sparkles.transform;
		obj.position = SparklesStartPos;
		obj.parent = ChurchFollowerManager.Instance.transform;
		ChurchFollowerManager.Instance.Sparkles.gameObject.SetActive(false);
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		GameManager.GetInstance().CamFollowTarget.ClearAllTargets();
		GameManager.GetInstance().CamFollowTarget.ForceSnapTo(camPosition);
		Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
		BiomeBaseManager.Instance.ActivateChurch();
		PlayerFarming.Instance.Spine.gameObject.SetActive(true);
		BaseLocationManager.Instance.Activatable = true;
		ChurchLocationManager.Instance.Activatable = true;
		deadWorshipper.Spine.AnimationState.SetAnimation(0, "dead-funeral", true);
		deadWorshipper.ItemIndicator.gameObject.SetActive(false);
		FollowerBrain.AllBrains.Remove(deadFollower);
		foreach (FollowerBrain item3 in Ritual.GetFollowersAvailableToAttendSermon())
		{
			Follower follower2 = FollowerManager.FindFollowerByID(item3.Info.ID);
			if ((object)follower2 != null)
			{
				follower2.HoodOn("pray", true);
			}
		}
		yield return new WaitForSeconds(1f);
		foreach (FollowerBrain item4 in Ritual.GetFollowersAvailableToAttendSermon())
		{
			Follower follower3 = FollowerManager.FindFollowerByID(item4.Info.ID);
			if ((bool)follower3)
			{
				StartCoroutine(FollowerMoveRoutine(follower3));
			}
		}
		AudioManager.Instance.PlayOneShot("event:/player/body_wrap");
		yield return new WaitForSeconds(1f);
		deadWorshipper.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack);
		DOTween.To(() => deadWorshipper.Spine.skeleton.A, delegate(float x)
		{
			deadWorshipper.Spine.skeleton.A = x;
		}, 0f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		StructureManager.RemoveStructure(StructureManager.GetAllStructuresOfType<Structures_DeadWorshipper>(FollowerLocation.Church)[0]);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		yield return new WaitForSeconds(2f);
		float EndingDelay = 0f;
		yield return null;
		foreach (FollowerBrain item5 in Ritual.FollowerToAttendSermon)
		{
			float num2 = UnityEngine.Random.Range(0.1f, 0.5f);
			EndingDelay += num2;
			StartCoroutine(DelayFollowerReaction(item5, num2));
			IDAndRelationship orCreateRelationship = item5.Info.GetOrCreateRelationship(deadFollower.Info.ID);
			if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Enemies)
			{
				item5.AddThought(Thought.AttendedEnemyFuneral);
			}
			else if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Friends)
			{
				item5.AddThought(Thought.AttendedFriendFuneral);
			}
			else
			{
				item5.AddThought(Thought.AttendedStrangerFuneral);
			}
		}
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);
		yield return new WaitForSeconds(1f);
		EndRitual();
		CompleteRitual(false, deadFollower.Info.ID);
		yield return new WaitForSeconds(1f);
		CultFaithManager.AddThought(Thought.Cult_Funeral, deadFollower.Info.ID, 1f);
	}

	private IEnumerator FollowerMoveRoutine(Follower follower)
	{
		Vector3 startingPosition = follower.Brain.LastPosition;
		float angle = Utils.GetAngle(follower.Brain.LastPosition, ChurchFollowerManager.Instance.RitualCenterPosition.position);
		FollowerTask_ManualControl task = new FollowerTask_ManualControl();
		follower.Brain.HardSwapToTask(task);
		bool waiting = true;
		task.GoToAndStop(follower, ChurchFollowerManager.Instance.RitualCenterPosition.position - (Vector3)Utils.DegreeToVector2(angle) * UnityEngine.Random.Range(0.5f, 0.75f), delegate
		{
			waiting = false;
		});
		follower.SetOutfit(FollowerOutfitType.Worshipper, true);
		while (waiting)
		{
			yield return null;
		}
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.SetBodyAnimation("action", true);
		yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 2f));
		task.GoToAndStop(follower, startingPosition, delegate
		{
			follower.State.facingAngle = Utils.GetAngle(follower.transform.position, ChurchFollowerManager.Instance.RitualCenterPosition.position);
			follower.State.CURRENT_STATE = StateMachine.State.Idle;
		});
	}

	private void EndRitual()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
		ChurchFollowerManager.Instance.EndRitualOverlay();
		GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
		Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
	}
}
