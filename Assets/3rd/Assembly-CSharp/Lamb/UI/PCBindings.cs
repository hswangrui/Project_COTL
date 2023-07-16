using System;
using Lamb.UI.Assets;
using src.Extensions;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class PCBindings : UISubmenuBase
	{
		[SerializeField]
		private BindingConflictResolver _bindingCOnflictResolver;

		[SerializeField]
		private InputDisplay[] _controllers;

		[SerializeField]
		private KeybindItem[] _keybindItems;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private GameObject _controlPrompts;

		public override void Awake()
		{
			base.Awake();
			KeybindItem[] keybindItems = _keybindItems;
			foreach (KeybindItem keybindItem in keybindItems)
			{
				keybindItem.KeyboardBinding.BindingButton.onClick.AddListener(delegate
				{
					OnBindingItemClicked(keybindItem.KeyboardBinding);
				});
				keybindItem.MouseBinding.BindingButton.onClick.AddListener(delegate
				{
					OnBindingItemClicked(keybindItem.MouseBinding);
				});
				keybindItem.KeybindConflictLookup.Configure(_bindingCOnflictResolver);
			}
		}

		protected override void OnShowStarted()
		{
			_scrollRect.enabled = false;
			_scrollRect.normalizedPosition = Vector2.one;
			_scrollRect.enabled = true;
		}

		public void Configure(InputType inputType)
		{
			InputDisplay[] controllers = _controllers;
			for (int i = 0; i < controllers.Length; i++)
			{
				controllers[i].Configure(inputType);
			}
		}

		private void OnBindingItemClicked(BindingItem bindingItem)
		{
			UIControlBindingOverlayController uIControlBindingOverlayController = MonoSingleton<UIManager>.Instance.BindingOverlayControllerTemplate.Instantiate();
			uIControlBindingOverlayController.Show(_bindingCOnflictResolver, bindingItem.BindingTerm, bindingItem.Category, bindingItem.Action, bindingItem.AxisContribution, bindingItem.ControllerType);
			PushInstance(uIControlBindingOverlayController);
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			_controlPrompts.SetActive(false);
			uIControlBindingOverlayController.OnHide = (Action)Delegate.Combine(uIControlBindingOverlayController.OnHide, (Action)delegate
			{
				_controlPrompts.SetActive(true);
			});
		}
	}
}
