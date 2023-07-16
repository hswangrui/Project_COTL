using System.Collections.Generic;

namespace Lamb.UI.Alerts
{
	public class RitualAlert : AlertBadge<UpgradeSystem.Type>
	{
		protected override AlertCategory<UpgradeSystem.Type> _source
		{
			get
			{
				return DataManager.Instance.Alerts.Rituals;
			}
		}

		protected override bool HasAlertSingle()
		{
			if (HasAlertTotal())
			{
				if (UpgradeSystem.GetUnlocked(_alert) && ObjectiveManager.HasCustomObjectiveOfType(Objectives.CustomQuestTypes.PerformAnyRitual))
				{
					return true;
				}
				List<Objectives_PerformRitual> objectivesOfType = ObjectiveManager.GetObjectivesOfType<Objectives_PerformRitual>();
				if (objectivesOfType != null)
				{
					foreach (Objectives_PerformRitual item in objectivesOfType)
					{
						if (item.Ritual == _alert)
						{
							return true;
						}
					}
				}
			}
			return base.HasAlertSingle();
		}

		protected override bool HasAlertTotal()
		{
			if (ObjectiveManager.HasCustomObjectiveOfType(Objectives.CustomQuestTypes.PerformAnyRitual))
			{
				return true;
			}
			if (ObjectiveManager.HasCustomObjective<Objectives_PerformRitual>())
			{
				return true;
			}
			return base.HasAlertTotal();
		}
	}
}
