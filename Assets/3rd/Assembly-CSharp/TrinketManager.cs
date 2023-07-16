using System.Collections.Generic;
using UnityEngine;

public class TrinketManager
{
	public delegate void TrinketsUpdated();

	public delegate void TrinketUpdated(TarotCards.Card trinket);

	public static Dictionary<TarotCards.Card, float> _cardCooldowns = new Dictionary<TarotCards.Card, float>();

	public static event TrinketsUpdated OnTrinketsCleared;

	public static event TrinketUpdated OnTrinketAdded;

	public static event TrinketUpdated OnTrinketRemoved;

	public static event TrinketUpdated OnTrinketCooldownStart;

	public static event TrinketUpdated OnTrinketCooldownEnd;

	public static void AddTrinket(TarotCards.TarotCard card)
	{
		PlayerFleeceManager.OnTarotCardPickedUp();
		if (!DataManager.Instance.PlayerRunTrinkets.Contains(card))
		{
			DataManager.Instance.PlayerRunTrinkets.Add(card);
		}
		TarotCards.ApplyInstantEffects(card);
		TrinketUpdated onTrinketAdded = TrinketManager.OnTrinketAdded;
		if (onTrinketAdded != null)
		{
			onTrinketAdded(card.CardType);
		}
	}

	public static void RemoveTrinket(TarotCards.Card card)
	{
		for (int num = DataManager.Instance.PlayerRunTrinkets.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.PlayerRunTrinkets[num].CardType == card)
			{
				DataManager.Instance.PlayerRunTrinkets.RemoveAt(num);
			}
		}
		TrinketUpdated onTrinketRemoved = TrinketManager.OnTrinketRemoved;
		if (onTrinketRemoved != null)
		{
			onTrinketRemoved(card);
		}
	}

	public static TarotCards.TarotCard RemoveRandomTrinket()
	{
		TarotCards.TarotCard tarotCard = DataManager.Instance.PlayerRunTrinkets[Random.Range(0, DataManager.Instance.PlayerRunTrinkets.Count)];
		DataManager.Instance.PlayerRunTrinkets.Remove(tarotCard);
		TrinketUpdated onTrinketRemoved = TrinketManager.OnTrinketRemoved;
		if (onTrinketRemoved != null)
		{
			onTrinketRemoved(tarotCard.CardType);
		}
		return tarotCard;
	}

	public static void RemoveAllTrinkets()
	{
		DataManager.Instance.PlayerRunTrinkets.Clear();
		TrinketsUpdated onTrinketsCleared = TrinketManager.OnTrinketsCleared;
		if (onTrinketsCleared != null)
		{
			onTrinketsCleared();
		}
	}

	public static bool HasTrinket(TarotCards.Card card)
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == card)
			{
				return true;
			}
		}
		return false;
	}

	public static void UpdateCooldowns(float deltaTime)
	{
		foreach (TarotCards.Card item in new List<TarotCards.Card>(_cardCooldowns.Keys))
		{
			if (!(_cardCooldowns[item] > 0f))
			{
				continue;
			}
			_cardCooldowns[item] = Mathf.Max(_cardCooldowns[item] - deltaTime, 0f);
			if (_cardCooldowns[item] <= 0f)
			{
				TrinketUpdated onTrinketCooldownEnd = TrinketManager.OnTrinketCooldownEnd;
				if (onTrinketCooldownEnd != null)
				{
					onTrinketCooldownEnd(item);
				}
			}
		}
	}

	public static void TriggerCooldown(TarotCards.Card card)
	{
		_cardCooldowns[card] = GetCardMaxCooldownSeconds(card);
		TrinketUpdated onTrinketCooldownStart = TrinketManager.OnTrinketCooldownStart;
		if (onTrinketCooldownStart != null)
		{
			onTrinketCooldownStart(card);
		}
	}

	public static bool IsOnCooldown(TarotCards.Card card)
	{
		return GetRemainingCooldownSeconds(card) > 0f;
	}

	public static float GetRemainingCooldownSeconds(TarotCards.Card card)
	{
		float value = 0f;
		_cardCooldowns.TryGetValue(card, out value);
		return value;
	}

	public static float GetRemainingCooldownPercent(TarotCards.Card card)
	{
		float remainingCooldownSeconds = GetRemainingCooldownSeconds(card);
		float cardMaxCooldownSeconds = GetCardMaxCooldownSeconds(card);
		if (cardMaxCooldownSeconds != 0f)
		{
			return remainingCooldownSeconds / cardMaxCooldownSeconds;
		}
		return 0f;
	}

	public static int GetSpiritAmmo()
	{
		int num = 0;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetSpiritAmmoCount(playerRunTrinket);
		}
		return num;
	}

	public static float GetWeaponDamageMultiplerIncrease()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetWeaponDamageMultiplerIncrease(playerRunTrinket);
		}
		return num;
	}

	public static float GetCurseDamageMultiplerIncrease()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetCurseDamageMultiplerIncrease(playerRunTrinket);
		}
		return num;
	}

	public static float GetWeaponCritChanceIncrease()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetWeaponCritChanceIncrease(playerRunTrinket);
		}
		return num;
	}

	public static int GetLootIncreaseModifier(InventoryItem.ITEM_TYPE itemType)
	{
		int num = 0;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetLootIncreaseModifier(playerRunTrinket, itemType);
		}
		return num;
	}

	private static float GetCardMaxCooldownSeconds(TarotCards.Card card)
	{
		TarotCards.TarotCard tarotCard = null;
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == card)
			{
				tarotCard = DataManager.Instance.PlayerRunTrinkets[i];
				break;
			}
		}
		switch (card)
		{
		case TarotCards.Card.HandsOfRage:
		{
			int upgradeIndex = tarotCard.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 5f;
			}
			return 10f;
		}
		case TarotCards.Card.BombOnRoll:
		{
			int upgradeIndex = tarotCard.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 5f;
			}
			return 10f;
		}
		case TarotCards.Card.GoopOnRoll:
		{
			int upgradeIndex = tarotCard.UpgradeIndex;
			if (upgradeIndex == 1)
			{
				return 5f;
			}
			return 10f;
		}
		case TarotCards.Card.BlackSoulAutoRecharge:
		{
			float num = 1f;
			switch (tarotCard.UpgradeIndex)
			{
			case 1:
				num = 1.5f;
				break;
			case 2:
				num = 0.2f;
				break;
			default:
				num = 2.5f;
				break;
			}
			if (DataManager.Instance.PlayerFleece == 6)
			{
				num /= 4f;
			}
			return num;
		}
		case TarotCards.Card.TheDeal:
			return float.MaxValue;
		default:
			return 0f;
		}
	}

	public static int GetDamageAllEnemiesAmount(TarotCards.Card card)
	{
		int num = 0;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetDamageAllEnemiesAmount(playerRunTrinket);
		}
		return num;
	}

	public static float GetAttackRateMultiplier()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetAttackRateMultiplier(playerRunTrinket);
		}
		return num;
	}

	public static float GetMovementSpeedMultiplier()
	{
		float num = 1f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetMovementSpeedMultiplier(playerRunTrinket);
		}
		return num;
	}

	public static float GetBlackSoulsMultiplier()
	{
		float num = 1f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetBlackSoulsMultiplier(playerRunTrinket);
		}
		return num;
	}

	public static float GetHealChance()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetHealChance(playerRunTrinket);
		}
		return num;
	}

	public static float GetNegateDamageChance()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetNegateDamageChance(playerRunTrinket);
		}
		return num;
	}

	public static int GetHealthAmountMultiplier()
	{
		int num = 1;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetHealthAmountMultiplier(playerRunTrinket);
		}
		return num;
	}

	public static float GetAmmoEfficiencyMultiplier()
	{
		float num = 1f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetAmmoEfficiency(playerRunTrinket);
		}
		return num;
	}

	public static int GetBlackSoulsOnDamaged()
	{
		int num = 0;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetBlackSoulsOnDamage(playerRunTrinket);
		}
		return num;
	}

	public static InventoryItem[] GetItemsToDrop()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			InventoryItem itemToDrop = TarotCards.GetItemToDrop(playerRunTrinket);
			if (itemToDrop != null)
			{
				list.Add(itemToDrop);
			}
		}
		return list.ToArray();
	}

	public static float GetChanceOfGainingBlueHeart()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetChanceOfGainingBlueHeart(playerRunTrinket);
		}
		return num;
	}

	public static float GetChanceForRelicsMultiplier()
	{
		float num = 1f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetChanceForRelicsMultiplier(playerRunTrinket);
		}
		return num;
	}

	public static float GetRelicChargeMultiplier()
	{
		float num = 1f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetRelicChargeMultiplier(playerRunTrinket);
		}
		return num;
	}

	public static float GetInvincibilityTimeEnteringNewRoom()
	{
		float num = 0f;
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			num += TarotCards.GetInvincibilityTimeEnteringNewRoom(playerRunTrinket);
		}
		return num;
	}

	public static bool DropBombOnRoll()
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == TarotCards.Card.BombOnRoll)
			{
				return true;
			}
		}
		return false;
	}

	public static bool DropBlackGoopOnRoll()
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == TarotCards.Card.GoopOnRoll)
			{
				return true;
			}
		}
		return false;
	}

	public static bool DropBlackGoopOnDamaged()
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == TarotCards.Card.GoopOnDamaged)
			{
				return true;
			}
		}
		return false;
	}

	public static bool DropBombOnDamaged()
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == TarotCards.Card.BombOnDamaged)
			{
				return true;
			}
		}
		return false;
	}

	public static bool DropTentacleOnDamaged()
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == TarotCards.Card.TentacleOnDamaged)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPoisonImmune()
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == TarotCards.Card.PoisonImmune)
			{
				return true;
			}
		}
		return false;
	}

	public static bool DamageEnemyOnRoll()
	{
		for (int i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			if (DataManager.Instance.PlayerRunTrinkets[i].CardType == TarotCards.Card.DamageOnRoll)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanNegateDamage()
	{
		foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
		{
			TarotCards.Card cardType = playerRunTrinket.CardType;
			if (cardType == TarotCards.Card.InvincibleWhileHealing)
			{
				return true;
			}
		}
		return false;
	}
}
