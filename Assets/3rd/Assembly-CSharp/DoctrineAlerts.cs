using System;
using System.Collections.Generic;

[Serializable]
public class DoctrineAlerts : AlertCategory<DoctrineUpgradeSystem.DoctrineType>
{
	protected override bool IsValidAlert(DoctrineUpgradeSystem.DoctrineType alert)
	{
		if (alert == DoctrineUpgradeSystem.DoctrineType.Special_Sacrifice || (uint)(alert - 49) <= 1u)
		{
			return false;
		}
		return true;
	}

	public List<DoctrineUpgradeSystem.DoctrineType> GetAlertsForCategory(SermonCategory sermonCategory)
	{
		List<DoctrineUpgradeSystem.DoctrineType> list = new List<DoctrineUpgradeSystem.DoctrineType>();
		foreach (DoctrineUpgradeSystem.DoctrineType alert in _alerts)
		{
			if (DoctrineUpgradeSystem.GetCategory(alert) == sermonCategory)
			{
				list.Add(alert);
			}
		}
		return list;
	}
}
