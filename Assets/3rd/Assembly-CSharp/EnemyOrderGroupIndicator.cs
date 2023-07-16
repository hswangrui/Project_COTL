using System;
using DG.Tweening;
using UnityEngine;

public class EnemyOrderGroupIndicator : MonoBehaviour
{
	public GameObject[] indicators;

	public GameObject[] indicatorsAlt;

	public GameObject redShield;

	public GameObject greenShield;

	private int _cachedOrder;

	private bool _numerals;

	private void OnEnable()
	{
		_numerals = SettingsManager.Settings.Accessibility.RomanNumerals;
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRomanNumeralsChanged = (Action<bool>)Delegate.Combine(instance.OnRomanNumeralsChanged, new Action<bool>(OnRomanNumeralsChanged));
	}

	private void OnDisable()
	{
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnRomanNumeralsChanged = (Action<bool>)Delegate.Remove(instance.OnRomanNumeralsChanged, new Action<bool>(OnRomanNumeralsChanged));
	}

	public void SetIndicatorForOrder(int order)
	{
		Debug.Log("Setting indicator for " + order);
		for (int i = 0; i < indicators.Length; i++)
		{
			indicators[i].SetActive(order == i && _numerals);
		}
		for (int j = 0; j < indicatorsAlt.Length; j++)
		{
			indicatorsAlt[j].SetActive(order == j && !_numerals);
		}
		SetVulnerable(order == 0);
		_cachedOrder = order;
	}

	public void SetVulnerable(bool vulnerable)
	{
		greenShield.SetActive(vulnerable);
		redShield.SetActive(!vulnerable);
		if (vulnerable)
		{
			base.transform.localScale = Vector3.one;
			base.transform.DOKill();
			base.transform.DOPunchScale(Vector3.one * 1.25f, 0.25f);
		}
	}

	private void OnRomanNumeralsChanged(bool state)
	{
		_numerals = state;
		SetIndicatorForOrder(_cachedOrder);
	}
}
