using System;
using System.Collections;
using FMOD.Studio;
using I2.Loc;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lamb.UI.UpgradeMenu
{
	public class RatauHomeShop : Interaction
	{
		[FormerlySerializedAs("shopMenuController")]
		[FormerlySerializedAs("shopMenu")]
		[SerializeField]
		private UIUpgradeShopMenuController _shopMenuControllerTemplate;

		public Transform DevotionSpawnPosition;

		private string sBuyCrownAbility;

		private EventInstance receiveLoop;

		private void Start()
		{
			UpdateLocalisation();
		}

		public override void UpdateLocalisation()
		{
			base.UpdateLocalisation();
			sBuyCrownAbility = ScriptLocalization.Interactions.BuyCrownAbility;
		}

		public override void GetLabel()
		{
			base.Label = sBuyCrownAbility;
		}

		public override void OnInteract(StateMachine state)
		{
			base.OnInteract(state);
			Play();
		}

		public void Play()
		{
			Time.timeScale = 0f;
			HUD_Manager.Instance.Hide(false, 0);
			UIUpgradeShopMenuController uIUpgradeShopMenuController = _shopMenuControllerTemplate.Instantiate();
			uIUpgradeShopMenuController.Show();
			uIUpgradeShopMenuController.OnHide = (Action)Delegate.Combine(uIUpgradeShopMenuController.OnHide, (Action)delegate
			{
				HUD_Manager.Instance.Show(0);
			});
			uIUpgradeShopMenuController.OnHidden = (Action)Delegate.Combine(uIUpgradeShopMenuController.OnHidden, (Action)delegate
			{
				Time.timeScale = 1f;
				GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
			});
			uIUpgradeShopMenuController.OnUpgradeChosen = (Action<UpgradeSystem.Type>)Delegate.Combine(uIUpgradeShopMenuController.OnUpgradeChosen, (Action<UpgradeSystem.Type>)delegate(UpgradeSystem.Type type)
			{
				UpgradeSystem.UnlockAbility(type);
				StartCoroutine(GetAbilityRoutine(type));
			});
		}

		private IEnumerator GetAbilityRoutine(UpgradeSystem.Type Type)
		{
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
			PlayerFarming.Instance.GoToAndStop(new Vector3(DevotionSpawnPosition.position.x, DevotionSpawnPosition.position.y - 2f, 0f));
			while (PlayerFarming.Instance.GoToAndStopping)
			{
				yield return null;
			}
			GameManager.GetInstance().AddToCamera(DevotionSpawnPosition.gameObject);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "gameover-fast", true);
			AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
			receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
			float Progress = 0f;
			float Duration = 3.6666667f;
			float StartingZoom = GameManager.GetInstance().CamFollowTarget.distance;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime);
				if (!(num < Duration - 0.5f))
				{
					break;
				}
				GameManager.GetInstance().CameraSetZoom(Mathf.Lerp(StartingZoom, 4f, Progress / Duration));
				if (Time.frameCount % 10 == 0)
				{
					SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, DevotionSpawnPosition.position, Color.black, null, 0.2f);
				}
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "specials/special-activate", false);
			yield return new WaitForSeconds(1f);
			AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
			receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "idle", true);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			GameManager.GetInstance().OnConversationEnd();
			UITutorialOverlayController TutorialOverlay = null;
			switch (Type)
			{
			case UpgradeSystem.Type.Ability_BlackHeart:
				if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.DarknessWithin))
				{
					TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.DarknessWithin);
				}
				break;
			case UpgradeSystem.Type.Ability_Resurrection:
				if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Resurrection))
				{
					TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Resurrection);
				}
				break;
			case UpgradeSystem.Type.Ability_TeleportHome:
				if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Omnipresence))
				{
					TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Omnipresence);
				}
				break;
			case UpgradeSystem.Type.Ability_Eat:
				if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.TheHunger))
				{
					TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.TheHunger);
				}
				break;
			}
			while (TutorialOverlay != null)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
}
