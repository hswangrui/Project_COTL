using UnityEngine;
using WebSocketSharp;

namespace Lamb.UI
{
	public class MMSelectable_Toggle : MMSelectable
	{
		[SerializeField]
		private MMToggle _toggle;

		public MMToggle Toggle
		{
			get
			{
				return _toggle;
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
				_toggle.Interactable = Interactable;
			}
		}

		public override bool TryPerformConfirmAction()
		{
			_toggle.Toggle();
			if (!_confirmSFX.IsNullOrEmpty())
			{
				UIManager.PlayAudio(_confirmSFX);
				RumbleManager.Instance.Rumble();
			}
			return true;
		}
	}
}
