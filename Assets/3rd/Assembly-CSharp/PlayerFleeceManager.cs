using Lamb.UI.DeathScreen;

public class PlayerFleeceManager
{
	public enum FleeceType
	{
		Default = 0,
		Gold = 1,
		Green = 2,
		Purple = 3,
		White = 4,
		Blue = 5,
		CurseInsteadOfWeapon = 6,
		OneHitKills = 7,
		HollowHeal = 8,
		NoRolling = 9,
		RelicInsteadOfCurse = 10,
		BlindFaith = 11,
		Outfit_999 = 999,
		GodOfDeath = 1000
	}

	public delegate void DamageEvent(float damageMultiplier);

	private static float damageMultiplier = 1f;

	public static DamageEvent OnDamageMultiplierModified;

	public static float OneHitKillHP = 1f;

	public static bool OneHitKillGivesRedHearts = false;

	public static float GetCursesDamageMultiplier()
	{
		switch ((FleeceType)DataManager.Instance.PlayerFleece)
		{
		case FleeceType.Gold:
			return damageMultiplier;
		case FleeceType.Green:
			return 1f;
		case FleeceType.CurseInsteadOfWeapon:
			return 0.75f;
		default:
			return 1f;
		}
	}

	public static float GetCursesFeverMultiplier()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.Green)
		{
			return 0.5f;
		}
		return 1f;
	}

	public static float GetWeaponDamageMultiplier()
	{
		switch ((FleeceType)DataManager.Instance.PlayerFleece)
		{
		case FleeceType.Gold:
			return damageMultiplier;
		case FleeceType.Green:
			return -0.5f;
		case FleeceType.OneHitKills:
			return 10f;
		default:
			return 0f;
		}
	}

	public static float GetDamageReceivedMultiplier()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.Gold)
		{
			return 1f;
		}
		return 0f;
	}

	public static float GetHealthMultiplier()
	{
		if (!GameManager.IsDungeon(PlayerFarming.Location))
		{
			return 1f;
		}
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.NoRolling)
		{
			return 2f;
		}
		return 1f;
	}

	public static float GetLootMultiplier(UIDeathScreenOverlayController.Results _result)
	{
		return 0f;
	}

	public static void OnTarotCardPickedUp()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.Purple && PlayerFarming.Instance.health.BlackHearts <= 0f)
		{
			PlayerFarming.Instance.health.BlackHearts = 2f;
		}
	}

	public static int GetFreeTarotCards()
	{
		switch ((FleeceType)DataManager.Instance.PlayerFleece)
		{
		case FleeceType.White:
			return 4;
		case FleeceType.CurseInsteadOfWeapon:
			return 4;
		default:
			return 0;
		}
	}

	public static void IncrementDamageModifier()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.Gold)
		{
			damageMultiplier += 0.05f;
			DamageEvent onDamageMultiplierModified = OnDamageMultiplierModified;
			if (onDamageMultiplierModified != null)
			{
				onDamageMultiplierModified(damageMultiplier);
			}
		}
	}

	public static void ResetDamageModifier()
	{
		damageMultiplier = 0f;
		DamageEvent onDamageMultiplierModified = OnDamageMultiplierModified;
		if (onDamageMultiplierModified != null)
		{
			onDamageMultiplierModified(damageMultiplier);
		}
	}

	public static bool FleeceCausesPoisonOnHit()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.Purple)
		{
			return true;
		}
		return false;
	}

	public static bool FleecePreventsHealthPickups(bool justRedHeartCheck = false)
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (justRedHeartCheck && playerFleece == FleeceType.OneHitKills && OneHitKillGivesRedHearts)
		{
			return false;
		}
		if ((uint)(playerFleece - 7) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool FleeceNoRedHeartsToUse()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.Blue || playerFleece == FleeceType.OneHitKills)
		{
			return true;
		}
		return false;
	}

	public static bool FleeceSwapsWeaponForCurse()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.CurseInsteadOfWeapon)
		{
			return true;
		}
		return false;
	}

	public static bool FleeceSwapsCurseForRelic()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.RelicInsteadOfCurse)
		{
			return true;
		}
		return false;
	}

	public static bool FleecePreventTarotCards()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.White)
		{
			return true;
		}
		return false;
	}

	public static bool FleecePreventsRoll()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.NoRolling)
		{
			return true;
		}
		return false;
	}

	public static float AmountToHealOnRoomComplete()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.NoRolling)
		{
			return 2f;
		}
		return 0f;
	}

	public static float GetRelicChargeMultiplier()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.RelicInsteadOfCurse)
		{
			return 3f;
		}
		return 1f;
	}

	public static bool FleecePreventsRespawn()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.OneHitKills)
		{
			return true;
		}
		return false;
	}

	public static bool BleatToHeal()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.HollowHeal)
		{
			return true;
		}
		return false;
	}

	public static float FleeceSpeedMultiplier()
	{
		FleeceType playerFleece = (FleeceType)DataManager.Instance.PlayerFleece;
		if (playerFleece == FleeceType.NoRolling)
		{
			return 1.65f;
		}
		return 1f;
	}
}
