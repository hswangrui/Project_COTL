using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MMScrollbar : Scrollbar, IMMSelectable
	{
		private const float kScrollThreshold = 0.35f;

		public Selectable Selectable
		{
			get
			{
				return this;
			}
		}

		public bool Interactable
		{
			get
			{
				return base.interactable;
			}
			set
			{
				base.interactable = value;
			}
		}

		protected override void Update()
		{
			base.Update();
			if (!Application.isPlaying || !Interactable)
			{
				return;
			}
			float verticalAxis = InputManager.UI.GetVerticalAxis();
			if (verticalAxis > 0.35f)
			{
				if (base.value >= 1f)
				{
					return;
				}
			}
			else if (verticalAxis < -0.35f && base.value <= 0f)
			{
				return;
			}
			base.value += verticalAxis * Time.deltaTime;
		}

		public bool TryPerformConfirmAction()
		{
			return false;
		}

		public IMMSelectable TryNavigateLeft()
		{
			return this;
		}

		public IMMSelectable TryNavigateRight()
		{
			return this;
		}

		public IMMSelectable TryNavigateUp()
		{
			return this;
		}

		public IMMSelectable TryNavigateDown()
		{
			return this;
		}

		public IMMSelectable FindSelectableFromDirection(Vector3 direction)
		{
			return Selectable.FindSelectable(direction) as IMMSelectable;
		}

		public void SetNormalTransitionState()
		{
		}

		public void SetInteractionState(bool state)
		{
		}
	}
}
