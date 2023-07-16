using System;
using System.Collections.Generic;
using Rewired;
using Unify.Input;
using UnityEngine;

namespace MMTools.UIInventory
{
	public class UIInventoryController : BaseMonoBehaviour
	{
		public Action Callback;

		[HideInInspector]
		public float PauseTimeSpeed = 0.005f;

		public UIInventoryList ItemsList;

		private Player _RewiredController;

		public UIInventoryItem CurrentSelection;

		public UIInventoryItem CurrentHighlighted;

		private UIInventoryListSelector CurrentSelectionGroup;

		public UIInventoryListSelector[] UIInventoryListSelector;

		[HideInInspector]
		public Player RewiredController
		{
			get
			{
				if (_RewiredController == null)
				{
					_RewiredController = RewiredInputManager.MainPlayer;
				}
				return _RewiredController;
			}
		}

		private void Start()
		{
			GameManager.SetTimeScale(PauseTimeSpeed);
			StartUIInventoryController();
		}

		public virtual void StartUIInventoryController()
		{
			InventoryItem inventoryItem = new InventoryItem();
			inventoryItem.Init(14, 1);
			UpdateCurrentSelection(inventoryItem);
			UpdateCurrentHighlighted(inventoryItem);
			List<InventoryItem> list = new List<InventoryItem>();
			InventoryItem inventoryItem2 = new InventoryItem();
			inventoryItem2.Init(14, 1);
			list.Add(inventoryItem2);
			inventoryItem2 = new InventoryItem();
			inventoryItem2.Init(9, 1);
			list.Add(inventoryItem2);
			inventoryItem2 = new InventoryItem();
			inventoryItem2.Init(11, 1);
			list.Add(inventoryItem2);
			ItemsList.PopulateList(list);
			SelectionManagementStart(0);
		}

		public void UpdateCurrentSelection(InventoryItem item)
		{
			if (CurrentSelection != null)
			{
				CurrentSelection.Init(item);
			}
		}

		public void UpdateCurrentHighlighted(InventoryItem item)
		{
			if (CurrentHighlighted != null)
			{
				CurrentHighlighted.Init(item);
			}
		}

		public void SelectionManagementStart(int Selection)
		{
			CurrentSelectionGroup = UIInventoryListSelector[0];
			CurrentSelectionGroup.SetActive(Selection);
			UIInventoryListSelector currentSelectionGroup = CurrentSelectionGroup;
			currentSelectionGroup.OnMove = (UIInventoryListSelector.ListSelectorAction)Delegate.Combine(currentSelectionGroup.OnMove, new UIInventoryListSelector.ListSelectorAction(OnMove));
			UIInventoryListSelector currentSelectionGroup2 = CurrentSelectionGroup;
			currentSelectionGroup2.OnSelect = (UIInventoryListSelector.ListSelectorAction)Delegate.Combine(currentSelectionGroup2.OnSelect, new UIInventoryListSelector.ListSelectorAction(OnSelect));
		}

		public virtual void OnMove(InventoryItem Item)
		{
			CurrentHighlighted.Init(Item);
		}

		public virtual void OnSelect(InventoryItem Item)
		{
			CurrentSelection.Init(Item);
		}

		private void Update()
		{
			UpdateUIInventoryController();
		}

		public virtual void UpdateUIInventoryController()
		{
			if (UIInventoryListSelector.Length != 0)
			{
				if (CurrentSelectionGroup != null)
				{
					CurrentSelectionGroup.DoUpdate(RewiredController);
				}
				if (CurrentSelectionGroup == UIInventoryListSelector[0] && InputManager.UI.GetCancelButtonDown())
				{
					Close();
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		private void GetSelectorGroups()
		{
			UIInventoryListSelector = GetComponentsInChildren<UIInventoryListSelector>();
		}

		public virtual void Close()
		{
			if (Callback != null)
			{
				Callback();
			}
			GameManager.SetTimeScale(1f);
		}
	}
}
