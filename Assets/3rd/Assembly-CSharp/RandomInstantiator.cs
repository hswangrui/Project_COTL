using UnityEngine;

public class RandomInstantiator : BaseMonoBehaviour
{
	public enum SelectionMode
	{
		RandomChance,
		RandomSingle
	}

	public GameObject[] GameObjects;

	public SelectionMode selectionMode;

	public bool pickingMultiple;

	[Range(0f, 100f)]
	public int chanceToEnable;

	private void Start()
	{
		switch (selectionMode)
		{
		case SelectionMode.RandomChance:
		{
			if (GameObjects == null || GameObjects.Length == 0)
			{
				break;
			}
			for (int i = 0; i < GameObjects.Length; i++)
			{
				if (Random.Range(0, 100) <= chanceToEnable && CanItemBeSpawned(GameObjects[i]))
				{
					ObjectPool.Spawn(GameObjects[i].gameObject, base.transform);
				}
			}
			break;
		}
		case SelectionMode.RandomSingle:
			if (GameObjects != null && GameObjects.Length != 0)
			{
				int num = Random.Range(0, GameObjects.Length);
				if (GameObjects[num] != null && CanItemBeSpawned(GameObjects[num]))
				{
					ObjectPool.Spawn(GameObjects[num].gameObject, base.transform);
				}
			}
			break;
		}
	}

	private bool CanItemBeSpawned(GameObject item)
	{
		if (InventoryItem.IsHeart(InventoryItem.GetInventoryItemTypeOf(item)) && PlayerFleeceManager.FleecePreventsHealthPickups())
		{
			return false;
		}
		return true;
	}
}
