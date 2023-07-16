using UnityEngine;

namespace Lamb.UI
{
	public class FollowerIndoctrinationTab : MMTab
	{
		[SerializeField]
		private Sprite _categorySprite;

		public Sprite CategorySprite
		{
			get
			{
				return _categorySprite;
			}
		}

		public override void Configure()
		{
		}
	}
}
