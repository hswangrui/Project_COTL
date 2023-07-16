using System;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public abstract class MMTabNavigatorBase<T> : MonoBehaviour where T : MMTab
	{
		public Action<int> OnTabChanged;

		[SerializeField]
		private UIMenuBase _uiMenuController;

		[SerializeField]
		protected int _defaultTabIndex;

		[SerializeField]
		protected T[] _tabs;

		[SerializeField]
		private GameObject _changeTabLeft;

		[SerializeField]
		private GameObject _changeTabRight;

		private T _currentTab;

		public UIMenuBase CurrentMenu
		{
			get
			{
				return _currentTab.Menu;
			}
		}

		public int NumTabs
		{
			get
			{
				return _tabs.Length;
			}
		}

		public int CurrentMenuIndex
		{
			get
			{
				return _tabs.IndexOf(_currentTab);
			}
		}

		public int DefaultTabIndex
		{
			get
			{
				return _defaultTabIndex;
			}
			set
			{
				_defaultTabIndex = value;
			}
		}

		protected virtual void Start()
		{
			T[] tabs = _tabs;
			foreach (T tab in tabs)
			{
				tab.Configure();
				T val = tab;
				val.OnTabPressed = (Action)Delegate.Combine(val.OnTabPressed, (Action)delegate
				{
					TransitionTo(tab);
				});
				UIMenuBase menu = tab.Menu;
				menu.OnHide = (Action)Delegate.Combine(menu.OnHide, (Action)delegate
				{
					IMMSelectable currentSelectable = MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable;
					if (currentSelectable != null && currentSelectable.Selectable.transform.IsChildOf(tab.Menu.transform))
					{
						MonoSingleton<UINavigatorNew>.Instance.Clear();
					}
				});
			}
			UIMenuBase uiMenuController = _uiMenuController;
			uiMenuController.OnShow = (Action)Delegate.Combine(uiMenuController.OnShow, new Action(OnMenuShow));
			UIMenuBase uiMenuController2 = _uiMenuController;
			uiMenuController2.OnHide = (Action)Delegate.Combine(uiMenuController2.OnHide, new Action(OnMenuHide));
		}

		protected virtual void OnMenuShow()
		{
			ShowDefault();
		}

		protected virtual void OnMenuHide()
		{
		}

		public void SetNavigationVisibility(bool visibility)
		{
			_changeTabLeft.SetActive(visibility);
			_changeTabRight.SetActive(visibility);
		}

		public virtual void ShowDefault()
		{
			if (!_tabs[_defaultTabIndex].Button.interactable)
			{
				int num = -1;
				for (int i = 0; i < _tabs.Length; i++)
				{
					if (_tabs[i].Button.interactable && num == -1)
					{
						num = i;
					}
				}
				_defaultTabIndex = num;
			}
			SetDefaultTab(_tabs[_defaultTabIndex]);
		}

		protected void SetDefaultTab(T tab)
		{
			_currentTab = tab;
			_currentTab.Menu.Show(true);
			UIMenuBase uiMenuController = _uiMenuController;
			uiMenuController.OnShow = (Action)Delegate.Remove(uiMenuController.OnShow, new Action(OnMenuShow));
		}

		public void OnEnable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnPageNavigateLeft = (Action)Delegate.Combine(instance.OnPageNavigateLeft, new Action(NavigatePageLeft));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnPageNavigateRight = (Action)Delegate.Combine(instance2.OnPageNavigateRight, new Action(NavigatePageRight));
		}

		private void OnDisable()
		{
			if (!(MonoSingleton<UINavigatorNew>.Instance == null))
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnPageNavigateLeft = (Action)Delegate.Remove(instance.OnPageNavigateLeft, new Action(NavigatePageLeft));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnPageNavigateRight = (Action)Delegate.Remove(instance2.OnPageNavigateRight, new Action(NavigatePageRight));
			}
		}

		private void TransitionTo(T tab)
		{
			if ((UnityEngine.Object)_currentTab != (UnityEngine.Object)tab)
			{
				PerformTransitionTo(_currentTab, tab);
				_currentTab = tab;
			}
		}

		protected virtual void PerformTransitionTo(T from, T to)
		{
			from.Menu.Hide();
			to.Menu.Show();
		}

		public bool TryNavigatePageLeft()
		{
			int num = _tabs.IndexOf(_currentTab);
			while (num-- > 0)
			{
				if (_tabs[num].gameObject.activeInHierarchy && _tabs[num].Button.interactable)
				{
					_tabs[num].Button.TryPerformConfirmAction();
					Action<int> onTabChanged = OnTabChanged;
					if (onTabChanged != null)
					{
						onTabChanged(num);
					}
					return true;
				}
			}
			return false;
		}

		private void NavigatePageLeft()
		{
			if (_uiMenuController.CanvasGroup.interactable)
			{
				TryNavigatePageLeft();
			}
		}

		public bool TryNavigatePageRight()
		{
			int num = _tabs.IndexOf(_currentTab);
			while (num++ < _tabs.Length - 1)
			{
				if (_tabs[num].gameObject.activeInHierarchy && _tabs[num].Button.interactable)
				{
					_tabs[num].Button.TryPerformConfirmAction();
					Action<int> onTabChanged = OnTabChanged;
					if (onTabChanged != null)
					{
						onTabChanged(num);
					}
					return true;
				}
			}
			return false;
		}

		public bool CanNavigateLeft()
		{
			if ((UnityEngine.Object)_currentTab == (UnityEngine.Object)null)
			{
				return false;
			}
			int num = _tabs.IndexOf(_currentTab);
			while (num-- > 0)
			{
				if (_tabs[num].gameObject.activeInHierarchy && _tabs[num].Button.interactable)
				{
					return true;
				}
			}
			return false;
		}

		public bool CanNavigateRight()
		{
			if ((UnityEngine.Object)_currentTab == (UnityEngine.Object)null)
			{
				return false;
			}
			int num = _tabs.IndexOf(_currentTab);
			while (num++ < _tabs.Length - 1)
			{
				if (_tabs[num].gameObject.activeInHierarchy && _tabs[num].Button.interactable)
				{
					return true;
				}
			}
			return false;
		}

		private void NavigatePageRight()
		{
			if (_uiMenuController.CanvasGroup.interactable)
			{
				TryNavigatePageRight();
			}
		}
	}
}
