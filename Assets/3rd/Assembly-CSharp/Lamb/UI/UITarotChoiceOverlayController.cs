using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Lamb.UI
{
	public class UITarotChoiceOverlayController : UIMenuBase
	{
		public Action<TarotCards.TarotCard> OnTarotCardSelected;

		[Header("Buttons")]
		[SerializeField]
		private MMButton _button1;

		[SerializeField]
		private MMButton _button2;

		[Header("Cards")]
		[SerializeField]
		private UITrinketCards _uiCard1;

		[SerializeField]
		private UITrinketCards _uiCard2;

		private TarotCards.TarotCard _card1;

		private TarotCards.TarotCard _card2;

		private TarotCards.TarotCard _chosenCard;

		private UITrinketCards _chosenUICard;

		public void Show(TarotCards.TarotCard card1, TarotCards.TarotCard card2, bool instant = false)
		{
			_card1 = card1;
			_card2 = card2;
			_uiCard1.Play(_card1);
			_uiCard2.Play(_card2);
			Show(instant);
		}

		private UITrinketCards GetOtherCard(UITrinketCards card)
		{
			if (card == _uiCard1)
			{
				return _uiCard2;
			}
			return _uiCard1;
		}

		protected override void OnShowStarted()
		{
			_button1.onClick.AddListener(delegate
			{
				_chosenUICard = _uiCard1;
				FinalizeSelection(_card1);
			});
			_button2.onClick.AddListener(delegate
			{
				_chosenUICard = _uiCard2;
				FinalizeSelection(_card2);
			});
			_button1.Interactable = false;
			_button2.Interactable = false;
		}

		protected override IEnumerator DoShowAnimation()
		{
			yield return new WaitForSecondsRealtime(1.5f);
			OverrideDefault(_button1);
			SetActiveStateForMenu(true);
		}

		private void FinalizeSelection(TarotCards.TarotCard card)
		{
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			_chosenCard = card;
			Hide();
		}

		protected override IEnumerator DoHideAnimation()
		{
			UIManager.PlayAudio("event:/ui/close_menu");
			_chosenUICard.GetComponentInChildren<UIWeaponCard>().InformationBox.DOFade(0f, 0.2f);
			_chosenUICard.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			UITrinketCards otherCard = GetOtherCard(_chosenUICard);
			otherCard.GetComponentInChildren<UIWeaponCard>().InformationBox.DOFade(0f, 0.2f);
			otherCard.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
			Action<TarotCards.TarotCard> onTarotCardSelected = OnTarotCardSelected;
			if (onTarotCardSelected != null)
			{
				onTarotCardSelected(_chosenCard);
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
