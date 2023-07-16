using System;
using UnityEngine;

namespace Lamb.UI.BuildMenu
{
	public class BuildMenuTab : MMTab
	{
		[SerializeField]
		private GameObject _alert;

		[SerializeField]
		private UIBuildMenuController.Category _category;

		public GameObject Alert
		{
			get
			{
				return _alert;
			}
		}

		public override void Configure()
		{
			if (DataManager.Instance.Alerts.Structures.GetAlertsForCategory(_category).Count > 0)
			{
				_alert.SetActive(true);
				StructureAlerts structures = DataManager.Instance.Alerts.Structures;
				structures.OnAlertRemoved = (Action<StructureBrain.TYPES>)Delegate.Combine(structures.OnAlertRemoved, new Action<StructureBrain.TYPES>(OnAlertRemoved));
			}
			else
			{
				_alert.SetActive(false);
			}
		}

		private void OnAlertRemoved(StructureBrain.TYPES type)
		{
			if (DataManager.Instance.Alerts.Structures.GetAlertsForCategory(_category).Count > 0)
			{
				_alert.SetActive(true);
				return;
			}
			_alert.SetActive(false);
			StructureAlerts structures = DataManager.Instance.Alerts.Structures;
			structures.OnAlertRemoved = (Action<StructureBrain.TYPES>)Delegate.Remove(structures.OnAlertRemoved, new Action<StructureBrain.TYPES>(OnAlertRemoved));
		}

		private void OnDisable()
		{
			if (DataManager.Instance != null)
			{
				StructureAlerts structures = DataManager.Instance.Alerts.Structures;
				structures.OnAlertRemoved = (Action<StructureBrain.TYPES>)Delegate.Remove(structures.OnAlertRemoved, new Action<StructureBrain.TYPES>(OnAlertRemoved));
			}
		}
	}
}
