using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class TarotInfoCard : UIInfoCardBase<TarotCards.TarotCard>
	{
		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private TarotCardAnimator _tarotCard;

		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemLore;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[SerializeField]
		private RectTransform _cardContainer;

		public TarotCardAnimator TarotCard
		{
			get
			{
				return _tarotCard;
			}
		}

		public RectTransform CardContainer
		{
			get
			{
				return _cardContainer;
			}
		}

		public override void Configure(TarotCards.TarotCard card)
		{
			_tarotCard.Configure(card);
			_itemHeader.text = TarotCards.LocalisedName(card.CardType);
			_itemLore.text = LocalizationManager.GetTranslation(string.Format("TarotCards/{0}/Lore", card.CardType));
			_itemDescription.text = TarotCards.LocalisedDescription(card.CardType);
		}

		protected override void DoShow(bool instant)
		{
			_canvasGroup.DOKill();
			base.RectTransform.DOKill();
			base.RectTransform.anchoredPosition = Vector3.zero;
			if (instant)
			{
				_canvasGroup.alpha = 1f;
				return;
			}
			_canvasGroup.DOFade(1f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
			base.RectTransform.DOShakePosition(0.5f, new Vector3(15f, 0f, 0f)).SetUpdate(true);
		}

		protected override void DoHide(bool instant)
		{
			_canvasGroup.DOKill();
			base.RectTransform.DOKill();
			base.RectTransform.anchoredPosition = Vector3.zero;
			if (instant)
			{
				_canvasGroup.alpha = 0f;
			}
			else
			{
				_canvasGroup.DOFade(0f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
			}
		}
	}
}
