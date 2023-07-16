using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Items
{
	public class RelicPlayerMenuItem : PlayerMenuItem<RelicData>
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private GameObject _lockedContainer;

		public RelicData Data { get; private set; }

		public override void Configure(RelicData data)
		{
			Data = data;
			_icon.gameObject.SetActive(Data != null);
			_lockedContainer.SetActive(Data == null);
			if (Data != null)
			{
				_icon.sprite = Data.UISprite;
			}
		}
	}
}
