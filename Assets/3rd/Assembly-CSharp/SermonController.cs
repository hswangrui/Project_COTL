using System;
using System.Collections;
using FMOD.Studio;
using Lamb.UI;
using src.Extensions;
using src.Rituals;
using src.UI.Overlays.TutorialOverlay;
using Unify;
using UnityEngine;

public class SermonController : MonoBehaviour
{
	private StateMachine state;

	private EventInstance sermonLoop;

	private Interaction_TempleAltar TempleAltar;

	private UIDoctrineBar UIDoctrineBar;

	private float barLocalXP;

	private SermonCategory SermonCategory = SermonCategory.PlayerUpgrade;

	private UIHeartsOfTheFaithfulChoiceMenuController _heartsOfTheFaithfulMenuTemplate;

	public void Play(StateMachine state)
	{
		this.state = state;
		TempleAltar = GetComponent<Interaction_TempleAltar>();
		StartCoroutine(TeachSermonRoutine());
	}

	private IEnumerator TeachSermonRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
		AudioManager.Instance.PlayOneShot("event:/sermon/start_sermon", PlayerFarming.Instance.gameObject);
		AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);
		StartCoroutine(TempleAltar.CentrePlayer());
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 12f);
		yield return StartCoroutine(TempleAltar.FollowersEnterForSermonRoutine());
		SimulationManager.Pause();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 7f);
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, -0.5f));
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 8f);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("sermons/sermon-loop", 0, true, 0f);
		sermonLoop = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
		yield return new WaitForSeconds(0.6f);
		TempleAltar.PulseDisplacementObject(state.transform.position);
		yield return new WaitForSeconds(0.4f);
		ChurchFollowerManager.Instance.StartSermonEffect();
		if (GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(TempleAltar.DoctrineXPPrefab, GameObject.FindWithTag("Canvas").transform);
			UIDoctrineBar = gameObject.GetComponent<UIDoctrineBar>();
			float xp = DoctrineUpgradeSystem.GetXPBySermon(SermonCategory);
			DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory);
			float num2 = 1.5f;
			yield return StartCoroutine(UIDoctrineBar.Show(xp, SermonCategory));
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 11f);
			int followersCount = Ritual.FollowerToAttendSermon.Count;
			float delay = 1.5f / (float)followersCount;
			barLocalXP = xp;
			int count = 0;
			while (GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
			{
				xp += 0.1f * (float)Ritual.FollowerToAttendSermon[count].Info.XPLevel;
				float target = DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory);
				int i = -1;
				while (true)
				{
					int num = i + 1;
					i = num;
					if (num >= Ritual.FollowerToAttendSermon[count].Info.XPLevel)
					{
						break;
					}
					SoulCustomTarget.Create(PlayerFarming.Instance.CrownBone.gameObject, Ritual.FollowerToAttendSermon[count].LastPosition, Color.white, delegate
					{
						IncrementXPBar();
					}, 0.2f, 500f);
					yield return new WaitForSeconds(0.1f);
				}
				yield return new WaitForSeconds(delay);
				if (Mathf.RoundToInt(xp * 10f) >= Mathf.RoundToInt(target * 10f))
				{
					yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
					StartCoroutine(UIDoctrineBar.FlashBarRoutine(0.3f, 1f));
					yield return new WaitForSeconds(0.5f);
					yield return StartCoroutine(UIDoctrineBar.Hide());
					UITutorialOverlayController TutorialOverlay = null;
					if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.PlayerLevelUp))
					{
						TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.PlayerLevelUp);
					}
					while (TutorialOverlay != null)
					{
						yield return null;
					}
					yield return StartCoroutine(PlayerUpgrade());
					xp = 0f;
					if (count < followersCount - 1 && GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
					{
						yield return StartCoroutine(UIDoctrineBar.Show(0f, SermonCategory));
						barLocalXP = 0f;
					}
					else
					{
						StartCoroutine(UIDoctrineBar.Hide());
					}
				}
				count++;
				if (count >= followersCount)
				{
					break;
				}
			}
			ChurchFollowerManager.Instance.EndSermonEffect();
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
			yield return new WaitForSeconds(0.5f);
			yield return StartCoroutine(UIDoctrineBar.Hide());
			DoctrineUpgradeSystem.SetXPBySermon(SermonCategory, xp);
			UnityEngine.Object.Destroy(UIDoctrineBar.gameObject);
		}
		else
		{
			yield return new WaitForSeconds(2f);
			Interaction_TempleAltar.Instance.PulseDisplacementObject(PlayerFarming.Instance.transform.position);
			BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "blue", "burst_big");
			PlayerFarming.Instance.health.BlueHearts += 2f;
			yield return new WaitForSeconds(1f);
			ChurchFollowerManager.Instance.EndSermonEffect();
		}
		yield return new WaitForSeconds(0.5f);
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("DELIVER_FIRST_SERMON"));
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/sermon/end_sermon", PlayerFarming.Instance.gameObject);
		sermonLoop.stop(STOP_MODE.ALLOWFADEOUT);
		AudioManager.Instance.StopLoop(sermonLoop);
		yield return new WaitForSeconds(1f / 3f);
		TempleAltar.ResetSprite();
		AudioManager.Instance.PlayOneShot("event:/sermon/book_put_down", PlayerFarming.Instance.gameObject);
		DataManager.Instance.PreviousSermonDayIndex = TimeManager.CurrentDay;
		PlayerFarming.Instance.Spine.UseDeltaTime = true;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTaskType != FollowerTaskType.AttendTeaching)
			{
				continue;
			}
			if (allBrain.HasTrait(FollowerTrait.TraitType.SermonEnthusiast))
			{
				allBrain.AddThought(Thought.WatchedSermonDevotee);
			}
			else
			{
				allBrain.AddThought(Thought.WatchedSermon);
			}
			Follower f = FollowerManager.FindFollowerByID(allBrain.Info.ID);
			if (f != null)
			{
				allBrain.GetWillLevelUp(FollowerBrain.AdorationActions.Sermon);
				allBrain.AddAdoration(FollowerBrain.AdorationActions.Sermon, delegate
				{
					if (f.Brain.CurrentTask != null && f.Brain.CurrentTask is FollowerTask_AttendTeaching)
					{
						((FollowerTask_AttendTeaching)f.Brain.CurrentTask).StartAgain(f);
					}
				});
			}
			StartCoroutine(TempleAltar.DelayFollowerReaction(allBrain, UnityEngine.Random.Range(0.1f, 0.5f)));
		}
		DataManager.Instance.PreviousSermonCategory = SermonCategory;
		TempleAltar.ResetSprite();
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GiveSermon);
		TempleAltar.Activated = false;
		if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SermonEnthusiast))
		{
			CultFaithManager.AddThought(Thought.Cult_SermonEnthusiast_Trait, -1, 1f);
			FaithBarFake.Play(FollowerThoughts.GetData(Thought.Cult_SermonEnthusiast_Trait).Modifier);
		}
		else
		{
			CultFaithManager.AddThought(Thought.Cult_Sermon, -1, 1f);
			FaithBarFake.Play(FollowerThoughts.GetData(Thought.Cult_Sermon).Modifier);
		}
		if (DataManager.Instance.WokeUpEveryoneDay == TimeManager.CurrentDay && TimeManager.CurrentPhase == DayPhase.Night && !FollowerBrainStats.IsWorkThroughTheNight)
		{
			CultFaithManager.AddThought(Thought.Cult_WokeUpEveryone, -1, 1f);
		}
		yield return new WaitForSeconds(1f);
		Interaction_TempleAltar.Instance.OnInteract(PlayerFarming.Instance.state);
	}

	private void IncrementXPBar()
	{
		barLocalXP += 0.1f;
		StartCoroutine(UIDoctrineBar.UpdateFirstBar(barLocalXP, 0.1f));
	}

	private void IncrementCustomXPBar(float value)
	{
		barLocalXP += value;
		StartCoroutine(UIDoctrineBar.UpdateFirstBar(barLocalXP, 0.1f));
	}

	public IEnumerator SacrificeLevelUp(int amount, Action Callback)
	{
		float xp = DoctrineUpgradeSystem.GetXPBySermon(SermonCategory);
		DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory);
		barLocalXP = xp;
		TempleAltar = Interaction_TempleAltar.Instance;
		GameObject gameObject = UnityEngine.Object.Instantiate(TempleAltar.DoctrineXPPrefab, GameObject.FindWithTag("Canvas").transform);
		UIDoctrineBar = gameObject.GetComponent<UIDoctrineBar>();
		yield return StartCoroutine(UIDoctrineBar.Show(xp, SermonCategory));
		float num = DoctrineUpgradeSystem.GetXPBySermon(SermonCategory) * 10f;
		float num2 = DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory) * 10f;
		float increment2 = 2f / (num2 - num);
		while (amount > 0)
		{
			SoulCustomTarget.Create(PlayerFarming.Instance.CrownBone.gameObject, Interaction_TempleAltar.Instance.PortalEffect.transform.position, Color.white, delegate
			{
				IncrementXPBar();
			}, 0.2f, 500f);
			xp += 0.1f;
			amount--;
			if (Mathf.RoundToInt(xp * 10f) >= Mathf.RoundToInt(DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory) * 10f))
			{
				break;
			}
			yield return new WaitForSeconds(increment2);
		}
		if (Mathf.RoundToInt(xp * 10f) >= Mathf.RoundToInt(DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory) * 10f))
		{
			yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
			yield return new WaitForSeconds(0.5f);
			StartCoroutine(UIDoctrineBar.FlashBarRoutine(0.3f, 1f));
			yield return new WaitForSeconds(0.5f);
			xp = 0f;
			DoctrineUpgradeSystem.SetXPBySermon(SermonCategory, xp);
			yield return StartCoroutine(UIDoctrineBar.Hide());
			yield return StartCoroutine(PlayerUpgrade());
		}
		else
		{
			DoctrineUpgradeSystem.SetXPBySermon(SermonCategory, xp);
		}
		if (amount <= 0 || !GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
		{
			yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
			yield return new WaitForSeconds(0.5f);
			StartCoroutine(UIDoctrineBar.Hide());
			Debug.Log("End now don't give any more souls!");
			if (Callback != null)
			{
				Callback();
			}
			yield break;
		}
		yield return StartCoroutine(UIDoctrineBar.Show(0f, SermonCategory));
		barLocalXP = 0f;
		if (GameManager.GetInstance().UpgradePlayerConfiguration.HasUnlockAvailable())
		{
			num = DoctrineUpgradeSystem.GetXPBySermon(SermonCategory) * 10f;
			num2 = DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory) * 10f;
			increment2 = 2f / (num2 - num);
			while (amount > 0)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.CrownBone.gameObject, Interaction_TempleAltar.Instance.PortalEffect.transform.position, Color.white, delegate
				{
					IncrementXPBar();
				}, 0.2f, 500f);
				xp += 0.1f;
				amount--;
				if (Mathf.RoundToInt(xp * 10f) >= Mathf.RoundToInt(DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory) * 10f))
				{
					break;
				}
				yield return new WaitForSeconds(increment2);
			}
			yield return StartCoroutine(UIDoctrineBar.UpdateSecondBar(xp, 0.5f));
			yield return new WaitForSeconds(0.5f);
			DoctrineUpgradeSystem.SetXPBySermon(SermonCategory, xp);
			yield return StartCoroutine(UIDoctrineBar.Hide());
		}
		if (Callback != null)
		{
			Callback();
		}
	}

	public IEnumerator PlayerUpgrade()
	{
		UpgradeSystem.DisciplePoints = 1;
		UIHeartsOfTheFaithfulChoiceMenuController.Types upgradeType = UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts;
		UpgradeSystem.Type upgrade = UpgradeSystem.Type.Count;
		UIUpgradePlayerTreeMenuController uIUpgradePlayerTreeMenuController = MonoSingleton<UIManager>.Instance.ShowPlayerUpgradeTree();
		uIUpgradePlayerTreeMenuController.OnUpgradeUnlocked = (Action<UpgradeSystem.Type>)Delegate.Combine(uIUpgradePlayerTreeMenuController.OnUpgradeUnlocked, (Action<UpgradeSystem.Type>)delegate(UpgradeSystem.Type type)
		{
			upgrade = type;
			upgradeType = RitualFlockOfTheFaithful.GetUpgradeType(type);
		});
		yield return uIUpgradePlayerTreeMenuController.YieldUntilHidden();
		yield return new WaitForSecondsRealtime(0.25f);
		if (WasRelicUpgrade(upgrade))
		{
			Time.timeScale = 0f;
			UIRelicMenuController uIRelicMenuController = MonoSingleton<UIManager>.Instance.RelicMenuTemplate.Instantiate();
			uIRelicMenuController.Show(EquipmentManager.GetRelicsForUpgradeType(upgrade));
			yield return uIRelicMenuController.YieldUntilHidden();
			Time.timeScale = 1f;
		}
		UpgradeSystem.DisciplePoints = 0;
		DataManager.Instance.Doctrine_PlayerUpgrade_Level++;
		yield return StartCoroutine(EmitParticles(upgradeType));
	}

	private bool WasRelicUpgrade(UpgradeSystem.Type upgrade)
	{
		if ((uint)(upgrade - 233) <= 3u || upgrade == UpgradeSystem.Type.Relic_Pack_Default)
		{
			return true;
		}
		return false;
	}

	private IEnumerator EmitParticles(UIHeartsOfTheFaithfulChoiceMenuController.Types chosenUpgrade)
	{
		yield return new WaitForSeconds(0.2f);
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
				switch (chosenUpgrade)
				{
				case UIHeartsOfTheFaithfulChoiceMenuController.Types.Hearts:
					AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", base.gameObject);
					AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
					BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "red", "burst_big");
					break;
				case UIHeartsOfTheFaithfulChoiceMenuController.Types.Strength:
					AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/swords_appear", base.gameObject);
					BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "strength", "strength");
					break;
				}
				Delay += 0.1f;
				yield return new WaitForSeconds(0.8f - Delay);
				continue;
			}
			break;
		}
	}
}
