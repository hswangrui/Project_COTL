using UnityEngine;

namespace Lamb.UI
{
	public class MMSelectable_Dropdown : MMSelectable
	{
		[SerializeField]
		private MMDropdown _dropdown;

		public MMDropdown Dropdown
		{
			get
			{
				return _dropdown;
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
				_dropdown.Interactable = Interactable;
			}
		}

		public override bool TryPerformConfirmAction()
		{
			_dropdown.Open();
			return true;
		}
	}
}
