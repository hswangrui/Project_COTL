using I2.Loc;
using UnityEngine;

[CreateAssetMenu(menuName = "COTL/Dungeon Modifier")]
public class DungeonModifier : ScriptableObject
{
	public Sprite modifierIcon;

	public DungeonPositiveModifier PositiveModifier;

	public DungeonNegativeModifier NegativeModifier;

	public DungeonNeutralModifier NeutralModifier;

	[Space]
	[Range(0f, 1f)]
	public float Probability = 0.5f;

	public static float ChanceOfModifier = 0.3f;

	private static DungeonModifier currentModifier;

	private static DungeonModifier[] dungeonModifiers;

	private bool noNeutralModifier
	{
		get
		{
			return NeutralModifier == DungeonNeutralModifier.None;
		}
	}

	public static DungeonModifier GetModifier(float increaseChance = 0f)
	{
		if (increaseChance == -1f || !DataManager.Instance.EnabledSpells)
		{
			return null;
		}
		if (DungeonSandboxManager.Active)
		{
			increaseChance += 0.3f;
		}
		float num = Random.Range(0f, 1f - increaseChance);
		if (ChanceOfModifier >= num)
		{
			if (dungeonModifiers == null)
			{
				dungeonModifiers = Resources.LoadAll<DungeonModifier>("Data/Dungeon Modifiers");
			}
			int num2 = 0;
			while (num2 < 100)
			{
				DungeonModifier dungeonModifier = dungeonModifiers[Random.Range(0, dungeonModifiers.Length)];
				if (dungeonModifier.NeutralModifier == DungeonNeutralModifier.LoseRedGainTarot && !DataManager.Instance.CanFindTarotCards)
				{
					num2++;
					continue;
				}
				if (dungeonModifier.NeutralModifier == DungeonNeutralModifier.ChestsDropFoodNotGold && DungeonSandboxManager.Active)
				{
					num2++;
					continue;
				}
				if ((dungeonModifier.NeutralModifier == DungeonNeutralModifier.LoseRedGainTarot || dungeonModifier.NeutralModifier == DungeonNeutralModifier.LoseRedGainBlackHeart) && PlayerFleeceManager.FleeceNoRedHeartsToUse())
				{
					num2++;
					continue;
				}
				if (dungeonModifier.PositiveModifier == DungeonPositiveModifier.DoubleGold && DungeonSandboxManager.Active)
				{
					num2++;
					continue;
				}
				if (dungeonModifier.NeutralModifier == DungeonNeutralModifier.LoseRedGainTarot && DungeonSandboxManager.Active && DungeonSandboxManager.CurrentScenario.ScenarioType == DungeonSandboxManager.ScenarioType.BossRushMode)
				{
					num2++;
					continue;
				}
				num = Random.Range(0f, 1f);
				if (dungeonModifier.Probability >= num)
				{
					return dungeonModifier;
				}
				num2++;
			}
		}
		return null;
	}

	public static void SetActiveModifier(DungeonModifier modifier)
	{
		currentModifier = modifier;
		if ((bool)currentModifier)
		{
			Debug.Log(string.Format("MODIFIER SET {0}-Positive    {1}-Negative,    {2}-Neutral", currentModifier.PositiveModifier, currentModifier.NegativeModifier, currentModifier.NeutralModifier));
		}
		else if (HUD_Manager.Instance != null)
		{
			HUD_Manager.Instance.CurrentDungeonModifierText.text = "";
			HUD_Manager.Instance.CurrentDungeonModifierText.gameObject.SetActive(false);
		}
	}

	public static bool HasModifierActive()
	{
		return currentModifier != null;
	}

	public static bool HasPositiveModifier(DungeonPositiveModifier positiveModifier)
	{
		if (currentModifier != null && currentModifier.NeutralModifier == DungeonNeutralModifier.None)
		{
			return currentModifier.PositiveModifier == positiveModifier;
		}
		return false;
	}

	public static bool HasNegativeModifier(DungeonNegativeModifier negativeModifier)
	{
		if (currentModifier != null && currentModifier.NeutralModifier == DungeonNeutralModifier.None)
		{
			return currentModifier.NegativeModifier == negativeModifier;
		}
		return false;
	}

	public static bool HasNeutralModifier(DungeonNeutralModifier neutralModifier)
	{
		if (currentModifier != null)
		{
			return currentModifier.NeutralModifier == neutralModifier;
		}
		return false;
	}

	public static float HasPositiveModifier(DungeonPositiveModifier positiveModifier, float trueResult, float falseResult)
	{
		if (!(currentModifier != null) || currentModifier.NeutralModifier != 0 || currentModifier.PositiveModifier != positiveModifier)
		{
			return falseResult;
		}
		return trueResult;
	}

	public static float HasNegativeModifier(DungeonNegativeModifier negativeModifier, float trueResult, float falseResult)
	{
		if (!(currentModifier != null) || currentModifier.NeutralModifier != 0 || currentModifier.NegativeModifier != negativeModifier)
		{
			return falseResult;
		}
		return trueResult;
	}

	public static float HasNeutralModifier(DungeonNeutralModifier neutralModifier, float trueResult, float falseResult)
	{
		if (!(currentModifier != null) || currentModifier.NeutralModifier != neutralModifier)
		{
			return falseResult;
		}
		return trueResult;
	}

	public static string GetCurrentModifierText()
	{
		if (currentModifier == null)
		{
			return "";
		}
		string text = "";
		if (currentModifier.PositiveModifier != 0)
		{
			text = text + LocalizationManager.GetTranslation("UI/DungeonModifier/Positive/" + currentModifier.PositiveModifier) + "<br>";
		}
		if (currentModifier.NegativeModifier != 0)
		{
			text = text + LocalizationManager.GetTranslation("UI/DungeonModifier/Negative/" + currentModifier.NegativeModifier) + "<br>";
		}
		if (currentModifier.NeutralModifier != 0)
		{
			text = text + LocalizationManager.GetTranslation("UI/DungeonModifier/Neutral/" + currentModifier.NeutralModifier) + "<br>";
		}
		return text;
	}
}
