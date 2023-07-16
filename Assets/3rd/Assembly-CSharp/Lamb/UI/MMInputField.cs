using System;
using Rewired;
using src.UINavigator;
using TMPro;
using UI.Keyboards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MMInputField : TMP_InputField, IMMSelectable, IKeyboardDelegate
	{
		public Action OnSelected;

		public Action OnDeselected;

		public Action OnPointerEntered;

		public Action OnPointerExited;

		public Action OnStartedEditing;

		public Action<string> OnEndedEditing;

		[SerializeField]
		private string _confirmSFX = "event:/ui/confirm_selection";

		private ControllerType _controllerType;

		private MMOnScreenKeyboard _onScreenKeyboard;

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

		protected override void OnEnable()
		{
			base.OnEnable();
			if (Application.isPlaying)
			{
				base.onEndEdit.AddListener(OnEndEdit);
				_controllerType = InputManager.General.GetLastActiveController().type;
				GeneralInputSource general = InputManager.General;
				general.OnActiveControllerChanged = (Action<Controller>)Delegate.Combine(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (Application.isPlaying)
			{
				base.onEndEdit.RemoveListener(OnEndEdit);
				GeneralInputSource general = InputManager.General;
				general.OnActiveControllerChanged = (Action<Controller>)Delegate.Remove(general.OnActiveControllerChanged, new Action<Controller>(OnActiveControllerChanged));
			}
		}

		private void OnActiveControllerChanged(Controller controller)
		{
			_controllerType = controller.type;
		}

		public void SetNormalTransitionState()
		{
			DoStateTransition(SelectionState.Normal, true);
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			if (Interactable && !base.isFocused)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(this);
				Action onPointerEntered = OnPointerEntered;
				if (onPointerEntered != null)
				{
					onPointerEntered();
				}
			}
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (Interactable && eventData.button == PointerEventData.InputButton.Left && !base.isFocused)
			{
				TryPerformConfirmAction();
			}
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			if (Interactable && !base.isFocused)
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
			Graphic[] componentsInChildren = GetComponentsInChildren<Graphic>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].raycastTarget = state;
			}
		}

		public virtual bool TryPerformConfirmAction()
		{
			if (base.isFocused)
			{
				return false;
			}
			if (MMOnScreenKeyboard.RequiresOnScreenKeyboard())
			{
				Select();
				ActivateInputField();
				_onScreenKeyboard = base.gameObject.AddComponent<MMOnScreenKeyboard>();
				Debug.Log("Send Open Keyboard");
				_onScreenKeyboard.Show(this, this);
			}
			else
			{
				ActivateInputField();
			}
			if (!base.isFocused)
			{
				Debug.Log("OnStartedEditing");
				Action onStartedEditing = OnStartedEditing;
				if (onStartedEditing != null)
				{
					onStartedEditing();
				}
				UIManager.PlayAudio(_confirmSFX);
				RumbleManager.Instance.Rumble();
			}
			return true;
		}

		public void KeyboardDismissed(string result)
		{
			base.text = result;
			UnityEngine.Object.Destroy(_onScreenKeyboard);
			_onScreenKeyboard = null;
		}

		public void Update()
		{
			if (_controllerType == ControllerType.Joystick && InputManager.UI.GetCancelButtonDown())
			{
				DeactivateInputField();
			}
		}

		public virtual IMMSelectable TryNavigateLeft()
		{
			if (base.isFocused)
			{
				return null;
			}
			return FindSelectableOnLeft() as IMMSelectable;
		}

		public virtual IMMSelectable TryNavigateRight()
		{
			if (base.isFocused)
			{
				return null;
			}
			return FindSelectableOnRight() as IMMSelectable;
		}

		public virtual IMMSelectable TryNavigateUp()
		{
			if (base.isFocused)
			{
				return null;
			}
			return FindSelectableOnUp() as IMMSelectable;
		}

		public virtual IMMSelectable TryNavigateDown()
		{
			if (base.isFocused)
			{
				return null;
			}
			return FindSelectableOnDown() as IMMSelectable;
		}

		public IMMSelectable FindSelectableFromDirection(Vector3 direction)
		{
			return Selectable.FindSelectable(direction) as IMMSelectable;
		}

		public override void OnPointerDown(PointerEventData pointerEventData)
		{
			base.OnPointerDown(pointerEventData);
			Action onStartedEditing = OnStartedEditing;
			if (onStartedEditing != null)
			{
				onStartedEditing();
			}
		}

		public override void OnSelect(BaseEventData eventData)
		{
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

		private void OnEndEdit(string str)
		{
			Debug.Log("Closed Keyboard OnEndEdit");
			m_Text = str.StripHtml();
			Action<string> onEndedEditing = OnEndedEditing;
			if (onEndedEditing != null)
			{
				onEndedEditing(str);
			}
		}
	}
}
