using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Interaction_WeaponChoiceChest : Interaction_WeaponSelectionPodium
{
	private new static List<Interaction_WeaponSelectionPodium> otherWeaponOptions = new List<Interaction_WeaponSelectionPodium>();

	private bool hasTriedFallback;

	private void Awake()
	{
		otherWeaponOptions.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		otherWeaponOptions.Remove(this);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		int i;
		for (i = otherWeaponOptions.Count - 1; i >= 0; i--)
		{
			if (otherWeaponOptions[i] != this)
			{
				otherWeaponOptions[i].Interactable = false;
				otherWeaponOptions[i].Lighting.SetActive(false);
				otherWeaponOptions[i].weaponBetterIcon.enabled = false;
				otherWeaponOptions[i].IconSpriteRenderer.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
				{
					otherWeaponOptions[i].IconSpriteRenderer.enabled = false;
				});
				otherWeaponOptions[i].podiumOn.SetActive(false);
				otherWeaponOptions[i].podiumOff.SetActive(true);
				otherWeaponOptions[i].particleEffect.Stop();
				otherWeaponOptions[i].AvailableGoop.Play("Hide");
				otherWeaponOptions[i].enabled = false;
			}
		}
		if (Type == Types.Weapon)
		{
			DataManager.Instance.CurrentRunWeaponLevel = WeaponLevel;
		}
		if (Type == Types.Curse)
		{
			DataManager.Instance.CurrentRunCurseLevel = WeaponLevel;
		}
	}

	protected override void SetWeapon(int ForceLevel = -1)
	{
		if (PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			SetCurse();
			return;
		}
		Debug.Log("SetWeapon()");
		IconSpriteRenderer.material = WeaponMaterial;
		for (int i = 0; i < 50; i++)
		{
			base.TypeOfWeapon = DataManager.Instance.GetRandomWeaponInPool();
			bool flag = false;
			foreach (Interaction_WeaponSelectionPodium otherWeaponOption in otherWeaponOptions)
			{
				if (otherWeaponOption != this && otherWeaponOption.TypeOfWeapon == base.TypeOfWeapon)
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
				if (hasTriedFallback || !DataManager.Instance.EnabledSpells)
				{
					base.gameObject.SetActive(false);
					return;
				}
				hasTriedFallback = true;
				SetCurse();
			}
		}
		Type = Types.Weapon;
		WeaponLevel = DataManager.Instance.CurrentRunWeaponLevel + 1;
		if (DataManager.Instance.ForcedStartingWeapon != EquipmentType.None)
		{
			base.TypeOfWeapon = DataManager.Instance.ForcedStartingWeapon;
			DataManager.Instance.ForcedStartingWeapon = EquipmentType.None;
		}
		if (DataManager.Instance.CurrentWeapon == EquipmentType.None)
		{
			WeaponLevel += DataManager.StartingEquipmentLevel;
		}
		WeaponLevel += Mathf.Clamp(LevelIncreaseAmount - 1, 0, LevelIncreaseAmount);
	}

	protected override void SetRelic()
	{
		IconSpriteRenderer.material = WeaponMaterial;
		IconSpriteRenderer.transform.localScale = Vector3.one * 0.41f;
		IconSpriteRenderer.transform.parent.localPosition = new Vector3(IconSpriteRenderer.transform.parent.localPosition.x, IconSpriteRenderer.transform.parent.localPosition.y, -1.5f);
		RelicData randomRelicData = EquipmentManager.GetRandomRelicData(false);
		base.TypeOfRelic = randomRelicData.RelicType;
		if (base.TypeOfRelic.ToString().Contains("Blessed") && DataManager.Instance.ForceBlessedRelic)
		{
			DataManager.Instance.ForceBlessedRelic = false;
		}
		else if (base.TypeOfRelic.ToString().Contains("Dammed") && DataManager.Instance.ForceDammedRelic)
		{
			DataManager.Instance.ForceDammedRelic = false;
		}
		Type = Types.Relic;
		if (!DataManager.Instance.SpawnedRelicsThisRun.Contains(base.TypeOfRelic))
		{
			DataManager.Instance.SpawnedRelicsThisRun.Add(base.TypeOfRelic);
		}
	}

	protected override void SetCurse(int ForceLevel = -1)
	{
		if (PlayerFleeceManager.FleeceSwapsCurseForRelic())
		{
			SetRelic();
			return;
		}
		IconSpriteRenderer.material = CurseMaterial;
		for (int i = 0; i < 50; i++)
		{
			base.TypeOfWeapon = DataManager.Instance.GetRandomCurseInPool();
			bool flag = false;
			foreach (Interaction_WeaponSelectionPodium otherWeaponOption in otherWeaponOptions)
			{
				if (otherWeaponOption != this && otherWeaponOption.TypeOfWeapon == base.TypeOfWeapon)
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
				if (hasTriedFallback || PlayerFleeceManager.FleeceSwapsWeaponForCurse())
				{
					base.gameObject.SetActive(false);
					return;
				}
				hasTriedFallback = true;
				SetWeapon();
			}
		}
		Type = Types.Curse;
		WeaponLevel = DataManager.Instance.CurrentRunCurseLevel + 1;
		if (DataManager.Instance.ForcedStartingCurse != EquipmentType.None)
		{
			base.TypeOfWeapon = DataManager.Instance.ForcedStartingCurse;
			DataManager.Instance.ForcedStartingCurse = EquipmentType.None;
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.None)
		{
			WeaponLevel += DataManager.StartingEquipmentLevel;
		}
		WeaponLevel += Mathf.Clamp(LevelIncreaseAmount - 1, 0, LevelIncreaseAmount);
	}
}
