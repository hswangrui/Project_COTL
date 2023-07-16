using System.Collections.Generic;

public class Structures_Temple : StructureBrain
{
	public List<FollowerTask_Study> Studiers = new List<FollowerTask_Study>();

	public const int AvailableStudySlotsMax = 4;

	public const float TempleDurationForXP = 12f;

	public const float TempleIncrementXP = 0.05f;

	public bool StudyAvailable
	{
		get
		{
			if (Studiers.Count < AvailableStudySlots)
			{
				return DataManager.Instance.TempleStudyXP < TempleMaxStudyXP;
			}
			return false;
		}
	}

	public static int AvailableStudySlots
	{
		get
		{
			if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_MonksUpgrade))
			{
				return 2;
			}
			return 4;
		}
	}

	public static float TempleMaxStudyXP
	{
		get
		{
			return UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_MonksUpgrade) ? 12 : 6;
		}
	}

	public bool CheckOverrideComplete()
	{
		return false;
	}

	public void AddStudier(FollowerTask_Study studier)
	{
		if (!Studiers.Contains(studier))
		{
			Studiers.Add(studier);
		}
	}

	public void RemoveStudier(FollowerTask_Study studier)
	{
		if (Studiers.Contains(studier))
		{
			Studiers.Remove(studier);
		}
	}
}
