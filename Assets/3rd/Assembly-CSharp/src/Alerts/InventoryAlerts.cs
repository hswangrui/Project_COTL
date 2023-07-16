namespace src.Alerts
{
	public class InventoryAlerts : AlertCategory<InventoryItem.ITEM_TYPE>
	{
		public override int Total
		{
			get
			{
				int num = 0;
				foreach (InventoryItem.ITEM_TYPE alert in _alerts)
				{
					if (HasAlert(alert))
					{
						num++;
					}
				}
				return num;
			}
		}

		public InventoryAlerts()
		{
			Inventory.OnItemAddedToInventory += OnItemAdded;
		}

		~InventoryAlerts()
		{
			Inventory.OnItemAddedToInventory -= OnItemAdded;
		}

		private void OnItemAdded(InventoryItem.ITEM_TYPE itemType, int Delta)
		{
			AddOnce(itemType);
		}

		public override bool HasAlert(InventoryItem.ITEM_TYPE alert)
		{
			if (base.HasAlert(alert) && Inventory.GetItemQuantity(alert) > 0)
			{
				return true;
			}
			return false;
		}
	}
}
