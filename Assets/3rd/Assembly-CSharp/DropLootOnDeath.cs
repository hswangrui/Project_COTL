using UnityEngine;

[RequireComponent(typeof(Health))]
public class DropLootOnDeath : BaseMonoBehaviour
{
	public enum DataSource
	{
		Local,
		StructureData
	}

	public DataSource DropLootDataSource;

	public InventoryItem.ITEM_TYPE LootToDrop;

	public int NumToDrop = 1;

	public int MaxLoot = 15;

	public Structure Structure;

	public bool ShakeOnHit = true;

	public bool OnlyPlayer = true;

	public bool OverrideBlackSoulsNumToDrop;

	private Health health;

	private float rotateSpeedY;

	private float rotateY;

	public bool IsNaturalResource;

	public float RotationToCamera = -60f;

	private float hp = -1f;

	public bool GiveXP { get; set; } = true;


	public void SetHealth()
	{
		if (health == null)
		{
			health = GetComponent<Health>();
		}
		if (health != null)
		{
			hp = health.totalHP;
		}
	}

	private void OnEnable()
	{
		if (health == null)
		{
			health = GetComponent<Health>();
		}
		if (health != null)
		{
			health.OnDie += OnDie;
			if (hp == -1f)
			{
				hp = health.totalHP;
			}
		}
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
	}

	public void Play()
	{
		if (LootToDrop == InventoryItem.ITEM_TYPE.NONE)
		{
			return;
		}
		if (LootToDrop == InventoryItem.ITEM_TYPE.BLACK_SOUL && health != null)
		{
			MaxLoot = 10;
			if (OverrideBlackSoulsNumToDrop)
			{
				InventoryItem.SpawnBlackSoul(Mathf.RoundToInt((float)Mathf.Min(MaxLoot, NumToDrop) * TrinketManager.GetBlackSoulsMultiplier()), base.transform.position, GiveXP);
			}
			else
			{
				InventoryItem.SpawnBlackSoul(Mathf.RoundToInt((float)Mathf.Min(MaxLoot, (int)hp) * TrinketManager.GetBlackSoulsMultiplier()), base.transform.position, GiveXP);
			}
			return;
		}
		bool num = DropLootDataSource == DataSource.StructureData;
		InventoryItem.ITEM_TYPE iTEM_TYPE = (num ? Structure.Structure_Info.LootToDrop : LootToDrop);
		int num2 = (num ? Structure.Structure_Info.LootCountToDrop : NumToDrop);
		if (IsNaturalResource)
		{
			num2 += TrinketManager.GetLootIncreaseModifier(iTEM_TYPE);
			num2 += UpgradeSystem.GetForageIncreaseModifier;
		}
		int num3 = -1;
		while (++num3 < num2)
		{
			InventoryItem.Spawn(iTEM_TYPE, 1, base.transform.position);
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		Play();
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
	}
}
