using UnityEngine;
using UnityEngine.Events;

public class Interaction_WeaponChoice : Interaction_WeaponSelectionPodium
{
	[SerializeField]
	private UnityEvent onInteract;

	private bool hasTriedFallback;

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		UnityEvent unityEvent = onInteract;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
	}

	protected override void SetWeapon(int ForceLevel = -1)
	{
		Debug.Log("SetWeapon()");
		IconSpriteRenderer.material = WeaponMaterial;
		if (PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			SetCurse();
			return;
		}
		for (int i = 0; i < 50; i++)
		{
			base.TypeOfWeapon = DataManager.Instance.GetRandomWeaponInPool();
			bool flag = false;
			Interaction_WeaponSelectionPodium[] array = otherWeaponOptions;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].TypeOfWeapon == base.TypeOfWeapon)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
			if (i >= 49)
			{
				if (!hasTriedFallback && DataManager.Instance.EnabledSpells)
				{
					hasTriedFallback = true;
					SetCurse();
					return;
				}
				base.gameObject.SetActive(false);
			}
		}
		Type = Types.Weapon;
		WeaponLevel = DataManager.Instance.CurrentRunWeaponLevel + 1;
		weaponBetterIcon.enabled = false;
		if (DataManager.Instance.ForcedStartingWeapon != EquipmentType.None)
		{
			base.TypeOfWeapon = DataManager.Instance.ForcedStartingWeapon;
			DataManager.Instance.ForcedStartingWeapon = EquipmentType.None;
		}
		if (DataManager.Instance.CurrentWeapon == EquipmentType.None)
		{
			WeaponLevel += DataManager.StartingEquipmentLevel;
		}
	}

	protected override void SetCurse(int ForceLevel = -1)
	{
		if (PlayerFleeceManager.FleeceSwapsCurseForRelic())
		{
			SetWeapon();
			return;
		}
		Debug.Log("SetCurse()");
		IconSpriteRenderer.material = CurseMaterial;
		for (int i = 0; i < 50; i++)
		{
			base.TypeOfWeapon = DataManager.Instance.GetRandomCurseInPool();
			bool flag = false;
			Interaction_WeaponSelectionPodium[] array = otherWeaponOptions;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].TypeOfWeapon == base.TypeOfWeapon)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
			if (i >= 49)
			{
				if (!hasTriedFallback && !PlayerFleeceManager.FleeceSwapsWeaponForCurse())
				{
					hasTriedFallback = true;
					SetWeapon();
					return;
				}
				base.gameObject.SetActive(false);
			}
		}
		Type = Types.Curse;
		WeaponLevel = DataManager.Instance.CurrentRunCurseLevel + 1;
		weaponBetterIcon.enabled = false;
		if (DataManager.Instance.ForcedStartingCurse != EquipmentType.None)
		{
			base.TypeOfWeapon = DataManager.Instance.ForcedStartingCurse;
			DataManager.Instance.ForcedStartingCurse = EquipmentType.None;
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.None)
		{
			WeaponLevel += DataManager.StartingEquipmentLevel;
		}
	}

	protected override void SetRelic()
	{
		for (int i = 0; i < 50; i++)
		{
			RelicData randomRelicData = EquipmentManager.GetRandomRelicData(false);
			base.TypeOfRelic = randomRelicData.RelicType;
			bool flag = false;
			Interaction_WeaponSelectionPodium[] array = otherWeaponOptions;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].TypeOfRelic == base.TypeOfRelic)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
			if (i >= 49)
			{
				base.gameObject.SetActive(false);
			}
		}
		SetRelic(base.TypeOfRelic);
	}
}
