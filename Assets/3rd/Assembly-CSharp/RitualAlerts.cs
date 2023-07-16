using System;

[Serializable]
public class RitualAlerts : AlertCategory<UpgradeSystem.Type>
{
	private UpgradeSystem.Type[] _allRituals;

	public RitualAlerts()
	{
		_allRituals = UpgradeSystem.AllRituals();
		UpgradeSystem.OnAbilityUnlocked = (Action<UpgradeSystem.Type>)Delegate.Combine(UpgradeSystem.OnAbilityUnlocked, new Action<UpgradeSystem.Type>(OnRitualAdded));
		UpgradeSystem.OnAbilityLocked = (Action<UpgradeSystem.Type>)Delegate.Combine(UpgradeSystem.OnAbilityLocked, new Action<UpgradeSystem.Type>(OnRitualRemoved));
	}

	~RitualAlerts()
	{
		_allRituals = null;
		UpgradeSystem.OnAbilityUnlocked = (Action<UpgradeSystem.Type>)Delegate.Remove(UpgradeSystem.OnAbilityUnlocked, new Action<UpgradeSystem.Type>(OnRitualAdded));
		UpgradeSystem.OnAbilityLocked = (Action<UpgradeSystem.Type>)Delegate.Remove(UpgradeSystem.OnAbilityLocked, new Action<UpgradeSystem.Type>(OnRitualRemoved));
	}

	private void OnRitualAdded(UpgradeSystem.Type upgradeType)
	{
		if (_allRituals.Contains(upgradeType))
		{
			AddOnce(upgradeType);
		}
	}

	private void OnRitualRemoved(UpgradeSystem.Type upgradeType)
	{
		Remove(upgradeType);
	}
}
