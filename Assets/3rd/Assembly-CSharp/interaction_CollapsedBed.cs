using System.Collections;
using I2.Loc;
using UnityEngine;

public class interaction_CollapsedBed : Interaction
{
	[SerializeField]
	private Interaction_Bed bed;

	private float GoldCost = 1f;

	private float WoodCost = 3f;

	private string sRepair;

	private void Start()
	{
		UpdateLocalisation();
	}

	private string GetAffordColor()
	{
		if ((float)Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.LOG) >= WoodCost)
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sRepair = ScriptLocalization.Interactions.Repair;
	}

	public override void GetLabel()
	{
		base.Label = string.Join(" ", sRepair, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.LOG, (int)WoodCost));
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if ((float)Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.LOG) >= WoodCost)
		{
			StartCoroutine(InteractRoutine());
		}
		else
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator InteractRoutine()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		yield return new WaitForEndOfFrame();
		float i = WoodCost;
		while (true)
		{
			float num = i - 1f;
			i = num;
			if (!(num >= 0f))
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.CameraBone.transform.position, InventoryItem.ITEM_TYPE.LOG, null);
			yield return new WaitForSeconds(0.1f - 0.1f * (i / WoodCost));
		}
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		Vector3 position = base.transform.position;
		state.facingAngle = Utils.GetAngle(state.transform.position, position);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.2f, 0.5f);
		AudioManager.Instance.PlayOneShot("event:/material/dirt_dig", position);
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.PlayOneShot("event:/building/finished_wood", position);
		GameManager.GetInstance().OnConversationEnd();
		Inventory.ChangeItemQuantity(1, (int)(0f - WoodCost));
		bed.StructureBrain.Rebuild();
		bed.UpdateBed();
	}
}
