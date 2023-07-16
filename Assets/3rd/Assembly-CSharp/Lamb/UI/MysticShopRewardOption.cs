using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MysticShopRewardOption : MonoBehaviour
	{
		[SerializeField]
		private GameObject _container;

		[SerializeField]
		private Image _itemGraphic;

		[SerializeField]
		private InventoryIconMapping _iconMapping;

		private InventoryItem.ITEM_TYPE _item;

		public void Configure(InventoryItem.ITEM_TYPE item)
		{
			_item = item;
			_itemGraphic.sprite = _iconMapping.GetImage(_item);
		}

		public void Choose()
		{
			_container.SetActive(false);
		}
	}
}
