using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class MMSelectable_HorizontalSelector : MMSelectable
	{
		[SerializeField]
		private MMHorizontalSelector _horizontalSelector;

		public MMHorizontalSelector HorizontalSelector
		{
			get
			{
				return _horizontalSelector;
			}
		}

		public override bool Interactable
		{
			get
			{
				return base.interactable;
			}
			set
			{
				base.Interactable = value;
				_horizontalSelector.Interactable = Interactable;
			}
		}

		public override IMMSelectable TryNavigateLeft()
		{
			if (Interactable)
			{
				_horizontalSelector.LeftButton.TryPerformConfirmAction();
			}
			return this;
		}

		public override IMMSelectable TryNavigateRight()
		{
			if (Interactable)
			{
				_horizontalSelector.RightButton.TryPerformConfirmAction();
			}
			return this;
		}
	}
}
