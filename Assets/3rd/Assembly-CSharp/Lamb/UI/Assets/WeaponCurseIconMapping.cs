using System;
using UnityEngine;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Weapon Curse Icon Mapping", menuName = "Massive Monster/Weapon Curse Icon Mapping", order = 1)]
	public class WeaponCurseIconMapping : ScriptableObject
	{
		[Serializable]
		public class SpriteItem
		{
			[SerializeField]
			private TarotCards.Card _itemType;

			[SerializeField]
			private Sprite _sprite;

			public TarotCards.Card ItemType
			{
				get
				{
					return _itemType;
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
		private SpriteItem[] _weaponSprites;

		public Sprite GetSprite(TarotCards.Card card)
		{
			SpriteItem[] weaponSprites = _weaponSprites;
			foreach (SpriteItem spriteItem in weaponSprites)
			{
				if (spriteItem.ItemType == card)
				{
					return spriteItem.Sprite;
				}
			}
			return null;
		}
	}
}
