using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class MMSelectable_Slider : MMSelectable
	{
		[SerializeField]
		private MMSlider _slider;

		public MMSlider Slider
		{
			get
			{
				return _slider;
			}
		}

		public override IMMSelectable TryNavigateLeft()
		{
			_slider.DecrementValue();
			return this;
		}

		public override IMMSelectable TryNavigateRight()
		{
			_slider.IncrementValue();
			return this;
		}
	}
}
