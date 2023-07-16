using System;
using System.Collections.Generic;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

public class RelicsUnlockRoom : MonoBehaviour
{
	[SerializeField]
	private Interaction_WeaponChoice[] weaponChoices;

	private void Start()
	{
		Interaction_WeaponChoice[] array = weaponChoices;
		foreach (Interaction_WeaponChoice interaction_WeaponChoice in array)
		{
			if (interaction_WeaponChoice != null)
			{
				interaction_WeaponChoice.gameObject.SetActive(false);
			}
		}
	}

	private void OnDestroy()
	{
		Interaction_WeaponChoice[] array = weaponChoices;
		foreach (Interaction_WeaponChoice interaction_WeaponChoice in array)
		{
			if (interaction_WeaponChoice != null)
			{
				interaction_WeaponChoice.gameObject.SetActive(true);
			}
		}
		PlayerRelic.OnRelicEquipped -= EquppedRelic;
	}

	public void ConversationCallback()
	{
		PlayerReturnToBase.Disabled = true;
		Time.timeScale = 0f;
		DataManager.Instance.OnboardedRelics = true;
		UIRelicMenuController uIRelicMenuController = MonoSingleton<UIManager>.Instance.RelicMenuTemplate.Instantiate();
		List<RelicType> list = new List<RelicType>();
		RelicData[] relicData = EquipmentManager.RelicData;
		foreach (RelicData relicData2 in relicData)
		{
			if (relicData2.UpgradeType == UpgradeSystem.Type.Relic_Pack_Default)
			{
				list.Add(relicData2.RelicType);
			}
		}
		uIRelicMenuController.Show(list);
		uIRelicMenuController.OnHidden = (Action)Delegate.Combine(uIRelicMenuController.OnHidden, (Action)delegate
		{
			Time.timeScale = 1f;
			weaponChoices[0].transform.localPosition = new Vector3(-1.21f, -1.96f, 0f);
			weaponChoices[0].RelicStartCharged = false;
			weaponChoices[0].Reveal();
			PlayerRelic.OnRelicEquipped += EquppedRelic;
		});
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Relic_Pack_Default);
		GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
	}

	private void EquppedRelic(RelicData relic)
	{
		RelicRoomManager.Instance.EquippedRelicConversation.Play();
	}
}
