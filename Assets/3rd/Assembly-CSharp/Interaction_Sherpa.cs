using System.Collections;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_Sherpa : Interaction_Follower
{
	public bool PaidSherpa;

	public int SherpaCost = 2;

	public SkeletonAnimation Spine;

	protected override void Start()
	{
		base.Start();
		if (!DataManager.Instance.SherpaFirstConvo)
		{
			AutomaticallyInteract = true;
		}
		string forceSkin = "";
		if (DataManager.Instance.Followers.Count > 0)
		{
			forceSkin = DataManager.Instance.Followers[Random.Range(0, DataManager.Instance.Followers.Count)].SkinName;
		}
		Villager_Info villager_Info = Villager_Info.NewCharacter(forceSkin);
		villager_Info.Outfit = WorshipperInfoManager.Outfit.Sherpa;
		wim.SetV_I(villager_Info);
	}

	protected override void Update()
	{
		base.Update();
		Interactable = Inventory.itemsDungeon.Count > 0;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!DataManager.Instance.SherpaFirstConvo)
		{
			GetComponent<Interaction_SimpleConversation>().Play();
			AutomaticallyInteract = false;
			DataManager.Instance.SherpaFirstConvo = true;
		}
		else if (!Activated)
		{
			if (Inventory.GetItemQuantity(20) >= SherpaCost)
			{
				StartCoroutine(GiveLootIE());
				Activated = true;
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", PlayerFarming.Instance.gameObject);
				MonoSingleton<Indicator>.Instance.PlayShake();
			}
		}
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) > SherpaCost)
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	public override void GetLabel()
	{
		if (Inventory.GetItemQuantity(20) >= SherpaCost)
		{
			base.Label = GetAffordColor() + "<sprite name=\"icon_blackgold\"> " + Inventory.GetItemQuantity(20) + " / " + SherpaCost + "</color> - " + ScriptLocalization.Interactions.TakeLoot;
		}
	}

	private IEnumerator GiveLootIE()
	{
		Interactable = false;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		for (int i = 0; i < SherpaCost; i++)
		{
			Inventory.GetItemByType(20);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.gameObject.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			Inventory.ChangeItemQuantity(20, -1);
		}
		yield return new WaitForSeconds(2f);
		foreach (InventoryItem item in Inventory.itemsDungeon)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, (InventoryItem.ITEM_TYPE)item.type, null);
			Inventory.AddItem(item.type, item.quantity, true);
			yield return new WaitForSeconds(0.2f);
		}
		Inventory.ClearDungeonItems();
		yield return Spine.YieldForAnimation("Reactions/react-bow");
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
		Spine.AnimationState.SetAnimation(0, "spawn-out", false);
		yield return new WaitForSeconds(0.9f);
		CameraManager.shakeCamera(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		Object.Destroy(base.gameObject);
	}
}
