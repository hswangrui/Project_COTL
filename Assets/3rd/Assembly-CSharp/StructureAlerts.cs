using System;
using System.Collections.Generic;
using Lamb.UI.BuildMenu;

[Serializable]
public class StructureAlerts : AlertCategory<StructureBrain.TYPES>
{
	public void CheckStructureUnlocked()
	{
		foreach (StructureBrain.TYPES value in Enum.GetValues(typeof(StructureBrain.TYPES)))
		{
			if (StructuresData.GetUnlocked(value))
			{
				AddOnce(value);
			}
		}
	}

	public List<StructureBrain.TYPES> GetAlertsForCategory(UIBuildMenuController.Category category)
	{
		List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
		foreach (StructureBrain.TYPES alert in _alerts)
		{
			if (StructuresData.CategoryForType(alert) == category && DataManager.Instance.UnlockedStructures.Contains(alert))
			{
				list.Add(alert);
			}
		}
		return list;
	}

	public override bool HasAlert(StructureBrain.TYPES alert)
	{
		if (alert == StructureBrain.TYPES.DECORATION_MUSHROOM_SCULPTURE && DataManager.Instance.SozoDecorationQuestActive)
		{
			return true;
		}
		return base.HasAlert(alert);
	}
}
