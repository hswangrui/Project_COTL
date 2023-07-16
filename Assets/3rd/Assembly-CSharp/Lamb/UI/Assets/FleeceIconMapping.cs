using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Fleece Icon Mapping", menuName = "Massive Monster/Fleece Icon Mapping", order = 1)]
	public class FleeceIconMapping : ScriptableObject
	{
		[Serializable]
		public class ItemSpritePair
		{
			public int FleeceIndex;

			public Sprite Sprite;
		}

		[SerializeField]
		private List<ItemSpritePair> _itemImages;

		private Dictionary<int, Sprite> _itemMap;

		private void Initialise()
		{
			_itemMap = new Dictionary<int, Sprite>();
			foreach (ItemSpritePair itemImage in _itemImages)
			{
				_itemMap.Add(itemImage.FleeceIndex, itemImage.Sprite);
			}
		}

		public Sprite GetImage(int index)
		{
			if (_itemMap == null || _itemImages.Count != _itemMap.Keys.Count)
			{
				Initialise();
			}
			Sprite value;
			if (_itemMap.TryGetValue(index, out value))
			{
				return value;
			}
			return null;
		}

		public void GetImage(int index, Image image)
		{
			Sprite sprite = (image.sprite = GetImage(index));
			if (sprite == null)
			{
				image.material = null;
				image.color = Color.magenta;
			}
		}
	}
}
