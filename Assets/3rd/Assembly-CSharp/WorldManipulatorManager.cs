using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Map;
using MMBiomeGeneration;
using MMTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class WorldManipulatorManager
{
	public enum Manipulations
	{
		GainRandomHeart = 0,
		HealHearts = 1,
		GainTarot = 2,
		DealDamageToAllEnemies = 3,
		ReceiveDemon = 4,
		InvincibleForTime = 5,
		DropRandomWeaponCurse = 6,
		ChargeCurrentRelic = 7,
		SpecialAttacksDamageIncrease = 8,
		DropRandomRelic = 9,
		NextChestGold = 10,
		TakeDamage = 100,
		IncreaseEnemyModifiersChance = 101,
		SpawnBombs = 102,
		LoseAllSpecialHearts = 103,
		DropPoisonOnAttack = 104,
		AllEnemiesHaveModifiersInNextRoom = 105,
		LoseRelic = 106,
		NoSpecialAttacks = 107,
		AllEnemiesHealed = 108,
		IncreasedBossesHealth = 109,
		CombatNodes = 110,
		ResetTempleCooldowns = 200,
		InstantlyBuildStructures = 201,
		GainFaith = 202,
		ResurrectBuriedFollower = 203,
		CureCursedFollowers = 204,
		ClearAllWaste = 205,
		FollowerInstantlyLevelled = 206,
		CropsInstantlyFertilised = 207,
		MissionarySuccessful = 208,
		InstantRefinedMaterials = 209,
		ResetEndlessModeCooldown = 210,
		AllFollowersPoopOrVomit = 300,
		BreakAllBeds = 301,
		SkipTime = 302,
		SleepFollowers = 303,
		RandomCursedState = 304,
		KillRandomFollower = 305,
		ToiletsInstantlyFull = 306,
		BodiesOutOfGraves = 307,
		MealsVanish = 308,
		FollowerLosesLevel = 309,
		QuestForMurder = 310
	}

	public static void TriggerManipulation(Manipulations manipulation, float delay = 0f, bool twitch = false)
	{
		GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
		{
			string text = "";
			switch (manipulation)
			{
			case Manipulations.ResetTempleCooldowns:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					UpgradeSystem.ClearAllCoolDowns();
				}));
				break;
			case Manipulations.InstantlyBuildStructures:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					StructureManager.BuildAllStructures();
				}));
				break;
			case Manipulations.GainFaith:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					CultFaithManager.AddThought(Thought.FaithIncreased, -1, 1f);
				}));
				break;
			case Manipulations.CureCursedFollowers:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					FollowerManager.CureAllCursedFollowers();
				}));
				break;
			case Manipulations.ResurrectBuriedFollower:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					FollowerManager.ResurrectBurriedFollower();
				}));
				break;
			case Manipulations.ClearAllWaste:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					StructureManager.ClearAllWaste();
				}));
				break;
			case Manipulations.RandomCursedState:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					int min = Mathf.Clamp(Mathf.RoundToInt((float)DataManager.Instance.Followers.Count / 6f), 1, DataManager.Instance.Followers.Count);
					int num5 = Mathf.Clamp(Mathf.RoundToInt((float)DataManager.Instance.Followers.Count / 4f), 1, DataManager.Instance.Followers.Count);
					FollowerManager.GiveFollowersRandomCurse(UnityEngine.Random.Range(min, num5 + 1));
				}));
				break;
			case Manipulations.BreakAllBeds:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					StructureManager.BreakRandomBeds();
				}));
				break;
			case Manipulations.SleepFollowers:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					FollowerManager.MakeAllFollowersFallAsleep();
				}));
				break;
			case Manipulations.KillRandomFollower:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					FollowerManager.KillRandomFollower(true);
				}));
				break;
			case Manipulations.AllFollowersPoopOrVomit:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					FollowerManager.MakeAllFollowersPoopOrVomit();
				}));
				break;
			case Manipulations.SkipTime:
				GameManager.GetInstance().StartCoroutine(WaitTillPlayerIsAtBase(delegate
				{
					TimeManager.SkipTime(600f);
				}));
				break;
			case Manipulations.GainRandomHeart:
				switch (HealthPlayer.GainRandomHeart())
				{
				case 0:
					text = "Inventory/BLACK_HEART";
					break;
				case 1:
					text = "Inventory/BLUE_HEART";
					break;
				case 2:
					text = "Inventory/RED_HEART";
					break;
				}
				break;
			case Manipulations.HealHearts:
			{
				int num4 = UnityEngine.Random.Range((int)(PlayerFarming.Instance.health.totalHP / 2f), (int)PlayerFarming.Instance.health.totalHP + 1);
				PlayerFarming.Instance.GetComponent<HealthPlayer>().Heal(num4);
				text = num4.ToString();
				break;
			}
			case Manipulations.DealDamageToAllEnemies:
				Health.DamageAllEnemies(5f + DataManager.GetWeaponDamageMultiplier(DataManager.Instance.CurrentWeaponLevel) * 2f, Health.DamageAllEnemiesType.Manipulation);
				break;
			case Manipulations.GainTarot:
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.TRINKET_CARD, 1, PlayerFarming.Instance.transform.position + Vector3.down, 0f).GetComponent<Interaction_TarotCard>().ForceAllow = true;
				break;
			case Manipulations.ReceiveDemon:
			{
				int num2 = 0;
				while (++num2 < 30)
				{
					int num3 = UnityEngine.Random.Range(0, 5);
					if (!DataManager.Instance.Followers_Demons_Types.Contains(num3))
					{
						BiomeGenerator.Instance.SpawnDemon(num3, -1, -1, true);
						break;
					}
				}
				break;
			}
			case Manipulations.InvincibleForTime:
			{
				float duration = 30f;
				PlayerFarming.Instance.playerController.MakeUntouchable(duration);
				text = duration.ToString();
				break;
			}
			case Manipulations.TakeDamage:
			{
				int num = Mathf.Max(1, UnityEngine.Random.Range(0, (int)(PlayerFarming.Instance.health.HP / 2f)));
				PlayerFarming.Instance.GetComponent<HealthPlayer>().DealDamage(num, PlayerFarming.Instance.gameObject, PlayerFarming.Instance.transform.position, false, Health.AttackTypes.Melee, true);
				text = num.ToString();
				break;
			}
			case Manipulations.IncreaseEnemyModifiersChance:
				DataManager.Instance.EnemyModifiersChanceMultiplier += 3f;
				break;
			case Manipulations.LoseAllSpecialHearts:
				HealthPlayer.LoseAllSpecialHearts();
				break;
			case Manipulations.SpawnBombs:
				BiomeGenerator.SpawnBombsInRoom(UnityEngine.Random.Range(15, 25));
				break;
			case Manipulations.DropPoisonOnAttack:
				DataManager.Instance.SpawnPoisonOnAttack = true;
				break;
			case Manipulations.AllEnemiesHaveModifiersInNextRoom:
				DataManager.Instance.EnemiesInNextRoomHaveModifiers = true;
				DataManager.Instance.CurrentRoomCoordinates = new Vector2(BiomeGenerator.Instance.CurrentRoom.x, BiomeGenerator.Instance.CurrentRoom.y);
				RoomLockController.OnRoomCleared += OnRoomCleared;
				break;
			case Manipulations.BodiesOutOfGraves:
				BaseLocationManager.Instance.PopOutDeadBodiesFromGraves(UnityEngine.Random.Range(5, 10));
				break;
			case Manipulations.ToiletsInstantlyFull:
				BaseLocationManager.Instance.InstantlyFillAllToilets();
				break;
			case Manipulations.MealsVanish:
				BaseLocationManager.Instance.InstantlyClearKitchenQueues();
				break;
			case Manipulations.FollowerLosesLevel:
			{
				FollowerBrain followerBrain = FollowerManager.MakeFollowerLoseLevel();
				if (followerBrain == null)
				{
					return;
				}
				text = "<color=#FFD201>" + followerBrain.Info.Name + "</color>";
				break;
			}
			case Manipulations.QuestForMurder:
			{
				FollowerBrain followerBrain = FollowerManager.GetRandomNonLockedFollower();
				if (followerBrain == null)
				{
					return;
				}
				text = "<color=#FFD201>" + followerBrain.Info.Name + "</color>";
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/TwitchQuest", Objectives.CustomQuestTypes.MurderFollower, followerBrain.Info.ID, 4800f), true);
				break;
			}
			case Manipulations.FollowerInstantlyLevelled:
			{
				FollowerBrain followerBrain = FollowerManager.MakeFollowerGainLevel();
				if (followerBrain == null)
				{
					return;
				}
				text = "<color=#FFD201>" + followerBrain.Info.Name + "</color>";
				break;
			}
			case Manipulations.CropsInstantlyFertilised:
				BaseLocationManager.Instance.InstantlyFertilizeAllCrops();
				break;
			case Manipulations.MissionarySuccessful:
				DataManager.Instance.NextMissionarySuccessful = true;
				break;
			case Manipulations.InstantRefinedMaterials:
				BaseLocationManager.Instance.InstantlyRefineMaterials();
				break;
			case Manipulations.ResetEndlessModeCooldown:
				DataManager.Instance.EndlessModeOnCooldown = false;
				break;
			case Manipulations.DropRandomWeaponCurse:
			case Manipulations.DropRandomRelic:
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Weapon Choice.prefab", PlayerFarming.Instance.transform.position, Quaternion.identity, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
				{
					Interaction_WeaponSelectionPodium component2 = obj.Result.GetComponent<Interaction_WeaponSelectionPodium>();
					if (manipulation == Manipulations.DropRandomRelic)
					{
						component2.Type = Interaction_WeaponSelectionPodium.Types.Relic;
					}
					component2.Reveal();
				};
				break;
			}
			case Manipulations.ChargeCurrentRelic:
				PlayerFarming.Instance.playerRelic.FullyCharge();
				break;
			case Manipulations.SpecialAttacksDamageIncrease:
				DataManager.Instance.SpecialAttackDamageMultiplier += 0.5f;
				break;
			case Manipulations.NextChestGold:
				DataManager.Instance.NextChestGold = true;
				break;
			case Manipulations.LoseRelic:
				PlayerFarming.Instance.playerRelic.RemoveRelic();
				break;
			case Manipulations.NoSpecialAttacks:
				DataManager.Instance.SpecialAttacksDisabled = true;
				break;
			case Manipulations.AllEnemiesHealed:
				foreach (Health item in Health.team2)
				{
					if (item != null)
					{
						item.HP = item.totalHP;
						ShowHPBar component = item.GetComponent<ShowHPBar>();
						if ((object)component != null)
						{
							component.OnHit(item.gameObject, Vector3.zero, Health.AttackTypes.Melee, false);
						}
						AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", item.gameObject.transform.position);
						BiomeConstants.Instance.EmitHeartPickUpVFX(item.transform.position - Vector3.forward, 0f, "red", "burst_big");
					}
				}
				break;
			case Manipulations.IncreasedBossesHealth:
				DataManager.Instance.BossHealthMultiplier += 0.5f;
				break;
			case Manipulations.CombatNodes:
				GameManager.GetInstance().StartCoroutine(ConvertAllMapNodesIE());
				break;
			}
			if (twitch)
			{
				if (string.IsNullOrEmpty(text))
				{
					NotificationCentre.Instance.PlayTwitchNotification(GetNotification(manipulation), "UI/Twitch/ThanksTwitchChat");
				}
				else
				{
					NotificationCentre.Instance.PlayTwitchNotification(GetNotification(manipulation), text, "UI/Twitch/ThanksTwitchChat");
				}
			}
		}));
	}

	private static IEnumerator WaitTillPlayerIsAtBase(Action callback)
	{
		while (PlayerFarming.Location != FollowerLocation.Base && !GameManager.IsDungeon(PlayerFarming.Location))
		{
			yield return null;
		}
		if (callback != null)
		{
			callback();
		}
	}

	private static void OnRoomCleared()
	{
		if (DataManager.Instance.CurrentRoomCoordinates != new Vector2(BiomeGenerator.Instance.CurrentRoom.x, BiomeGenerator.Instance.CurrentRoom.y))
		{
			DataManager.Instance.EnemiesInNextRoomHaveModifiers = false;
			DataManager.Instance.CurrentRoomCoordinates = Vector2.zero;
			RoomLockController.OnRoomCleared -= OnRoomCleared;
		}
	}

	public static string GetLocalisation(Manipulations manipulation)
	{
		return LocalizationManager.GetTranslation(string.Format("Manipulations/{0}", manipulation));
	}

	public static string GetNotification(Manipulations manipulation)
	{
		return string.Format("Manipulations/{0}/Notification", manipulation);
	}

	public static List<Manipulations> GetPossibleDungeonPositiveManipulations()
	{
		List<Manipulations> list = new List<Manipulations>();
		list.Add(Manipulations.GainRandomHeart);
		list.Add(Manipulations.DealDamageToAllEnemies);
		list.Add(Manipulations.GainTarot);
		list.Add(Manipulations.ReceiveDemon);
		list.Add(Manipulations.InvincibleForTime);
		list.Add(Manipulations.DropRandomWeaponCurse);
		list.Add(Manipulations.NextChestGold);
		if (PlayerFarming.Instance.health.HP < PlayerFarming.Instance.health.totalHP)
		{
			list.Add(Manipulations.HealHearts);
		}
		if (PlayerFarming.Instance.playerRelic.CurrentRelic != null)
		{
			list.Add(Manipulations.DropRandomRelic);
			if (PlayerFarming.Instance.playerRelic.ChargedAmount / PlayerFarming.Instance.playerRelic.RequiredChargeAmount < 0.1f)
			{
				list.Add(Manipulations.ChargeCurrentRelic);
			}
		}
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks))
		{
			list.Add(Manipulations.SpecialAttacksDamageIncrease);
		}
		list.Shuffle();
		return list;
	}

	public static List<Manipulations> GetPossibleDungeonNegativeManipulations()
	{
		List<Manipulations> list = new List<Manipulations>();
		list.Add(Manipulations.TakeDamage);
		list.Add(Manipulations.IncreaseEnemyModifiersChance);
		list.Add(Manipulations.SpawnBombs);
		list.Add(Manipulations.DropPoisonOnAttack);
		list.Add(Manipulations.AllEnemiesHaveModifiersInNextRoom);
		list.Add(Manipulations.AllEnemiesHealed);
		list.Add(Manipulations.IncreasedBossesHealth);
		if (PlayerFarming.Instance.health.BlackHearts > 0f || PlayerFarming.Instance.health.TotalSpiritHearts > 0f || PlayerFarming.Instance.health.BlueHearts > 0f)
		{
			list.Add(Manipulations.LoseAllSpecialHearts);
		}
		if (PlayerFarming.Instance.playerRelic.CurrentRelic != null)
		{
			list.Add(Manipulations.LoseRelic);
		}
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks))
		{
			list.Add(Manipulations.NoSpecialAttacks);
		}
		if (MapManager.Instance != null && (MapManager.Instance.CurrentLayer <= 2 || MapGenerator.Nodes.Count == 0))
		{
			list.Add(Manipulations.CombatNodes);
		}
		list.Shuffle();
		return list;
	}

	public static List<Manipulations> GetPossibleBasePositiveManipulations()
	{
		List<Manipulations> list = new List<Manipulations>();
		list.Add(Manipulations.GainFaith);
		bool flag = false;
		for (int num = DataManager.Instance.UpgradeCoolDowns.Count - 1; num >= 0; num--)
		{
			if (!UpgradeSystem.IsRitualActive(DataManager.Instance.UpgradeCoolDowns[num].Type))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			list.Add(Manipulations.ResetTempleCooldowns);
		}
		List<StructureBrain> list2 = new List<StructureBrain>(StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.BUILD_SITE));
		list2.AddRange(StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.BUILDSITE_BUILDINGPROJECT));
		if (list2.Count > 0)
		{
			list.Add(Manipulations.InstantlyBuildStructures);
		}
		List<Structures_Grave> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Grave>(FollowerLocation.Base);
		List<FollowerInfo> list3 = new List<FollowerInfo>(DataManager.Instance.Followers_Dead);
		bool flag2 = false;
		for (int num2 = list3.Count - 1; num2 >= 0; num2--)
		{
			foreach (Structures_Grave item in allStructuresOfType)
			{
				if (item.Data.FollowerID == list3[num2].ID)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				break;
			}
		}
		if (flag2)
		{
			list.Add(Manipulations.ResurrectBuriedFollower);
		}
		bool flag3 = false;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Info.CursedState != 0 && allBrain.Info.CursedState != Thought.OldAge && !DataManager.Instance.Followers_Recruit.Contains(allBrain._directInfoAccess))
			{
				flag3 = true;
				break;
			}
		}
		if (flag3)
		{
			list.Add(Manipulations.CureCursedFollowers);
		}
		List<StructureBrain> allStructuresOfType2 = StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.VOMIT);
		List<StructureBrain> allStructuresOfType3 = StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.POOP);
		if (allStructuresOfType2.Count > 0 || allStructuresOfType3.Count > 0)
		{
			list.Add(Manipulations.ClearAllWaste);
		}
		List<FollowerBrain> list4 = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num3 = list4.Count - 1; num3 >= 0; num3--)
		{
			if (FollowerManager.FollowerLocked(list4[num3].Info.ID) || list4[num3].Stats.Adoration >= list4[num3].Stats.MAX_ADORATION)
			{
				list4.RemoveAt(num3);
			}
		}
		if (list4.Count > 0)
		{
			list.Add(Manipulations.FollowerInstantlyLevelled);
		}
		List<Structures_FarmerPlot> list5 = new List<Structures_FarmerPlot>(StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>());
		for (int num4 = list5.Count - 1; num4 >= 0; num4--)
		{
			if (!list5[num4].HasPlantedSeed() || list5[num4].HasFertilized())
			{
				list5.RemoveAt(num4);
			}
		}
		if (list5.Count > 0)
		{
			list.Add(Manipulations.CropsInstantlyFertilised);
		}
		if (StructureManager.GetAllStructuresOfType<Structures_Missionary>().Count > 0 && !DataManager.Instance.NextMissionarySuccessful)
		{
			list.Add(Manipulations.MissionarySuccessful);
		}
		List<Structures_Refinery> list6 = new List<Structures_Refinery>(StructureManager.GetAllStructuresOfType<Structures_Refinery>());
		for (int num5 = list6.Count - 1; num5 >= 0; num5--)
		{
			if (list6[num5].Data.QueuedResources.Count <= 0)
			{
				list6.RemoveAt(num5);
			}
		}
		if (list6.Count > 0)
		{
			list.Add(Manipulations.InstantRefinedMaterials);
		}
		if (DataManager.Instance.EndlessModeOnCooldown && TimeManager.CurrentPhase <= DayPhase.Morning)
		{
			list.Add(Manipulations.ResetEndlessModeCooldown);
		}
		list.Shuffle();
		return list;
	}

	public static List<Manipulations> GetPossibleBaseNegativeManipulations()
	{
		List<Manipulations> list = new List<Manipulations>();
		if (TimeManager.CurrentPhase == DayPhase.Morning || TimeManager.CurrentPhase == DayPhase.Dawn)
		{
			list.Add(Manipulations.SkipTime);
		}
		bool flag = false;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			list.Add(Manipulations.AllFollowersPoopOrVomit);
			list.Add(Manipulations.KillRandomFollower);
			if (TimeManager.CurrentPhase != DayPhase.Night || TimeManager.CurrentPhase != DayPhase.Dusk)
			{
				list.Add(Manipulations.SleepFollowers);
			}
		}
		if (FollowerBrain.RandomAvailableBrainNoCurseState() != null)
		{
			list.Add(Manipulations.RandomCursedState);
		}
		List<Structures_Bed> list2 = new List<Structures_Bed>(StructureManager.GetAllStructuresOfType<Structures_Bed>());
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			if (list2[num].IsCollapsed)
			{
				list2.Remove(list2[num]);
			}
		}
		if (list2.Count > 0)
		{
			list.Add(Manipulations.BreakAllBeds);
		}
		List<Structures_Outhouse> list3 = new List<Structures_Outhouse>(StructureManager.GetAllStructuresOfType<Structures_Outhouse>());
		for (int num2 = list3.Count - 1; num2 >= 0; num2--)
		{
			if (list3[num2].IsFull)
			{
				list3.RemoveAt(num2);
			}
		}
		if (list3.Count > 0)
		{
			list.Add(Manipulations.ToiletsInstantlyFull);
		}
		List<Structures_Grave> list4 = new List<Structures_Grave>(StructureManager.GetAllStructuresOfType<Structures_Grave>());
		for (int num3 = list4.Count - 1; num3 >= 0; num3--)
		{
			if (list4[num3].Data.FollowerID == -1)
			{
				list4.RemoveAt(num3);
			}
		}
		if (list4.Count > 0)
		{
			list.Add(Manipulations.BodiesOutOfGraves);
		}
		List<Structures_Kitchen> list5 = new List<Structures_Kitchen>(StructureManager.GetAllStructuresOfType<Structures_Kitchen>());
		for (int num4 = list5.Count - 1; num4 >= 0; num4--)
		{
			if (list5[num4].Data.QueuedMeals.Count <= 0 && list5[num4].Data.QueuedResources.Count <= 0)
			{
				list5.RemoveAt(num4);
			}
		}
		if (list5.Count > 0)
		{
			list.Add(Manipulations.MealsVanish);
		}
		List<FollowerBrain> list6 = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num5 = list6.Count - 1; num5 >= 0; num5--)
		{
			if (FollowerManager.FollowerLocked(list6[num5].Info.ID))
			{
				list6.RemoveAt(num5);
			}
		}
		if (list6.Count > 0)
		{
			list.Add(Manipulations.FollowerLosesLevel);
		}
		List<FollowerBrain> list7 = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num6 = list7.Count - 1; num6 >= 0; num6--)
		{
			if (FollowerManager.FollowerLocked(list7[num6].Info.ID))
			{
				list7.RemoveAt(num6);
			}
		}
		if (list7.Count > 0)
		{
			list.Add(Manipulations.QuestForMurder);
		}
		list.Shuffle();
		return list;
	}

	private static IEnumerator ConvertAllMapNodesIE()
	{
		while (Health.team2.Count > 0 || LetterBox.IsPlaying || MMConversation.isPlaying)
		{
			yield return null;
		}
		while (PlayerFarming.Instance.state.CURRENT_STATE != 0 && PlayerFarming.Instance.state.CURRENT_STATE != StateMachine.State.Moving)
		{
			yield return null;
		}
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.ConvertAllNodesToCombatNodes();
		MapManager.Instance.CloseMap();
		while (adventureMapOverlayController.IsHiding)
		{
			yield return null;
		}
	}
}
