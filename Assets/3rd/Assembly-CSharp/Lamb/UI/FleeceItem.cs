using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class FleeceItem : PlayerMenuItem<int>
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private FleeceIconMapping _fleeceIconMapping;

		public override void Configure(int item)
		{
			_fleeceIconMapping.GetImage(item, _icon);
		}
	}
}
