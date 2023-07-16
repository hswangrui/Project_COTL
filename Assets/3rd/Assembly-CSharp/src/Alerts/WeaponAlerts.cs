using System;

namespace src.Alerts
{
	public class WeaponAlerts : AlertCategory<EquipmentType>
	{
		public WeaponAlerts()
		{
			DataManager.OnWeaponUnlocked = (Action<EquipmentType>)Delegate.Combine(DataManager.OnWeaponUnlocked, new Action<EquipmentType>(OnWeaponUnlocked));
		}

		~WeaponAlerts()
		{
			if (DataManager.Instance != null)
			{
				DataManager.OnWeaponUnlocked = (Action<EquipmentType>)Delegate.Remove(DataManager.OnWeaponUnlocked, new Action<EquipmentType>(OnWeaponUnlocked));
			}
		}

		private void OnWeaponUnlocked(EquipmentType weapon)
		{
			AddOnce(weapon);
		}
	}
}
