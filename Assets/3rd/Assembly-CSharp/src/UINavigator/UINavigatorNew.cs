using System;
using Lamb.UI;
using Rewired;
using Rewired.Integration.UnityUI;
using Unify;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace src.UINavigator
{
	public class UINavigatorNew : MonoSingleton<UINavigatorNew>
	{
		public const float kHorizontalAxisThreshold = 0.2f;

		public const float kVerticalAxisThreshold = 0.2f;

		private const float kSelectionDelayMax = 0.2f;

		private const float kSelectionHoldDelayReduction = 0.05f;

		private const float kSelectionHoldDelayReductionThreshold = 2f;

		private const float kButtonDownDelayMax = 0.1f;

		private const float kButtonHoldDelayMax = 0.25f;

		private const float kButtonHoldDelayReduction = 0.05f;

		private const int kButtonHoldDelayReductionThreshold = 2;

		public Action<Selectable, Selectable> OnSelectionChanged;

		public Action<Selectable> OnDefaultSetComplete;

		public Action OnCancelDown;

		public Action OnPageNavigateLeft;

		public Action OnPageNavigateRight;

		public Action OnClear;

		public bool AllowAcceptHold;

		public bool LockNavigation;

		public bool LockInput;

		[SerializeField]
		private bool _disableSFX;

		private RewiredStandaloneInputModule _inputModule;

		private Vector3 _recentMoveVector;

		private float _selectionDelay;

		private float _buttonDownDelay;

		private int _navigationHold;

		private int _consecutiveHold;

		private IMMSelectable _currentSelectable;

		private Vector3 _previousMouseInput;

		public IMMSelectable CurrentSelectable
		{
			get
			{
				return _currentSelectable;
			}
		}

		public Vector2 RecentMoveVector
		{
			get
			{
				return _recentMoveVector;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitializeUINavigator()
		{
			GameObject obj = new GameObject();
			obj.AddComponent<UINavigatorNew>();
			obj.name = "UINavigator";
		}

		public override void Start()
		{
			base.Start();
			base.transform.SetParent(null);
			EnsureInputModuleUpdated();
			SceneManager.activeSceneChanged += OnActiveSceneChanged;
		}

		private void OnActiveSceneChanged(Scene current, Scene next)
		{
			EnsureInputModuleUpdated();
		}

		private void EnsureInputModuleUpdated()
		{
			if (EventSystem.current != null)
			{
				EventSystem.current.transform.SetParent(null);
			}
			if (MonoSingleton<RewiredEventSystem>.Instance != null)
			{
				_inputModule = MonoSingleton<RewiredEventSystem>.Instance.GetComponent<RewiredStandaloneInputModule>();
				_inputModule.allowMouseInput = InputManager.General.MouseInputActive;
				_previousMouseInput = Input.mousePosition;
			}
		}

		private void Update()
		{
			UnifyManager.Platform platform = UnifyManager.platform;
			if ((platform == UnifyManager.Platform.Standalone || platform == UnifyManager.Platform.None) && _inputModule != null)
			{
				Controller lastActiveController = InputManager.General.GetLastActiveController();
				if (lastActiveController != null)
				{
					if (!_inputModule.allowMouseInput)
					{
						if (Input.mousePosition != _previousMouseInput)
						{
							_inputModule.allowMouseInput = true;
							InputManager.General.MouseInputActive = true;
						}
					}
					else if (lastActiveController.type != ControllerType.Mouse && InputManager.General.GetAnyButton())
					{
						_previousMouseInput = Input.mousePosition;
						_inputModule.allowMouseInput = false;
						InputManager.General.MouseInputActive = false;
						if (_currentSelectable != null)
						{
							_currentSelectable.Selectable.OnPointerExit(null);
						}
					}
				}
			}
			if (!LockInput)
			{
				if (_buttonDownDelay <= 0f)
				{
					if (InputManager.UI.GetAcceptButtonDown() || (AllowAcceptHold && InputManager.UI.GetAcceptButtonHeld()))
					{
						PerformConfirmAction();
					}
					else if (InputManager.UI.GetCancelButtonDown())
					{
						PerformCancelAction();
					}
				}
				else
				{
					_buttonDownDelay -= Time.unscaledDeltaTime;
				}
				if (InputManager.UI.GetAcceptButtonUp())
				{
					_consecutiveHold = 0;
					_buttonDownDelay = 0f;
				}
			}
			if (LockNavigation)
			{
				return;
			}
			if (_selectionDelay <= 0f)
			{
				if ((UnityEngine.Object)_currentSelectable != null)
				{
					IMMSelectable iMMSelectable = null;
					if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) > Mathf.Abs(InputManager.UI.GetVerticalAxis()))
					{
						if (InputManager.UI.GetHorizontalAxis() > 0.2f)
						{
							iMMSelectable = _currentSelectable.TryNavigateRight();
						}
						else if (InputManager.UI.GetHorizontalAxis() < -0.2f)
						{
							iMMSelectable = _currentSelectable.TryNavigateLeft();
						}
					}
					else if (InputManager.UI.GetVerticalAxis() > 0.2f)
					{
						iMMSelectable = _currentSelectable.TryNavigateUp();
					}
					else if (InputManager.UI.GetVerticalAxis() < -0.2f)
					{
						iMMSelectable = _currentSelectable.TryNavigateDown();
					}
					if (iMMSelectable != null)
					{
						ChangeSelection(iMMSelectable);
					}
				}
				if (InputManager.UI.GetPageNavigateLeftDown())
				{
					PerformPageNavigationLeft();
				}
				if (InputManager.UI.GetPageNavigateRightDown())
				{
					PerformPageNavigationRight();
				}
			}
			else
			{
				_selectionDelay -= Time.unscaledDeltaTime;
			}
			if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) < 0.2f && Mathf.Abs(InputManager.UI.GetVerticalAxis()) < 0.2f)
			{
				_navigationHold = 0;
				_selectionDelay = 0f;
			}
		}

		private void PerformConfirmAction()
		{
			if (_currentSelectable != null && _currentSelectable.Interactable)
			{
				PerformButtonAction(true);
				_currentSelectable.TryPerformConfirmAction();
			}
		}

		private void PerformCancelAction()
		{
			PerformButtonAction();
			Action onCancelDown = OnCancelDown;
			if (onCancelDown != null)
			{
				onCancelDown();
			}
		}

		private void PerformPageNavigationLeft()
		{
			PerformNavigationAction();
			Action onPageNavigateLeft = OnPageNavigateLeft;
			if (onPageNavigateLeft != null)
			{
				onPageNavigateLeft();
			}
		}

		private void PerformPageNavigationRight()
		{
			PerformNavigationAction();
			Action onPageNavigateRight = OnPageNavigateRight;
			if (onPageNavigateRight != null)
			{
				onPageNavigateRight();
			}
		}

		private void PerformButtonAction(bool confirmation = false)
		{
			if (AllowAcceptHold && confirmation)
			{
				_buttonDownDelay = 0.25f - Mathf.Min(Mathf.Max(0.05f * (float)(_consecutiveHold - 2), 0f), 0.15f);
				_consecutiveHold++;
			}
			else
			{
				_buttonDownDelay = 0.1f;
			}
		}

		private void ChangeSelection(IMMSelectable newSelectable)
		{
			PerformNavigationAction();
			if (_currentSelectable != newSelectable && newSelectable.Interactable)
			{
				_recentMoveVector = newSelectable.Selectable.transform.position - _currentSelectable.Selectable.transform.position;
				Action<Selectable, Selectable> onSelectionChanged = OnSelectionChanged;
				if (onSelectionChanged != null)
				{
					onSelectionChanged(newSelectable.Selectable, _currentSelectable.Selectable);
				}
				NavigateTo(newSelectable);
			}
		}

		private void NavigateTo(IMMSelectable newSelectable)
		{
			if (_currentSelectable != newSelectable && newSelectable.Interactable)
			{
				if (!_disableSFX)
				{
					UIManager.PlayAudio("event:/ui/change_selection");
				}
				_currentSelectable = newSelectable;
				_currentSelectable.Selectable.Select();
			}
		}

		public void NavigateToNew(IMMSelectable newSelectable)
		{
			if (_currentSelectable != newSelectable)
			{
				if (_currentSelectable != null)
				{
					_currentSelectable.SetNormalTransitionState();
				}
				_recentMoveVector.x = (_recentMoveVector.y = 0f);
				NavigateTo(newSelectable);
				Action<Selectable> onDefaultSetComplete = OnDefaultSetComplete;
				if (onDefaultSetComplete != null)
				{
					onDefaultSetComplete(newSelectable.Selectable);
				}
			}
		}

		public void ForcePerformNavigationAction()
		{
			PerformNavigationAction();
		}

		private void PerformNavigationAction()
		{
			_selectionDelay = 0.2f - Mathf.Min(Mathf.Max(0.05f * ((float)_navigationHold - 2f), 0f), 0.15f);
			_navigationHold++;
		}

		public void Clear()
		{
			Action onClear = OnClear;
			if (onClear != null)
			{
				onClear();
			}
			_currentSelectable = null;
			_selectionDelay = 0f;
			_buttonDownDelay = 0f;
			_navigationHold = 0;
			_consecutiveHold = 0;
			_recentMoveVector.x = (_recentMoveVector.y = 0f);
		}
	}
}
