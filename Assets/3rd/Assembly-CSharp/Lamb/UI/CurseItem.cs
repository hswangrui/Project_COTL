using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class CurseItem : PlayerMenuItem<EquipmentType>
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
				_icon.sprite = EquipmentManager.GetEquipmentData(type).WorldSprite;
			}
		}
	}
}
