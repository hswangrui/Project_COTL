using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spiderShop : MonoBehaviour
{
	[SerializeField]
	private GameObject returnCustomer0;

	[SerializeField]
	private GameObject returnCustomer1;

	[SerializeField]
	private GameObject badFollower;

	[SerializeField]
	private GameObject postGameConvo;

	[SerializeField]
	private Interaction_FollowerInSpiderWeb shopKeeper;

	public List<FollowersToBuy> followersToBuy = new List<FollowersToBuy>();

	[SerializeField]
	private Transform SpiderSeller;

	public int TotalCount;

	private bool gotOne;

	private FollowersToBuy pickedFollower;

	private void ChooseFollower()
	{
		while (!gotOne)
		{
			foreach (FollowersToBuy item in followersToBuy)
			{
				if (UnityEngine.Random.Range(0, 100) < item.chanceToSpawn)
				{
					pickedFollower = item;
					gotOne = true;
				}
			}
		}
		shopKeeper.followerToBuy = pickedFollower;
		switch (pickedFollower.followerTypes)
		{
		case FollowersToBuy.FollowerBuyTypes.Old:
			shopKeeper.SetOld();
			break;
		case FollowersToBuy.FollowerBuyTypes.Ill:
			shopKeeper.SetIll();
			break;
		case FollowersToBuy.FollowerBuyTypes.Faithful:
			shopKeeper.SetFaithful();
			break;
		}
	}

	private void Start()
	{
		ChooseFollower();
		Debug.Log("s_ " + PlayerFarming.Location);
		Debug.Log("s_ Beaten dungeon 1? " + DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1));
		if (PlayerFarming.Location == FollowerLocation.Base && !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
		{
			base.gameObject.SetActive(false);
		}
		TotalCount = DataManager.Instance.FollowerShopUses;
		if (!DataManager.Instance.DeathCatBeaten && postGameConvo != null)
		{
			UnityEngine.Object.Destroy(postGameConvo);
		}
	}

	public void BoughtFollower()
	{
		if (shopKeeper.IsDungeon)
		{
			DataManager.Instance.FollowerShopUses++;
			TotalCount = DataManager.Instance.FollowerShopUses;
		}
	}

	public void GiveTarot()
	{
		StartCoroutine(GiveTarotRoutine());
	}

	private IEnumerator GiveTarotRoutine()
	{
		DataManager.Instance.FollowerShopUses++;
		shopKeeper.Interactable = false;
		yield return new WaitForEndOfFrame();
		Debug.Log("Spawn Tarot");
		GameManager.GetInstance().OnConversationNew();
		PlayerFarming.Instance._state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameObject speaker = TarotCustomTarget.Create(SpiderSeller.transform.position, PlayerFarming.Instance.transform.position, 1.5f, TarotCards.Card.Arrows, pickedUp).gameObject;
		GameManager.GetInstance().OnConversationNext(speaker);
	}

	private void pickedUp()
	{
		GameManager.GetInstance().OnConversationEnd();
		PlayerFarming.Instance._state.CURRENT_STATE = StateMachine.State.Idle;
		shopKeeper.Interactable = true;
	}

	public void CheckCount()
	{
		if (!shopKeeper.IsDungeon)
		{
			return;
		}
		int followerShopUses = DataManager.Instance.FollowerShopUses;
		SimpleBark component = shopKeeper.normalBark.gameObject.GetComponent<SimpleBark>();
		if (followerShopUses > 3 && !DataManager.Instance.PlayerFoundTrinkets.Contains(TarotCards.Card.Arrows))
		{
			component.Close();
			returnCustomer1.SetActive(true);
			return;
		}
		switch (followerShopUses)
		{
		case 1:
			Debug.Log("Free Follower");
			shopKeeper.free = true;
			component.Close();
			returnCustomer0.SetActive(true);
			break;
		case 5:
			shopKeeper.SetOld();
			shopKeeper.free = true;
			badFollower.SetActive(true);
			break;
		case 10:
			shopKeeper.SetIll();
			shopKeeper.free = true;
			badFollower.SetActive(true);
			break;
		case 3:
			component.Close();
			returnCustomer1.SetActive(true);
			break;
		case 8:
			component.Close();
			shopKeeper.free = true;
			returnCustomer0.SetActive(true);
			break;
		}
	}

	private void OnEnable()
	{
		Interaction_FollowerInSpiderWeb interaction_FollowerInSpiderWeb = shopKeeper;
		interaction_FollowerInSpiderWeb.FollowerCreated = (Action)Delegate.Combine(interaction_FollowerInSpiderWeb.FollowerCreated, new Action(CheckCount));
		Interaction_FollowerInSpiderWeb interaction_FollowerInSpiderWeb2 = shopKeeper;
		interaction_FollowerInSpiderWeb2.BoughtFollowerCallback = (Action)Delegate.Combine(interaction_FollowerInSpiderWeb2.BoughtFollowerCallback, new Action(BoughtFollower));
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(CheckIfShouldShow));
	}

	private void OnDisable()
	{
		Interaction_FollowerInSpiderWeb interaction_FollowerInSpiderWeb = shopKeeper;
		interaction_FollowerInSpiderWeb.FollowerCreated = (Action)Delegate.Remove(interaction_FollowerInSpiderWeb.FollowerCreated, new Action(CheckCount));
		Interaction_FollowerInSpiderWeb interaction_FollowerInSpiderWeb2 = shopKeeper;
		interaction_FollowerInSpiderWeb2.BoughtFollowerCallback = (Action)Delegate.Remove(interaction_FollowerInSpiderWeb2.BoughtFollowerCallback, new Action(BoughtFollower));
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(CheckIfShouldShow));
	}

	private void CheckIfShouldShow()
	{
		Debug.Log("s_ ========= Check if I should show");
		Debug.Log("s_PlayerFarming.Location: " + PlayerFarming.Location);
		Debug.Log("s_DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_2): " + DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_2));
		if (PlayerFarming.Location == FollowerLocation.Base && !DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_2) && !DataManager.Instance.DeathCatBeaten)
		{
			Debug.Log("s_Turn him off!!");
			base.gameObject.SetActive(false);
		}
	}
}
