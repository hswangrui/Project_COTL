using System;
using System.Collections.Generic;
using I2.Loc;
using Unify;
using UnityEngine;

[Serializable]
public class TarotCards
{
	public enum Card
	{
		Hearts1,
		Hearts2,
		Hearts3,
		Lovers1,
		Lovers2,
		Sun,
		Moon,
		GiftFromBelow,
		Spider,
		DiseasedHeart,
		EyeOfWeakness,
		Arrows,
		DeathsDoor,
		TheDeal,
		Telescope,
		HandsOfRage,
		NaturesGift,
		Skull,
		Potion,
		Sword,
		Dagger,
		Axe,
		Blunderbuss,
		Hammer,
		Fireball,
		Tripleshot,
		Tentacles,
		EnemyBlast,
		ProjectileAOE,
		Vortex,
		MegaSlash,
		MovementSpeed,
		AttackRate,
		IncreasedDamage,
		IncreaseBlackSoulsDrop,
		HealChance,
		NegateDamageChance,
		BombOnRoll,
		GoopOnDamaged,
		GoopOnRoll,
		PoisonImmune,
		DamageOnRoll,
		HealTwiceAmount,
		InvincibleWhileHealing,
		AmmoEfficient,
		BlackSoulAutoRecharge,
		BlackSoulOnDamage,
		NeptunesCurse,
		RabbitFoot,
		Gauntlet,
		HoldToHeal,
		MoreRelics,
		TentacleOnDamaged,
		InvincibilityPerRoom,
		BombOnDamaged,
		ImmuneToTraps,
		ImmuneToProjectiles,
		WalkThroughBlocks,
		DecreaseRelicCharge,
		Count
	}

	public class TarotCard
	{
		public Card CardType;

		public int UpgradeIndex;

		public TarotCard()
		{
		}

		public TarotCard(Card type, int upgrade)
		{
			CardType = type;
			UpgradeIndex = upgrade;
		}
	}

	public enum CardCategory
	{
		Weapon,
		Curse,
		Trinket
	}

	public Card Type;

	public bool Unlocked;

	public float UnlockProgress;

	public bool Used;

	public static Card[] DefaultCards = new Card[15]
	{
		Card.Hearts1,
		Card.Lovers1,
		Card.EyeOfWeakness,
		Card.Telescope,
		Card.DiseasedHeart,
		Card.Spider,
		Card.AttackRate,
		Card.IncreasedDamage,
		Card.IncreaseBlackSoulsDrop,
		Card.NegateDamageChance,
		Card.AmmoEfficient,
		Card.HealTwiceAmount,
		Card.DeathsDoor,
		Card.GiftFromBelow,
		Card.RabbitFoot
	};

	public CardCategory cardCategory;

	public static int TarotCardsUnlockedCount()
	{
		return DataManager.Instance.PlayerFoundTrinkets.Count;
	}

	public static TarotCards Create(Card Type, bool Unlocked)
	{
		TarotCards tarotCards = new TarotCards();
		tarotCards.Type = Type;
		tarotCards.Unlocked = Unlocked;
		if (Unlocked)
		{
			tarotCards.UnlockProgress = 1f;
		}
		return tarotCards;
	}

	public CardCategory GetCardCategory(Card Type)
	{
		switch (Type)
		{
		case Card.Sword:
		case Card.Dagger:
		case Card.Axe:
		case Card.Blunderbuss:
			return CardCategory.Weapon;
		case Card.Fireball:
		case Card.Tripleshot:
			return CardCategory.Curse;
		default:
			return CardCategory.Trinket;
		}
	}

	public static string LocalisedName(Card type)
	{
		int upgradeIndex = 0;
		foreach (TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			if (playerRunTrinket.CardType == type)
			{
				upgradeIndex = playerRunTrinket.UpgradeIndex;
				break;
			}
		}
		return LocalisedName(type, upgradeIndex);
	}

	public static string LocalisedName(Card Card, int upgradeIndex)
	{
		string text = "";
		for (int i = 0; i < upgradeIndex; i++)
		{
			text += "+";
		}
		string text2 = "";
		switch (upgradeIndex)
		{
		case 1:
			text2 = "<color=green>";
			break;
		case 2:
			text2 = "<color=purple>";
			break;
		}
		return text2 + LocalizationManager.GetTranslation(string.Format("TarotCards/{0}/Name", Card)) + text + "</color>";
	}

	public static string LocalisedDescription(Card Type)
	{
		int upgradeIndex = 0;
		foreach (TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			if (playerRunTrinket.CardType == Type)
			{
				upgradeIndex = playerRunTrinket.UpgradeIndex;
				break;
			}
		}
		return LocalisedDescription(Type, upgradeIndex);
	}

	public static string LocalisedDescription(Card Type, int upgradeIndex)
	{
		string text = string.Format("TarotCards/{0}/Description", Type);
		if (upgradeIndex > 0)
		{
			text += upgradeIndex;
		}
		return LocalizationManager.GetTranslation(text);
	}

	public static string LocalisedLore(Card Type)
	{
		return LocalizationManager.GetTranslation(string.Format("TarotCards/{0}/Lore", Type));
	}

	public static string Skin(Card Type)
	{
		switch (Type)
		{
		case Card.Hearts1:
			return "Trinkets/TheHearts1";
		case Card.Hearts2:
			return "Trinkets/TheHearts2";
		case Card.Hearts3:
			return "Trinkets/TheHearts3";
		case Card.Lovers1:
			return "Trinkets/TheLovers1";
		case Card.Lovers2:
			return "Trinkets/TheLovers2";
		case Card.Sun:
			return "Trinkets/Sun";
		case Card.Moon:
			return "Trinkets/Moon";
		case Card.GiftFromBelow:
			return "Trinkets/GiftFromBelow";
		case Card.Spider:
			return "Trinkets/Spider";
		case Card.DiseasedHeart:
			return "Trinkets/DiseasedHeart";
		case Card.EyeOfWeakness:
			return "Trinkets/EyeOfWeakness";
		case Card.Arrows:
			return "Trinkets/Arrows";
		case Card.DeathsDoor:
			return "Trinkets/DeathsDoor";
		case Card.TheDeal:
			return "Trinkets/TheDeal";
		case Card.Telescope:
			return "Trinkets/Telescope";
		case Card.HandsOfRage:
			return "Trinkets/HandsOfRage";
		case Card.NaturesGift:
			return "Trinkets/NaturesGift";
		case Card.Skull:
			return "Trinkets/Skull";
		case Card.Potion:
			return "Trinkets/Potion";
		case Card.MovementSpeed:
			return "Trinkets/MovementSpeed";
		case Card.AttackRate:
			return "Trinkets/AttackRate";
		case Card.IncreasedDamage:
			return "Trinkets/IncreasedDamage";
		case Card.IncreaseBlackSoulsDrop:
			return "Trinkets/IncreasedXP";
		case Card.HealChance:
			return "Trinkets/HealChance";
		case Card.NegateDamageChance:
			return "Trinkets/NegateDamageChance";
		case Card.BombOnRoll:
			return "Trinkets/BombOnRoll";
		case Card.GoopOnDamaged:
			return "Trinkets/GoopOnDamaged";
		case Card.GoopOnRoll:
			return "Trinkets/GoopOnRoll";
		case Card.PoisonImmune:
			return "Trinkets/PoisonImmune";
		case Card.DamageOnRoll:
			return "Trinkets/DamageOnRoll";
		case Card.HealTwiceAmount:
			return "Trinkets/HealTwiceAmount";
		case Card.InvincibleWhileHealing:
			return "Trinkets/InvincibleWhileHealing";
		case Card.AmmoEfficient:
			return "Trinkets/AmmoEfficient";
		case Card.BlackSoulAutoRecharge:
			return "Trinkets/BlackSoulAutoRecharge";
		case Card.BlackSoulOnDamage:
			return "Trinkets/BlackSoulOnDamage";
		case Card.NeptunesCurse:
			return "Trinkets/NeptunesCurse";
		case Card.RabbitFoot:
			return "Trinkets/RabbitFoot";
		case Card.MoreRelics:
			return "Trinkets/MoreRelics";
		case Card.DecreaseRelicCharge:
			return "Trinkets/DecreaseRelicCharge";
		case Card.TentacleOnDamaged:
			return "Trinkets/TentacleOnDamaged";
		case Card.InvincibilityPerRoom:
			return "Trinkets/InvincibilityPerRoom";
		case Card.BombOnDamaged:
			return "Trinkets/BombOnDamaged";
		case Card.ImmuneToTraps:
			return "Trinkets/ImmuneToTraps";
		case Card.ImmuneToProjectiles:
			return "Trinkets/ImmuneToProjectiles";
		case Card.WalkThroughBlocks:
			return "Trinkets/WalkThroughBlocks";
		case Card.HoldToHeal:
			return "Trinkets/HoldToHeal";
		case Card.Sword:
			return "Weapons/Sword";
		case Card.Dagger:
			return "Weapons/Dagger";
		case Card.Axe:
			return "Weapons/Axe";
		case Card.Blunderbuss:
			return "Weapons/Blunderbuss";
		case Card.Fireball:
			return "Curses/Fireball";
		case Card.Tripleshot:
			return "Curses/Tripleshot";
		case Card.Tentacles:
			return "Curses/Tentacles";
		case Card.EnemyBlast:
			return "Curses/EnemyBlast";
		default:
			return "";
		}
	}

	public static int GetTarotCardWeight(Card cardType)
	{
		switch (cardType)
		{
		case Card.Hearts1:
			return 50;
		case Card.Hearts2:
			return 60;
		case Card.Hearts3:
			return 70;
		case Card.Lovers1:
			return 80;
		case Card.Lovers2:
			return 100;
		case Card.Sun:
			return 50;
		case Card.Moon:
			return 50;
		case Card.GiftFromBelow:
			return 50;
		case Card.Spider:
			return 50;
		case Card.DiseasedHeart:
			return 50;
		case Card.EyeOfWeakness:
			return 50;
		case Card.DeathsDoor:
			return 50;
		case Card.TheDeal:
			return 50;
		case Card.Telescope:
			return 50;
		case Card.HandsOfRage:
			return 50;
		case Card.NaturesGift:
			return 50;
		case Card.Skull:
			return 50;
		case Card.Potion:
			return 50;
		case Card.MovementSpeed:
			return 75;
		case Card.AttackRate:
			return 75;
		case Card.IncreasedDamage:
			return 75;
		case Card.IncreaseBlackSoulsDrop:
			return 50;
		case Card.HealChance:
			return 75;
		case Card.NegateDamageChance:
			return 75;
		case Card.BombOnRoll:
			return 50;
		case Card.GoopOnDamaged:
			return 50;
		case Card.GoopOnRoll:
			return 50;
		case Card.PoisonImmune:
			return 50;
		case Card.DamageOnRoll:
			return 100;
		case Card.RabbitFoot:
			return 60;
		case Card.HealTwiceAmount:
			return 75;
		case Card.AmmoEfficient:
			return 100;
		case Card.BlackSoulAutoRecharge:
			return 100;
		case Card.BlackSoulOnDamage:
			return 75;
		default:
			return 100;
		}
	}

	public static int GetMaxTarotCardLevel(Card cardType)
	{
		switch (cardType)
		{
		case Card.Hearts1:
			return 0;
		case Card.Hearts2:
			return 0;
		case Card.Hearts3:
			return 0;
		case Card.Lovers1:
			return 0;
		case Card.Lovers2:
			return 0;
		case Card.Sun:
			return 0;
		case Card.Moon:
			return 0;
		case Card.GiftFromBelow:
			return 1;
		case Card.Spider:
			return 0;
		case Card.DiseasedHeart:
			return 0;
		case Card.EyeOfWeakness:
			return 0;
		case Card.Arrows:
			return 0;
		case Card.DeathsDoor:
			return 2;
		case Card.TheDeal:
			return 0;
		case Card.Telescope:
			return 0;
		case Card.HandsOfRage:
			return 1;
		case Card.NaturesGift:
			return 0;
		case Card.Skull:
			return 0;
		case Card.Potion:
			return 2;
		case Card.MovementSpeed:
			return 2;
		case Card.AttackRate:
			return 2;
		case Card.IncreasedDamage:
			return 1;
		case Card.IncreaseBlackSoulsDrop:
			return 1;
		case Card.HealChance:
			return 1;
		case Card.NegateDamageChance:
			return 1;
		case Card.BombOnRoll:
			return 1;
		case Card.GoopOnDamaged:
			return 0;
		case Card.GoopOnRoll:
			return 1;
		case Card.PoisonImmune:
			return 0;
		case Card.DamageOnRoll:
			return 0;
		case Card.HealTwiceAmount:
			return 0;
		case Card.InvincibleWhileHealing:
			return 0;
		case Card.AmmoEfficient:
			return 2;
		case Card.BlackSoulAutoRecharge:
			return 1;
		case Card.BlackSoulOnDamage:
			return 1;
		case Card.NeptunesCurse:
			return 0;
		case Card.RabbitFoot:
			return 0;
		case Card.MoreRelics:
			return 0;
		case Card.DecreaseRelicCharge:
			return 2;
		default:
			return 0;
		}
	}

	public static string AnimationSuffix(Card Type)
	{
		switch (Type)
		{
		case Card.Sword:
			return "sword";
		case Card.Dagger:
			return "dagger";
		case Card.Axe:
			return "axe";
		case Card.Blunderbuss:
			return "blunderbuss";
		case Card.Hammer:
			return "hammer";
		case Card.Fireball:
			return "fireball";
		case Card.Tentacles:
			return "tentacle";
		case Card.EnemyBlast:
			return "enemyblast";
		case Card.Tripleshot:
			return "tripleshot";
		case Card.Gauntlet:
			return "guantlets";
		default:
			return string.Format("Card {0} Animation Suffix not set", Type);
		}
	}

	public static TarotCard DrawRandomCard()
	{
		List<Card> unusedFoundTrinkets = GetUnusedFoundTrinkets();
		if (unusedFoundTrinkets.Count <= 0)
		{
			return null;
		}
		Card card = unusedFoundTrinkets[UnityEngine.Random.Range(0, unusedFoundTrinkets.Count)];
		int num = 0;
		if (DataManager.Instance.dungeonRun >= 5)
		{
			while (UnityEngine.Random.Range(0f, 1f) < 0.275f * DataManager.Instance.GetLuckMultiplier())
			{
				num++;
			}
		}
		num = Mathf.Min(num, GetMaxTarotCardLevel(card));
		return new TarotCard(card, num);
	}

	public static Card GiveNewTrinket()
	{
		List<Card> unfoundTrinkets = GetUnfoundTrinkets();
		return unfoundTrinkets[UnityEngine.Random.Range(0, unfoundTrinkets.Count)];
	}

	public static List<Card> GetUnfoundTrinkets()
	{
		List<Card> list = new List<Card>();
		foreach (Card allTrinket in DataManager.AllTrinkets)
		{
			bool flag = false;
			foreach (Card playerFoundTrinket in DataManager.Instance.PlayerFoundTrinkets)
			{
				if (playerFoundTrinket == allTrinket)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(allTrinket);
			}
		}
		return list;
	}

	public static bool IsUnlocked(Card card)
	{
		return DataManager.Instance.PlayerFoundTrinkets.Contains(card);
	}

	public static bool UnlockTrinket(Card card)
	{
		if (!DataManager.Instance.PlayerFoundTrinkets.Contains(card))
		{
			DataManager.Instance.Alerts.TarotCardAlerts.AddOnce(card);
			DataManager.Instance.PlayerFoundTrinkets.Add(card);
			if (DataManager.Instance.PlayerFoundTrinkets.Count >= DataManager.AllTrinkets.Count)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_TAROTS_UNLOCKED"));
			}
			return true;
		}
		return false;
	}

	public static void UnlockTrinkets(params Card[] cards)
	{
		for (int i = 0; i < cards.Length; i++)
		{
			UnlockTrinket(cards[i]);
		}
	}

	public static Card UnlockRandomTrinket()
	{
		List<Card> list = new List<Card>();
		foreach (Card allTrinket in DataManager.AllTrinkets)
		{
			if (!DataManager.Instance.PlayerFoundTrinkets.Contains(allTrinket))
			{
				list.Add(allTrinket);
			}
		}
		if (list.Count > 0)
		{
			Card card = list[UnityEngine.Random.Range(0, list.Count)];
			DataManager.Instance.PlayerFoundTrinkets.Add(card);
			if (DataManager.Instance.PlayerFoundTrinkets.Count >= DataManager.AllTrinkets.Count)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_TAROTS_UNLOCKED"));
			}
			return card;
		}
		return Card.Count;
	}

	public static List<Card> GetUnusedFoundTrinkets()
	{
		List<Card> list = new List<Card>();
		foreach (Card playerFoundTrinket in DataManager.Instance.PlayerFoundTrinkets)
		{
			if ((IsCurseRelatedTarotCard(playerFoundTrinket) && !DataManager.Instance.EnabledSpells) || (IsWeaponRelatedTarotCard(playerFoundTrinket) && DataManager.Instance.PlayerFleece == 6) || (IsHealthRelatedTarotCard(playerFoundTrinket) && (DataManager.Instance.PlayerFleece == 7 || DataManager.Instance.PlayerFleece == 8)) || (IsResourceRelatedCard(playerFoundTrinket) && DungeonSandboxManager.Active) || (playerFoundTrinket == Card.WalkThroughBlocks && DataManager.Instance.PlayerScaleModifier > 1) || (playerFoundTrinket == Card.TheDeal && DataManager.Instance.PlayerFleece != 7))
			{
				continue;
			}
			bool flag = false;
			foreach (TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
			{
				if (playerRunTrinket.CardType == playerFoundTrinket)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(playerFoundTrinket);
			}
		}
		return list;
	}

	private static bool IsHealthRelatedTarotCard(Card card)
	{
		switch (card)
		{
		case Card.Hearts1:
		case Card.Hearts2:
		case Card.Hearts3:
		case Card.Lovers1:
		case Card.Lovers2:
		case Card.DiseasedHeart:
		case Card.DeathsDoor:
		case Card.TheDeal:
		case Card.HealChance:
		case Card.GoopOnDamaged:
		case Card.HealTwiceAmount:
		case Card.BlackSoulOnDamage:
		case Card.TentacleOnDamaged:
		case Card.BombOnDamaged:
			return true;
		default:
			return false;
		}
	}

	private static bool IsCurseRelatedTarotCard(Card card)
	{
		switch (card)
		{
		case Card.Arrows:
		case Card.Potion:
		case Card.IncreaseBlackSoulsDrop:
		case Card.AmmoEfficient:
		case Card.BlackSoulAutoRecharge:
		case Card.BlackSoulOnDamage:
			return true;
		default:
			return false;
		}
	}

	private static bool IsWeaponRelatedTarotCard(Card card)
	{
		if (card == Card.HandsOfRage || (uint)(card - 32) <= 1u)
		{
			return true;
		}
		return false;
	}

	private static bool IsResourceRelatedCard(Card card)
	{
		if (card == Card.NaturesGift || card == Card.NeptunesCurse)
		{
			return true;
		}
		return false;
	}

	public static void ApplyInstantEffects(TarotCard card)
	{
		switch (card.CardType)
		{
		case Card.IncreasedDamage:
			AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/swords_appear", PlayerFarming.Instance.transform.position);
			BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "strength", "strength");
			break;
		case Card.Lovers1:
			if (!PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				PlayerFarming.Instance.GetComponent<HealthPlayer>().BlueHearts += 2f;
				Vector3 position = PlayerFarming.Instance.CameraBone.transform.position;
				BiomeConstants.Instance.EmitHeartPickUpVFX(position, 0f, "blue", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/player/collect_blue_heart", position);
			}
			break;
		case Card.Lovers2:
			if (!PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				PlayerFarming.Instance.GetComponent<HealthPlayer>().BlueHearts += 4f;
				Vector3 position2 = PlayerFarming.Instance.CameraBone.transform.position;
				BiomeConstants.Instance.EmitHeartPickUpVFX(position2, 0f, "blue", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/player/collect_blue_heart", position2);
			}
			break;
		case Card.DiseasedHeart:
			if (!PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				PlayerFarming.Instance.GetComponent<HealthPlayer>().BlackHearts += 2f;
				Vector3 position3 = PlayerFarming.Instance.CameraBone.transform.position;
				BiomeConstants.Instance.EmitHeartPickUpVFX(position3, 0f, "black", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/player/collect_blue_heart", position3);
			}
			break;
		case Card.TheDeal:
			if (ResurrectOnHud.ResurrectionType == ResurrectionType.Pyre)
			{
				ResurrectOnHud.OverridenResurrectionType = ResurrectionType.Pyre;
			}
			ResurrectOnHud.ResurrectionType = ResurrectionType.DealTarot;
			break;
		case Card.Hearts1:
		case Card.Hearts2:
		case Card.Hearts3:
			if (!PlayerFleeceManager.FleecePreventsHealthPickups())
			{
				((HealthPlayer)PlayerFarming.Instance.health).TotalSpiritHearts += GetSpiritHeartCount(card);
				BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "red", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/player/collect_blue_heart", PlayerFarming.Instance.transform.position);
			}
			break;
		}
		FaithAmmo.Ammo = FaithAmmo.Ammo;
	}

	public static float GetSpiritHeartCount(TarotCard card)
	{
		switch (card.CardType)
		{
		case Card.Hearts1:
			return 1f;
		case Card.Hearts2:
			return 2f;
		case Card.Hearts3:
			return 4f;
		default:
			return 0f;
		}
	}

	public static int GetSpiritAmmoCount(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.Arrows)
		{
			float f = (float)(DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO + DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO) * 0.2f;
			return Mathf.Max(1, Mathf.RoundToInt(f));
		}
		return 0;
	}

	public static float GetWeaponDamageMultiplerIncrease(TarotCard card)
	{
		switch (card.CardType)
		{
		case Card.Moon:
			if (!TimeManager.IsNight)
			{
				return 0f;
			}
			return 0.3f;
		case Card.Sun:
			if (!TimeManager.IsDay)
			{
				return 0f;
			}
			return 0.2f;
		case Card.IncreasedDamage:
		{
			int upgradeIndex = card.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 0.5f;
			}
			return 0.2f;
		}
		default:
			return 0f;
		}
	}

	public static float GetCurseDamageMultiplerIncrease(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.Potion)
		{
			switch (card.UpgradeIndex)
			{
			case 1:
				return 0.5f;
			case 2:
				return 1f;
			default:
				return 0.25f;
			}
		}
		return 0f;
	}

	public static float GetWeaponCritChanceIncrease(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.EyeOfWeakness)
		{
			return 0.1f;
		}
		return 0f;
	}

	public static int GetLootIncreaseModifier(TarotCard card, InventoryItem.ITEM_TYPE itemType)
	{
		Card cardType = card.CardType;
		if (cardType == Card.NaturesGift)
		{
			return 1;
		}
		return 0;
	}

	public static float GetMovementSpeedMultiplier(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.MovementSpeed)
		{
			switch (card.UpgradeIndex)
			{
			case 1:
				return 0.5f;
			case 2:
				return 1f;
			default:
				return 0.25f;
			}
		}
		return 0f;
	}

	public static float GetAttackRateMultiplier(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.AttackRate)
		{
			switch (card.UpgradeIndex)
			{
			case 1:
				return 0.5f;
			case 2:
				return 2f;
			default:
				return 0.25f;
			}
		}
		return 0f;
	}

	public static float GetBlackSoulsMultiplier(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.IncreaseBlackSoulsDrop)
		{
			int upgradeIndex = card.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 2f;
			}
			return 1f;
		}
		return 0f;
	}

	public static float GetHealChance(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.HealChance)
		{
			int upgradeIndex = card.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 0.2f;
			}
			return 0.1f;
		}
		return 0f;
	}

	public static float GetNegateDamageChance(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.NegateDamageChance)
		{
			int upgradeIndex = card.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 0.2f;
			}
			return 0.1f;
		}
		return 0f;
	}

	public static int GetDamageAllEnemiesAmount(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.DeathsDoor)
		{
			switch (card.UpgradeIndex)
			{
			case 1:
				return 3;
			case 2:
				return 10;
			default:
				return 2;
			}
		}
		return 0;
	}

	public static int GetHealthAmountMultiplier(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.HealTwiceAmount)
		{
			return 1;
		}
		return 0;
	}

	public static float GetAmmoEfficiency(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.AmmoEfficient)
		{
			switch (card.UpgradeIndex)
			{
			case 1:
				return 0.5f;
			case 2:
				return 0.75f;
			default:
				return 0.25f;
			}
		}
		return 0f;
	}

	public static int GetBlackSoulsOnDamage(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.BlackSoulOnDamage)
		{
			int upgradeIndex = card.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 10;
			}
			return 5;
		}
		return 0;
	}

	public static InventoryItem GetItemToDrop(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.NeptunesCurse && UnityEngine.Random.Range(0f, 1f) > 0.9f)
		{
			List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>
			{
				InventoryItem.ITEM_TYPE.FISH,
				InventoryItem.ITEM_TYPE.FISH_BIG,
				InventoryItem.ITEM_TYPE.FISH_SMALL
			};
			return new InventoryItem(list[UnityEngine.Random.Range(0, list.Count)]);
		}
		return null;
	}

	public static float GetChanceOfGainingBlueHeart(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.GiftFromBelow)
		{
			int upgradeIndex = card.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 0.1f;
			}
			return 0.05f;
		}
		return 0f;
	}

	public static float GetChanceForRelicsMultiplier(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.MoreRelics)
		{
			return 5f;
		}
		return 0f;
	}

	public static float GetRelicChargeMultiplier(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.DecreaseRelicCharge)
		{
			switch (card.UpgradeIndex)
			{
			case 2:
				return 1f;
			case 1:
				return 0.5f;
			default:
				return 0.25f;
			}
		}
		return 0f;
	}

	public static float GetInvincibilityTimeEnteringNewRoom(TarotCard card)
	{
		Card cardType = card.CardType;
		if (cardType == Card.InvincibilityPerRoom)
		{
			switch (card.UpgradeIndex)
			{
			case 2:
				return 4f;
			case 1:
				return 3.5f;
			default:
				return 3f;
			}
		}
		return 0f;
	}
}
