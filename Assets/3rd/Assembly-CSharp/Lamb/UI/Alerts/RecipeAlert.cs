namespace Lamb.UI.Alerts
{
	public class RecipeAlert : AlertBadge<InventoryItem.ITEM_TYPE>
	{
		protected override AlertCategory<InventoryItem.ITEM_TYPE> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Recipes;
			}
		}
	}
}
