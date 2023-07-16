using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class WeaponInfoCard : UIInfoCardBase<EquipmentType>
	{
		[SerializeField]
		private WeaponItem _weaponItem;

		[SerializeField]
		private TextMeshProUGUI _itemHeader;

		[SerializeField]
		private TextMeshProUGUI _itemLore;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[SerializeField]
		private TextMeshProUGUI _levelText;

		[SerializeField]
		private GameObject _statsHeader;

		[SerializeField]
		private GameObject _statsContainer;

		[SerializeField]
		private TextMeshProUGUI _damageText;

		[SerializeField]
		private TextMeshProUGUI _speedText;

		public override void Configure(EquipmentType equipmentType)
		{
			_weaponItem.Configure(equipmentType);
			if (equipmentType != EquipmentType.None)
			{
				_itemHeader.text = LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Name", equipmentType)) + " " + DataManager.Instance.CurrentWeaponLevel.ToNumeral();
				_itemLore.text = LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Lore", equipmentType));
				_itemDescription.text = LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Description", equipmentType));
				_statsHeader.SetActive(true);
				_statsContainer.SetActive(true);
				_levelText.text = DataManager.Instance.CurrentWeaponLevel.ToNumeral();
				_damageText.text = ScriptLocalization.UI_WeaponSelect.Damage + ": <color=#FFD201>" + PlayerFarming.Instance.playerWeapon.GetAverageWeaponDamage(equipmentType, DataManager.Instance.CurrentWeaponLevel);
				_speedText.text = ScriptLocalization.UI_WeaponSelect.Speed + ": <color=#FFD201>" + PlayerFarming.Instance.playerWeapon.GetWeaponSpeed(equipmentType);
			}
			else
			{
				_itemHeader.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/NoWeapon");
				_itemLore.gameObject.SetActive(false);
				_itemDescription.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/NoWeapon/Description");
				_levelText.text = "";
				_statsHeader.SetActive(false);
				_statsContainer.SetActive(false);
			}
		}
	}
}
