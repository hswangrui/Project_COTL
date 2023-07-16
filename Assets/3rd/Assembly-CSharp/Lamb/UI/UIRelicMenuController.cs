using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using I2.Loc;
using src.Extensions;
using src.UI.InfoCards;
using src.UI.Items;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIRelicMenuController : UIMenuBase
	{
		private const string kUnlockAnimationState = "Unlock";

		private const string kShowUnlockAnimationState = "Show Unlock";

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private TMP_Text _collectedText;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private RelicInfoCardController _infoCardController;

		[Header("Reveal Sequence")]
		[SerializeField]
		private CanvasGroup _unlockHeaderCanvasGroup;

		[SerializeField]
		private RectTransform _front;

		private List<RelicItem> _items = new List<RelicItem>();

		private RelicData _showRelic;

		private List<RelicData> _showRelics;

		private int _relicCount;

		public void Show(RelicType relicType, bool instant = false)
		{
			_showRelic = EquipmentManager.GetRelicData(relicType);
			_relicCount = Mathf.Clamp(DataManager.Instance.PlayerFoundRelics.Count - 1, 0, int.MaxValue);
			Show(instant);
		}

		public void Show(List<RelicType> relicTypes, bool instant = false)
		{
			Show(EquipmentManager.GetRelicData(relicTypes), instant);
		}

		public void Show(List<RelicData> relicDatas, bool instant = false)
		{
			_showRelics = relicDatas;
			_relicCount = Mathf.Clamp(DataManager.Instance.PlayerFoundRelics.Count - _showRelics.Count, 0, int.MaxValue);
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			UIManager.PlayAudio("event:/ui/open_menu");
			_controlPrompts.HideAcceptButton();
			_scrollRect.normalizedPosition = Vector2.one;
			RelicData[] relicData = EquipmentManager.RelicData;
			foreach (RelicData data in relicData)
			{
				RelicItem relicItem = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.RelicItemTemplate, _contentContainer);
				relicItem.Configure(data);
				_items.Add(relicItem);
			}
			if (_showRelic == null && _showRelics == null)
			{
				_relicCount = Mathf.Clamp(DataManager.Instance.PlayerFoundRelics.Count, 0, int.MaxValue);
				OverrideDefault(_items[0].Selectable);
				ActivateNavigation();
			}
			_collectedText.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", _relicCount, EquipmentManager.RelicData.Length));
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_showRelic != null)
			{
				yield return DoUnlockAnimationSingle();
			}
			else if (_showRelics != null && _showRelics.Count > 0)
			{
				yield return DoUnlockAnimationMulti();
			}
			else
			{
				yield return _003C_003En__0();
			}
		}

		private IEnumerator DoUnlockAnimationSingle()
		{
			_canvasGroup.interactable = false;
			_scrollRect.vertical = false;
			_controlPrompts.HideCancelButton();
			RelicItem target = _items[0];
			foreach (RelicItem item in _items)
			{
				if (item.Data.RelicType == _showRelic.RelicType)
				{
					target = item;
					target.ForceLockedState();
					target.Alert.TryRemoveAlert();
				}
				else
				{
					item.ForceIncognitoState();
				}
			}
			_infoCardController.enabled = false;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			RelicInfoCard infoCard = _infoCardController.Card1;
			infoCard.Configure(_showRelic);
			infoCard.Hide(true);
			infoCard.Animator.enabled = false;
			infoCard.CanvasGroup.alpha = 0f;
			infoCard.RectTransform.SetParent(_front);
			infoCard.RectTransform.anchoredPosition = Vector2.zero;
			Transform originalParent = infoCard.IconContainer.parent;
			RectTransform iconContainer = infoCard.IconContainer;
			CanvasGroup iconCanvasGroup = infoCard.IconCanvasGroup;
			Vector2 originalScale = iconContainer.localScale;
			Vector2 originalPosition = iconContainer.localPosition;
			iconContainer.SetParent(_front);
			iconCanvasGroup.alpha = 0f;
			iconContainer.localScale = originalScale * 3f;
			iconContainer.localPosition = Vector2.zero;
			yield return _animator.YieldForAnimation("Unlock");
			yield return new WaitForSecondsRealtime(0.15f);
			iconContainer.DOScale(originalScale * 5f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			iconCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.75f);
			Vector2 vector = _front.InverseTransformPoint(originalParent.TransformPoint(originalPosition));
			iconContainer.DOLocalMove(vector, 0.65f).SetUpdate(true).SetEase(Ease.InBack);
			iconContainer.DOScale(originalScale, 0.65f).SetUpdate(true).SetEase(Ease.InBack);
			yield return new WaitForSecondsRealtime(0.15f);
			infoCard.CanvasGroup.DOFade(1f, 0.75f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.55f);
			iconContainer.SetParent(originalParent);
			_controlPrompts.ShowAcceptButton();
			while (!InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			_controlPrompts.HideAcceptButton();
			infoCard.RectTransform.SetParent(_infoCardController.transform, true);
			infoCard.RectTransform.DOAnchorPos(Vector2.zero, 0.66f).SetUpdate(true);
			_unlockHeaderCanvasGroup.DOFade(0f, 0.66f).SetUpdate(true);
			yield return _animator.YieldForAnimation("Show Unlock");
			yield return new WaitForSecondsRealtime(0.25f);
			OverrideDefaultOnce(target.Selectable);
			yield return _scrollRect.DoScrollTo(target.RectTransform);
			_relicCount++;
			UIManager.PlayAudio("event:/player/new_item_sequence_close");
			yield return target.DoUnlock();
			_collectedText.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", _relicCount, EquipmentManager.RelicData.Length));
			target.Alert.gameObject.SetActive(true);
			MMSelectable selectable = target.Selectable;
			selectable.OnDeselected = (Action)Delegate.Combine(selectable.OnDeselected, (Action)delegate
			{
				target.Alert.gameObject.SetActive(false);
			});
			infoCard.Animator.enabled = true;
			infoCard.Show(true);
			_infoCardController.ForceCurrentCard(infoCard, _showRelic);
			_infoCardController.enabled = true;
			SetActiveStateForMenu(true);
			_controlPrompts.ShowCancelButton();
			_scrollRect.vertical = true;
			_canvasGroup.interactable = true;
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		private IEnumerator DoUnlockAnimationMulti()
		{
			_canvasGroup.interactable = false;
			_infoCardController.enabled = false;
			_scrollRect.ScrollSpeedModifier = 2f;
			_controlPrompts.HideCancelButton();
			List<RelicItem> relicItems = new List<RelicItem>();
			foreach (RelicItem item in _items)
			{
				item.Selectable.Interactable = false;
				if (_showRelics.Contains(item.Data))
				{
					relicItems.Add(item);
					item.ForceLockedState();
				}
				else
				{
					item.ForceIncognitoState();
				}
				MMSelectable selectable = item.Selectable;
				selectable.OnDeselected = (Action)Delegate.Combine(selectable.OnDeselected, (Action)delegate
				{
					item.Alert.gameObject.SetActive(false);
				});
			}
			relicItems.Sort((RelicItem a, RelicItem b) => a.RectTransform.GetSiblingIndex().CompareTo(b.RectTransform.GetSiblingIndex()));
			yield return _animator.YieldForAnimation("Show");
			yield return new WaitForSecondsRealtime(0.1f);
			for (int i = 0; i < relicItems.Count; i++)
			{
				float scrollSpeedModifier = 2f + Mathf.Floor((float)i / 3f) * 0.3f;
				_scrollRect.ScrollSpeedModifier = scrollSpeedModifier;
				_relicCount++;
				yield return _scrollRect.DoScrollTo(relicItems[i].RectTransform);
				_collectedText.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", _relicCount, EquipmentManager.RelicData.Length));
				yield return relicItems[i].Flash();
			}
			for (int j = 0; j < relicItems.Count; j++)
			{
				StartCoroutine(relicItems[j].ShowAlert());
			}
			yield return new WaitForSecondsRealtime(0.1f);
			_scrollRect.ScrollSpeedModifier = 1f;
			OverrideDefault(relicItems.LastElement().Selectable);
			_infoCardController.enabled = true;
			SetActiveStateForMenu(true);
			_controlPrompts.ShowCancelButton();
			_canvasGroup.interactable = true;
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
