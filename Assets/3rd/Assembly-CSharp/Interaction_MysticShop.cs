using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using MMTools;
using Spine.Unity;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using src.Utilities;
using UnityEngine;

public class Interaction_MysticShop : Interaction
{
	private const int maxAmountOfTalismanPieces = 12;

	private const int maxAmountOfCrystalDoctrines = 20;

	[SerializeField]
	private Interaction_SimpleConversation onboardGodTears;

	[SerializeField]
	private Interaction_SimpleConversation onboardEndlessModeA;

	[SerializeField]
	private Interaction_SimpleConversation onboardEndlessModeB;

	[SerializeField]
	private Interaction_SimpleConversation[] enterMysticDimension;

	[SerializeField]
	private Interaction_SimpleConversation beatenLeshy;

	[SerializeField]
	private Interaction_SimpleConversation beatenHeket;

	[SerializeField]
	private Interaction_SimpleConversation beatenKallamar;

	[SerializeField]
	private Interaction_SimpleConversation beatenShamura;

	[SerializeField]
	private Interaction_SimpleConversation beatenAllA;

	[SerializeField]
	private Interaction_SimpleConversation beatenAllB;

	[SerializeField]
	private Interaction_SimpleConversation beatenAllC;

	[SerializeField]
	private Interaction_SimpleConversation deathCatConvo;

	[SerializeField]
	private Interaction_SimpleConversation firstPurchaseConvo;

	[SerializeField]
	private SimpleBark boughtBark;

	[SerializeField]
	private SimpleBark noGodTearsBark;

	[SerializeField]
	private SimpleSetCamera simpleCamera;

	[SerializeField]
	private SimpleSetCamera simpleCameraTwo;

	[SerializeField]
	private Reveal_MysticShop mysticShopReveal;

	[SerializeField]
	private SkeletonAnimation mysticKeeperSpine;

	[SerializeField]
	private GameObject cameraPos;

	[SerializeField]
	private GameObject followerToSpawn;

	[SerializeField]
	private SkeletonAnimation portalSpine;

	[SerializeField]
	private ParticleSystem recruitParticles;

	[SerializeField]
	private GameObject deathCatTargetPosition;

	[Header("God Tear Sequencing")]
	[SerializeField]
	private GameObject _godTearPrefab;

	[SerializeField]
	private Transform _godTearTarget;

	[SerializeField]
	private Interaction_KeyPiece _keyPiecePrefab;

	[SerializeField]
	private GameObject LUTOverlay;

	private bool waitingForContinueSequence = true;

	private int GodTearCost = 1;

	private List<string> possibleSkins;

	private List<InventoryItem.ITEM_TYPE> necklaces;

	private List<InventoryItem.ITEM_TYPE> allNecklaces = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.Necklace_Dark,
		InventoryItem.ITEM_TYPE.Necklace_Demonic,
		InventoryItem.ITEM_TYPE.Necklace_Gold_Skull,
		InventoryItem.ITEM_TYPE.Necklace_Light,
		InventoryItem.ITEM_TYPE.Necklace_Loyalty,
		InventoryItem.ITEM_TYPE.Necklace_Missionary
	};

	[SerializeField]
	private Transform playerPosition;

	private EventInstance loopedSound;

	public GameObject LightingOverrideAngry;

	private EventInstance loopedSoundDeathCat;

	private string mysticKeeperName
	{
		get
		{
			if (string.IsNullOrEmpty(DataManager.Instance.MysticKeeperName))
			{
				return "???";
			}
			return DataManager.Instance.MysticKeeperName;
		}
		set
		{
			DataManager.Instance.MysticKeeperName = value;
			SetConvosName();
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		SetCost();
	}

	private void TestGodTear()
	{
		onboardGodTears.enabled = true;
		onboardGodTears.AutomaticallyInteract = true;
		simpleCamera.enabled = false;
	}

	private void TestOnboardEndless()
	{
		onboardEndlessModeA.enabled = true;
		foreach (ConversationEntry entry in onboardEndlessModeA.Entries)
		{
			entry.CharacterName = mysticKeeperName;
		}
	}

	private void Start()
	{
		onboardGodTears.enabled = false;
		onboardEndlessModeA.enabled = false;
		onboardEndlessModeB.enabled = false;
		if (DataManager.Instance.OnboardedMysticShop && !DataManager.Instance.OnboardedGodTear && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.GOD_TEAR) >= 1)
		{
			onboardGodTears.enabled = true;
			onboardGodTears.AutomaticallyInteract = true;
			simpleCamera.enabled = false;
		}
		else if (!DataManager.Instance.OnboardedEndlessMode && DataManager.GetGodTearNotchesTotal() >= 4)
		{
			onboardEndlessModeA.enabled = true;
			foreach (ConversationEntry entry in onboardEndlessModeA.Entries)
			{
				entry.CharacterName = mysticKeeperName;
			}
		}
		else if (!DataManager.Instance.MysticKeeperBeatenLeshy && DataManager.Instance.BeatenLeshyLayer2)
		{
			beatenLeshy.enabled = true;
		}
		else if (!DataManager.Instance.MysticKeeperBeatenHeket && DataManager.Instance.BeatenHeketLayer2)
		{
			beatenHeket.enabled = true;
		}
		else if (!DataManager.Instance.MysticKeeperBeatenKallamar && DataManager.Instance.BeatenKallamarLayer2)
		{
			beatenKallamar.enabled = true;
		}
		else if (!DataManager.Instance.MysticKeeperBeatenShamura && DataManager.Instance.BeatenShamuraLayer2)
		{
			beatenShamura.enabled = true;
		}
		if (!DataManager.Instance.MysticKeeperBeatenAll && DataManager.Instance.BeatenLeshyLayer2 && DataManager.Instance.BeatenHeketLayer2 && DataManager.Instance.BeatenKallamarLayer2 && DataManager.Instance.BeatenShamuraLayer2)
		{
			beatenAllA.enabled = true;
			if (StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5))
			{
				beatenAllA.Entries[3].TermToSpeak = "Conversation_NPC/MysticShopSeller/BeatenAll/3_Follower";
			}
			else
			{
				beatenAllA.Entries[3].TermToSpeak = "Conversation_NPC/MysticShopSeller/BeatenAll/3_Shrine";
			}
		}
		if (DataManager.Instance.OnboardedMysticShop)
		{
			ActivateDistance = 6f;
		}
		SetConvosName();
	}

	private void SetConvosName()
	{
		Interaction_SimpleConversation[] array = enterMysticDimension;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (ConversationEntry entry in array[i].Entries)
			{
				entry.CharacterName = mysticKeeperName;
			}
		}
		foreach (ConversationEntry entry2 in boughtBark.Entries)
		{
			entry2.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry3 in noGodTearsBark.Entries)
		{
			entry3.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry4 in beatenLeshy.Entries)
		{
			entry4.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry5 in beatenHeket.Entries)
		{
			entry5.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry6 in beatenKallamar.Entries)
		{
			entry6.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry7 in beatenShamura.Entries)
		{
			entry7.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry8 in beatenAllA.Entries)
		{
			entry8.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry9 in beatenAllB.Entries)
		{
			entry9.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry10 in beatenAllC.Entries)
		{
			entry10.CharacterName = mysticKeeperName;
		}
		foreach (ConversationEntry entry11 in firstPurchaseConvo.Entries)
		{
			entry11.CharacterName = mysticKeeperName;
		}
	}

	public override void GetLabel()
	{
		base.GetLabel();
		if (DataManager.Instance.OnboardedMysticShop)
		{
			if (DataManager.Instance.OnboardedGodTear)
			{
				AutomaticallyInteract = false;
				base.Label = LocalizationManager.GetTranslation("Interactions/GiveGodTear") + ": " + InventoryItem.CapacityString(InventoryItem.ITEM_TYPE.GOD_TEAR, GodTearCost);
			}
			else
			{
				base.Label = "";
			}
		}
		else
		{
			base.Label = ".";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		if (DataManager.Instance.OnboardedMysticShop && DataManager.Instance.OnboardedGodTear)
		{
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.GOD_TEAR) >= GodTearCost)
			{
				StartCoroutine(DoRewardSequence());
				return;
			}
			MonoSingleton<Indicator>.Instance.PlayShake();
			noGodTearsBark.gameObject.SetActive(true);
			noGodTearsBark.Show();
			Interactable = true;
		}
		else
		{
			mysticShopReveal.Reveal();
			base.enabled = false;
			ActivateDistance = 6f;
		}
	}

	public void NameMysticKeeper()
	{
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.MysticShopReturn);
		UIMysticSellerNameMenuController mysticSellerNameMenu = MonoSingleton<UIManager>.Instance.MysticSellerNameMenuTemplate.Instantiate();
		mysticSellerNameMenu.Show(false, false);
		UIMysticSellerNameMenuController uIMysticSellerNameMenuController = mysticSellerNameMenu;
		uIMysticSellerNameMenuController.OnNameConfirmed = (Action<string>)Delegate.Combine(uIMysticSellerNameMenuController.OnNameConfirmed, (Action<string>)delegate(string name)
		{
			mysticKeeperName = name;
			List<ConversationEntry> list = new List<ConversationEntry>();
			list.Add(new ConversationEntry(cameraPos, LocalizationManager.GetTranslation("Conversation_NPC/MysticShopSeller/GodTearIntro/4")));
			list[0].CharacterName = mysticKeeperName;
			list[0].SkeletonData = mysticKeeperSpine;
			list[0].SetZoom = true;
			list[0].Zoom = 8f;
			list[0].Offset = new Vector3(0f, 2f, 0f);
			MMConversation.Play(new ConversationObject(list, null, delegate
			{
				simpleCamera.enabled = true;
				StopMusic();
			}));
		});
		UIMysticSellerNameMenuController uIMysticSellerNameMenuController2 = mysticSellerNameMenu;
		uIMysticSellerNameMenuController2.OnHide = (Action)Delegate.Combine(uIMysticSellerNameMenuController2.OnHide, (Action)delegate
		{
		});
		UIMysticSellerNameMenuController uIMysticSellerNameMenuController3 = mysticSellerNameMenu;
		uIMysticSellerNameMenuController3.OnHidden = (Action)Delegate.Combine(uIMysticSellerNameMenuController3.OnHidden, (Action)delegate
		{
			mysticSellerNameMenu = null;
		});
		DataManager.Instance.OnboardedGodTear = true;
	}

	public void GiveDefeatBishopsQuest()
	{
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/NewGamePlus", Objectives.CustomQuestTypes.NewGamePlus1), true);
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/NewGamePlus", Objectives.CustomQuestTypes.NewGamePlus2), true);
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/NewGamePlus", Objectives.CustomQuestTypes.NewGamePlus3), true);
		ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/NewGamePlus", Objectives.CustomQuestTypes.NewGamePlus4), true);
		base.enabled = true;
		DataManager.Instance.UnlockedBossTempleDoor.Clear();
		DataManager.Instance.OnboardedLayer2 = true;
	}

	public void UnlockEndlessMode()
	{
		simpleCamera.Reset();
		simpleCamera.AutomaticallyActivate = false;
		onboardGodTears.enabled = false;
		CrownStatueController.Instance.EndlessModeOnboarded(delegate
		{
			foreach (ConversationEntry entry in onboardEndlessModeB.Entries)
			{
				entry.CharacterName = mysticKeeperName;
			}
			onboardEndlessModeB.Entries[onboardEndlessModeB.Entries.Count - 1].TermToSpeak = string.Format(LocalizationManager.GetTranslation("Conversation_NPC/MysticShopSeller/EndlessModeIntro/4"), mysticKeeperName);
			onboardEndlessModeB.Play();
			onboardEndlessModeB.Callback.AddListener(delegate
			{
				StopMusic();
				simpleCamera.AutomaticallyActivate = false;
			});
		});
	}

	private IEnumerator DoRewardSequence()
	{
		PlayMusic();
		simpleCameraTwo.enabled = false;
		GameManager.GetInstance().OnConversationNew();
		Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.GOD_TEAR, -GodTearCost);
		GameObject godTear = UnityEngine.Object.Instantiate(_godTearPrefab, PlayerFarming.Instance.transform.position + new Vector3(0f, 1f, -1f), Quaternion.identity, base.transform.parent);
		godTear.transform.localScale = Vector3.zero;
		godTear.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
		GameManager.GetInstance().OnConversationNext(godTear, 5.5f);
		yield return new WaitForSeconds(1.25f);
		LUTOverlay.gameObject.SetActive(true);
		AudioManager.Instance.PlayOneShot("event:/player/float_follower", godTear);
		yield return new WaitForSeconds(0.25f);
		Vector3 position = _godTearTarget.position;
		godTear.transform.DOMove(position, 1f).SetEase(Ease.InBack);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/Stings/global_faith_up", godTear);
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", godTear);
		SkeletonAnimation componentInChildren = godTear.GetComponentInChildren<SkeletonAnimation>();
		componentInChildren.timeScale = 0.75f;
		yield return componentInChildren.YieldForAnimation("godTearScale");
		GameManager.GetInstance().OnConversationNext(_godTearTarget.gameObject, 3f);
		UnityEngine.Object.Destroy(godTear);
		DoWheelReward();
	}

	private void CheckAvailableRewards()
	{
		possibleSkins = new List<string>(DataManager.MysticShopKeeperSkins);
		necklaces = new List<InventoryItem.ITEM_TYPE>(allNecklaces);
		for (int num = necklaces.Count - 1; num >= 0; num--)
		{
			if (Inventory.GetItemQuantity(necklaces[num]) > 0)
			{
				necklaces.Remove(necklaces[num]);
			}
		}
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (necklaces.Contains(follower.Necklace))
			{
				necklaces.Remove(follower.Necklace);
			}
		}
		if (DataManager.Instance.HasBaalSkin && necklaces.Contains(InventoryItem.ITEM_TYPE.Necklace_Light))
		{
			necklaces.Remove(InventoryItem.ITEM_TYPE.Necklace_Light);
		}
		if (DataManager.Instance.HasAymSkin && necklaces.Contains(InventoryItem.ITEM_TYPE.Necklace_Dark))
		{
			necklaces.Remove(InventoryItem.ITEM_TYPE.Necklace_Dark);
		}
		if (necklaces.Count <= 0)
		{
			necklaces = new List<InventoryItem.ITEM_TYPE>(allNecklaces);
			necklaces.Remove(InventoryItem.ITEM_TYPE.Necklace_Light);
			necklaces.Remove(InventoryItem.ITEM_TYPE.Necklace_Dark);
		}
		for (int num2 = possibleSkins.Count - 1; num2 >= 0; num2--)
		{
			if (DataManager.GetFollowerSkinUnlocked(possibleSkins[num2]))
			{
				possibleSkins.RemoveAt(num2);
			}
		}
	}

	private void SetCost()
	{
		int num = 0 + 12 + DataManager.MysticShopKeeperSkins.Length + allNecklaces.Count;
		GodTearCost = 1;
	}

	public void DoWheelReward()
	{
		CheckAvailableRewards();
		WeightedCollection<InventoryItem.ITEM_TYPE> weightedCollection = new WeightedCollection<InventoryItem.ITEM_TYPE>();
		bool num = 20 - DataManager.Instance.CrystalDoctrinesReceivedFromMysticShop > 0;
		bool flag = DataManager.Instance.TalismanPiecesReceivedFromMysticShop < 12;
		bool flag2 = possibleSkins.Count > 0;
		int num2 = 1;
		if (flag)
		{
			num2++;
		}
		if (flag2)
		{
			num2++;
		}
		float num3 = 0.6f / (float)num2;
		if (num)
		{
			weightedCollection.Add(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE, 0.4f);
		}
		else
		{
			num3 += 0.4f / (float)num2;
		}
		if (flag)
		{
			weightedCollection.Add(InventoryItem.ITEM_TYPE.TALISMAN, num3);
		}
		if (flag2)
		{
			weightedCollection.Add(InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN, num3);
		}
		weightedCollection.Add(necklaces[UnityEngine.Random.Range(0, necklaces.Count)], num3);
		if (weightedCollection.Count <= 1)
		{
			weightedCollection.Clear();
			weightedCollection.Add(necklaces[UnityEngine.Random.Range(0, necklaces.Count)], 0.3f);
			weightedCollection.Add(InventoryItem.ITEM_TYPE.LOG_REFINED, 0.2333f);
			weightedCollection.Add(InventoryItem.ITEM_TYPE.MEAT, 0.2333f);
			weightedCollection.Add(InventoryItem.ITEM_TYPE.STONE_REFINED, 0.2333f);
		}
		InventoryItem.ITEM_TYPE iTEM_TYPE = DataManager.Instance.PreviousMysticShopItem;
		if (weightedCollection.Count <= 2)
		{
			iTEM_TYPE = InventoryItem.ITEM_TYPE.NONE;
		}
		List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
		if (iTEM_TYPE != 0)
		{
			list.Add(iTEM_TYPE);
		}
		if (!DataManager.Instance.OnboardedCrystalDoctrine)
		{
			DataManager.Instance.OnboardedCrystalDoctrine = true;
			foreach (InventoryItem.ITEM_TYPE item in weightedCollection)
			{
				list.Add(item);
			}
			list.Remove(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE);
		}
		InventoryItem.ITEM_TYPE chosenReward = InventoryItem.ITEM_TYPE.NONE;
		UIMysticShopOverlayController uIMysticShopOverlayController = MonoSingleton<UIManager>.Instance.MysticShopOverlayTemplate.Instantiate();
		uIMysticShopOverlayController.Show(weightedCollection);
		uIMysticShopOverlayController.OnRewardChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIMysticShopOverlayController.OnRewardChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE reward)
		{
			chosenReward = reward;
		});
		uIMysticShopOverlayController.OnHidden = (Action)Delegate.Combine(uIMysticShopOverlayController.OnHidden, (Action)delegate
		{
			DataManager.Instance.PreviousMysticShopItem = chosenReward;
			StartCoroutine(GiveChosenReward(chosenReward));
			LUTOverlay.gameObject.SetActive(false);
		});
	}

	private IEnumerator GiveChosenReward(InventoryItem.ITEM_TYPE chosenReward)
	{
		if (chosenReward == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
		{
			GameObject necklace = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Resources/ResourceCustomTarget"), null, true);
			necklace.transform.position = _godTearTarget.position;
			necklace.transform.rotation = Quaternion.identity;
			ResourceCustomTarget resourceCustomTarget = necklace.GetComponent<ResourceCustomTarget>();
			resourceCustomTarget.createdObject = necklace;
			resourceCustomTarget.inventoryItemDisplay.SetImage(chosenReward);
			resourceCustomTarget.inventoryItemDisplay.transform.localPosition = Vector2.zero;
			resourceCustomTarget.enabled = false;
			GameManager.GetInstance().OnConversationNext(necklace, 6f);
			AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.gameObject);
			necklace.transform.localScale = Vector3.zero;
			necklace.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
			yield return new WaitForSecondsRealtime(1.25f);
			AudioManager.Instance.PlayOneShot("event:/player/float_follower", necklace.gameObject);
			yield return new WaitForSecondsRealtime(0.25f);
			necklace.transform.DOMove(PlayerFarming.Instance.transform.position + new Vector3(0f, 1f, -1f), 1f).SetEase(Ease.InBack);
			yield return new WaitForSecondsRealtime(1f);
			resourceCustomTarget.CollectMe();
			Inventory.AddItem(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE, 1);
			DataManager.Instance.CrystalDoctrinesReceivedFromMysticShop++;
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_CrystalDoctrine);
			yield return new WaitForSecondsRealtime(0.25f);
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.CrystalDoctrine))
			{
				while (MonoSingleton<UIManager>.Instance.MenusBlocked)
				{
					yield return null;
				}
				UITutorialOverlayController menu = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.CrystalDoctrine);
				yield return menu.YieldUntilHidden();
				UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController = MonoSingleton<UIManager>.Instance.PlayerUpgradesMenuTemplate.Instantiate();
				uIPlayerUpgradesMenuController.ShowCrystalUnlock();
				yield return uIPlayerUpgradesMenuController.YieldUntilHidden();
			}
		}
		else if (necklaces.Contains(chosenReward))
		{
			GameObject necklace = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Resources/ResourceCustomTarget"), null, true);
			necklace.transform.position = _godTearTarget.position;
			necklace.transform.rotation = Quaternion.identity;
			ResourceCustomTarget resourceCustomTarget = necklace.GetComponent<ResourceCustomTarget>();
			resourceCustomTarget.createdObject = necklace;
			resourceCustomTarget.inventoryItemDisplay.SetImage(chosenReward);
			resourceCustomTarget.inventoryItemDisplay.transform.localPosition = Vector2.zero;
			resourceCustomTarget.enabled = false;
			GameManager.GetInstance().OnConversationNext(necklace, 6f);
			AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.gameObject);
			necklace.transform.localScale = Vector3.zero;
			necklace.transform.DOScale(Vector3.one * 1.5f, 2f).SetEase(Ease.OutBack);
			yield return new WaitForSecondsRealtime(1.25f);
			AudioManager.Instance.PlayOneShot("event:/player/float_follower", necklace.gameObject);
			yield return new WaitForSecondsRealtime(0.25f);
			necklace.transform.DOMove(PlayerFarming.Instance.transform.position + new Vector3(0f, 1f, -1f), 1f).SetEase(Ease.InBack);
			yield return new WaitForSecondsRealtime(1f);
			resourceCustomTarget.CollectMe();
			Inventory.AddItem(chosenReward, 1);
			if (!DataManager.Instance.FoundItems.Contains(chosenReward))
			{
				DataManager.Instance.FoundItems.Add(chosenReward);
				UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
				uINewItemOverlayController.Show(UINewItemOverlayController.TypeOfCard.Necklace, base.transform.position, chosenReward);
				yield return uINewItemOverlayController.YieldUntilHidden();
			}
			yield return new WaitForSecondsRealtime(0.25f);
		}
		else
		{
			switch (chosenReward)
			{
			case InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN:
			{
				string skin = possibleSkins[UnityEngine.Random.Range(0, possibleSkins.Count)];
				FollowerSkinCustomTarget followerSkinCustomTarget = FollowerSkinCustomTarget.Create(_godTearTarget.position, PlayerFarming.Instance.transform.position + new Vector3(0f, 1f, -1f), null, 1.25f, skin, null);
				GameManager.GetInstance().OnConversationNext(followerSkinCustomTarget.gameObject, 6f);
				AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.gameObject);
				AudioManager.Instance.PlayOneShot("event:/player/float_follower", followerSkinCustomTarget.gameObject);
				yield return new WaitForSecondsRealtime(1.25f);
				yield return new WaitForSecondsRealtime(1.5f);
				while (UIMenuBase.ActiveMenus.Count > 0)
				{
					yield return null;
				}
				break;
			}
			case InventoryItem.ITEM_TYPE.TALISMAN:
			{
				Interaction_KeyPiece keyPiece = UnityEngine.Object.Instantiate(_keyPiecePrefab, _godTearTarget.position, Quaternion.identity, base.transform.parent);
				keyPiece.transform.localScale = Vector3.zero;
				keyPiece.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
				AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short", _godTearTarget.position);
				GameManager.GetInstance().OnConversationNext(keyPiece.gameObject, 6f);
				yield return new WaitForSeconds(1.5f);
				keyPiece.transform.DOMove(PlayerFarming.Instance.transform.position + new Vector3(0f, 1f, -1f), 1f).SetEase(Ease.InBack);
				AudioManager.Instance.PlayOneShot("event:/player/float_follower", keyPiece.gameObject);
				yield return new WaitForSeconds(1f);
				keyPiece.OnInteract(PlayerFarming.Instance.state);
				DataManager.Instance.TalismanPiecesReceivedFromMysticShop++;
				yield return new WaitForSeconds(2.5f);
				UnityEngine.Object.Destroy(keyPiece.gameObject);
				break;
			}
			default:
			{
				new List<InventoryItem.ITEM_TYPE>();
				int num = UnityEngine.Random.Range(10, 20);
				for (int i = 0; i < num; i++)
				{
					InventoryItem.Spawn(chosenReward, 1, base.transform.position + Vector3.down).SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
				}
				break;
			}
			}
		}
		Interactable = true;
		GameManager.GetInstance().OnConversationEnd();
		base.HasChanged = true;
		DataManager.Instance.MysticRewardCount++;
		SetCost();
		boughtBark.gameObject.SetActive(true);
		boughtBark.Show();
		if (!DataManager.Instance.MysticKeeperFirstPurchase)
		{
			firstPurchaseConvo.Play();
		}
		GameManager.GetInstance().OnConversationEnd(false);
		StopMusic();
		yield return null;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		AudioManager.Instance.StopLoop(loopedSound);
	}

	private IEnumerator AngrySequenceIE()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.25f);
		PlayerFarming.Instance.unitObject.LockToGround = false;
		LightingOverrideAngry.SetActive(true);
		loopedSound = AudioManager.Instance.CreateLoop("event:/atmos/adventure_map/hum_adventure_map", true);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "floating-boss-loop", true);
		AudioManager.Instance.SetMusicPsychedelic(1f);
		PlayerFarming.Instance._state.facingAngle = 90f;
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.transform.DOMove(new Vector3(0f, 42f, -5f), 1.5f).SetEase(Ease.OutCirc);
		yield return new WaitForSeconds(1.5f);
		PlayerFarming.Instance.transform.DOMove(new Vector3(0f, 42f, -1.8f), 1.5f).SetEase(Ease.InSine);
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.StopLoop(loopedSound);
		AudioManager.Instance.SetMusicPsychedelic(0f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "floating-boss-land", false);
		yield return new WaitForSeconds(1.5f);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		PlayerFarming.Instance.unitObject.LockToGround = true;
		enterMysticDimension[DataManager.Instance.PlayerTriedToEnterMysticDimensionCount].Spoken = false;
		enterMysticDimension[DataManager.Instance.PlayerTriedToEnterMysticDimensionCount].Finished = false;
		enterMysticDimension[DataManager.Instance.PlayerTriedToEnterMysticDimensionCount].Play();
		DataManager.Instance.PlayerTriedToEnterMysticDimensionCount++;
		DataManager.Instance.PlayerTriedToEnterMysticDimensionCount = Mathf.Clamp(DataManager.Instance.PlayerTriedToEnterMysticDimensionCount, 0, enterMysticDimension.Length - 1);
		yield return null;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		LightingOverrideAngry.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			StartCoroutine(AngrySequenceIE());
		}
	}

	private void TestFinalReward()
	{
		DataManager.Instance.BeatenLeshyLayer2 = true;
		DataManager.Instance.BeatenHeketLayer2 = true;
		DataManager.Instance.BeatenKallamarLayer2 = true;
		DataManager.Instance.BeatenShamuraLayer2 = true;
		if (StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5))
		{
			beatenAllA.Entries[3].TermToSpeak = "Conversation_NPC/MysticShopSeller/BeatenAll/3_Follower";
		}
		else
		{
			beatenAllA.Entries[3].TermToSpeak = "Conversation_NPC/MysticShopSeller/BeatenAll/3_Shrine";
		}
		PlayMusic();
		beatenAllA.Play();
	}

	public void GiveDeathCatReward()
	{
		StartCoroutine(GiveDeathCatRewardIE());
	}

	private IEnumerator GiveDeathCatRewardIE()
	{
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		bool waiting = true;
		if (!StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5))
		{
			GameManager.GetInstance().OnConversationNew();
			waiting = true;
			DecorationCustomTarget.Create(mysticKeeperSpine.transform.position - Vector3.forward + Vector3.down * 1f, PlayerFarming.Instance.transform.position, 1.5f, StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5, base.transform.parent, delegate
			{
				waiting = false;
			});
			while (waiting)
			{
				yield return null;
			}
		}
		else
		{
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(deathCatTargetPosition);
			simpleCameraTwo.enabled = false;
			FollowerInfo info = FollowerInfo.NewCharacter(FollowerLocation.Base, "Boss Death Cat");
			FollowerManager.SpawnedFollower follower = FollowerManager.SpawnCopyFollower(info, deathCatConvo.transform.position, base.transform.parent, FollowerLocation.Base);
			follower.Follower.SetBodyAnimation("picked-up-hate", true);
			follower.Follower.gameObject.SetActive(false);
			follower.Follower.transform.position = deathCatConvo.transform.position + Vector3.back * 2f + Vector3.up * 5f;
			follower.Follower.transform.localScale = Vector3.zero;
			MeshRenderer renderer = follower.Follower.Spine.GetComponent<MeshRenderer>();
			renderer.material.SetColor("_FillColor", Color.white);
			renderer.material.SetFloat("_FillAlpha", 1f);
			yield return new WaitForSeconds(2f);
			CameraManager.instance.ShakeCameraForDuration(1.2f, 1.3f, 0.25f);
			LightingOverrideAngry.SetActive(true);
			loopedSoundDeathCat = AudioManager.Instance.CreateLoop("event:/door/eye_beam_door_open", PlayerFarming.Instance.gameObject, true);
			MMVibrate.RumbleContinuous(0.1f, 0.2f);
			DeviceLightingManager.FlashColor(Color.red);
			yield return new WaitForSeconds(0.25f);
			CameraManager.instance.ShakeCameraForDuration(0.2f, 0.3f, 3f);
			yield return new WaitForSeconds(2f);
			follower.Follower.gameObject.SetActive(true);
			float t = 0f;
			DOTween.To(() => t, delegate(float x)
			{
				t = x;
			}, 1f, 2f).OnUpdate(delegate
			{
				renderer.material.SetFloat("_FillAlpha", 1f - t);
			});
			follower.Follower.transform.DOScale(1f, 1.25f);
			follower.Follower.transform.DOMove(deathCatTargetPosition.transform.position, 2f);
			yield return new WaitForSeconds(1f);
			yield return new WaitForSeconds(2f);
			foreach (ConversationEntry entry in deathCatConvo.Entries)
			{
				entry.Speaker = follower.Follower.Spine.gameObject;
				entry.SkeletonData = follower.Follower.Spine;
				entry.soundPath = "event:/dialogue/followers/boss/fol_deathcat";
				entry.pitchValue = info.follower_pitch;
				entry.vibratoValue = info.follower_vibrato;
			}
			yield return new WaitForEndOfFrame();
			LightingOverrideAngry.SetActive(false);
			AudioManager.Instance.StopLoop(loopedSoundDeathCat);
			MMVibrate.StopRumble();
			deathCatConvo.Play();
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().CamFollowTarget.TargetOffset = new Vector3(0f, 0f, 0.3f);
			while (LetterBox.IsPlaying)
			{
				yield return null;
			}
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(follower.Follower.gameObject, 6f);
			Vector3 targetPosition = follower.Follower.transform.position + Vector3.left * 1.5f;
			PlayerFarming.Instance.GoToAndStop(targetPosition, follower.Follower.gameObject);
			while (PlayerFarming.Instance.GoToAndStopping)
			{
				yield return null;
			}
			follower.Follower.Spine.GetComponent<SimpleSpineAnimator>().enabled = false;
			if (state == null)
			{
				state = GetComponent<StateMachine>();
			}
			GameManager.GetInstance().OnConversationNext(follower.Follower.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/ascend", base.gameObject);
			yield return new WaitForEndOfFrame();
			LightingOverrideAngry.SetActive(false);
			GameManager.GetInstance().OnConversationNext(follower.Follower.gameObject);
			follower.Follower.transform.DOMove(follower.Follower.transform.position + Vector3.forward, 2f);
			follower.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			follower.Follower.SetBodyAnimation("convert-short", false);
			if ((bool)recruitParticles)
			{
				recruitParticles.Play();
			}
			portalSpine.gameObject.SetActive(true);
			portalSpine.AnimationState.SetAnimation(0, "convert-short", false);
			float duration = PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "specials/special-activate-long", false).Animation.Duration;
			PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
			yield return new WaitForSeconds(duration - 1f);
			float value = UnityEngine.Random.value;
			Thought thought = Thought.None;
			if (value < 0.7f)
			{
				value = UnityEngine.Random.value;
				if (value <= 0.3f)
				{
					thought = Thought.HappyConvert;
				}
				else if (value > 0.3f && value < 0.6f)
				{
					thought = Thought.GratefulConvert;
				}
				else if (value >= 0.6f)
				{
					thought = Thought.SkepticalConvert;
				}
			}
			else
			{
				value = UnityEngine.Random.value;
				thought = ((!(value <= 0.3f) || DataManager.Instance.Followers.Count <= 0) ? Thought.InstantBelieverConvert : Thought.ResentfulConvert);
			}
			ThoughtData data = FollowerThoughts.GetData(thought);
			data.Init();
			info.Thoughts.Add(data);
			FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base, "Boss Death Cat");
			followerInfo.ID = FollowerManager.DeathCatID;
			DataManager.Instance.Followers_Recruit.Add(followerInfo);
			if (BiomeBaseManager.Instance.SpawnExistingRecruits)
			{
				BiomeBaseManager.Instance.SpawnExistingRecruits = false;
			}
			FollowerManager.CleanUpCopyFollower(follower);
		}
		beatenAllC.Play();
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.MysticShopReturn);
		yield return null;
		while (waitingForContinueSequence)
		{
			yield return null;
		}
		DataManager.Instance.UnlockedFleeces.Add(1000);
		UIPlayerUpgradesMenuController uIPlayerUpgradesMenuController = MonoSingleton<UIManager>.Instance.PlayerUpgradesMenuTemplate.Instantiate();
		uIPlayerUpgradesMenuController.ShowNewFleecesUnlocked(new PlayerFleeceManager.FleeceType[1] { PlayerFleeceManager.FleeceType.GodOfDeath }, true);
		yield return uIPlayerUpgradesMenuController.YieldUntilHidden();
		simpleCameraTwo.enabled = false;
		AudioManager.Instance.PlayOneShot("event:/Stings/finish_game_second_time", PlayerFarming.Instance.gameObject);
		AudioManager.Instance.PlayOneShot("event:/ui/heretics_defeated", PlayerFarming.Instance.gameObject);
		float tt = 0f;
		DOTween.To(() => tt, delegate(float x)
		{
			tt = x;
		}, 1f, 10f).OnUpdate(delegate
		{
			GameManager.GetInstance().CameraSetZoom(Mathf.Lerp(14f, 20f, tt));
		});
		yield return new WaitForSeconds(7f);
		DataManager.Instance.BeatenPostGame = true;
		SaveAndLoad.Save();
		StopMusic();
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Credits", 2f, "", null);
		Credits.GoToMainMenu = true;
	}

	public void ContinueSequence()
	{
		waitingForContinueSequence = false;
	}

	public void CheckConversation()
	{
		Debug.Log("CheckConversation()".Colour(Color.yellow));
		if (!DataManager.Instance.MysticKeeperBeatenAll && DataManager.Instance.BeatenLeshyLayer2 && DataManager.Instance.BeatenHeketLayer2 && DataManager.Instance.BeatenKallamarLayer2 && DataManager.Instance.BeatenShamuraLayer2)
		{
			StartCoroutine(WaitForConvoToFinish());
		}
		else
		{
			StopMusic();
		}
	}

	private IEnumerator WaitForConvoToFinish()
	{
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		PlayMusic();
		beatenAllA.Play();
	}

	public void PlayMusic()
	{
		Debug.Log("Play music!".Colour(Color.green));
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.Temple);
	}

	public void StopMusic()
	{
		Debug.Log("Stop music!".Colour(Color.yellow));
		AudioManager.Instance.SetMusicBaseID(SoundConstants.BaseID.DungeonDoor);
	}
}
