using Lamb.UI.Alerts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lamb.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class TarotCardItemBase : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		[SerializeField]
		protected TarotCardAnimator _tarotCard;

		[SerializeField]
		protected TarotCardAlertBase _alert;

		[SerializeField]
		protected MMSelectable _selectable;

		private TarotCards.TarotCard _card;

		public TarotCards.TarotCard Card
		{
			get
			{
				return _card;
			}
		}

		public TarotCards.Card Type
		{
			get
			{
				return _card.CardType;
			}
		}

		public RectTransform RectTransform { get; private set; }

		public TarotCardAnimator TarotCard
		{
			get
			{
				return _tarotCard;
			}
		}

		public MMSelectable Selectable
		{
			get
			{
				return _selectable;
			}
		}

		public TarotCardAlertBase Alert
		{
			get
			{
				return _alert;
			}
		}

		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
		}

		public virtual void Configure(TarotCards.TarotCard card)
		{
			_card = card;
			_alert.Configure(card.CardType);
			_tarotCard.Configure(card);
		}

		public virtual void Configure(TarotCards.Card card)
		{
			Configure(new TarotCards.TarotCard(card, 0));
		}

		public virtual void OnSelect(BaseEventData eventData)
		{
			_alert.TryRemoveAlert();
		}

		public virtual void OnDeselect(BaseEventData eventData)
		{
		}
	}
}
