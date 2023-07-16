using System;
using UnityEngine;

namespace Map
{
	[Serializable]
	public struct LayerSprite
	{
		public FollowerLocation DungeonLocation;

		public Sprite Sprite;

		public Sprite SpriteNoDecoration;

		public Sprite flair;
	}
}
