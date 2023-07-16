using System;
using System.Collections;
using System.Collections.Generic;
using MMTools.UIInventory;
using UnityEngine;

public class UI_SpellSelect : UIInventoryController
{
	public PlayerSpells playerSpells;

	private bool Closing;

	public static void Play(Action Callback, PlayerSpells playerSpells)
	{
		UI_SpellSelect component = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("MMUIInventory/UI Spell Select"), GlobalCanvasReference.Instance).GetComponent<UI_SpellSelect>();
		component.playerSpells = playerSpells;
		component.Callback = Callback;
	}

	public override void StartUIInventoryController()
	{
		new List<InventoryItem.ITEM_TYPE>();
		List<InventoryItem> itemsToPopulate = new List<InventoryItem>();
		int selection = 0;
		ItemsList.PopulateList(itemsToPopulate);
		SelectionManagementStart(selection);
	}

	public override void UpdateUIInventoryController()
	{
		if (!Closing)
		{
			base.UpdateUIInventoryController();
		}
	}

	private IEnumerator CloseRoutine()
	{
		Closing = true;
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.unscaledDeltaTime);
			if (!(num < 0.5f))
			{
				break;
			}
			yield return null;
		}
		if (Callback != null)
		{
			Callback();
		}
		Close();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
