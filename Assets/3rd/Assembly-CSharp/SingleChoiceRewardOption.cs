using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SingleChoiceRewardOption : BaseMonoBehaviour
{
	public UnityEvent Callback;

	public bool StartHidden;

	public TextMeshPro QuantityText;

	public Material materialPresetText;

	[SerializeField]
	private Vector2 randomOffset = new Vector2(-2f, 2f);

	[SerializeField]
	private InventoryItemDisplay itemDisplay;

	[SerializeField]
	private Interaction_PickUpLoot interaction;

	[Space]
	[SerializeField]
	private SingleChoiceRewardOption[] otherOptions;

	[SerializeField]
	public List<BuyEntry> itemOptions;

	public bool AllowDecorationAndSkin;

	public bool AllowDuplicateFood;

	public BuyEntry Option { get; private set; }

	private void Start()
	{
		SetOption();
		if (StartHidden)
		{
			Hide();
		}
	}

	public void Reveal()
	{
		base.gameObject.SetActive(true);
		base.transform.localScale = Vector3.zero;
		base.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(Ease.OutBack);
		if (materialPresetText != null)
		{
			QuantityText.fontSharedMaterial = materialPresetText;
		}
	}

	private void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public virtual List<BuyEntry> GetOptions()
	{
		return itemOptions;
	}

	private void SetOption()
	{
		List<BuyEntry> options = GetOptions();
		for (int num = options.Count - 1; num >= 0; num--)
		{
			if (options[num].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION)
			{
				if (!DataManager.GetDecorationsAvailableCategory(PlayerFarming.Location))
				{
					options.Remove(options[num]);
				}
			}
			else if (options[num].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN)
			{
				if (!DataManager.CheckIfThereAreSkinsAvailable())
				{
					options.Remove(options[num]);
				}
			}
			else if (options[num].itemToBuy == InventoryItem.ITEM_TYPE.DOCTRINE_STONE)
			{
				if (!DoctrineUpgradeSystem.TrySermonsStillAvailable() || !DoctrineUpgradeSystem.TryGetStillDoctrineStone())
				{
					options.Remove(options[num]);
				}
			}
			else if (options[num].itemToBuy == InventoryItem.ITEM_TYPE.TRINKET_CARD && PlayerFleeceManager.FleecePreventTarotCards())
			{
				options.Remove(options[num]);
			}
			else if (!DataManager.Instance.ShowLoyaltyBars && (options[num].itemToBuy == InventoryItem.ITEM_TYPE.GIFT_SMALL || options[num].itemToBuy == InventoryItem.ITEM_TYPE.GIFT_MEDIUM))
			{
				options.Remove(options[num]);
			}
			else if (InventoryItem.IsHeart(options[num].itemToBuy) && PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				options.Remove(options[num]);
			}
		}
		SingleChoiceRewardOption[] array = otherOptions;
		foreach (SingleChoiceRewardOption singleChoiceRewardOption in array)
		{
			if (singleChoiceRewardOption.Option == null)
			{
				continue;
			}
			for (int num2 = options.Count - 1; num2 >= 0; num2--)
			{
				if (options[num2].itemToBuy == singleChoiceRewardOption.Option.itemToBuy)
				{
					options.RemoveAt(num2);
				}
				else if (!AllowDuplicateFood && InventoryItem.IsFood(options[num2].itemToBuy) && InventoryItem.IsFood(singleChoiceRewardOption.Option.itemToBuy))
				{
					options.RemoveAt(num2);
				}
				else if (InventoryItem.IsFish(options[num2].itemToBuy) && InventoryItem.IsFish(singleChoiceRewardOption.Option.itemToBuy))
				{
					options.RemoveAt(num2);
				}
				else if (InventoryItem.IsGiftOrNecklace(options[num2].itemToBuy) && InventoryItem.IsGiftOrNecklace(singleChoiceRewardOption.Option.itemToBuy))
				{
					options.RemoveAt(num2);
				}
				else if (options[num2].GroupID != -1 && options[num2].GroupID == singleChoiceRewardOption.Option.GroupID)
				{
					options.RemoveAt(num2);
				}
				else if (!AllowDecorationAndSkin && ((options[num2].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN && singleChoiceRewardOption.Option.itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION) || (options[num2].itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION && singleChoiceRewardOption.Option.itemToBuy == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN)))
				{
					options.RemoveAt(num2);
				}
			}
		}
		if (options.Count <= 0)
		{
			base.gameObject.SetActive(false);
			return;
		}
		Option = options[Random.Range(0, options.Count)];
		if (StartHidden && Option.itemToBuy == InventoryItem.ITEM_TYPE.BERRY)
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.BERRY;
				break;
			case FollowerLocation.Dungeon1_2:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.PUMPKIN;
				break;
			case FollowerLocation.Dungeon1_3:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.CAULIFLOWER;
				break;
			case FollowerLocation.Dungeon1_4:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.BEETROOT;
				break;
			}
		}
		if (StartHidden && Option.itemToBuy == InventoryItem.ITEM_TYPE.SEED)
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.SEED;
				break;
			case FollowerLocation.Dungeon1_2:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.SEED_PUMPKIN;
				break;
			case FollowerLocation.Dungeon1_3:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER;
				break;
			case FollowerLocation.Dungeon1_4:
				Option.itemToBuy = InventoryItem.ITEM_TYPE.SEED_BEETROOT;
				break;
			}
		}
		if (!Option.SingleQuantityItem)
		{
			Option.quantity = Mathf.Clamp(Option.quantity + Random.Range((int)randomOffset.x, (int)randomOffset.y), 1, int.MaxValue);
		}
		itemDisplay.SetImage(Option.itemToBuy);
		if (Option.quantity > 1)
		{
			QuantityText.text = "x" + Option.quantity;
		}
		else
		{
			QuantityText.text = "";
		}
		interaction.Init(Option.itemToBuy, Option.quantity);
		interaction.OnInteraction += OnInteraction;
		Debug.Log(("Option.itemToBuy: " + Option.itemToBuy).Colour(Color.green));
	}

	private void OnInteraction(StateMachine state)
	{
		ChoiceDisabled();
		SingleChoiceRewardOption[] array = otherOptions;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ChoiceDisabled();
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}

	public void ChoiceDisabled()
	{
		itemDisplay.gameObject.SetActive(false);
		QuantityText.gameObject.SetActive(false);
	}
}
