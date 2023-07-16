using UnityEngine;

public class HungerRecipeController : MonoBehaviour
{
	public BarController BarController;

	private float HungerDelta;

	private void Start()
	{
		AddRecipe(InventoryItem.ITEM_TYPE.NONE);
	}

	public void AddRecipe(InventoryItem.ITEM_TYPE recipe)
	{
		UpdateDelta(GetDelta(recipe));
	}

	public void RemoveRecipe(InventoryItem.ITEM_TYPE recipe)
	{
		UpdateDelta(0f - GetDelta(recipe));
	}

	private void UpdateDelta(float modification)
	{
		HungerDelta += modification;
		BarController.SetBarSizeForInfo((HungerBar.Count + HungerDelta) / HungerBar.MAX_HUNGER, HungerBar.HungerNormalized, FollowerBrainStats.Fasting);
	}

	public float GetDelta(InventoryItem.ITEM_TYPE recipe)
	{
		float totalNonLockedFollowers = FollowerManager.GetTotalNonLockedFollowers();
		if (totalNonLockedFollowers <= 0f)
		{
			return 0f;
		}
		return ((float)CookingData.GetSatationAmount(recipe) + HungerBar.ReservedSatiation) / totalNonLockedFollowers;
	}
}
