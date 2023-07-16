using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentCurse : MonoBehaviour
{
	private EquipmentType Curse = EquipmentType.None;

	private int Level;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image Background;

	[SerializeField]
	private TextMeshProUGUI LevelText;

	private void Start()
	{
		if (!DataManager.Instance.EnabledSpells)
		{
			base.gameObject.SetActive(false);
		}
		else if (DataManager.Instance.CurrentCurse == EquipmentType.None || !GameManager.IsDungeon(PlayerFarming.Location) || PlayerFarming.Location == FollowerLocation.IntroDungeon)
		{
			base.transform.localScale = Vector3.zero;
		}
		PlayerSpells.OnCurseChanged = (PlayerSpells.CurseEvent)Delegate.Combine(PlayerSpells.OnCurseChanged, new PlayerSpells.CurseEvent(SetCurse));
		Interaction_WeaponSelectionPodium.OnHighlightCurse = (Action<bool>)Delegate.Combine(Interaction_WeaponSelectionPodium.OnHighlightCurse, new Action<bool>(HighlightCurse));
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRomanNumeralsChanged = (Action<bool>)Delegate.Combine(instance.OnRomanNumeralsChanged, new Action<bool>(OnRomanNumeralSettingChanged));
	}

	private void OnDestroy()
	{
		PlayerSpells.OnCurseChanged = (PlayerSpells.CurseEvent)Delegate.Remove(PlayerSpells.OnCurseChanged, new PlayerSpells.CurseEvent(SetCurse));
		Interaction_WeaponSelectionPodium.OnHighlightCurse = (Action<bool>)Delegate.Remove(Interaction_WeaponSelectionPodium.OnHighlightCurse, new Action<bool>(HighlightCurse));
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRomanNumeralsChanged = (Action<bool>)Delegate.Remove(instance.OnRomanNumeralsChanged, new Action<bool>(OnRomanNumeralSettingChanged));
	}

	private void HighlightCurse(bool Toggle)
	{
		if (Curse != EquipmentType.None)
		{
			Background.transform.DOKill();
			base.transform.DOKill();
			Background.enabled = Toggle;
			if (Background.enabled)
			{
				Background.transform.localScale = Vector3.one * 1.1f;
				Background.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
				base.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.OutBack);
			}
			else
			{
				base.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			}
		}
	}

	private void OnRomanNumeralSettingChanged(bool state)
	{
		LevelText.text = Level.ToNumeral();
	}

	private void SetCurse(EquipmentType curse, int level)
	{
		SetCurse(curse, level, true);
	}

	private void SetCurse(EquipmentType curse, int level, bool punch)
	{
		if (DataManager.Instance.EnabledSpells)
		{
			base.gameObject.SetActive(true);
		}
		Curse = curse;
		Level = level;
		icon.enabled = curse != EquipmentType.None;
		Background.enabled = curse != EquipmentType.None;
		LevelText.text = level.ToNumeral();
		if (curse == EquipmentType.None || EquipmentManager.GetCurseData(curse) == null)
		{
			return;
		}
		icon.sprite = EquipmentManager.GetCurseData(curse).UISprite;
		base.transform.localScale = Vector3.one;
		if (punch)
		{
			icon.transform.localScale = Vector3.one * 2f;
			icon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			Background.transform.DOKill();
			Background.enabled = true;
			Background.transform.localScale = Vector3.one * 0.9f;
			Background.transform.DOScale(Vector3.one * 3f, 0.5f);
			Color c = Background.color;
			Background.DOFade(0f, 0.5f).OnComplete(delegate
			{
				Background.color = c;
				Background.enabled = false;
			});
		}
	}
}
