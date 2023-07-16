using System;
using I2.Loc;
using UnityEngine;

public class Interaction_DevotionShop : Interaction
{
	public GameObject ReceiveSoulPosition;

	private float BasePrice = 1f;

	private string sLabel;

	private int LastDayUsed
	{
		get
		{
			return DataManager.Instance.MidasDevotionLastUsed;
		}
		set
		{
			DataManager.Instance.MidasDevotionLastUsed = value;
		}
	}

	private float Cost
	{
		get
		{
			return DataManager.Instance.MidasDevotionCost;
		}
		set
		{
			DataManager.Instance.MidasDevotionCost = value;
		}
	}

	private void Start()
	{
		if (!GameManager.HasUnlockAvailable() && !DataManager.Instance.DeathCatBeaten)
		{
			base.gameObject.SetActive(false);
		}
		ActivateDistance = 2f;
		UpdateLocalisation();
		CoolDownPrice();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(CoolDownPrice));
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(CoolDownPrice));
	}

	private void CoolDownPrice()
	{
		if (TimeManager.CurrentDay > LastDayUsed)
		{
			int num = TimeManager.CurrentDay - LastDayUsed;
			Cost -= 10 * num;
			if (Cost < BasePrice)
			{
				Cost = BasePrice;
			}
			base.HasChanged = true;
		}
		LastDayUsed = TimeManager.CurrentDay;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.Buy;
	}

	private string GetAffordColor()
	{
		if ((float)Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) < Cost)
		{
			return "<color=red>";
		}
		return "<color=#f4ecd3>";
	}

	public override void GetLabel()
	{
		base.Label = string.Format(ScriptLocalization.UI_ItemSelector_Context.Buy, "10x<sprite name=\"icon_spirits\">", CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, (int)Cost));
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
	}

	public override void OnInteract(StateMachine state)
	{
		if ((float)Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) < Cost)
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
			return;
		}
		base.OnInteract(state);
		if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
		{
			int num = 10;
			while (--num >= 0)
			{
				SoulCustomTarget.Create(state.gameObject, ReceiveSoulPosition.transform.position, Color.white, GivePlayerSoul);
			}
		}
		else
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
		}
		Inventory.ChangeItemQuantity(20, -(int)Cost);
		Cost = Mathf.CeilToInt(Cost * 1.2f);
		base.HasChanged = true;
		LastDayUsed = TimeManager.CurrentDay;
	}

	private void GivePlayerSoul()
	{
		PlayerFarming.Instance.GetSoul(1);
	}
}
