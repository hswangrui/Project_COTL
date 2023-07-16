using System;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using UnityEngine;

public class Interaction_SeedShop : Interaction
{
	[SerializeField]
	private GameObject postGameConvo;

	public SkeletonAnimation ShopKeeperSpine;

	private GameObject g;

	public TraderTracker TraderInfo;

	private void Start()
	{
		if (!DataManager.Instance.HasMetChefShop && DataManager.Instance.BossesCompleted.Count <= 0)
		{
			base.gameObject.SetActive(false);
		}
		TraderInfo.traderName = base.gameObject.name;
		AddShopItems();
		ApplyAnimationState();
		if (DataManager.Instance.ShopKeeperChefState == 1)
		{
			foreach (TraderTrackerItems item in TraderInfo.itemsToTrade)
			{
				item.SellOffset = item.SellPrice;
			}
		}
		if (DataManager.Instance.DeathCatBeaten)
		{
			foreach (TraderTrackerItems item2 in TraderInfo.itemsToTrade)
			{
				item2.SellPrice *= 2;
			}
			return;
		}
		if (postGameConvo != null)
		{
			UnityEngine.Object.Destroy(postGameConvo);
		}
	}

	private void AddShopItems()
	{
		TraderInfo.blackList.Clear();
		if (!DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_2) && !DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_2) && !DataManager.Instance.DeathCatBeaten)
		{
			TraderInfo.blackList.Add(InventoryItem.ITEM_TYPE.SEED_PUMPKIN);
		}
		if (!DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_3) && !DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_3) && !DataManager.Instance.DeathCatBeaten)
		{
			TraderInfo.blackList.Add(InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER);
		}
		if (!DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_4) && !DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_4) && !DataManager.Instance.DeathCatBeaten)
		{
			TraderInfo.blackList.Add(InventoryItem.ITEM_TYPE.SEED_BEETROOT);
		}
		TraderInfo.GetItemsForSale();
	}

	private List<InventoryItem> GetItemsForSale()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (InventoryItem.ITEM_TYPE item in TraderInfo.itemsForSale)
		{
			list.Add(new InventoryItem(item, 999));
		}
		return list;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		AddShopItems();
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(CheckIfShouldShow));
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(CheckIfShouldShow));
	}

	private void CheckIfShouldShow()
	{
	}

	public override void GetLabel()
	{
		base.Label = LocalizationManager.GetTranslation("Interactions/SeedShop");
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		List<InventoryItem> itemsForSale = GetItemsForSale();
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
		cameraFollowTarget.SetOffset(new Vector3(0f, 4.5f, 2f));
		cameraFollowTarget.AddTarget(base.gameObject, 1f);
		HUD_Manager.Instance.Hide(false, 0);
		UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(itemsForSale, new ItemSelector.Params
		{
			Key = "seed_shop",
			Context = ItemSelector.Context.Buy,
			Offset = new Vector2(0f, 150f),
			ShowEmpty = true,
			RequiresDiscovery = false,
			HideQuantity = true
		});
		itemSelector.CostProvider = GetTradeItem;
		UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			TraderTrackerItems tradeItem = GetTradeItem(chosenItem);
			Inventory.ChangeItemQuantity(20, -tradeItem.SellPriceActual);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
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
			Interactable = true;
			base.HasChanged = true;
		});
	}

	public void ApplyAnimationState()
	{
		if ((bool)ShopKeeperSpine && ShopKeeperSpine.AnimationState != null)
		{
			if (DataManager.Instance.ShopKeeperChefState == 1)
			{
				ShopKeeperSpine.AnimationState.SetAnimation(0, "animation-angry", true);
			}
			else if (DataManager.Instance.ShopKeeperChefState == 2)
			{
				ShopKeeperSpine.AnimationState.SetAnimation(0, "scared", true);
			}
		}
	}

	private void FinalizeTransaction(InventoryItem.ITEM_TYPE itemType, int amount)
	{
		InventoryItem.Spawn(itemType, amount, base.gameObject.transform.position);
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
}
