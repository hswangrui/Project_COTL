using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lamb.UI
{
	public class TarotCardItem_Unlocked : TarotCardItemBase
	{
		private bool _isUnlocked;

		public override void Configure(TarotCards.Card card)
		{
			base.Configure(card);
			_isUnlocked = TarotCards.IsUnlocked(card);
			if (_isUnlocked)
			{
				base.TarotCard.Spine.color = Color.white;
				_tarotCard.SetStaticFront();
			}
			else
			{
				base.TarotCard.Spine.color = new Color(0f, 1f, 1f, 1f);
				_tarotCard.SetStaticBack();
			}
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (!_isUnlocked)
			{
				base.TarotCard.Spine.DOKill();
				base.TarotCard.Spine.DOColor(Color.white, 0.25f).SetUpdate(true);
			}
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			if (!_isUnlocked)
			{
				base.TarotCard.Spine.DOKill();
				base.TarotCard.Spine.DOColor(new Color(0f, 1f, 1f, 1f), 0.25f).SetUpdate(true);
			}
		}

		public void ForceIncognitoMode()
		{
			_alert.gameObject.SetActive(false);
			base.TarotCard.Spine.color = new Color(0f, 1f, 1f, 1f);
		}

		public void AnimateIncognitoOut()
		{
			base.TarotCard.Spine.DOColor(Color.white, 0.25f).SetUpdate(true);
		}

		public IEnumerator ShowAlert()
		{
			Vector3 one = Vector3.one;
			_alert.transform.localScale = Vector3.zero;
			_alert.transform.DOScale(one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
			_alert.gameObject.SetActive(true);
			yield return new WaitForSecondsRealtime(0.5f);
		}
	}
}
