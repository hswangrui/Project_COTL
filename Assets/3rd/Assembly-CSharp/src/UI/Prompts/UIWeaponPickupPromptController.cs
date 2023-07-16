using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;

namespace src.UI.Prompts
{
	public class UIWeaponPickupPromptController : UIPromptBase
	{
		[Header("Content")]
		[SerializeField]
		private InfoCardOutlineRenderer _outlineRenderer;

		[SerializeField]
		private TextMeshProUGUI _title;

		[SerializeField]
		private TextMeshProUGUI _lore;

		[SerializeField]
		private TextMeshProUGUI _description;

		[Header("Stats")]
		[SerializeField]
		private TextMeshProUGUI _damageText;

		[SerializeField]
		private TextMeshProUGUI _speedText;

		[Header("Stats")]
		[SerializeField]
		private TextMeshProUGUI _charge;

		[SerializeField]
		private GameObject _fragileIcon;

		private EquipmentType _weaponType;

		private RelicType _relicType;

		private float _damage;

		private float _speed;

		private int _weaponLevel;

		private bool _isWeapon
		{
			get
			{
				return _weaponType < EquipmentType.Tentacles;
			}
		}

		public void Show(EquipmentType weaponType, float damage, float speed, int weaponLevel, bool instant = false)
		{
			Show(instant);
			_weaponType = weaponType;
			_damage = damage;
			_speed = speed;
			_weaponLevel = weaponLevel;
			_damage = Mathf.Round(damage * 100f) / 100f;
			_damageText.gameObject.SetActive(_isWeapon);
			_speedText.gameObject.SetActive(_isWeapon);
			if (_isWeapon)
			{
				_outlineRenderer.BadgeVariant = 6;
			}
			else
			{
				_outlineRenderer.BadgeVariant = 7;
			}
		}

		public void Show(RelicType relicType, bool instant = false)
		{
			Show(instant);
			_relicType = relicType;
			_weaponType = EquipmentType.None;
			_damageText.gameObject.SetActive(false);
			_speedText.gameObject.SetActive(false);
			_outlineRenderer.BadgeVariant = 7;
		}

		protected override void Localize()
		{
			if (_relicType == RelicType.None)
			{
				_title.text = EquipmentManager.GetEquipmentData(_weaponType).GetLocalisedTitle() + " " + _weaponLevel.ToNumeral();
				_lore.text = EquipmentManager.GetEquipmentData(_weaponType).GetLocalisedLore();
				_description.text = EquipmentManager.GetEquipmentData(_weaponType).GetLocalisedDescription();
			}
			else
			{
				_title.text = RelicData.GetTitleLocalisation(_relicType);
				_lore.text = RelicData.GetLoreLocalization(_relicType);
				_description.text = RelicData.GetDescriptionLocalisation(_relicType);
				RelicData relicData = EquipmentManager.GetRelicData(_relicType);
				_fragileIcon.gameObject.SetActive(relicData.InteractionType == RelicInteractionType.Fragile);
				RelicChargeCategory chargeCategory = RelicData.GetChargeCategory(relicData);
				_charge.gameObject.SetActive(true);
				if (relicData.InteractionType == RelicInteractionType.Fragile)
				{
					_charge.text = ScriptLocalization.UI.Fragile;
				}
				else
				{
					_charge.text = LocalizationManager.GetTranslation("UI/Charge") + "<b>" + RelicData.GetChargeCategoryColor(chargeCategory) + LocalizationManager.GetTranslation(string.Format("UI/{0}", chargeCategory));
				}
			}
			if (_isWeapon)
			{
				float averageWeaponDamage = PlayerFarming.Instance.playerWeapon.GetAverageWeaponDamage(DataManager.Instance.CurrentWeapon, DataManager.Instance.CurrentWeaponLevel);
				float weaponSpeed = PlayerFarming.Instance.playerWeapon.GetWeaponSpeed(DataManager.Instance.CurrentWeapon);
				string damage = ScriptLocalization.UI_WeaponSelect.Damage;
				string text = "";
				string arg = "<color=#F5EDD5>";
				if (averageWeaponDamage > _damage)
				{
					text = "<sprite name=\"icon_FaithDown\">";
					arg = "<color=#FF1C1C>";
				}
				else if (averageWeaponDamage < _damage)
				{
					text = "<sprite name=\"icon_FaithUp\">";
					arg = "<color=#2DFF1C>";
				}
				string speed = ScriptLocalization.UI_WeaponSelect.Speed;
				string text2 = "";
				string arg2 = "<color=#F5EDD5>";
				if (weaponSpeed > _speed)
				{
					text2 = "<sprite name=\"icon_FaithDown\">";
					arg2 = "<color=#FF1C1C>";
				}
				else if (weaponSpeed < _speed)
				{
					text2 = "<sprite name=\"icon_FaithUp\">";
					arg2 = "<color=#2DFF1C>";
				}
				_damageText.text = string.Format(damage + ": " + text + "{0}{1}</color>", arg, _damage);
				_speedText.text = string.Format(speed + ": " + text2 + "{0}{1}</color>", arg2, _speed);
			}
		}
	}
}
