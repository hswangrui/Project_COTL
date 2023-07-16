using System;
using I2.Loc;

[Serializable]
public class Objectives_UnlockUpgrade : ObjectivesData
{
	[Serializable]
	public class FinalizedData_UnlockUpgrade : ObjectivesDataFinalized
	{
		public UpgradeSystem.Type UnlockType;

		public override string GetText()
		{
			return LocalizationManager.GetTranslation(string.Format("Objectives/Unlock/{0}", UnlockType.ToString()));
		}
	}

	public UpgradeSystem.Type UnlockType;

	public override string Text
	{
		get
		{
			return LocalizationManager.GetTranslation(string.Format("Objectives/Unlock/{0}", UnlockType.ToString()));
		}
	}

	public Objectives_UnlockUpgrade()
	{
	}

	public Objectives_UnlockUpgrade(string groupId, UpgradeSystem.Type unlockType)
		: base(groupId)
	{
		Type = Objectives.TYPES.UNLOCK;
		UnlockType = unlockType;
		UpgradeSystem.OnUpgradeUnlocked += UpgradeSystem_OnUpgradeUnlocked;
	}

	public override void Init(bool initialAssigning)
	{
		UpgradeSystem.OnUpgradeUnlocked += UpgradeSystem_OnUpgradeUnlocked;
		base.Init(initialAssigning);
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_UnlockUpgrade
		{
			GroupId = GroupId,
			Index = Index,
			UnlockType = UnlockType,
			UniqueGroupID = UniqueGroupID
		};
	}

	private void UpgradeSystem_OnUpgradeUnlocked(UpgradeSystem.Type upgradeType)
	{
		if (upgradeType == UnlockType)
		{
			ObjectiveManager.UpdateObjective(this);
			UpgradeSystem.OnUpgradeUnlocked -= UpgradeSystem_OnUpgradeUnlocked;
		}
	}

	protected override bool CheckComplete()
	{
		return UpgradeSystem.GetUnlocked(UnlockType);
	}
}
