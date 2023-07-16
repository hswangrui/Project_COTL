using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Upgrade Type Icon Mapping", menuName = "Massive Monster/Upgrade Type Icon Mapping", order = 1)]
	public class UpgradeTypeMapping : ScriptableObject
	{
		[Serializable]
		public class SpriteItem
		{
			[SerializeField]
			private UpgradeSystem.Type _ugpradeType;

			[SerializeField]
			private UpgradeSystem.Category _category;

			[SerializeField]
			private Sprite _sprite;

			public UpgradeSystem.Type UpgradeType
			{
				get
				{
					return _ugpradeType;
				}
			}

			public UpgradeSystem.Category Category
			{
				get
				{
					return _category;
				}
			}

			public Sprite Sprite
			{
				get
				{
					return _sprite;
				}
			}
		}

		[SerializeField]
		private List<SpriteItem> _upgradeImage;

		private Dictionary<UpgradeSystem.Type, SpriteItem> _upgradeMap;

		private void Initialise()
		{
			_upgradeMap = new Dictionary<UpgradeSystem.Type, SpriteItem>();
			foreach (SpriteItem item in _upgradeImage)
			{
				if (!_upgradeMap.ContainsKey(item.UpgradeType))
				{
					_upgradeMap.Add(item.UpgradeType, item);
				}
			}
		}

		public SpriteItem GetItem(UpgradeSystem.Type type)
		{
			if (_upgradeMap == null || _upgradeMap.Keys.Count != _upgradeImage.Count)
			{
				Initialise();
			}
			SpriteItem value;
			if (_upgradeMap.TryGetValue(type, out value))
			{
				return value;
			}
			return null;
		}
	}
}
