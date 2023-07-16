using System;

namespace Lamb.UI
{
	[Serializable]
	public class FinalizedItemNotification : FinalizedNotification
	{
		public InventoryItem.ITEM_TYPE ItemType;

		public int ItemDelta;
	}
}
