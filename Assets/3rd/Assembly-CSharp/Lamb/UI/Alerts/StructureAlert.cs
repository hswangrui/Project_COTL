namespace Lamb.UI.Alerts
{
	public class StructureAlert : AlertBadge<StructureBrain.TYPES>
	{
		protected override AlertCategory<StructureBrain.TYPES> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Structures;
			}
		}
	}
}
