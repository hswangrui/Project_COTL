using System;
using System.Collections;
using System.Collections.Generic;
using src.Extensions;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public abstract class UIMenuBase : MonoBehaviour
	{
		protected const string kShownAnimationState = "Shown";

		protected const string kShowAnimationState = "Show";

		protected const string kHideAnimationState = "Hide";

		protected const string kHiddenAnimationState = "Hidden";

		private const float kNoAnimatorFadeTime = 0.1f;

		public static readonly List<UIMenuBase> ActiveMenus = new List<UIMenuBase>();

		public Action OnShow;

		public Action OnShown;

		public Action OnHide;

		public Action OnHidden;

		public Action OnCancel;

		public static Action OnFirstMenuShow;

		public static Action OnFirstMenuShown;

		public static Action OnFinalMenuHide;

		public static Action OnFinalMenuHidden;

		[Header("Base Menu Components")]
		[SerializeField]
		protected Animator _animator;

		[SerializeField]
		protected CanvasGroup _canvasGroup;

		[Header("Navigation Defaults")]
		[SerializeField]
		private Selectable _defaultSelectable;

		[SerializeField]
		protected Selectable[] _defaultSelectableFallbacks;

		protected Canvas _canvas;

		private Selectable _cachedSelectable;

		protected virtual bool _addToActiveMenus
		{
			get
			{
				return true;
			}
		}

		public bool IsHiding { get; private set; }

		public bool IsShowing { get; private set; }

		public CanvasGroup CanvasGroup
		{
			get
			{
				return _canvasGroup;
			}
		}

		public Canvas Canvas
		{
			get
			{
				return _canvas;
			}
		}

		public virtual void Awake()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnCancelDown = (Action)Delegate.Combine(instance.OnCancelDown, new Action(OnCancelButtonInput));
			Canvas component;
			if (TryGetComponent<Canvas>(out component))
			{
				_canvas = component;
				UpdateSortingOrder();
			}
		}

		protected virtual void UpdateSortingOrder()
		{
			try
			{
				if (_canvas != null)
				{
					if (ActiveMenus.Count > 0)
					{
						_canvas.sortingOrder = ActiveMenus.LastElement()._canvas.sortingOrder + 1;
					}
					else
					{
						_canvas.sortingOrder = 100;
					}
				}
			}
			catch (Exception)
			{
			}
		}

		protected virtual void OnDestroy()
		{
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnCancelDown = (Action)Delegate.Remove(instance.OnCancelDown, new Action(OnCancelButtonInput));
			}
		}

		public void Show(bool immediate = false)
		{
			Debug.Log("Show " + base.gameObject.name);
			IsShowing = true;
			if (_addToActiveMenus && _canvas != null && !ActiveMenus.Contains(this))
			{
				ActiveMenus.Add(this);
				UpdateSortingOrder();
			}
			base.gameObject.SetActive(true);
			StopAllCoroutines();
			if (immediate)
			{
				if (_animator != null)
				{
					_animator.Play("Shown");
				}
				else
				{
					_canvasGroup.alpha = 1f;
				}
				SetActiveStateForMenu(true);
				OnShowStarted();
				if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 1)
				{
					Action onFirstMenuShow = OnFirstMenuShow;
					if (onFirstMenuShow != null)
					{
						onFirstMenuShow();
					}
				}
				Action onShow = OnShow;
				if (onShow != null)
				{
					onShow();
				}
				if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 1)
				{
					Action onFirstMenuShown = OnFirstMenuShown;
					if (onFirstMenuShown != null)
					{
						onFirstMenuShown();
					}
				}
				Action onShown = OnShown;
				if (onShown != null)
				{
					onShown();
				}
				OnShowCompleted();
				IsShowing = false;
			}
			else
			{
				StartCoroutine(DoShow());
			}
		}

		protected virtual IEnumerator DoShow()
		{
			yield return null;
			SetActiveStateForMenu(true);
			OnShowStarted();
			if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 1)
			{
				Action onFirstMenuShow = OnFirstMenuShow;
				if (onFirstMenuShow != null)
				{
					onFirstMenuShow();
				}
			}
			Action onShow = OnShow;
			if (onShow != null)
			{
				onShow();
			}
			yield return DoShowAnimation();
			if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 1)
			{
				Action onFirstMenuShown = OnFirstMenuShown;
				if (onFirstMenuShown != null)
				{
					onFirstMenuShown();
				}
			}
			Action onShown = OnShown;
			if (onShown != null)
			{
				onShown();
			}
			OnShowCompleted();
			IsShowing = false;
		}

		protected virtual IEnumerator DoShowAnimation()
		{
			if (_animator != null)
			{
				yield return _animator.YieldForAnimation("Show");
				yield break;
			}
			while (_canvasGroup.alpha < 1f)
			{
				_canvasGroup.alpha += Time.unscaledDeltaTime * 10f;
				yield return null;
			}
		}

		protected virtual void OnShowStarted()
		{
		}

		protected virtual void OnShowCompleted()
		{
		}

		public void Hide(bool immediate = false)
		{
			IsHiding = true;
			OnHideStarted();
			if (_addToActiveMenus && _canvas != null && ActiveMenus.Contains(this))
			{
				ActiveMenus.Remove(this);
				if (ActiveMenus.Count == 0)
				{
					MonoSingleton<UINavigatorNew>.Instance.Clear();
				}
			}
			StopAllCoroutines();
			if (immediate)
			{
				if (_animator != null)
				{
					_animator.Play("Hidden");
				}
				else
				{
					_canvasGroup.alpha = 0f;
				}
				if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 0)
				{
					Action onFinalMenuHide = OnFinalMenuHide;
					if (onFinalMenuHide != null)
					{
						onFinalMenuHide();
					}
				}
				Action onHide = OnHide;
				if (onHide != null)
				{
					onHide();
				}
				base.gameObject.SetActive(false);
				if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 0)
				{
					Action onFinalMenuHidden = OnFinalMenuHidden;
					if (onFinalMenuHidden != null)
					{
						onFinalMenuHidden();
					}
				}
				Action onHidden = OnHidden;
				if (onHidden != null)
				{
					onHidden();
				}
				OnHideCompleted();
				SetActiveStateForMenu(false);
				IsHiding = false;
			}
			else
			{
				StartCoroutine(DoHide());
			}
		}

		protected virtual IEnumerator DoHide()
		{
			if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 0)
			{
				Action onFinalMenuHide = OnFinalMenuHide;
				if (onFinalMenuHide != null)
				{
					onFinalMenuHide();
				}
			}
			Action onHide = OnHide;
			if (onHide != null)
			{
				onHide();
			}
			SetActiveStateForMenu(false);
			yield return DoHideAnimation();
			if (_addToActiveMenus && _canvas != null && ActiveMenus.Count == 0)
			{
				Action onFinalMenuHidden = OnFinalMenuHidden;
				if (onFinalMenuHidden != null)
				{
					onFinalMenuHidden();
				}
			}
			base.gameObject.SetActive(false);
			Action onHidden = OnHidden;
			if (onHidden != null)
			{
				onHidden();
			}
			OnHideCompleted();
			IsHiding = false;
		}

		protected virtual IEnumerator DoHideAnimation()
		{
			if (_animator != null)
			{
				yield return _animator.YieldForAnimation("Hide");
				yield break;
			}
			while (_canvasGroup.alpha > 0f)
			{
				_canvasGroup.alpha -= Time.unscaledDeltaTime * 10f;
				yield return null;
			}
		}

		protected virtual void OnHideStarted()
		{
		}

		protected virtual void OnHideCompleted()
		{
		}

		public virtual T Push<T>(T menu) where T : UIMenuBase
		{
			T val = menu.Instantiate();
			val.Show();
			return PushInstance(val);
		}

		public virtual T PushInstance<T>(T menu) where T : UIMenuBase
		{
			if (MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable != null)
			{
				OverrideDefaultOnce(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable);
			}
			menu.OnHide = (Action)Delegate.Combine(menu.OnHide, new Action(DoRelease));
			OnPush();
			SetActiveStateForMenu(false);
			return menu;
		}

		protected virtual void DoRelease()
		{
			OnRelease();
			if (!IsHiding)
			{
				SetActiveStateForMenu(true);
			}
		}

		protected virtual void OnPush()
		{
		}

		protected virtual void OnRelease()
		{
		}

		protected void ActivateNavigation()
		{
			Selectable selectable = ((_cachedSelectable != null) ? _cachedSelectable : _defaultSelectable);
			if (selectable == null || !selectable.interactable || !selectable.gameObject.activeInHierarchy)
			{
				for (int i = 0; i < _defaultSelectableFallbacks.Length; i++)
				{
					if (_defaultSelectableFallbacks[i].gameObject.activeSelf && _defaultSelectableFallbacks[i].interactable && _defaultSelectableFallbacks[i].gameObject.activeInHierarchy)
					{
						selectable = _defaultSelectableFallbacks[i];
						break;
					}
				}
			}
			if (!(selectable == null))
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(selectable as IMMSelectable);
				_cachedSelectable = null;
			}
		}

		protected void OverrideDefaultOnce(Selectable selectable)
		{
			_cachedSelectable = selectable;
		}

		protected void OverrideDefault(Selectable selectable)
		{
			_defaultSelectable = selectable;
		}

		private bool WillProvideNavigation()
		{
			if (!(_defaultSelectable != null) && _defaultSelectableFallbacks.Length == 0)
			{
				return _cachedSelectable != null;
			}
			return true;
		}

		protected virtual void SetActiveStateForMenu(bool state)
		{
			if (_canvasGroup != null)
			{
				_canvasGroup.interactable = state;
				SetActiveStateForMenu(base.gameObject, state);
				if (WillProvideNavigation() && state)
				{
					ActivateNavigation();
				}
			}
		}

		protected virtual void SetActiveStateForMenu(GameObject target, bool state)
		{
			IMMSelectable[] componentsInChildren = target.GetComponentsInChildren<IMMSelectable>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetInteractionState(state);
			}
			MMScrollRect[] componentsInChildren2 = target.GetComponentsInChildren<MMScrollRect>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = state;
			}
		}

		public virtual void OnCancelButtonInput()
		{
		}
	}
}
