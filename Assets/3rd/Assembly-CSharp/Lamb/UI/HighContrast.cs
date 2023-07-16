using System;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class HighContrast : MonoBehaviour
	{
		[SerializeField]
		private HighContrastConfiguration _configuration;

		private HighContrastTarget _target;

		private void Awake()
		{
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnHighContrastTextChanged = (Action<bool>)Delegate.Combine(instance.OnHighContrastTextChanged, new Action<bool>(OnHighContrastSettingChanged));
			SelectableColourProxy component;
			Selectable component2;
			TMP_Text component3;
			if (TryGetComponent<SelectableColourProxy>(out component))
			{
				_target = new SelectableColorProxyHighContrastTarget(component, _configuration);
			}
			else if (TryGetComponent<Selectable>(out component2))
			{
				if (component2.transition == Selectable.Transition.ColorTint)
				{
					_target = new SelectableColorTransitionHighContrastTarget(component2, _configuration);
				}
				else if (component2.transition == Selectable.Transition.Animation)
				{
					_target = new SelectableAnimatedHighContrastTarget(component2, _configuration);
				}
			}
			else if (TryGetComponent<TMP_Text>(out component3))
			{
				_target = new TextHighContrastTarget(component3, _configuration);
			}
			if (_target != null)
			{
				_target.Init();
			}
			if (SettingsManager.Settings != null && SettingsManager.Settings.Accessibility != null && SettingsManager.Settings.Accessibility.HighContrastText)
			{
				OnHighContrastSettingChanged(true);
			}
		}

		private void OnDestroy()
		{
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnHighContrastTextChanged = (Action<bool>)Delegate.Remove(instance.OnHighContrastTextChanged, new Action<bool>(OnHighContrastSettingChanged));
		}

		private void OnHighContrastSettingChanged(bool state)
		{
			if (_target != null)
			{
				_target.Apply(state);
			}
		}
	}
}
