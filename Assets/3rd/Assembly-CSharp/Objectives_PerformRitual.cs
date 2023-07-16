using System;
using I2.Loc;

[Serializable]
public class Objectives_PerformRitual : ObjectivesData
{
	[Serializable]
	public class FinalizedData_PerformRitual : ObjectivesDataFinalized
	{
		public UpgradeSystem.Type Ritual;

		public string TargetFollowerName_1;

		public string TargetFollowerName_2;

		public override string GetText()
		{
			return string.Format(LocalizationManager.GetTranslation("Objectives/PerformRitual/" + Ritual), TargetFollowerName_1, TargetFollowerName_2);
		}
	}

	public UpgradeSystem.Type Ritual;

	public int TargetFollowerID_1 = -1;

	public int TargetFollowerID_2 = -1;

	public int RequiredFollowers;

	private bool complete;

	public override string Text
	{
		get
		{
			string translation = LocalizationManager.GetTranslation("Objectives/PerformRitual/" + Ritual);
			object obj;
			if (TargetFollowerID_1 == -1)
			{
				obj = "";
			}
			else
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(TargetFollowerID_1, true);
				obj = ((infoByID != null) ? infoByID.Name : null);
			}
			string arg = (string)obj;
			object obj2;
			if (TargetFollowerID_2 == -1)
			{
				obj2 = "";
			}
			else
			{
				FollowerInfo infoByID2 = FollowerInfo.GetInfoByID(TargetFollowerID_2, true);
				obj2 = ((infoByID2 != null) ? infoByID2.Name : null);
			}
			string arg2 = (string)obj2;
			return string.Format(translation, arg, arg2);
		}
	}

	public Objectives_PerformRitual()
	{
	}

	public Objectives_PerformRitual(string groupId, UpgradeSystem.Type ritual, int targetFollowerID = -1, int requiredFollowers = 0, float questExpireDuration = -1f)
		: base(groupId, questExpireDuration)
	{
		Type = Objectives.TYPES.PERFORM_RITUAL;
		Ritual = ritual;
		RequiredFollowers = requiredFollowers;
		TargetFollowerID_1 = targetFollowerID;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		complete = false;
		if (!UpgradeSystem.GetUnlocked(Ritual))
		{
			Failed();
		}
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		FinalizedData_PerformRitual obj = new FinalizedData_PerformRitual
		{
			GroupId = GroupId,
			Index = Index,
			Ritual = Ritual
		};
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(TargetFollowerID_1, true);
		obj.TargetFollowerName_1 = ((infoByID != null) ? infoByID.Name : null);
		FollowerInfo infoByID2 = FollowerInfo.GetInfoByID(TargetFollowerID_2, true);
		obj.TargetFollowerName_2 = ((infoByID2 != null) ? infoByID2.Name : null);
		obj.UniqueGroupID = UniqueGroupID;
		return obj;
	}

	public void Init(int targetFollowerID)
	{
		TargetFollowerID_1 = targetFollowerID;
		Init(true);
	}

	protected override bool CheckComplete()
	{
		if (!IsFailed && !complete && !FailLocked)
		{
			if (TargetFollowerID_1 != -1 && FollowerInfo.GetInfoByID(TargetFollowerID_1) == null)
			{
				Failed();
			}
			else if (TargetFollowerID_2 != -1 && FollowerInfo.GetInfoByID(TargetFollowerID_2) == null)
			{
				Failed();
			}
			else if (TargetFollowerID_1 == -1 && TargetFollowerID_2 == -1 && base.Follower != -1 && FollowerInfo.GetInfoByID(base.Follower) == null)
			{
				Failed();
			}
			else if (!UpgradeSystem.GetUnlocked(Ritual))
			{
				Failed();
			}
		}
		if (IsFailed)
		{
			return false;
		}
		return complete;
	}

	public void CheckComplete(int targetFollowerID_1, int targetFollowerID_2)
	{
		if (RequiredFollowers == 1 && TargetFollowerID_1 == targetFollowerID_1)
		{
			complete = true;
		}
		else if (RequiredFollowers == 2 && ((TargetFollowerID_1 == targetFollowerID_1 && TargetFollowerID_2 == targetFollowerID_2) || (TargetFollowerID_1 == targetFollowerID_2 && TargetFollowerID_2 == targetFollowerID_1)))
		{
			complete = true;
		}
		else if (RequiredFollowers <= 0)
		{
			complete = true;
		}
	}
}
