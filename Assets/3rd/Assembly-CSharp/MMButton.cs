using System;
using Lamb.UI;
using src.UINavigator;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;

public class MMButton : Button, IMMSelectable, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Action OnSelected;

	public Action OnDeselected;

	public Action OnConfirmDenied;

	public Action OnPointerEntered;

	public Action OnPointerExited;

	public Action OnTryNavigateRight;

	public Action OnTryNavigateLeft;

	[SerializeField]
	private MaskableGraphic[] _targetGraphics;

	[SerializeField]
	private bool _disableControlTransfer;

	[SerializeField]
	public string _confirmSFX = "event:/ui/confirm_selection";

	[SerializeField]
	public string _confirmDeniedSFX = "";

	[SerializeField]
	public bool _vibrateOnConfirm = true;

	[SerializeField]
	public bool _vibrateOnDeny = true;

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

	public bool Confirmable { get; set; } = true;


	public IMMSelectable FindSelectableFromDirection(Vector3 direction)
	{
		return Selectable.FindSelectable(direction) as IMMSelectable;
	}

	public void SetNormalTransitionState()
	{
		try
		{
			base.enabled = false;
			base.enabled = true;
			DoStateTransition(SelectionState.Normal, true);
		}
		catch
		{
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (_targetGraphics == null)
		{
			return;
		}
		Color targetColor = GetTargetColor(state, base.colors);
		MaskableGraphic[] targetGraphics = _targetGraphics;
		foreach (MaskableGraphic maskableGraphic in targetGraphics)
		{
			if (maskableGraphic != null)
			{
				SelectableColourProxy component;
				if (maskableGraphic.TryGetComponent<SelectableColourProxy>(out component))
				{
					targetColor = GetTargetColor(state, component.Colors);
				}
				maskableGraphic.CrossFadeColor(targetColor, instant ? 0f : base.colors.fadeDuration, true, true);
			}
		}
	}

	private Color GetTargetColor(SelectionState state, ColorBlock colorBlock)
	{
		switch (state)
		{
		default:
			return Color.white;
		case SelectionState.Selected:
			return colorBlock.selectedColor;
		case SelectionState.Pressed:
			return colorBlock.pressedColor;
		case SelectionState.Normal:
			return colorBlock.normalColor;
		case SelectionState.Highlighted:
			return colorBlock.highlightedColor;
		case SelectionState.Disabled:
			return colorBlock.disabledColor;
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && IsActive() && IsInteractable())
		{
			TryPerformConfirmAction();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsActive() && IsInteractable())
		{
			base.OnPointerEnter(eventData);
			Action onPointerEntered = OnPointerEntered;
			if (onPointerEntered != null)
			{
				onPointerEntered();
			}
			if (!_disableControlTransfer)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(this);
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (IsActive() && IsInteractable())
		{
			base.OnPointerExit(eventData);
			Action onPointerExited = OnPointerExited;
			if (onPointerExited != null)
			{
				onPointerExited();
			}
		}
	}

	public void SetInteractionState(bool state)
	{
		if (base.enabled)
		{
			Interactable = state;
			Graphic component = GetComponent<Graphic>();
			if (component != null)
			{
				component.raycastTarget = state;
			}
			Graphic[] componentsInChildren = GetComponentsInChildren<Graphic>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].raycastTarget = state;
			}
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (Interactable)
		{
			Action onSelected = OnSelected;
			if (onSelected != null)
			{
				onSelected();
			}
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

	public IMMSelectable TryNavigateLeft()
	{
		Action onTryNavigateLeft = OnTryNavigateLeft;
		if (onTryNavigateLeft != null)
		{
			onTryNavigateLeft();
		}
		return FindSelectableOnLeft() as IMMSelectable;
	}

	public IMMSelectable TryNavigateRight()
	{
		Action onTryNavigateRight = OnTryNavigateRight;
		if (onTryNavigateRight != null)
		{
			onTryNavigateRight();
		}
		return FindSelectableOnRight() as IMMSelectable;
	}

	public IMMSelectable TryNavigateUp()
	{
		return FindSelectableOnUp() as IMMSelectable;
	}

	public IMMSelectable TryNavigateDown()
	{
		return FindSelectableOnDown() as IMMSelectable;
	}

	public bool TryPerformConfirmAction()
	{
		if (Confirmable)
		{
			ButtonClickedEvent buttonClickedEvent = base.onClick;
			if (buttonClickedEvent != null)
			{
				buttonClickedEvent.Invoke();
			}
			if (!_confirmSFX.IsNullOrEmpty())
			{
				UIManager.PlayAudio(_confirmSFX);
				if (_vibrateOnConfirm)
				{
					RumbleManager.Instance.Rumble();
				}
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
			if (_vibrateOnDeny)
			{
				RumbleManager.Instance.Rumble();
			}
		}
		return false;
	}
}
