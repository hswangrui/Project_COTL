using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using I2.Loc;
using src.Extensions;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UITarotCardsMenuController : UIMenuBase
	{
		private const string kUnlockAnimationState = "Unlock";

		private const string kShowUnlockAnimationState = "Show Unlock";

		[Header("Tarot Cards Menu")]
		[SerializeField]
		private TarotCardItem_Unlocked _tarotCardItemTemplate;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private TextMeshProUGUI _collectedText;

		[SerializeField]
		private TarotInfoCardController _tarotInfoCardController;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[Header("Reveal Sequence")]
		[SerializeField]
		private CanvasGroup _unlockHeaderCanvasGroup;

		[SerializeField]
		private RectTransform _front;

		private TarotCards.TarotCard _showCard = new TarotCards.TarotCard(TarotCards.Card.Count, 0);

		private TarotCards.Card[] _showCards = new TarotCards.Card[0];

		private List<TarotCardItem_Unlocked> _items = new List<TarotCardItem_Unlocked>();

		private int _tarotCount;

		public void Show(TarotCards.Card newCard, bool instant = false)
		{
			_showCard = new TarotCards.TarotCard(newCard, 0);
			_showCards = new TarotCards.Card[1] { _showCard.CardType };
			TarotCards.UnlockTrinket(newCard);
			TarotCardAnimator tarotCard = _tarotInfoCardController.Card1.TarotCard;
			tarotCard.Configure(_showCard);
			tarotCard.SetStaticBack();
			_controlPrompts.HideCancelButton();
			Show(instant);
		}

		public void Show(TarotCards.Card[] cards, bool instant = false)
		{
			TarotCards.UnlockTrinkets(cards);
			_showCards = cards;
			_controlPrompts.HideCancelButton();
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			UIManager.PlayAudio("event:/ui/open_menu");
			_controlPrompts.HideAcceptButton();
			_scrollRect.normalizedPosition = Vector2.one;
			foreach (TarotCards.Card allTrinket in DataManager.AllTrinkets)
			{
				TarotCardItem_Unlocked tarotCardItem_Unlocked = GameObjectExtensions.Instantiate(_tarotCardItemTemplate, _contentContainer);
				tarotCardItem_Unlocked.Configure(allTrinket);
				_items.Add(tarotCardItem_Unlocked);
			}
			if (_showCards.Length == 0)
			{
				_tarotCount = Mathf.Max(DataManager.Instance.PlayerFoundTrinkets.Count, 0);
				OverrideDefault(_items[0].Selectable);
				ActivateNavigation();
			}
			else
			{
				_tarotCount = Mathf.Max(DataManager.Instance.PlayerFoundTrinkets.Count - _showCards.Length, 0);
			}
			_collectedText.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", _tarotCount, DataManager.AllTrinkets.Count));
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_showCard.CardType != TarotCards.Card.Count)
			{
				yield return ShowCardSingle();
			}
			else if (_showCards.Length != 0)
			{
				yield return ShowCardsMultiple();
			}
			else
			{
				yield return _003C_003En__0();
			}
		}

		private IEnumerator ShowCardSingle()
		{
			_scrollRect.vertical = false;
			TarotCardItem_Unlocked target = _items[0];
			foreach (TarotCardItem_Unlocked item in _items)
			{
				if (item.Type == _showCard.CardType)
				{
					target = item;
					break;
				}
			}
			target.TarotCard.SetStaticBack();
			target.Alert.TryRemoveAlert();
			_tarotInfoCardController.enabled = false;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			TarotInfoCard infoCard = _tarotInfoCardController.Card1;
			infoCard.Configure(_showCard);
			infoCard.Hide(true);
			infoCard.CanvasGroup.alpha = 0f;
			infoCard.RectTransform.SetParent(_front);
			infoCard.RectTransform.anchoredPosition = Vector2.zero;
			TarotCardAnimator tarotCardAnimator = infoCard.TarotCard;
			tarotCardAnimator.Configure(_showCard);
			tarotCardAnimator.SetStaticBack();
			tarotCardAnimator.RectTransform.SetParent(_front);
			tarotCardAnimator.RectTransform.anchoredPosition = Vector2.zero;
			Vector2 vector = tarotCardAnimator.RectTransform.localScale;
			tarotCardAnimator.RectTransform.localScale = vector * 0.25f;
			tarotCardAnimator.RectTransform.DOScale(vector, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
			yield return _animator.YieldForAnimation("Unlock");
			yield return new WaitForSecondsRealtime(0.15f);
			UIManager.PlayAudio("event:/tarot/tarot_card_reveal");
			yield return tarotCardAnimator.YieldForReveal();
			yield return new WaitForSecondsRealtime(0.5f);
			Vector2 endValue = _front.InverseTransformPoint(infoCard.CardContainer.TransformPoint(Vector2.zero));
			tarotCardAnimator.RectTransform.DOAnchorPos(endValue, 0.5f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.15f);
			infoCard.CanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.4f);
			tarotCardAnimator.RectTransform.SetParent(infoCard.CardContainer, true);
			_controlPrompts.ShowAcceptButton();
			while (!InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			_controlPrompts.HideAcceptButton();
			infoCard.RectTransform.SetParent(_tarotInfoCardController.transform, true);
			infoCard.RectTransform.DOAnchorPos(Vector2.zero, 0.66f).SetUpdate(true);
			_unlockHeaderCanvasGroup.DOFade(0f, 0.66f).SetUpdate(true);
			yield return _animator.YieldForAnimation("Show Unlock");
			yield return new WaitForSecondsRealtime(0.25f);
			OverrideDefaultOnce(target.Selectable);
			yield return _scrollRect.DoScrollTo(target.RectTransform);
			RectTransform tarotCardContainer = target.TarotCard.RectTransform.parent as RectTransform;
			target.TarotCard.RectTransform.SetParent(_scrollRect.viewport.parent, true);
			target.TarotCard.RectTransform.SetSiblingIndex(target.TarotCard.transform.parent.childCount);
			_tarotCount++;
			UIManager.PlayAudio("event:/player/new_item_sequence_close");
			yield return target.TarotCard.YieldForReveal();
			_collectedText.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", _tarotCount, DataManager.AllTrinkets.Count));
			target.TarotCard.RectTransform.SetParent(tarotCardContainer, true);
			target.TarotCard.RectTransform.SetSiblingIndex(0);
			target.Alert.gameObject.SetActive(true);
			MMSelectable selectable = target.Selectable;
			selectable.OnDeselected = (Action)Delegate.Combine(selectable.OnDeselected, (Action)delegate
			{
				target.Alert.gameObject.SetActive(false);
			});
			infoCard.Show(true);
			_tarotInfoCardController.ForceCurrentCard(infoCard, _showCard);
			_tarotInfoCardController.enabled = true;
			SetActiveStateForMenu(true);
			_controlPrompts.ShowCancelButton();
			_scrollRect.vertical = true;
		}

		private IEnumerator ShowCardsMultiple()
		{
			_canvasGroup.interactable = false;
			_tarotInfoCardController.enabled = false;
			_scrollRect.ScrollSpeedModifier = 2f;
			_controlPrompts.HideCancelButton();
			SetActiveStateForMenu(false);
			List<TarotCardItem_Unlocked> tarotItems = new List<TarotCardItem_Unlocked>();
			foreach (TarotCardItem_Unlocked tarotItem in _items)
			{
				if (_showCards.Contains(tarotItem.Card.CardType))
				{
					tarotItems.Add(tarotItem);
					tarotItem.TarotCard.SetStaticBack();
				}
				tarotItem.TarotCard.SetStaticBack();
				tarotItem.ForceIncognitoMode();
				MMSelectable selectable = tarotItem.Selectable;
				selectable.OnDeselected = (Action)Delegate.Combine(selectable.OnDeselected, (Action)delegate
				{
					tarotItem.Alert.gameObject.SetActive(false);
				});
			}
			tarotItems.Sort((TarotCardItem_Unlocked a, TarotCardItem_Unlocked b) => a.RectTransform.GetSiblingIndex().CompareTo(b.RectTransform.GetSiblingIndex()));
			yield return _animator.YieldForAnimation("Show");
			yield return new WaitForSecondsRealtime(0.1f);
			for (int i = 0; i < tarotItems.Count; i++)
			{
				float timeScale = 2f + Mathf.Floor((float)i / 3f) * 0.1f;
				_tarotCount++;
				_scrollRect.ScrollSpeedModifier = timeScale;
				yield return _scrollRect.DoScrollTo(tarotItems[i].RectTransform);
				_collectedText.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", _tarotCount, DataManager.AllTrinkets.Count));
				RectTransform tarotCardContainer = tarotItems[i].TarotCard.RectTransform.parent as RectTransform;
				tarotItems[i].TarotCard.RectTransform.SetParent(_scrollRect.viewport.parent, true);
				tarotItems[i].TarotCard.RectTransform.SetSiblingIndex(tarotItems[i].TarotCard.transform.parent.childCount);
				UIManager.PlayAudio("event:/player/new_item_sequence_close");
				tarotItems[i].AnimateIncognitoOut();
				yield return tarotItems[i].TarotCard.YieldForReveal(timeScale - 1f);
				tarotItems[i].TarotCard.RectTransform.SetParent(tarotCardContainer, true);
				tarotItems[i].TarotCard.RectTransform.SetSiblingIndex(0);
			}
			for (int j = 0; j < tarotItems.Count; j++)
			{
				StartCoroutine(tarotItems[j].ShowAlert());
			}
			yield return new WaitForSecondsRealtime(0.1f);
			_scrollRect.ScrollSpeedModifier = 1f;
			tarotItems.LastElement().Selectable.OnSelected = null;
			OverrideDefault(tarotItems.LastElement().Selectable);
			_tarotInfoCardController.enabled = true;
			SetActiveStateForMenu(true);
			_controlPrompts.ShowCancelButton();
			_canvasGroup.interactable = true;
		}

		protected override void OnHideStarted()
		{
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoShowAnimation();
		}
	}
}
