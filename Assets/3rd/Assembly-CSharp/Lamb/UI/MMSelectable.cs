using System;
using src.UINavigator;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;

namespace Lamb.UI
{
	public class MMSelectable : Selectable, IMMSelectable, ISelectHandler, IEventSystemHandler
	{
		public Action OnSelected;

		public Action OnConfirm;

		public Action OnConfirmDenied;

		public Action OnDeselected;

		public Action OnPointerEntered;

		public Action OnPointerExited;

		[SerializeField]
		private MaskableGraphic[] _targetGraphics;

		[SerializeField]
		public string _confirmSFX = "event:/ui/confirm_selection";

		[SerializeField]
		public string _confirmDeniedSFX = "";

		private Color _startingColourHighConstrast;

		public Selectable Selectable
		{
			get
			{
				return this;
			}
		}

		public virtual bool Interactable
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

		public bool Confirmable { get; set; }

		public void SetNormalTransitionState()
		{
			DoStateTransition(SelectionState.Normal, true);
		}

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			if (_targetGraphics == null)
			{
				return;
			}
			Color color;
			switch (state)
			{
			default:
				color = Color.white;
				break;
			case SelectionState.Selected:
				color = base.colors.selectedColor;
				break;
			case SelectionState.Pressed:
				color = base.colors.pressedColor;
				break;
			case SelectionState.Normal:
				color = base.colors.normalColor;
				break;
			case SelectionState.Highlighted:
				color = base.colors.highlightedColor;
				break;
			case SelectionState.Disabled:
				color = base.colors.disabledColor;
				break;
			}
			Color targetColor = color;
			MaskableGraphic[] targetGraphics = _targetGraphics;
			foreach (MaskableGraphic maskableGraphic in targetGraphics)
			{
				if (maskableGraphic != null)
				{
					maskableGraphic.CrossFadeColor(targetColor, instant ? 0f : base.colors.fadeDuration, true, true);
				}
			}
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			if (Interactable)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(this);
				Action onPointerEntered = OnPointerEntered;
				if (onPointerEntered != null)
				{
					onPointerEntered();
				}
			}
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			if (Interactable)
			{
				Action onPointerExited = OnPointerExited;
				if (onPointerExited != null)
				{
					onPointerExited();
				}
			}
		}

		public void SetInteractionState(bool state)
		{
			Interactable = state;
			Graphic component = GetComponent<Graphic>();
			if (component != null)
			{
				component.raycastTarget = state;
			}
			Graphic[] componentsInChildren = GetComponentsInChildren<Graphic>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].raycastTarget = state;
			}
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			if (Interactable)
			{
				Action onDeselected = OnDeselected;
				if (onDeselected != null)
				{
					onDeselected();
				}
			}
		}

		public virtual bool TryPerformConfirmAction()
		{
			if (Confirmable)
			{
				Action onConfirm = OnConfirm;
				if (onConfirm != null)
				{
					onConfirm();
				}
				if (!_confirmSFX.IsNullOrEmpty())
				{
					UIManager.PlayAudio(_confirmSFX);
					RumbleManager.Instance.Rumble();
				}
				return true;
			}
			Action onConfirmDenied = OnConfirmDenied;
			if (onConfirmDenied != null)
			{
				onConfirmDenied();
			}
			if (!_confirmDeniedSFX.IsNullOrEmpty())
			{
				UIManager.PlayAudio(_confirmDeniedSFX);
				RumbleManager.Instance.Rumble();
			}
			return false;
		}

		public virtual IMMSelectable TryNavigateLeft()
		{
			return FindSelectableOnLeft() as IMMSelectable;
		}

		public virtual IMMSelectable TryNavigateRight()
		{
			return FindSelectableOnRight() as IMMSelectable;
		}

		public virtual IMMSelectable TryNavigateUp()
		{
			return FindSelectableOnUp() as IMMSelectable;
		}

		public virtual IMMSelectable TryNavigateDown()
		{
			return FindSelectableOnDown() as IMMSelectable;
		}

		public IMMSelectable FindSelectableFromDirection(Vector3 direction)
		{
			return Selectable.FindSelectable(direction) as IMMSelectable;
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			Action onSelected = OnSelected;
			if (onSelected != null)
			{
				onSelected();
			}
		}
	}
}
