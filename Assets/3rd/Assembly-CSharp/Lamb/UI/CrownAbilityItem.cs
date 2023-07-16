using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class CrownAbilityItem : PlayerMenuItem<UpgradeSystem.Type>
	{
		[SerializeField]
		private bool _ignoreLockedState;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private GameObject _lockedContainer;

		[SerializeField]
		private UpgradeTypeMapping _upgradeTypeMapping;

		public UpgradeSystem.Type Type { get; private set; }

		public override void Configure(UpgradeSystem.Type type)
		{
			Type = type;
			if (_ignoreLockedState)
			{
				_lockedContainer.SetActive(false);
			}
			else
			{
				_lockedContainer.SetActive(!UpgradeSystem.GetUnlocked(type));
			}
			_icon.sprite = _upgradeTypeMapping.GetItem(type).Sprite;
		}
	}
}
