using System;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_Trader : Interaction
{
	public TraderTracker TraderInfo;

	public bool SellOnly;

	[SerializeField]
	private string _buyKey;

	[SerializeField]
	private string _sellKey;

	private string sSell;

	private string sBuy;

	public UnityEvent TraderOpen;

	public UnityEvent TraderClosed;

	public bool ShowCoinsQuantity = true;

	private void Start()
	{
		TraderInfo.traderName = base.gameObject.name;
		if (DataManager.Instance.ReturnTrader(TraderInfo.location) == null)
		{
			DataManager.Instance.Traders.Add(TraderInfo);
		}
		TraderInfo = DataManager.Instance.ReturnTrader(TraderInfo.location);
		TraderInfo.GetItemsForSale();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(UpdatePrices));
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sSell = ScriptLocalization.Interactions.Sell;
		sBuy = ScriptLocalization.Interactions.Buy;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(UpdatePrices));
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		HasSecondaryInteraction = !SellOnly;
		SecondaryInteractable = HasSecondaryInteraction;
	}

	private void UpdatePrices()
	{
		for (int i = 0; i < TraderInfo.itemsToTrade.Count; i++)
		{
			int num = TimeManager.CurrentDay - TraderInfo.itemsToTrade[i].LastDayChecked;
			TraderInfo.itemsToTrade[i].BuyOffset = Mathf.Clamp(TraderInfo.itemsToTrade[i].BuyOffset - num * 5, 0, 100);
			TraderInfo.itemsToTrade[i].LastDayChecked = TimeManager.CurrentDay;
		}
	}

	public override void GetLabel()
	{
		if (SellOnly)
		{
			base.Label = sSell;
		}
		else
		{
			base.Label = string.Format(sBuy, "");
		}
	}

	public override void GetSecondaryLabel()
	{
		base.Label = sSell;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (SellOnly)
		{
			DoSell();
		}
		else
		{
			DoBuy();
		}
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		DoSell();
	}

	private void DoSell()
	{
		if (Inventory.GetItemQuantities(TraderInfo.itemsForSale) == 0)
		{
			return;
		}
		UnityEvent traderOpen = TraderOpen;
		if (traderOpen != null)
		{
			traderOpen.Invoke();
		}
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
		cameraFollowTarget.SetOffset(new Vector3(0f, 4.5f, 2f));
		cameraFollowTarget.AddTarget(base.gameObject, 1f);
		HUD_Manager.Instance.Hide(false, 0);
		UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(TraderInfo.itemsForSale, new ItemSelector.Params
		{
			Key = _sellKey,
			Context = ItemSelector.Context.Sell,
			Offset = new Vector2(0f, 100f),
			ShowCoins = ShowCoinsQuantity
		});
		itemSelector.CostProvider = GetTradeItem;
		UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			TraderTrackerItems tradeItem = GetTradeItem(chosenItem);
			int cost = tradeItem.BuyPriceActual;
			Inventory.ChangeItemQuantity((int)chosenItem, -1);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, chosenItem, delegate
			{
				FinalizeTransaction(InventoryItem.ITEM_TYPE.BLACK_GOLD, cost);
			});
			tradeItem.BuyOffset = Mathf.Min(tradeItem.BuyOffset + 2, 100);
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
		uIItemSelectorOverlayController2.OnCancel = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnCancel, (Action)delegate
		{
			HUD_Manager.Instance.Show(0);
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController3 = itemSelector;
		uIItemSelectorOverlayController3.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController3.OnHidden, (Action)delegate
		{
			UnityEvent traderClosed = TraderClosed;
			if (traderClosed != null)
			{
				traderClosed.Invoke();
			}
			cameraFollowTarget.SetOffset(Vector3.zero);
			cameraFollowTarget.RemoveTarget(base.gameObject);
			state.CURRENT_STATE = StateMachine.State.Idle;
			itemSelector = null;
			base.HasChanged = true;
			Interactable = true;
		});
	}

	private void DoBuy()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
		cameraFollowTarget.SetOffset(new Vector3(0f, 4.5f, 2f));
		cameraFollowTarget.AddTarget(base.gameObject, 1f);
		HUD_Manager.Instance.Hide(false, 0);
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (InventoryItem.ITEM_TYPE item in TraderInfo.itemsForSale)
		{
			list.Add(new InventoryItem(item, 999));
		}
		UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(list, new ItemSelector.Params
		{
			Key = _buyKey,
			Context = ItemSelector.Context.Buy,
			Offset = new Vector2(0f, 100f),
			ShowEmpty = true,
			RequiresDiscovery = false,
			HideQuantity = true
		});
		itemSelector.CostProvider = GetTradeItem;
		UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			TraderTrackerItems tradeItem = GetTradeItem(chosenItem);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
			Inventory.ChangeItemQuantity(20, -tradeItem.SellPriceActual);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, delegate
			{
				FinalizeTransaction(chosenItem, 1);
			});
			tradeItem.BuyOffset = Mathf.Min(tradeItem.BuyOffset + 2, 100);
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
		uIItemSelectorOverlayController2.OnCancel = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnCancel, (Action)delegate
		{
			HUD_Manager.Instance.Show(0);
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController3 = itemSelector;
		uIItemSelectorOverlayController3.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController3.OnHidden, (Action)delegate
		{
			cameraFollowTarget.SetOffset(Vector3.zero);
			cameraFollowTarget.RemoveTarget(base.gameObject);
			state.CURRENT_STATE = StateMachine.State.Idle;
			itemSelector = null;
			base.HasChanged = true;
			Interactable = true;
		});
	}

	private TraderTrackerItems GetTradeItem(InventoryItem.ITEM_TYPE item)
	{
		foreach (TraderTrackerItems item2 in TraderInfo.itemsToTrade)
		{
			if (item2.itemForTrade == item)
			{
				return item2;
			}
		}
		return null;
	}

	private void FinalizeTransaction(InventoryItem.ITEM_TYPE itemType, int cost)
	{
		InventoryItem.Spawn(itemType, cost, base.gameObject.transform.position);
	}
}
