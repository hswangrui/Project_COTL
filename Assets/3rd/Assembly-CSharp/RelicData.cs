using I2.Loc;
using UnityEngine;

[CreateAssetMenu(menuName = "Massive Monster/Relic Data")]
public class RelicData : ScriptableObject
{
	public RelicType RelicType;

	public RelicInteractionType InteractionType;

	public RelicSubType RelicSubType;

	public float DamageRequiredToCharge = 30f;

	[Space]
	public Sprite UISprite;

	public Sprite UISpriteOutline;

	public Sprite WorldSprite;

	[Space]
	public float Weight = 1f;

	public bool RequiresCult;

	public bool ShowAnimationAbovePlayer = true;

	public UpgradeSystem.Type UpgradeType = UpgradeSystem.Type.Count;

	public VFXAbilitySequenceData VFXData;

	public Material Material;

	public static string GetTitleLocalisation(RelicType relicType)
	{
		return LocalizationManager.GetTermTranslation(string.Format("Relics/{0}", relicType));
	}

	public static string GetDescriptionLocalisation(RelicType relicType)
	{
		return LocalizationManager.GetTermTranslation(string.Format("Relics/{0}/Description", relicType));
	}

	public static string GetLoreLocalization(RelicType relicType)
	{
		return LocalizationManager.GetTermTranslation(string.Format("Relics/{0}/Lore", relicType));
	}

	public static RelicChargeCategory GetChargeCategory(RelicType relicType)
	{
		return GetChargeCategory(EquipmentManager.GetRelicData(relicType));
	}

	public static RelicChargeCategory GetChargeCategory(RelicData relicData)
	{
		float damageRequiredToCharge = relicData.DamageRequiredToCharge;
		if (damageRequiredToCharge < 50f)
		{
			return RelicChargeCategory.Fast;
		}
		if (damageRequiredToCharge < 80f)
		{
			return RelicChargeCategory.Average;
		}
		return RelicChargeCategory.Slow;
	}

	public static string GetChargeCategoryColor(RelicChargeCategory category)
	{
		switch (category)
		{
		case RelicChargeCategory.Fast:
			return "<color=#00FFC2>";
		case RelicChargeCategory.Average:
			return "<color=#FF8C2F>";
		case RelicChargeCategory.Slow:
			return "<color=#FD1D03>";
		default:
			return "<color=#00FFC2>";
		}
	}
}
