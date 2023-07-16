using UnityEngine;
using UnityEngine.Events;

public class RatauGiveBlackGoldToPlayer : BaseMonoBehaviour
{
	public UnityEvent Callback;

	public Interaction_Follower interaction_Follower;

	public void Play()
	{
		int num = interaction_Follower.Cost;
		Debug.Log("i Cost: " + num);
		while (--num >= 0)
		{
			Debug.Log("i " + num);
			if (num == 0)
			{
				ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, base.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, GiveGoldAndCallBack);
			}
			else
			{
				ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, base.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, GiveGold);
			}
		}
	}

	private void GiveGoldAndCallBack()
	{
		Debug.Log("A");
		GiveGold();
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
	}

	private void GiveGold()
	{
		Debug.Log("B");
		Inventory.AddItem(20, 1);
	}
}
