using I2.Loc;
using UnityEngine;

public class EquipmentData : ScriptableObject
{
	public EquipmentType EquipmentType;

	public EquipmentType PrimaryEquipmentType;

	[Space]
	public bool CanBreakDodge;

	[Space]
	public Sprite UISprite;

	public Sprite WorldSprite;

	[Space]
	public string PickupAnimationKey;

	public string PerformActionSound;

	[Space]
	public float Weight;

	public string GetLocalisedTitle()
	{
		return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Name", EquipmentType));
	}

	public string GetLocalisedDescription()
	{
		if (PrimaryEquipmentType == EquipmentType.ProjectileAOE)
		{
			return "<color=#FFD201>" + ScriptLocalization.UI_Settings_Controls.HoldToAim + "</color><br>" + LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Description", EquipmentType));
		}
		return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Description", EquipmentType));
	}

	public string GetLocalisedLore()
	{
		return LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Lore", EquipmentType));
	}
}
