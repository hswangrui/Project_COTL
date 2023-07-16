using Lamb.UI.Alerts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lamb.UI
{
	public class GenericInventoryItem : UIInventoryItem, ISelectHandler, IEventSystemHandler
	{
		[SerializeField]
		protected InventoryAlert _alert;

		public override void Configure(InventoryItem item, bool showQuantity = true)
		{
			base.Configure(item, showQuantity);
			if (_alert != null)
			{
				_alert.Configure(base.Type);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (_alert != null)
			{
				_alert.TryRemoveAlert();
			}
		}
	}
}
