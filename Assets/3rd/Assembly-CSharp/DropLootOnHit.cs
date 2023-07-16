using UnityEngine;

public class DropLootOnHit : BaseMonoBehaviour
{
	public InventoryItem.ITEM_TYPE LootToDrop;

	[SerializeField]
	private Vector2 randomAmount;

	[SerializeField]
	private Vector2 randomForce = new Vector2(2f, 4f);

	public bool DontDropOnPlayerFullAmmo;

	private Health health;

	public bool IsNaturalResource;

	public bool dropLootOnHitWithProjectile;

	private void Start()
	{
		SetHealth();
	}

	public void SetHealth()
	{
		if (!(health != null))
		{
			health = GetComponent<Health>();
			if (health != null)
			{
				health.OnHit += OnHit;
			}
		}
	}

	public void Play(Health.AttackTypes AttackType)
	{
		if (LootToDrop == InventoryItem.ITEM_TYPE.NONE)
		{
			return;
		}
		if (LootToDrop == InventoryItem.ITEM_TYPE.BLACK_SOUL && health != null)
		{
			if (DontDropOnPlayerFullAmmo && FaithAmmo.Ammo >= FaithAmmo.Total)
			{
				return;
			}
			if (!dropLootOnHitWithProjectile)
			{
				if (AttackType != Health.AttackTypes.Projectile)
				{
					BlackSoul blackSoul = InventoryItem.SpawnBlackSoul(Mathf.RoundToInt(Random.Range(randomAmount.x, randomAmount.y + 1f) * TrinketManager.GetBlackSoulsMultiplier()), base.transform.position, false, true);
					if ((bool)blackSoul)
					{
						blackSoul.SetAngle(Random.Range(0, 360), new Vector2(randomForce.x, randomForce.y));
					}
				}
			}
			else
			{
				BlackSoul blackSoul2 = InventoryItem.SpawnBlackSoul(Mathf.RoundToInt(Random.Range(randomAmount.x, randomAmount.y + 1f) * TrinketManager.GetBlackSoulsMultiplier()), base.transform.position, false, true);
				if ((bool)blackSoul2)
				{
					blackSoul2.SetAngle(Random.Range(0, 360), new Vector2(randomForce.x, randomForce.y));
				}
			}
		}
		else
		{
			int num = Mathf.RoundToInt(Random.Range(randomAmount.x, randomAmount.y + 1f));
			if (IsNaturalResource)
			{
				num += TrinketManager.GetLootIncreaseModifier(LootToDrop);
				num += UpgradeSystem.GetForageIncreaseModifier;
			}
			int num2 = -1;
			while (++num2 < num)
			{
				InventoryItem.Spawn(LootToDrop, 1, base.transform.position);
			}
		}
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		if (Attacker != null)
		{
			Health component = Attacker.GetComponent<Health>();
			if (component != null && component.team != Health.Team.PlayerTeam)
			{
				return;
			}
		}
		Play(AttackType);
	}
}
