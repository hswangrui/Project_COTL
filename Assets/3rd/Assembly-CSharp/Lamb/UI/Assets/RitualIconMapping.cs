using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Ritual Icon Mapping", menuName = "Massive Monster/Ritual Icon Mapping", order = 1)]
	public class RitualIconMapping : ScriptableObject
	{
		[Serializable]
		public class RitualSprite
		{
			public UpgradeSystem.Type RitualType;

			public Sprite Sprite;
		}

		[SerializeField]
		private List<RitualSprite> _items;

		private Dictionary<UpgradeSystem.Type, Sprite> _itemMap;

		private void Initialise()
		{
			_itemMap = new Dictionary<UpgradeSystem.Type, Sprite>();
			foreach (RitualSprite item in _items)
			{
				_itemMap.Add(item.RitualType, item.Sprite);
			}
		}

		public Sprite GetImage(UpgradeSystem.Type type)
		{
			if (type == UpgradeSystem.Type.Count)
			{
				return null;
			}
			if (_itemMap == null || _items.Count != _itemMap.Keys.Count)
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

		public void GetImage(UpgradeSystem.Type type, Image image)
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
