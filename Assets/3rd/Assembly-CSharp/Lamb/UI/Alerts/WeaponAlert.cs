namespace Lamb.UI.Alerts
{
	public class WeaponAlert : AlertBadge<EquipmentType>
	{
		protected override AlertCategory<EquipmentType> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Weapons;
			}
		}
	}
}
