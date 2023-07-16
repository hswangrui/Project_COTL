using System.Collections;
using Spine.Unity;
using UnityEngine;

public class Interaction_DeathNPC : Interaction
{
	public SkeletonAnimation ShopKeeperSpine;

	public SkeletonAnimation FollowerSpine;

	private bool Activated;

	private FollowerInfo _followerInfo;

	private FollowerOutfit _outfit;

	public Interaction_SimpleConversation Conversation;

	public static Interaction_DeathNPC Instance;

	public GameObject Notification;

	public override void OnEnableInteraction()
	{
		if (DataManager.Instance.FollowerTokens > 0 && !DataManager.Instance.firstRecruit)
		{
			Notification.SetActive(true);
		}
		else
		{
			Notification.SetActive(false);
		}
		base.OnEnableInteraction();
		Instance = this;
	}

	private void Start()
	{
		if (!DataManager.Instance.BlackSoulsEnabled)
		{
			base.enabled = false;
		}
		else
		{
			Object.Destroy(Conversation);
		}
		NewRecruitable();
	}

	private void NewRecruitable()
	{
		_followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Church);
		_outfit = new FollowerOutfit(_followerInfo);
		_outfit.SetOutfit(FollowerSpine, false);
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : "<sprite name=\"icon_Followers\"> x1");
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (DataManager.Instance.FollowerTokens >= 1)
		{
			StartCoroutine(Purchase());
		}
		else
		{
			SpiderAnimationCantAfford();
		}
	}

	private IEnumerator Purchase()
	{
		Activated = true;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		for (int i = 0; i < 1; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", state.transform.position);
			ResourceCustomTarget.Create(base.gameObject, state.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			yield return new WaitForSeconds(0.1f);
		}
		Inventory.FollowerTokens--;
		SpiderAnimationBoughtItem();
		yield return new WaitForSeconds(1f);
		Notification.SetActive(false);
		GameManager.GetInstance().OnConversationNext(ChurchFollowerManager.Instance.RitualCenterPosition.gameObject, 4f);
		yield return new WaitForSeconds(1f);
		CameraManager.shakeCamera(0.5f, Random.Range(0, 360));
		NewRecruitable();
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		GameManager.GetInstance().AddPlayerToCamera();
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		Activated = false;
	}

	private void SpiderAnimationBoughtItem()
	{
	}

	private void SpiderAnimationCantAfford()
	{
	}
}
