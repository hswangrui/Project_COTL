using System;
using System.Collections;
using System.Runtime.CompilerServices;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

public class Interaction_Tarot : Interaction
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass27_0
	{
		public Interaction_Tarot _003C_003E4__this;

		public TarotCards.TarotCard card1;

		public TarotCards.TarotCard card2;

		internal void _003CDoRoutineRoutine_003Eb__3(TarotCards.TarotCard card)
		{
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.BackToIdleRoutine(card, 0f));
			DataManager.Instance.PlayerRunTrinkets.Remove(_003CDoRoutineRoutine_003Eg__GetOther_007C2(card));
		}

		internal void _003CDoRoutineRoutine_003Eb__0()
		{
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.BackToIdleRoutine(card1, 0f));
		}

		internal void _003CDoRoutineRoutine_003Eb__1()
		{
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.BackToIdleRoutine(card2, 0f));
		}

		internal TarotCards.TarotCard _003CDoRoutineRoutine_003Eg__GetOther_007C2(TarotCards.TarotCard card)
		{
			if (card == card1)
			{
				return card2;
			}
			return card1;
		}
	}

	private string sSummonTrinket;

	public bool Activated;

	public SimpleBark simpleBarkBeforeCard;

	public SimpleBark simpleBarkAfterCard;

	public Interaction_SimpleConversation conversation;

	public GameObject tarotcards;

	public Interaction_SimpleConversation relicRiddle;

	public Interaction_SimpleConversation introGiveDeck;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	private int devotionCost;

	private bool _playedSFX;

	public Sprite TarotCardSprite;

	public Transform SpawnPosition;

	private void Start()
	{
		UpdateLocalisation();
		simpleBarkBeforeCard.enabled = DataManager.Instance.HasEncounteredTarot || DungeonSandboxManager.Active;
		simpleBarkAfterCard.enabled = false;
		base.enabled = DataManager.Instance.HasEncounteredTarot || DungeonSandboxManager.Active;
		if (DataManager.Instance.GivenRelicLighthouseRiddle)
		{
			SetRelicRiddleManual();
		}
		else if (DataManager.Instance.OnboardedRelics && !DungeonSandboxManager.Active)
		{
			relicRiddle.OnInteraction += delegate
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/ClauneckRelic", Objectives.CustomQuestTypes.FindClauneckRelic), true);
			};
		}
		conversation.enabled = !DataManager.Instance.HasEncounteredTarot && !DungeonSandboxManager.Active;
		introGiveDeck.enabled = false;
		_playedSFX = false;
	}

	public void SetRelicRiddleManual()
	{
		relicRiddle.ActivateDistance = 1.25f;
		relicRiddle.AutomaticallyInteract = false;
		GameManager.GetInstance().WaitForSeconds(1f, delegate
		{
			relicRiddle.Finished = false;
			relicRiddle.Spoken = false;
		});
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sSummonTrinket = ScriptLocalization.Interactions.TakeTrinket;
	}

	public override void OnEnableInteraction()
	{
		if (!(PlayerFarming.Instance == null))
		{
			base.OnEnableInteraction();
			if (!_playedSFX)
			{
				AudioManager.Instance.PlayOneShot("event:/Stings/tarot_room", base.transform.position);
				_playedSFX = true;
			}
		}
	}

	public override void GetLabel()
	{
		Interactable = true;
		base.Label = (Activated ? "" : sSummonTrinket);
	}

	public void FinishedConversation()
	{
		StartCoroutine(GiveIntroTarots());
	}

	public void TarotCardsGiven()
	{
		StartCoroutine(GiveIntroTarotsDeck());
	}

	private IEnumerator GiveIntroTarotsDeck()
	{
		DataManager.Instance.HasEncounteredTarot = true;
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		UITarotCardsMenuController uITarotCardsMenuController = MonoSingleton<UIManager>.Instance.TarotCardsMenuTemplate.Instantiate();
		uITarotCardsMenuController.Show(TarotCards.DefaultCards);
		yield return uITarotCardsMenuController.YieldUntilHidden();
		LetterBox.Hide();
		HUD_Manager.Instance.Show(0);
		GameManager.GetInstance().CameraResetTargetZoom();
		c.DisablePlayerLook = false;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		pFarming.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		StopAllCoroutines();
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			simpleBarkBeforeCard.Close();
			simpleBarkBeforeCard.enabled = false;
			base.OnInteract(state);
			Activated = true;
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
			DoRoutine();
		}
	}

	private IEnumerator CentrePlayer()
	{
		float Progress = 0f;
		Vector3 StartPosition = PlayerFarming.Instance.state.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * 2f);
			if (num < 0.5f)
			{
				PlayerFarming.Instance.state.transform.position = Vector3.Lerp(StartPosition, base.transform.position + Vector3.down, Mathf.SmoothStep(0f, 1f, Progress));
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

	private TarotCards.TarotCard GetCard()
	{
		TarotCards.TarotCard tarotCard;
		if (!DataManager.Instance.FirstTarot && TarotCards.DrawRandomCard() != null && !DungeonSandboxManager.Active)
		{
			DataManager.Instance.FirstTarot = true;
			tarotCard = new TarotCards.TarotCard(TarotCards.Card.Lovers1, 0);
		}
		else
		{
			tarotCard = TarotCards.DrawRandomCard();
			if (tarotCard != null && tarotCard.CardType == TarotCards.Card.Spider && EquipmentManager.IsPoisonWeapon(DataManager.Instance.CurrentWeapon))
			{
				tarotCard = TarotCards.DrawRandomCard();
			}
		}
		if (tarotCard != null)
		{
			DataManager.Instance.PlayerRunTrinkets.Add(tarotCard);
		}
		return tarotCard;
	}

	private IEnumerator GiveIntroTarots()
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 4f);
		DoRoutine();
	}

	private IEnumerator DoRoutineRoutine()
	{
		_003C_003Ec__DisplayClass27_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass27_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		HUD_Manager.Instance.Hide(false, 0);
		GameManager.GetInstance().CameraSetTargetZoom(4f);
		LetterBox.Show(false);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.state.facingAngle = -90f;
		c = CameraFollowTarget.Instance;
		c.DisablePlayerLook = true;
		pFarming = PlayerFarming.Instance;
		StartCoroutine(CentrePlayer());
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", base.gameObject);
		pFarming.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
		pFarming.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0f);
		yield return new WaitForSeconds(0.1f);
		GameManager.GetInstance().CameraSetTargetZoom(6f);
		if (tarotcards != null)
		{
			tarotcards.SetActive(false);
		}
		CS_0024_003C_003E8__locals0.card1 = GetCard();
		CS_0024_003C_003E8__locals0.card2 = GetCard();
		if (CS_0024_003C_003E8__locals0.card1 != null && CS_0024_003C_003E8__locals0.card2 != null)
		{
			UITarotChoiceOverlayController tarotChoiceOverlayInstance = MonoSingleton<UIManager>.Instance.ShowTarotChoice(CS_0024_003C_003E8__locals0.card1, CS_0024_003C_003E8__locals0.card2);
			UITarotChoiceOverlayController uITarotChoiceOverlayController = tarotChoiceOverlayInstance;
			uITarotChoiceOverlayController.OnTarotCardSelected = (Action<TarotCards.TarotCard>)Delegate.Combine(uITarotChoiceOverlayController.OnTarotCardSelected, (Action<TarotCards.TarotCard>)delegate(TarotCards.TarotCard card)
			{
				CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003C_003E4__this.BackToIdleRoutine(card, 0f));
				DataManager.Instance.PlayerRunTrinkets.Remove(CS_0024_003C_003E8__locals0._003CDoRoutineRoutine_003Eg__GetOther_007C2(card));
			});
			UITarotChoiceOverlayController uITarotChoiceOverlayController2 = tarotChoiceOverlayInstance;
			uITarotChoiceOverlayController2.OnHidden = (Action)Delegate.Combine(uITarotChoiceOverlayController2.OnHidden, (Action)delegate
			{
				tarotChoiceOverlayInstance = null;
			});
		}
		else if (CS_0024_003C_003E8__locals0.card1 != null || CS_0024_003C_003E8__locals0.card2 != null)
		{
			if (CS_0024_003C_003E8__locals0.card1 != null)
			{
				UITrinketCards.Play(CS_0024_003C_003E8__locals0.card1, delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003C_003E4__this.BackToIdleRoutine(CS_0024_003C_003E8__locals0.card1, 0f));
				});
			}
			else if (CS_0024_003C_003E8__locals0.card2 != null)
			{
				UITrinketCards.Play(CS_0024_003C_003E8__locals0.card2, delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003C_003E4__this.BackToIdleRoutine(CS_0024_003C_003E8__locals0.card2, 0f));
				});
			}
		}
		else
		{
			int i = -1;
			while (true)
			{
				int num = i + 1;
				i = num;
				if (num > 25)
				{
					break;
				}
				AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
				CameraManager.shakeCamera(UnityEngine.Random.Range(0.4f, 0.6f));
				PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f);
				pickUp.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
				pickUp.MagnetDistance = 3f;
				pickUp.CanStopFollowingPlayer = false;
				yield return new WaitForSeconds(0.01f);
			}
			yield return new WaitForSeconds(1f);
			StartCoroutine(BackToIdleRoutine(null, 0f));
		}
		if (!DataManager.Instance.HasEncounteredTarot)
		{
			introGiveDeck.enabled = true;
		}
	}

	private IEnumerator BackToIdleRoutine(TarotCards.TarotCard card, float delay)
	{
		LetterBox.Hide();
		HUD_Manager.Instance.Show(0);
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
		GameManager.GetInstance().CameraResetTargetZoom();
		c.DisablePlayerLook = false;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		yield return null;
		pFarming.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
		pFarming.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		simpleBarkAfterCard.enabled = true;
		StopAllCoroutines();
		GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine(card, delay));
	}

	private IEnumerator DelayEffectsRoutine(TarotCards.TarotCard card, float delay)
	{
		yield return new WaitForSeconds(0.2f + delay);
		if (card != null)
		{
			TrinketManager.AddTrinket(card);
		}
	}
}
