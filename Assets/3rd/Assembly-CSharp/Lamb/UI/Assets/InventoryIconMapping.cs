using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Inventory Icon Mapping", menuName = "Massive Monster/Inventory Icon Mapping", order = 1)]
	public class InventoryIconMapping : ScriptableObject
	{
		[Serializable]
		public class ItemSpritePair
		{
			public InventoryItem.ITEM_TYPE ItemType;

			public Sprite Sprite;
		}

		[SerializeField]
		private List<ItemSpritePair> _itemImages;

		private Dictionary<InventoryItem.ITEM_TYPE, Sprite> _itemMap;

		private void Initialise()
		{
			_itemMap = new Dictionary<InventoryItem.ITEM_TYPE, Sprite>();
			foreach (ItemSpritePair itemImage in _itemImages)
			{
				_itemMap.Add(itemImage.ItemType, itemImage.Sprite);
			}
		}

		public Sprite GetImage(InventoryItem.ITEM_TYPE type)
		{
			if (type == InventoryItem.ITEM_TYPE.NONE)
			{
				return null;
			}
			if (_itemMap == null || _itemImages.Count != _itemMap.Keys.Count)
			{
				Initialise();
			}
			Sprite value;
			if (_itemMap.TryGetValue(type, out value))
			{
				return value;
			}
			return null;
		}

		public void GetImage(InventoryItem.ITEM_TYPE type, Image image)
		{
			Sprite sprite = (image.sprite = GetImage(type));
			if (sprite == null)
			{
				image.material = null;
				image.color = Color.magenta;
			}
		}
	}
}
