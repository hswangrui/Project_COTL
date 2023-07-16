using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Items
{
	public class ActiveRelicItem : MonoBehaviour
	{
		[SerializeField]
		private Image _relicImage;

		[SerializeField]
		private MMSelectable _selectable;

		public RelicData RelicData { get; private set; }

		public MMSelectable Selectable
		{
			get
			{
				return _selectable;
			}
		}

		public void Configure(RelicData relicData)
		{
			RelicData = relicData;
			_relicImage.sprite = relicData.UISprite;
		}
	}
}
