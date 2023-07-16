using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DivineInspirationCounts : MonoBehaviour
{
	public TextMeshProUGUI DivineInspiration;

	public TextMeshProUGUI DisciplePoints;

	private void Start()
	{
		CheckText();
	}

	private void OnEnable()
	{
		UpgradeSystem.OnAbilityPointDelta = (Action)Delegate.Combine(UpgradeSystem.OnAbilityPointDelta, new Action(CheckText));
		UpgradeSystem.OnDisciplePointDelta = (Action)Delegate.Combine(UpgradeSystem.OnDisciplePointDelta, new Action(CheckText));
	}

	private void OnDisable()
	{
		UpgradeSystem.OnAbilityPointDelta = (Action)Delegate.Remove(UpgradeSystem.OnAbilityPointDelta, new Action(CheckText));
		UpgradeSystem.OnDisciplePointDelta = (Action)Delegate.Remove(UpgradeSystem.OnDisciplePointDelta, new Action(CheckText));
	}

	private void CheckText()
	{
		if (UpgradeSystem.AbilityPoints > 0)
		{
			if (DivineInspiration.text != UpgradeSystem.AbilityPoints.ToString())
			{
				DivineInspiration.transform.parent.localScale = Vector3.one * 1.5f;
				DivineInspiration.transform.parent.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			}
			DivineInspiration.text = UpgradeSystem.AbilityPoints.ToString();
			DivineInspiration.transform.parent.gameObject.SetActive(true);
		}
		else
		{
			DivineInspiration.text = "";
			DivineInspiration.transform.parent.gameObject.SetActive(false);
		}
		if (UpgradeSystem.DisciplePoints > 0)
		{
			if (DisciplePoints.text != UpgradeSystem.DisciplePoints.ToString())
			{
				DisciplePoints.transform.parent.localScale = Vector3.one * 1.5f;
				DisciplePoints.transform.parent.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			}
			DisciplePoints.text = UpgradeSystem.DisciplePoints.ToString();
			DisciplePoints.transform.parent.gameObject.SetActive(true);
		}
		else
		{
			DisciplePoints.text = "";
			DisciplePoints.transform.parent.gameObject.SetActive(false);
		}
	}
}
