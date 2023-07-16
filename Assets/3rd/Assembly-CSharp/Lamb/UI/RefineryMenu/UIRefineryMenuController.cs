using System;
using System.Collections.Generic;
using DG.Tweening;
using src.UI.InfoCards;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI.RefineryMenu
{
	public class UIRefineryMenuController : UIMenuBase
	{
		public Action<InventoryItem.ITEM_TYPE> OnItemQueued;

		[SerializeField]
		private RefineryInfoCardController _infoCardController;

		public RefineryItem refineryIconPrefab;

		public Transform Container;

		[SerializeField]
		private Transform queuedContainer;

		[SerializeField]
		private TMP_Text amountQueuedText;

		[SerializeField]
		private GameObject[] _vacantSlots;

		[SerializeField]
		private GameObject _addToQueuePrompt;

		[SerializeField]
		private GameObject _removeFromQueuePrompt;

		[SerializeField]
		private GameObject _queueFullPrompt;

		private List<RefineryItem> _refineryItems = new List<RefineryItem>();

		private List<RefineryItem> _queuedItems = new List<RefineryItem>();

		private Tween _queuedTextTween;

		private StructuresData _structureInfo;

		private Interaction_Refinery _interactionRefinery;

		private InventoryItem.ITEM_TYPE[] _refinableResources = new InventoryItem.ITEM_TYPE[4]
		{
			InventoryItem.ITEM_TYPE.LOG_REFINED,
			InventoryItem.ITEM_TYPE.STONE_REFINED,
			InventoryItem.ITEM_TYPE.BLACK_GOLD,
			InventoryItem.ITEM_TYPE.GOLD_REFINED
		};

		private int kMaxItems
		{
			get
			{
				if (_structureInfo.Type != StructureBrain.TYPES.REFINERY)
				{
					return 10;
				}
				return 5;
			}
		}

		public void Show(StructuresData refineryData, Interaction_Refinery interactionRefinery, bool instant = false)
		{
			_structureInfo = refineryData;
			_interactionRefinery = interactionRefinery;
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			for (int i = 0; i < _vacantSlots.Length; i++)
			{
				_vacantSlots[i].SetActive(i < kMaxItems);
			}
			InventoryItem.ITEM_TYPE[] refinableResources = _refinableResources;
			foreach (InventoryItem.ITEM_TYPE type in refinableResources)
			{
				RefineryItem refineryItem = UnityEngine.Object.Instantiate(refineryIconPrefab, Container);
				refineryItem.OnItemSelected = (Action<RefineryItem>)Delegate.Combine(refineryItem.OnItemSelected, new Action<RefineryItem>(OnItemSelected));
				MMButton button = refineryItem.Button;
				button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(OnQueueableItemSelected));
				refineryItem.Configure(type);
				refineryItem.FadeIn((float)_refineryItems.Count * 0.03f);
				_refineryItems.Add(refineryItem);
			}
			OverrideDefault(_refineryItems[0].Button);
			ActivateNavigation();
			for (int k = 0; k < _structureInfo.QueuedResources.Count; k++)
			{
				MakeQueuedItem(_structureInfo.QueuedResources[k]);
			}
			UpdateQueueText();
			UpdateQuantities();
		}

		private void UpdateQueueText()
		{
			Color colour = ((_queuedItems.Count > 0) ? StaticColors.GreenColor : StaticColors.RedColor);
			amountQueuedText.text = string.Format("{0}/{1}", _queuedItems.Count, kMaxItems).Colour(colour);
		}

		private void UpdateQuantities()
		{
			foreach (RefineryItem refineryItem in _refineryItems)
			{
				refineryItem.UpdateQuantity();
			}
			foreach (RefineryItem queuedItem in _queuedItems)
			{
				queuedItem.UpdateQuantity();
			}
		}

		private void OnItemSelected(RefineryItem item)
		{
			if (item.CanAfford && _structureInfo.QueuedResources.Count < kMaxItems)
			{
				if (_structureInfo.QueuedResources.Count >= kMaxItems)
				{
					ShowMaxQueued();
					item.Shake();
				}
				else
				{
					AddToQueue(item.Type);
				}
			}
		}

		private void OnQueueableItemSelected()
		{
			if (_structureInfo.QueuedResources.Count >= kMaxItems)
			{
				_addToQueuePrompt.SetActive(false);
				_removeFromQueuePrompt.SetActive(false);
				_queueFullPrompt.SetActive(true);
			}
			else
			{
				_addToQueuePrompt.SetActive(true);
				_removeFromQueuePrompt.SetActive(false);
				_queueFullPrompt.SetActive(false);
			}
		}

		private void OnQueuedItemSelected()
		{
			_addToQueuePrompt.SetActive(false);
			_removeFromQueuePrompt.SetActive(true);
			_queueFullPrompt.SetActive(false);
		}

		private void AddToQueue(InventoryItem.ITEM_TYPE resource)
		{
			_structureInfo.QueuedResources.Add(resource);
			foreach (StructuresData.ItemCost item in Structures_Refinery.GetCost(resource))
			{
				Inventory.ChangeItemQuantity((int)item.CostItem, -item.CostValue);
			}
			RefineryItem refineryItem = MakeQueuedItem(resource);
			Vector3 localScale = refineryItem.RectTransform.localScale;
			refineryItem.RectTransform.localScale = Vector3.one * 1.2f;
			refineryItem.RectTransform.DOScale(localScale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			_infoCardController.CurrentCard.Configure(resource);
			Action<InventoryItem.ITEM_TYPE> onItemQueued = OnItemQueued;
			if (onItemQueued != null)
			{
				onItemQueued(resource);
			}
			UpdateQueueText();
			UpdateQuantities();
			OnQueueableItemSelected();
			foreach (RefineryItem refineryItem2 in _refineryItems)
			{
				refineryItem2.Button.Confirmable = _queuedItems.Count < kMaxItems && refineryItem2.CanAfford;
			}
		}

		private RefineryItem MakeQueuedItem(InventoryItem.ITEM_TYPE resource)
		{
			RefineryItem refineryItem = UnityEngine.Object.Instantiate(refineryIconPrefab, queuedContainer);
			refineryItem.Configure(resource, true, _queuedItems.Count, false);
			refineryItem.UpdateRefiningProgress(_structureInfo.Progress / ((Structures_Refinery)StructureBrain.GetOrCreateBrain(_structureInfo)).RefineryDuration(refineryItem.Type));
			MMButton button = refineryItem.Button;
			button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(OnQueuedItemSelected));
			refineryItem.OnItemSelected = (Action<RefineryItem>)Delegate.Combine(refineryItem.OnItemSelected, new Action<RefineryItem>(RemoveFromQueue));
			_vacantSlots[_queuedItems.Count].SetActive(false);
			_queuedItems.Add(refineryItem);
			refineryItem.transform.SetSiblingIndex(_queuedItems.Count - 1);
			return refineryItem;
		}

		private void RemoveFromQueue(RefineryItem item)
		{
			if (_infoCardController.CurrentCard != null)
			{
				_infoCardController.CurrentCard.Configure(item.Type);
			}
			IMMSelectable iMMSelectable = item.Button.FindSelectableOnRight() as IMMSelectable;
			IMMSelectable iMMSelectable2 = item.Button.FindSelectableOnLeft() as IMMSelectable;
			if (_queuedItems.IndexOf(item) < _queuedItems.Count - 1 && iMMSelectable != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(iMMSelectable);
			}
			else if (_queuedItems.IndexOf(item) > 0 && iMMSelectable2 != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(iMMSelectable2);
			}
			else if (_queuedItems.Count - 1 > 0)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_queuedItems[_queuedItems.Count - 2].Button);
			}
			else
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_refineryItems[0].Button);
			}
			int num = _queuedItems.IndexOf(item);
			_structureInfo.QueuedResources.RemoveAt(num);
			foreach (StructuresData.ItemCost item2 in Structures_Refinery.GetCost(item.Type))
			{
				Inventory.AddItem((int)item2.CostItem, item2.CostValue);
			}
			_queuedItems.Remove(item);
			_vacantSlots[_queuedItems.Count].SetActive(true);
			if (num == 0 && _queuedItems.Count > 0)
			{
				_queuedItems[0].Configure(_queuedItems[0].Type, true, 0, false);
				_structureInfo.Progress = 0f;
			}
			if (_queuedItems.Count == 0 && _interactionRefinery != null)
			{
				_interactionRefinery.OnCompleteRefining();
			}
			UnityEngine.Object.Destroy(item.gameObject);
			UpdateQueueText();
			UpdateQuantities();
		}

		private void ShowMaxQueued()
		{
			if (_queuedTextTween != null && !_queuedTextTween.IsComplete())
			{
				_queuedTextTween.Complete();
				amountQueuedText.transform.localScale = Vector3.one;
			}
			_queuedTextTween = amountQueuedText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
