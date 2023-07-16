using System;

namespace src.Alerts
{
	public class RelicAlerts : AlertCategory<RelicType>
	{
		private UpgradeSystem.Type[] _allRituals;

		public RelicAlerts()
		{
			EquipmentManager.OnRelicUnlocked = (Action<RelicType>)Delegate.Combine(EquipmentManager.OnRelicUnlocked, new Action<RelicType>(OnRelicUnlocked));
		}

		~RelicAlerts()
		{
			EquipmentManager.OnRelicUnlocked = (Action<RelicType>)Delegate.Remove(EquipmentManager.OnRelicUnlocked, new Action<RelicType>(OnRelicUnlocked));
		}

		private void OnRelicUnlocked(RelicType relicType)
		{
			AddOnce(relicType);
		}
	}
}
