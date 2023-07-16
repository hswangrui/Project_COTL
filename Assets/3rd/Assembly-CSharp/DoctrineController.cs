using System;
using System.Collections;
using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.Rituals;
using Lamb.UI.SermonWheelOverlay;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

public class DoctrineController : MonoBehaviour
{
	private Interaction_TempleAltar TempleAltar;

	private InventoryItem.ITEM_TYPE _currency;

	private EventInstance loop;

	public static Action OnUnlockedFirstRitual;

	private void OnEnable()
	{
		TempleAltar = GetComponent<Interaction_TempleAltar>();
		UIPlayerUpgradesMenuController.OnDoctrineUnlockSelected = (Action)Delegate.Combine(UIPlayerUpgradesMenuController.OnDoctrineUnlockSelected, new Action(Play));
		UIPlayerUpgradesMenuController.OnCrystalDoctrineUnlockSelected = (Action)Delegate.Combine(UIPlayerUpgradesMenuController.OnCrystalDoctrineUnlockSelected, new Action(PlayCrystalDoctrine));
	}

	private void OnDisable()
	{
		UIPlayerUpgradesMenuController.OnDoctrineUnlockSelected = (Action)Delegate.Remove(UIPlayerUpgradesMenuController.OnDoctrineUnlockSelected, new Action(Play));
		UIPlayerUpgradesMenuController.OnCrystalDoctrineUnlockSelected = (Action)Delegate.Remove(UIPlayerUpgradesMenuController.OnCrystalDoctrineUnlockSelected, new Action(PlayCrystalDoctrine));
	}

	private void Play()
	{
		_currency = InventoryItem.ITEM_TYPE.DOCTRINE_STONE;
		StartCoroutine(PlayIE());
	}

	private void PlayCrystalDoctrine()
	{
		_currency = InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE;
		StartCoroutine(PlayIE());
	}

	private IEnumerator PlayIE()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "sermons/doctrine-start", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "sermons/doctrine-loop", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/sermon/start_sermon", PlayerFarming.Instance.gameObject);
		AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);
		loop = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", PlayerFarming.Instance.gameObject, true, false);
		StartCoroutine(TempleAltar.CentrePlayer());
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 12f);
		float t = Time.time;
		yield return StartCoroutine(Interaction_TempleAltar.Instance.FollowersEnterForSermonRoutine());
		if (Time.time - t < 0.5f)
		{
			yield return new WaitForSeconds(1f);
		}
		if (!DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.Special_Bonfire))
		{
			TempleAltar.SermonCategory = SermonCategory.Special;
			StartCoroutine(DeclareDoctrine());
			AudioManager.Instance.PlayOneShot("event:/sermon/select_sermon", PlayerFarming.Instance.gameObject);
			yield break;
		}
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 11f);
		AudioManager.Instance.PlayOneShot("event:/sermon/sermon_menu_appear", PlayerFarming.Instance.gameObject);
		SermonCategory sermonCategory = SermonCategory.None;
		UISermonWheelController uISermonWheelController = MonoSingleton<UIManager>.Instance.SermonWheelTemplate.Instantiate();
		uISermonWheelController.Show(_currency);
		uISermonWheelController.OnItemChosen = (Action<SermonCategory>)Delegate.Combine(uISermonWheelController.OnItemChosen, (Action<SermonCategory>)delegate(SermonCategory chosenCategory)
		{
			Debug.Log(string.Format("Chose category {0}", chosenCategory).Colour(Color.yellow));
			sermonCategory = chosenCategory;
		});
		yield return uISermonWheelController.YieldUntilHide();
		if (sermonCategory != 0)
		{
			TempleAltar.SermonCategory = sermonCategory;
			StartCoroutine(DeclareDoctrine());
			AudioManager.Instance.PlayOneShot("event:/sermon/select_sermon", PlayerFarming.Instance.gameObject);
		}
		else
		{
			yield return CancelDoctrineDeclaration();
		}
	}

	private IEnumerator DeclareDoctrine()
	{
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 1f));
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 7f);
		PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
		PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
		PlayerFarming.Instance.Spine.Update(Time.deltaTime);
		yield return new WaitForSeconds(0.6f);
		TempleAltar.PulseDisplacementObject(TempleAltar.state.transform.position);
		yield return new WaitForSeconds(0.4f);
		ChurchFollowerManager.Instance.StartSermonEffectClean();
		AudioManager.Instance.PlayOneShot("event:/sermon/upgrade_menu_appear");
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(TempleAltar.AskQuestionRoutine(_currency));
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "sermons/declare-doctrine", false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		if (_currency == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
		{
			PlayerDoctrineStone.Instance.Spine.Skeleton.SetSkin("crystal");
		}
		PlayerDoctrineStone.Instance.CanvasGroup.alpha = 1f;
		PlayerDoctrineStone.Instance.Spine.AnimationState.SetAnimation(0, "declare-doctrine", false);
		PlayerDoctrineStone.Instance.Spine.enabled = true;
		AudioManager.Instance.PlayOneShot("event:/temple_key/fragment_move", PlayerFarming.Instance.gameObject);
		yield return new WaitForSeconds(2.9f);
		if (DoctrineUpgradeSystem.UnlockedUpgrades.Count > 0)
		{
			DoctrineUpgradeSystem.DoctrineType type = DoctrineUpgradeSystem.UnlockedUpgrades[DoctrineUpgradeSystem.UnlockedUpgrades.Count - 1];
			if (DoctrineUpgradeSystem.ShowDoctrineTutorialForType(type) == DoctrineUpgradeSystem.DoctrineCategory.Ritual)
			{
				DoctrineUpgradeSystem.RitualForDoctrineUpgrade(type);
				yield return new WaitForSecondsRealtime(0.5f);
				GameManager.GetInstance().CameraSetOffset(Vector3.left * 2.25f);
				UIRitualsMenuController ritualsMenu = MonoSingleton<UIManager>.Instance.RitualsMenuTemplate.Instantiate();
				ritualsMenu.Show(UpgradeSystem.UnlockedUpgrades[UpgradeSystem.UnlockedUpgrades.Count - 1]);
				UIRitualsMenuController uIRitualsMenuController = ritualsMenu;
				uIRitualsMenuController.OnHidden = (Action)Delegate.Combine(uIRitualsMenuController.OnHidden, (Action)delegate
				{
					ritualsMenu = null;
				});
				while (ritualsMenu != null)
				{
					yield return null;
				}
				yield return new WaitForSecondsRealtime(0.25f);
				GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 1f));
			}
		}
		ChurchFollowerManager.Instance.EndSermonEffectClean();
		AudioManager.Instance.PlayOneShot("event:/sermon/end_sermon", PlayerFarming.Instance.gameObject);
		loop.stop(STOP_MODE.ALLOWFADEOUT);
		AudioManager.Instance.StopLoop(loop);
		yield return new WaitForSeconds(1f / 3f);
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
			allBrain.GetWillLevelUp(FollowerBrain.AdorationActions.Sermon);
			allBrain.AddAdoration(FollowerBrain.AdorationActions.Sermon, delegate
			{
				if (f.Brain.CurrentTaskType == FollowerTaskType.AttendTeaching)
				{
					f.Brain.CurrentTask.StartAgain(f);
				}
			});
			StartCoroutine(TempleAltar.DelayFollowerReaction(allBrain, UnityEngine.Random.Range(0.1f, 0.5f)));
		}
		PlayerDoctrineStone.Instance.Spine.Skeleton.SetSkin("normal");
		PlayerDoctrineStone.Instance.CanvasGroup.alpha = 0f;
		PlayerDoctrineStone.Instance.Spine.enabled = false;
		TempleAltar.ResetSprite();
		AudioManager.Instance.PlayOneShot("event:/sermon/book_put_down", PlayerFarming.Instance.gameObject);
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
        PlayerFarming.Instance.Spine.UseDeltaTime = true;
        if (_currency == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
		{
			UpgradeSystem.AddCooldown(UpgradeSystem.Type.Ritual_CrystalDoctrine, 1200f);
			Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE, -1);
		}
		else
		{
			UpgradeSystem.AddCooldown(UpgradeSystem.PrimaryRitual1, 1200f);
			PlayerDoctrineStone.Instance.CompletedDoctrineStones--;
		}
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.DeclareDoctrine);
		DoctrineUpgradeSystem.DoctrineCategory doctrineCategory = DoctrineUpgradeSystem.ShowDoctrineTutorialForType(Interaction_TempleAltar.DoctrineUnlockType);
		Debug.Log("category: " + doctrineCategory);
		switch (doctrineCategory)
		{
		case DoctrineUpgradeSystem.DoctrineCategory.Trait:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Traits))
			{
				MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Traits);
			}
			break;
		case DoctrineUpgradeSystem.DoctrineCategory.FollowerAction:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.FollowerAction))
			{
				MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.FollowerAction);
			}
			break;
		case DoctrineUpgradeSystem.DoctrineCategory.Ritual:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Rituals))
			{
				Action onUnlockedFirstRitual = OnUnlockedFirstRitual;
				if (onUnlockedFirstRitual != null)
				{
					onUnlockedFirstRitual();
				}
			}
			break;
		}
		yield return new WaitForSeconds(1f);
		Interaction_TempleAltar.Instance.OnInteract(PlayerFarming.Instance.state);
	}

	private IEnumerator CancelDoctrineDeclaration()
	{
		ChurchFollowerManager.Instance.EndSermonEffectClean();
		AudioManager.Instance.PlayOneShot("event:/sermon/end_sermon", PlayerFarming.Instance.gameObject);
		loop.stop(STOP_MODE.ALLOWFADEOUT);
		AudioManager.Instance.StopLoop(loop);
		yield return new WaitForSeconds(1f / 3f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-stop-nobook", 0, false);
		yield return new WaitForSeconds(0.7f);
		TempleAltar.ResetSprite();
		AudioManager.Instance.PlayOneShot("event:/sermon/book_put_down", PlayerFarming.Instance.gameObject);
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
        PlayerFarming.Instance.Spine.UseDeltaTime = true;
        Interaction_TempleAltar.Instance.OnInteract(PlayerFarming.Instance.state);
	}
}
