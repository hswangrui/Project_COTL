using System;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class CurseInfoCard : UIInfoCardBase<EquipmentType>
	{
		[SerializeField]
		private CurseItem _curseItem;

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

		public override void Configure(EquipmentType equipmentType)
		{
			_curseItem.Configure(equipmentType);
			if (equipmentType != EquipmentType.None)
			{
				_itemHeader.text = LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Name", equipmentType)) + " " + DataManager.Instance.CurrentCurseLevel.ToNumeral();
				_itemLore.text = LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Lore", equipmentType));
				_itemDescription.text = LocalizationManager.GetTranslation(string.Format("UpgradeSystem/{0}/Description", equipmentType));
				_levelText.text = DataManager.Instance.CurrentCurseLevel.ToNumeral();
				_statsHeader.SetActive(true);
				_statsContainer.SetActive(true);
				_damageText.text = ScriptLocalization.UI_WeaponSelect.Damage + ": <color=#FFD201>" + Math.Round(EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Damage * PlayerSpells.GetCurseDamageMultiplier(), 1);
			}
			else
			{
				_itemHeader.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/NoCurse");
				_itemLore.gameObject.SetActive(false);
				_itemDescription.text = LocalizationManager.GetTranslation("UI/PauseScreen/Player/NoCurse/Description");
				_levelText.text = "";
				_statsHeader.SetActive(false);
				_statsContainer.SetActive(false);
			}
		}
	}
}
