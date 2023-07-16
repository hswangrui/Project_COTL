using System.Collections;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;

public class RatauGiveHealing : BaseMonoBehaviour
{
	public Interaction_SimpleConversation FollowUpConversation;

	public CanvasGroup ControlsHUD;

	public spineChangeAnimationSimple ChangeAnimation;

	public SkeletonAnimation RatauSpine;

	private EventInstance receiveLoop;

	private void OnEnable()
	{
		ControlsHUD.alpha = 0f;
		FollowUpConversation.gameObject.SetActive(false);
	}

	private void Start()
	{
		DataManager.Instance.EnabledSword = false;
		DataManager.Instance.EnabledSpells = false;
	}

	public void GiveHealing()
	{
		DataManager.Instance.EnabledHealing = true;
		StartCoroutine(WaitForHealing());
	}

	private IEnumerator WaitForHealing()
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		yield return null;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "gameover-fast", true);
		RatauSpine.AnimationState.SetAnimation(0, "warning", true);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
		receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
		float Progress = 0f;
		float Duration = 3.6666667f;
		float StartingZoom = GameManager.GetInstance().CamFollowTarget.distance;
		float num;
		while (true)
		{
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration - 0.5f))
			{
				break;
			}
			GameManager.GetInstance().CameraSetZoom(Mathf.Lerp(StartingZoom, 4f, Progress / Duration));
			if (Time.frameCount % 10 == 0)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, base.gameObject.transform.position, Color.black, null, 0.2f);
			}
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		RatauSpine.AnimationState.SetAnimation(0, "idle", true);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "specials/special-activate", false);
		yield return new WaitForSeconds(1f);
		UIAbilityUnlock.Play(UIAbilityUnlock.Ability.Heal);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "idle", true);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
		receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
		GameManager.GetInstance().OnConversationEnd();
		HUD_Manager.Instance.Show(0);
		Progress = 0f;
		Duration = 0.5f;
		while (true)
		{
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ControlsHUD.alpha = Mathf.Lerp(0f, 1f, Progress / Duration);
			yield return null;
		}
		HealthPlayer healthPlayer = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		Debug.Log(healthPlayer.HP + healthPlayer.SpiritHearts + " - " + (healthPlayer.totalHP + healthPlayer.TotalSpiritHearts));
		num = healthPlayer.TotalSpiritHearts;
		if (!0f.Equals(num))
		{
			if (1f.Equals(num))
			{
				healthPlayer.SpiritHearts = healthPlayer.TotalSpiritHearts - 1f;
				healthPlayer.HP = healthPlayer.totalHP - 1f;
			}
			else
			{
				healthPlayer.SpiritHearts = healthPlayer.TotalSpiritHearts - 2f;
				healthPlayer.HP = healthPlayer.totalHP;
			}
		}
		else
		{
			healthPlayer.HP = healthPlayer.totalHP - 2f;
		}
		while (healthPlayer.HP + healthPlayer.SpiritHearts < healthPlayer.totalHP + healthPlayer.TotalSpiritHearts || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Heal)
		{
			if (!FaithAmmo.CanAfford(FaithAmmo.Total))
			{
				FaithAmmo.Reload();
			}
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		ControlsHUD.alpha = 0f;
		FollowUpConversation.gameObject.SetActive(true);
	}

	public void EndConversation()
	{
		DataManager.Instance.EnabledSword = true;
		DataManager.Instance.EnabledSpells = true;
		RoomLockController.RoomCompleted();
		FaithAmmo.Reload();
		ChangeAnimation.Play();
	}
}
