public class RatauGiveSeeds : BaseMonoBehaviour
{
	public void Play()
	{
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.SEED, 6, base.transform.position);
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.SEED_PUMPKIN, 3, base.transform.position);
	}
}
