using UnityEngine;

public class RelicTester : MonoBehaviour
{
	public RelicType relicType = RelicType.LightningStrike;

	public RelicType secondaryRelicType = RelicType.RerollWeapon;

	private void RelicsUnlocked()
	{
		DataManager.Instance.OnboardedRelics = true;
	}

	private void ChangeRelic()
	{
		DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Relics);
		PlayerFarming.Instance.playerRelic.EquipRelic(EquipmentManager.GetRelicData(relicType));
	}

	private void ChargeRelic()
	{
		PlayerFarming.Instance.playerRelic.FullyCharge();
	}

	private void RemoveRelicUpgrades()
	{
		UpgradeSystem.UnlockedUpgrades.Remove(UpgradeSystem.Type.Relics_Blessed_1);
		UpgradeSystem.UnlockedUpgrades.Remove(UpgradeSystem.Type.Relics_Dammed_1);
		UpgradeSystem.UnlockedUpgrades.Remove(UpgradeSystem.Type.Relic_Pack1);
		UpgradeSystem.UnlockedUpgrades.Remove(UpgradeSystem.Type.Relic_Pack2);
	}

	private void AddRelicUpgrades()
	{
		UpgradeSystem.UnlockedUpgrades.Add(UpgradeSystem.Type.Relic_Pack_Default);
		UpgradeSystem.UnlockedUpgrades.Add(UpgradeSystem.Type.Relics_Blessed_1);
		UpgradeSystem.UnlockedUpgrades.Add(UpgradeSystem.Type.Relics_Dammed_1);
		UpgradeSystem.UnlockedUpgrades.Add(UpgradeSystem.Type.Relic_Pack1);
		UpgradeSystem.UnlockedUpgrades.Add(UpgradeSystem.Type.Relic_Pack2);
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.X))
		{
			RelicType relicType = this.relicType;
			this.relicType = secondaryRelicType;
			secondaryRelicType = relicType;
			ChangeRelic();
		}
		if (Input.GetKeyUp(KeyCode.C))
		{
			Debug.Log("PlayerFarming.Instance.playerRelic: " + PlayerFarming.Instance.playerRelic);
			if (DataManager.Instance.CurrentRelic == RelicType.None)
			{
				ChangeRelic();
			}
			else
			{
				ChargeRelic();
			}
		}
	}
}
