using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Interaction_Apothecary : Interaction_Cook
{
	private List<InventoryItem> item
	{
		get
		{
			return structure.Structure_Info.Inventory;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sCook = "Brew";
		sCancel = ScriptLocalization.Interactions.Cancel;
	}

	public override IEnumerator CookFood()
	{
		loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/cooking/cooking_loop", base.gameObject, true);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().AddPlayerToCamera();
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.2f, 0.5f);
		if (CookingDuration > 0.5f)
		{
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		}
		yield return new WaitForSeconds(CookingDuration);
		InventoryItem.ITEM_TYPE MealToCreate = InventoryItem.ITEM_TYPE.BLACK_GOLD;
		if (item[0].type == 55 && item[1].type == 55 && item[2].type == 55)
		{
			MealToCreate = InventoryItem.ITEM_TYPE.Necklace_1;
		}
		if (item[0].type == 56 && item[1].type == 56 && item[2].type == 56)
		{
			MealToCreate = InventoryItem.ITEM_TYPE.Necklace_2;
		}
		if (item[0].type == 9 && item[1].type == 9 && item[2].type == 9)
		{
			MealToCreate = InventoryItem.ITEM_TYPE.Necklace_1;
		}
		ResourceCustomTarget.Create(state.gameObject, base.transform.position, MealToCreate, delegate
		{
			Inventory.AddItem((int)MealToCreate, 1);
		});
		AudioManager.Instance.PlayOneShot("event:/cooking/meal_cooked", base.transform.position);
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		structure.Structure_Info.Inventory.Clear();
		cauldron.enabled = true;
		base.enabled = false;
	}
}
