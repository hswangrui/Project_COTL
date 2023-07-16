using UnityEngine;

public class DropLootOnDestroy : BaseMonoBehaviour
{
	public InventoryItem.ITEM_TYPE LootToDrop;

	public int NumToDrop = 1;

	public bool DropSoul = true;

	public bool chanceOfDrop;

	[Range(0f, 100f)]
	public float chanceToDrop = 100f;

	private Health _heath;

	private void OnEnable()
	{
		_heath = base.gameObject.GetComponent<Health>();
		if (_heath != null)
		{
			_heath.OnDie += OnDie;
		}
	}

	private void OnDisable()
	{
		if (_heath != null)
		{
			_heath.OnDie -= OnDie;
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		CameraManager.shakeCamera(0.25f, Random.Range(0, 360));
		if ((DropSoul && LootToDrop == InventoryItem.ITEM_TYPE.SOUL) || LootToDrop == InventoryItem.ITEM_TYPE.NONE)
		{
			return;
		}
		if (chanceOfDrop)
		{
			if (Random.Range(0f, 100f) <= chanceToDrop)
			{
				int num = -1;
				while (++num < NumToDrop)
				{
					InventoryItem.Spawn(LootToDrop, 1, base.transform.position);
				}
			}
		}
		else
		{
			int num2 = -1;
			while (++num2 < NumToDrop)
			{
				InventoryItem.Spawn(LootToDrop, 1, base.transform.position);
			}
		}
	}

	public void dropLoot()
	{
		CameraManager.shakeCamera(0.25f, Random.Range(0, 360));
		if (DropSoul && LootToDrop == InventoryItem.ITEM_TYPE.SOUL)
		{
			return;
		}
		if (chanceOfDrop)
		{
			if (Random.Range(0f, 100f) <= chanceToDrop)
			{
				int num = -1;
				while (++num < NumToDrop)
				{
					InventoryItem.Spawn(LootToDrop, 1, base.transform.position);
				}
			}
		}
		else
		{
			int num2 = -1;
			while (++num2 < NumToDrop)
			{
				InventoryItem.Spawn(LootToDrop, 1, base.transform.position);
			}
		}
	}
}
