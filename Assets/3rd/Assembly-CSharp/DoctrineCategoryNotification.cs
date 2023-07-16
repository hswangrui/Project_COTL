using System;
using UnityEngine;

public class DoctrineCategoryNotification : MonoBehaviour
{
	private SermonCategory _sermonCategory;

	private DoctrineUpgradeSystem.DoctrineType _doctrineType;

	public void Configure(SermonCategory sermonCategory)
	{
		_sermonCategory = sermonCategory;
		int count = DataManager.Instance.Alerts.Doctrine.GetAlertsForCategory(_sermonCategory).Count;
		base.gameObject.SetActive(count > 0);
	}

	public void Configure(DoctrineUpgradeSystem.DoctrineType type)
	{
		_doctrineType = type;
		base.gameObject.SetActive(DataManager.Instance.Alerts.Doctrine.HasAlert(type));
	}

	public void OnEnable()
	{
		DoctrineAlerts doctrine = DataManager.Instance.Alerts.Doctrine;
		doctrine.OnAlertRemoved = (Action<DoctrineUpgradeSystem.DoctrineType>)Delegate.Combine(doctrine.OnAlertRemoved, new Action<DoctrineUpgradeSystem.DoctrineType>(OnAlertRemoved));
	}

	public void OnDisable()
	{
		if (DataManager.Instance != null)
		{
			DoctrineAlerts doctrine = DataManager.Instance.Alerts.Doctrine;
			doctrine.OnAlertRemoved = (Action<DoctrineUpgradeSystem.DoctrineType>)Delegate.Remove(doctrine.OnAlertRemoved, new Action<DoctrineUpgradeSystem.DoctrineType>(OnAlertRemoved));
		}
	}

	private void OnAlertRemoved(DoctrineUpgradeSystem.DoctrineType doctrineType)
	{
		if (_sermonCategory != 0)
		{
			Configure(_sermonCategory);
		}
		else if (_doctrineType != 0)
		{
			Configure(_doctrineType);
		}
	}

	public void TryRemoveAlert()
	{
		DataManager.Instance.Alerts.Doctrine.Remove(_doctrineType);
	}
}
