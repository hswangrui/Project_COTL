using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMTools;
using UnityEngine;

public class HubShoreManager : BaseMonoBehaviour
{
	public GameObject[] ShopKeepers;

	public Interaction_LighthouseBurner lighthouseBurner;

	public GameObject GemUnlit;

	public GameObject Gem;

	public GameObject Boat;

	public GameObject Demons;

	public GameObject HideAfterShopsAppear;

	public SpriteRenderer LighthouseDoorSpriteRenderer;

	public Sprite LighthouseDoorClosed;

	public Sprite LighthouseDoorOpen;

	public EnterBuilding LighthouseEntrance;

	public GameObject colliderAndBark;

	public Transform LighthouseKeeper;

	public RoomSwapManager RoomSwapManager;

	private bool Activated;

	public bool LighthouseLit;

	private int startingDay;

	public GameObject LambPropaganda;

	public GameObject CaughtFishConvo;

	public GameObject DidntCatchFishConvo;

	public GameObject FishermanTrader;

	public GameObject FishermanConvo;

	public GameObject Fisherman;

	public GameObject GiveCrystalQuestConvo;

	public GameObject CompleteCrystalQuestConvo;

	public List<GameObject> CrystalitemsToTurnOn = new List<GameObject>();

	public GameObject Shop;

	public void GiveObjectiveFixLighthouse()
	{
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/FixTheLighthouse", Objectives.CustomQuestTypes.FixTheLighthouse));
		CheckRequirements();
	}

	public void CompleteObjectiveFixLighthouse()
	{
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.FixTheLighthouse);
		DataManager.Instance.Lighthouse_Lit = true;
		StartCoroutine(GiveSkinAndOpenShop());
	}

	private IEnumerator GiveSkinAndOpenShop()
	{
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(Shop.gameObject, 6f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForSeconds(0.1f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForSeconds(1.5f);
		GotItem();
	}

	public void CheckMusicRoutine()
	{
		StartCoroutine(CheckMusic());
	}

	private IEnumerator CheckMusic()
	{
		yield return new WaitForSeconds(0.1f);
		if (DataManager.Instance.Lighthouse_Lit)
		{
			AudioManager.Instance.SetMusicRoomID(6, "shore_id");
		}
		else
		{
			AudioManager.Instance.SetMusicRoomID(0, "shore_id");
		}
	}

	private void GotItem()
	{
		OpenShop();
		CheckRequirements();
		GameManager.GetInstance().OnConversationEnd();
	}

	public void RevealShop()
	{
		OpenShop();
		Vector3 zero = Vector3.zero;
		Shop.transform.localPosition = new Vector3(0f, 0f, 1f);
		Shop.transform.DOLocalMove(zero, 2f).SetEase(Ease.OutBack);
		CultFaithManager.AddThought(Thought.Cult_PledgedToYou, -1, 1f);
	}

	private void OnEnable()
	{
		CheckRequirements();
		startingDay = TimeManager.CurrentDay;
		CheckHasDoneTutorial();
		CheckMusicRoutine();
		GiveCrystalQuestConvo.SetActive(!DataManager.Instance.Lighthouse_QuestGiven && DataManager.Instance.Lighthouse_LitFirstConvo);
		CompleteCrystalQuestConvo.SetActive(DataManager.Instance.Lighthouse_QuestGiven && !DataManager.Instance.CompletedLighthouseCrystalQuest && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL) >= 25);
		FishermanConvo.gameObject.SetActive(DataManager.Instance.CompletedLighthouseCrystalQuest);
	}

	public void OpenLighthouse()
	{
		LighthouseEntrance.gameObject.SetActive(true);
		colliderAndBark.SetActive(false);
		LighthouseDoorSpriteRenderer.sprite = LighthouseDoorOpen;
	}

	public void CheckHasDoneTutorial()
	{
		LighthouseEntrance.gameObject.SetActive(true);
		colliderAndBark.SetActive(false);
	}

	public void CheckRequirements()
	{
		if (DataManager.Instance.Lighthouse_Lit)
		{
			RoomSwapManager.TransitionOutRoomId = 6;
			Debug.Log("LighthouseLit");
			Gem.SetActive(true);
			GemUnlit.SetActive(false);
			SpawnShopKeeper();
			if (HideAfterShopsAppear != null)
			{
				HideAfterShopsAppear.SetActive(false);
			}
			OpenShop();
			Demons.SetActive(false);
		}
		else
		{
			Debug.Log("LighthouseUnlit");
			HideAfterShopsAppear.SetActive(true);
			DisableShopKeepers();
			CloseShop();
			GemUnlit.SetActive(true);
			Gem.SetActive(false);
		}
		if (DataManager.Instance.CompletedLighthouseCrystalQuest)
		{
			foreach (GameObject item in CrystalitemsToTurnOn)
			{
				item.gameObject.SetActive(true);
			}
			return;
		}
		foreach (GameObject item2 in CrystalitemsToTurnOn)
		{
			item2.gameObject.SetActive(false);
		}
	}

	public void OpenShop()
	{
		Shop.SetActive(true);
	}

	private void CloseShop()
	{
		Shop.SetActive(false);
	}

	private void DisableShopKeepers()
	{
		LambPropaganda.SetActive(false);
		GameObject[] shopKeepers = ShopKeepers;
		for (int i = 0; i < shopKeepers.Length; i++)
		{
			shopKeepers[i].SetActive(false);
		}
		Activated = false;
	}

	private void SpawnShopKeeper()
	{
		LambPropaganda.SetActive(true);
		GameObject[] shopKeepers = ShopKeepers;
		for (int i = 0; i < shopKeepers.Length; i++)
		{
			shopKeepers[i].SetActive(true);
		}
		Debug.Log("ActivateShopKeeper");
		Activated = true;
	}

	private void Update()
	{
		if (startingDay != TimeManager.CurrentDay)
		{
			CheckRequirements();
			startingDay = TimeManager.CurrentDay;
		}
	}

	public void GiveCrystalQuest()
	{
		ObjectiveManager.Add(new Objectives_CollectItem("Objectives/GroupTitles/CrystalForLighthouse", InventoryItem.ITEM_TYPE.CRYSTAL, 25, true, FollowerLocation.Dungeon1_3)
		{
			CustomTerm = "Objectives/Custom/CrystalForLighthouse"
		});
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL) >= 25)
		{
			CompleteCrystalQuestConvo.GetComponent<Interaction_SimpleConversation>().AutomaticallyInteract = false;
			CompleteCrystalQuestConvo.SetActive(true);
		}
	}

	public void CompleteCrystalQuest()
	{
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.LighthouseReturn);
		Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL, -25);
		DataManager.Instance.CompletedLighthouseCrystalQuest = true;
		StartCoroutine(GiveItemsRoutine(InventoryItem.ITEM_TYPE.CRYSTAL, 25));
	}

	private IEnumerator GiveItemsRoutine(InventoryItem.ITEM_TYPE itemType, int quantity)
	{
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		CheckRequirements();
		GameManager.GetInstance().OnConversationNew();
		for (int i = 0; i < Mathf.Max(quantity, 10); i++)
		{
			ResourceCustomTarget.Create(CompleteCrystalQuestConvo, PlayerFarming.Instance.transform.position, itemType, null);
			yield return new WaitForSeconds(0.025f);
		}
		StartCoroutine(GiveCrystalSkin());
	}

	private IEnumerator GiveCrystalSkin()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForSeconds(0.1f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		bool waiting = true;
		FollowerSkinCustomTarget.Create(lighthouseBurner.leader.gameObject.transform.position, PlayerFarming.Instance.gameObject.transform.position, 0.5f, "Axolotl", delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForEndOfFrame();
		FishermanConvo.SetActive(true);
	}
}
