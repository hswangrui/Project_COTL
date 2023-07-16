using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class WeaponItem : PlayerMenuItem<EquipmentType>
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private GameObject _lockedContainer;

		public EquipmentType Type { get; private set; }

		public override void Configure(EquipmentType type)
		{
			Type = type;
			_icon.gameObject.SetActive(type != EquipmentType.None);
			_lockedContainer.SetActive(type == EquipmentType.None);
			if (type != EquipmentType.None)
			{
				Image icon = _icon;
				Sprite sprite = (_icon.sprite = EquipmentManager.GetEquipmentData(type).WorldSprite);
				icon.sprite = sprite;
			}
		}
	}
}
