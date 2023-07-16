using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Lamb.UI
{
	public class UIFleeceTarotRewardOverlayController : UIMenuBase
	{
		[Header("Cards")]
		[SerializeField]
		private UITrinketCards _uiCard1;

		[SerializeField]
		private UITrinketCards _uiCard2;

		[SerializeField]
		private UITrinketCards _uiCard3;

		[SerializeField]
		private UITrinketCards _uiCard4;

		[Header("Misc")]
		[SerializeField]
		private GameObject _controlPrompts;

		private TarotCards.TarotCard _card1;

		private TarotCards.TarotCard _card2;

		private TarotCards.TarotCard _card3;

		private TarotCards.TarotCard _card4;

		public void Show(TarotCards.TarotCard card1, TarotCards.TarotCard card2, TarotCards.TarotCard card3, TarotCards.TarotCard card4, bool instant = false)
		{
			_card1 = card1;
			_card2 = card2;
			_card3 = card3;
			_card4 = card4;
			_uiCard1.enabled = false;
			_uiCard2.enabled = false;
			_uiCard3.enabled = false;
			_uiCard4.enabled = false;
			_controlPrompts.SetActive(false);
			Show(instant);
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_card1 != null)
			{
				_uiCard1.enabled = true;
				_uiCard1.Play(_card1);
				yield return new WaitForSecondsRealtime(0.2f);
			}
			if (_card2 != null)
			{
				_uiCard2.enabled = true;
				_uiCard2.Play(_card2);
				yield return new WaitForSecondsRealtime(0.2f);
			}
			if (_card3 != null)
			{
				_uiCard3.enabled = true;
				_uiCard3.Play(_card3);
				yield return new WaitForSecondsRealtime(0.2f);
			}
			if (_card4 != null)
			{
				_uiCard4.enabled = true;
				_uiCard4.Play(_card4);
				yield return new WaitForSecondsRealtime(1.5f);
			}
		}

		protected override void OnShowCompleted()
		{
			StartCoroutine(RunMenu());
		}

		private IEnumerator RunMenu()
		{
			_controlPrompts.SetActive(true);
			while (!InputManager.UI.GetAcceptButtonDown() && !InputManager.UI.GetCancelButtonDown())
			{
				yield return null;
			}
			_controlPrompts.SetActive(false);
			Hide();
		}

		protected override IEnumerator DoHideAnimation()
		{
			if (_card1 != null)
			{
				HideCard(_uiCard1);
				yield return new WaitForSecondsRealtime(0.15f);
			}
			if (_card2 != null)
			{
				HideCard(_uiCard2);
				yield return new WaitForSecondsRealtime(0.15f);
			}
			if (_card3 != null)
			{
				HideCard(_uiCard3);
				yield return new WaitForSecondsRealtime(0.15f);
			}
			if (_card4 != null)
			{
				HideCard(_uiCard4);
				yield return new WaitForSecondsRealtime(0.5f);
			}
		}

		private void HideCard(UITrinketCards card)
		{
			card.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
		}

		protected override void OnHideCompleted()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
