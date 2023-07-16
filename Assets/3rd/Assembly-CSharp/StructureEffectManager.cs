using System;
using System.Collections.Generic;
using I2.Loc;

public class StructureEffectManager
{
	public enum EffectType
	{
		Shrine_DevotionEffeciency,
		Shrine_ExtendedShift,
		Farm_ExtendedShift,
		Farm_BloodFertilizer,
		Farm_CorpseFertilizer,
		Cooking_FoodScraps,
		Cooking_BulkierMeals,
		Cooking_Fast,
		Cooking_MindAlteringMeals
	}

	public enum State
	{
		DoesntExist,
		Active,
		Cooldown,
		On,
		Off
	}

	public static Action<int, EffectType, State> OnEffectChange;

	public static List<StructureEffect> StructureEffects
	{
		get
		{
			return DataManager.Instance.StructureEffects;
		}
		set
		{
			DataManager.Instance.StructureEffects = value;
		}
	}

	public static string GetLocalizedName(EffectType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/StructureEffect/{0}", Type));
	}

	public static string GetLocalizedKey(EffectType type)
	{
		return string.Format("Structures/StructureEffect/{0}", type);
	}

	public static string GetLocalizedDescription(EffectType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Structures/StructureEffect/{0}/Description", Type));
	}

	public static string GetLocalizedDescriptionKey(EffectType type)
	{
		return string.Format("Structures/StructureEffect/{0}/Description", type);
	}

	public static void Tick()
	{
		int num = -1;
		while (++num < StructureEffects.Count)
		{
			if (!StructureEffects[num].CoolingDown && TimeManager.TotalElapsedGameTime - StructureEffects[num].TimeStarted >= StructureEffects[num].DurationInGameMinutes)
			{
				StructureEffects[num].BeginCooldown();
			}
			if (TimeManager.TotalElapsedGameTime - StructureEffects[num].TimeStarted >= StructureEffects[num].DurationInGameMinutes + StructureEffects[num].CoolDownInGameMinutes)
			{
				Action<int, EffectType, State> onEffectChange = OnEffectChange;
				if (onEffectChange != null)
				{
					onEffectChange(StructureEffects[num].StructureID, StructureEffects[num].Type, State.Off);
				}
				StructureEffects.RemoveAt(num);
				num--;
			}
		}
	}

	public static State GetEffectAvailability(int StructureID, EffectType Type)
	{
		foreach (StructureEffect structureEffect in StructureEffects)
		{
			if (structureEffect.StructureID == StructureID && structureEffect.Type == Type)
			{
				if (TimeManager.TotalElapsedGameTime - structureEffect.TimeStarted < structureEffect.DurationInGameMinutes)
				{
					return State.Active;
				}
				if (TimeManager.TotalElapsedGameTime - structureEffect.TimeStarted >= structureEffect.DurationInGameMinutes && TimeManager.TotalElapsedGameTime - structureEffect.TimeStarted < structureEffect.DurationInGameMinutes + structureEffect.CoolDownInGameMinutes)
				{
					return State.Cooldown;
				}
			}
		}
		return State.DoesntExist;
	}

	public static float GetEffectCoolDownProgress(int StructureID, EffectType Type)
	{
		foreach (StructureEffect structureEffect in StructureEffects)
		{
			if (structureEffect.StructureID == StructureID && structureEffect.Type == Type)
			{
				return structureEffect.CoolDownProgress;
			}
		}
		return 0f;
	}

	public static void CreateEffect(int StructureID, EffectType Type)
	{
		StructureEffect structureEffect = new StructureEffect();
		structureEffect.Create(StructureID, Type);
		StructureEffects.Add(structureEffect);
		Action<int, EffectType, State> onEffectChange = OnEffectChange;
		if (onEffectChange != null)
		{
			onEffectChange(StructureID, Type, State.On);
		}
	}
}
