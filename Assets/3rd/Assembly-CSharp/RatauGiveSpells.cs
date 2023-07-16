using System;
using System.Collections;
using FMOD.Studio;
using Lamb.UI;
using Spine.Unity;
using UnityEngine;

public class RatauGiveSpells : BaseMonoBehaviour
{
	public Interaction_SimpleConversation FollowUpConversation;

	public CanvasGroup ControlsHUD;

	public spineChangeAnimationSimple ChangeAnimation;

	public SkeletonAnimation RatauSpine;

	public static RatauGiveSpells Instance;

	private int ShootCount;

	private EventInstance receiveLoop;

	public int DummyCount;

	public static Action OnDummyShot;

	private void OnEnable()
	{
		ControlsHUD.alpha = 0f;
		FollowUpConversation.gameObject.SetActive(false);
		Instance = this;
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Start()
	{
		DataManager.Instance.EnabledSword = false;
	}

	public void GiveSpells()
	{
		DataManager.Instance.EnabledSpells = true;
		HUD_Manager.Instance.Show(0);
		StartCoroutine(WaitForShooting());
	}

	private void OnObjectiveComplete(string GroupID)
	{
		Debug.Log("Group ID: " + GroupID);
		if (GroupID == "Objectives/GroupTitles/RatauGiveCurse")
		{
			ObjectiveManager.OnObjectiveGroupCompleted = (Action<string>)Delegate.Remove(ObjectiveManager.OnObjectiveGroupCompleted, new Action<string>(OnObjectiveComplete));
			StartCoroutine(EndSequenceRoutine());
		}
	}

	private IEnumerator WaitForShooting()
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		yield return null;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "gameover-fast", true);
		RatauSpine.AnimationState.SetAnimation(0, "warning", true);
		AudioManager.Instance.PlayOneShot("event:/dialogue/ratau/ratau_song", RatauSpine.transform.position);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
		receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
		float Progress = 0f;
		float Duration2 = 3.6666667f;
		float StartingZoom = GameManager.GetInstance().CamFollowTarget.distance;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration2 - 0.5f))
			{
				break;
			}
			GameManager.GetInstance().CameraSetZoom(Mathf.Lerp(StartingZoom, 4f, Progress / Duration2));
			if (Time.frameCount % 10 == 0)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, base.gameObject.transform.position, Color.black, null, 0.2f);
			}
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		RatauSpine.AnimationState.SetAnimation(0, "idle", true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "specials/special-activate", false);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
		receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "idle", true);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		GameManager.GetInstance().OnConversationEnd();
		PlayerFarming.Instance.playerSpells.SetSpell(EquipmentType.Fireball, 1);
		DataManager.Instance.CurrentRunCurseLevel++;
		Progress = 0f;
		Duration2 = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			ControlsHUD.alpha = Mathf.Lerp(0f, 1f, Progress / Duration2);
			yield return null;
		}
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.SpecialCombat);
		ObjectiveManager.OnObjectiveGroupCompleted = (Action<string>)Delegate.Combine(ObjectiveManager.OnObjectiveGroupCompleted, new Action<string>(OnObjectiveComplete));
		ObjectiveManager.Add(new Objectives_ShootDummy("Objectives/GroupTitles/RatauGiveCurse"));
		PlayerSpells.OnCastSpell = (Action)Delegate.Combine(PlayerSpells.OnCastSpell, new Action(OnCastSpell));
	}

	public void DummyDestroyed()
	{
		DummyCount++;
		Action onDummyShot = OnDummyShot;
		if (onDummyShot != null)
		{
			onDummyShot();
		}
	}

	private IEnumerator EndSequenceRoutine()
	{
		PlayerSpells.OnCastSpell = (Action)Delegate.Remove(PlayerSpells.OnCastSpell, new Action(OnCastSpell));
		yield return new WaitForSeconds(0.5f);
		ControlsHUD.alpha = 0f;
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.StandardAmbience);
		FollowUpConversation.gameObject.SetActive(true);
	}

	public void EndConversation()
	{
		DataManager.Instance.EnabledSword = true;
		DataManager.Instance.RatauToGiveCurseNextRun = false;
		RoomLockController.RoomCompleted();
		FaithAmmo.Reload();
		ChangeAnimation.Play();
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Fervor))
		{
			MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Fervor);
		}
	}

	private void OnCastSpell()
	{
		StartCoroutine(DelayReload());
		RatauSpine.AnimationState.SetAnimation(0, "warning", false);
		RatauSpine.AnimationState.AddAnimation(0, "idle", true, 0f);
	}

	private IEnumerator DelayReload()
	{
		yield return new WaitForSeconds(0.3f);
		if (FaithAmmo.Ammo < FaithAmmo.Total * 0.3f)
		{
			FaithAmmo.Reload();
		}
	}

	private void OnDestroy()
	{
		PlayerSpells.OnCastSpell = (Action)Delegate.Remove(PlayerSpells.OnCastSpell, new Action(OnCastSpell));
		ObjectiveManager.OnObjectiveGroupCompleted = (Action<string>)Delegate.Remove(ObjectiveManager.OnObjectiveGroupCompleted, new Action<string>(OnObjectiveComplete));
	}
}
