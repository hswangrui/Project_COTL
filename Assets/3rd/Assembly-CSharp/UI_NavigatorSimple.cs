using System;
using Rewired;
using Unify.Input;
using UnityEngine;
using UnityEngine.UI;

public class UI_NavigatorSimple : BaseMonoBehaviour
{
	public delegate void ChangeSelection(Selectable NewSelectable, Selectable PrevSelectable);

	public Selectable startingItem;

	public CanvasGroup canvasGroup;

	public string HorizontalNavAxisName = "Horizontal";

	public string VerticalNavAxisName = "Vertical";

	private float ButtonDownDelay;

	public bool DisableSFX;

	public Selectable prevSelectable;

	public Selectable selectable;

	private float SelectionDelay;

	public bool released;

	public bool cancelReleased;

	public Selectable newSelect;

	public bool canvasOff;

	public ChangeSelection OnChangeSelection;

	public Action OnSelectDown;

	public Action OnCancelDown;

	public Action OnDefaultSetComplete;

	private Player player
	{
		get
		{
			return RewiredInputManager.MainPlayer;
		}
	}

	private void Start()
	{
		setDefault();
	}

	public void setDefault()
	{
		if (startingItem != null && canvasGroup.interactable)
		{
			canvasOff = false;
			selectable = startingItem;
			newSelect = startingItem;
			prevSelectable = startingItem;
			unityNavigation();
		}
		Action onDefaultSetComplete = OnDefaultSetComplete;
		if (onDefaultSetComplete != null)
		{
			onDefaultSetComplete();
		}
	}

	private void unityNavigation()
	{
		if (!(newSelect != null) || !newSelect.interactable || !newSelect.gameObject.activeSelf)
		{
			return;
		}
		SelectionDelay = 2.5f;
		selectable = newSelect;
		selectable.Select();
		if (prevSelectable != selectable)
		{
			ChangeSelection onChangeSelection = OnChangeSelection;
			if (onChangeSelection != null)
			{
				onChangeSelection(selectable, prevSelectable);
			}
			if (!DisableSFX)
			{
				AudioManager.Instance.PlayOneShot("event:/ui/change_selection");
			}
		}
		prevSelectable = selectable;
	}

	private void Update()
	{
		if (!canvasGroup.interactable || canvasGroup.alpha == 0f)
		{
			canvasOff = true;
			return;
		}
		if (canvasOff)
		{
			setDefault();
		}
		SelectionDelay -= Time.unscaledDeltaTime;
		if (player == null)
		{
			return;
		}
		if (!released && !InputManager.UI.GetAcceptButtonHeld())
		{
			released = true;
		}
		if ((ButtonDownDelay -= Time.unscaledDeltaTime) < 0f && released && InputManager.UI.GetAcceptButtonDown() && selectable != null)
		{
			Action onSelectDown = OnSelectDown;
			if (onSelectDown != null)
			{
				onSelectDown();
			}
			Button component = selectable.GetComponent<Button>();
			if ((object)component != null)
			{
				Button.ButtonClickedEvent onClick = component.onClick;
				if (onClick != null)
				{
					onClick.Invoke();
				}
			}
			ButtonDownDelay = 0.2f;
			RumbleManager.Instance.Rumble();
			if (!DisableSFX)
			{
				AudioManager.Instance.PlayOneShot("event:/ui/confirm_selection");
			}
		}
		if (!cancelReleased && !InputManager.UI.GetCancelButtonHeld())
		{
			cancelReleased = true;
		}
		if ((ButtonDownDelay -= Time.unscaledDeltaTime) < 0f && cancelReleased && InputManager.UI.GetCancelButtonUp())
		{
			Action onCancelDown = OnCancelDown;
			if (onCancelDown != null)
			{
				onCancelDown();
			}
			ButtonDownDelay = 0.2f;
		}
		if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) <= 0.2f && Mathf.Abs(InputManager.UI.GetVerticalAxis()) <= 0.2f)
		{
			SelectionDelay = 0f;
		}
		if (!(SelectionDelay < 0f) || !(selectable != null))
		{
			return;
		}
		if (InputManager.UI.GetHorizontalAxis() > 0.2f)
		{
			newSelect = selectable.FindSelectableOnRight();
			unityNavigation();
		}
		if (InputManager.UI.GetHorizontalAxis() < -0.2f)
		{
			newSelect = selectable.FindSelectableOnLeft();
			unityNavigation();
		}
		if (InputManager.UI.GetVerticalAxis() < -0.35f)
		{
			if (selectable != null && selectable is Scrollbar && ((Scrollbar)selectable).value > 0.01f)
			{
				return;
			}
			newSelect = selectable.FindSelectableOnDown();
			unityNavigation();
		}
		if (InputManager.UI.GetVerticalAxis() > 0.35f && (!(selectable != null) || !(selectable is Scrollbar) || !(((Scrollbar)selectable).value < 0.99f)))
		{
			newSelect = selectable.FindSelectableOnUp();
			unityNavigation();
		}
	}
}
