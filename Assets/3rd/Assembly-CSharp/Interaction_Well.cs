using System.Collections;
using UnityEngine;

public class Interaction_Well : Interaction
{
	public override void GetLabel()
	{
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (1 <= Inventory.GetItemQuantity(20))
		{
			StartCoroutine(GiveGold());
			return;
		}
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.transform.position);
		MonoSingleton<Indicator>.Instance.PlayShake();
	}

	private IEnumerator GiveGold()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
		ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
		Inventory.ChangeItemQuantity(20, -1);
		yield return new WaitForSeconds(1f);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.25f, 0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
	}
}
