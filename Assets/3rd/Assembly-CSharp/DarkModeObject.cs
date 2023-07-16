using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DarkModeObject : MonoBehaviour
{
	private Image image;

	private TextMeshProUGUI text;

	[SerializeField]
	private bool EnableDisable = true;

	[SerializeField]
	private bool EnableOnDark = true;

	[SerializeField]
	private bool ChangeColor;

	[SerializeField]
	private Color darkModeColor;

	private Color _startColor;

	private void Start()
	{
		image = GetComponent<Image>();
		text = GetComponent<TextMeshProUGUI>();
		if (image != null)
		{
			_startColor = image.color;
		}
		if (text != null)
		{
			_startColor = text.color;
		}
		UpdateObject();
		GraphicsSettingsUtilities.OnDarkModeSettingsChanged = (Action)Delegate.Combine(GraphicsSettingsUtilities.OnDarkModeSettingsChanged, new Action(UpdateObject));
	}

	private void OnDestroy()
	{
		GraphicsSettingsUtilities.OnDarkModeSettingsChanged = (Action)Delegate.Remove(GraphicsSettingsUtilities.OnDarkModeSettingsChanged, new Action(UpdateObject));
	}

	private void UpdateObject()
	{
		if (text != null)
		{
			if (SettingsManager.Settings != null && SettingsManager.Settings.Accessibility != null && SettingsManager.Settings.Accessibility.DarkMode)
			{
				text.color = darkModeColor;
			}
			else
			{
				text.color = _startColor;
			}
		}
		if (!(image != null))
		{
			return;
		}
		if (SettingsManager.Settings != null && SettingsManager.Settings.Accessibility != null && SettingsManager.Settings.Accessibility.DarkMode)
		{
			if (ChangeColor)
			{
				image.color = darkModeColor;
			}
			if (EnableDisable)
			{
				if (EnableOnDark)
				{
					image.enabled = true;
				}
				else
				{
					image.enabled = false;
				}
			}
			return;
		}
		if (ChangeColor)
		{
			image.color = _startColor;
		}
		if (EnableDisable)
		{
			if (EnableOnDark)
			{
				image.enabled = false;
			}
			else
			{
				image.enabled = true;
			}
		}
	}
}
