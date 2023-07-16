using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_BuyItem : Interaction
{
	private bool Activated;

	private int devotionCost = 10;

	public BuyEntry itemForSale;

	public bool customItemForSale;

	public UnityEvent CallBack;

	[Range(0f, 100f)]
	public int chanceOfSale = 7;

	public bool SaleIsOn;

	public int SaleAmount;

	public float saleAmountFloat;

	private string BuyString;

	private string off;

	public bool UsingDecorations;

	private float AmountToDiscount;

	public shopKeeperManager shopKeeperManager;

	public bool DoublePrice;

	public Action OnItemBought;

	private bool createdDiscount;

	private int randomChanceOfSale;

	public GameObject DecorationItem;

	private string DecorationName = string.Empty;

	private string SaleText = string.Empty;

	private Vector3 BookTargetPosition;

	private float Timer;

	public GameObject PlayerPosition;

	public List<GameObject> clones;

	public GameObject spawnObject;

	private void Start()
	{
		shopKeeperManager = GetComponentInParent<shopKeeperManager>();
		UpdateLocalisation();
		ActivateDistance = 2f;
		SaleIsOn = false;
		SaleAmount = 0;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		randomChanceOfSale = UnityEngine.Random.Range(0, 100);
	}

	public int GetCost()
	{
		if (randomChanceOfSale <= chanceOfSale && !DoublePrice)
		{
			if (!createdDiscount)
			{
				Debug.Log("Random chance of sale = " + randomChanceOfSale + "Chance: " + chanceOfSale);
				devotionCost = itemForSale.itemCost;
				SaleIsOn = true;
				SaleAmount = UnityEngine.Random.Range(1, 5);
				saleAmountFloat = SaleAmount;
				saleAmountFloat *= 0.1f;
				Debug.Log("Amount to discount = " + devotionCost + " - " + saleAmountFloat);
				AmountToDiscount = Mathf.Round((float)devotionCost - (float)devotionCost * saleAmountFloat);
				if (AmountToDiscount < 1f)
				{
					AmountToDiscount = 1f;
				}
				createdDiscount = true;
			}
			return devotionCost = (int)AmountToDiscount;
		}
		if (SaleIsOn)
		{
			AmountToDiscount = Mathf.Round((float)itemForSale.itemCost - (float)itemForSale.itemCost * saleAmountFloat);
			return devotionCost = (int)AmountToDiscount;
		}
		if (DoublePrice)
		{
			return itemForSale.itemCost * 2;
		}
		return itemForSale.itemCost;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		BuyString = ScriptLocalization.Interactions.Buy;
		off = ScriptLocalization.UI_Generic.Off;
	}

	public void GetDecoration()
	{
		GetComponent<InventoryItemDisplay>().SetImage(TypeAndPlacementObjects.GetByType(itemForSale.decorationToBuy).IconImage, false);
		base.transform.localScale = Vector3.one * 0.12f;
	}

	public override void GetLabel()
	{
		if (itemForSale.Decoration && itemForSale.decorationToBuy == StructureBrain.TYPES.NONE && itemForSale.Decoration)
		{
			Interactable = false;
			return;
		}
		if (itemForSale.Decoration)
		{
			DecorationName = StructuresData.LocalizedName(itemForSale.decorationToBuy);
		}
		else if (itemForSale.TarotCard)
		{
			DecorationName = TarotCards.LocalisedName(itemForSale.Card);
		}
		else
		{
			DecorationName = InventoryItem.LocalizedName(itemForSale.itemToBuy);
		}
		if (DoublePrice)
		{
			SaleText = "200%".Colour(Color.red);
		}
		else if (SaleIsOn)
		{
			if (SaleAmount == 0)
			{
				SaleText = string.Empty;
			}
			else
			{
				float num = SaleAmount * 10;
				SaleText = string.Format(off, num).Colour(Color.yellow);
			}
		}
		if (!SaleIsOn)
		{
			SaleText = string.Empty;
		}
		string buy = ScriptLocalization.UI_ItemSelector_Context.Buy;
		buy = ((itemForSale.quantity <= 1) ? string.Format(buy, DecorationName, CostFormatter.FormatCost(itemForSale.costType, GetCost())) : string.Format(buy, string.Join(" ", itemForSale.quantity, DecorationName), CostFormatter.FormatCost(itemForSale.costType, GetCost())));
		if (!string.IsNullOrEmpty(SaleText))
		{
			buy = string.Join(" ", buy, SaleText);
		}
		base.Label = buy;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Inventory.GetItemQuantity((int)itemForSale.costType) >= GetCost() && !Activated)
		{
			AudioManager.Instance.PlayOneShot("event:/shop/buy", base.gameObject);
			Activated = true;
			boughtItem();
			Interactable = false;
			HUD_Manager.Instance.Hide(false);
			for (int i = 0; i < GetCost(); i++)
			{
				if (i < 10)
				{
					Inventory.GetItemByType((int)itemForSale.costType);
					AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
					ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.gameObject.transform.position, itemForSale.costType, null);
				}
				Inventory.ChangeItemQuantity((int)itemForSale.costType, -1);
			}
			if (itemForSale.TarotCard || itemForSale.Decoration)
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			}
			StartCoroutine(DelayOnActivate());
			CameraManager.shakeCamera(0.3f, UnityEngine.Random.Range(0, 360));
		}
		else
		{
			cantAfford();
		}
	}

	public void AlreadyBought()
	{
		base.gameObject.SetActive(false);
	}

	private void cantAfford()
	{
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
		MonoSingleton<Indicator>.Instance.PlayShake();
		if (shopKeeperManager.CantAffordBark != null)
		{
			if (shopKeeperManager.NormalBark != null)
			{
				shopKeeperManager.NormalBark.SetActive(false);
			}
			shopKeeperManager.CantAffordBark.SetActive(true);
		}
	}

	private void boughtItem()
	{
		if (shopKeeperManager.BoughtItemBark != null)
		{
			if (shopKeeperManager.NormalBark != null)
			{
				shopKeeperManager.NormalBark.SetActive(false);
			}
			shopKeeperManager.BoughtItemBark.SetActive(true);
		}
	}

	private IEnumerator DelayOnActivate()
	{
		yield return new WaitForSeconds(0.25f);
		Activate();
	}

	private IEnumerator BoughtCustomItem()
	{
		yield return new WaitForSeconds(0.2f);
		UnityEvent callBack = CallBack;
		if (callBack != null)
		{
			callBack.Invoke();
		}
	}

	protected override void Update()
	{
		base.Update();
		bool flag = PlayerFarming.Instance == null;
	}

	public void updateQuantity()
	{
		clones.Clear();
		if (itemForSale.quantity <= 1)
		{
			return;
		}
		SpriteRenderer component = base.gameObject.GetComponent<SpriteRenderer>();
		for (int i = 0; i < itemForSale.quantity; i++)
		{
			if (spawnObject != null)
			{
				Vector3 position = new Vector3(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.025f, 0.025f), 0f) + base.transform.position;
				GameObject gameObject = UnityEngine.Object.Instantiate(spawnObject, position, base.transform.rotation, base.transform.parent);
				gameObject.SetActive(true);
				gameObject.GetComponent<SpriteRenderer>().sprite = component.sprite;
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				clones.Add(gameObject);
			}
		}
	}

	private void Activate()
	{
		Debug.Log("activate");
		Action onItemBought = OnItemBought;
		if (onItemBought != null)
		{
			onItemBought();
		}
		if (customItemForSale)
		{
			return;
		}
		if (itemForSale.Decoration)
		{
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
			RumbleManager.Instance.Rumble();
			StructuresData.CompleteResearch(itemForSale.decorationToBuy);
			StructuresData.SetRevealed(itemForSale.decorationToBuy);
			foreach (BuyEntry item in shopKeeperManager.shop.itemsForSale)
			{
				if (item.decorationToBuy == itemForSale.decorationToBuy)
				{
					item.Bought = true;
					break;
				}
			}
			DataManager.Instance.UpdateShop(shopKeeperManager.shop);
			UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
			uINewItemOverlayController.pickedBuilding = itemForSale.decorationToBuy;
			uINewItemOverlayController.Show(UINewItemOverlayController.TypeOfCard.Decoration, base.transform.position, false);
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (itemForSale.TarotCard)
		{
			BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
			RumbleManager.Instance.Rumble();
			TarotCustomTarget.Create(base.transform.position, PlayerFarming.Instance.transform.position, 0f, itemForSale.Card, null);
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		PickUp pickUp = null;
		if (itemForSale.quantity > 1)
		{
			for (int i = 0; i < clones.Count; i++)
			{
				Debug.Log("Spawn: " + itemForSale.itemToBuy);
				InventoryItem.Spawn(itemForSale.itemToBuy, 1, base.transform.position);
				UnityEngine.Object.Destroy(clones[i]);
			}
		}
		else
		{
			pickUp = InventoryItem.Spawn(itemForSale.itemToBuy, 1, base.transform.position);
		}
		itemForSale.Bought = true;
		if (shopKeeperManager != null)
		{
			foreach (BuyEntry item2 in shopKeeperManager.shop.itemsForSale)
			{
				if (item2.itemToBuy == itemForSale.itemToBuy)
				{
					item2.Bought = true;
				}
			}
			if (pickUp != null)
			{
				pickUp.transform.position = PlayerFarming.Instance.transform.position;
				pickUp.MagnetToPlayer = true;
				pickUp.GetComponent<Interaction>().AutomaticallyInteract = true;
			}
			DataManager.Instance.UpdateShop(shopKeeperManager.shop);
		}
		base.gameObject.SetActive(false);
	}
}
