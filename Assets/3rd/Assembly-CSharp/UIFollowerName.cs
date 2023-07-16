using System;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

public class UIFollowerName : MonoBehaviour
{
	[SerializeField]
	private TMP_Text nameText;

	[SerializeField]
	private Localize _localize;

	[SerializeField]
	private Material normalMaterial;

	[SerializeField]
	private Material normalMaterialNoStyle;

	[SerializeField]
	private Material twitchMaterial;

	[SerializeField]
	private Material twitchMaterialNoStyle;

	private Follower follower;

	private bool _shown = true;

	private bool showAllNames;

	private bool showTwitchNames;

	private TMP_FontAsset regularFont;

	private void Awake()
	{
		follower = GetComponentInParent<Follower>();
		if (follower != null)
		{
			Follower obj = follower;
			obj.OnFollowerBrainAssigned = (Action)Delegate.Combine(obj.OnFollowerBrainAssigned, new Action(OnBrainAssigned));
			if (follower.Brain != null)
			{
				OnBrainAssigned();
			}
		}
		showAllNames = SettingsManager.Settings.Game.ShowFollowerNames;
		//showTwitchNames = TwitchManager.FollowerNamesEnabled;
		if (nameText.font)
			regularFont = nameText.font;
		if (!showAllNames)
		{
			Hide(false);
		}
		GameplaySettingsUtilities.OnShowFollowerNamesChanged = (Action<bool>)Delegate.Combine(GameplaySettingsUtilities.OnShowFollowerNamesChanged, new Action<bool>(OnFollowerNamesEnabledChanged));
		//TwitchManager.OnFollowerNamesEnabledChanged = (Action<bool>)Delegate.Combine(TwitchManager.OnFollowerNamesEnabledChanged, new Action<bool>(OnTwitchFollowerNamesEnabledChanged));
	}

	private void OnDestroy()
	{
		if (follower != null)
		{
			Follower obj = follower;
			obj.OnFollowerBrainAssigned = (Action)Delegate.Remove(obj.OnFollowerBrainAssigned, new Action(OnBrainAssigned));
		}
		GameplaySettingsUtilities.OnShowFollowerNamesChanged = (Action<bool>)Delegate.Remove(GameplaySettingsUtilities.OnShowFollowerNamesChanged, new Action<bool>(OnFollowerNamesEnabledChanged));
	//	TwitchManager.OnFollowerNamesEnabledChanged = (Action<bool>)Delegate.Remove(TwitchManager.OnFollowerNamesEnabledChanged, new Action<bool>(OnTwitchFollowerNamesEnabledChanged));
	}

	private void OnEnable()
	{
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRemoveTextStylingChanged = (Action<bool>)Delegate.Combine(instance.OnRemoveTextStylingChanged, new Action<bool>(OnFontChanged));
		AccessibilityManager instance2 = Singleton<AccessibilityManager>.Instance;
		instance2.OnDyslexicFontValueChanged = (Action<bool>)Delegate.Combine(instance2.OnDyslexicFontValueChanged, new Action<bool>(OnFontChanged));
	}

	private void OnDisable()
	{
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRemoveTextStylingChanged = (Action<bool>)Delegate.Remove(instance.OnRemoveTextStylingChanged, new Action<bool>(OnFontChanged));
		AccessibilityManager instance2 = Singleton<AccessibilityManager>.Instance;
		instance2.OnDyslexicFontValueChanged = (Action<bool>)Delegate.Remove(instance2.OnDyslexicFontValueChanged, new Action<bool>(OnFontChanged));
	}

	private void OnFontChanged(bool value)
	{
		bool dyslexicFont = SettingsManager.Settings.Accessibility.DyslexicFont;
		if (LocalizationManager.CurrentLanguage == "English" && dyslexicFont)
		{
			nameText.font = LocalizationManager.DyslexicFontAsset;
		}
		else
		{
			nameText.font = regularFont;
		}
	}

	private void OnFollowerNamesEnabledChanged(bool value)
	{
		showAllNames = value;
		//if (!TwitchManager.FollowerNamesEnabled || !IsTwitchFollower())
		//{
		//	if (value)
		//	{
		//		Show();
		//	}
		//	else
		//	{
		//		Hide();
		//	}
		//}
	}

	private void OnTwitchFollowerNamesEnabledChanged(bool value)
	{
		showTwitchNames = value;
		if (IsTwitchFollower() && !SettingsManager.Settings.Game.ShowFollowerNames)
		{
			if (value)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	private bool IsTwitchFollower()
	{
		return !string.IsNullOrEmpty(follower.Brain.Info.ViewerID);
	}

	private void OnBrainAssigned()
	{
		Follower obj = follower;
		obj.OnFollowerBrainAssigned = (Action)Delegate.Remove(obj.OnFollowerBrainAssigned, new Action(OnBrainAssigned));
		SetText();
	}

	private void Update()
	{
		SetText();
	}

	private void SetText()
	{
		if (!(follower != null) || follower.Brain == null)
		{
			return;
		}
		string text = "";
		if (!string.IsNullOrEmpty(follower.Brain.Info.ViewerID) && (showAllNames || showTwitchNames))
		{
			text = ((!string.IsNullOrEmpty(follower.Brain.Info.ViewerID)) ? ((showTwitchNames ? "<sprite name=\"icon_TwitchIcon\">" : "") + " " + follower.Brain.Info.Name) : "");
		}
		else if (showAllNames)
		{
			text = follower.Brain.Info.Name;
		}
		if (nameText.text != text)
		{
			nameText.text = text;
			nameText.transform.localScale = Vector3.one;
		}
		if (SettingsManager.Settings.Accessibility.DyslexicFont || !(LocalizationManager.CurrentLanguage == "English"))
		{
			return;
		}
		if (!string.IsNullOrEmpty(follower.Brain.Info.ViewerID) && showTwitchNames)
		{
			if (SettingsManager.Settings.Accessibility.RemoveTextStyling)
			{
				nameText.fontSharedMaterial = twitchMaterialNoStyle;
			}
			else
			{
				nameText.fontSharedMaterial = twitchMaterial;
			}
		}
		else if (showAllNames)
		{
			if (SettingsManager.Settings.Accessibility.RemoveTextStyling)
			{
				nameText.fontSharedMaterial = normalMaterialNoStyle;
			}
			else
			{
				nameText.fontSharedMaterial = normalMaterial;
			}
		}
	}

	public void Show(bool animate = true)
	{
		if (!_shown && (showAllNames || (!string.IsNullOrEmpty(follower.Brain.Info.ViewerID) && showTwitchNames)))
		{
			_shown = true;
			base.transform.DOKill();
			base.transform.localScale = Vector3.zero;
			if (animate)
			{
				base.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
			}
			base.transform.localScale = Vector3.one;
		}
	}

	public void Hide(bool animate = true)
	{
		if (_shown)
		{
			_shown = false;
			base.transform.DOKill();
			base.transform.localScale = Vector3.one;
			if (animate)
			{
				base.transform.DOScale(Vector3.zero, 0.3f).SetUpdate(true).SetEase(Ease.InBack);
			}
			else
			{
				base.transform.localScale = Vector3.zero;
			}
		}
	}
}
