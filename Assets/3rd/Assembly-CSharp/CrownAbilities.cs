using System;
using I2.Loc;
using UnityEngine;

[Serializable]
public class CrownAbilities
{
	public enum TYPE
	{
		Combat_Arrows,
		Combat_HeavyAttack,
		Combat_Arrows_I,
		Combat_Hearts,
		Followers_CheerUp,
		Followers_Charisma,
		Followers_Bliss,
		Abilities_GrappleHook,
		Abilities_FishingRod,
		Abilities_SpecialKey,
		Abilities_Hunting,
		Abilities_Heart_I,
		Abilities_Heart_II,
		Abilities_Heart_III
	}

	public TYPE Type;

	public bool Unlocked;

	public float UnlockProgress;

	public bool Used;

	public static int CrownAbilitiesUnlocked()
	{
		int num = 0;
		foreach (CrownAbilities item in DataManager.Instance.CrownAbilitiesUnlocked)
		{
			if (item.Unlocked)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetCrownAbilitiesCost(TYPE Type)
	{
		switch (Type)
		{
		case TYPE.Combat_Arrows:
			return 1;
		case TYPE.Followers_CheerUp:
			return 1;
		case TYPE.Abilities_GrappleHook:
			return 1;
		case TYPE.Combat_Arrows_I:
			return 1;
		case TYPE.Followers_Charisma:
			return 1;
		case TYPE.Abilities_FishingRod:
			return 1;
		default:
			return 1;
		}
	}

	public static void OnUnlockAbility(TYPE Types)
	{
		switch (Types)
		{
		case TYPE.Combat_Arrows:
			UnityEngine.Object.FindObjectOfType<HUD_Ammo>().Play();
			UIAbilityUnlock.Play(UIAbilityUnlock.Ability.Arrows);
			break;
		case TYPE.Abilities_GrappleHook:
			UIAbilityUnlock.Play(UIAbilityUnlock.Ability.GrappleHook);
			break;
		case TYPE.Abilities_FishingRod:
			UIAbilityUnlock.Play(UIAbilityUnlock.Ability.FishingRod);
			break;
		case TYPE.Combat_HeavyAttack:
			UIAbilityUnlock.Play(UIAbilityUnlock.Ability.HeavyAttack);
			break;
		case TYPE.Abilities_Heart_I:
		case TYPE.Abilities_Heart_II:
		case TYPE.Abilities_Heart_III:
		{
			HealthPlayer healthPlayer = UnityEngine.Object.FindObjectOfType<HealthPlayer>();
			healthPlayer.totalHP += 2f;
			healthPlayer.HP = healthPlayer.totalHP;
			break;
		}
		}
	}

	public static void UnlockAbility(TYPE Types)
	{
		if (!CrownAbilityUnlocked(Types))
		{
			DataManager.Instance.CrownAbilitiesUnlocked.Add(new CrownAbilities
			{
				Type = Types,
				Unlocked = true
			});
			OnUnlockAbility(Types);
		}
	}

	public static bool CrownAbilityUnlocked(TYPE Types)
	{
		if (DataManager.Instance.CrownAbilitiesUnlocked.Count == 0)
		{
			return false;
		}
		foreach (CrownAbilities item in DataManager.Instance.CrownAbilitiesUnlocked)
		{
			if (item.Type == Types)
			{
				return true;
			}
		}
		return false;
	}

	public static string LocalisedName(TYPE Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Abilities/{0}/Title", Type));
	}

	public static string LocalisedDescription(TYPE Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Abilities/{0}/Description", Type));
	}

	public static string LocalisedExplanation(TYPE Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Abilities/{0}/Explanation", Type));
	}
}
