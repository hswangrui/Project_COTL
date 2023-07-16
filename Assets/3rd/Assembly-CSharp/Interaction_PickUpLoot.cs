using I2.Loc;
using UnityEngine;

public class Interaction_PickUpLoot : Interaction
{
	private InventoryItem.ITEM_TYPE itemType;

	private int quantity;

	public void Init(InventoryItem.ITEM_TYPE itemType, int quantity)
	{
		this.itemType = itemType;
		this.quantity = quantity;
		if (itemType == InventoryItem.ITEM_TYPE.NONE)
		{
			base.gameObject.SetActive(false);
		}
	}

	public override void GetLabel()
	{
		if (quantity <= 1)
		{
			base.Label = ScriptLocalization.Interactions.Choose + " " + FontImageNames.GetIconByType(itemType) + " " + InventoryItem.LocalizedName(itemType);
		}
		else
		{
			base.Label = string.Format("{0} x{1}", ScriptLocalization.Interactions.Choose + " " + FontImageNames.GetIconByType(itemType) + " " + InventoryItem.LocalizedName(itemType), quantity);
		}
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		AudioManager.Instance.PlayOneShot("event:/ui/arrow_change_selection", base.gameObject);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		for (int i = 0; i < quantity; i++)
		{
			PickUp pickUp = InventoryItem.Spawn(itemType, 1, base.transform.position + Vector3.back, 0f);
			if (quantity == 1)
			{
				pickUp.SetInitialSpeedAndDiraction(5f, 270f);
			}
			else
			{
				pickUp.SetInitialSpeedAndDiraction(4f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
			pickUp.MagnetDistance = 3f;
			pickUp.CanStopFollowingPlayer = false;
			InventoryItem.ITEM_TYPE iTEM_TYPE = itemType;
			if (iTEM_TYPE == InventoryItem.ITEM_TYPE.DOCTRINE_STONE)
			{
				Interaction_DoctrineStone component = pickUp.GetComponent<Interaction_DoctrineStone>();
				if (component != null)
				{
					component.MagnetToPlayer();
				}
			}
			else
			{
				FoundItemPickUp component2 = pickUp.GetComponent<FoundItemPickUp>();
				if (component2 != null)
				{
					component2.MagnetToPlayer();
				}
			}
		}
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
		base.enabled = false;
	}
}
