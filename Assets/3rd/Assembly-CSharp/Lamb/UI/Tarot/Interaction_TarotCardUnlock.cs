using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI.Tarot
{
	public class Interaction_TarotCardUnlock : Interaction
	{
		private string sSummonTrinket;

		private string sNoAvailableTrinkets;

		public bool Activated;

		public SpriteRenderer sprite;

		private PlayerFarming pFarming;

		private CameraFollowTarget c;

		private int devotionCost;

		public GameObject Menu;

		public TarotCards.Card CardOverride = TarotCards.Card.Count;

		public Transform SpawnPosition;

		private TarotCards.TarotCard DrawnCard;

		private bool Activating;

		private void Start()
		{
			UpdateLocalisation();
		}

		public override void UpdateLocalisation()
		{
			base.UpdateLocalisation();
			sSummonTrinket = ScriptLocalization.Interactions.PickUp;
		}

		public override void GetLabel()
		{
			Interactable = true;
			base.Label = (Activated ? "" : sSummonTrinket);
		}

		public override void OnInteract(StateMachine state)
		{
			if (!Activated)
			{
				base.OnInteract(state);
				Activated = true;
				state.CURRENT_STATE = StateMachine.State.InActive;
				DoRoutine();
			}
		}

		private IEnumerator CentrePlayer()
		{
			float Progress = 0f;
			Vector3 StartPosition = state.transform.position;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime * 2f);
				if (num < 0.5f)
				{
					state.transform.position = Vector3.Lerp(StartPosition, base.transform.position + Vector3.down, Mathf.SmoothStep(0f, 1f, Progress));
					yield return null;
					continue;
				}
				break;
			}
		}

		private void DoRoutine()
		{
			StartCoroutine(DoRoutineRoutine());
		}

		private IEnumerator DoRoutineRoutine()
		{
			base.transform.DOMove(PlayerFarming.Instance.CameraBone.transform.position + new Vector3(0f, 0f, -1f), 0.5f).SetEase(Ease.InOutBack).OnComplete(delegate
			{
				for (int j = 0; j < base.transform.childCount; j++)
				{
					base.transform.GetChild(j).gameObject.SetActive(false);
				}
			});
			HUD_Manager.Instance.Hide(false, 0);
			base.transform.DOScale(0f, 1f);
			GameManager.GetInstance().CameraSetTargetZoom(4f);
			LetterBox.Show(false);
			state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			state.facingAngle = -90f;
			c = CameraFollowTarget.Instance;
			c.DisablePlayerLook = true;
			pFarming = state.GetComponent<PlayerFarming>();
			StartCoroutine(CentrePlayer());
			AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", base.gameObject);
			pFarming.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
			pFarming.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0f);
			NotificationCentre.Instance.PlayGenericNotification("UI/TarotMenu/NewTarotCardUnlocked");
			yield return new WaitForSeconds(1f);
			GameManager.GetInstance().CameraSetTargetZoom(6f);
			for (int i = 0; i < base.transform.childCount; i++)
			{
				base.transform.GetChild(i).gameObject.SetActive(false);
			}
			OpenMenu();
		}

		private void OpenMenu()
		{
			HUD_Manager.Instance.Hide(false, 0);
			GameManager.GetInstance().CameraSetOffset(new Vector3(-3f, 0f, 0f));
			UITarotCardsMenuController uITarotCardsMenuController = MonoSingleton<UIManager>.Instance.TarotCardsMenuTemplate.Instantiate();
			uITarotCardsMenuController.Show(CardOverride);
			uITarotCardsMenuController.OnHide = (Action)Delegate.Combine(uITarotCardsMenuController.OnHide, (Action)delegate
			{
				HUD_Manager.Instance.Show(0);
			});
			uITarotCardsMenuController.OnHidden = (Action)Delegate.Combine(uITarotCardsMenuController.OnHidden, (Action)delegate
			{
				GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
				BackToIdle();
			});
			DrawnCard = new TarotCards.TarotCard(CardOverride, 0);
		}

		private void CallBack()
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			Activating = false;
		}

		private void BackToIdle()
		{
			StartCoroutine(BackToIdleRoutine());
		}

		private IEnumerator BackToIdleRoutine()
		{
			LetterBox.Hide();
			HUD_Manager.Instance.Show(0);
			AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
			GameManager.GetInstance().CameraResetTargetZoom();
			c.DisablePlayerLook = false;
			state.CURRENT_STATE = StateMachine.State.Idle;
			yield return null;
			pFarming.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
			pFarming.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			StopAllCoroutines();
			GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine());
		}

		private IEnumerator DelayEffectsRoutine()
		{
			yield return new WaitForSeconds(0.2f);
			if (LocationManager.LocationIsDungeon(PlayerFarming.Location))
			{
				TrinketManager.AddTrinket(DrawnCard);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
