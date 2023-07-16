using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentWeapon : MonoBehaviour
{
	private EquipmentType Weapon = EquipmentType.None;

	private int Level;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image Background;

	[SerializeField]
	private TextMeshProUGUI LevelText;

	private bool playing;

	public void OnEnable()
	{
		if (DataManager.Instance.CurrentWeapon == EquipmentType.None || !GameManager.IsDungeon(PlayerFarming.Location) || PlayerFarming.Location == FollowerLocation.IntroDungeon)
		{
			base.transform.localScale = Vector3.zero;
		}
		PlayerWeapon.OnWeaponChanged = (PlayerWeapon.WeaponEvent)Delegate.Combine(PlayerWeapon.OnWeaponChanged, new PlayerWeapon.WeaponEvent(SetWeapon));
		Interaction_WeaponSelectionPodium.OnHighlightWeapon = (Action<bool>)Delegate.Combine(Interaction_WeaponSelectionPodium.OnHighlightWeapon, new Action<bool>(HighlightWeapon));
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRomanNumeralsChanged = (Action<bool>)Delegate.Combine(instance.OnRomanNumeralsChanged, new Action<bool>(OnRomanNumeralSettingChanged));
		Background.enabled = false;
	}

	private void HighlightWeapon(bool Toggle)
	{
		if (Weapon == EquipmentType.None)
		{
			return;
		}
		Background.transform.DOKill();
		base.transform.DOKill();
		Background.enabled = Toggle;
		if (Background.enabled)
		{
			Background.transform.localScale = Vector3.one * 1.2f;
			Background.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			base.transform.DOScale(Vector3.one * 1.1f, 0f);
			if (!playing)
			{
				StartCoroutine(Highlighting());
			}
		}
		else
		{
			base.transform.DOKill();
			base.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		}
	}

	private IEnumerator Highlighting()
	{
		while (Background.enabled)
		{
			playing = true;
			base.transform.DOKill();
			base.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.5f);
			yield return new WaitForSeconds(1f);
		}
		playing = false;
	}

	private void OnDisable()
	{
		PlayerWeapon.OnWeaponChanged = (PlayerWeapon.WeaponEvent)Delegate.Remove(PlayerWeapon.OnWeaponChanged, new PlayerWeapon.WeaponEvent(SetWeapon));
		Interaction_WeaponSelectionPodium.OnHighlightWeapon = (Action<bool>)Delegate.Remove(Interaction_WeaponSelectionPodium.OnHighlightWeapon, new Action<bool>(HighlightWeapon));
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRomanNumeralsChanged = (Action<bool>)Delegate.Remove(instance.OnRomanNumeralsChanged, new Action<bool>(OnRomanNumeralSettingChanged));
	}

	private void SetWeapon(EquipmentType weapon, int level)
	{
		SetWeapon(weapon, level, true);
	}

	private void OnRomanNumeralSettingChanged(bool state)
	{
		LevelText.text = Level.ToNumeral();
	}

	private void SetWeapon(EquipmentType weapon, int level, bool punch)
	{
		Weapon = weapon;
		Level = level;
		icon.enabled = weapon != EquipmentType.None;
		Background.enabled = weapon != EquipmentType.None;
		LevelText.text = level.ToNumeral();
		if (weapon == EquipmentType.None || EquipmentManager.GetWeaponData(weapon) == null)
		{
			return;
		}
		icon.sprite = PlayerFarming.Instance.playerWeapon.GetCurrentIcon(weapon);
		base.transform.localScale = Vector3.one;
		if (punch)
		{
			icon.transform.localScale = Vector3.one * 2f;
			icon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			Background.transform.DOKill();
			Background.enabled = true;
			Background.transform.localScale = Vector3.one * 0.9f;
			Background.transform.DOScale(Vector3.one * 2f, 0.5f);
			Color c = Background.color;
			Background.DOFade(0f, 0.5f).OnComplete(delegate
			{
				Background.color = c;
				Background.enabled = false;
			});
		}
	}
}
