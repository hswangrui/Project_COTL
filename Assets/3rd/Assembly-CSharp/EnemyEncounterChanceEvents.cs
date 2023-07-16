using System;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

public class EnemyEncounterChanceEvents : BaseMonoBehaviour
{
	public bool affectAllRooms;

	[Header("Shielded Enemy options")]
	public float layer1AllShieldChance;

	public float layer2AllShieldChance = 0.05f;

	public float layer1IndividualShieldChance = 0.05f;

	public float layer2IndividualShieldChance = 0.05f;

	public bool allowShieldsBeforeLayer2;

	public bool shieldsPreHeavyAttackGained;

	[Header("Grouped Enemy Options")]
	public float layer1ChanceOfGroupedEnemies = 0.05f;

	public float layer2ChanceOfGroupedEnemies = 0.05f;

	public float groupDamageProtectionMultiplier;

	public int minimumGroupSize = 3;

	public int maximumGroupSize = 4;

	public GameObject orderIndicatorPrefab;

	public bool allowExtraNonGrouped;

	public bool alsoProtectedFromTraps = true;

	public Vector3 orderIndicatorOffset = Vector3.zero;

	private List<UnitObject> shieldUnits = new List<UnitObject>();

	private List<UnitObject> groupableUnits = new List<UnitObject>();

	private UnitObject[] allUnits;

	private int currentOrder;

	private void Start()
	{
		if (affectAllRooms)
		{
			BiomeGenerator.OnBiomeChangeRoom += AssignShieldsAndGroups;
			return;
		}
		Debug.Log("Assigning shields and groups for this room " + Time.realtimeSinceStartup);
		AssignShieldsAndGroups();
	}

	private void OnDestroy()
	{
		Debug.Log("Changed room removed as I've been destroyed");
		BiomeGenerator.OnBiomeChangeRoom -= AssignShieldsAndGroups;
	}

	private void AssignShieldsAndGroups()
	{
		Debug.Log("Assigning shields and groups for " + base.gameObject.name + " time: " + Time.realtimeSinceStartup);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(BiomeGenerator.Instance.CurrentRoom.Seed);
		FindEnemyGroups();
		if (shieldUnits.Count > 0)
		{
			AssignShieldEnemies();
		}
		if (groupableUnits.Count > 0)
		{
			AssignGroupedEnemies();
		}
		UnityEngine.Random.state = state;
	}

	private void FindEnemyGroups()
	{
		allUnits = UnityEngine.Object.FindObjectsOfType<UnitObject>();
		shieldUnits.Clear();
		groupableUnits.Clear();
		for (int i = 0; i < allUnits.Length; i++)
		{
			UnitObject unitObject = allUnits[i];
			if (unitObject.health != null && unitObject.health.team == Health.Team.Team2)
			{
				shieldUnits.Add(unitObject);
				if (groupableUnits.Count < maximumGroupSize && unitObject.orderIndicator == null && unitObject.GetComponent<ShowHPBar>() != null)
				{
					groupableUnits.Add(unitObject);
				}
			}
		}
		if (groupableUnits.Count < minimumGroupSize || (groupableUnits.Count > maximumGroupSize && !allowExtraNonGrouped))
		{
			groupableUnits.Clear();
		}
	}

	private void AssignShieldEnemies()
	{
		if (PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			return;
		}
		float num = layer1AllShieldChance;
		float num2 = layer1IndividualShieldChance;
		if (GameManager.Layer2)
		{
			num = layer2AllShieldChance;
			num2 = layer2IndividualShieldChance;
		}
		bool flag = false;
		bool flag2 = (GameManager.Layer2 || allowShieldsBeforeLayer2) && DataManager.Instance.PlayerFleece != 6 && !DataManager.Instance.SpecialAttacksDisabled && (CrownAbilities.CrownAbilityUnlocked(CrownAbilities.TYPE.Combat_HeavyAttack) || UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) || shieldsPreHeavyAttackGained);
		if (flag2)
		{
			flag = UnityEngine.Random.value < num;
		}
		for (int i = 0; i < shieldUnits.Count; i++)
		{
			EnemyHasShield component = shieldUnits[i].gameObject.GetComponent<EnemyHasShield>();
			if (!(component != null))
			{
				continue;
			}
			float num3 = 0f;
			if (flag)
			{
				num3 = 1f;
			}
			else
			{
				if (flag2 || component.allowBeforeGotHeavyAttack)
				{
					num3 = num2;
				}
				if (flag2 && !GameManager.Layer2 && component.layer1ShieldChanceOverride != -1f)
				{
					num3 = component.layer1ShieldChanceOverride;
				}
				else if (flag2 && GameManager.Layer2 && component.layer2ShieldChanceOverride != -1f)
				{
					num3 = component.layer2ShieldChanceOverride;
				}
			}
			if (UnityEngine.Random.value < num3)
			{
				component.AddShield();
			}
		}
	}

	private void AssignGroupedEnemies()
	{
		float num = layer1ChanceOfGroupedEnemies;
		if (GameManager.Layer2)
		{
			num = layer2ChanceOfGroupedEnemies;
		}
		if (!(UnityEngine.Random.value < num))
		{
			return;
		}
		for (int i = 0; i < groupableUnits.Count; i++)
		{
			UnitObject unitObject = groupableUnits[i];
			if (unitObject == null)
			{
				continue;
			}
			if (alsoProtectedFromTraps && i > 0)
			{
				unitObject.health.ImmuneToTraps = true;
			}
			if (i < groupableUnits.Count - 1)
			{
				MinionProtector minionProtector = unitObject.gameObject.AddComponent<MinionProtector>();
				UnitObject[] protectedUnits = new UnitObject[1] { groupableUnits[i + 1] };
				minionProtector.protectedUnits = protectedUnits;
				minionProtector.showLineToMinions = false;
				minionProtector.healthLineMaterial = null;
				minionProtector.damageMultiplier = groupDamageProtectionMultiplier;
				minionProtector.destroyedAction = (Action)Delegate.Combine(minionProtector.destroyedAction, new Action(protectorDestroyed));
			}
			ShowHPBar component = unitObject.GetComponent<ShowHPBar>();
			if (component != null)
			{
				component.Init();
				Debug.Log("Positioning by the healthbar?");
				unitObject.orderIndicator = component.hpBar.groupIndicator;
				component.hpBar.gameObject.SetActive(true);
				unitObject.orderIndicator.gameObject.SetActive(true);
				unitObject.orderIndicator.SetIndicatorForOrder(i);
				EnemyScuttleSwiper component2 = unitObject.gameObject.GetComponent<EnemyScuttleSwiper>();
				if (component2 != null && component2.StartHidden == EnemyScuttleSwiper.StartingStates.Hidden)
				{
					unitObject.orderIndicator.gameObject.SetActive(false);
					component.hpBar.gameObject.SetActive(false);
				}
			}
			else
			{
				Debug.Log("Problem finding the order indicator!");
			}
		}
	}

	private void protectorDestroyed()
	{
		currentOrder++;
		if (groupableUnits != null && groupableUnits[currentOrder] != null && groupableUnits[currentOrder].orderIndicator != null && currentOrder < groupableUnits.Count)
		{
			Debug.Log("Protector has been destroyed updating next available order indicator to green " + currentOrder);
			groupableUnits[currentOrder].orderIndicator.SetVulnerable(true);
			if (alsoProtectedFromTraps)
			{
				groupableUnits[currentOrder].health.ImmuneToTraps = false;
			}
		}
	}
}
